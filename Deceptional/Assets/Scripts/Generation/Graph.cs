using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public class Graph {
        private readonly Dictionary<Node, List<Node>> referenceLookup = new Dictionary<Node, List<Node>>();
        public Dictionary<Node, List<Node>> ReferenceLookup { get { return referenceLookup; } }

        private readonly List<Node> nodes = new List<Node>();
        public List<Node> Nodes { get { return nodes; } }
    }
}
