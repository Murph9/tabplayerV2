using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace murph9.TabPlayer.Songs.Models;

public class Instrument {
    public string Name { get; }
    public float LastNoteTime => Notes?.Last().Time ?? 0;
    public InstrumentConfig Config { get; }
    public IReadOnlyCollection<NoteBlock> Notes { get; }
    public IReadOnlyCollection<Section> Sections { get; }

    [JsonIgnore]
    public SimulationControlData ControlData { get; private set; }

    [JsonConstructor]
    public Instrument(string name, IEnumerable<NoteBlock> notes, IEnumerable<Section> sections, InstrumentConfig config) {
        Name = name;
        Notes = notes.OrderBy(x => x.Time).ToList();
        Config = config;

        Sections = sections.ToList();

        ControlData = CalcControlData(Notes);
    }

    public int TotalNoteCount() => Notes.Count;
    public int ChordCount() => Notes.Count(x => x.IsChord);
    public int SingleNoteCount() => Notes.Count(x => !x.IsChord);

    public float GetNoteDensity(SongInfo songInfo) => TotalNoteCount() / songInfo.Metadata.SongLength;

    public static string CalcTuningName(short[] t, float? capoFret) {
        if (capoFret.HasValue && capoFret != 0 && capoFret != 255) {
            return CalcTuningName(t).Replace(" Standard", string.Empty) + $", Capo: {capoFret}";
        }
        return CalcTuningName(t);
    }

    public static string CalcTuningName(short[] t) {
        if (t == null)
            return "<unknown>";

        // https://en.wikipedia.org/wiki/List_of_guitar_tunings
        if (Enumerable.SequenceEqual(t, new short[] { 1, 1, 1, 1, 1, 1 }))
            return "F Standard";
        if (Enumerable.SequenceEqual(t, new short[] { 0, 0, 0, 0, 0, 0 }))
            return "E Standard";
        if (Enumerable.SequenceEqual(t, new short[] { -2, 0, 0, 0, 0, 0 }))
            return "Drop D";
        if (Enumerable.SequenceEqual(t, new short[] { -1, -1, -1, -1, -1, -1 }))
            return "Eb Standard";
        if (Enumerable.SequenceEqual(t, new short[] { -3, -1, -1, -1, -1, -1 }))
            return "Eb Drop Db";
        if (Enumerable.SequenceEqual(t, new short[] { -2, -2, -2, -2, -2, -2 }))
            return "D Standard";
        if (Enumerable.SequenceEqual(t, new short[] { -4, -2, -2, -2, -2, -2 }))
            return "D Drop C";
        if (Enumerable.SequenceEqual(t, new short[] { -3, -3, -3, -3, -3, -3 }))
            return "Db Standard";
        if (Enumerable.SequenceEqual(t, new short[] { -5, -3, -3, -3, -3, -3 }))
            return "Db Drop B";
        if (Enumerable.SequenceEqual(t, new short[] { -4, -4, -4, -4, -4, -4 }))
            return "C Standard";
        if (Enumerable.SequenceEqual(t, new short[] { -6, -4, -4, -4, -4, -4 }))
            return "C Drop Bb";
        if (Enumerable.SequenceEqual(t, new short[] { -5, -5, -5, -5, -5, -5 }))
            return "B Standard";

        return string.Join(",", t.Select((x, i) => NoteForIndexAndOffset(i, x)));
    }

    private static SimulationControlData CalcControlData(IEnumerable<NoteBlock> notes) {
        var noteCenterAt = new List<CenterFret>();
        var lastFret = -1f;
        var lastWidth = -1f;
        foreach (var n in notes) {
            if (lastFret != n.FretWindowStart || lastWidth != n.FretWindowLength) {
                noteCenterAt.Add(new CenterFret(n.Time, n.FretWindowStart, n.FretWindowLength));
                lastFret = n.FretWindowStart;
                lastWidth = n.FretWindowLength;
            }
        }

        return new SimulationControlData() {
            NoteCenterFret = noteCenterAt
        };
    }

    private readonly static int[] NOTE_OFFSET = new int[] { 0, 5, 10, 3, 7, 0 };
    private readonly static string[] NOTE_LIST = new string[] {
            "E", "F", "Gb", "G", "Ab", "A", "Bb", "B", "C", "Db", "D", "Eb"
        };
    private static string NoteForIndexAndOffset(int index, int offset) {
        var pos = NOTE_OFFSET[index] + offset;
        while (pos < 0) {
            pos += NOTE_LIST.Length;
        }
        var result = NOTE_LIST[pos % NOTE_LIST.Length];
        if (index == 5)
            return result.ToLowerInvariant();
        return result;
    }
}

