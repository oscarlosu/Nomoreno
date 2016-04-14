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

        private Random r = new Random(DateTime.Now.Millisecond);
        private int minglers = 0;
        private object lockObj = new object();

        public MinglingDirector() {
            minglers = r.Next(3) + 2;
        }

        public List<NPC> RequestMinglingTargets(NPC initiator) {
            lock (lockObj) {
                Clue minglingClue = initiator.Conversation.FirstStatementClue;
                List<Clue> minglingClues = NPC.NPCList.Where(npc => npc != initiator).Select(npc => npc.Conversation.FirstStatementClue).ToList();
                List<NPC> interestNPCs = new List<NPC>();

                if (minglingClue.Identifier == ClueIdentifier.Descriptive) {
                    var interestClues = minglingClues.Where(clue => clue.NPCPartType == minglingClue.NPCPartType);
                    interestNPCs = NPC.NPCList.Where(npc => interestClues.Any(clue => clue.Equals(npc.Conversation.FirstStatementClue))).ToList();

                    //if (interestClues.Count() > 0) {
                    //    interestNPC = interestNPCs.ToList()[new Random(DateTime.Now.Millisecond).Next(interestNPCs.Count())];
                    //    //interestNPC = NPC.NPCList.FirstOrDefault(npc => npc.Conversation.FirstStatementClue.Equals(interestClues.FirstOrDefault()));
                    //}
                } else {
                    var interestClues = minglingClues.Where(clue => clue.Target == minglingClue.Target);
                    interestNPCs = NPC.NPCList.Where(npc => interestClues.Any(clue => clue.Equals(npc.Conversation.FirstStatementClue))).ToList();
                    
                    //if (interestNPCs.Count() > 0) {
                    //    interestNPC = interestNPCs.ToList()[new Random(DateTime.Now.Millisecond).Next(interestNPCs.Count())];
                    //    //interestNPC = NPC.NPCList.FirstOrDefault(npc => npc.Conversation.FirstStatementClue.Equals(interestClues.FirstOrDefault()));
                    //}
                }
                
                return interestNPCs;
            }
        }

        public bool IsMingler() {
            if (minglers > 0) {
                return true;
            } else
                return false;
        }

        public void ResetMinglers() {
            minglers = r.Next(3) + 2;
        }

        public void DecrementMinglers() {
            minglers--;
        }
    }
}
