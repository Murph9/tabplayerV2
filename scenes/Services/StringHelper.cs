public static class StringHelper
{
    public static string FixedWidthString(this string str, int length) {
        if (str == null || length < 1) return null;
        if (str.Length > length) return str.Substring(0, length);
        return str.PadRight(length);
    }        
}
