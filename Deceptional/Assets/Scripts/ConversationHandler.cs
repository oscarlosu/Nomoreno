using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public static class ConversationHandler {

        public static Graph TruthGraph { get; set; }
        public static Graph LieGraph { get; set; }

        public static void SetupConversations(double percentageLiars, double percentageDescriptive) {
            // At least one descriptive lie.
            var descriptiveLieCount = (int)(NPC.NPCList.Count * percentageLiars * percentageDescriptive);
            descriptiveLieCount = descriptiveLieCount < 1 ? 1 : descriptiveLieCount;
            // Rest is misc lies.
            var miscLieCount = (int)(NPC.NPCList.Count * percentageLiars) - descriptiveLieCount;
            miscLieCount = miscLieCount < 0 ? 0 : miscLieCount;

            // Setup LieGraph.
            LieGraph = GraphBuilder.BuildLieGraph(descriptiveLieCount, miscLieCount, AIDirector.Instance.HidingDescriptiveNPCs, TruthGraph);

            // Hook generated conversations to NPCs
            foreach (NPC npc in NPC.NPCList) {
                var truthNode = TruthGraph.Nodes.FirstOrDefault(node => node.NPC.Equals(npc));
                var lieNode = LieGraph.Nodes.FirstOrDefault(node => node.NPC.Equals(npc));

                if (truthNode == null) { throw new Exception("TruthGraph does not contain clues for all NPCs!"); }

                npc.Conversation = new Conversation(truthNode.NodeClue);
                if (lieNode != null) npc.Conversation.PushStatement(lieNode.NodeClue);
            }

            foreach (NPC liar in LieGraph.Nodes.Select(node => node.NPC)) {
                if (liar.Conversation.IsTrue)
                    throw new Exception("Liars have not been connected with corrosponding lies.");
            }
        }
    }
}
