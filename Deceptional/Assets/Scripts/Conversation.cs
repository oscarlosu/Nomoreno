using System;
using Assets.Scripts;

public class Conversation
{

    public string ShownStatement;
    public string FirstStatement;
    public string SecondStatement = "*Triggering intensifies*";

    public Clue FirstStatementClue;
    public Clue SecondStatementClue;
    public Clue ActualClue;
    public bool IsTrue = true;

    private bool isDisabled;

    public Conversation(Clue firstStatement) {
        ActualClue = FirstStatementClue = firstStatement;
    }

    public Conversation(Clue truthStatement, Clue lieStatement) : this(truthStatement) {
        PushStatement(lieStatement);
    }

    [Obsolete]
    public Conversation(string firstStatement, Clue firstStatementClue) {
        ShownStatement = FirstStatement = firstStatement;
        ActualClue = FirstStatementClue = firstStatementClue;
    }
    
    [Obsolete]
    public Conversation(string firstStatement) {
        ShownStatement = FirstStatement = firstStatement;
    }

    [Obsolete]
    public Conversation(string truthStatement, string lieStatement) : this(truthStatement) {
        PushStatement(lieStatement);
    }

    public bool Next(bool choice) {
        if (!isDisabled)
            ShownStatement = SecondStatement;

        return IsTrue == choice;
    }

    public void Disable() { isDisabled = true; }

    [Obsolete]
    public void PushStatement(string newStatement) {
        SecondStatement = FirstStatement;
        ShownStatement = FirstStatement = newStatement;
        IsTrue = false;
    }

    [Obsolete]
    public void PushStatement(string newStatement, Clue newStatementClue) {
        PushStatement(newStatement);
        PushStatement(newStatementClue);
    }

    public void PushStatement(Clue newStatement) {
        SecondStatementClue = FirstStatementClue;
        ActualClue = FirstStatementClue = newStatement;
        IsTrue = false;
    }
}
