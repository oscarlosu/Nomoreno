using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour {

    public bool MouseDown;
    public Vector3 lastMousePos;
    public Vector3 currentMousePos;
    public Vector3 center;

    void Awake() {
        // Calculate projection of the center of the waiting room cylinder in screen space
        center = Camera.main.WorldToScreenPoint(Vector3.zero);
    }
	// Update is called once per frame
	void Update () {
	    if(Input.touchSupported) {
            if(Input.GetTouch(0).phase == TouchPhase.Began) {
                MouseDown = true;
                currentMousePos = Input.GetTouch(0).position;
            } else if(MouseDown && Input.GetTouch(0).phase == TouchPhase.Moved) {
                lastMousePos = currentMousePos;
                currentMousePos = Input.GetTouch(0).position;
                // Apply rotation to camera
                Vector3 start = lastMousePos - center;
                Vector3 end = currentMousePos - center;
                transform.RotateAround(Vector3.zero, Vector3.up, CalculateAngle(start, end));
            }
        } else {
            if (Input.GetMouseButtonDown(0)) {
                MouseDown = true;
                currentMousePos = Input.mousePosition;
            } else if(MouseDown && Input.GetMouseButton(0)) {
                lastMousePos = currentMousePos;
                currentMousePos = Input.mousePosition;
                // Apply rotation to camera
                Vector3 start = lastMousePos - center;
                Vector3 end = currentMousePos - center;                
                transform.RotateAround(Vector3.zero, Vector3.up, CalculateAngle(start, end));
            }
            RotationWithMouseWheel();
        }        
    }

    private float CalculateAngle(Vector3 start, Vector3 end) {
        float angle = Vector3.Angle(start, end);

        Vector3 cross = Vector3.Cross(start.normalized, end.normalized);
        if (Vector3.Dot(cross, Vector3.forward) < 0) {
            angle = -angle;
        }
        return angle;
    }

    void RotationWithMouseWheel() {
        if (!Mathf.Approximately(Input.mouseScrollDelta.y, 0.0f)) {
            transform.RotateAround(Vector3.zero, Vector3.up, Input.mouseScrollDelta.y * 10);
        }
    }
}
