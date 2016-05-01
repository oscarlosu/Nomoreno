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
        public Node CreateDescriptiveNode(Node target, NPC hookNPC, int partIdx) {
            var descriptiveNode = new Node() {
                IsDescriptive = true,
                TargetNodes = new List<Node>() { target }
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
            descriptiveNode.NodeClue = new Clue(template, descriptiveNode.TargetNodes.First().NPC, ClueIdentifier.Descriptive, cluePartType);

            return descriptiveNode;
        }

        public Node CreateDescriptiveNode(Node target, NPC hookNPC) {
            return CreateDescriptiveNode(target, hookNPC, PlayerController.Instance.Rng.Next(3));
        }

        /// <summary>
        /// Creates a list of Descriptive nodes with equal distribution.
        /// </summary>
        /// <param name="targetNode">The target of description.</param>
        /// <param name="count">The amount of nodes returned.</param>
        /// <returns></returns>
        public List<Node> CreateDescriptiveNodes(Node targetNode, int count) {
            List<Node> returnList = new List<Node>();
            var nonKillers = NPC.NPCList.Where(npc => !npc.IsKiller).ToList();

            if (count == 1) {
                // Select one body part
                int partIdx = PlayerController.Instance.Rng.Next(3);
                var hookNPC = nonKillers.FirstOrDefault();
                returnList.Add(CreateDescriptiveNode(targetNode, hookNPC, partIdx));
                nonKillers.Remove(hookNPC);
            } else if (count == 2) {
                int excludeIdx = PlayerController.Instance.Rng.Next(3);
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

        #region Support Nodes (Murder Location, People Location, Pointer, Accusation)
        /// <summary>
        /// Creates a node, hooked to a NPC, and attaches a support clue to it, referencing the target Node.
        /// </summary>
        /// <param name="target">The Node containing target information.</param>
        /// <param name="hookNPC">The NPC related to this Node.</param>
        /// <returns>A new support node, connected to the hookNPC.</returns>
        public Node CreateSupportNode(Node target, NPC hookNPC) { return CreateSupportNode(target, hookNPC, ClueIdentifier.PeopleLocation); }
        public Node CreateSupportNode(Node target, NPC hookNPC, ClueIdentifier identifier) {
            var supportNode = new Node() {
                NPC = hookNPC
            };

            SetupSupportNode(supportNode, target, identifier);

            return supportNode;
        }

        public Node CreatePeopleLocationNode(List<Node> targets, NPC hookNPC, string location) {
            var peopleLocNode = new Node() {
                NPC = hookNPC,
                TargetNodes = targets
            };

            var template = ClueConverter.GetClueTemplate(ClueIdentifier.PeopleLocation);
            peopleLocNode.NodeClue = new Clue(template, targets.Select(node => node.NPC).ToList(), ClueIdentifier.PeopleLocation, NPCPart.NPCPartType.None) { Location = location };

            return peopleLocNode;
        }

        public Node CreatePointerNode(Node target, NPC hookNPC) {
            var pointerNode = new Node() {
                NPC = hookNPC
            };

            //SetupSupportNode(pointerNode, target, ClueIdentifier.Pointer);
            pointerNode.TargetNodes = new List<Node>() { target };
            var template = ClueConverter.GetClueTemplate(ClueIdentifier.Pointer);
            pointerNode.NodeClue = new Clue(template, target.NPC, ClueIdentifier.Pointer, NPCPart.NPCPartType.None);

            return pointerNode;
        }

        // Doesn't actually "need" the target for the statement to make sense.
        // Target is included to avoid exceptions during graph generation.
        public Node CreateMurderLocationNode(Node target, NPC hookNPC, string location) {
            var murderNode = new Node() {
                NPC = hookNPC,
                TargetNodes = new List<Node>() { target }
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
        public void SetupSupportNode(Node baseNode, Node targetNode) { SetupSupportNode(baseNode, targetNode, ClueIdentifier.PeopleLocation); }
        public void SetupSupportNode(Node baseNode, Node targetNode, ClueIdentifier identifier) {
            baseNode.TargetNodes = new List<Node>() { targetNode };
            var template = ClueConverter.GetClueTemplate(identifier);
            baseNode.NodeClue = new Clue(template, baseNode.TargetNodes.First().NPC, identifier, NPCPart.NPCPartType.None);
        }
        #endregion

        #region Murderer Nodes
        public Node CreateKillerNode() {
            var killerNode = new Node() {
                NPC = NPC.NPCList.FirstOrDefault(npc => npc.IsKiller),
                IsKiller = true
            };

            return killerNode;
        }
        #endregion
        #endregion
    }
}
