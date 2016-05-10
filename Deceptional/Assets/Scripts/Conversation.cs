using System;
using System.Collections.Generic;
using Assets.Scripts;

public class Conversation
{
    /// <summary>
    /// Currently visible clue
    /// </summary>
    public Clue ActualClue;
    public Clue FirstStatementClue;
    /// <summary>
    /// Only relevant when FirstStatementClue is false
    /// </summary>
    public Clue SecondStatementClue = new Clue("", new List<NPC>(), ClueIdentifier.PeopleLocation, NPCPart.NPCPartType.None);
    public readonly string MoodyMessage = "You called me a liar! I'm not gonna talk to you!";
    private bool isTrue = true;
    public bool IsTrue {
        get { return isTrue; }
        private set { isTrue = value; }
    }

    public Conversation(Clue firstStatement) {
        ActualClue = FirstStatementClue = firstStatement;
    }

    public Conversation(Clue truthStatement, Clue lieStatement) : this(truthStatement) {
        PushStatement(lieStatement);
    }

    public bool Next(bool choice) {
            ActualClue = SecondStatementClue;
        
        return IsTrue == choice;
    }

    public void PushStatement(Clue newStatement) {
        SecondStatementClue = FirstStatementClue;
        ActualClue = FirstStatementClue = newStatement;
        IsTrue = false;
    }
}
