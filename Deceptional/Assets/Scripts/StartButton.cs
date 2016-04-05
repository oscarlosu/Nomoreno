using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class StartButton : MonoBehaviour, IPointerClickHandler {

    private Vector3 origPos;

    void Start() {
        origPos = transform.position;
    }

    public void OnPointerClick(PointerEventData eventData) {
        StartCoroutine(AnimateButton());
    }

    private IEnumerator AnimateButton() {
        bool hasMoved = false;
        while (transform.position.y > origPos.y - 0.25 && !hasMoved) {
            //Debug.Log(transform.position.y);
            transform.position -= new Vector3(0, 0.05f, 0);
            yield return new WaitForSeconds(0.005f);
        }
        hasMoved = true;
        while (transform.position.y < origPos.y && hasMoved) {
            //Debug.Log(transform.position.y);
            transform.position += new Vector3(0, 0.01f, 0);
            yield return new WaitForSeconds(0.005f);
        }
        //Debug.Log("DONE");
        transform.position = origPos;

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
