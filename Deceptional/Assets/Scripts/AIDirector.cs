using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    /// <summary>
    /// Dynamically adjusts the difficulty, depending on how well the player is doing.
    /// Higher difficultyValue makes for a harder game. Every increment introduces new obstacles, while every decrement removes some of these obstacles.
    /// Max difficulty is 5, minimum is -5.
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

        private float difficultyValue = 0;

        public int Difficulty = 0;

        public int HidingDescriptiveNPCs = 0;

        public int MinLiars = 0;
        public float PercentageLiars = 0.4f;
        public float PercentageDescriptiveLiars = 0.2f;

        public int NumDescriptiveClues = 3;
        
        public int NumMinglers = 3;
        public float MingleValue = 2.5f;

        private readonly List<Clue> hardClues = new List<Clue>();
        public List<Clue> HardClues { get { return hardClues; } }
        private readonly List<Clue> dailyClues = new List<Clue>();
        public List<Clue> DailyClues { get { return dailyClues; } }

        public void CalculateDifficulty() {
            difficultyValue = Difficulty;
            difficultyValue += HardClues.Count / 2f;
            difficultyValue -= (DailyClues.Count - 5f) / 5f;
            
            if (difficultyValue > 0) { 
                for (float innerDifficulty = 0; innerDifficulty < difficultyValue; innerDifficulty++) {
                    if (NumMinglers > 1) NumMinglers--;
                    if (MingleValue > 0.5f) MingleValue -= 0.5f;
                    if (NumDescriptiveClues > 1) NumDescriptiveClues--;
                    if (PercentageLiars < 0.7f) PercentageLiars += 0.06f;
                    if (HidingDescriptiveNPCs <= NumDescriptiveClues / 2) HidingDescriptiveNPCs++;
                    if (PercentageDescriptiveLiars < 0.4f) PercentageDescriptiveLiars += 0.05f;

                }
            } else if (difficultyValue < 0) {
                for (float innerDifficulty = 0; innerDifficulty > difficultyValue; innerDifficulty--) {
                    if (NumMinglers < 5) NumMinglers++;
                    if (MingleValue < 5) MingleValue += 0.5f;
                    if (NumDescriptiveClues < 6) NumDescriptiveClues++;
                    if (PercentageLiars > 0.1f) PercentageLiars -= 0.06f;
                    if (HidingDescriptiveNPCs >= NumDescriptiveClues / 2) HidingDescriptiveNPCs--;
                    if (PercentageDescriptiveLiars > 0) PercentageDescriptiveLiars -= 0.05f;

                }
            } else {
                PercentageLiars = 0.4f;
                PercentageDescriptiveLiars = 0.2f;
                NumDescriptiveClues = 3;
                NumMinglers = 3;
                MingleValue = 2.5f;
                HidingDescriptiveNPCs = 0;
            }
        }
    }
}
