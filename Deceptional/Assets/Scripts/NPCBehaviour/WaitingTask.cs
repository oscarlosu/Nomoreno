using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.NPCBehaviour {
    public class WaitingTask : ITask {
        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            throw new NotImplementedException();
        }

        public void Interrupt() {
            throw new NotImplementedException();
        }
    }
}
