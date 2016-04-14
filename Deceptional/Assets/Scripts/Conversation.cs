using System;
using Assets.Scripts;

public class Conversation
{
    public Clue ActualClue;
    public Clue FirstStatementClue;
<<<<<<< HEAD
    public Clue SecondStatementClue = new Clue("*Triggering intensifies*", null, ClueIdentifier.Informational, NPCPart.NPCPartType.None);
    public readonly string MoodyMessage = "You called me a liar! I'm not gonna talk to you!";
=======
    public Clue SecondStatementClue = new Clue("I wasn't lying...\nI'm not gonna talk to you!", null, ClueIdentifier.Informational, NPCPart.NPCPartType.None);
>>>>>>> 5dcb75035ba9b0b444d031992904d8696196e748
    private bool isTrue = true;
    public bool IsTrue {
        get { return isTrue; }
        private set { isTrue = value; }
    }

    private bool isDisabled;

    public Conversation(Clue firstStatement) {
        ActualClue = FirstStatementClue = firstStatement;
    }

    public Conversation(Clue truthStatement, Clue lieStatement) : this(truthStatement) {
        PushStatement(lieStatement);
    }

    public bool Next(bool choice) {
        if (!isDisabled)
            ActualClue = SecondStatementClue;
        
        return IsTrue == choice;
    }

    public void Disable() { isDisabled = true; }

    public void PushStatement(Clue newStatement) {
        SecondStatementClue = FirstStatementClue;
        ActualClue = FirstStatementClue = newStatement;
        IsTrue = false;
    }
}
