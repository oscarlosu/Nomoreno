using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public class MinglingDirector {
        private static MinglingDirector _instance;
        public static MinglingDirector Instance {
            get {
                if (_instance == null) _instance = new MinglingDirector();
                return _instance;
            }
        }
        
        public List<NPC> RequestMinglingTargets(NPC initiator) {
            Clue initiatorClue = initiator.Conversation.ActualClue;
            // Get list of other NPCs that are available to mingle and list of their clues
            List<NPC> interestNPCs = NPC.NPCList.Where(other => other != initiator && other.CanMingle).ToList();
            interestNPCs = interestNPCs.Where(other => initiator.GetMingleResult(other) != null).ToList();                
            return interestNPCs;
        }
    }
}
