using System;
using System.Collections.Generic;

namespace CapitaineToinon.DarkSoulsMemory
{
    public class GameProgress
    {
        internal event EventHandler OnGameProgressUpdated;
        public bool Completed
        {
            get
            {
                return (Percentage == 100);
            }
        }
        public List<Requirement> Requirements { get; set; }
        private double percentage;
        public double Percentage
        {
            get
            {
                return percentage;
            }
            set
            {
                if (value != percentage)
                {
                    value = (value < 0) ? 0 : value;
                    percentage = value;
                    this.OnGameProgressUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
        }


        private static Func<int[]> DefaultCallback
        {
            get
            {
                // Default callback for a requirement. Never really used but might as well keep it
                return new Func<int[]>(() => { return new int[] { 0, 1 }; });
            }
        }

        public GameProgress(List<Requirement> requirements)
        {
            this.Requirements = requirements;
            this.Percentage = 0;
        }

        public GameProgress() : this(new List<Requirement>()
            {
                { new Requirement("Treasure Locations", 0.2, DefaultCallback) },
                { new Requirement("Bosses", 0.25, DefaultCallback) },
                { new Requirement("Non-respawning Enemies", 0.15, DefaultCallback) },
                { new Requirement("NPC Questlines", 0.2, DefaultCallback) },
                { new Requirement("Shortcuts / Locked Doors", 0.1, DefaultCallback) },
                { new Requirement("Illusory Walls", 0.025, DefaultCallback) },
                { new Requirement("Foggates", 025, DefaultCallback) },
                { new Requirement("Kindled Bonfires", 0.05, DefaultCallback) },
            })
        {

        }

        public void Reset()
        {
            // Resets all the requirements. Sets -1 to be sure the percentage
            // will also be updated
            Requirements.ForEach(r => r.Progression = new int[] { 0, 1 });
            Percentage = -1;
        }

        public void UpdatePercentage(Func<bool> IsGameStillRunning)
        {
            if (Requirements != null)
            {
                List<Requirement> tmpRequirements = Requirements;
                double p = 0;
                foreach (Requirement r in tmpRequirements)
                {
                    // Call the function assigned to the requirement
                    r.Progression = r.Callback();
                    double tmpPercentage = r.Progression[0] * (r.Weight / r.Progression[1]);
                    p += tmpPercentage;
                }
                p *= 100;

                // Only updates if the game is still running. The game could have been closed while 
                // we were checking the values and thus could be wrong.
                if (IsGameStillRunning())
                {
                    Requirements = tmpRequirements;
                    Percentage = p;
                }
            }
            
        }
    }

    public class Requirement
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public int[] Progression { get; set; }
        public string ProgressionS
        {
            get
            {
                return string.Format("{0} / {1}", Progression[0], Progression[1]);
            }
        }
        public Func<int[]> Callback { get; set; }

        public Requirement(string Name, double Weight, Func<int[]> Callback)
        {
            this.Name = Name;
            this.Weight = Weight;
            this.Progression = new int[] { 0, 1 };
            this.Callback = Callback;
        }
    }
}
