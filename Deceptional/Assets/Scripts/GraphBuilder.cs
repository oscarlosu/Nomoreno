using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts {
    public static class GraphBuilder {
        private static Random r = new Random();

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

                switch (r.Next(3)) {
                    case 0: descriptiveNode.Clue = ClueConverter.ConstructClue(true, NPCDescriptionToString(descriptiveNode.TargetNode.NPC.Head.Description), "hat", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale); break;
                    case 1: descriptiveNode.Clue = ClueConverter.ConstructClue(true, NPCDescriptionToString(descriptiveNode.TargetNode.NPC.Torso.Description), "shirt", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale); break;
                    case 2: descriptiveNode.Clue = ClueConverter.ConstructClue(true, NPCDescriptionToString(descriptiveNode.TargetNode.NPC.Legs.Description), "pants", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale); break;
                    default: throw new Exception("Random generator tried to access non-existant clothing.");
                }

                descriptiveNode.NPC = nonKillers.FirstOrDefault();
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
                switch (r.Next(3)) {
                    case 0: clothingColor = NPCDescriptionToString(restNode.TargetNode.NPC.Head.Description); break;
                    case 1: clothingColor = NPCDescriptionToString(restNode.TargetNode.NPC.Torso.Description); break;
                    case 2: clothingColor = NPCDescriptionToString(restNode.TargetNode.NPC.Legs.Description); break;
                    default: throw new Exception("Random generator tried to access non-existant clothing.");
                }

                restNode.Clue = ClueConverter.ConstructClue(clothingColor, restNode.TargetNode.NPC.Name, restNode.TargetNode.NPC.IsMale);
            }

            // Set up killerNode.Clue
            //Console.WriteLine("Setting up killer node clue...");
            var restNodes = g.Nodes.Where(n => !n.IsDescriptive && !n.IsKiller).ToList();
            g.Nodes[0].TargetNode = restNodes[r.Next(restNodes.Count)];
            string kTargetClothingColor = "<COLOR_ERROR>";
            switch (r.Next(3)) {
                case 0: kTargetClothingColor = NPCDescriptionToString(g.Nodes[0].TargetNode.NPC.Head.Description); break;
                case 1: kTargetClothingColor = NPCDescriptionToString(g.Nodes[0].TargetNode.NPC.Torso.Description); break;
                case 2: kTargetClothingColor = NPCDescriptionToString(g.Nodes[0].TargetNode.NPC.Legs.Description); break;
                default: throw new Exception("Random generator tried to access non-existant clothing.");
            }
            g.Nodes[0].Clue = ClueConverter.ConstructClue(kTargetClothingColor, g.Nodes[0].TargetNode.NPC.Name, g.Nodes[0].TargetNode.NPC.IsMale);

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
