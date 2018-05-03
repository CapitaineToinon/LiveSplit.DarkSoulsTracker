using System;
using System.Collections.Generic;

namespace LiveSplit.DarkSoulsTracker
{
    public class Requirement
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public List<int> Flags { get; set; }
        public int[] Progress { get; set; }

        public override string ToString()
        {
            return string.Format("{0} / {1}", Progress[0], Progress[1]);
        }

        public Func<int[]> Callback { get; set; }

        public Requirement(string Name, double Weight, List<int> Flags, Func<int[]> Callback)
        {
            this.Name = Name;
            this.Flags = Flags;
            this.Weight = Weight;
            this.Progress = new int[] { 0, 1 };
            this.Callback = Callback;
        }

        public static Func<int[]> DefaultCallback
        {
            get
            {
                // Default callback for a requirement. Never really used but might as well keep it
                return new Func<int[]>(() => { return new int[] { 0, 1 }; });
            }
        }
    }
}
