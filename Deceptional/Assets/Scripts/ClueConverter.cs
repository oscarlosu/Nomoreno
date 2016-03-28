using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Assets.Scripts {
    public static class ClueConverter {

        #region Fields & properties
        private static Random r = PlayerController.UseFixedSeed ? new Random(PlayerController.GeneratorSeed) : new Random(DateTime.Now.Millisecond);

        private static bool useCockney;

        /// <summary>
        /// Clue templates are used to create clues dynamically, based on other information.
        /// The clues are either Accusatory (i.e. directed at a liar), Informative (i.e. directed at an honest person), or Descriptive (i.e. directed at a potential killer).
        /// Clue templates use the following abstract terms to differ between info:
            // X = color
            // Y = piece of clothing
            // Z = npc names
            // Q[i] = gender (i refers to particular pronoun instance)
        /// </summary>
        #region Clue templates
        private static List<string> cockneyClueTemplates = new List<string>() {
            "[A] Blimey! [Z] said [Q[2]] was puttin' up [Q[3]] laundry an' saw da murderer, but I fnk [Q[2]] just wan' attenshun. Nuff said, yeah?",
            "[A] Blimey! Why are yew askin' me? I did not do anything. But I am not so sure abaaaht [Z], [Q[2]] just seem so shady. Nuff said, yeah?",
            "[A] Awright geeezzaa! Don't talk ter me, talk ter [Z] [Q[2]] is braggin' abaaaht almost catchin' da murderer. Sorted mate.",
            "[A] Now I do not put much value in gossip, but I 'eard [Z] cannot 'elp, [Q[2]] always lies fer da fun ov it! OK?",
            "[A] I 'eard abaaaht da murder from [Z], [Q[2]] told me [Q[2]] saw da murderer an' what da murderer looked [Q[5]] straight in da eye... But I'm not sure abaaaht what part. OK?",
            "[A] I 'eard what [Z] actually searched da victim fer valuables befawer da police arrived, [Q[2]] is such a immoral sneak!",
            "[A] Blimey! I saw nothing, yew can ask [Z] cause I was warmin' [Q[2]] bed. Homeless [Z] might 'ave seen somethin' if [Q[2]] was sober enuff ter see straight. Nuff said, yeah?",
            "[A] [Q[4]] are liars... All ov 'em, they cannot be trusted. Just take [Z] fer instance, [Q[2]] just like ter be da cen'er ov attenshun., innit.",
            "[A] So little [Q[6]]-know-it-all says what [Q[2]] is certain what [Q[2]] saw da \"real\" murderer, I doubt it, [Z] just fnk [Q[2]] is so clever... [Q[2]] is not! Sorted mate.",
            "[I] Gawdon Bennet! I know that [Z] ran in ter da murderer when [Q[2]] was 'eadin' aaaht fer work, yew might wan' ter talk wiv [Q[2]]. OK?",
            "[I] When I 'eard abaaaht da murder, I immediately thought what [Z] would 'ad seen i' since [Q[2]] usually walks [Q[3]] dog around what time.",
            "[I] [Z] is sayin' what [Q[2]] is sure what [Q[2]] saw da murderer befawer da murder.",
            "[I] I am pret'y sure what [Z] would 'ave a good view ov da murder scene from [Q[3]] window on da second floor, ask [Q[5]].",
            "[I] Me an' [Z] was at da pub when da murder 'appen, so we did not see anything.",
            "[I] [Z] an' I was already at work when da murder 'appened. Know what I mean?",
            "[I] Awright geeezzaa! I 'eard from [Z] what [Q[2]] saw somethin' when [Q[2]] wen' fer [Q[3]] mornin' piss. Sorted mate.",
            "[I] Lawd above! I saw Ann comin' away from around where da murder 'appened, [Q[2]] was white as a ghost. [Q[2]] must 'ave seen something., innit.",
            "[I] I was walkin' me dog, an' was just comin' back from da park. There was not a lot ov people, but I fnk I saw [Z] comin' 'ome from da pub.",
            "[D] When I opened me door dis mornin' I was shoved back in by a big [Q[1]], I saw [Q[2]] 'ad a [X] [Y].",
            "[D] When I got up dis mornin' someone 'ad crashed in ter me flaaahrs, there was a [X] piece ov clof stuck ter 'em., innit.",
            "[D] I was emptyin' da night pot, an' I saw two [Q[4]] strugglin' in da street, da [Q[1]] standin' over da uvver 'ad a [X] [Y]. But I did not buvver ter get involved.",
            "[D] Awright geeezzaa! I saw da murder 'appen! I was so scared, I 'id benearf da windowsill, I was shaking! I peaked up an' saw a [X] [Y] turning, an' quickly ducked down again! Sorted mate.",
            "[D] I was comin' aaaht ov da pub ter take a piss, when I noticed dis big [Q[1]] passin' by lookin' shady. I greeted [Q[5]], but da dow' said nothing. [Q[2]] 'ad a [X] [Y] on. Know what I mean?",
            "[D] I was groomin' da 'orse when somethin' scared 'em, I looked around an' saw a [Q[1]] standin' over [Q[1]]. I turned ter calm da 'orse, when I looked again there was only one [Q[1]] lyin' facedown in da street. The uvver [Q[1]] 'ad [X] [Y].",
            "[D] I was openin' me shop, I 'eard some commoshun in da street, when I wen' ter look I just saw a glimpse ov da murderer's [Y] which was [X]."
        };

        private static List<string> englishClueTemplates = new List<string>() {
            "[A] [Z] said [Q[2]] was putting up [Q[3]] laundry and saw the murderer, but I think [Q[2]] just want attention.",
            "[A] Why are you asking me? I did not do anything. But I am not so sure about [Z], [Q[2]] just seem so shady.",
            "[A] Now I do not put much value in gossip, but I heard [Z] cannot help, [Q[2]] always lies for the fun of it!",
            "[A] I heard that [Z] actually searched the victim for valuables before the police arrived, [Q[2]] is such a immoral sneak! ",
            "[A] I saw nothing! You can ask [Z] cause I was warming [Q[2]] bed. Homeless [Z] might have seen something if [Q[2]] was sober enough to see straight.",
            "[A] [Q[4]] are liars... all of them, they cannot be trusted. Just take [Z] for instance, [Q[2]] just like to be the center of attention.",
            "[A] I heard about the murder from [Z], [Q[2]] told me [Q[2]] saw the murderer and that the murderer looked [Q[5]] straight in the eye... But I'm not sure about that part.",
            "[A] Don't talk to me, talk to [Z] [Q[2]] is bragging about almost catching the murderer.",
            "[A] So little [Q[6]]-know-it-all says that [Q[2]] is certain that [Q[2]] saw the “real” murderer, I doubt it, [Z] just think [Q[2]] is so clever... [Q[2]] is not!",
            "[I] I know that [Z] ran into the murderer when  [Q[2]] was heading out for work, you might want to talk with [Q[2]].",
            "[I] I saw [Z] coming away from around where the murder happened, [Q[2]] was white as a ghost. [Q[2]] must have seen something.",
            "[I] When I heard about the murder, I immediately thought that [Z] would had seen it, since [Q[2]] usually walks [Q[3]] dog around that time.",
            "[I] I was walking my dog, and was just coming back from the park. There was not a lot of people, but I think I saw [Z]coming home from the pub.",
            "[I] [Z] is saying that [Q[2]] is sure that [Q[2]] saw the murderer before the murder.",
            "[I] I am pretty sure that [Z] would have a good view of the murder scene from [Q[3]] window on the second floor, ask [Q[5]].",
            "[I] Me and [Z] was at the pub when the murder happen, so we did not see anything.",
            "[I] [Z] and I were already at work when the murder happened.",
            "[I] I heard from [Z] that [Q[2]] saw something when [Q[2]] went for [Q[3]] morning piss.",
            "[D] When I open my door this morning I was shoved back in by a big [Q[1]], I saw [Q[2]] had a [X] [Y].",
            "[D] When I got up this morning someone had crashed into my flowers, there was a [X] piece of cloth stuck to them.",
            "[D] I was emptying the night pot, and I saw two [Q[4]] struggling in the street, the [Q[1]] standing over the other had a [X] [Y]. But I did not bother to get involved.",
            "[D] I saw the murder happen! I was so scared, I hid beneath  the windowsill, I was shaking! I peaked up and saw a [X] [Y] turning, and quickly ducked down again!",
            "[D] I was coming out of the pub to take a piss, when I noticed this big [Q[1]] passing by looking shady. I greeted [Q[5]], but the dolt said nothing. [Q[2]] had a [X] [Y] on.",
            "[D] I was grooming the horse when something scared them, I looked around and saw a [Q[1]] standing over [Q[1]]. I turned to calm the horse, when I looked again there was only one [Q[1]] lying facedown in the street. The other [Q[1]] had [X] [Y].",
            "[D] I saw a [Q[1]] fall backwards from the murderer, and then [Q[2]] started bleeding... a lot... I fainted, cannot stand the sight of blood. I saw so much red, but I think [Q[3]] coat was really [X].",
            "[D] I was opening my shop, I heard some commotion in the street, when I went to look I just saw a glimpse of the murderer's [Y] which was [X]"
        };
        #endregion

        #region Pronouns
        private static List<string> malePronouns = new List<string>() { "man", "he", "his", "men", "him", "Mister" };
        private static List<string> femalePronouns = new List<string>() { "woman", "she", "hers", "women", "her", "Miss" };
        #endregion

        private static Regex cluePattern = new Regex(@"(\[X\])|(\[Y\])|(\[Z\])|(\[Q\[\d\]\])");
        #endregion

        #region Private methods
        private static string GetClueTemplate(ClueIdentifier identifier) {
            Regex cluePattern = new Regex(@"(\[A\])|(\[I\])|(\[D\])");
            List<string> templateList = useCockney ? cockneyClueTemplates : englishClueTemplates;

            List<string> relevantClues;
            switch (identifier) {
                case ClueIdentifier.Accusatory:     relevantClues = templateList.Where(s => cluePattern.Match(s).Groups[1].Success).ToList(); break;
                case ClueIdentifier.Informational:  relevantClues = templateList.Where(s => cluePattern.Match(s).Groups[2].Success).ToList(); break;
                case ClueIdentifier.Descriptive:    relevantClues = templateList.Where(s => cluePattern.Match(s).Groups[3].Success).ToList(); break;
                default: relevantClues = templateList; break;
            }
            int index = r.Next(relevantClues.Count);
            return relevantClues[index];
        }
        
        private static string CocknifyPronoun(string pronoun) {
            if (pronoun == "he" || pronoun == "his" || pronoun == "him" || pronoun == "hers" || pronoun == "her")
                return pronoun.Replace("h", "'");
            else
                return pronoun;
        }
        #endregion

        public static string ConstructClue(bool isAccusatory, string npcName, bool isMale) {
            return isAccusatory ?
                ConstructClue(ClueIdentifier.Accusatory, "<COLOR_ERROR>", "<CLOTHING_ERROR>", npcName, isMale) :
                ConstructClue(ClueIdentifier.Informational, "<COLOR_ERROR>", "<CLOTHING_ERROR>", npcName, isMale);
        }

        public static string ConstructClue(string color, string npcName, bool isMale) {
            return ConstructClue(ClueIdentifier.Informational, color, "<CLOTHING_ERROR>", npcName, isMale);
        }

        public static string ConstructClue(bool isDescriptive, string color, string clothing, string npcName, bool isMale) {
            return isDescriptive ?
                ConstructClue(ClueIdentifier.Descriptive, color, clothing, npcName, isMale) :
                ConstructClue(ClueIdentifier.Informational, color, clothing, npcName, isMale);
        }

        public static string ConstructClue(ClueIdentifier identifier, string color, string clothing, string npcName, bool isMale) {
            return ConstructClue(GetClueTemplate(identifier), color, clothing, npcName, isMale);
        }

        public static string ConstructClue(ClueIdentifier identifier, string npcName, bool isMale) {
            if (identifier.Equals(ClueIdentifier.Descriptive)) throw new ArgumentException("Identifier cannot be Descriptive if no color or clothing is provided.");
            return ConstructClue(GetClueTemplate(identifier).Equals(ClueIdentifier.Accusatory), npcName, isMale);
        }

        /// <summary>
        /// Constructs a statement using all custom parameters.
        /// </summary>
        /// <param name="template">The neccessary clue template.</param>
        /// <param name="color">The color of the target clothing.</param>
        /// <param name="clothing">Clothing type of target clothing.</param>
        /// <param name="npcName">Name of target NPC.</param>
        /// <param name="isMale">Gender of target NPC (true = male, false = female).</param>
        /// <returns></returns>
        public static string ConstructClue(string template, string color, string clothing, string npcName, bool isMale) {
            var matches = cluePattern.Matches(template);

            StringBuilder clueBuilder = new StringBuilder(template);
            clueBuilder.Remove(0, 3);

            // Inserting correct pronoun
            string usedPronoun = "";
            foreach (Match m in matches) {
                if (m.Groups[4].Success) {
                    int pronounIdx = int.Parse(Regex.Match(m.Groups[4].Value, @"\d+").Value);
                    usedPronoun = isMale ? malePronouns[pronounIdx - 1] : femalePronouns[pronounIdx - 1];
                    usedPronoun = useCockney ? CocknifyPronoun(usedPronoun) : usedPronoun;
                    clueBuilder.Replace(m.Groups[4].Value, usedPronoun);
                }
            }

            // Replacing abstract tokens with format tokens
            clueBuilder.Replace(@"[X]", color);
            clueBuilder.Replace(@"[Y]", clothing);
            clueBuilder.Replace(@"[Z]", npcName);

            return clueBuilder.ToString().Trim();
        }
    }
}
