﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public class Node {
        public Node TargetNode { get; set; }
        public string Clue { get; set; }
        public bool IsVisited { get; set; }
        public bool IsDescriptive { get; set; }
        public bool IsKiller { get; set; }
    }
}
