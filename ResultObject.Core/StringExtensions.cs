using System.Text.RegularExpressions;

namespace ResultObject.Core
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string value)
        {
            var noCaps = Regex.Replace(value, "(?:[A-Z]+)", match => match.Index > 0
                ? $"-{match.Value.ToLowerInvariant()}"
                : match.Value.ToLowerInvariant());

            var noDashes = Regex.Replace(noCaps, "-+", "_");
            var noDoubleUnderscores = Regex.Replace(noDashes, "_{2,}", "_");
            var noMessagePrefix = Regex.Replace(noDoubleUnderscores, "^message-", "", RegexOptions.IgnoreCase);
            return noMessagePrefix;
          
        }
        
        public static string ToKebabCase(this string value)
        {
            var noCaps = Regex.Replace(value, "(?:[A-Z]+)", match => match.Index > 0
                ? $"-{match.Value.ToLowerInvariant()}"
                : match.Value.ToLowerInvariant());

            var noUnderscores = Regex.Replace(noCaps, "_+", "-");
            var noDoubleDashes = Regex.Replace(noUnderscores, "-{2,}", "-");
            var noMessagePrefix = Regex.Replace(noDoubleDashes, "^message-", "", RegexOptions.IgnoreCase);
            return noMessagePrefix;
        }
    }
}