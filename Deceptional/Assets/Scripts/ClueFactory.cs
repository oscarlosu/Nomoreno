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
            }
            else if (count == 2) {
                int excludeIdx = PlayerController.Instance.Rng.Next(3);
                for (int i = 0; i < 3; i++) {
                    // Skip one body part
                    if (i == excludeIdx) continue;
                    var hookNPC = nonKillers.FirstOrDefault();
                    returnList.Add(CreateDescriptiveNode(targetNode, hookNPC, i));
                    nonKillers.Remove(hookNPC);
                }
            }
            else {
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
        public Node CreateSupportNode(Node target, NPC hookNPC) { return CreateSupportNode(target, hookNPC, ClueIdentifier.Informational); }
        public Node CreateSupportNode(Node target, NPC hookNPC, ClueIdentifier identifier) {
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
        public void SetupSupportNode(Node baseNode, Node targetNode) { SetupSupportNode(baseNode, targetNode, ClueIdentifier.Informational); }
        public void SetupSupportNode(Node baseNode, Node targetNode, ClueIdentifier identifier) {
            baseNode.TargetNode = targetNode;
            var template = ClueConverter.GetClueTemplate(identifier);
            baseNode.NodeClue = new Clue(template, baseNode.TargetNode.NPC, identifier, NPCPart.NPCPartType.None);
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
        #endregion
        #endregion
    }
}
