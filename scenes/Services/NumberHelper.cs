namespace murph9.TabPlayer.scenes.Services;

public static class NumberHelper {
    public static string ToMinSec(this float value, bool frac = false) {
        return ((double)value).ToMinSec(frac);
    }
    public static string ToMinSec(this double value, bool frac = false) {
        var fracStr = frac ? $" {(int)(((decimal)value % 1) * 1000)}ms" : null;
        return $"{Math.Floor(value/60f)}m {Math.Floor(value % 60)}s{fracStr}";
    }

    public static string ToFixedPlaces(this float value, int count, bool leadChar) {
        return ((double)value).ToFixedPlaces(count, leadChar);
    }

    public static string ToFixedPlaces(this double value, int count, bool leadChar) {
        if (count < 0) count = 0;
        var zeros = new string('0', count);
        if (leadChar)
            return Math.Round(value, count).ToString($"+0.{zeros};-0.{zeros};0");
        return Math.Round(value, count).ToString($"0.{zeros};-0.{zeros};0");
    }
}
