using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts {
    public static class GraphBuilder {        
        public static Graph BuildLieGraph(int descriptiveLies, int miscLies, int hidingDescriptive, Graph truthGraph) {
            // Calculating amount of liars.
            int liarCount = descriptiveLies + hidingDescriptive + miscLies;
            List<NPC> liars = new List<NPC>();

            // Create neccessary graph and lists.
            var lieGraph = new Graph();
            var nonKillerNodes = truthGraph.Nodes.Where(node => !node.IsKiller).ToList();
            var nonKillerNPCs = NPC.NPCList.Where(npc => !npc.IsKiller);

            // Create descriptive lists.
            // This is to make a suitable amount of Descriptive NPCs lie.
            var descriptiveNodes = nonKillerNodes.Where(node => node.IsDescriptive).ToList();
            var descriptiveNPCs = nonKillerNPCs.Where(npc => descriptiveNodes.Any(node => node.NPC == npc)).ToList();
            // If there is very few Descriptive NPCs, they will all be in hiding.
            if (descriptiveNPCs.Count < hidingDescriptive) {
                hidingDescriptive = descriptiveNPCs.Count;
                liarCount = descriptiveLies + hidingDescriptive + miscLies;
            }

            // Create non-similar lists.
            // This is to avoid lies accidently aligning with the truth.
            var killer = NPC.NPCList.First(npc => npc.IsKiller);
            var nonSimilarHats = nonKillerNPCs.Where(npc => npc.Head.Description != killer.Head.Description);
            var nonSimilarTorsos = nonKillerNPCs.Where(npc => npc.Torso.Description != killer.Torso.Description);
            var nonSimilarPants = nonKillerNPCs.Where(npc => npc.Legs.Description != killer.Legs.Description);

            while (--liarCount >= 0) {
                // Find liar node and remove it from nonKillers.
                Node n;
                if (hidingDescriptive > 0) {
                    n = descriptiveNodes[PlayerController.Instance.Rng.Next(descriptiveNodes.Count)];
                    descriptiveNodes.Remove(n);
                    hidingDescriptive--;
                } else {
                    n = nonKillerNodes[PlayerController.Instance.Rng.Next(nonKillerNodes.Count)];
                }
                // Remove the node from nonKillerNodes and add the NPC to liars.
                nonKillerNodes.Remove(n);
                liars.Add(n.NPC);

                if (nonKillerNodes.Count == 0) { throw new Exception("NonKillers has been exhausted. Less liars required."); }

                List<Node> possibleTargets;
                // Creates lie-related Clue object.
                Node n_lie;
                // Rolls a dice to determine whether new lie should be descriptive.
                if (PlayerController.Instance.Rng.Next(nonKillerNPCs.Count()) < descriptiveNPCs.Count) {
                    int partIdx = PlayerController.Instance.Rng.Next(3);
                    switch (partIdx) {
                        case 0: possibleTargets = nonKillerNodes.Where(node => nonSimilarHats.Any(npc => npc == node.NPC)).ToList(); break;
                        case 1: possibleTargets = nonKillerNodes.Where(node => nonSimilarTorsos.Any(npc => npc == node.NPC)).ToList(); break;
                        case 2: possibleTargets = nonKillerNodes.Where(node => nonSimilarPants.Any(npc => npc == node.NPC)).ToList(); break;
                        default: throw new Exception("Random generator tried to access non-existant clothing.");
                    }

                    Node lieTargetNode = possibleTargets[PlayerController.Instance.Rng.Next(possibleTargets.Count)];
                    n_lie = ClueFactory.Instance.CreateDescriptiveNode(lieTargetNode, n.NPC, partIdx);
                } else {
                    possibleTargets = truthGraph.Nodes.Where(node => node.NPC != n.NPC).ToList();
                    Node lieTargetNode = possibleTargets[PlayerController.Instance.Rng.Next(possibleTargets.Count)];

                    // Current liar should initially tell the truth about his target, since references are flipped upon graph merge.
                    var lieIdentifier = liars.Contains(n.TargetNodes.First().NPC) ? ClueIdentifier.Accusatory : ClueIdentifier.Informational;
                    n_lie = ClueFactory.Instance.CreateSupportNode(lieTargetNode, n.NPC, lieIdentifier);
                }

                lieGraph.Nodes.Add(n_lie);
                // Setup current reference in the graph.
                SetupReference(lieGraph, n_lie);
                // Setup references to the lie.
                SetupLieReference(truthGraph, lieGraph, n);
            }

            return lieGraph;
        }

        #region Graph Rigging
        private static void SetupReference(Graph g, Node n) {
            n.IsVisited = false;
            if (g.ReferenceLookup.ContainsKey(n.TargetNodes.First()))
                g.ReferenceLookup[n.TargetNodes.First()].Add(n);
            else
                g.ReferenceLookup.Add(n.TargetNodes.First(), new List<Node>() { n });
        }

        private static void SetupLieReference(Graph truthGraph, Graph lieGraph, Node n) {
            List<Node> refNodes = new List<Node>();
            // Inverts truth statements targeted at NPC hooked to current node.
            if (truthGraph.ReferenceLookup.TryGetValue(n, out refNodes))
                foreach (Node refN in refNodes) {
                    var newRefClueTemplate = ClueConverter.GetClueTemplate(ClueIdentifier.Accusatory);
                    refN.NodeClue = new Clue(newRefClueTemplate, refN.TargetNodes.First().NPC, ClueIdentifier.Accusatory, NPCPart.NPCPartType.None);
                }
            // Inverts deceptive statements targeted at NPC hooked to current node.
            if (lieGraph.ReferenceLookup.TryGetValue(n, out refNodes))
                foreach (Node refN in refNodes)
                    if (!refN.IsDescriptive) {
                        var newRefClueTemplate = ClueConverter.GetClueTemplate(ClueIdentifier.Informational);
                        refN.NodeClue = new Clue(newRefClueTemplate, refN.TargetNodes.First().NPC, ClueIdentifier.Informational, NPCPart.NPCPartType.None);
                    }
        }
        #endregion

        /// <summary>
        /// Builds a graph using random parameters.
        /// </summary>
        /// <param name="nodeCount">The total node count of the graph.</param>
        /// <param name="descriptiveCount">The amount of descriptive nodes that should appear in the graph.</param>
        /// <returns>A new Graph built using random parameters.</returns>
        public static Graph BuildRandomGraph(int nodeCount, int descriptiveCount) {
            while (nodeCount - descriptiveCount - 1 < 0 && descriptiveCount > 0)
                descriptiveCount--;

            /// WorkFlow:
            // Add Killer Node.
            // Add/Setup Descriptive Nodes.
            // Add/Setup Support Nodes.
            // Setup Killer Node.
            Graph g = new Graph();

            // Adding Killer Node
            g.Nodes.Add(ClueFactory.Instance.CreateKillerNode());

            // Adds descriptive nodes and hooks them to the kiler node.
            g.Nodes.AddRange(ClueFactory.Instance.CreateDescriptiveNodes(g.Nodes[0], descriptiveCount));

            // Creating support Nodes
            while (g.Nodes.Count < nodeCount) {
                // Find random NPC which does not currently have a truth statement attached.
                var hookNPC = NPC.NPCList.Where(npc => !g.Nodes.Any(node => node.NPC.Equals(npc))).FirstOrDefault();

                // Find TargetNodes without self-referencing.
                Node newTarget = g.Nodes.Where(node => node.NPC != hookNPC).ToList()[PlayerController.Instance.Rng.Next(g.Nodes.Count)];
                //while (newTarget == newTarget.TargetNodes) {
                if (newTarget.TargetNodes != null && newTarget == newTarget.TargetNodes.First()) {
                    //newTarget = g.Nodes[r.Next(g.Nodes.Count)];
                    throw new Exception("NewTarget references itself.");
                }

                var restNode = ClueFactory.Instance.CreateSupportNode(newTarget, hookNPC);
                g.Nodes.Add(restNode);
            }

            // Set up killerNode.Clue
            var restNodes = g.Nodes.Where(n => !n.IsDescriptive && !n.IsKiller).ToList();
            // If there's not any other NPCs than descriptive & killer, pick descriptive.
            if (restNodes.Count < 1)
                restNodes = g.Nodes.Where(n => !n.IsKiller).ToList();
            // If there's not any other NPCs than killer, return g with killer referencing self.
            if (restNodes.Count < 1) {
                g.Nodes[0].TargetNodes = new List<Node>() { g.Nodes[0] };
                ClueFactory.Instance.SetupSupportNode(g.Nodes[0], g.Nodes[0].TargetNodes.First());
                return g;
            } else { 
                g.Nodes[0].TargetNodes = new List<Node>() { restNodes[PlayerController.Instance.Rng.Next(restNodes.Count)] };
                // Sets up the KillerNode with a support clue.
                ClueFactory.Instance.SetupSupportNode(g.Nodes[0], g.Nodes[0].TargetNodes.First());
            }

            // Managing ReferenceLookup.
            foreach (Node n in g.Nodes) {
                SetupReference(g, n);
            }

            // Returning.
            return g;
        }

        private static string NPCDescriptionToString(NPCPart.NPCPartDescription description) {
            return Enum.GetName(typeof(NPCPart.NPCPartDescription), description);
        }
    }
}
