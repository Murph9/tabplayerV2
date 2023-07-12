using murph9.TabPlayer.Songs.Models;
using Rocksmith2014PsarcLib.Psarc;
using Rocksmith2014PsarcLib.Psarc.Asset;
using Rocksmith2014PsarcLib.Psarc.Models.Sng;

namespace murph9.TabPlayer.Songs.Convert
{
    public class PsarcFileConverter
    {
        private readonly PsarcFile _p;
        private readonly string _nameFilter;
        public PsarcFileConverter(PsarcFile p, string nameFilter)
        {
            _p = p;
            _nameFilter = nameFilter;
        }

        public SongInfo ConvertToSongInfo()
        {
            var instrumentNames = _p.TOC.Entries.Where(x => (_nameFilter == null || x.Path.Contains(_nameFilter))
                    && x.Path.Contains(".sng"))
                    .Where(x => x.Path.Contains('_') && !x.Path.Contains(SongInfo.VOCALS_NAME))
                    .Select(x => new {toc = x, label = x.Path.Split('_')[1].Replace(".sng", "")});

            var instruments = instrumentNames
                    .Select(x => ConvertInstrument(x.toc, x.label))
                    .ToList();

            var firstMetadata = _p.ExtractArrangementManifests().FirstOrDefault(x =>
                (_nameFilter == null || x.Attributes.BlockAsset.Contains(_nameFilter)) && x.Attributes.SongName != null);

            var vocals = ConvertVocals();

            var metadata = new SongMetadata(firstMetadata.Attributes.SongName, firstMetadata.Attributes.ArtistName, firstMetadata.Attributes.AlbumName, firstMetadata.Attributes.SongYear, firstMetadata.Attributes.SongLength);
            return new SongInfo(metadata, instruments.Where(x => x != null), vocals);
        }

