using UnityEngine;
using System.Collections;

public class NameLabel : MonoBehaviour {
    private float epsilon = 0.5f;
    public Vector3 Center;
    void OnEnable() {
        SetRotation();
    }
	// Update is called once per frame
	void Update () {
        SetRotation();
    }
    private void SetRotation() {
        if (Vector3.SqrMagnitude(transform.position) > epsilon) {
            transform.LookAt(Center);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

}
