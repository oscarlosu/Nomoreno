using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class StartButton : MonoBehaviour, IPointerClickHandler {

    public float SpeedDown = 0.05f;
    public float SpeedUp = 0.01f;

    private Vector3 startPos;

    void Start() {
        startPos = transform.position;
    }

    public void OnPointerClick(PointerEventData eventData) {
        StartCoroutine(AnimateButton());
    }

    private IEnumerator AnimateButton() {
        float travelled = 0;
        while (travelled < 0.25f) {
            travelled += SpeedDown;
            transform.position -= transform.up * SpeedDown;
            yield return null;
        }
        travelled = 0;
        while (travelled < 0.25f) {
            travelled += SpeedUp;
            transform.position += transform.up * SpeedUp;
            yield return null;
        }

        TransitionManager.Instance.StartTransition();
    }
}
