using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Livesplit.DarkSouls100Tracker.Logic
{
    public class Requirement
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public int[] Progression { get; set; }
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
