using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LiveSplit.TimeFormatters;
using System.Drawing.Drawing2D;
using CapitaineToinon.DarkSoulsMemory;
using Livesplit.DarkSouls100Tracker;

namespace LiveSplit.UI.Components
{
    public class DarkSouls100Tracker : IComponent
    {
        protected PercentageTextComponent InternalComponent { get; set; }
        public DarkSouls100TrackerSettings Settings { get; set; }
        private DeltaTimeFormatter Formatter { get; set; }

        private LiveSplitState state;
        private DarkSoulsMemory tracker;
        private GameProgress progress;
        private DetailedView detailedView;
        private bool firstSettings = true;

        public IDictionary<string, Action> ContextMenuControls => null;

        public DarkSouls100Tracker(LiveSplitState state)
        {
            // Sets the initial varialbes
            this.state = state;
            tracker = new DarkSoulsMemory();
            progress = new GameProgress();

            // Subscribe to the gameProgress event
            tracker.OnGameProgressUpdated += GameTracker_OnGameProgressUpdated;

            // Subscribe to the Start and Reset events of Livesplit
            this.state.OnReset += _state_OnReset;
            this.state.OnStart += _state_OnStart;

            // Create the settings and subscribe to the Events mean to update settings and the detailed view
            Settings = new DarkSouls100TrackerSettings()
            {
                CurrentState = state
            };
            Settings.OnDetailedSettingsChanged += Settings_OnDetailedSettingsChanged;
            Settings.OnToggleDetails += Settings_OnToggleDetails;
            Settings.OnSettingsLoaded += Settings_OnSettingsLoaded;

            // Creates the Text component (variant of the normal Text componant with different Font behavior)
            this.InternalComponent = new PercentageTextComponent("Progression", "-", Settings);

            // Starts the splits if the timer is already running
            if (state.IsGameTimeInitialized)
                tracker.Start();
        }

        private void _state_OnStart(object sender, EventArgs e)
        {
            tracker.Start();
        }

        void _state_OnReset(object sender, TimerPhase t)
        {
            tracker.Quit();
        }

        private void GameTracker_OnGameProgressUpdated(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(GameProgress))
            {
                progress = (GameProgress)sender;

                if (detailedView != null)
                {
                    detailedView.GameProgress = progress;
                }
            }
        }

        public void Dispose()
        {
            state.OnReset -= _state_OnReset;
            state.OnStart -= _state_OnStart;

            tracker.OnGameProgressUpdated -= GameTracker_OnGameProgressUpdated;
            tracker.Quit();

            if (detailedView != null)
            {
                detailedView.Close();
                detailedView = null;
            }
        }

        private void Settings_OnSettingsLoaded(object sender, EventArgs e)
        {
            if (firstSettings && Settings.OpenAtLaunch)
            {
                ToggleDetailedView();
            }
            firstSettings = false;
        }

        private void Settings_OnDetailedSettingsChanged(object sender, EventArgs e)
        {
            if (detailedView != null)
            {
                detailedView.Accuracy = Settings.Accuracy;
                detailedView.ShowPercentage = Settings.ShowPercentage;
                detailedView.DarkTheme = Settings.DarkTheme;
            }
        }

        private void Settings_OnToggleDetails(object sender, EventArgs e)
        {
            ToggleDetailedView();
        }

        private void ToggleDetailedView()
        {
            if (detailedView == null)
            {
                detailedView = new DetailedView()
                {
                    Accuracy = Settings.Accuracy,
                    ShowPercentage = Settings.ShowPercentage,
                    DarkTheme = Settings.DarkTheme,
                };

                detailedView.Left = Settings.DetailedTrackerX;
                detailedView.Top = Settings.DetailedTrackerY;

                detailedView.Show();

                detailedView.OnClosed += DetailedView_OnClosed;
                detailedView.OnLocationChanged += DetailedView_OnLocationChanged;

                detailedView.GameProgress = progress;
            }
            else
            {
                detailedView.Close();
                detailedView = null;
            }
        }

        private void DetailedView_OnLocationChanged(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(DetailedView))
            {
                DetailedView d = (DetailedView)sender;
                Settings.DetailedTrackerX = d.Left;
                Settings.DetailedTrackerY = d.Top;
            }
        }

        private void DetailedView_OnClosed(object sender, EventArgs e)
        {
            detailedView.OnClosed -= DetailedView_OnClosed;
            Settings.DetailedTrackerX = detailedView.Left;
            Settings.DetailedTrackerY = detailedView.Top;
            detailedView = null;
        }

        #region Useless shit
        public string ComponentName
        {
            get { return "Dark Souls 100% Tracker"; }
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
            InternalComponent.ValueLabel.ForeColor = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            string textPercentage = (tracker.IsRunning) ? PercentageFormatter.Format(progress.Percentage, Settings.Accuracy) : "-";
            if (invalidator != null && this.InternalComponent.InformationValue != textPercentage)
            {
                this.InternalComponent.InformationValue = textPercentage;
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
        public float PaddingTop => InternalComponent.PaddingTop;
        public float PaddingLeft => InternalComponent.PaddingLeft;
        public float PaddingBottom => InternalComponent.PaddingBottom;
        public float PaddingRight => InternalComponent.PaddingRight;
        #endregion
    }
}
