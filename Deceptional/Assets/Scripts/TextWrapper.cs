using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts {
    public static class TextWrapper {
        public static int DefaultMaxLength = 20; // 20 characters.


        public static string BreakLine(string s, int maxLen) {
            var retString = SplitByLength(s, maxLen).Aggregate((wS, n) => wS + "\n" + n);

            return retString;
        }

        public static string BreakLine(string s) {
            var retString = SplitByLength(s, DefaultMaxLength).Aggregate((wS, n) => wS + "\n" + n);

            return retString;
        }

        private static IEnumerable<string> SplitByLength(string input, int maxLen) {
            return Regex.Split(input, @"(.{1," + maxLen + @"})(?:\s|$)").Where(x => x.Length > 0).Select(x => x.Trim());
        }
    }
}
