using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.TimeFormatters;
using LiveSplit.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class DarkSouls100TrackerSettings : UserControl
    {
        public event EventHandler OnToggleDetails;
        public event EventHandler OnDetailedSettingsChanged;

        public Color TextColor { get; set; }
        public bool OverrideTextColor { get; set; }
        public TimeAccuracy Accuracy { get; set; }
        public bool ShowPercentage { get; set; }

        public Color BackgroundColor { get; set; }

        public LiveSplitState CurrentState { get; set; }
        public bool Display2Rows { get; set; }

        public LayoutMode Mode { get; set; }

        public DarkSouls100TrackerSettings()
        {
            InitializeComponent();

            TextColor = Color.FromArgb(255, 255, 255);
            OverrideTextColor = false;
            Accuracy = TimeAccuracy.Hundredths;
            BackgroundColor = Color.Transparent;
            Display2Rows = false;
            ShowPercentage = true;

            chkOverrideTextColor.DataBindings.Add("Checked", this, "OverrideTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnTextColor.DataBindings.Add("BackColor", this, "TextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkShowPercentage.DataBindings.Add("Checked", this, "ShowPercentage", true, DataSourceUpdateMode.OnPropertyChanged);
        }

        void chkOverrideTextColor_CheckedChanged(object sender, EventArgs e)
        {
            label1.Enabled = btnTextColor.Enabled = chkOverrideTextColor.Checked;
        }

        void DeltaSettings_Load(object sender, EventArgs e)
        {
            chkOverrideTextColor_CheckedChanged(null, null);
            rdoSeconds.Checked = Accuracy == TimeAccuracy.Seconds;
            rdoTenths.Checked = Accuracy == TimeAccuracy.Tenths;
            rdoHundredths.Checked = Accuracy == TimeAccuracy.Hundredths;
            if (Mode == LayoutMode.Horizontal)
            {
                chkTwoRows.Enabled = false;
                chkTwoRows.DataBindings.Clear();
                chkTwoRows.Checked = true;
            }
            else
            {
                chkTwoRows.Enabled = true;
                chkTwoRows.DataBindings.Clear();
                chkTwoRows.DataBindings.Add("Checked", this, "Display2Rows", false, DataSourceUpdateMode.OnPropertyChanged);
            }
        }

        void rdoHundredths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void rdoSeconds_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void UpdateAccuracy()
        {
            if (rdoSeconds.Checked)
                Accuracy = TimeAccuracy.Seconds;
            else if (rdoTenths.Checked)
                Accuracy = TimeAccuracy.Tenths;
            else
                Accuracy = TimeAccuracy.Hundredths;
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            TextColor = SettingsHelper.ParseColor(element["TextColor"]);
            OverrideTextColor = SettingsHelper.ParseBool(element["OverrideTextColor"]);
            Accuracy = SettingsHelper.ParseEnum<TimeAccuracy>(element["Accuracy"]);
            BackgroundColor = SettingsHelper.ParseColor(element["BackgroundColor"]);
            Display2Rows = SettingsHelper.ParseBool(element["Display2Rows"]);
            ShowPercentage = SettingsHelper.ParseBool(element["ShowPercentage"]);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            return parent;
        }

        public int GetSettingsHashCode()
        {
            return CreateSettingsNode(null, null);
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "Version", "1.4") ^
            SettingsHelper.CreateSetting(document, parent, "TextColor", TextColor) ^
            SettingsHelper.CreateSetting(document, parent, "OverrideTextColor", OverrideTextColor) ^
            SettingsHelper.CreateSetting(document, parent, "Accuracy", Accuracy) ^
            SettingsHelper.CreateSetting(document, parent, "BackgroundColor", BackgroundColor) ^
            SettingsHelper.CreateSetting(document, parent, "Display2Rows", Display2Rows) ^
            SettingsHelper.CreateSetting(document, parent, "ShowPercentage", ShowPercentage);
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            SettingsHelper.ColorButtonClick((Button)sender, this);
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            this.OnToggleDetails(sender, EventArgs.Empty);
        }

        private void chkShowPercentage_CheckedChanged(object sender, EventArgs e)
        {
            this.OnDetailedSettingsChanged(sender, EventArgs.Empty);
        }
    }
}


