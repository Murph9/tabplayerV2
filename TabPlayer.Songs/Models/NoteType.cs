namespace murph9.TabPlayer.Songs.Models;

public enum NoteBlockFlags
{
    UNDEFINED = 0,
    MUTE = 1,

}

public enum NoteType
{
    UNDEFINED = 0,
    MISSING = 1,
    CHORD = 2,
    OPEN = 3,
    FRETHANDMUTE = 4,
    TREMOLO = 5,
    HARMONIC = 6,
    PALMMUTE = 7,
    SLAP = 8,
    PLUCK = 9,
    POP = 10,
    HAMMERON = 11,
    PULLOFF = 12,
    SLIDE = 13,
    BEND = 14,
    SUSTAIN = 15,
    TAP = 16,
    PINCHHARMONIC = 17,
    VIBRATO = 18,
    MUTE = 19,
    IGNORE = 20,
    LEFTHAND = 21,
    RIGHTHAND = 22,
    HIGHDENSITY = 23,
    SLIDEUNPITCHEDTO = 24,
    SINGLE = 25,
    CHORDNOTES = 26,
    DOUBLESTOP = 27,
    ACCENT = 28,
    PARENT = 29,
    CHILD = 30,
    ARPEGGIO = 31,
    MISSING2 = 32,
    STRUM = 33,
}
