using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public class CaseHandler {
        private readonly Dictionary<string, List<NPC>> npcLocations = new Dictionary<string, List<NPC>>();
        public Dictionary<string, List<NPC>> NPCLocations { get { return npcLocations; } }

        public string MurderLocation { get; set; }

        public CaseHandler(List<NPC> NPCs) {
            List<string> locations = IO.FileLoader.GetLocations();
            foreach (string loc in locations) { NPCLocations.Add(loc, new List<NPC>()); }

            foreach (NPC npc in NPCs.Where(npc => !npc.IsKiller)) {
                var npcLoc = locations[PlayerController.Instance.Rng.Next(locations.Count)];
                NPCLocations[npcLoc].Add(npc);
            }

            var populatedLocs = NPCLocations.Where(kvp => kvp.Value.Count > 0).ToList();
            NPCLocations.Clear();
            foreach (KeyValuePair<string, List<NPC>> kvp in populatedLocs) NPCLocations.Add(kvp.Key, kvp.Value);

            MurderLocation = NPCLocations.Keys.ElementAt(PlayerController.Instance.Rng.Next(NPCLocations.Count));
            NPCLocations[MurderLocation].Add(NPC.NPCList.FirstOrDefault(npc => npc.IsKiller));
        }

        public CaseHandler(List<NPC> NPCs, string murderLocation) : this(NPCs) {
            if (NPCLocations.Keys.Contains(murderLocation)) MurderLocation = murderLocation;
            else throw new ArgumentException("Invalid murderLocation: No such populated location.");
            NPCLocations[MurderLocation].Add(NPC.NPCList.FirstOrDefault(npc => npc.IsKiller));
        }
    }
}