        private Instrument ConvertInstrument(PsarcTOCEntry tocEntry, string name)
        {            
            var sng = new SngAsset();
            using (var mem = new MemoryStream()) {
                _p.InflateEntry(tocEntry, mem);
                sng.ReadFrom(mem);
            }

            if (!sng.Arrangements.Any())
                throw new Exception("No notes found");

            var jsonManifests = _p.ExtractArrangementManifests();
            var metadata = jsonManifests.FirstOrDefault(x => (_nameFilter == null || x.Attributes.BlockAsset.Contains(_nameFilter))
                    && x.Attributes.ArrangementName.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            var allNotes = new List<Note>();
            // for each arrangement get the highest note count phrase (phrase.MaxDifficulty needs investigation)
            var phraseIterations = sng.PhraseIterations.Select((x, i) => new {x, i});
            
            foreach (var phr in phraseIterations) {
                foreach (var arr in sng.Arrangements.OrderByDescending(x => x.Difficulty)) {
                    var phrNotes = arr.Notes.Where(x => x.PhraseIterationId == phr.i);
                    if (phrNotes.Any()) {
                        allNotes.AddRange(phrNotes);
                        break;
                    }
                }
            }
            
            var sections = new List<Songs.Models.Section>();
            foreach (var phr in phraseIterations) {
                var it = sng.Phrases[phr.x.PhraseId];
                sections.Add(new Songs.Models.Section(phr.x.StartTime, phr.x.NextPhraseTime, it.Name));
            }

            var notes = allNotes.Where(x => !x.Flags.HasFlag(Note.NoteMaskFlag.CHORD))
                .Select(FromSingleNote)
                .ToList();
            
            // try to ignore similar notes in chords
            notes.AddRange(allNotes.Where(x => x.Flags.HasFlag(Note.NoteMaskFlag.CHORD))
                .GroupBy(x => x.Time)
                .Select(x => FromChord(sng, x.Single()))); // hopefully this will throw when theres more than one note
            
            // parse any notes at exactly the same time and make them into a chord
            var groupedNotes = notes.Where(x => x.Notes.Count() == 1).GroupBy(x => x.Time).Where(x => x.Count() > 1).ToList();
            foreach (var n in groupedNotes.SelectMany(x => x)) {
                notes.Remove(n); // remove from original list
            }
            // add them back as chords
            foreach (var n in groupedNotes) {
                notes.Add(FromNotes(n));
            }
            
            return new Instrument(name, notes, sections, new InstrumentConfig(Instrument.DEFAULT_SONG_SCALE, sng.Metadata.Tuning, metadata?.Attributes?.CapoFret ?? 0, metadata?.Attributes?.CentOffset ?? 0));
        }

        private Lyrics ConvertVocals() {
            var vocals = _p.TOC.Entries.Where(x => (_nameFilter == null || x.Path.Contains(_nameFilter))
                    && x.Path.Contains(SongInfo.VOCALS_NAME) && x.Path.Contains(".sng")).FirstOrDefault();            
            if (vocals == null)
                return null;
            
            var sng = new SngAsset();
            using (var mem = new MemoryStream()) {
                _p.InflateEntry(vocals, mem);
                sng.ReadFrom(mem);
            }

            var lines = new List<LyricLine>();
            var cur = new List<Lyric>();
            foreach (var v in sng.Vocals) {
                cur.Add(new Lyric(v.Lyric.TrimEnd('+'), v.Time, v.Length));
                
                if (v.Lyric.EndsWith("+")) {
                    lines.Add(new LyricLine(cur));
                    cur = new List<Lyric>();
                }
            }

            return new Lyrics(lines);
        }

        public string ExportPsarcMainWem(string tempFolder, bool forceConvert = false)
        {
            var a = _p.ExtractArrangementManifests().First(x => (_nameFilter == null || x.Attributes.BlockAsset.Contains(_nameFilter)));
            var bnk_file = a.Attributes.SongBank;
            var result = _p.InflateEntry<BkhdAsset>(_p.TOC.Entries.First(x => x.Path.Contains(bnk_file)));

            var w = _p.TOC.Entries.Where(x => x.Path.Contains(result.GetWemId().ToString())).Single();

            Console.WriteLine(w.Path);
            var filePath = Path.Join(tempFolder, Path.GetFileName(w.Path));
            if (File.Exists(filePath) && !forceConvert) {
                Console.WriteLine("Skipping extracting wem file as it already exists.");
                return filePath;
            }
            if (forceConvert) { // delete all audio files then
                foreach (var f in new DirectoryInfo(tempFolder).GetFiles("*.wem"))
                    f.Delete();
                foreach (var f in new DirectoryInfo(tempFolder).GetFiles("*.ogg"))
                    f.Delete();
            }
            
            using var file = File.OpenWrite(filePath);
            _p.InflateEntry(w, file);
            file.Flush();
            
            Console.WriteLine(filePath);
            return filePath;
        }

        private static NoteBlock FromSingleNote(Note note)
        {
            var flags = ConvertToModel(note.Flags);

            SingleSlide? slideTo = null;
            if (note.SlideTo != 255)
                slideTo = new SingleSlide((int)note.SlideTo, false);
            if (note.SlideUnpitchTo != 255)
                slideTo = new SingleSlide((int)note.SlideUnpitchTo, true);

            var bendArray = FromBendData32(note.BendData, note.Time, note.Sustain);
            if ((bendArray == null || !bendArray.Any()) && flags.Contains(NoteType.BEND))
                flags = flags.Where(x => x != NoteType.BEND);
            //TODO pre bends
            var singleNote = new SingleNote(note.StringIndex, note.FretId, note.Sustain, flags, bendArray, slideTo);
            return new NoteBlock(note.Time, note.AnchorFretId, note.AnchorWidth, singleNote);
        }

        private static NoteBlock FromChord(SngAsset sng, Note note) {
            var sngChord = sng.Chords[note.ChordId];
            var chordFrets = sngChord.Frets;

            ChordNotes chordNotes = new ChordNotes();
            if (note.ChordNotesId != -1)
                chordNotes = sng.ChordNotes[note.ChordNotesId];

            var flags = ConvertToModel(note.Flags);
            // create note objects from the chord objects in the sng
            var singleNotes = new List<SingleNote>();
            for (int i = 0; i < chordFrets.Length; i++) {
                if (chordFrets[i] == 255)
                    continue;
                if (chordNotes.BendData == null) {
                    singleNotes.Add(new SingleNote(i, chordFrets[i], 0, flags, null, null));
                } else {
                    var mask = chordNotes.NoteMask.Length > i ? ConvertToModel((Note.NoteMaskFlag)chordNotes.NoteMask[i]) : null;
                    
                    SingleSlide? slideTo = null;
                    if (chordNotes.SlideTo.Length > i && chordNotes.SlideTo[i] != 255)
                        slideTo = new SingleSlide((int)chordNotes.SlideTo[i], false);
                    if (chordNotes.SlideUnpitchTo.Length > i && chordNotes.SlideUnpitchTo[i] != 255)
                        slideTo = new SingleSlide((int)chordNotes.SlideUnpitchTo[i], true);

                    var bendData = chordNotes.BendData.Length > i ? chordNotes.BendData[i].BendData32.ToArray() : null;
                    var bendArray = FromBendData32(bendData, note.Time, note.Sustain);
                    if ((bendArray == null || !bendArray.Any()) && mask.Contains(NoteType.BEND))
                        mask = mask.Where(x => x != NoteType.BEND);
                    // TODO vibrato
                    singleNotes.Add(new SingleNote(i, chordFrets[i], note.Sustain, flags.Union(mask).Distinct(), bendArray, slideTo));
                }
            }
            var chordNoteFlags = new List<NoteBlockFlags>();
            if (singleNotes.All(x => x.Type.Contains(NoteType.MUTE)))
                chordNoteFlags.Add(NoteBlockFlags.MUTE);
            if (singleNotes.All(x => x.Type.Contains(NoteType.FRETHANDMUTE)))
                chordNoteFlags.Add(NoteBlockFlags.MUTE);

            if (!singleNotes.Any()) {
                // fix no-note chords failing to import
                singleNotes.Add(new SingleNote(0, 0, 0, null, null, null));
            }

            return new NoteBlock(note.Time, note.AnchorFretId, note.AnchorWidth, singleNotes, chordNoteFlags.Distinct()) {
                Label = sngChord.Name
            };
        }

        private static NoteBlock FromNotes(IEnumerable<NoteBlock> notes)
        {
            var singleNotes = new List<SingleNote>();
            singleNotes.AddRange(notes.SelectMany(x => x.Notes));
            foreach (var n in singleNotes) {
                n.Type.Append(NoteType.CHORD);
            }
            return new NoteBlock(notes.First().Time, notes.First().FretWindowStart, notes.First().FretWindowLength, singleNotes, null);
        }

        private static IEnumerable<NoteType> ConvertToModel(Note.NoteMaskFlag flag)
        {
            // if (flag.HasFlag(Note.NoteMaskFlag.UNDEFINED)) yield return NoteType.UNDEFINED;
            if (flag.HasFlag(Note.NoteMaskFlag.MISSING)) yield return NoteType.MISSING;
            if (flag.HasFlag(Note.NoteMaskFlag.CHORD)) yield return NoteType.CHORD;
            if (flag.HasFlag(Note.NoteMaskFlag.OPEN)) yield return NoteType.OPEN;
            if (flag.HasFlag(Note.NoteMaskFlag.FRETHANDMUTE)) yield return NoteType.FRETHANDMUTE;
            if (flag.HasFlag(Note.NoteMaskFlag.TREMOLO)) yield return NoteType.TREMOLO;
            if (flag.HasFlag(Note.NoteMaskFlag.HARMONIC)) yield return NoteType.HARMONIC;
            if (flag.HasFlag(Note.NoteMaskFlag.PALMMUTE)) yield return NoteType.PALMMUTE;
            if (flag.HasFlag(Note.NoteMaskFlag.SLAP)) yield return NoteType.SLAP;
            if (flag.HasFlag(Note.NoteMaskFlag.PLUCK)) yield return NoteType.PLUCK;
            if (flag.HasFlag(Note.NoteMaskFlag.POP)) yield return NoteType.POP;
            if (flag.HasFlag(Note.NoteMaskFlag.HAMMERON)) yield return NoteType.HAMMERON;
            if (flag.HasFlag(Note.NoteMaskFlag.PULLOFF)) yield return NoteType.PULLOFF;
            if (flag.HasFlag(Note.NoteMaskFlag.SLIDE)) yield return NoteType.SLIDE;
            if (flag.HasFlag(Note.NoteMaskFlag.BEND)) yield return NoteType.BEND;
            if (flag.HasFlag(Note.NoteMaskFlag.SUSTAIN)) yield return NoteType.SUSTAIN;
            if (flag.HasFlag(Note.NoteMaskFlag.TAP)) yield return NoteType.TAP;
            if (flag.HasFlag(Note.NoteMaskFlag.PINCHHARMONIC)) yield return NoteType.PINCHHARMONIC;
            if (flag.HasFlag(Note.NoteMaskFlag.VIBRATO)) yield return NoteType.VIBRATO;
            if (flag.HasFlag(Note.NoteMaskFlag.MUTE)) yield return NoteType.MUTE;
            if (flag.HasFlag(Note.NoteMaskFlag.IGNORE)) yield return NoteType.IGNORE;
            if (flag.HasFlag(Note.NoteMaskFlag.LEFTHAND)) yield return NoteType.LEFTHAND;
            if (flag.HasFlag(Note.NoteMaskFlag.RIGHTHAND)) yield return NoteType.RIGHTHAND;
            if (flag.HasFlag(Note.NoteMaskFlag.HIGHDENSITY)) yield return NoteType.HIGHDENSITY;
            if (flag.HasFlag(Note.NoteMaskFlag.SLIDEUNPITCHEDTO)) yield return NoteType.SLIDEUNPITCHEDTO;
            if (flag.HasFlag(Note.NoteMaskFlag.SINGLE)) yield return NoteType.SINGLE;
            if (flag.HasFlag(Note.NoteMaskFlag.CHORDNOTES)) yield return NoteType.CHORDNOTES;
            if (flag.HasFlag(Note.NoteMaskFlag.DOUBLESTOP)) yield return NoteType.DOUBLESTOP;
            if (flag.HasFlag(Note.NoteMaskFlag.ACCENT)) yield return NoteType.ACCENT;
            if (flag.HasFlag(Note.NoteMaskFlag.PARENT)) yield return NoteType.PARENT;
            if (flag.HasFlag(Note.NoteMaskFlag.CHILD)) yield return NoteType.CHILD;
            if (flag.HasFlag(Note.NoteMaskFlag.ARPEGGIO)) yield return NoteType.ARPEGGIO;
            //if (flag.HasFlag(Note.NoteMaskFlag.MISSING2)) yield return NoteType.MISSING2;
            if (flag.HasFlag(Note.NoteMaskFlag.STRUM)) yield return NoteType.STRUM;
        }

        private static ICollection<SingleBend> FromBendData32(BendData32[] data, float noteStart, float sustainLength)
        {
            if (data == null || data.Length < 1)
                return null;
            
            var actualData = data.Where(x => x.Time != 0).Where(x => x.Time >= noteStart);
            if (!actualData.Any())
                return null;
            
            var bendList = actualData.Select(x => new SingleBend(x.Step, x.Time)).ToList();
            if (Math.Abs(noteStart + sustainLength - bendList.Last().Time) > 0.1f) {
                // add a remaining bend return when it ends
                bendList.Add(new SingleBend(0, noteStart + sustainLength));
            }
            
            return bendList;
        }
    }
}
