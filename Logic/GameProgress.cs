using LiveSplit.TimeFormatters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Livesplit.DarkSouls100Tracker.Logic
{
    public class GameProgress
    {
        public List<Requirement> Requirements { get; set; }
        public TimeAccuracy TimeAccuracy;
        public double Percentage { get; set; }
        public string PercentageS
        {
            get
            {
                if (TimeAccuracy == TimeAccuracy.Seconds)
                {
                    return string.Format("{0}%", (Math.Truncate(Percentage)).ToString());
                }
                else if (TimeAccuracy == TimeAccuracy.Tenths)
                {
                    double tmp = Math.Truncate(Percentage * 10);
                    return string.Format("{0}%", (tmp / 10).ToString("0.0"));
                }
                else
                {
                    double tmp = Math.Truncate(Percentage * 100);
                    return string.Format("{0}%", (tmp / 100).ToString("0.00"));
                }
            }
        }

        public GameProgress(List<Requirement> requirements)
        {
            this.Requirements = requirements;
            this.Percentage = 0;
            this.TimeAccuracy = TimeAccuracy.Seconds;
        }

        public GameProgress() : this(new List<Requirement>()
            {
                { new Requirement("Treasure Locations", 0.2, () => { return new int[] { 0, 1 }; }) },
                { new Requirement("Bosses", 0.25, () => { return new int[] { 0, 1 }; }) },
                { new Requirement("Non-respawning Enemies", 0.15, () => { return new int[] { 0, 1 }; }) },
                { new Requirement("NPC Questlines", 0.2, () => { return new int[] { 0, 1 }; }) },
                { new Requirement("Shortcuts / Locked Doors", 0.1, () => { return new int[] { 0, 1 }; }) },
                { new Requirement("Illusory Walls", 0.025, () => { return new int[] { 0, 1 }; }) },
                { new Requirement("Foggates", 025, () => { return new int[] { 0, 1 }; }) },
                { new Requirement("Kindled Bonfires", 0.05, () => { return new int[] { 0, 1 }; }) },
            })
        {

        }

        public void UpdatePercentage()
        {
            double p = 0;
            if (Requirements != null)
            {
                foreach (Requirement r in Requirements)
                {
                    p += r.Progression[0] * (r.Weight / r.Progression[1]);
                }
            }
            Percentage = p;
        }
    }
}
