using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts {
    public static class GraphBuilder {
        private static CaseHandler caseHandler;

        private static void SetupReferences(Graph g, Node n) {
            foreach (NPC t in n.TargetNodes) {
                Node t_node = g.Nodes.FirstOrDefault(node => node.NPC == t);
                if (t_node == null) { UnityEngine.Debug.LogWarning("NPC has been set up without target."); return; }

                if (g.ReferenceLookup.ContainsKey(t_node)) {
                    g.ReferenceLookup[t_node].Add(n);
                } else {
                    g.ReferenceLookup.Add(t_node, new List<Node>() { n });
                }
            }
        }

        public static Graph BuildGraph(int descriptions, int locationClues, int murderLocationClues, int pointers) {
            Graph g = new Graph();
            bool hasKiller = false;
            bool hasDescriptions = false;
            bool hasKillerLocation = false;
            caseHandler = new CaseHandler(NPC.NPCList);

            Node killerNode = new Node();
            List<Node> pointerTargets = new List<Node>();
            List<NPC> remainingNPCs = new List<NPC>(NPC.NPCList);

            /* KILLER NODE */
            if (!hasKiller) {
                killerNode = ClueFactory.Instance.CreateKillerNode();
                g.Nodes.Add(killerNode);
                // This sets up the murderer with a People Location clue to the crime scene.
                var targetLoc = caseHandler.NPCLocations.Keys.ElementAt(PlayerController.Instance.Rng.Next(caseHandler.NPCLocations.Count));
                ClueFactory.Instance.SetupMultiSupportNode(killerNode, caseHandler.NPCLocations[targetLoc], targetLoc);
                hasKiller = true;
                remainingNPCs.Remove(killerNode.NPC);
                SetupReferences(g, killerNode);
            }

            while (murderLocationClues-- > 0) {
                var newNode = ClueFactory.Instance.CreateMurderLocationNode(
                    killerNode.NPC,
                    remainingNPCs[PlayerController.Instance.Rng.Next(remainingNPCs.Count)],
                    caseHandler.MurderLocation);

                g.Nodes.Add(newNode);
                pointerTargets.Add(newNode);
                remainingNPCs.Remove(newNode.NPC);
                // Not added to ReferenceLookup due to the clue not being a reference to its target.
            }

            /* DESCRIPTIVE NODES */
            if (!hasDescriptions) {
                var descriptiveNodes = ClueFactory.Instance.CreateDescriptiveNodes(killerNode.NPC, remainingNPCs, descriptions);
                pointerTargets.AddRange(descriptiveNodes);
                g.Nodes.AddRange(descriptiveNodes);
                hasDescriptions = true;
                remainingNPCs.RemoveAll(npc => pointerTargets.Select(node => node.NPC).Contains(npc));
                foreach (Node n in descriptiveNodes) SetupReferences(g, n);
            }

            /* PEOPLE LOCATION NODES */
            while (locationClues-- > 0) {
                var newNode = new Node();
                // Should probably be a counter instead of a boolean.
                if (!hasKillerLocation) {
                    var murderScenePois = caseHandler.NPCLocations[caseHandler.MurderLocation];

                    newNode = ClueFactory.Instance.CreatePeopleLocationNode(
                        PickRandomTargets(murderScenePois, 4),
                        murderScenePois.Where(npc => !npc.IsKiller).ElementAt(PlayerController.Instance.Rng.Next(murderScenePois.Count-1)),
                        caseHandler.MurderLocation);
                    hasKillerLocation = true;
                    pointerTargets.Add(newNode);
                } else {
                    var locationString = caseHandler.NPCLocations.Keys.ElementAt(PlayerController.Instance.Rng.Next(caseHandler.NPCLocations.Count));
                    var locationPois = caseHandler.NPCLocations[locationString];
                    // Can never pick killer, since killer is not part of remainingNPCs at this time.
                    var availablePois = locationPois.Where(npc => remainingNPCs.Contains(npc));
                    // If no NPCs are available for the location, spread onto other locations.
                    if (availablePois.Count() == 0) { availablePois = remainingNPCs; }

                    newNode = ClueFactory.Instance.CreatePeopleLocationNode(
                        PickRandomTargets(locationPois, 4),
                        availablePois.ElementAt(PlayerController.Instance.Rng.Next(availablePois.Count())),
                        locationString);
                }

                g.Nodes.Add(newNode);
                remainingNPCs.Remove(newNode.NPC);
                SetupReferences(g, newNode);
            }

            /* POINTER NODES */
            while (pointers-- > 0) {
                var pointerTarget = pointerTargets.ElementAt(PlayerController.Instance.Rng.Next(pointerTargets.Count));
                var newNode = ClueFactory.Instance.CreatePointerNode(pointerTarget.NPC, remainingNPCs[PlayerController.Instance.Rng.Next(remainingNPCs.Count)]);

                g.Nodes.Add(newNode);
                remainingNPCs.Remove(newNode.NPC);
                SetupReferences(g, newNode);
            }

            return g;
        }

        public static Graph BuildLieGraph(Graph truthGraph, int descriptiveLies, int miscLies, int hidingDescriptive) {
            // Create neccessary graph and lists.
            var lieGraph = new Graph();
            List<NPC> nonKillerNPCs = NPC.NPCList.Where(npc => !npc.IsKiller).ToList();

            List<Node> descriptiveNodes = truthGraph.Nodes.Where(node => node.IsDescriptive).ToList();
            // Create non-similar lists.
            // This is to avoid lies accidently aligning with the truth.
            var killer = NPC.NPCList.First(npc => npc.IsKiller);
            var nonSimilarHats = nonKillerNPCs.Where(npc => npc.Hat.Description != killer.Hat.Description);
            var nonSimilarTorsos = nonKillerNPCs.Where(npc => npc.Torso.Description != killer.Torso.Description);
            var nonSimilarPants = nonKillerNPCs.Where(npc => npc.Legs.Description != killer.Legs.Description);
            Node lieNode = new Node();

            // Setup hiding descriptive nodes.
            while (hidingDescriptive-- > 0 && descriptiveNodes.Count > 0) {
                Node newLie = ClueFactory.Instance.CreateRandomLie(descriptiveNodes[0].NPC, caseHandler, truthGraph);

                nonKillerNPCs.Remove(descriptiveNodes[0].NPC);
                descriptiveNodes.RemoveAt(0);
                lieGraph.Nodes.Add(newLie);
                SetupReferences(lieGraph, newLie);
            }
            
            var possibleTargets = new List<NPC>();
            // Setup descriptive liars
            while (descriptiveLies-- > 0) {
                if (nonKillerNPCs.Count == 0) { throw new Exception("No remaining NPCs. Less liars required."); }

                int partIdx = PlayerController.Instance.Rng.Next(3);
                switch (partIdx) {
                    case 0: possibleTargets = nonKillerNPCs.Where(npc1 => nonSimilarHats.Any(npc2 => npc1 == npc2)).ToList(); break;
                    case 1: possibleTargets = nonKillerNPCs.Where(npc1 => nonSimilarTorsos.Any(npc2 => npc1 == npc2)).ToList(); break;
                    case 2: possibleTargets = nonKillerNPCs.Where(npc1 => nonSimilarPants.Any(npc2 => npc1 == npc2)).ToList(); break;
                    default: throw new Exception("Random generator tried to access non-existant clothing.");
                }

                NPC lieTarget = possibleTargets[PlayerController.Instance.Rng.Next(possibleTargets.Count)];
                NPC newLiar = nonKillerNPCs[PlayerController.Instance.Rng.Next(nonKillerNPCs.Count)];
                lieNode = ClueFactory.Instance.CreateDescriptiveNode(lieTarget, newLiar, partIdx);

                lieGraph.Nodes.Add(lieNode);
                nonKillerNPCs.Remove(newLiar);
                SetupReferences(lieGraph, lieNode);
            }

            // Setup miscellanous lies.
            while (miscLies-- > 0 && nonKillerNPCs.Count > 0) {
                Node newLie = ClueFactory.Instance.CreateRandomLie(nonKillerNPCs[PlayerController.Instance.Rng.Next(nonKillerNPCs.Count)], caseHandler, truthGraph);

                nonKillerNPCs.Remove(newLie.NPC);
                lieGraph.Nodes.Add(newLie);
                SetupReferences(lieGraph, newLie);
            }

            if (miscLies > 0) { UnityEngine.Debug.LogWarning(string.Format("RemainingNPCs has been exhausted before all lies distributed! {0} lies remaining.", miscLies)); }

            return lieGraph;
        }

        public static void AttachAccusationsToGraph(Graph truth, Graph lies) {
            var remainingNPCs = NPC.NPCList.Where(npc => !truth.Nodes.Select(node => node.NPC).Contains(npc));
            var liars = lies.Nodes.Select(node => node.NPC);

            List<NPC> possibleTargets;

            // Setup lying accusations. This is done after node creation to ensure logical coherency.
            possibleTargets = NPC.NPCList.Except(lies.Nodes.Select(node => node.NPC)).ToList();
            var lyingAccusers = lies.Nodes.Where(n => n.NodeClue.Identifier == ClueIdentifier.Accusatory);
            foreach (Node n in lyingAccusers) {
                var target = possibleTargets[PlayerController.Instance.Rng.Next(possibleTargets.Count)];
                n.TargetNodes.Add(target);
                n.NodeClue.Targets.Add(target);
            }

            // Setup true accusations. This is done after lying accusations, so that all liars are on equal footing.
            foreach (NPC npc in remainingNPCs) {
                possibleTargets = liars.Where(n => n != npc).ToList();
                Node accusation = ClueFactory.Instance.CreateAccusationNode(possibleTargets[PlayerController.Instance.Rng.Next(possibleTargets.Count)], npc);
                truth.Nodes.Add(accusation);
            }
        }

        public static List<NPC> PickRandomTargets(List<NPC> baseList, int count) {
            if (count >= baseList.Count) return baseList;

            var pickedIdx = new List<int>();
            var pickedElements = new List<NPC>();
            for (int i = 0; i < count; i++) {
                var nextIdx = PlayerController.Instance.Rng.Next(baseList.Count);
                while (pickedIdx.Contains(nextIdx % baseList.Count)) { nextIdx = (nextIdx + 1) % baseList.Count; }
                pickedElements.Add(baseList[nextIdx]);
            }

            return pickedElements;
        }
    }
}
