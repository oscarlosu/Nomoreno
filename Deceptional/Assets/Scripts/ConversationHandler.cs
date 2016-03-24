using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public static class ConversationHandler {
        private static Random r = new Random();
        private static List<string> colors = new List<string>() { "Red", "Blue", "Green", "Yellow", "Black", "White" };
        private static List<string> clothing = new List<string>() { "Hat", "Shirt", "Pants" };

        public static Graph TruthGraph { get; set; }
        public static Graph LieGraph { get; set; }

        public static void SetupConversations(double percentageLiars) {
            int liarCount = (int)((double)TruthGraph.Nodes.Where(n => !n.IsVisited).ToList().Count() * percentageLiars);
            
            foreach (Node n in TruthGraph.Nodes)
                n.NPC.Conversation = new Conversation(n.Clue);
            
            // GRAPH THINGS
            LieGraph = new Graph();
            var nonKillers = TruthGraph.Nodes.Where(node => !node.IsKiller).ToList();
            while (--liarCount >= 0) {
                if (nonKillers.Count == 0) { throw new Exception("NonKillers has been exhausted. Less liars required."); }

                Node n = nonKillers[r.Next(nonKillers.Count)];
                nonKillers.Remove(n);
                n.NPC.Conversation.IsTrue = false;

                Node n_lie = new Node() { NPC = n.NPC };
                var possibleTargets = TruthGraph.Nodes.Where(node => node.NPC != n_lie.NPC).ToList();
                n_lie.TargetNode = possibleTargets[r.Next(possibleTargets.Count)];
                n_lie.IsDescriptive = !Convert.ToBoolean(r.Next(5));

                LieGraph.Nodes.Add(n_lie);
                if (LieGraph.ReferenceLookup.ContainsKey(n_lie.TargetNode)) LieGraph.ReferenceLookup[n_lie.TargetNode].Add(n_lie);
                else LieGraph.ReferenceLookup.Add(n_lie.TargetNode, new List<Node>() { n_lie });

                var lieTarget = n_lie.TargetNode.NPC;
                var lieClothing = clothing[r.Next(clothing.Count)];
                var lieColor = string.Empty;
                NPCPart.NPCPartType npcPartType;
                switch (lieClothing) {
                    case "Hat":
                        lieColor = Enum.GetName(typeof(NPCPart.NPCPartDescription), lieTarget.Head.Description);
                        npcPartType = NPCPart.NPCPartType.Hat;
                        break;
                    case "Shirt":
                        lieColor = Enum.GetName(typeof(NPCPart.NPCPartDescription), lieTarget.Torso.Description);
                        npcPartType = NPCPart.NPCPartType.Shirt;
                        break;
                    case "Pants":
                        lieColor = Enum.GetName(typeof(NPCPart.NPCPartDescription), lieTarget.Legs.Description);
                        npcPartType = NPCPart.NPCPartType.Pants;
                        break;
                    default: throw new Exception("Clothing doesn't exist");
                }

                ClueIdentifier lieClueID;
                if (n_lie.IsDescriptive) {
                    n_lie.Clue = ClueConverter.ConstructClue(n_lie.IsDescriptive, lieColor, lieClothing, lieTarget.Name, lieTarget.IsMale);
                    lieClueID = ClueIdentifier.Descriptive;
                } else {
                    n_lie.Clue = ClueConverter.ConstructClue(n_lie.TargetNode.NPC.Conversation.IsTrue, lieTarget.Name, lieTarget.IsMale);
                    lieClueID = n_lie.TargetNode.NPC.Conversation.IsTrue ? ClueIdentifier.Accusatory : ClueIdentifier.Informational;
                }
                //n_lie.Clue = n_lie.IsDescriptive ?
                //    ClueConverter.ConstructClue(n_lie.IsDescriptive, lieColor, lieClothing, lieTarget.Name, lieTarget.IsMale) :
                //    ClueConverter.ConstructClue(n_lie.TargetNode.NPC.Conversation.IsTrue, lieTarget.Name, lieTarget.IsMale);

                // Construct NodeClue
                n_lie.NodeClue = new Clue(n_lie.Clue, n_lie.TargetNode.NPC, lieClueID, npcPartType);

                // Change references to fit liar.
                List<Node> refNodes = new List<Node>();
                if (TruthGraph.ReferenceLookup.TryGetValue(n, out refNodes))
                    foreach (Node refN in refNodes) { 
                        refN.Clue = ClueConverter.ConstructClue(true, n.NPC.Name, n.NPC.IsMale);
                        // NPCPart should be redundant here, Hat acts as placeholder.
                        refN.NodeClue = new Clue(refN.Clue, refN.TargetNode.NPC, ClueIdentifier.Accusatory, NPCPart.NPCPartType.Hat); 
                    }
                if (LieGraph.ReferenceLookup.TryGetValue(n, out refNodes))
                    foreach (Node refN in refNodes)
                        if (!refN.IsDescriptive) {
                            refN.Clue = ClueConverter.ConstructClue(false, n.NPC.Name, n.NPC.IsMale);
                            // NPCPart should be redundant here, Hat acts as placeholder.
                            refN.NodeClue = new Clue(refN.Clue, refN.TargetNode.NPC, ClueIdentifier.Informational, NPCPart.NPCPartType.Hat);
                        }
            }

            int i = 0;
            foreach (NPC npc in NPC.NPCList) {
                i++;
                var truthNode = TruthGraph.Nodes.FirstOrDefault(node => node.NPC.Equals(npc));
                var lieNode = LieGraph.Nodes.FirstOrDefault(node => node.NPC.Equals(npc));

                if (truthNode == null) { throw new Exception("TruthGraph does not contain clues for all NPCs."); }

                npc.Conversation = new Conversation(truthNode.Clue);
                if (lieNode != null) npc.Conversation.PushStatement(lieNode.Clue);
            }
        }
    }
}
