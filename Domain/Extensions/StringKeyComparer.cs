using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Domain.Extensions
{
    public static class StringKeyComparer
    {
        public static bool CompareStrings(string s1, string s2)
        {
            string Normalize(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return "";

                input = input.Trim().ToLowerInvariant();
                string normalized = input.Normalize(NormalizationForm.FormD);

                StringBuilder sb = new StringBuilder();
                foreach (char c in normalized)
                {
                    if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                        sb.Append(c);
                }

                return Regex.Replace(sb.ToString(), @"\s+", "");
            }

            return Normalize(s1) == Normalize(s2);
        }
    }
}
