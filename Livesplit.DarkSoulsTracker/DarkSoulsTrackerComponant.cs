using LiveSplit.Model;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System.Xml;
using System;
using System.Linq;
using Livesplit.DarkSoulsTracker.UI;
using System.Windows.Forms;

namespace Livesplit.DarkSoulsTracker
{
    public class DarkSoulsTrackerComponant : LogicComponent
    {
        #region Variables
        private TimerModel _timer;
        private LiveSplitState _state;
        private GameMemory _gameMemory;

        #endregion

        private DarkSoulsTrackerUIComponant UI
        {
            get
            {
                return _state.Layout.Components.FirstOrDefault(
                    c => c.GetType() == typeof(DarkSoulsTrackerUIComponant)) as DarkSoulsTrackerUIComponant;
            }
        }

        public override string ComponentName
        {
            get { return "Dark Souls 100% Tracker"; }
        }

        public DarkSoulsTrackerComponant(LiveSplitState state)
        {
            _state = state;
            _timer = new TimerModel();
            _timer.CurrentState = state;

            _gameMemory = new GameMemory();
            _gameMemory.UpdatePercentage += gameMemory_UpdatePercentage;
            _state.OnStart += gameMemory_OnStart;
            _state.OnReset += gameMemory_OnReset;
        }

        private void _gameMemory_UpdateDebug(object sender, EventArgs e)
        {
            if (sender is string s)
            {
                this.UI.DebugState(s);
            }    
        }

        ~DarkSoulsTrackerComponant()
        {
            _state.OnStart -= gameMemory_OnStart;
            _state.OnReset -= gameMemory_OnReset;
        }

        private void gameMemory_OnStart(object sender, EventArgs e)
        {
            if (_gameMemory != null)
                _gameMemory.StartReading();
        }

        private void gameMemory_OnReset(object sender, TimerPhase value)
        {
            if (_gameMemory != null)
                _gameMemory.Stop();
        }

        void gameMemory_UpdatePercentage(object sender, EventArgs e)
        {
            if (sender is double p)
            {
                if (_state.CurrentPhase != TimerPhase.NotRunning && _state.CurrentPhase != TimerPhase.Ended)
                {
                    if (this.UI != null)
                        this.UI.UpdatePercentage(p);
                }
            }
        }

        public override void Dispose()
        {
            if (_gameMemory != null)
                _gameMemory.Stop();
        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return document.CreateElement("Settings");
        }

        public override System.Windows.Forms.Control GetSettingsControl(LayoutMode mode)
        {
            return null;
        }

        public override void SetSettings(XmlNode settings)
        {
            
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            
        }
    }
}
