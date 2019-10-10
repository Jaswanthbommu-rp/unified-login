using System.Text.RegularExpressions;

namespace RP.Enterprise.Foundation.Audit.Core.Component
{
    internal static class StringExtensions
    {
        public static string RpToLowerWithUnderscores(this string source)
        {
            // PmcId ==> pmc_id
            return Regex.Replace(source, @"(\p{Ll})(\p{Lu})", "$1_$2").ToLower();
        }
    }
}
