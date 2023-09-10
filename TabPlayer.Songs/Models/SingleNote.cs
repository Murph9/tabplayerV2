using System;
using System.Collections.Generic;

namespace murph9.TabPlayer.Songs.Models;

public class SingleNote
{
    public int StringNum { get; private set; }
    public int FretNum { get; private set; }
    public float Length { get; private set; }
    public IEnumerable<NoteType> Type { get; private set; }
    public ICollection<SingleBend> Bends { get; private set; }
    public SingleSlide? Slide { get; private set; }
    public SingleNote(int stringNum, int fretNum, float length, IEnumerable<NoteType> type, ICollection<SingleBend> bends, SingleSlide? slide)
    {
        StringNum = stringNum;
        FretNum = fretNum;
        Length = length;
        Type = type ?? Array.Empty<NoteType>();
        Bends = bends;
        Slide = slide;

        if (Slide != null && Length <= 0)
        {
            // Slide without a length for note, setting as no slide
            Slide = null;
        }
    }
}

public record struct SingleBend(float Step, float Time);
public record struct SingleSlide(int ToFret, bool SlideUnpitched);
