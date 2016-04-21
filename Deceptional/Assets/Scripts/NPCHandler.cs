using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts {
    public static class NPCHandler {
        #region Static lists        
        private static List<string> maleFirsts = new List<string>() {
            "Rudolph", "Davis", "Frank", "August", "Jeremy", "John", "Jacob", "Claude",
            "Joseph", "Michael", "Elias", "Oliver", "Jimmy", "Amon", "Bart", "Henry",
            "Giles", "Sam", "Mike", "Tobias"
        };

        private static List<string> maleSurs = new List<string>() {
            "Dudley", "Rowley", "Shelton", "Wylde", "Kerley", "Odlam", "Bruslin", "Coogan", "Coffey",
            "Montgomery", "Despard", "Gantley", "Shields", "Paxton", "McHugh", "Footter", "Pilkington",
            "Renehan", "Lyon", "Coughlin", "Goodfellow", "Cookman", "Wickfield"
        };

        private static List<string> femaleFirsts = new List<string>() {
            "Agatha", "Ann", "Maria", "Sue", "Mary", "Lilly", "Molly", "Anna", "Abby", "Becky", "Jennie",
            "Ginny", "Joanna", "Maxine", "Lorraine", "Cynthia", "Lenora", "Allie", "Natalie", "Josephine",
            "Theodosia", "Dahlia", "Gertie"
        };

        private static List<string> femaleSurs = new List<string>() {
            "Kain", "Cotton", "Wickham", "Costello", "Oxenforde", "Nayler", "Morrell", "Gouran", "West",
            "Pinker", "Mullane", "Edgerton", "Teelong", "Rason", "Houghlin", "Tottenham", "Birmingham",
            "Greypartridge", "Hewitson", "Colligan", "Heferord", "Reynolds", "McGlinn"
        };
        #endregion

        public static void GenerateMultipleWitnesses(int count) {
            PlayerController.NPCParent.SetActive(false);
            while (--count > 0) GenerateRandomWitness();
            GenerateKiller();
        }

        public static void GenerateKiller() {
            GameObject killerGO = GenerateRandomWitness();
            killerGO.GetComponent<NPC>().IsKiller = true;
        }

        public static GameObject GenerateRandomWitness() {
            // Instantiate new npc from prefab, get NPC script
            var npcGO = GameObject.Instantiate(PlayerController.Instance.DefaultNPC);
            NPC npc = npcGO.GetComponent<NPC>();
            // Make NPCS game object as parent
            npcGO.transform.SetParent(PlayerController.NPCParent.transform);
                      

            // Set gender and name
            bool npcGender = Convert.ToBoolean(PlayerController.Instance.Rng.Next(0, 2));
            npc.IsMale = npcGender;
            npc.Name = GetRandomName(npcGender, useFullNames);
            
            NPCPart.NPCPartDescription randomDesc;
            var maxValue = Enum.GetValues(typeof(NPCPart.NPCPartDescription)).Length;
            randomDesc = (NPCPart.NPCPartDescription)PlayerController.Instance.Rng.Next(maxValue);
            var newHead = new NPCPart(NPCPart.NPCPartType.Hat, randomDesc);
            randomDesc = (NPCPart.NPCPartDescription)PlayerController.Instance.Rng.Next(maxValue);
            var newTorso = new NPCPart(NPCPart.NPCPartType.Shirt, randomDesc);
            randomDesc = (NPCPart.NPCPartDescription)PlayerController.Instance.Rng.Next(maxValue);
            var newLegs = new NPCPart(NPCPart.NPCPartType.Pants, randomDesc);

            npc.Assemble(newHead, newTorso, newLegs);

            
            return npcGO;
        }

        private static bool useFullNames = false;
        private static int nextMaleIdx = 0;
        private static int nextFemaleIdx = 0;
        public static string GetRandomName(bool isMale, bool useFullName) {
            var name = string.Empty;
            if (useFullName) {
                name = isMale ?
                    maleFirsts[PlayerController.Instance.Rng.Next(maleFirsts.Count)] + " " + maleSurs[PlayerController.Instance.Rng.Next(maleSurs.Count)] :
                    femaleFirsts[PlayerController.Instance.Rng.Next(femaleFirsts.Count)] + " " + femaleSurs[PlayerController.Instance.Rng.Next(femaleSurs.Count)];
            } else {
                name = isMale ?
                    maleFirsts[nextMaleIdx++ % maleFirsts.Count] :
                    femaleFirsts[nextFemaleIdx++ % femaleFirsts.Count];
            }
            return name;
        }
    }
}
