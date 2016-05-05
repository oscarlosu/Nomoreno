using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour {

    public bool mouseDownOnPlatform;
    public Vector3 lastClickPos;
    public Vector3 currentClickPos;
    public Vector3 center;

    private Camera cam;
    public float MaximumSpeed;
    public float Drag;

    public float MinAngle;
    private float velocity;



    void Awake() {        
        //center = Vector3.zero;
        cam = GetComponent<Camera>();
        // Calculate projection of the center of the waiting room cylinder in screen space
        center = Vector3.zero;
    }

    void OnEnable() {
        velocity = 0;
    }
	// Update is called once per frame
	void Update () {
        //if(Input.touchSupported) {
        //       if(Input.GetTouch(0).phase == TouchPhase.Began) {
        //           MouseDown = true;
        //           currentClickPos = cam.ScreenToWorldPoint(Input.GetTouch(0).position);
        //       } else if(MouseDown && Input.GetTouch(0).phase == TouchPhase.Moved) {
        //           lastClickPos = currentClickPos;
        //           currentClickPos = cam.ScreenToWorldPoint(Input.GetTouch(0).position);
        //           // Apply rotation to camera
        //           Vector3 start = lastClickPos - center;
        //           Vector3 end = currentClickPos - center;
        //           transform.RotateAround(Vector3.zero, Vector3.up, CalculateAngle(start, end));
        //       }
        //   } else {
        if (Input.GetMouseButtonDown(0)) {
            mouseDownOnPlatform = UpdateCurrentClickPos();
        } else if (mouseDownOnPlatform) {
            lastClickPos = currentClickPos;
            if (Input.GetMouseButton(0)) {
                if(UpdateCurrentClickPos()) {
                    mouseDownOnPlatform = true;
                    UpdateCameraVelocity();
                }
            } else {
                mouseDownOnPlatform = false;
            }
        }
        velocity += (Mathf.Sign(-velocity) * Mathf.Min(Drag, Mathf.Abs(velocity)));
        transform.RotateAround(center, Vector3.up, velocity * Time.deltaTime);
        //RotationWithMouseWheel();
        //}        
    }

    private void UpdateCameraVelocity() {
        // Apply rotation to camera
        Vector3 start = lastClickPos - center;
        Vector3 end = currentClickPos - center;
        float angle = CalculateAngle(start, end);
        if(Mathf.Abs(angle) > MinAngle) {
            velocity += (Mathf.Sign(-angle) * MaximumSpeed);
        }              
    }

    private bool UpdateCurrentClickPos() {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100)) {
            currentClickPos = hit.point;
            return true;
        }
        return false;
    }

    private float CalculateAngle(Vector3 start, Vector3 end) {
        float angle = Vector3.Angle(start, end);
        Vector3 cross = Vector3.Cross(start.normalized, end.normalized);
        return angle * Mathf.Sign(cross.y);
    }

    void RotationWithMouseWheel() {
        if (!Mathf.Approximately(Input.mouseScrollDelta.y, 0.0f)) {
            transform.RotateAround(Vector3.zero, Vector3.up, Input.mouseScrollDelta.y * 10);
        }
    }
}
