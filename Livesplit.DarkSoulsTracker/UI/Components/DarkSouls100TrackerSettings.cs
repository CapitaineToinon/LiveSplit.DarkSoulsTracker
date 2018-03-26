using LiveSplit.Model;
using LiveSplit.TimeFormatters;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class DarkSouls100TrackerSettings : UserControl
    {
        public event EventHandler OnToggleDetails;
        public event EventHandler OnDetailedSettingsChanged;
        public event EventHandler OnSettingsLoaded;

        public Color TextColor { get; set; }
        public bool OverrideTextColor { get; set; }

        private TimeAccuracy accuracy;
        private bool showPercetnage;
        private bool darkTheme;

        public TimeAccuracy Accuracy
        {
            get
            {
                return accuracy;
            }
            set
            {
                accuracy = value;
                this.OnDetailedSettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public bool ShowPercentage
        {
            get
            {
                return showPercetnage;
            }
            set
            {
                showPercetnage = value;
                this.OnDetailedSettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public bool DarkTheme
        {
            get
            {
                return darkTheme;
            }
            set
            {
                darkTheme = value;
                this.OnDetailedSettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public bool OpenAtLaunch { get; set; }

        public Color BackgroundColor { get; set; }
        public Color BackgroundColor2 { get; set; }
        public GradientType BackgroundGradient { get; set; }
        public string GradientString
        {
            get { return BackgroundGradient.ToString(); }
            set { BackgroundGradient = (GradientType)Enum.Parse(typeof(GradientType), value); }
        }

        public LiveSplitState CurrentState { get; set; }
        public bool Display2Rows { get; set; }

        public LayoutMode Mode { get; set; }
        public int DetailedTrackerX { get; set; }
        public int DetailedTrackerY { get; set; }
        public Point DetailedTrackerLocation
        {
            get
            {
                return new Point(DetailedTrackerX, DetailedTrackerY);
            }
        }

        public DarkSouls100TrackerSettings()
        {
            InitializeComponent();

            TextColor = Color.FromArgb(255, 255, 255);
            OverrideTextColor = false;
            Accuracy = TimeAccuracy.Hundredths;
            BackgroundColor = Color.Transparent;
            BackgroundColor2 = Color.Transparent;
            BackgroundGradient = GradientType.Plain;
            Display2Rows = false;

            chkOverrideTextColor.DataBindings.Add("Checked", this, "OverrideTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnTextColor.DataBindings.Add("BackColor", this, "TextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbGradientType.DataBindings.Add("SelectedItem", this, "GradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor2.DataBindings.Add("BackColor", this, "BackgroundColor2", false, DataSourceUpdateMode.OnPropertyChanged);
            chkShowPercentage.DataBindings.Add("Checked", this, "ShowPercentage", false, DataSourceUpdateMode.OnPropertyChanged);
            chkDarkTheme.DataBindings.Add("Checked", this, "DarkTheme", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOpenAtLaunch.DataBindings.Add("Checked", this, "OpenAtLaunch", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        void ChkOverrideTextColor_CheckedChanged(object sender, EventArgs e)
        {
            TextLabel.Enabled = btnTextColor.Enabled = chkOverrideTextColor.Checked;
        }

        void DeltaSettings_Load(object sender, EventArgs e)
        {
            ChkOverrideTextColor_CheckedChanged(null, null);

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

        void RdoHundredths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void RdoSeconds_CheckedChanged(object sender, EventArgs e)
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
            BackgroundColor2 = SettingsHelper.ParseColor(element["BackgroundColor2"]);
            GradientString = SettingsHelper.ParseString(element["BackgroundGradient"]);
            Display2Rows = SettingsHelper.ParseBool(element["Display2Rows"]);
            ShowPercentage = SettingsHelper.ParseBool(element["ShowPercentage"]);
            DarkTheme = SettingsHelper.ParseBool(element["DarkTheme"]);
            OpenAtLaunch = SettingsHelper.ParseBool(element["OpenAtLaunch"]);
            DetailedTrackerX = SettingsHelper.ParseInt(element["X"]);
            DetailedTrackerY = SettingsHelper.ParseInt(element["Y"]);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            this.OnSettingsLoaded?.Invoke(this, EventArgs.Empty);
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
            SettingsHelper.CreateSetting(document, parent, "BackgroundColor2", BackgroundColor2) ^
            SettingsHelper.CreateSetting(document, parent, "BackgroundGradient", BackgroundGradient) ^
            SettingsHelper.CreateSetting(document, parent, "Display2Rows", Display2Rows) ^
            SettingsHelper.CreateSetting(document, parent, "ShowPercentage", ShowPercentage) ^
            SettingsHelper.CreateSetting(document, parent, "DarkTheme", DarkTheme) ^
            SettingsHelper.CreateSetting(document, parent, "OpenAtLaunch", OpenAtLaunch) ^
            SettingsHelper.CreateSetting(document, parent, "X", DetailedTrackerX) ^
            SettingsHelper.CreateSetting(document, parent, "Y", DetailedTrackerY);
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            SettingsHelper.ColorButtonClick((Button)sender, this);
        }

        private void BtnDetails_Click(object sender, EventArgs e)
        {
            this.OnToggleDetails?.Invoke(sender, EventArgs.Empty);
        }

        private void CmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnColor1.Visible = cmbGradientType.SelectedItem.ToString() != "Plain";
            btnColor2.DataBindings.Clear();
            btnColor2.DataBindings.Add("BackColor", this, btnColor1.Visible ? "BackgroundColor2" : "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            GradientString = cmbGradientType.SelectedItem.ToString();
        }
    }
}


