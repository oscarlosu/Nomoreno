using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Assets.Scripts;
using System.Reflection;

public class ButtonController : MonoBehaviour, IPointerClickHandler
{
    public string Label;
    public string MethodName;
    public TextMesh TextMesh;
    private Vector3 origPos;

    void Start() {
        origPos = transform.position;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MethodBase mb = typeof(PlayerController).GetMethod(MethodName);
        mb.Invoke(PlayerController.Instance, new Object[0]);
        StartCoroutine(AnimateButton());
    }

    public void ChangeButton(string newLabel, string newMethodName) {
        Label = newLabel;
        TextMesh.text = newLabel;
        MethodName = newMethodName;
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
    }
}
