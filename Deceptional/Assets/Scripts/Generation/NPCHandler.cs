using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts {
    public static class NPCHandler {
        #region Static lists        
        private static List<string> maleFirsts = IO.FileLoader.GetNames(true);
        private static List<string> femaleFirsts = IO.FileLoader.GetNames(false);

        private static List<string> maleSurs = new List<string>() {
            "Dudley", "Rowley", "Shelton", "Wylde", "Kerley", "Odlam", "Bruslin", "Coogan", "Coffey",
            "Montgomery", "Despard", "Gantley", "Shields", "Paxton", "McHugh", "Footter", "Pilkington",
            "Renehan", "Lyon", "Coughlin", "Goodfellow", "Cookman", "Wickfield"
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
            NPC killer = killerGO.GetComponent<NPC>();

            // Generate likeness NPCs
            var generationStep = 0;
            var doppelgangerCount = 2;
            while (generationStep < 2) {
                while (doppelgangerCount-- > 0) {
                    switch(generationStep) {
                        case 0: GenerateWitnessLikeness(new List<NPCPart>() { killer.Hat }); break;
                        case 1: GenerateWitnessLikeness(new List<NPCPart>() { killer.Torso }); break;
                        default: throw new Exception("GenerationStep overflow; no such generationStep");
                    }
                }
                doppelgangerCount = 2;
                generationStep++;
            }

            killer.IsKiller = true;
        }

        public static GameObject GenerateRandomWitness() { return GenerateWitnessLikeness(new List<NPCPart>()); }
        public static GameObject GenerateWitnessLikeness(List<NPCPart> likeness) {
            // Instantiate new npc from prefab, get NPC script
            var npcGO = GameObject.Instantiate(PlayerController.Instance.DefaultNPC);
            NPC npc = npcGO.GetComponent<NPC>();
            // Make NPCS game object as parent
            npcGO.transform.SetParent(PlayerController.NPCParent.transform);

            // Set gender and name
            bool npcGender = Convert.ToBoolean(PlayerController.Instance.Rng.Next(0, 2));
            npc.IsMale = npcGender;
            npc.Name = GetRandomName(npcGender, useFullNames);

            Dictionary<NPCPart.NPCPartType, NPCPart> isCustomPart = new Dictionary<NPCPart.NPCPartType, NPCPart>() {
                { NPCPart.NPCPartType.Hat, null },
                { NPCPart.NPCPartType.Shirt, null },
                { NPCPart.NPCPartType.Pants, null },
                { NPCPart.NPCPartType.Item, null }
            };

            foreach (NPCPart part in likeness) isCustomPart[part.Type] = part;

            var newHat = isCustomPart[NPCPart.NPCPartType.Hat] != null ? isCustomPart[NPCPart.NPCPartType.Hat] : GetRandomPart(NPCPart.NPCPartType.Hat);
            var newShirt = isCustomPart[NPCPart.NPCPartType.Shirt] != null ? isCustomPart[NPCPart.NPCPartType.Shirt] : GetRandomPart(NPCPart.NPCPartType.Shirt);
            var newPants = isCustomPart[NPCPart.NPCPartType.Pants] != null ? isCustomPart[NPCPart.NPCPartType.Pants] : GetRandomPart(NPCPart.NPCPartType.Pants);
            var newItem = isCustomPart[NPCPart.NPCPartType.Item] != null ? isCustomPart[NPCPart.NPCPartType.Item] : GetRandomPart(NPCPart.NPCPartType.Item);

            npc.Assemble(newHat, newShirt, newPants);

            return npcGO;
        }
        public static NPCPart GetRandomPart(NPCPart.NPCPartType identifier) {
            var parts = IO.FileLoader.GetParts(identifier);
            return new NPCPart(identifier, PlayerController.Instance.Rng.Next(parts.Count));
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
