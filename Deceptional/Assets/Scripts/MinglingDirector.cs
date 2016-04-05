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

        private object lockObj = new object();

        public MinglingDirector() {
        }

        public NPC RequestMinglingTarget(NPC initiator) {
            lock (lockObj) {
                Clue minglingClue = initiator.Conversation.FirstStatementClue;
                List<Clue> minglingClues = NPC.NPCList.Where(npc => npc != initiator).Select(npc => npc.Conversation.FirstStatementClue).ToList();
                NPC interestNPC = null;

                if (minglingClue.Identifier == ClueIdentifier.Descriptive) {
                    var interestClues = minglingClues.Where(clue => clue.NPCPartType == minglingClue.NPCPartType);

                    if (interestClues.Count() > 0) {
                        interestNPC = NPC.NPCList.FirstOrDefault(npc => npc.Conversation.FirstStatementClue.Equals(interestClues.FirstOrDefault()));
                    }
                } else {
                    var interestClues = minglingClues.Where(clue => clue.Target == minglingClue.Target);

                    if (interestClues.Count() > 0) {
                        interestNPC = NPC.NPCList.FirstOrDefault(npc => npc.Conversation.FirstStatementClue.Equals(interestClues.FirstOrDefault()));
                    }
                }

                if (interestNPC != null) {
                    return interestNPC;
                } else {
                    int idx = UnityEngine.Random.Range(0, NPC.NPCList.Count - 1);
                    interestNPC = NPC.NPCList.Where(npc => npc != initiator).ToList()[idx];
                    return interestNPC;
                }
            }
        }
    }
}
