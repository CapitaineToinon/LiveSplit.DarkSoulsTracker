using System.Reflection;
using Livesplit.DarkSoulsTracker;
using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;


[assembly: ComponentFactory(typeof(DarkSoulsTrackerFactory))]

namespace Livesplit.DarkSoulsTracker
{
    public class DarkSoulsTrackerFactory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "Dark Souls 100%"; }
        }

        public string Description
        {
            get { return ""; }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Control; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new DarkSoulsTrackerComponant(state);
        }

        public string UpdateName
        {
            get { return this.ComponentName; }
        }

        public string UpdateURL
        {
            get { return ""; }
        }

        public Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public string XMLURL
        {
            get { return ""; }
        }
    }
}
