using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.DarkSoulsTracker.LiveSplit
{
    class DarkSoulsTracker : IComponent
    {
        protected InfoTextComponent /*PercentageTextComponent*/ InternalComponent { get; set; }
        public string ComponentName => "Dark Souls 100% Tracker";
        public LiveSplitState state;
        public DarkSoulsProcess darksoulsProcess;
        public DarkSoulsProgress soulsProgress;
        public int TestValue = 0;

        public DarkSoulsTracker(LiveSplitState state)
        {
            this.state = state;
            this.state.OnStart += State_OnStart;
            this.state.OnReset += State_OnReset;
            this.darksoulsProcess = new DarkSoulsProcess();
            this.soulsProgress = darksoulsProcess.DarkSoulsProgress;
            this.InternalComponent = new InfoTextComponent("Progression", TestValue.ToString());
        }

        private void State_OnStart(object sender, EventArgs e)
        {
            darksoulsProcess.Start();
        }

        private void State_OnReset(object sender, TimerPhase value)
        {
            darksoulsProcess.Stop();
        }

        /// <summary>
        /// Main update loop
        /// </summary>
        /// <param name="invalidator"></param>
        /// <param name="state"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (invalidator != null)
            {
                InternalComponent.InformationValue = soulsProgress.Percentage.ToString();
                invalidator.Invalidate(0f, 0f, width, height);
                InternalComponent.Update(invalidator, state, width, height, mode);
            }
        }   

        #region [Interface Properties]
        public float MinimumWidth { get { return InternalComponent.MinimumWidth; } }
        public float MinimumHeight { get { return InternalComponent.MinimumHeight; } }
        public float VerticalHeight { get { return InternalComponent.VerticalHeight; } }
        public float HorizontalWidth { get { return InternalComponent.HorizontalWidth; } }
        public float PaddingTop => InternalComponent.PaddingTop;
        public float PaddingLeft => InternalComponent.PaddingLeft;
        public float PaddingBottom => InternalComponent.PaddingBottom;
        public float PaddingRight => InternalComponent.PaddingRight;

        public IDictionary<string, Action> ContextMenuControls => null;
        #endregion

        #region [Interface methods]
        public void Dispose()
        {
            // TODO
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
            //if (Settings.BackgroundColor.A > 0
            //    || Settings.BackgroundGradient != GradientType.Plain
            //    && Settings.BackgroundColor2.A > 0)
            //{
            //    var gradientBrush = new LinearGradientBrush(
            //                new PointF(0, 0),
            //                Settings.BackgroundGradient == GradientType.Horizontal
            //                ? new PointF(width, 0)
            //                : new PointF(0, height),
            //                Settings.BackgroundColor,
            //                Settings.BackgroundGradient == GradientType.Plain
            //                ? Settings.BackgroundColor
            //                : Settings.BackgroundColor2);
            //    g.FillRectangle(gradientBrush, 0, 0, width, height);
            //}

            var gradientBrush = new LinearGradientBrush(
                            new PointF(0, 0),
                            new PointF(width, 0),
                            Color.Black,
                            Color.Black);

            g.FillRectangle(gradientBrush, 0, 0, width, height);
   
        }

        void PrepareDraw(LiveSplitState state)
        {
            InternalComponent.NameLabel.HasShadow
                = InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;

            InternalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return null;
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return null;
        }

        public void SetSettings(XmlNode settings)
        {

        }
        #endregion
    }
}
