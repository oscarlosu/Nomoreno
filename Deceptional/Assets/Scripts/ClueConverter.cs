using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Assets.Scripts {
    public static class ClueConverter {

        #region Fields & properties
        private static Random r = new Random();

        private static bool useCockney;

        #region Clue templates
        private static List<string> cockneyClueTemplates = new List<string>() {
            "Gawdon Bennet! I know that [Z] ran in ter da murderer when [Q[2]] was 'eadin' aaaht fer work, yew might wan' ter talk wiv [Q[2]]. OK?",
            "When I opened me door dis mornin' I was shoved back in by a big [Q[1]], I saw [Q[2]] 'ad a [X] [Y].",
            "Blimey! [Z] said [Q[2]] was puttin' up [Q[3]] laundry an' saw da murderer, but I fnk [Q[2]] just wan' attenshun. Nuff said, yeah?",
            "When I got up dis mornin' someone 'ad crashed in ter me flaaahrs, there was a [X] piece ov clof stuck ter 'em., innit.",
            "Blimey! Why are yew askin' me? I did not do anything. But I am not so sure abaaaht [Z], [Q[2]] just seem so shady. Nuff said, yeah?",
            "I was emptyin' da night pot, an' I saw two [Q[4]] strugglin' in da street, da [Q[1]] standin' over da uvver 'ad a [X] [Y]. But I did not buvver ter get involved.",
            "Awright geeezzaa! I saw da murder 'appen! I was so scared, I 'id benearf da windowsill, I was shaking! I peaked up an' saw a [X] [Y] turning, an' quickly ducked down again! Sorted mate.",
            "Lawd above! I saw Ann comin' away from around where da murder 'appened, [Q[2]] was white as a ghost. [Q[2]] must 'ave seen something., innit.",
            "Now I do not put much value in gossip, but I 'eard [Z] cannot 'elp, [Q[2]] always lies fer da fun ov it! OK?",
            "When I 'eard abaaaht da murder, I immediately thought what [Z] would 'ad seen i' since [Q[2]] usually walks [Q[3]] dog around what time.",
            "I 'eard what [Z] actually searched da victim fer valuables befawer da police arrived, [Q[2]] is such a immoral sneak!",
            "Blimey! I saw nothing, yew can ask [Z] cause I was warmin' [Q[2]] bed. Homeless [Z] might 'ave seen somethin' if [Q[2]] was sober enuff ter see straight. Nuff said, yeah?",
            "I was comin' aaaht ov da pub ter take a piss, when I noticed dis big [Q[1]] passin' by lookin' shady. I greeted [Q[5]], but da dow' said nothing. [Q[2]] 'ad a [X] [Y] on. Know what I mean?",
            "[Q[4]] are liars... All ov 'em, they cannot be trusted. Just take [Z] fer instance, [Q[2]] just like ter be da cen'er ov attenshun., innit.",
            "I was groomin' da 'orse when somethin' scared 'em, I looked around an' saw a [Q[1]] standin' over [Q[1]]. I turned ter calm da 'orse, when I looked again there was only one [Q[1]] lyin' facedown in da street. The uvver [Q[1]] 'ad [X] [Y].",
            "I was walkin' me dog, an' was just comin' back from da park. There was not a lot ov people, but I fnk I saw [Z] comin' 'ome from da pub.",
            "I 'eard abaaaht da murder from [Z], [Q[2]] told me [Q[2]] saw da murderer an' what da murderer looked [Q[5]] straight in da eye... But I'm not sure abaaaht what part. OK?",
            "Awright geeezzaa! Don't talk ter me, talk ter [Z] [Q[2]] is braggin' abaaaht almost catchin' da murderer. Sorted mate.",
            "Lawd above! I almost caught da murder when I saw [Q[5]] killin' da [Q[1]], but [Q[2]] was an' all fast fer me.",
            "I saw da [Q[1]] fall away from da murderer, an' then [Q[2]] started bleeding... A lot... I fain'ed, cannot stand da sight ov blood. I saw so much red, but I fnk [Q[3]] coat was really [X]. Sorted mate."
        };

        private static List<string> englishClueTemplates = new List<string>() {
            "I know that [Z] ran into the murderer when  [Q[2]] was heading out for work, you might want to talk with [Q[2]].",
            "When I open my door this morning I was shoved back in by a big [Q[1]], I saw [Q[2]] had a [X] [Y].",
            "[Z] said [Q[2]] was putting up [Q[3]] laundry and saw the murderer, but I think [Q[2]] just want attention.",
            "When I got up this morning someone had crashed into my flowers, there was a [X] piece of cloth stuck to them.",
            "Why are you asking me? I did not do anything. But I am not so sure about [Z], [Q[2]] just seem so shady.",
            "I was emptying the night pot, and I saw two [Q[4]] struggling in the street, the [Q[1]] standing over the other had a [X] [Y]. But I did not bother to get involved.",
            "I saw the murder happen! I was so scared, I hid beneath  the windowsill, I was shaking! I peaked up and saw a [X] [Y] turning, and quickly ducked down again!",
            "I saw [Z] coming away from around where the murder happened, [Q[2]] was white as a ghost. [Q[2]] must have seen something.",
            "Now I do not put much value in gossip, but I heard [Z] cannot help, [Q[2]] always lies for the fun of it!",
            "When I heard about the murder, I immediately thought that [Z] would had seen it, since [Q[2]] usually walks [Q[3]] dog around that time.",
            "I heard that [Z] actually searched the victim for valuables before the police arrived, [Q[2]] is such a immoral sneak!",
            "I saw nothing! You can ask [Z] cause I was warming [Q[2]] bed. Homeless [Z] might have seen something if [Q[2]] was sober enough to see straight.",
            "I was coming out of the pub to take a piss, when I noticed this big [Q[1]] passing by looking shady. I greeted [Q[5]], but the dolt said nothing. [Q[2]] had a [X] [Y] on.",
            "[Q[4]] are liars... all of them, they cannot be trusted. Just take [Z] for instance, [Q[2]] just like to be the center of attention.",
            "I was grooming the horse when something scared them, I looked around and saw a [Q[1]] standing over [Q[1]]. I turned to calm the horse, when I looked again there was only one [Q[1]] lying facedown in the street. The other [Q[1]] had [X] [Y].",
            "I was walking my dog, and was just coming back from the park. There was not a lot of people, but I think I saw [Z]coming home from the pub.",
            "I heard about the murder from [Z], [Q[2]] told me [Q[2]] saw the murderer and that the murderer looked [Q[5]] straight in the eye... But I'm not sure about that part.",
            "Don't talk to me, talk to [Z] [Q[2]] is bragging about almost catching the murderer.",
            "I almost caught the murder when I saw [Q[5]] killing the [Q[1]], but [Q[2]] was too fast for me.",
            "I saw the [Q[1]] fall away from the murderer, and then [Q[2]] started bleeding... a lot... I fainted, cannot stand the sight of blood. I saw so much red, but I think [Q[3]] coat was really [X]."
        };
        #endregion

        #region Pronouns
        private static List<string> malePronouns = new List<string>() { "man", "he", "his", "men", "him" };
        private static List<string> femalePronouns = new List<string>() { "woman", "she", "hers", "women", "her" };
        #endregion

        private static Regex cluePattern = new Regex(@"(\[X\])|(\[Y\])|(\[Z\])|(\[Q\[\d\]\])");
        #endregion

        #region Private methods
        private static string GetClueTemplate(bool isDescriptive) {
            Regex descriptivePattern = new Regex(@"(\[Y\])");
            List<string> usedList = useCockney ? cockneyClueTemplates : englishClueTemplates;
            List<string> descriptiveClues = usedList.Where(s => descriptivePattern.IsMatch(s)).ToList();
            List<string> nonDescriptiveClues = usedList.Except(descriptiveClues).ToList();

            int clueIdx = isDescriptive ? r.Next(descriptiveClues.Count) : r.Next(nonDescriptiveClues.Count);
            string clue = isDescriptive ? descriptiveClues[clueIdx] : nonDescriptiveClues[clueIdx];

            return clue;
        }
        
        private static string CocknifyPronoun(string pronoun) {
            if (pronoun == "he" || pronoun == "his" || pronoun == "him" || pronoun == "hers" || pronoun == "her")
                return pronoun.Replace("h", "'");
            else
                return pronoun;
        }
        #endregion

        public static string ConstructClue(string color, string npcName, bool isMale) {
            return ConstructClue(false, color, "<CLOTHING_ERROR>", npcName, isMale);
        }

        public static string ConstructClue(bool isDescriptive, string color, string clothing, string npcName, bool isMale) {
            return ConstructClue(GetClueTemplate(isDescriptive), color, clothing, npcName, isMale);
        }

        // X = color
        // Y = piece of clothing
        // Z = npc names
        // Q = gender
        public static string ConstructClue(string template, string color, string clothing, string npcName, bool isMale) {
            var matches = cluePattern.Matches(template);

            StringBuilder clueBuilder = new StringBuilder(template);

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

            return clueBuilder.ToString();
        }
    }
}
