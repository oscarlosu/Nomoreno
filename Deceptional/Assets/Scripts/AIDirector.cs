using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public class AIDirector {
        public float PercentageLiars;
        public float PercentageDescriptiveLiars;

        public int NumMinglers;
        public int MinLiars;
        public int NumDescriptiveClues;
        public float MingleValue;

        public virtual void ChangeParameters() { }
    }
}
