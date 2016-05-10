using UnityEngine;
using System.Collections;

public abstract class Transition : MonoBehaviour {
    public enum TransitionState {
        Waiting,
        Running,
        Done
    }
    public TransitionState State;

    public void Start() {
        State = TransitionState.Waiting;
    }
    public abstract void Execute();
}
