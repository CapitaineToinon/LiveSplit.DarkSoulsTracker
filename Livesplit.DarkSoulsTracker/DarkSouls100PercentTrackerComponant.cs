using LiveSplit.Model;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LiveSplit.TimeFormatters;
using System.Drawing.Drawing2D;

namespace Livesplit.DarkSouls100PercentTracker
{
    public class DarkSoulsTrackerUIComponant : IComponent
    {
        protected InfoTextComponent InternalComponent { get; set; }
        public DarkSouls100PercentTrackerSettings Settings { get; set; }
        private DeltaTimeFormatter Formatter { get; set; }

        private LiveSplitState _state;
        private Tracker tracker;
        private DetailedView detailedView;

        private double _percentage = -1;
        private string _percentageString
        {
            get
            {
                if (_percentage == -1 || !tracker.IsThreadRunning)
                {
                    return "-";
                } 
                else if (Settings.Accuracy == TimeAccuracy.Seconds)
                {
                    return string.Format("{0}%", (Math.Truncate(_percentage)).ToString());
                }
                else if (Settings.Accuracy == TimeAccuracy.Tenths)
                {
                    double tmp = Math.Truncate(_percentage * 10);
                    return string.Format("{0}%", (tmp / 10).ToString("0.0"));
                }
                else
                {
                    double tmp = Math.Truncate(_percentage * 100);
                    return string.Format("{0}%", (tmp / 100).ToString("0.00"));
                }
            }
        }

        public float PaddingTop => InternalComponent.PaddingTop;
        public float PaddingLeft => InternalComponent.PaddingLeft;
        public float PaddingBottom => InternalComponent.PaddingBottom;
        public float PaddingRight => InternalComponent.PaddingRight;

        public IDictionary<string, Action> ContextMenuControls => null;

        public DarkSoulsTrackerUIComponant(LiveSplitState state)
        {
            Settings = new DarkSouls100PercentTrackerSettings()
            {
                CurrentState = state
            };
            Settings.OnToggleDetails += Settings_OnToggleDetails;

            this.InternalComponent = new InfoTextComponent("Progression", "-");
            
            

            tracker = new Tracker();
            tracker.OnPercentageUpdated += Tracker_PercentageUpdated;

            _state = state;
            _state.OnReset += _state_OnReset;
            _state.OnStart += _state_OnStart;
        }

        private void Settings_FontChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Settings_OnToggleDetails(object sender, EventArgs e)
        {
            if (detailedView == null)
            {
                detailedView = new DetailedView
                {
                    TextFont = InternalComponent.NameLabel,
                    // TODO : Font not working
                    TimesFont = InternalComponent.ValueLabel,
                };

                detailedView.OnClosed += DetailedView_OnClosed;
                detailedView.Show();
            } else
            {
                detailedView.Close();
                detailedView = null;
            }
        }

        private void DetailedView_OnClosed(object sender, EventArgs e)
        {
            detailedView.OnClosed -= DetailedView_OnClosed;
            detailedView = null;
        }

        private void Tracker_PercentageUpdated(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(Tracker))
            {
                Tracker t = (Tracker)sender;
                _percentage = t.TotalPercentage;

                if (detailedView != null)
                {
                    detailedView.DefeatedBossesCount = t.DefeatedBossesCount;
                    detailedView.ItemsPickedUp = t.ItemsPickedUp;
                    detailedView.DissolvedFoggatesCount = t.DissolvedFoggatesCount;
                    detailedView.RevealedIllusoryWallsCount = t.RevealedIllusoryWallsCount;
                    detailedView.UnlockedShortcutsAndLockedDoorsCount = t.UnlockedShortcutsAndLockedDoorsCount;
                    detailedView.CompletedQuestlinesCount = t.CompletedQuestlinesCount;
                    detailedView.KilledNonRespawningEnemiesCount = t.KilledNonRespawningEnemiesCount;
                    detailedView.FullyKindledBonfires = t.FullyKindledBonfires;
                    detailedView.Percentage = _percentageString;

                    detailedView.TextFont = InternalComponent.NameLabel;
                    // TODO : Font not working
                    detailedView.TimesFont = InternalComponent.ValueLabel;
                    detailedView.BackgroundColor = _state.LayoutSettings.BackgroundColor;
                }
            }
        }

        public string ComponentName
        {
            get { return "Dark Souls 100% Tracker"; }
        }

        private void _state_OnStart(object sender, EventArgs e)
        {
            _percentage = 0;
            tracker.Start();
        }

        void _state_OnReset(object sender, TimerPhase t)
        {
            tracker.Stop();
            _percentage = -1;
        }

        public void Dispose()
        {
            _state.OnReset -= _state_OnReset;
            _state.OnStart -= _state_OnStart;

            if (detailedView != null)
            {
                detailedView.Close();
                detailedView = null;
            }
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

            InternalComponent.NameLabel.Font = _state.LayoutSettings.TextFont;
            // TODO : Font not working
            InternalComponent.ValueLabel.Font = state.LayoutSettings.TimesFont;
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
