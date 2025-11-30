namespace ParallaxEditor
{
    internal static class StringExt
    {
        public static string StripPrefix(this string s, string prefix)
        {
            if (s.StartsWith(prefix))
                return s.Substring(prefix.Length);
            return s;
        }
    }
}
