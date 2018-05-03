using LiveSplit.DarkSoulsTracker.LiveSplit;
using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: ComponentFactory(typeof(DarkSouls100TrackerFactory))]

namespace LiveSplit.DarkSoulsTracker.LiveSplit
{
    class DarkSouls100TrackerFactory : IComponentFactory
    {
        public string ComponentName => "Dark Souls 100% Tracker";

        public string Description => "Memory Tracker for the Dark Souls 100% speedrun.";

        public ComponentCategory Category => ComponentCategory.Information;

        public string UpdateName => ComponentName;

        public string XMLURL => "";

        public string UpdateURL => "https://twitter.com/CapitaineToinon";

        public Version Version => new Version("1.0");

        public IComponent Create(LiveSplitState state)
        {
            return new DarkSoulsTracker(state);
        }
    }
}
