using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    /// <summary>
    /// Dynamically adjusts the difficulty, depending on how well the player is doing.
    /// Higher difficultyValue makes for a harder game. Every increment introduces new obstacles, while every decrement removes some of these obstacles.
    /// Max difficulty is 5, minimum is -5.
    /// 
    /// difficultyValue can only differ from Difficulty by a maximum 3 points.
    /// Higher difficulty should tip the scales in favour of a higher difficulty value.
    /// </summary>
    public class AIDirector {
        private static AIDirector _instance;
        public static AIDirector Instance {
            get {
                if (_instance == null) _instance = new AIDirector();
                return _instance;
            }
        }

        private AIDirector() { }

        /* BASE PARAMETERS*/
        private float difficultyValue = 0;
        private readonly List<Clue> hardClues = new List<Clue>();
        public List<Clue> HardClues { get { return hardClues; } }
        private readonly List<Clue> dailyClues = new List<Clue>();
        public List<Clue> DailyClues { get { return dailyClues; } }

        /* PARAMETERS FROM PLAYERCONTROLLER */
        private int Difficulty { get { return PlayerController.Instance.Difficulty; } }
        public int NumberOfDescriptiveClues {
            get { return PlayerController.Instance.NumberOfDescriptiveClues; }
            set { PlayerController.Instance.NumberOfDescriptiveClues = value; }
        }

        #region Parameters
        #region Liars
        public float PercentageLiars = 0.4f;
        public float PercentageDescriptiveLiars = 0.2f;
        public int HidingDescriptiveNPCs = 0;
        #endregion
        
        #region Mingling
        public float MingleValue = 2.5f;
        public int NumMinglers = 3;
        #endregion

        #region Clues
        public int PeopleLocationClues = 2;
        public int MurderLocationClues = 2;
        public int Pointers = 4;
        #endregion
        #endregion

        /// <summary>
        /// Calculates a new difficulty and changes game parameters accordingly.
        /// Since AIDirector yields to Difficulty parameter, AIDirector will never change difficulty by more than 2 points.
        /// </summary>
        public void CalculateDifficulty() {
            difficultyValue = Difficulty - 5;
            var difficultyModifier = (HardClues.Count * (5f / 2f) + (HardClues.Count * (2f / 5f)) * (float)(PlayerController.Instance.Rng.NextDouble() + 0.5f));
            difficultyModifier = difficultyModifier >= 0 ? Math.Min(difficultyModifier, 2) : Math.Max(difficultyModifier, -2);
            difficultyValue += difficultyModifier;

            SetDifficulty((int)difficultyValue);
            CleanDay();
        }

        /// <summary>
        /// Default Values:
        ///     - NumMinglers                   = 3
        ///     - PercentageLiars               = 40 %
        ///     - PercentageDescriptiveLiars    = 20 %
        ///     - MingleValue                   = 2.5
        ///     - NumDescriptiveClues           = 3
        ///     - NumHidingDescriptive          = 0 : NumDescriptiveClues
        /// Max Values:
        ///     - NumMinglers                   = 5
        ///     - PercentageLiars               = 75 %
        ///     - PercentageDescriptiveLiars    = 40 %
        ///     - MingleValue                   = 4.0
        ///     - NumDescriptiveClues           = 5
        ///     - NumHidingDescriptive          = (0 : NumDescriptiveClues) - 1 + difficultyValue/2
        /// Min Values
        ///     - NumMinglers                   = 1
        ///     - PercentageLiars               = 5 %
        ///     - PercentageDescriptiveLiars    = 0 %
        ///     - MingleValue                   = 1.0
        ///     - NumDescriptiveClues           = 1
        ///     - NumHidingDescriptive          = 0 : (NumDescriptiveClues / 2)
        /// </summary>
        /// <param name="difficultyValue">The current difficulty.</param>
        private void SetDifficulty(int difficultyValue) {
            if (difficultyValue == 0) { // Set to default values.
                NumMinglers = 3;
                PercentageLiars = 0.4f;
                PercentageDescriptiveLiars = 0.2f;
                MingleValue = 2.5f;
                NumberOfDescriptiveClues = 3;
                HidingDescriptiveNPCs = PlayerController.Instance.Rng.Next(NumberOfDescriptiveClues);
            } else if (difficultyValue > 0) {
                NumMinglers = 3 - difficultyValue / 2;
                PercentageLiars = 0.075f * difficultyValue + 0.375f;
                PercentageDescriptiveLiars = 0.04f * difficultyValue + 0.2f;
                MingleValue = 2.5f - (0.03f * difficultyValue);
                NumberOfDescriptiveClues = (int)(-(1f / 3f) * difficultyValue + (2f + 2f / 3f));
                HidingDescriptiveNPCs = PlayerController.Instance.Rng.Next(NumberOfDescriptiveClues) - 1 + (int)Math.Ceiling(difficultyValue / 2.0);
            } else {
                NumMinglers = 3 - difficultyValue / 2;
                PercentageLiars = 0.075f * difficultyValue - 0.425f;
                PercentageDescriptiveLiars = 0.2f + (0.04f * difficultyValue);
                MingleValue = 2.5f - (0.03f * difficultyValue);
                NumberOfDescriptiveClues = (int)(-(1f / 3f) * difficultyValue + (2f + 2f / 3f));
                HidingDescriptiveNPCs = PlayerController.Instance.Rng.Next(Math.Abs(NumberOfDescriptiveClues) / 2);
            }
        }

        public void CleanDay() {
            HardClues.Clear();
            DailyClues.Clear();
        }
    }
}
