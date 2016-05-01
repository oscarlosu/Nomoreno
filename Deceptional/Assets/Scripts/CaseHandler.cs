using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public class CaseHandler {
        private readonly Dictionary<string, List<NPC>> npcLocations = new Dictionary<string, List<NPC>>();
        public Dictionary<string, List<NPC>> NPCLocations { get { return npcLocations; } }

        public string MurderLocation { get; set; }

        public CaseHandler(List<NPC> NPCs, string murderLocation) {
            List<string> locations = IO.FileLoader.GetLocations();
            foreach (string loc in locations) { NPCLocations.Add(loc, new List<NPC>()); }

            MurderLocation = murderLocation;

            foreach (NPC npc in NPCs) {
                var npcLoc = locations[PlayerController.Instance.Rng.Next(locations.Count)];
                NPCLocations[npcLoc].Add(npc);
            }
        }
    }
}
