using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Assets.Scripts;
using System.Reflection;

public class ButtonController : MonoBehaviour, IPointerClickHandler
{
    public string MethodName;

    public void OnPointerClick(PointerEventData eventData)
    {
        MethodBase mb = typeof(PlayerController).GetMethod(MethodName);
        mb.Invoke(PlayerController.Instance, new Object[0]);
    }
}
