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
    private float travelledDistance;
    private float targetDistance;

    //public new void Start() {
    //    base.Start();
    //    Execute();
    //}
    public override void Execute() {
        StartCoroutine(GoThroughTargets());
    }
    private IEnumerator GoThroughTargets() {
        State = TransitionState.Running;
        transform.localPosition = Targets[0];
        // Go from startPos through all the targets
        for (int i = 1; i < Targets.Count; ++i) {

            // Lerp towards current target
            elapsedTime = 0;
            travelledDistance = 0;
            targetDistance = Vector3.Distance(transform.localPosition, Targets[i]);
            while (!LerpTowardsTarget(i)) {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
        }
        State = TransitionState.Done;
    }
    private bool LerpTowardsTarget(int index) {
        Vector3 dir = Targets[index] - transform.localPosition;
        dir.Normalize();
        float remainingDistance = targetDistance - travelledDistance;
        float stepLength = Mathf.Min(SpeedCurves[index - 1].Evaluate(elapsedTime), remainingDistance);
        travelledDistance += stepLength;
        transform.localPosition = transform.localPosition + dir * stepLength;
        return travelledDistance >= targetDistance;
    }
}
