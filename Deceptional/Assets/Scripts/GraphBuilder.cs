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
            //Console.WriteLine("Building Graph...");
            Graph g = new Graph();

            //Console.WriteLine("Adding killer node...");
            var killerNode = new Node();
            killerNode.NPC = NPCList.Instance.Get(0);
            //killerNode.NPC = new DataNPC() { // Should be considered a standard NPC or the like.
            //    IsMale = Convert.ToBoolean(r.Next(2)), 
            //    HatColor = ClueConverter.GetRandomColor(),
            //    ShirtColor = ClueConverter.GetRandomColor(),
            //    PantsColor = ClueConverter.GetRandomColor()
            //};
            //killerNode.NPC.Name = ClueConverter.GetRandomName(killerNode.NPC.IsMale);
            killerNode.IsKiller = true;
            killerNode.IsVisited = true;
            g.Nodes.Add(killerNode);

            //Console.WriteLine("Adding descriptive nodes...");
            for (int i = 0; i < descriptiveCount; i++) {
                var descriptiveNode = new Node() { 
                    IsDescriptive = true,
                    IsVisited = true,
                    TargetNode = g.Nodes[0]
                };

                switch (r.Next(3)) {
                    case 0: descriptiveNode.Clue = ClueConverter.ConstructClue(true, descriptiveNode.TargetNode.NPC.HatColor, "hat", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale); break;
                    case 1: descriptiveNode.Clue = ClueConverter.ConstructClue(true, descriptiveNode.TargetNode.NPC.ShirtColor, "shirt", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale); break;
                    case 2: descriptiveNode.Clue = ClueConverter.ConstructClue(true, descriptiveNode.TargetNode.NPC.PantsColor, "pants", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale); break;
                    default: throw new Exception("Random generator tried to access non-existant clothing.");
                }

                g.Nodes.Add(descriptiveNode);
            }

            //Console.WriteLine("Adding remaining nodes...");
            for (int i = descriptiveCount; i < nodeCount; i++) {
                var restNode = new Node();
                restNode.NPC = new DataNPC() {
                    IsMale = Convert.ToBoolean(r.Next(2)),
                    HatColor = ClueConverter.GetRandomColor(),
                    ShirtColor = ClueConverter.GetRandomColor(),
                    PantsColor = ClueConverter.GetRandomColor()
                };
                restNode.NPC.Name = ClueConverter.GetRandomName(restNode.NPC.IsMale);
                g.Nodes.Add(restNode);
            }

            //Console.WriteLine("Setting up remaining nodes...");
            foreach (Node restNode in g.Nodes.Where(n => !n.IsVisited)) {
                Node targetNode = null;
                while (restNode != targetNode) 
                    targetNode = g.Nodes[r.Next(g.Nodes.Count)];
                restNode.TargetNode = targetNode;

                string clothingColor = "<COLOR_ERROR>";
                switch (r.Next(3)) {
                    case 0: clothingColor = restNode.TargetNode.NPC.HatColor; break;
                    case 1: clothingColor = restNode.TargetNode.NPC.ShirtColor; break;
                    case 2: clothingColor = restNode.TargetNode.NPC.PantsColor; break;
                    default: throw new Exception("Random generator tried to access non-existant clothing.");
                }
                restNode.Clue = ClueConverter.ConstructClue(clothingColor, restNode.TargetNode.NPC.Name, restNode.TargetNode.NPC.IsMale);
            }

            // Set up killerNode.Clue
            //Console.WriteLine("Setting up killer node clue...");
            var restNodes = g.Nodes.Where(n => !n.IsVisited).ToList();
            g.Nodes[0].TargetNode = restNodes[r.Next(restNodes.Count)];
            string kTargetClothingColor = "<COLOR_ERROR>";
            switch (r.Next(3)) {
                case 0: kTargetClothingColor = g.Nodes[0].TargetNode.NPC.HatColor; break;
                case 1: kTargetClothingColor = g.Nodes[0].TargetNode.NPC.ShirtColor; break;
                case 2: kTargetClothingColor = g.Nodes[0].TargetNode.NPC.PantsColor; break;
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
    }
}
