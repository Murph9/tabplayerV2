using Godot;

public class DisplayConst
{
    private static Dictionary<int, Color> colourMap = new Dictionary<int, Color>()
    {
        {0, Colors.Red},
        {1, Colors.Yellow},
        {2, Colors.Blue},
        {3, Colors.Orange},
        {4, Colors.Green},
        {5, Colors.Purple},
    };
    public static Color GetColorFromString(int stringNum)
    {
        return colourMap[stringNum];
    }

    public const float TRACK_BOTTOM_WORLD = -0.5f;
    public const float STRING_DISTANCE_APART = 1;

    /*
    |---fret---|
    <----------> = fret width
         xx      = fret in pos
               x = fret pos
    */
    public static float CalcFretPosZ(int fret)
    {
        return fret; //TODO make frets get smaller http://www.buildyourguitar.com/resources/tips/fretdist.htm
    }

    public static float CalcInFretPosZ(int fret)
    {
        return CalcFretPosZ(fret - 1) + (CalcFretPosZ(fret) - CalcFretPosZ(fret - 1)) / 2f;
    }

    public static float CalcFretWidthZ(int fret, int width = 1)
    {
        return CalcFretPosZ(fret + width - 1) - CalcFretPosZ(fret - 1);
    }

    public static float CalcMiddleWindowZ(int fretStart, int windowLength) {
        return CalcFretPosZ(fretStart - 1) + CalcFretWidthZ(fretStart, windowLength)/2f;
    }
}