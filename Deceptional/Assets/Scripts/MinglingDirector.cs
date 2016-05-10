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

        //private int minglers = 0;



        public MinglingDirector() {
            //ResetMinglers();
        }

        public List<NPC> RequestMinglingTargets(NPC initiator) {
                Clue initiatorClue = initiator.Conversation.ActualClue;
                // Get list of other NPCs that are available to mingle and list of their clues
                List<NPC> interestNPCs = NPC.NPCList.Where(other => other != initiator && other.CanMingle).ToList();
                //List<Clue> minglingClues = interestNPCs.Select(npc => npc.Conversation.ActualClue).ToList();

                if (initiatorClue.Identifier == ClueIdentifier.Descriptive) {
                //var interestClues = minglingClues.Where(clue => clue.NPCPartType == minglingClue.NPCPartType);
                //interestNPCs = interestNPCs.Where(npc => interestClues.Any(clue => clue.Equals(npc.Conversation.ActualClue))).ToList();
                interestNPCs = interestNPCs.Where(other => other.Conversation.ActualClue.NPCPartType == initiatorClue.NPCPartType).ToList();
                //if (interestClues.Count() > 0) {
                //    interestNPC = interestNPCs.ToList()[new Random(DateTime.Now.Millisecond).Next(interestNPCs.Count())];
                //    //interestNPC = NPC.NPCList.FirstOrDefault(npc => npc.Conversation.FirstStatementClue.Equals(interestClues.FirstOrDefault()));
                //}
            } else {
                //var interestClues = minglingClues.Where(clue => clue.Target == minglingClue.Target || clue.Target == initiator);
                //interestNPCs = interestNPCs.Where(npc => minglingClue.Target == npc || interestClues.Any(clue => clue.Equals(npc.Conversation.ActualClue))).ToList();

                interestNPCs = interestNPCs.Where(other => (other.Conversation.ActualClue.Identifier != ClueIdentifier.Descriptive && 
                                                           (other.Conversation.ActualClue.Targets.Contains(initiator) || other.Conversation.ActualClue.Targets.Any(npc => initiatorClue.Targets.Any(oNPC => npc == oNPC)))) || 
                                                            initiatorClue.Targets.Contains(other)).ToList();
                //if (interestNPCs.Count() > 0) {
                //    interestNPC = interestNPCs.ToList()[new Random(DateTime.Now.Millisecond).Next(interestNPCs.Count())];
                //    //interestNPC = NPC.NPCList.FirstOrDefault(npc => npc.Conversation.FirstStatementClue.Equals(interestClues.FirstOrDefault()));
                //}
            }
                
            return interestNPCs;
        }

        //public bool IsMingler() {
        //    if (minglers > 0) {
        //        return true;
        //    } else
        //        return false;
        //}

        //public void ResetMinglers() {
        //    minglers = PlayerController.Instance.Rng.Next(PlayerController.Instance.MinMinglers, PlayerController.Instance.MaxMinglers + 1);
        //}

        //public void DecrementMinglers() {
        //    minglers--;
        //}
    }
}
