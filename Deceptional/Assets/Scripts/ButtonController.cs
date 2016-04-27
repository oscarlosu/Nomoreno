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
       
    public float SpeedDown = 0.05f;
    public float SpeedUp = 0.01f;

    private Vector3 startPos;

    public enum TriggerMode {
        OnDown,
        OnUp
    }
    public TriggerMode Mode;

    void Start() {
        startPos = transform.position;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(Mode == TriggerMode.OnDown) {
            MethodBase mb = typeof(PlayerController).GetMethod(MethodName);
            mb.Invoke(PlayerController.Instance, new Object[0]);
        }        
        StartCoroutine(AnimateButton());
    }

    public void ChangeButton(string newLabel, string newMethodName) {
        Label = newLabel;
        TextMesh.text = newLabel;
        MethodName = newMethodName;
    }

    private IEnumerator AnimateButton() {
        float travelled = 0;
        while (travelled < 0.25f) {
            travelled += SpeedDown;
            transform.position -= new Vector3(0, SpeedDown, 0);
            yield return null;
        }
        travelled = 0;
        while (travelled < 0.25f) {
            travelled += SpeedUp;
            transform.position += new Vector3(0, SpeedUp, 0);
            yield return null;
        }

        if (Mode == TriggerMode.OnUp) {
            MethodBase mb = typeof(PlayerController).GetMethod(MethodName);
            mb.Invoke(PlayerController.Instance, new Object[0]);
        }
    }
}