public class SongInfo {
    public const string LEAD_NAME = "lead";
    public const string LEAD1_NAME = "lead1";
    public const string LEAD2_NAME = "lead2";
    public const string COMBO_NAME = "combo";
    public const string COMBO1_NAME = "combo1";
    public const string COMBO2_NAME = "combo2";
    public const string COMBO3_NAME = "combo3";
    public const string COMBO4_NAME = "combo4";
    public const string RHYTHM_NAME = "rhythm";
    public const string RHYTHM1_NAME = "rhythm1";
    public const string RHYTHM2_NAME = "rhythm2";
    public const string BASS_NAME = "bass";
    public const string BASS1_NAME = "bass1";
    public const string BASS2_NAME = "bass2";
    public const string VOCALS_NAME = "vocals";
    public static readonly string[] STANDARD_INSTRUMENT_TYPES = new string[] { LEAD_NAME, RHYTHM_NAME, BASS_NAME };

    public static readonly Dictionary<string, int> INSTRUMENT_ORDER = new() {
        {LEAD_NAME, 0},
        {LEAD1_NAME, 1},
        {LEAD2_NAME, 2},
        {COMBO_NAME, 3},
        {COMBO1_NAME, 4},
        {COMBO2_NAME, 5},
        {COMBO3_NAME, 6},
        {COMBO4_NAME, 7},
        {RHYTHM_NAME, 8},
        {RHYTHM1_NAME, 9},
        {RHYTHM2_NAME, 10},
        {BASS_NAME, 11},
        {BASS1_NAME, 12},
        {BASS2_NAME, 13},
        {VOCALS_NAME, 14},
    };

    [JsonIgnore]
    public Instrument MainInstrument { get; private set; }
    public ICollection<Instrument> Instruments { get; private set; }

    public Lyrics Lyrics { get; private set; }

    public SongMetadata Metadata { get; private set; }

    [JsonConstructor]
    public SongInfo(SongMetadata metadata, IEnumerable<Instrument> instruments, Lyrics lyrics) {
        Metadata = metadata;
        Instruments = instruments.ToArray() ?? Array.Empty<Instrument>();
        Lyrics = lyrics ?? new Lyrics(new List<LyricLine>());

        MainInstrument = Instruments.FirstOrDefault(x => x.Name == LEAD_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == LEAD1_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == LEAD2_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == RHYTHM_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == RHYTHM1_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == RHYTHM2_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == BASS_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == BASS1_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == BASS2_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == COMBO_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == COMBO1_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == COMBO2_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == COMBO3_NAME);
        MainInstrument ??= Instruments.FirstOrDefault(x => x.Name == COMBO4_NAME);

        MainInstrument = Instruments.FirstOrDefault(); // default to the first one when we have no option

        if (MainInstrument == null)
            throw new Exception("No instrument found");

        SplitLyricsIntoLines();
    }

    private void SplitLyricsIntoLines() {
        // a little hack which forces single line lyric songs to be multiple lines
        if (Lyrics.Lines.Count != 1) {
            return;
        }

        var list = new List<LyricLine>();
        var blockStartTime = float.MaxValue;
        List<Lyric> lyricLineWords = null;
        foreach (var word in Lyrics.Lines.First().Words) {
            if (blockStartTime == float.MaxValue || word.Time > blockStartTime + 10) { // max 10 sec line
                blockStartTime = word.Time;
                if (lyricLineWords != null)
                    list.Add(new LyricLine(lyricLineWords));
                lyricLineWords = new List<Lyric>();
            }
            lyricLineWords.Add(word);
        }

        Lyrics = new Lyrics(list);
    }
}

public record struct InstrumentConfig(float NoteSpeed, short[] Tuning, float CapoFret, float CentOffset);

public class SongMetadata {
    public string Name { get; private set; }
    public string Artist { get; private set; }
    public string Album { get; private set; }
    public int? Year { get; private set; }
    public float SongLength { get; private set; }
    public string Genre { get; private set; }

    public SongMetadata(string name, string artist, string album, int? year, float songLength) : this(name, artist, album, year, songLength, null) { }

    [JsonConstructor]
    public SongMetadata(string name, string artist, string album, int? year, float songLength, string genre) {
        Name = name;
        Artist = artist;
        Album = album;
        Year = year;
        SongLength = songLength;
        Genre = genre;
    }
}

public class SimulationControlData {
    public ICollection<CenterFret> NoteCenterFret { get; set; } = new List<CenterFret>();
}

public record struct CenterFret(float Time, int Fret, int WindowSize);

public record struct Section(float StartTime, float EndTime, string Name);
