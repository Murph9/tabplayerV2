using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace murph9.TabPlayer.Songs.Models;

public class Instrument
{
    public static readonly float DEFAULT_SONG_SCALE = 40f;
    public string Name { get; private set; }
    public float LastNoteTime => Notes?.Last().Time ?? 0;
    public InstrumentConfig Config { get; private set; }
    public IReadOnlyCollection<NoteBlock> Notes { get; private set; }
    public IReadOnlyCollection<Section> Sections { get; private set; }

    [JsonIgnore]
    public SimulationControlData ControlData { get; private set; }

    [JsonConstructor]
    public Instrument(string name, IEnumerable<NoteBlock> notes, IEnumerable<Section> sections, InstrumentConfig config)
    {
        this.Name = name;
        this.Notes = notes.OrderBy(x => x.Time).ToList();
        this.Config = config;

        this.Sections = sections.ToList();

        this.ControlData = CalcControlData(this.Notes);
    }

    public string CalcTuningName()
    {
        var t = this.Config.Tuning;
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
            return "Eb Drop D";
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


    public string CalcFullTuningStr()
    {
        if (Config.CapoFret != 0 && Config.CapoFret != 255)
        {
            return CalcTuningName().Replace(" Standard", string.Empty) + $", Capo: {Config.CapoFret}";
        }

        return CalcTuningName();
    }

    private static SimulationControlData CalcControlData(IEnumerable<NoteBlock> notes)
    {
        var noteCenterAt = new List<CenterFret>();
        var lastFret = -1f;
        var lastWidth = -1f;
        foreach (var n in notes)
        {
            if (lastFret != n.FretWindowStart || lastWidth != n.FretWindowLength)
            {
                noteCenterAt.Add(new CenterFret(n.Time, n.FretWindowStart, n.FretWindowLength));
                lastFret = n.FretWindowStart;
                lastWidth = n.FretWindowLength;
            }
        }

        return new SimulationControlData()
        {
            NoteCenterFret = noteCenterAt
        };
    }

    private static int[] NOTE_OFFSET = new int[] { 0, 5, 10, 3, 7, 0 };
    private static string[] NOTE_LIST = new string[] {
            "E", "F", "Gb", "G", "Ab", "A", "Bb", "B", "C", "Db", "D", "Eb"
        };
    private static string NoteForIndexAndOffset(int index, int offset)
    {
        var pos = NOTE_OFFSET[index] + offset;
        while (pos < 0)
        {
            pos += NOTE_LIST.Length;
        }
        var result = NOTE_LIST[pos % NOTE_LIST.Length];
        if (index == 5)
            return result.ToLowerInvariant();
        return result;
    }
}

public class SongInfo
{
    public const string LEAD_NAME = "lead";
    public const string COMBO_NAME = "combo";
    public const string COMBO1_NAME = "combo1";
    public const string COMBO2_NAME = "combo2";
    public const string RHYTHM_NAME = "rhythm";
    public const string BASS_NAME = "bass";
    public const string VOCALS_NAME = "vocals";
    public static string[] STANDARD_INSTRUMENT_TYPES = new string[] { LEAD_NAME, RHYTHM_NAME, BASS_NAME };

    [JsonIgnore]
    public Instrument MainInstrument { get; private set; }
    public ICollection<Instrument> Instruments { get; private set; }

    public Lyrics Lyrics { get; private set; }

    public SongMetadata Metadata { get; private set; }

    [JsonConstructor]
    public SongInfo(SongMetadata metadata, IEnumerable<Instrument> instruments, Lyrics lyrics)
    {
        Metadata = metadata;
        Instruments = instruments.ToArray() ?? new Instrument[0];
        Lyrics = lyrics ?? new Lyrics(new List<LyricLine>());

        MainInstrument = Instruments.FirstOrDefault(x => x.Name == SongInfo.LEAD_NAME);
        if (MainInstrument == null) MainInstrument = Instruments.FirstOrDefault(x => x.Name == SongInfo.RHYTHM_NAME);
        if (MainInstrument == null) MainInstrument = Instruments.FirstOrDefault(x => x.Name == SongInfo.BASS_NAME);
        if (MainInstrument == null) MainInstrument = Instruments.FirstOrDefault(x => x.Name == SongInfo.COMBO_NAME);
        if (MainInstrument == null) MainInstrument = Instruments.FirstOrDefault(x => x.Name == SongInfo.COMBO1_NAME);
        if (MainInstrument == null) MainInstrument = Instruments.FirstOrDefault(x => x.Name == SongInfo.COMBO2_NAME);

        if (MainInstrument == null)
            throw new Exception("No instrument found");
    }
}

public record struct InstrumentConfig(float NoteSpeed, short[] Tuning, float CapoFret, float CentOffset);

public class SongMetadata
{
    public string Name { get; private set; }
    public string Artist { get; private set; }
    public string Album { get; private set; }
    public int? Year { get; private set; }
    public float SongLength { get; private set; }
    //TODO genre

    [JsonConstructor]
    public SongMetadata(string name, string artist, string album, int? year, float songLength)
    {
        this.Name = name;
        this.Artist = artist;
        this.Album = album;
        this.Year = year;
        this.SongLength = songLength;
    }
}

public class SimulationControlData
{
    public ICollection<CenterFret> NoteCenterFret { get; set; } = new List<CenterFret>();
}

public record struct CenterFret(float Time, int Fret, int WindowSize);

public record struct Section(float StartTime, float EndTime, string Name);
