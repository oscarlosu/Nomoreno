using UnityEngine;
using System.Collections;

public class ButtonTester : MonoBehaviour {

    public TextMesh tm;

	public void ArrestButton()
    {
        tm.text = "Arrest";
    }
    public void AccuseButton()
    {
        tm.text = "You Lie!";
    }
    public void CallInButton()
    {
        tm.text = "Call in";
    }
}
