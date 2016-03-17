using UnityEngine;
using System.Collections;

public class Conversation
{
    public string ShownStatement;

    private bool isDisabled;
    private bool isTrue = true;
    private string firstStatement;
    private string secondStatement = "*Triggering intensifies*";

    public Conversation(string firstStatement) {
        ShownStatement = this.firstStatement = firstStatement;
    }

    public void Next(bool choice) {
        if (!isDisabled)
            ShownStatement += "\n(Liar)\n" + secondStatement; 
    }

    public void Disable() { isDisabled = true; }
    
    public void PushStatement(string newStatement) {
        secondStatement = firstStatement;
        firstStatement = newStatement;
        isTrue = false;
    }
}
