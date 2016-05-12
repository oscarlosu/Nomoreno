using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Assets.Scripts {
    public static class ClueConverter {
        public static string GetClueTemplate(ClueIdentifier identifier) {
            List<string> relevantClues = IO.FileLoader.GetTemplates(identifier);
            int index = PlayerController.Instance.Rng.Next(relevantClues.Count);
            return relevantClues[index];
        }

        public static string GetLocationSubTemplate(bool isAlibi) {
            List<string> relevantClues = isAlibi ? IO.FileLoader.GetAlibis() : IO.FileLoader.GetIncriminates();
            int index = PlayerController.Instance.Rng.Next(relevantClues.Count);
            return relevantClues[index];
        }
        
        private static string CocknifyPronoun(string pronoun) {
            if (pronoun == "he" || pronoun == "his" || pronoun == "him" || pronoun == "hers" || pronoun == "her")
                return pronoun.Replace("h", "'");
            else
                return pronoun;
        }
    }
}
