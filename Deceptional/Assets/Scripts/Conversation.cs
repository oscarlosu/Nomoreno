using UnityEngine;
using System.Collections;

public class Conversation
{
    public string ShownStatement;

    private bool isDisabled;
    private bool isTrue;
    private string firstStatement;
    private string leftStatement;
    private string rightStatement;

    public Conversation() {
        ShownStatement = firstStatement;
    }

    public void Next(bool choice) {
        if (!isDisabled)
            if (choice == isTrue)
                ShownStatement += "\n(True?)\n" + leftStatement;
            else
                ShownStatement += "\n(False?)\n" + rightStatement; 
    }

    public void Disable() { isDisabled = true; }
}
