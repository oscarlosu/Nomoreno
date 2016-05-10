using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts {
    public static class GraphBuilder {
        private static CaseHandler caseHandler;

        //public static Graph BuildLieGraph(int descriptiveLies, int miscLies, int hidingDescriptive, Graph truthGraph) {
        //    // Calculating amount of liars.
        //    int liarCount = descriptiveLies + hidingDescriptive + miscLies;
        //    List<NPC> liars = new List<NPC>();

        //    // Create neccessary graph and lists.
        //    var lieGraph = new Graph();
        //    var nonKillerNodes = truthGraph.Nodes.Where(node => !node.IsKiller).ToList();
        //    var nonKillerNPCs = NPC.NPCList.Where(npc => !npc.IsKiller);

        //    // Create descriptive lists.
        //    // This is to make a suitable amount of Descriptive NPCs lie.
        //    var descriptiveNodes = nonKillerNodes.Where(node => node.IsDescriptive).ToList();
        //    var descriptiveNPCs = nonKillerNPCs.Where(npc => descriptiveNodes.Any(node => node.NPC == npc)).ToList();
        //    // If there is very few Descriptive NPCs, they will all be in hiding.
        //    if (descriptiveNPCs.Count < hidingDescriptive) {
        //        hidingDescriptive = descriptiveNPCs.Count;
        //        liarCount = descriptiveLies + hidingDescriptive + miscLies;
        //    }

        //    // Create non-similar lists.
        //    // This is to avoid lies accidently aligning with the truth.
        //    var killer = NPC.NPCList.First(npc => npc.IsKiller);
        //    var nonSimilarHats = nonKillerNPCs.Where(npc => npc.Head.Description != killer.Head.Description);
        //    var nonSimilarTorsos = nonKillerNPCs.Where(npc => npc.Torso.Description != killer.Torso.Description);
        //    var nonSimilarPants = nonKillerNPCs.Where(npc => npc.Legs.Description != killer.Legs.Description);

        //    while (--liarCount >= 0) {
        //        // Find liar node and remove it from nonKillers.
        //        Node n;
        //        if (hidingDescriptive > 0) {
        //            n = descriptiveNodes[PlayerController.Instance.Rng.Next(descriptiveNodes.Count)];
        //            descriptiveNodes.Remove(n);
        //            hidingDescriptive--;
        //        } else {
        //            n = nonKillerNodes[PlayerController.Instance.Rng.Next(nonKillerNodes.Count)];
        //        }
        //        // Remove the node from nonKillerNodes and add the NPC to liars.
        //        nonKillerNodes.Remove(n);
        //        liars.Add(n.NPC);

        //        if (nonKillerNodes.Count == 0) { throw new Exception("NonKillers has been exhausted. Less liars required."); }

        //        List<Node> possibleTargets;
        //        // Creates lie-related Clue object.
        //        Node n_lie;
        //        // Rolls a dice to determine whether new lie should be descriptive.
        //        if (PlayerController.Instance.Rng.Next(nonKillerNPCs.Count()) < descriptiveNPCs.Count) {
        //            int partIdx = PlayerController.Instance.Rng.Next(3);
        //            switch (partIdx) {
        //                case 0: possibleTargets = nonKillerNodes.Where(node => nonSimilarHats.Any(npc => npc == node.NPC)).ToList(); break;
        //                case 1: possibleTargets = nonKillerNodes.Where(node => nonSimilarTorsos.Any(npc => npc == node.NPC)).ToList(); break;
        //                case 2: possibleTargets = nonKillerNodes.Where(node => nonSimilarPants.Any(npc => npc == node.NPC)).ToList(); break;
        //                default: throw new Exception("Random generator tried to access non-existant clothing.");
        //            }

        //            Node lieTargetNode = possibleTargets[PlayerController.Instance.Rng.Next(possibleTargets.Count)];
        //            n_lie = ClueFactory.Instance.CreateDescriptiveNode(lieTargetNode, n.NPC, partIdx);
        //        } else {
        //            possibleTargets = truthGraph.Nodes.Where(node => node.NPC != n.NPC).ToList();
        //            Node lieTargetNode = possibleTargets[PlayerController.Instance.Rng.Next(possibleTargets.Count)];

        //            // Current liar cannot accuse any other liar of being a liar, but all other clues are fine.
        //            //var lieIdentifier = liars.Contains(n.TargetNodes.First().NPC) ? ClueIdentifier.Accusatory : ClueIdentifier.Informational;
        //            //ClueIdentifier lieIdentifier;
        //            // Roll max is 5 if a single target is the liar, else it is 4 to avoid accusations against liars.
        //            var roll =
        //                n.NodeClue.Identifier != ClueIdentifier.PeopleLocation && !liars.Contains(n.TargetNodes.First().NPC) ?
        //                PlayerController.Instance.Rng.Next(4) :
        //                PlayerController.Instance.Rng.Next(3);
        //            switch (roll) {
        //                case 0:
        //                    n_lie = ClueFactory.Instance.CreateMurderLocationNode(
        //                        lieTargetNode, 
        //                        n.NPC,
        //                        caseHandler.NPCLocations.Keys.ElementAt(PlayerController.Instance.Rng.Next(caseHandler.NPCLocations.Keys.Count))
        //                    );
        //                    break;
        //                case 1:
        //                    n_lie = ClueFactory.Instance.CreatePeopleLocationNode(
        //                        new List<Node> { lieTargetNode }, 
        //                        n.NPC,
        //                        caseHandler.NPCLocations.Keys.ElementAt(PlayerController.Instance.Rng.Next(caseHandler.NPCLocations.Keys.Count))
        //                    );
        //                    break;
        //                case 2: n_lie = ClueFactory.Instance.CreatePointerNode(lieTargetNode, n.NPC); break;
        //                case 3: n_lie = ClueFactory.Instance.CreateSupportNode(lieTargetNode, n.NPC, ClueIdentifier.Accusatory); break;
        //                default: throw new Exception("GraphBuilder tried to access invalid ClueIdentifier index.");
        //            }
        //            //n_lie = ClueFactory.Instance.CreateSupportNode(lieTargetNode, n.NPC, lieIdentifier);
        //        }

        //        lieGraph.Nodes.Add(n_lie);
        //        // Setup current reference in the graph.
        //        SetupReference(lieGraph, n_lie);
        //        // Setup references to the lie.
        //        SetupLieReference(truthGraph, lieGraph, n);
        //    }

        //    return lieGraph;
        //}

        #region Graph Rigging
        //private static void SetupReference(Graph g, Node n) {
        //    n.IsVisited = false;
        //    // Throws InvalidOperationException if TargetNodes is empty.
        //    if (g.ReferenceLookup.ContainsKey(n.TargetNodes.First()))
        //        g.ReferenceLookup[n.TargetNodes.First()].Add(n);
        //    else
        //        g.ReferenceLookup.Add(n.TargetNodes.First(), new List<Node>() { n });
        //}

        //private static void SetupLieReference(Graph truthGraph, Graph lieGraph, Node n) {
        //    List<Node> refNodes = new List<Node>();
        // Inverts truth statements targeted at NPC hooked to current node.
        //if (truthGraph.ReferenceLookup.TryGetValue(n, out refNodes))
        //    foreach (Node refN in refNodes) {
        //        var newRefClueTemplate = ClueConverter.GetClueTemplate(ClueIdentifier.Accusatory);
        //        refN.NodeClue = new Clue(newRefClueTemplate, refN.TargetNodes.First().NPC, ClueIdentifier.Accusatory, NPCPart.NPCPartType.None);
        //    }
        // Inverts deceptive statements targeted at NPC hooked to current node.
        //if (lieGraph.ReferenceLookup.TryGetValue(n, out refNodes))
        //    foreach (Node refN in refNodes) { }
        //        if (refN.NodeClue.Identifier == ClueIdentifier.Accusatory) {
        //            //refN = ClueFactory.Instance.CreatePeopleLocationNode(refN.TargetNodes, refN.NPC, caseHandler.NPCLocations.Keys.ElementAt(PlayerController.Instance.Rng.Next(caseHandler.NPCLocations.Keys.Count)));
        //            var newRefClueTemplate = ClueConverter.GetClueTemplate(ClueIdentifier.PeopleLocation);
        //            refN.NodeClue = new Clue(newRefClueTemplate, refN.TargetNodes.First().NPC, ClueIdentifier.PeopleLocation, NPCPart.NPCPartType.None);
        //        }
        //}
        #endregion

        /// <summary>
        /// Builds a graph using random parameters.
        /// </summary>
        /// <param name="nodeCount">The total node count of the graph.</param>
        /// <param name="descriptiveCount">The amount of descriptive nodes that should appear in the graph.</param>
        /// <returns>A new Graph built using random parameters.</returns>
        //public static Graph BuildRandomGraph(int nodeCount, int descriptiveCount) {
        //    while (nodeCount - descriptiveCount - 1 < 0 && descriptiveCount > 0)
        //        descriptiveCount--;

        //    /// WorkFlow:
        //    // Add Killer Node.
        //    // Add/Setup Descriptive Nodes.
        //    // Add/Setup Pointers.
        //    // Add/Setup Murder Location (Entire murdercase?).
        //    // Add/Setup People Locations (Inspired by murdercase?).
        //    // Add/Setup Accusations. (Add after adding lies?).
        //    // Setup Killer Node.
        //    Graph g = new Graph();

        //    // Adding Killer Node
        //    g.Nodes.Add(ClueFactory.Instance.CreateKillerNode());
        //    caseHandler = new CaseHandler(NPC.NPCList);

        //    // Getting NPCs at crime scene.
        //    var killerRelevantNPCs = caseHandler.NPCLocations[caseHandler.MurderLocation];

        //    // Adds descriptive nodes and hooks them to the killer node.
        //    // TODO: Take into account the location of the NPCs
        //    var descriptiveNodes = ClueFactory.Instance.CreateDescriptiveNodes(g.Nodes[0], descriptiveCount); 
        //    g.Nodes.AddRange(descriptiveNodes);

        //    // Adds one truthful pointer for each descriptive node.
        //    // TODO: Take into account the location of the NPCs
        //    foreach (Node node in descriptiveNodes) {
        //        var hookNPC = NPC.NPCList.Where(npc => !g.Nodes.Any(n => n.NPC.Equals(npc))).FirstOrDefault();
        //        g.Nodes.Add(ClueFactory.Instance.CreatePointerNode(node, hookNPC));
        //    }

        //    var murderLocationClues = 2;
        //    var peopleLocationClues = 2;
        //    // Creating support Nodes
        //    while (g.Nodes.Count < nodeCount) {
        //        // Find random NPC which does not currently have a truth statement attached.
        //        var hookNPC = NPC.NPCList.Where(npc => !g.Nodes.Any(node => node.NPC.Equals(npc))).FirstOrDefault();
        //        if (--murderLocationClues >= 0) {
        //            g.Nodes.Add(ClueFactory.Instance.CreateMurderLocationNode(g.Nodes[0], hookNPC, caseHandler.MurderLocation));
        //            continue;
        //        }

        //        if (--peopleLocationClues >= 0) {
        //            // TODO: Make sure this takes random targets, not just "the first 3"
        //            string location;
        //            // Avoiding desolate locations, as these gives errors during ClueStatement construction.
        //            do { location = caseHandler.NPCLocations.Keys.ElementAt(PlayerController.Instance.Rng.Next(caseHandler.NPCLocations.Keys.Count)); }
        //            while (caseHandler.NPCLocations[location].Count < 1);
        //            var targets = g.Nodes.Where(node => caseHandler.NPCLocations[location].Any(npc => npc == node.NPC)).ToList();
        //            g.Nodes.Add(ClueFactory.Instance.CreatePeopleLocationNode(targets, hookNPC, location));
        //            continue;
        //        }

        //        // Find TargetNodes without self-referencing.
        //        Node newTarget = g.Nodes.Where(node => node.NPC != hookNPC).ToList()[PlayerController.Instance.Rng.Next(g.Nodes.Count)];
        //        //while (newTarget == newTarget.TargetNodes) {
        //        if (newTarget.TargetNodes != null && newTarget == newTarget.TargetNodes.FirstOrDefault()) {
        //            //newTarget = g.Nodes[r.Next(g.Nodes.Count)];
        //            throw new Exception("NewTarget references itself.");
        //        }

        //        var restNode = ClueFactory.Instance.CreateSupportNode(newTarget, hookNPC);
        //        g.Nodes.Add(restNode);
        //    }

        //    // Set up killerNode.Clue
        //    var restNodes = g.Nodes.Where(n => !n.IsDescriptive && !n.IsKiller).ToList();
        //    // If there's not any other NPCs than descriptive & killer, pick descriptive.
        //    if (restNodes.Count < 1)
        //        restNodes = g.Nodes.Where(n => !n.IsKiller).ToList();
        //    // If there's not any other NPCs than killer, return g with killer referencing self.
        //    if (restNodes.Count < 1) {
        //        g.Nodes[0].TargetNodes = new List<Node>() { g.Nodes[0] };
        //        ClueFactory.Instance.SetupSupportNode(g.Nodes[0], g.Nodes[0].TargetNodes);
        //        return g;
        //    } else { 
        //        g.Nodes[0].TargetNodes = new List<Node>() { restNodes[PlayerController.Instance.Rng.Next(restNodes.Count)] };
        //        // Sets up the KillerNode with a support clue.
        //        ClueFactory.Instance.SetupSupportNode(g.Nodes[0], g.Nodes[0].TargetNodes);
        //    }

        //    // Managing ReferenceLookup.
        //    foreach (Node n in g.Nodes) {
        //        SetupReference(g, n);
        //    }

        //    // Returning.
        //    return g;
        //}

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
                ClueFactory.Instance.SetupSupportNode(killerNode, caseHandler.NPCLocations[caseHandler.MurderLocation]);
                hasKiller = true;
                // The killer, explaining the people at the murder location, is a valid pointer target.
                pointerTargets.Add(killerNode);
                remainingNPCs.Remove(killerNode.NPC);
            }

            while (murderLocationClues-- > 0) {
                var newNode = ClueFactory.Instance.CreateMurderLocationNode(
                    killerNode.NPC,
                    remainingNPCs[PlayerController.Instance.Rng.Next(remainingNPCs.Count)],
                    caseHandler.MurderLocation);

                g.Nodes.Add(newNode);
                pointerTargets.Add(newNode);
                remainingNPCs.Remove(newNode.NPC);
            }

            /* DESCRIPTIVE NODES */
            if (!hasDescriptions) {
                var descriptiveNodes = ClueFactory.Instance.CreateDescriptiveNodes(killerNode.NPC, remainingNPCs, descriptions);
                pointerTargets.AddRange(descriptiveNodes);
                g.Nodes.AddRange(descriptiveNodes);
                hasDescriptions = true;
                remainingNPCs.RemoveAll(npc => pointerTargets.Select(node => node.NPC).Contains(npc));
            }

            /* PEOPLE LOCATION NODES */
            while (locationClues-- > 0) {
                var newNode = new Node();
                // Should probably be a counter instead of a boolean.
                if (!hasKillerLocation) {
                    var murderScenePois = caseHandler.NPCLocations[caseHandler.MurderLocation];

                    newNode = ClueFactory.Instance.CreatePeopleLocationNode(
                        murderScenePois,
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
                        locationPois,
                        availablePois.ElementAt(PlayerController.Instance.Rng.Next(availablePois.Count())),
                        locationString);
                }

                g.Nodes.Add(newNode);
                remainingNPCs.Remove(newNode.NPC);
            }

            while (pointers-- > 0) {
                var pointerTarget = pointerTargets.ElementAt(PlayerController.Instance.Rng.Next(pointerTargets.Count));
                var newNode = ClueFactory.Instance.CreatePointerNode(pointerTarget.NPC, remainingNPCs[PlayerController.Instance.Rng.Next(remainingNPCs.Count)]);

                g.Nodes.Add(newNode);
                remainingNPCs.Remove(newNode.NPC);
            }

            return g;
        }

        public static Graph BuildLieGraph(Graph truthGraph, int descriptiveLies, int miscLies, int hidingDescriptive) {
            // Create neccessary graph and lists.
            var lieGraph = new Graph();
            List<NPC> remainingNPCs = NPC.NPCList.Where(npc => !truthGraph.Nodes.Select(node => node.NPC).Contains(npc)).ToList();
            List<NPC> nonKillerNPCs = NPC.NPCList.Where(npc => !npc.IsKiller).ToList();

            List<Node> descriptiveNodes = truthGraph.Nodes.Where(node => node.IsDescriptive).ToList();
            // Create non-similar lists.
            // This is to avoid lies accidently aligning with the truth.
            var killer = NPC.NPCList.First(npc => npc.IsKiller);
            var nonSimilarHats = nonKillerNPCs.Where(npc => npc.Hat.Description != killer.Hat.Description);
            var nonSimilarTorsos = nonKillerNPCs.Where(npc => npc.Torso.Description != killer.Torso.Description);
            var nonSimilarPants = nonKillerNPCs.Where(npc => npc.Legs.Description != killer.Legs.Description);
            var possibleTargets = new List<NPC>();
            Node lieNode = new Node();
            while (hidingDescriptive-- > 0 && descriptiveNodes.Count > 0) {
                Node newLie = ClueFactory.Instance.CreateRandomLie(descriptiveNodes[0].NPC, caseHandler, truthGraph);

                remainingNPCs.Remove(descriptiveNodes[0].NPC);
                descriptiveNodes.RemoveAt(0);
                lieGraph.Nodes.Add(newLie);
            }

            while (descriptiveLies-- > 0) {
                if (remainingNPCs.Count == 0) { throw new Exception("No remaining NPCs. Less liars required."); }

                int partIdx = PlayerController.Instance.Rng.Next(3);
                switch (partIdx) {
                    case 0: possibleTargets = nonKillerNPCs.Where(npc1 => nonSimilarHats.Any(npc2 => npc1 == npc2)).ToList(); break;
                    case 1: possibleTargets = nonKillerNPCs.Where(npc1 => nonSimilarTorsos.Any(npc2 => npc1 == npc2)).ToList(); break;
                    case 2: possibleTargets = nonKillerNPCs.Where(npc1 => nonSimilarPants.Any(npc2 => npc1 == npc2)).ToList(); break;
                    default: throw new Exception("Random generator tried to access non-existant clothing.");
                }

                NPC lieTarget = possibleTargets[PlayerController.Instance.Rng.Next(possibleTargets.Count)];
                NPC newLiar = remainingNPCs[PlayerController.Instance.Rng.Next(remainingNPCs.Count)];
                lieNode = ClueFactory.Instance.CreateDescriptiveNode(lieTarget, newLiar, partIdx);

                lieGraph.Nodes.Add(lieNode);
                remainingNPCs.Remove(newLiar);
            }

            while (miscLies-- > 0 && remainingNPCs.Count > 0) {
                Node newLie = ClueFactory.Instance.CreateRandomLie(remainingNPCs[PlayerController.Instance.Rng.Next(remainingNPCs.Count)], caseHandler, truthGraph);

                remainingNPCs.Remove(newLie.NPC);
                lieGraph.Nodes.Add(newLie);
            }

            if (miscLies > 0) { UnityEngine.Debug.LogWarning(string.Format("RemainingNPCs has been exhausted before all lies distributed! {0} lies remaining.", miscLies)); }

            return lieGraph;
        }

        public static void AttachAccusationsToGraph(Graph truth, Graph lies) {
            var remainingNPCs = NPC.NPCList.Where(npc => !truth.Nodes.Select(node => node.NPC).Contains(npc));
            var liars = lies.Nodes.Select(node => node.NPC);

            foreach (NPC npc in remainingNPCs) {
                var possibleTargets = liars.Where(n => n != npc).ToList();
                Node accusation = ClueFactory.Instance.CreateAccusationNode(possibleTargets[PlayerController.Instance.Rng.Next(possibleTargets.Count)], npc);
                truth.Nodes.Add(accusation);
            }
        }

        private static string NPCDescriptionToString(NPCPart.NPCPartDescription description) {
            return Enum.GetName(typeof(NPCPart.NPCPartDescription), description);
        }
    }
}
