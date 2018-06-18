using LiveSplit.UI;
using LiveSplit.UI.Components;
using System.Drawing;

namespace LiveSplit.DarkSoulsTracker
{
    public class PercentageTextComponent : InfoTextComponent
    {
        public DarkSouls100TrackerSettings Settings { get; set; }

        public PercentageTextComponent(string Name, string Value, DarkSouls100TrackerSettings settings)
            : base(Name, Value)
        {
            Settings = settings;
        }

        public override void PrepareDraw(Model.LiveSplitState state, LayoutMode mode)
        {
            NameMeasureLabel.Font = state.LayoutSettings.TextFont;
            ValueLabel.Font = state.LayoutSettings.TimesFont;
            NameLabel.Font = state.LayoutSettings.TextFont;
            if (mode == LayoutMode.Vertical)
            {
                NameLabel.VerticalAlignment = StringAlignment.Center;
                ValueLabel.VerticalAlignment = StringAlignment.Center;
            }
            else
            {
                NameLabel.VerticalAlignment = StringAlignment.Near;
                ValueLabel.VerticalAlignment = StringAlignment.Far;
            }
        }
    }
}
