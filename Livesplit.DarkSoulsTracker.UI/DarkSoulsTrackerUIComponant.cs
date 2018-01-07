using LiveSplit.Model;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace Livesplit.DarkSoulsTracker.UI
{
    public class DarkSoulsTrackerUIComponant : IComponent
    {
        public string ComponentName
        {
            get { return "Dark Souls 100% Tracker UI"; }
        }

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }
        protected InfoTextComponent InternalComponent;

        private LiveSplitState _state;
        private string debugState;
        private double _percentage = -1;
        private string _percentageString
        {
            get
            {
                if (_percentage == -1)
                    return "-";
                else
                    return string.Format("{0}%", _percentage.ToString(CultureInfo.InvariantCulture));
            }
        }

        public DarkSoulsTrackerUIComponant(LiveSplitState state)
        {

            this.ContextMenuControls = new Dictionary<String, Action>();
            this.InternalComponent = new InfoTextComponent("100% Progression", "-");

            _state = state;
            _state.OnReset += _state_OnReset;
            _state.OnStart += _state_OnStart;
        }

        public void DebugState(string s)
        {
            debugState = s;
        } 

        private void _state_OnStart(object sender, EventArgs e)
        {
            _percentage = 0;
        }

        void _state_OnReset(object sender, TimerPhase t)
        {
            _percentage = -1;
        }

        public void Dispose()
        {
            _state.OnReset -= _state_OnReset;
            _state.OnStart -= _state_OnStart;
        }

        public void UpdatePercentage(double p)
        {
            _percentage = p;
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region region)
        {
            this.PrepareDraw(state);
            this.InternalComponent.DrawVertical(g, state, width, region);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region region)
        {
            this.PrepareDraw(state);
            this.InternalComponent.DrawHorizontal(g, state, height, region);
        }

        void PrepareDraw(LiveSplitState state)
        {
            this.InternalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            this.InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
            this.InternalComponent.NameLabel.HasShadow
                = this.InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (invalidator != null && this.InternalComponent.InformationValue != _percentageString)
            {
                this.InternalComponent.InformationValue = _percentageString;
                invalidator.Invalidate(0f, 0f, width, height);
            }
        }

        public XmlNode GetSettings(XmlDocument document) { return document.CreateElement("Settings"); }
        public Control GetSettingsControl(LayoutMode mode) { return null; }
        public void SetSettings(XmlNode settings) { }

        public float MinimumWidth { get { return this.InternalComponent.MinimumWidth; } }
        public float MinimumHeight { get { return this.InternalComponent.MinimumHeight; } }
        public float VerticalHeight { get { return this.InternalComponent.VerticalHeight; } }
        public float HorizontalWidth { get { return this.InternalComponent.HorizontalWidth; } }
        public float PaddingLeft { get { return this.InternalComponent.PaddingLeft; } }
        public float PaddingRight { get { return this.InternalComponent.PaddingRight; } }
        public float PaddingTop { get { return this.InternalComponent.PaddingTop; } }
        public float PaddingBottom { get { return this.InternalComponent.PaddingBottom; } }
    }
}
