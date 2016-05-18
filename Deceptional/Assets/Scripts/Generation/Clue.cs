using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Assets.Scripts {
    public struct Clue {
        public static NPC LatestVictim { get; set; }

        public string Statement { get { return ConstructClue(); } }
        public string Template { get; set; }
        public List<NPC> Targets { get; set; }
        public ClueIdentifier Identifier { get; set; }
        public NPCPart.NPCPartType NPCPartType { get; set; }
        public string NPCDescription { get; set; }
        public string Location { get; set; }

        #region Constructors
        public Clue(string template, NPC target, ClueIdentifier identifier, NPCPart.NPCPartType targetPart)
            : this(template, new List<NPC> { target }, identifier, targetPart) { }

        public Clue(string template, List<NPC> targets, ClueIdentifier identifier, NPCPart.NPCPartType targetPart) {
            Template = template;
            Targets = new List<NPC>();
            Targets.AddRange(targets);
            Identifier = identifier;
            NPCPartType = targetPart;
            switch (NPCPartType) {
                case NPCPart.NPCPartType.None:  NPCDescription = string.Empty; break;
                case NPCPart.NPCPartType.Hat:   NPCDescription = Targets[0].Hat.Description; break;
                case NPCPart.NPCPartType.Shirt: NPCDescription = Targets[0].Torso.Description; break;
                case NPCPart.NPCPartType.Pants: NPCDescription = Targets[0].Legs.Description; break;
                default: throw new Exception("NPCPart unidentifiable");
            }
            Location = "Nowhere";
            //if (targets.Count == 0) { UnityEngine.Debug.LogWarning("Clue of type: '" + Identifier.ToString() + "' constructed with no target" /*+ ConstructClue()*/); }
        }
        #endregion

        /// <summary>
        /// Constructs a statement using all custom parameters.
        /// </summary>
        /// <returns>Returns the fully generated statement.</returns>
        public string ConstructClue() {
            StringBuilder clueBuilder = new StringBuilder(Template);

            // Inserting correct pronoun
            var matches = cluePattern.Matches(Template);
            string usedPronoun = string.Empty;
            foreach (Match m in matches) {
                // Replacing abstract tokens with correct values
                switch (m.Groups[1].Value) {
                    case "name":     // Falls into namelist case.
                    case "namelist": clueBuilder.Replace(m.Value, SerializeTargets(Targets)); break;
                    case "location": clueBuilder.Replace(m.Value, Location); break;
                    case "item":     // Falls into part case.
                    case "part":     clueBuilder.Replace(m.Value, NPCDescription); break;
                    case "victim":   clueBuilder.Replace(m.Value, LatestVictim.Name); break;
                    case "gender":   break;
                    default: throw new Exception(string.Format("Found unexpected token {0} while constructing statement.", m.Value));
                }

                // If the match group 2 is a success, the token must be gender.
                // Finds the correct pronoun and replaces the abstract token with the correct value.
                if (m.Groups[2].Success) {
                    int pronounIdx = int.Parse(m.Groups[2].Value);
                    usedPronoun = Targets[0].IsMale ? malePronouns[pronounIdx - 1] : femalePronouns[pronounIdx - 1];
                    usedPronoun = useCockney ? CocknifyPronoun(usedPronoun) : usedPronoun;
                    clueBuilder.Replace(m.Value, usedPronoun);
                }
            }

            var capitalCapture = new Regex(@"\.\s(\w)");
            var capitalMatches = capitalCapture.Matches(clueBuilder.ToString());
            foreach (Match m in capitalMatches) {
                // m.Groups[1].Value[0] is the only character in the string.
                clueBuilder[m.Groups[1].Index] = m.Groups[1].Value.ToUpper()[0];
            }
        
            return clueBuilder.ToString().Trim();
        }

        #region Static fields
        private static bool useCockney = true;
        private static Regex cluePattern = new Regex(@"\[(\w+)(?:\[(\d)\])?\]");

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

        private static string SerializeTargets(List<NPC> targets) {
            if (targets.Count == 0) {
                return "nobody";
            } else if (targets.Count > 1) {
                string returnValue = targets.Take(targets.Count - 1).Select(npc => npc.Name).Aggregate((ag, s) => ag + ", " + s);
                returnValue += useCockney ? " and " + targets.Last().Name : " an' " + targets.Last().Name;
                return returnValue;
            } else {
                return targets.First().Name;
            }
        }
        #endregion
    }
}
