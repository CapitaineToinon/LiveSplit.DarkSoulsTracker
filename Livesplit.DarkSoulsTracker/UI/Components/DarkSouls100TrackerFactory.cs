using System.Reflection;
using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;

[assembly: ComponentFactory(typeof(DarkSouls100PercentTrackerFactory))]

namespace LiveSplit.UI.Components
{
    public class DarkSouls100PercentTrackerFactory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "Dark Souls 100% Tracker"; }
        }

        public string Description
        {
            get { return "Memory Tracker for Dark Souls 100%."; }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Information; }
        }

        public string UpdateName
        {
            get { return this.ComponentName; }
        }

        public string UpdateURL
        {
            get { return "https://twitter.com/CapitaineToinon"; }
        }

        public Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new DarkSouls100Tracker(state);
        }

        public string XMLURL
        {
            get { return ""; }
        }
    }
}
