using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public class ClueFactory {

        #region Singleton Fields
        private static ClueFactory _instance;
        public static ClueFactory Instance {
            get {
                if (_instance == null)
                    _instance = new ClueFactory();

                return _instance;
            }
            private set { _instance = value; }
        }
        #endregion

        #region Node Creation
        #region Descriptive Nodes
        /// <summary>
        /// Creates a Node for the truth graph, targeting the Node target and hooks the new Node to hookNPC
        /// </summary>
        /// <param name="target">Node which the new node should reference.</param>
        /// <param name="hookNPC">NPC which is connected to the new Node.</param>
        /// <param name="partIdx">Part of NPC being described (0: Head, 1: Torso, 2: Legs).</param>
        /// <returns>New Descriptive clue Node.</returns>
        public Node CreateDescriptiveNode(NPC target, NPC hookNPC, int partIdx) {
            var descriptiveNode = new Node() {
                IsDescriptive = true,
                TargetNodes = new List<NPC>() { target }
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
            descriptiveNode.NodeClue = new Clue(template, descriptiveNode.TargetNodes, ClueIdentifier.Descriptive, cluePartType);

            return descriptiveNode;
        }

        public Node CreateDescriptiveNode(NPC target, NPC hookNPC) {
            return CreateDescriptiveNode(target, hookNPC, PlayerController.Instance.Rng.Next(3));
        }

        /// <summary>
        /// Creates a list of Descriptive nodes with equal distribution.
        /// </summary>
        /// <param name="targetNode">The target of description.</param>
        /// <param name="count">The amount of nodes returned.</param>
        /// <returns></returns>
        public List<Node> CreateDescriptiveNodes(NPC target, int count) {
            List<Node> returnList = new List<Node>();
            var nonKillers = NPC.NPCList.Where(npc => !npc.IsKiller).ToList();

            if (count == 1) {
                // Select one body part
                int partIdx = PlayerController.Instance.Rng.Next(3);
                var hookNPC = nonKillers.FirstOrDefault();
                returnList.Add(CreateDescriptiveNode(target, hookNPC, partIdx));
                nonKillers.Remove(hookNPC);
            } else if (count == 2) {
                int excludeIdx = PlayerController.Instance.Rng.Next(3);
                for (int i = 0; i < 3; i++) {
                    // Skip one body part
                    if (i == excludeIdx) continue;
                    var hookNPC = nonKillers.FirstOrDefault();
                    returnList.Add(CreateDescriptiveNode(target, hookNPC, i));
                    nonKillers.Remove(hookNPC);
                }
            } else {
                for (int i = 0; i < count; i++) {
                    var hookNPC = nonKillers.FirstOrDefault();
                    returnList.Add(CreateDescriptiveNode(target, hookNPC, i % 3));
                    nonKillers.Remove(hookNPC);
                }
            }

            return returnList;
        }
        #endregion

        #region Support Nodes (Murder Location, People Location, Pointer, Accusation)
        /// <summary>
        /// Creates a node, hooked to a NPC, and attaches a support clue to it, referencing the target Node.
        /// </summary>
        /// <param name="target">The Node containing target information.</param>
        /// <param name="hookNPC">The NPC related to this Node.</param>
        /// <returns>A new support node, connected to the hookNPC.</returns>
        public Node CreateSupportNode(NPC target, NPC hookNPC) { return CreateSupportNode(target, hookNPC, ClueIdentifier.Accusatory); }
        public Node CreateSupportNode(NPC target, NPC hookNPC, ClueIdentifier identifier) {
            var supportNode = new Node() {
                NPC = hookNPC
            };

            SetupSupportNode(supportNode, new List<NPC>() { target }, identifier);

            return supportNode;
        }

        public Node CreatePeopleLocationNode(List<NPC> targets, NPC hookNPC, string location) {
            var peopleLocNode = new Node() {
                NPC = hookNPC,
                TargetNodes = targets
            };

            var template = ClueConverter.GetClueTemplate(ClueIdentifier.PeopleLocation);
            peopleLocNode.NodeClue = new Clue(template, targets, ClueIdentifier.PeopleLocation, NPCPart.NPCPartType.None) { Location = location };

            return peopleLocNode;
        }

        public Node CreatePointerNode(NPC target, NPC hookNPC) {
            var pointerNode = new Node() {
                NPC = hookNPC
            };

            //SetupSupportNode(pointerNode, target, ClueIdentifier.Pointer);
            pointerNode.TargetNodes = new List<NPC>() { target };
            var template = ClueConverter.GetClueTemplate(ClueIdentifier.Pointer);
            pointerNode.NodeClue = new Clue(template, target, ClueIdentifier.Pointer, NPCPart.NPCPartType.None);

            return pointerNode;
        }

        // Doesn't actually "need" the target for the statement to make sense.
        // Target is included to avoid exceptions during graph generation.
        public Node CreateMurderLocationNode(NPC target, NPC hookNPC, string location) {
            var murderNode = new Node() {
                NPC = hookNPC,
                TargetNodes = new List<NPC>() { target }
            };

            //SetupSupportNode(murderNode, null, ClueIdentifier.MurderLocation);
            var template = ClueConverter.GetClueTemplate(ClueIdentifier.MurderLocation);
            murderNode.NodeClue = new Clue(template, new List<NPC>(), ClueIdentifier.MurderLocation, NPCPart.NPCPartType.None) { Location = location };

            return murderNode;
        }

        /// <summary>
        /// Attaches a support clue to a specified Node, referencing the target Node.
        /// </summary>
        /// <param name="baseNode">The Node in need of a clue.</param>
        /// <param name="targetNode">The Node containing target information.</param>
        /// <param name="identifier">The identifier that determines the nature of the statement.</param>
        public void SetupSupportNode(Node baseNode, List<NPC> targets) { SetupSupportNode(baseNode, targets, ClueIdentifier.PeopleLocation); }
        public void SetupSupportNode(Node baseNode, List<NPC> targets, ClueIdentifier identifier) {
            baseNode.TargetNodes = new List<NPC>();
            baseNode.TargetNodes.AddRange(targets);
            var template = ClueConverter.GetClueTemplate(identifier);
            baseNode.NodeClue = new Clue(template, baseNode.TargetNodes, identifier, NPCPart.NPCPartType.None);
        }
        #endregion

        #region Misc. Nodes
        public Node CreateKillerNode() {
            var killerNode = new Node() {
                NPC = NPC.NPCList.FirstOrDefault(npc => npc.IsKiller),
                IsKiller = true
            };

            return killerNode;
        }

        /* DOES NOT CREATE DESCRIPTIVE LIES! THESE SHOULD BE CREATED EXPLICITLY*/
        public Node CreateRandomLie(NPC hookNPC, CaseHandler caseHandler, Graph truthGraph) {
            var lieIdx = PlayerController.Instance.Rng.Next(4);
            IEnumerable<NPC> possibleTargets;
            string lieLocation = "";
            switch (lieIdx) {
                case 0: // Murder Location
                    lieLocation = caseHandler.NPCLocations.Keys.Where(s => s != caseHandler.MurderLocation).ElementAt(PlayerController.Instance.Rng.Next(caseHandler.NPCLocations.Count - 1));
                    return CreateMurderLocationNode(truthGraph.Nodes[0].NPC, hookNPC, lieLocation);
                case 1: // People Location
                    var peopleLocation = caseHandler.NPCLocations.Keys.ElementAt(PlayerController.Instance.Rng.Next(caseHandler.NPCLocations.Count));
                    lieLocation = caseHandler.NPCLocations.Keys.Where(s => s != peopleLocation).ElementAt(PlayerController.Instance.Rng.Next(caseHandler.NPCLocations.Count - 1));
                    return CreatePeopleLocationNode(caseHandler.NPCLocations[peopleLocation], hookNPC, lieLocation);
                case 2: // Pointer
                    IEnumerable<NPC> pointerTargets = truthGraph.Nodes.Where(
                        node =>
                            node.NodeClue.Identifier == ClueIdentifier.Descriptive ||
                            node.NodeClue.Identifier == ClueIdentifier.MurderLocation ||
                            (node.NodeClue.Identifier == ClueIdentifier.PeopleLocation && node.NodeClue.Location == caseHandler.MurderLocation)
                        ).Select(node => node.NPC);
                    // Selects all possible targets, defined as NPCs which aren't part of the real pointer targets.
                    possibleTargets = NPC.NPCList.Where(npc => pointerTargets.Contains(npc));
                    return CreatePointerNode(possibleTargets.ElementAt(PlayerController.Instance.Rng.Next(possibleTargets.Count())), hookNPC);
                case 3: // Accusation
                    possibleTargets = truthGraph.Nodes.Select(node => node.NPC);
                    return CreateSupportNode(possibleTargets.ElementAt(PlayerController.Instance.Rng.Next(possibleTargets.Count())), hookNPC, ClueIdentifier.Accusatory);
                default: throw new Exception("lieIdx tried to access a number larger than 4.");
            }
        }
        #endregion
        #endregion
    }
}
