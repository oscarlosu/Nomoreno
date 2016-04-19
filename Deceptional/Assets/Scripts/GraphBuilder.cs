using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts {
    public static class GraphBuilder {
        private static Random r = PlayerController.Instance.UseFixedSeed ? new Random(PlayerController.Instance.GeneratorSeed) : new Random(DateTime.Now.Millisecond);
        
        public static Graph BuildLieGraph(double percentageLiars, int descriptiveLies, Graph truthGraph) {
            // Calculating amount of liars.
            int liarCount = (int)(truthGraph.Nodes.Where(n => !n.IsVisited).Count() * percentageLiars);
            List<NPC> liars = new List<NPC>();

            // Create neccessary graph and lists.
            var lieGraph = new Graph();
            var nonKillerNodes = truthGraph.Nodes.Where(node => !node.IsKiller).ToList();
            var nonKillerNPCs = NPC.NPCList.Where(npc => !npc.IsKiller);

            // Create non-similar lists.
            var killer = NPC.NPCList.First(npc => npc.IsKiller);
            var nonSimilarHats = nonKillerNPCs.Where(npc => npc.Head.Description != killer.Head.Description);
            var nonSimilarTorsos = nonKillerNPCs.Where(npc => npc.Torso.Description != killer.Torso.Description);
            var nonSimilarPants = nonKillerNPCs.Where(npc => npc.Legs.Description != killer.Legs.Description);

            
            while (--liarCount >= 0) {
                // Find liar node and remove it from nonKillers.
                Node n = nonKillerNodes[r.Next(nonKillerNodes.Count)];
                nonKillerNodes.Remove(n);
                liars.Add(n.NPC);

                if (nonKillerNodes.Count == 0) { throw new Exception("NonKillers has been exhausted. Less liars required."); }

                var possibleTargets = truthGraph.Nodes.Where(node => node.NPC != n.NPC).ToList();
                Node lieTargetNode = possibleTargets[r.Next(possibleTargets.Count)];
                // Creates lie-related Clue object.
                Node n_lie;
                if (--descriptiveLies >= 0) {
                    n_lie = CreateDescriptiveNode(lieTargetNode, n.NPC);
                } else {
                    // Current liar should initially tell the truth about his target, since references are flipped upon graph merge.
                    var lieIdentifier = liars.Contains(n.TargetNode.NPC) ? ClueIdentifier.Accusatory : ClueIdentifier.Informational;
                    n_lie = CreateSupportNode(lieTargetNode, n.NPC, lieIdentifier);
                }

                lieGraph.Nodes.Add(n_lie);
                // Setup current reference in the graph.
                SetupReference(lieGraph, n_lie);
                // Setup references to the lie.
                SetupLieReference(truthGraph, lieGraph, n);
            }

            return lieGraph;
        }

        #region Node handling
        #region Descriptive Nodes
        /// <summary>
        /// Creates a Node for the truth graph, targeting the Node target and hooks the new Node to hookNPC
        /// </summary>
        /// <param name="target">Node which the new node should reference.</param>
        /// <param name="hookNPC">NPC which is connected to the new Node.</param>
        /// <param name="partIdx">Part of NPC being described (0: Head, 1: Torso, 2: Legs).</param>
        /// <returns>New Descriptive clue Node.</returns>
        public static Node CreateDescriptiveNode(Node target, NPC hookNPC, int partIdx) {
            var descriptiveNode = new Node() {
                IsDescriptive = true,
                TargetNode = target
            };

            string template = ClueConverter.GetClueTemplate(ClueIdentifier.Descriptive);
            NPCPart.NPCPartType cluePartType;
            switch (partIdx) {
                case 0: cluePartType = NPCPart.NPCPartType.Hat; break;
                case 1: cluePartType = NPCPart.NPCPartType.Shirt; break;
                case 2: cluePartType = NPCPart.NPCPartType.Pants; break;
                default: throw new Exception("Random generator tried to access non-existant clothing.");
            }

            descriptiveNode.NPC = hookNPC;
            descriptiveNode.NodeClue = new Clue(template, descriptiveNode.TargetNode.NPC, ClueIdentifier.Descriptive, cluePartType);

            return descriptiveNode;
        }

        public static Node CreateDescriptiveNode(Node target, NPC hookNPC) {
            return CreateDescriptiveNode(target, hookNPC, r.Next(3));
        }
        
        /// <summary>
        /// Creates a list of Descriptive nodes with equal distribution.
        /// </summary>
        /// <param name="targetNode">The target of description.</param>
        /// <param name="count">The amount of nodes returned.</param>
        /// <returns></returns>
        private static List<Node> CreateDescriptiveNodes(Node targetNode, int count) {
            List<Node> returnList = new List<Node>();
            var nonKillers = NPC.NPCList.Where(npc => !npc.IsKiller).ToList();
            
            if (count == 1) {
                // Select one body part
                int partIdx = r.Next(3);
                var hookNPC = nonKillers.FirstOrDefault();
                returnList.Add(CreateDescriptiveNode(targetNode, hookNPC, partIdx));
                nonKillers.Remove(hookNPC);
            } else if (count == 2) {
                int excludeIdx = r.Next(3);
                for (int i = 0; i < 3; i++) {
                    // Skip one body part
                    if (i == excludeIdx) continue;
                    var hookNPC = nonKillers.FirstOrDefault();
                    returnList.Add(CreateDescriptiveNode(targetNode, hookNPC, i));
                    nonKillers.Remove(hookNPC);
                }
            } else {
                for (int i = 0; i < count; i++) {
                    var hookNPC = nonKillers.FirstOrDefault();
                    returnList.Add(CreateDescriptiveNode(targetNode, hookNPC, i % 3));
                    nonKillers.Remove(hookNPC);
                }
            }

            return returnList;
        }
        #endregion

        #region Support Nodes (Accusatory/Informative)
        /// <summary>
        /// Creates a node, hooked to a NPC, and attaches a support clue to it, referencing the target Node.
        /// </summary>
        /// <param name="target">The Node containing target information.</param>
        /// <param name="hookNPC">The NPC related to this Node.</param>
        /// <returns>A new support node, connected to the hookNPC.</returns>
        public static Node CreateSupportNode(Node target, NPC hookNPC) { return CreateSupportNode(target, hookNPC, ClueIdentifier.Informational); }
        public static Node CreateSupportNode(Node target, NPC hookNPC, ClueIdentifier identifier) {
            var supportNode = new Node() {
                NPC = hookNPC,
                TargetNode = target
            };

            SetupSupportNode(supportNode, target, identifier);

            return supportNode;
        }

        /// <summary>
        /// Attaches a support clue to a specified Node, referencing the target Node.
        /// </summary>
        /// <param name="baseNode">The Node in need of a clue.</param>
        /// <param name="targetNode">The Node containing target information.</param>
        /// <param name="identifier">The identifier that determines the nature of the statement.</param>
        public static void SetupSupportNode(Node baseNode, Node targetNode) { SetupSupportNode(baseNode, targetNode, ClueIdentifier.Informational); }
        public static void SetupSupportNode(Node baseNode, Node targetNode, ClueIdentifier identifier) {
            baseNode.TargetNode = targetNode;
            var template = ClueConverter.GetClueTemplate(identifier);
            baseNode.NodeClue = new Clue(template, baseNode.TargetNode.NPC, identifier, NPCPart.NPCPartType.None);
        }
        #endregion

        #region Misc. Nodes
        public static Node CreateKillerNode() {
            var killerNode = new Node() {
                NPC = NPC.NPCList.FirstOrDefault(npc => npc.IsKiller),
                IsKiller = true
            };

            return killerNode;
        }
        #endregion

        #region Graph Rigging
        private static void SetupReference(Graph g, Node n) {
            n.IsVisited = false;
            if (g.ReferenceLookup.ContainsKey(n.TargetNode))
                g.ReferenceLookup[n.TargetNode].Add(n);
            else
                g.ReferenceLookup.Add(n.TargetNode, new List<Node>() { n });
        }

        private static void SetupLieReference(Graph truthGraph, Graph lieGraph, Node n) {
            List<Node> refNodes = new List<Node>();
            // Inverts truth statements targeted at NPC hooked to current node.
            if (truthGraph.ReferenceLookup.TryGetValue(n, out refNodes))
                foreach (Node refN in refNodes) {
                    var newRefClueTemplate = ClueConverter.GetClueTemplate(ClueIdentifier.Accusatory);
                    refN.NodeClue = new Clue(newRefClueTemplate, refN.TargetNode.NPC, ClueIdentifier.Accusatory, NPCPart.NPCPartType.None);
                }
            // Inverts deceptive statements targeted at NPC hooked to current node.
            if (lieGraph.ReferenceLookup.TryGetValue(n, out refNodes))
                foreach (Node refN in refNodes)
                    if (!refN.IsDescriptive) {
                        var newRefClueTemplate = ClueConverter.GetClueTemplate(ClueIdentifier.Informational);
                        refN.NodeClue = new Clue(newRefClueTemplate, refN.TargetNode.NPC, ClueIdentifier.Informational, NPCPart.NPCPartType.None);
                    }
        }
        #endregion
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
            g.Nodes.Add(CreateKillerNode());

            // Adds descriptive nodes and hooks them to the kiler node.
            g.Nodes.AddRange(CreateDescriptiveNodes(g.Nodes[0], descriptiveCount));

            // Creating support Nodes
            while (g.Nodes.Count < nodeCount) {
                // Find random NPC which does not currently have a truth statement attached.
                var hookNPC = NPC.NPCList.Where(npc => !g.Nodes.Any(node => node.NPC.Equals(npc))).FirstOrDefault();

                // Find TargetNode without self-referencing.
                Node newTarget = g.Nodes.Where(node => node.NPC != hookNPC).ToList()[r.Next(g.Nodes.Count)];
                //while (newTarget == newTarget.TargetNode) {
                if (newTarget == newTarget.TargetNode) {
                    //newTarget = g.Nodes[r.Next(g.Nodes.Count)];
                    throw new Exception("NewTarget references itself.");
                }

                var restNode = CreateSupportNode(newTarget, hookNPC);
                g.Nodes.Add(restNode);
            }

            // Set up killerNode.Clue
            var restNodes = g.Nodes.Where(n => !n.IsDescriptive && !n.IsKiller).ToList();
            // If there's not any other NPCs than descriptive & killer, pick descriptive.
            if (restNodes.Count < 1)
                restNodes = g.Nodes.Where(n => !n.IsKiller).ToList();
            // If there's not any other NPCs than killer, return g with killer referencing self.
            if (restNodes.Count < 1) {
                g.Nodes[0].TargetNode = g.Nodes[0];
                SetupSupportNode(g.Nodes[0], g.Nodes[0].TargetNode);
                return g;
            } else { 
                g.Nodes[0].TargetNode = restNodes[r.Next(restNodes.Count)];
                // Sets up the KillerNode with a support clue.
                SetupSupportNode(g.Nodes[0], g.Nodes[0].TargetNode);
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
