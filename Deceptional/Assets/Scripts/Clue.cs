using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Assets.Scripts {
    public struct Clue {
        public string Statement { get { return ConstructClue(); } }
        public string Template { get; set; }
        public NPC Target { get; set; }
        public ClueIdentifier Identifier { get; set; }
        public NPCPart.NPCPartType NPCPartType { get; set; }
        public NPCPart.NPCPartDescription NPCDescription { get; set; }

        public Clue(string template, NPC target, ClueIdentifier identifier, NPCPart.NPCPartType targetPart) {
            Template = template;
            Target = target;
            Identifier = identifier;
            NPCPartType = targetPart;
            switch (NPCPartType) {
                case NPCPart.NPCPartType.None:  NPCDescription = NPCPart.NPCPartDescription.Black; break; // Black used as placeholder, value should be irrelevant.
                case NPCPart.NPCPartType.Hat:   NPCDescription = Target.Head.Description; break;
                case NPCPart.NPCPartType.Shirt: NPCDescription = Target.Torso.Description; break;
                case NPCPart.NPCPartType.Pants: NPCDescription = Target.Legs.Description; break;
                default: throw new Exception("NPCPart unidentifiable");
            }
        }

        /// <summary>
        /// Constructs a statement using all custom parameters.
        /// </summary>
        /// <returns>Returns the fully generated statement.</returns>
        public string ConstructClue() {
            StringBuilder clueBuilder = new StringBuilder(Template);
            clueBuilder.Remove(0, 3);

            // Inserting correct pronoun
            var matches = cluePattern.Matches(Template);
            string usedPronoun = string.Empty;
            foreach (Match m in matches) {
                if (m.Groups[4].Success) {
                    int pronounIdx = int.Parse(Regex.Match(m.Groups[4].Value, @"\d+").Value);
                    usedPronoun = Target.IsMale ? malePronouns[pronounIdx - 1] : femalePronouns[pronounIdx - 1];
                    usedPronoun = useCockney ? CocknifyPronoun(usedPronoun) : usedPronoun;
                    clueBuilder.Replace(m.Groups[4].Value, usedPronoun);
                }
            }

            // Replacing abstract tokens with format tokens
            clueBuilder.Replace(@"[X]", NPCDescription.ToString());
            clueBuilder.Replace(@"[Y]", NPCPartType.ToString());
            clueBuilder.Replace(@"[Z]", Target.Name);

            return clueBuilder.ToString().Trim();
        }

        #region Static fields
        private static bool useCockney = true;
        private static Regex cluePattern = new Regex(@"(\[X\])|(\[Y\])|(\[Z\])|(\[Q\[\d\]\])");

        #region Pronouns
        private static List<string> malePronouns = new List<string>() { "man", "he", "his", "men", "him", "Mister" };
        private static List<string> femalePronouns = new List<string>() { "woman", "she", "hers", "women", "her", "Miss" };
        #endregion

        private static string CocknifyPronoun(string pronoun) {
            if (pronoun == "he" || pronoun == "his" || pronoun == "him" || pronoun == "hers" || pronoun == "her")
                return pronoun.Replace("h", "'");
            else
                return pronoun;
        }
        #endregion
    }
}
