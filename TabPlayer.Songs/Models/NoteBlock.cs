using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace murph9.TabPlayer.Songs.Models;

public class NoteBlock {
    public string? Label { get; set; }
    public float Time { get; set; }
    [JsonProperty("fws")] // these take up quite the space in the json file
    public int FretWindowStart { get; set; }
    [JsonProperty("fwl")] // these take up quite the space in the json file
    public int FretWindowLength { get; set; }
    public IEnumerable<NoteBlockFlags> ChordFlags { get; set; }
    public IEnumerable<SingleNote> Notes { get; set; }
    public bool IsChord => Notes.Count() > 1;

    public NoteBlock(float time, int fretWindowStart, int fretWindowLength, SingleNote note) : this(time, fretWindowStart, fretWindowLength, new SingleNote[] { note }, null) { }
    [JsonConstructor]
    public NoteBlock(float time, int fretWindowStart, int fretWindowLength, IEnumerable<SingleNote> notes, IEnumerable<NoteBlockFlags> chordFlags) {
        if (notes == null || !notes.Any())
            throw new ArgumentOutOfRangeException(nameof(notes), "must not be empty");

        Time = time;
        FretWindowStart = fretWindowStart;
        FretWindowLength = fretWindowLength;
        Notes = notes;
        ChordFlags = chordFlags ?? Array.Empty<NoteBlockFlags>();

        if (FretWindowLength < 1) FretWindowLength = 4;
    }

    private readonly static NoteType[] IGNORED_TYPES = new NoteType[] { NoteType.UNDEFINED, NoteType.MISSING, NoteType.CHORD, NoteType.OPEN, NoteType.IGNORE, NoteType.HIGHDENSITY, NoteType.SINGLE, NoteType.CHORDNOTES, NoteType.DOUBLESTOP, NoteType.MISSING2, NoteType.STRUM, NoteType.ACCENT };

    public bool IsSameChordAs(NoteBlock other) {
        if (other == null)
            return false;

        var thisHasStuff = Notes.Any(x => (x.Bends != null && x.Bends.Any()) || x.Slide != null);
        if (thisHasStuff) return false;

        var otherHasStuff = other.Notes.Any(x => (x.Bends != null && x.Bends.Any()) || x.Slide != null);
        if (otherHasStuff) return false;

        if (Notes.Count() != other.Notes.Count())
            return false;

        if (!Enumerable.SequenceEqual(ChordFlags, other.ChordFlags))
            return false;

        using var thisE = Notes.GetEnumerator();
        using var otherE = other.Notes.GetEnumerator();
        while (thisE.MoveNext() && otherE.MoveNext()) {
            if (thisE.Current.FretNum != otherE.Current.FretNum)
                return false;
            if (thisE.Current.StringNum != otherE.Current.StringNum)
                return false;

            var typesThis = thisE.Current.Type.Except(IGNORED_TYPES);
            var typesOther = otherE.Current.Type.Except(IGNORED_TYPES);
            if (!Enumerable.SequenceEqual(typesThis, typesOther))
                return false;
        }

        return true;
    }
}
