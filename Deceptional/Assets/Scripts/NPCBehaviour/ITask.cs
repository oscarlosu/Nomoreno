using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Assets.Scripts.NPCBehaviour {
    public interface ITask {
        void Interrupt();
        bool CanExecute(object parameter);
        IEnumerator Execute(object parameter);
    }
}
