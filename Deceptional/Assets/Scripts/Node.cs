using System.Collections.Generic;

namespace Assets.Scripts {
    public class Node {
        public List<NPC> TargetNodes { get; set; }
        public NPC NPC { get; set; }
        public Clue NodeClue { get; set; }
        public bool IsVisited { get; set; }
        public bool IsDescriptive { get; set; }
        public bool IsKiller { get; set; }
    }
}
