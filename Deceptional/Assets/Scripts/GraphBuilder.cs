using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public class GraphBuilder {
        public static Graph BuildRandomGraph(int nodeCount, int descriptiveCount) {
            Graph g = new Graph();
            Random r = new Random();
            if (nodeCount < descriptiveCount)
                throw new ArgumentException("NodeCount cannot be less than DescriptiveCount.\n");

            // Fill node list.
            for (int i = 0; i < nodeCount; i++)
                g.Nodes.Add(new Node());

            // Create killer node.
            g.Nodes[0].IsKiller = true;
            g.Nodes[0].IsVisited = true;

            // Hook descriptive nodes to killer node.
            for (int i = 0; i < descriptiveCount; i++) {
                int descNodeIdx = r.Next(0, nodeCount);

                if (g.Nodes[descNodeIdx].IsVisited) {
                    i--;
                    continue;
                }

                g.Nodes[descNodeIdx].IsDescriptive = true;
                g.Nodes[descNodeIdx].IsVisited = true;
                g.Nodes[descNodeIdx].TargetNode = g.Nodes[0];
            }


            // Hook remaining nodes & killerNode.
            IEnumerable<Node> remainingNodes = g.Nodes.Where(n => !n.IsVisited);
            int killerTargetNode = r.Next(0, remainingNodes.Count());
            int idx = 0;
            foreach (Node n in remainingNodes) {
                int targetNodeIdx = r.Next(0, nodeCount);
                if (g.Nodes[targetNodeIdx].Equals(n))
                    targetNodeIdx = targetNodeIdx == nodeCount - 1 ?
                        targetNodeIdx - 1 :
                        targetNodeIdx + 1;

                n.TargetNode = g.Nodes[targetNodeIdx];

                idx++;
                if (idx == killerTargetNode) {
                    g.Nodes[0].TargetNode = n;
                }
            }

            foreach (Node n in g.Nodes)
                n.IsVisited = false;
            
            return g; 
        }
    }
}
