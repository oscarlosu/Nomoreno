using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotateTransition : Transition {
    public Vector3 Axis;
    public Vector3 Center;
    public float TargetAngle;
    public AnimationCurve Speed;
    public float RotationSense;

    private float elapsedTime;
    private float currentAngle;


    //public new void Start() {
    //    base.Start();
    //    Execute();
    //}
    public override void Execute() {
        StartCoroutine(RotateAroundCenterAboutAxis(RotationSense));
    }
    public void Execute(float sense) {
        StartCoroutine(RotateAroundCenterAboutAxis(sense));
    }
    private IEnumerator RotateAroundCenterAboutAxis(float sense) {
        State = TransitionState.Running;

        currentAngle = 0;
        // Lerp towards current target
        elapsedTime = 0;
        while (!LerpTowardsTarget()) {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        State = TransitionState.Done;
    }
    private bool LerpTowardsTarget() {

        float remaining = TargetAngle - currentAngle;
        float angleInc = Mathf.Min(Speed.Evaluate(elapsedTime), remaining);
        transform.RotateAround(Center, Axis, RotationSense * angleInc);
        currentAngle += angleInc;
        return Mathf.Approximately(currentAngle - TargetAngle, 0.0f);


        //Vector3 dir = Targets[index] - lastTarget;
        //dir.Normalize();
        //float distanceToTarget = Vector3.Distance(transform.position, Targets[index]);
        //transform.position = transform.position + dir * Mathf.Min(SpeedCurves[index - 1].Evaluate(elapsedTime), distanceToTarget);

    }
}
