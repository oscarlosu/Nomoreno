using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DropTransition : Transition {
    /// <summary>
    /// Positions through which the object has to go
    /// </summary>
    public List<Vector3> Targets;
    /// <summary>
    /// Curve with index i defines the duration and speed of the lerping between Targets[i] and
    /// Targets[i+1]
    /// </summary>
    public List<AnimationCurve> SpeedCurves;

    private float elapsedTime;
    private Vector3 lastTarget;


    //public new void Start() {
    //    base.Start();
    //    Execute();
    //}
    public override void Execute() {
        StartCoroutine(GoThroughTargets());
    }
    private IEnumerator GoThroughTargets() {
        State = TransitionState.Running;
        transform.position = Targets[0];
        // Go from startPos through all the targets
        for(int i = 1; i < Targets.Count; ++i) {
            // Set last target to previous
            lastTarget = Targets[i - 1];
            // Lerp towards current target
            elapsedTime = 0;
            while (!LerpTowardsTarget(i)) {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
        }
        State = TransitionState.Done;
    }
    private bool LerpTowardsTarget(int index) {
        Vector3 dir = Targets[index] - transform.position;
        dir.Normalize();
        float distanceToTarget = Vector3.Distance(transform.position, Targets[index]);
        transform.position = transform.position + dir * Mathf.Min(SpeedCurves[index - 1].Evaluate(elapsedTime), distanceToTarget);
        return Vector3.Distance(transform.position, Targets[index]) <= Mathf.Epsilon;
    }
}
