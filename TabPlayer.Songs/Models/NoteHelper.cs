using System.Collections.Generic;
using System.Linq;

namespace murph9.TabPlayer.Songs.Models;

public class NoteHelper
{
    public static IEnumerable<string> GetSymbol(SingleNote note)
    {
        if (note.Type.Contains(NoteType.HAMMERON))
            yield return "h";

        if (note.Type.Contains(NoteType.PULLOFF))
            yield return "p";

        // if (note.Type.Contains(NoteType.SLIDE))
            // yield return "s";

        // if (note.Type.Contains(NoteType.TREMOLO))
            // yield return "tr";

        if (note.Type.Contains(NoteType.BEND))
            yield return "b" + note.Bends.Max(x => (int)x.Step);

        if (note.Type.Contains(NoteType.LEFTHAND))
            yield return "L";

        // if (note.Type.Contains(NoteType.SUSTAIN))
        // yield return "@";//+Math.Round(note.Length, 2);

        if (note.Type.Contains(NoteType.MUTE) || note.Type.Contains(NoteType.PALMMUTE))
            yield return "x";

        if (note.Type.Contains(NoteType.TAP))
            yield return "T";

        // if (note.Type.Contains(NoteType.VIBRATO))
            // yield return "~";

        if (note.Type.Contains(NoteType.HARMONIC))
            yield return "H";

        if (note.Type.Contains(NoteType.PINCHHARMONIC))
            yield return "o";

        if (note.Type.Contains(NoteType.FRETHANDMUTE))
            yield return ".";

        // if (note.Type.Contains(NoteType.SLIDEUNPITCHEDTO) && note.Slide.HasValue && note.Slide.Value.SlideUnpitched && note.FretNum > note.Slide.Value.ToFret)
            // yield return "\\";

        // if (note.Type.Contains(NoteType.SLIDEUNPITCHEDTO) && note.Slide.HasValue && note.Slide.Value.SlideUnpitched && note.FretNum < note.Slide.Value.ToFret)
            // yield return "/";

        // if (note.Type.Contains(NoteType.OPEN))
            // yield return "-"; //why would i need to know?
    }
}
