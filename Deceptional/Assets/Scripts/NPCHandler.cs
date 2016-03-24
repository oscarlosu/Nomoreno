using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts {
    public static class NPCHandler {
        private static System.Random r = new System.Random();

        

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
        
        [Obsolete("MurderWitness method accessible in PlayerController.")]
        public static void MurderRandomWitness() {
            var allWitnesses = NPC.NPCList.Where(npc => !npc.IsKiller);
            var randomWitness = allWitnesses.ToList()[r.Next(allWitnesses.Count())];
            NPC.NPCList.Remove(randomWitness);
            UnityEngine.Object.Destroy(randomWitness.gameObject);
        }

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
            var npcGO = NPC.DefaultNPC;
            NPC npc = npcGO.GetComponent<NPC>();
            // Make NPCS game object as parent
            npcGO.transform.SetParent(PlayerController.NPCParent.transform);
                      

            // Set gender and name
            bool npcGender = Convert.ToBoolean(r.Next(0, 2));
            npc.IsMale = npcGender;
            npc.Name = GetRandomName(npcGender);
            
            NPCPart.NPCPartDescription randomDesc;
            var maxValue = Enum.GetValues(typeof(NPCPart.NPCPartDescription)).Length;
            randomDesc = (NPCPart.NPCPartDescription)r.Next(maxValue);
            var newHead = new NPCPart(NPCPart.NPCPartType.Hat, randomDesc);
            randomDesc = (NPCPart.NPCPartDescription)r.Next(maxValue);
            var newTorso = new NPCPart(NPCPart.NPCPartType.Shirt, randomDesc);
            randomDesc = (NPCPart.NPCPartDescription)r.Next(maxValue);
            var newLegs = new NPCPart(NPCPart.NPCPartType.Pants, randomDesc);

            npcScript.Assemble(newHead, newTorso, newLegs);

            NPC.NPCList.Add(npc);
            return npcGO;
        }

        public static string GetRandomName(bool isMale) {
            var name = isMale ?
                maleFirsts[r.Next(maleFirsts.Count)] + " " + maleSurs[r.Next(maleSurs.Count)] :
                femaleFirsts[r.Next(femaleFirsts.Count)] + " " + femaleSurs[r.Next(femaleSurs.Count)];
            return name;
        }
    }
}
