namespace murph9.TabPlayer.scenes.Services;

public static class StringHelper
{
    public static string FixedWidthString(this string str, int length) {
        if (str == null || length < 1) return null;
        if (str.Length > length) return str[..length];
        return str.PadRight(length);
    }
}
