namespace Granite.Infrastructure.Common
{
    public static class StringExtensions
    {
        public static string ToFormat(this string content,
            params object[] replacements)
        {
            return string.Format(content, replacements);
        }
    }
}