namespace LiveSplit.UI.Components
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
            NameMeasureLabel.Font = /*Settings.OverrideFont1 ? Settings.Font1 :*/ state.LayoutSettings.TextFont;
            ValueLabel.Font = /*Settings.OverrideFont2 ? Settings.Font2 :*/ state.LayoutSettings.TimesFont;
            NameLabel.Font = /*Settings.OverrideFont1 ? Settings.Font1 :*/ state.LayoutSettings.TextFont;
        }
    }
}