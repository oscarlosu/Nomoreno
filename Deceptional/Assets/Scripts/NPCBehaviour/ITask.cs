using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.NPCBehaviour {
    public interface ITask {
        void Interrupt();
        bool CanExecute(object parameter);
        void Execute(object parameter);
    }
}
