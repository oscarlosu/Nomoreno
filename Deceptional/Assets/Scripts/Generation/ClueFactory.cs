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
        public List<Node> CreateDescriptiveNodes(NPC target, List<NPC> possibleHooks, int count) {
            List<Node> returnList = new List<Node>();
            // The amount of possibilities for differnet parts.
            var partCount = 3;

            if (count == 1) {
                // Select one body part
                int partIdx = PlayerController.Instance.Rng.Next(partCount);
                var hookNPC = possibleHooks.FirstOrDefault();
                returnList.Add(CreateDescriptiveNode(target, hookNPC, partIdx));
                possibleHooks.Remove(hookNPC);
            } else if (count == 2) {
                int excludeIdx = PlayerController.Instance.Rng.Next(partCount);
                for (int i = 0; i < count + 1; i++) {
                    // Skip one body part
                    if (i == excludeIdx) continue;
                    var hookNPC = possibleHooks.FirstOrDefault();
                    returnList.Add(CreateDescriptiveNode(target, hookNPC, i));
                    possibleHooks.Remove(hookNPC);
                }
            } else {
                for (int i = 0; i < count; i++) {
                    var hookNPC = possibleHooks.FirstOrDefault();
                    returnList.Add(CreateDescriptiveNode(target, hookNPC, i % partCount));
                    possibleHooks.Remove(hookNPC);
                }
            }

            return returnList;
        }
        #endregion

        #region Support Nodes (Murder Location, People Location, Pointer, Accusation)
        #region Base Node
        /// <summary>
        /// Creates a node, hooked to a NPC, and attaches a support clue to it, referencing the target Node.
        /// </summary>
        /// <param name="target">The Node containing target information.</param>
        /// <param name="hookNPC">The NPC related to this Node.</param>
        /// <returns>A new support node, connected to the hookNPC.</returns>
        public Node CreateSupportNode(NPC target, NPC hookNPC) { return CreateSupportNode(target, hookNPC, ClueIdentifier.Accusatory); }
        public Node CreateSupportNode(NPC target, NPC hookNPC, ClueIdentifier identifier) {
            var supportNode = new Node() { NPC = hookNPC };
            SetupSupportNode(supportNode, target, identifier);

            return supportNode;
        }

        public Node CreateMultiSupportNode(List<NPC> targets, NPC hookNPC, string location) { return CreateMultiSupportNode(targets, hookNPC, location, ClueIdentifier.PeopleLocation); }
        public Node CreateMultiSupportNode(List<NPC> targets, NPC hookNPC, string location, ClueIdentifier identifier) {
            var supportNode = new Node() { NPC = hookNPC };
            SetupMultiSupportNode(supportNode, targets, location, identifier);

            return supportNode;
        }
        #endregion

        public Node CreateKillerNode() {
            var killerNode = new Node() {
                NPC = NPC.NPCList.FirstOrDefault(npc => npc.IsKiller),
                IsKiller = true
            };

            return killerNode;
        }

        #region Derived Nodes
        public Node CreatePointerNode(NPC target, NPC hookNPC) {
            var pointerNode = new Node() { NPC = hookNPC };
            SetupSupportNode(pointerNode, target, ClueIdentifier.Pointer);

            return pointerNode;
        }

        public Node CreatePeopleLocationNode(List<NPC> targets, NPC hookNPC, string location) {
            return CreateMultiSupportNode(targets, hookNPC, location); // Is implicitly PeopleLocation.
        }

        // Doesn't actually "need" the target for the statement to make sense.
        // Target is included to avoid exceptions during graph generation.
        public Node CreateMurderLocationNode(NPC target, NPC hookNPC, string location) {
            var murderNode = new Node() { NPC = hookNPC };
            SetupMultiSupportNode(murderNode, new List<NPC>() { target }, location, ClueIdentifier.MurderLocation);

            return murderNode;
        }

        public Node CreateAccusationNode(NPC target, NPC hookNPC) {
            var accusationNode = new Node() { NPC = hookNPC };
            SetupSupportNode(accusationNode, target, ClueIdentifier.Accusatory);

            return accusationNode;
        }
        #endregion

        #region Node Setup
        /// <summary>
        /// Attaches a support clue to a specified Node, referencing the target Node.
        /// </summary>
        /// <param name="baseNode">The Node in need of a clue.</param>
        /// <param name="targetNode">The Node containing target information.</param>
        /// <param name="identifier">The identifier that determines the nature of the statement.</param>
        public void SetupSupportNode(Node baseNode, NPC target) { SetupSupportNode(baseNode, target, ClueIdentifier.Accusatory); }
        public void SetupSupportNode(Node baseNode, NPC target, ClueIdentifier identifier) {
            baseNode.TargetNodes = new List<NPC>() { target };
            var template = ClueConverter.GetClueTemplate(identifier);
            baseNode.NodeClue = new Clue(template, baseNode.TargetNodes, identifier, NPCPart.NPCPartType.None);
        }

        public void SetupMultiSupportNode(Node baseNode, List<NPC> targets, string location) { SetupMultiSupportNode(baseNode, targets, location, ClueIdentifier.PeopleLocation); }
        public void SetupMultiSupportNode(Node baseNode, List<NPC> targets, string location, ClueIdentifier identifier) {
            baseNode.TargetNodes = PickRandomTargets(targets, 4);
            var template = "";
            if (identifier == ClueIdentifier.PeopleLocation) {
                if (targets.Contains(baseNode.NPC)) {
                    template = ClueConverter.GetLocationSubTemplate(true);
                    targets.Remove(baseNode.NPC);
                } else {
                    template = ClueConverter.GetLocationSubTemplate(false);
                }
            } else {
                template = ClueConverter.GetClueTemplate(identifier);
            }
            //var template = ClueConverter.GetClueTemplate(identifier);
            baseNode.NodeClue = new Clue(template, baseNode.TargetNodes, identifier, NPCPart.NPCPartType.None) { Location = location };
        }
        #endregion
        #endregion

        // Should only be used for accusations, as these need to be certain that their target doesn't change status.
        public Node CreateEmptyNode(NPC hookNPC) {
            Node emptyNode = new Node() {
                NPC = hookNPC,
                TargetNodes = new List<NPC>()
            };

            // Assumes that above comment is adhered to.
            var template = ClueConverter.GetClueTemplate(ClueIdentifier.Accusatory);
            emptyNode.NodeClue = new Clue(template, emptyNode.TargetNodes, ClueIdentifier.Accusatory, NPCPart.NPCPartType.None);
            return emptyNode;
        }

        // DOES NOT CREATE DESCRIPTIVE LIES! THESE SHOULD BE CREATED EXPLICITLY
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
                    possibleTargets = NPC.NPCList.Where(npc => !pointerTargets.Contains(npc));
                    return CreatePointerNode(possibleTargets.ElementAt(PlayerController.Instance.Rng.Next(possibleTargets.Count())), hookNPC);
                case 3: // Accusation
                    return CreateEmptyNode(hookNPC);
                default: throw new Exception("lieIdx tried to access a number larger than 3.");
            }
        }

        public static List<NPC> PickRandomTargets(List<NPC> baseList, int count) {
            if (count >= baseList.Count) return baseList;

            var pickedIdx = new List<int>();
            var pickedElements = new List<NPC>();
            for (int i = 0; i < count; i++) {
                var nextIdx = PlayerController.Instance.Rng.Next(baseList.Count);
                while (pickedIdx.Contains(nextIdx % baseList.Count)) { nextIdx = (nextIdx + 1) % baseList.Count; }
                pickedIdx.Add(nextIdx);
                pickedElements.Add(baseList[nextIdx]);
            }

            return pickedElements;
        }
    }
}
