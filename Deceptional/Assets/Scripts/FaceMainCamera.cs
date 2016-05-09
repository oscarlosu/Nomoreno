using UnityEngine;
using System.Collections;

public class FaceMainCamera : MonoBehaviour {
	public float rotationOffset = -82.80086f;
    void OnEnable() {
        UpdateRotation();
    }
	// Update is called once per frame
	void Update () {
        UpdateRotation();
    }
    public void UpdateRotation() {        
		transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y + rotationOffset, 0);
    }

}
