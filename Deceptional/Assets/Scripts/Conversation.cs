using UnityEngine;
using System.Collections;

public class Conversation
{
    public string ShownStatement;
    public string FirstStatement;
    public string SecondStatement = "*Triggering intensifies*";
    public bool IsTrue = true;

    private bool isDisabled;

    public Conversation(string firstStatement) {
        ShownStatement = FirstStatement = firstStatement;
    }
    
    public Conversation(string truthStatement, string lieStatement) : this(truthStatement) {
        PushStatement(lieStatement);
    }

    public void Next(bool choice) {
        if (!isDisabled)
            ShownStatement += "\n(Liar)\n" + SecondStatement; 
    }

    public void Disable() { isDisabled = true; }
    
    public void PushStatement(string newStatement) {
        SecondStatement = FirstStatement;
        FirstStatement = newStatement;
        IsTrue = false;
    }
}
