using UnityEngine;
using System.Collections;

public class NextDayText : MonoBehaviour {
    void OnEnable() {
        SetRotation();
    }
    // Update is called once per frame
    void Update() {
        SetRotation();
    }
    private void SetRotation() {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, 0);
    }

}
