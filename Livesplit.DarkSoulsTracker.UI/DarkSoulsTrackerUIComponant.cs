using LiveSplit.Model;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using LiveSplit.TimeFormatters;
using System.Drawing.Drawing2D;

namespace Livesplit.DarkSoulsTracker.UI
{
    public class DarkSoulsTrackerUIComponant : IComponent
    {
        protected InfoTextComponent InternalComponent { get; set; }
        public DarkSoulsTrackerUISettings Settings { get; set; }
        private DeltaTimeFormatter Formatter { get; set; }

        public float PaddingTop => InternalComponent.PaddingTop;
        public float PaddingLeft => InternalComponent.PaddingLeft;
        public float PaddingBottom => InternalComponent.PaddingBottom;
        public float PaddingRight => InternalComponent.PaddingRight;

        public IDictionary<string, Action> ContextMenuControls => null;

        public DarkSoulsTrackerUIComponant(LiveSplitState state)
        {
            Settings = new DarkSoulsTrackerUISettings()
            {
                CurrentState = state
            };

            this.InternalComponent = new InfoTextComponent("Progression", "-");

            _state = state;
            _state.OnReset += _state_OnReset;
            _state.OnStart += _state_OnStart;
        }

        public string ComponentName
        {
            get { return "Dark Souls 100% Tracker UI"; }
        }

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
                    if (Settings.Accuracy == TimeAccuracy.Seconds)
                    
                        return string.Format("{0}%", MathFloorWithPrecision(_percentage, 0).ToString(CultureInfo.InvariantCulture));
                    else if (Settings.Accuracy == TimeAccuracy.Tenths)
                        return string.Format("{0}%", MathFloorWithPrecision(_percentage, 1).ToString(CultureInfo.InvariantCulture));
                    else
                        return string.Format("{0}%", MathFloorWithPrecision(_percentage, 2).ToString(CultureInfo.InvariantCulture));
            }
        }

        // Function made to floor the percentage but with decimals
        // Can't use Math.Round to avoid, for example, getting 99.9% rounded up to 100%
        // TODO : Fix the format so 0% is displayed at 0.0% or 0.00% etc
        private double MathFloorWithPrecision(double value, int precision)
        {
            switch (precision)
            {
                default:
                case 0:
                    return Math.Floor(value);
                case 1:
                    return (Math.Floor(value * 10)) / 10;
                case 2:
                    return (Math.Floor(value * 20)) / 20;
            }
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
            DrawBackground(g, state, width, VerticalHeight);
            PrepareDraw(state);
            InternalComponent.DrawVertical(g, state, width, region);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region region)
        {
            DrawBackground(g, state, HorizontalWidth, height);
            PrepareDraw(state);
            InternalComponent.DrawHorizontal(g, state, height, region);
        }

        private void DrawBackground(Graphics g, LiveSplitState state, float width, float height)
        {
            if (Settings.BackgroundColor.A > 0
                || Settings.BackgroundGradient != GradientType.Plain
                && Settings.BackgroundColor2.A > 0)
            {
                var gradientBrush = new LinearGradientBrush(
                            new PointF(0, 0),
                            Settings.BackgroundGradient == GradientType.Horizontal
                            ? new PointF(width, 0)
                            : new PointF(0, height),
                            Settings.BackgroundColor,
                            Settings.BackgroundGradient == GradientType.Plain
                            ? Settings.BackgroundColor
                            : Settings.BackgroundColor2);
                g.FillRectangle(gradientBrush, 0, 0, width, height);
            }
        }

        void PrepareDraw(LiveSplitState state)
        {
            InternalComponent.DisplayTwoRows = Settings.Display2Rows;

            InternalComponent.NameLabel.HasShadow
                = InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;

            InternalComponent.NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (invalidator != null && this.InternalComponent.InformationValue != _percentageString)
            {
                this.InternalComponent.InformationValue = _percentageString;
                invalidator.Invalidate(0f, 0f, width, height);
                InternalComponent.Update(invalidator, state, width, height, mode);
            }
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            Settings.Mode = mode;
            return Settings;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public float MinimumWidth { get { return this.InternalComponent.MinimumWidth; } }
        public float MinimumHeight { get { return this.InternalComponent.MinimumHeight; } }
        public float VerticalHeight { get { return this.InternalComponent.VerticalHeight; } }
        public float HorizontalWidth { get { return this.InternalComponent.HorizontalWidth; } }   
    }
}
