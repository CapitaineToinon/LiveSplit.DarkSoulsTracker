using System.Reflection;
using Livesplit.DarkSoulsTracker.UI;
using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;

[assembly: ComponentFactory(typeof(DarkSoulsTrackerUIFactory))]

namespace Livesplit.DarkSoulsTracker.UI
{
    class DarkSoulsTrackerUIFactory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "Dark Souls 100% UI"; }
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
            return new DarkSoulsTrackerUIComponant(state);
        }

        public string XMLURL
        {
            get { return ""; }
        }
    }
}
