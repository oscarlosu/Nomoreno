using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public class PlayerController {

        #region Static fields & properties
        public static void Construct() { Instance = new PlayerController(); }

        public static PlayerController Instance { get; private set; }
        #endregion

        #region Constructer
        private PlayerController() {

        }
        #endregion

        #region Instance fields & properties
        public NPC CurrentInterrogationTarget;
        public bool UseRealTime;

        private int interactionCount;
        private int dailyInteracions;
        private int currentTime;
        private int dayDuration;
        #endregion

        #region Instance methods
        public void Call(NPC target) {
            if (CurrentInterrogationTarget != null)
                Dismiss();

            CurrentInterrogationTarget = target;
            CurrentInterrogationTarget.GoToInterrogation();
        }

        public void DisplayConversation() {
            // Set UI text to CurrentInterrogationTarget.Conversation.Statement
        }

        public void Arrest() {
            // Check if victorious. Else ->
            // Make NPC angry + Make invulnerable.
            NPC currentInterrogationNPC = CurrentInterrogationTarget.GetComponent<NPC>();
            if (currentInterrogationNPC.IsKiller) { /* Run Victory */ }
            else currentInterrogationNPC.Mood = false;

            // Disables NPCS. 
            // (Obsolete, should skip to next day).
            //foreach (NPC poi in NPCList.Instance) {
            //    poi.Conversation.ShownStatement = "I think we are done here"; // Placeholder for "ArrestStatement".
            //    poi.Conversation.Disable();
            //}

            NextDay();
        }
        public void Accuse() { CurrentInterrogationTarget.Conversation.Next(false); }
        public void Question() { CurrentInterrogationTarget.Conversation.Next(true); }
        public void Dismiss() {
            CurrentInterrogationTarget.GoToWaiting();
            CurrentInterrogationTarget = null;
            if (UseRealTime && currentTime == 0)
                NextDay();
            else if (!UseRealTime && interactionCount == 0)
                NextDay();
        }
        
        public void NextDay() {
            currentTime = dayDuration;
            interactionCount = dailyInteracions;
            // Hard-reset the scene.
            // Disable scene.
            // Murder new witness.
        }
        #endregion
    }
}
