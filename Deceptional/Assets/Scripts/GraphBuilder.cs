using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts {
    public static class GraphBuilder {
        private static Random r = new Random();
        private static List<string> colors = new List<string>() { "Red", "Blue", "Green", "Yellow", "Black", "White" };
        private static List<string> clothing = new List<string>() { "Hat", "Shirt", "Pants" };

        public static Graph BuildLieGraph(double percentageLiars, Graph truthGraph) {
            int liarCount = (int)(truthGraph.Nodes.Where(n => !n.IsVisited).Count() * percentageLiars);
            List<NPC> liars = new List<NPC>();

            var lieGraph = new Graph();
            var nonKillers = truthGraph.Nodes.Where(node => !node.IsKiller).ToList();
            while (--liarCount >= 0) {
                if (nonKillers.Count == 0) { throw new Exception("NonKillers has been exhausted. Less liars required."); }

                Node n = nonKillers[r.Next(nonKillers.Count)];
                nonKillers.Remove(n);
                liars.Add(n.NPC); //n.NPC.Conversation.IsTrue = false;

                Node n_lie = new Node() { NPC = n.NPC };
                var possibleTargets = truthGraph.Nodes.Where(node => node.NPC != n_lie.NPC).ToList();
                n_lie.TargetNode = possibleTargets[r.Next(possibleTargets.Count)];
                n_lie.IsDescriptive = !Convert.ToBoolean(r.Next(5));

                lieGraph.Nodes.Add(n_lie);
                if (lieGraph.ReferenceLookup.ContainsKey(n_lie.TargetNode)) lieGraph.ReferenceLookup[n_lie.TargetNode].Add(n_lie);
                else lieGraph.ReferenceLookup.Add(n_lie.TargetNode, new List<Node>() { n_lie });

                var lieTarget = n_lie.TargetNode.NPC;
                
                // Creates lie-related Clue object.
                ClueIdentifier lieClueID;
                NPCPart.NPCPartType npcPartType = NPCPart.NPCPartType.None;
                if (n_lie.IsDescriptive) {
                    var lieClothing = clothing[r.Next(clothing.Count)];
                    var lieColor = string.Empty;
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

                    n_lie.Clue = ClueConverter.ConstructClue(n_lie.IsDescriptive, lieColor, lieClothing, lieTarget.Name, lieTarget.IsMale);
                    lieClueID = ClueIdentifier.Descriptive;
                } else {
                    n_lie.Clue = ClueConverter.ConstructClue(liars.Contains(n_lie.TargetNode.NPC), lieTarget.Name, lieTarget.IsMale);
                    lieClueID = liars.Contains(n_lie.TargetNode.NPC) ? ClueIdentifier.Accusatory : ClueIdentifier.Informational; // n_lie.TargetNode.NPC.Conversation.IsTrue
                }

                n_lie.NodeClue = new Clue(n_lie.Clue, n_lie.TargetNode.NPC, lieClueID, npcPartType);

                // Inverts truth statements targeted at NPC hooked to current node.
                List<Node> refNodes = new List<Node>();
                if (truthGraph.ReferenceLookup.TryGetValue(n, out refNodes))
                    foreach (Node refN in refNodes) {
                        refN.Clue = ClueConverter.ConstructClue(true, n.NPC.Name, n.NPC.IsMale);
                        refN.NodeClue = new Clue(refN.Clue, refN.TargetNode.NPC, ClueIdentifier.Accusatory, NPCPart.NPCPartType.None);
                    }
                // Inverts deceptive statements targeted at NPC hooked to current node.
                if (lieGraph.ReferenceLookup.TryGetValue(n, out refNodes))
                    foreach (Node refN in refNodes)
                        if (!refN.IsDescriptive) {
                            refN.Clue = ClueConverter.ConstructClue(false, n.NPC.Name, n.NPC.IsMale);
                            refN.NodeClue = new Clue(refN.Clue, refN.TargetNode.NPC, ClueIdentifier.Informational, NPCPart.NPCPartType.None);
                        }
            }

            return lieGraph;
        }

        public static Graph BuildRandomGraph(int nodeCount, int descriptiveCount) {
            if (nodeCount < descriptiveCount) throw new ArgumentException("NodeCount cannot be less than DescriptiveCount.\n");

            // Add Killer Node.
            // Add Descriptive Nodes.
            // Add Informational Nodes.
            // Add Boolean Nodes..?
            Graph g = new Graph();

            // Adding Killer Node
            var killerNode = new Node();
            killerNode.NPC = NPC.NPCList.FirstOrDefault(npc => npc.IsKiller);
            killerNode.IsKiller = true;
            killerNode.IsVisited = true;
            g.Nodes.Add(killerNode);

            //Console.WriteLine("Adding descriptive nodes...");
            var nonKillers = NPC.NPCList.Where(npc => !npc.IsKiller).ToList();
            for (int i = 0; i < descriptiveCount; i++) {
                var descriptiveNode = new Node() { 
                    IsDescriptive = true,
                    IsVisited = true,
                    TargetNode = g.Nodes[0]
                };

                NPCPart.NPCPartType cluePartType;
                switch (r.Next(3)) {
                    case 0:
                        descriptiveNode.Clue = ClueConverter.ConstructClue(true, NPCDescriptionToString(descriptiveNode.TargetNode.NPC.Head.Description), "hat", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale);
                        cluePartType = NPCPart.NPCPartType.Hat;
                        break;
                    case 1:
                        descriptiveNode.Clue = ClueConverter.ConstructClue(true, NPCDescriptionToString(descriptiveNode.TargetNode.NPC.Torso.Description), "shirt", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale);
                        cluePartType = NPCPart.NPCPartType.Shirt;
                        break;
                    case 2:
                        descriptiveNode.Clue = ClueConverter.ConstructClue(true, NPCDescriptionToString(descriptiveNode.TargetNode.NPC.Legs.Description), "pants", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale);
                        cluePartType = NPCPart.NPCPartType.Pants;
                        break;
                    default: throw new Exception("Random generator tried to access non-existant clothing.");
                }


                descriptiveNode.NPC = nonKillers.FirstOrDefault();

                // Create Clue object for Node
                descriptiveNode.NodeClue = new Clue(descriptiveNode.Clue, descriptiveNode.TargetNode.NPC, ClueIdentifier.Descriptive, cluePartType);

                g.Nodes.Add(descriptiveNode);
                nonKillers.Remove(descriptiveNode.NPC);
            }

            //Console.WriteLine("Adding remaining nodes...");
            while (g.Nodes.Count < nodeCount) {
                var restNode = new Node();
                restNode.NPC = NPC.NPCList.Where(npc => !g.Nodes.Any(node => node.NPC.Equals(npc))).FirstOrDefault();
                g.Nodes.Add(restNode);
            }

            //Console.WriteLine("Setting up remaining nodes...");
            foreach (Node restNode in g.Nodes.Where(n => !n.IsVisited)) {
                do restNode.TargetNode = g.Nodes[r.Next(g.Nodes.Count)];
                while (restNode == restNode.TargetNode);

                string clothingColor = "<COLOR_ERROR>";
                NPCPart.NPCPartType restNPCPartType;
                switch (r.Next(3)) {
                    case 0:
                        clothingColor = NPCDescriptionToString(restNode.TargetNode.NPC.Head.Description);
                        restNPCPartType = NPCPart.NPCPartType.Hat;
                        break;
                    case 1:
                        clothingColor = NPCDescriptionToString(restNode.TargetNode.NPC.Torso.Description);
                        restNPCPartType = NPCPart.NPCPartType.Shirt;
                        break;
                    case 2:
                        clothingColor = NPCDescriptionToString(restNode.TargetNode.NPC.Legs.Description);
                        restNPCPartType = NPCPart.NPCPartType.Pants;
                        break;
                    default: throw new Exception("Random generator tried to access non-existant clothing.");
                }

                restNode.Clue = ClueConverter.ConstructClue(clothingColor, restNode.TargetNode.NPC.Name, restNode.TargetNode.NPC.IsMale);
                restNode.NodeClue = new Clue(restNode.Clue, restNode.TargetNode.NPC, ClueIdentifier.Informational, restNPCPartType);
            }

            // Set up killerNode.Clue
            //Console.WriteLine("Setting up killer node clue...");
            var restNodes = g.Nodes.Where(n => !n.IsDescriptive && !n.IsKiller).ToList();
            g.Nodes[0].TargetNode = restNodes[r.Next(restNodes.Count)];
            string kTargetClothingColor = "<COLOR_ERROR>";
            NPCPart.NPCPartType npcPartType;
            switch (r.Next(3)) {
                case 0:
                    kTargetClothingColor = NPCDescriptionToString(g.Nodes[0].TargetNode.NPC.Head.Description);
                    npcPartType = NPCPart.NPCPartType.Hat;
                    break;
                case 1:
                    kTargetClothingColor = NPCDescriptionToString(g.Nodes[0].TargetNode.NPC.Torso.Description);
                    npcPartType = NPCPart.NPCPartType.Shirt;
                    break;
                case 2:
                    kTargetClothingColor = NPCDescriptionToString(g.Nodes[0].TargetNode.NPC.Legs.Description);
                    npcPartType = NPCPart.NPCPartType.Pants;
                    break;
                default: throw new Exception("Random generator tried to access non-existant clothing.");
            }
            g.Nodes[0].Clue = ClueConverter.ConstructClue(kTargetClothingColor, g.Nodes[0].TargetNode.NPC.Name, g.Nodes[0].TargetNode.NPC.IsMale);
            g.Nodes[0].NodeClue = new Clue(g.Nodes[0].Clue, g.Nodes[0].TargetNode.NPC, ClueIdentifier.Informational, npcPartType);

            //Console.WriteLine("Setting up reference lookup...");
            foreach (Node n in g.Nodes) { 
                n.IsVisited = false;
                if (g.ReferenceLookup.ContainsKey(n.TargetNode))
                    g.ReferenceLookup[n.TargetNode].Add(n);
                else
                    g.ReferenceLookup.Add(n.TargetNode, new List<Node>() { n });
            }

            //Console.WriteLine("Returning graph!");
            return g;
        }

        private static string NPCDescriptionToString(NPCPart.NPCPartDescription description) {
            return Enum.GetName(typeof(NPCPart.NPCPartDescription), description);
        }
    }
}
