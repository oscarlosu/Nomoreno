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

            if (initiatorClue.Identifier == ClueIdentifier.Descriptive) {
                interestNPCs = interestNPCs.Where(other => other.Conversation.ActualClue.NPCPartType == initiatorClue.NPCPartType).ToList();
            } else {
                interestNPCs = interestNPCs.Where(other => (other.Conversation.ActualClue.Identifier != ClueIdentifier.Descriptive && 
                                                           (other.Conversation.ActualClue.Targets.Contains(initiator) || other.Conversation.ActualClue.Targets.Any(npc => initiatorClue.Targets.Any(oNPC => npc == oNPC)))) || 
                                                            initiatorClue.Targets.Contains(other)).ToList();
            }
                
            return interestNPCs;
        }
    }
}
