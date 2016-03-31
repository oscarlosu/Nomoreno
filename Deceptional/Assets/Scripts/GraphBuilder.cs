using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts {
    public static class GraphBuilder {
        private static Random r = PlayerController.UseFixedSeed ? new Random(PlayerController.GeneratorSeed) : new Random(DateTime.Now.Millisecond);
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
                liars.Add(n.NPC); // (REPLACING n.NPC.Conversation.IsTrue = false;)

                var possibleTargets = truthGraph.Nodes.Where(node => node.NPC != n.NPC).ToList();
                var lieTargetNode = possibleTargets[r.Next(possibleTargets.Count)];
                var lieIsDescriptive = !Convert.ToBoolean(r.Next(5));

                // Creates lie-related Clue object.
                Node n_lie;
                var lieColor = string.Empty;
                if (lieIsDescriptive) {
                    n_lie = CreateDescriptiveNode(lieTargetNode, n.NPC);
                } else {
                    var lieIdentifier = liars.Contains(n.TargetNode.NPC) ? ClueIdentifier.Accusatory : ClueIdentifier.Informational;
                    n_lie = CreateSupportNode(lieTargetNode, n.NPC, lieIdentifier);
                }

                lieGraph.Nodes.Add(n_lie);
                if (lieGraph.ReferenceLookup.ContainsKey(n_lie.TargetNode)) lieGraph.ReferenceLookup[n_lie.TargetNode].Add(n_lie);
                else lieGraph.ReferenceLookup.Add(n_lie.TargetNode, new List<Node>() { n_lie });

                // Inverts truth statements targeted at NPC hooked to current node.
                List<Node> refNodes = new List<Node>();
                if (truthGraph.ReferenceLookup.TryGetValue(n, out refNodes))
                    foreach (Node refN in refNodes) {
                        var newRefClue = ClueConverter.ConstructClue(true, n.NPC.Name, n.NPC.IsMale);
                        refN.NodeClue = new Clue(newRefClue, refN.TargetNode.NPC, ClueIdentifier.Accusatory, NPCPart.NPCPartType.None);
                    }
                // Inverts deceptive statements targeted at NPC hooked to current node.
                if (lieGraph.ReferenceLookup.TryGetValue(n, out refNodes))
                    foreach (Node refN in refNodes)
                        if (!refN.IsDescriptive) {
                            var newRefClue = ClueConverter.ConstructClue(false, n.NPC.Name, n.NPC.IsMale);
                            refN.NodeClue = new Clue(newRefClue, refN.TargetNode.NPC, ClueIdentifier.Informational, NPCPart.NPCPartType.None);
                        }
            }

            return lieGraph;
        }

        #region Node handling
        #region Descriptive Nodes
        /// <summary>
        /// Creates a Node for the truth graph, targeting the Node target and hooks the new Node to hookNPC
        /// </summary>
        /// <param name="target">Node which the new node should reference.</param>
        /// <param name="hookNPC">NPC which is connected to the new Node</param>
        /// <returns>New Descriptive clue Node.</returns>
        public static Node CreateDescriptiveNode(Node target, NPC hookNPC) {
            var descriptiveNode = new Node() {
                IsDescriptive = true,
                TargetNode = target
            };

            string statement = string.Empty;
            NPCPart.NPCPartType cluePartType;
            switch (r.Next(3)) {
                case 0:
                    statement = ClueConverter.ConstructClue(true, NPCDescriptionToString(descriptiveNode.TargetNode.NPC.Head.Description), "hat", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale);
                    cluePartType = NPCPart.NPCPartType.Hat;
                    break;
                case 1:
                    statement = ClueConverter.ConstructClue(true, NPCDescriptionToString(descriptiveNode.TargetNode.NPC.Torso.Description), "shirt", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale);
                    cluePartType = NPCPart.NPCPartType.Shirt;
                    break;
                case 2:
                    statement = ClueConverter.ConstructClue(true, NPCDescriptionToString(descriptiveNode.TargetNode.NPC.Legs.Description), "pants", descriptiveNode.TargetNode.NPC.Name, descriptiveNode.TargetNode.NPC.IsMale);
                    cluePartType = NPCPart.NPCPartType.Pants;
                    break;
                default: throw new Exception("Random generator tried to access non-existant clothing.");
            }

            descriptiveNode.NPC = hookNPC;
            descriptiveNode.NodeClue = new Clue(statement, descriptiveNode.TargetNode.NPC, ClueIdentifier.Descriptive, cluePartType);

            return descriptiveNode;
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
            var statement = ClueConverter.ConstructClue(identifier, baseNode.TargetNode.NPC.Name, baseNode.TargetNode.NPC.IsMale);
            baseNode.NodeClue = new Clue(statement, baseNode.TargetNode.NPC, identifier, NPCPart.NPCPartType.None);
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
            // Add Boolean Nodes..?
            Graph g = new Graph();

            // Adding Killer Node
            g.Nodes.Add(CreateKillerNode());

            // Adds descriptive nodes and hooks them to the kiler node.
            var nonKillers = NPC.NPCList.Where(npc => !npc.IsKiller).ToList();
            for (int i = 0; i < descriptiveCount; i++) {
                var hookNPC = nonKillers.FirstOrDefault();
                g.Nodes.Add(CreateDescriptiveNode(g.Nodes[0], hookNPC));
                nonKillers.Remove(hookNPC);
            }

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
            // If there's not any other NPCs than descriptive & killer.
            if (restNodes.Count < 1)
                restNodes = g.Nodes.Where(n => !n.IsKiller).ToList();
            // If there's not any other NPCs than killer.
            if (restNodes.Count < 1) {
                g.Nodes[0].TargetNode = g.Nodes[0];
                SetupSupportNode(g.Nodes[0], g.Nodes[0].TargetNode);
                return g;
            } else { 
                g.Nodes[0].TargetNode = restNodes[r.Next(restNodes.Count)];
                var targetNode = restNodes[r.Next(restNodes.Count)];
                // Sets up the KillerNode with a support clue.
                SetupSupportNode(g.Nodes[0], targetNode);
            }

            // Managing ReferenceLookup.
            foreach (Node n in g.Nodes) { 
                n.IsVisited = false;
                if (g.ReferenceLookup.ContainsKey(n.TargetNode))
                    g.ReferenceLookup[n.TargetNode].Add(n);
                else
                    g.ReferenceLookup.Add(n.TargetNode, new List<Node>() { n });
            }

            // Returning.
            return g;
        }

        private static string NPCDescriptionToString(NPCPart.NPCPartDescription description) {
            return Enum.GetName(typeof(NPCPart.NPCPartDescription), description);
        }
    }
}
