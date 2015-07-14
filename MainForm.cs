using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// If this application launch mysteriously halts on entry to the MainForm constructor, or
// if you hit the exception:
//
// "Mixed mode assembly is built against version 'v1.1.4322' of the runtime
// and cannot be loaded in the 4.0 runtime without additional configuration information."
//
// ...then make sure that the app.config file has the following entry in the configuration section:
//
//   <startup useLegacyV2RuntimeActivationPolicy="true" ></startup>
//
// If you change settings or anything else that causes app.config to be recreated by MSVC, you
// will need to re-add this change.

namespace Teddy
{
    // Delegates to communicate with CommsForm
    public delegate void UpdateUInt32Delegate(UInt32 value);
    public delegate void UpdateInt32Delegate(Int32 value);
    public delegate void UpdateBooleanDelegate(Boolean value);
    public delegate void UpdateStringDelegate(String str);
    public delegate void UpdateVoidDelegate();
    public delegate void UpdateDataRowDelegate(DataRow value);

    public partial class MainForm : Form
    {
        private CommsForm commsForm;

        // Delegate to enables asynchronous callbacks.
        delegate void SetControlTextCallback(Control control, string text);
        delegate void SetControlVisibilityCallback(Control control, Boolean onNotOff);
        delegate void SetControlBackgroundImageCallback(Control control, Bitmap image);
        delegate void RefreshControlCallback(Control control);
        delegate void RefreshChartCallback(System.Windows.Forms.DataVisualization.Charting.Chart chart);
        delegate void RefreshMapCallback(double latitude, double longitude);

        // Things to do with getting an item on the system menu of MainForm
        private const int WM_SYSCOMMAND = 0x112;
        private const int MF_STRING = 0x0;
        private const int MF_SEPARATOR = 0x800;
        private int SYSTEM_MENU_SETUP_ID = 0x1;

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu,
        Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        // Variables to do with appearance
        private static readonly Size neededScreenResolution = new Size(1920, 1080);
        private System.Threading.Timer tickTimer = null;
        private static readonly UInt32 tickTimerDurationSeconds = 1;

        // Variables to do with projected energy data
        public DataTable energyTable = null;
        public static readonly string energyDataFilename = "teddy_energy_data.xml";

        // Data storage items
        public DataTable sensorDataTable = null;
        public static readonly string sensorDataTableFilename = "teddy_sensor_data.xml";
        public readonly string sensorDataTableName = "teddySensors";
        public DataTable trafficDataTable = null;
        public static readonly string trafficDataTableFilename = "teddy_traffic_data.xml";
        public readonly string trafficDataTableName = "teddyTraffic";
        
        public readonly string dataTableRowTimeName = "time";

        public readonly string dataTableRowLatitudeName = "latitude";
        public readonly string dataTableRowLongitudeName = "longitude";
        public readonly string dataTableRowElevationName = "elevation";
        public readonly string dataTableRowSpeedName = "speed";
        public readonly string dataTableRowOrientationName = "orientation";
        public readonly string dataTableRowHugsName = "hugs";
        public readonly string dataTableRowSlapsName = "slaps";
        public readonly string dataTableRowDropsName = "drops";
        public readonly string dataTableRowNudgesName = "nudges";
        public readonly string dataTableRowSoundLevelName = "soundLevel";
        public readonly string dataTableRowLuminosityName = "luminosity";
        public readonly string dataTableRowTemperatureName = "temperature";
        public readonly string dataTableRowRssiName = "rssi";
        public readonly string dataTableRowRssidBmName = "rssidBm";
        public readonly string dataTableRowChargeStateName = "chargeState";
        public readonly string dataTableRowBatterymVName = "batterymV";
        public readonly string dataTableRowEnergyuWhName = "energyuWh";
        public readonly string dataTableRowEnergymWPerHourName = "energymWPerHour";
        public readonly string dataTableRowEnergyTotalmWhName = "energyTotalmWh";
        public readonly string dataTableRowUplinkBytesName = "uplinkBytes";
        public readonly string dataTableRowUplinkDatagramsName = "uplinkDatagrams";
        public readonly string dataTableRowDownlinkBytesName = "downlinkBytes";
        public readonly string dataTableRowDownlinkDatagramsName = "downlinkDatagrams";

        public readonly Boolean testMode = false;  // Set this to true and do whatever you wish under it at your own risk
        public readonly Boolean noTamperingAllowed = false;  // Set this to true to stop tampering with the configuration

        ///
        ///  Configuration methods 
        ///

        // Constructor
        public MainForm()
        {
            InitializeComponent();
            this.FormClosed += MainForm_FormClosed;

            // Setup the data tables for sensor data
            setupSensorDataStorage();
            // Setup the data tables for traffic
            setupTrafficDataStorage();

            // Setup misc graphs
            setupElevationGraph(chartElevation);
            setupSpeedGraph(chartSpeed);
            setupOrientationGraph(chartOrientation);
            setupInteractionGraph(chartInteraction);
            setupRssiGraph(chartRssi);
            setupPowerGraph(chartPower);
            setupTrafficGraph(chartTraffic);
            setupTemperatureGraph(chartTemperature);
            setupLuminosityGraph(chartLuminosity);
            setupSoundLevelGraph(chartSoundLevel);

            // Find the right screen for the main form
            setupScreen(this, neededScreenResolution);
            this.Invalidate();
        }

        // Add the menu for the setup form to the system menu for this form
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            IntPtr systemMenuHandle = GetSystemMenu(this.Handle, false);
            commsForm = new CommsForm(this);

            if (systemMenuHandle != IntPtr.Zero)
            {
                // Add a separator
                AppendMenu(systemMenuHandle, MF_SEPARATOR, 0, string.Empty);

                // Add the About menu item
                AppendMenu(systemMenuHandle, MF_STRING, SYSTEM_MENU_SETUP_ID, "&Setup…");
            }
            pictureBoxCommsForm_Click(null, null);

            // Start the tick timer
            tickTimer = new System.Threading.Timer(tickTimerCallback, null, tickTimerDurationSeconds * 1000, 0);
        }

        // Catch the system menu in action and detect the setup item being selected
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // Test if the About item was selected from the system menu
            if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == SYSTEM_MENU_SETUP_ID))
            {
                pictureBoxCommsForm_Click(null, null);
            }
        }

        // Tear stuff down and save stuff
        private void MainForm_FormClosed(object sender, EventArgs e)
        {
            commsForm.close();
        }

        // Put the given form on a screen with the desired resolution,
        // if there is one
        private void setupScreen(Form form, Size desiredResolution)
        {
            // Find a 1920 x 1080 screen and display on that one as the form
            // won't display properly on another
            if (!(Screen.FromControl(this).Bounds.Size == desiredResolution))
            {
                UInt32 x;
                Boolean found = false;
                for (x = 0; (x < Screen.AllScreens.Count()) && !found; x++)
                {
                    if (Screen.AllScreens[x].Bounds.Size == desiredResolution)
                    {
                        form.WindowState = FormWindowState.Normal;
                        form.StartPosition = FormStartPosition.Manual;
                        form.BringToFront();
                        form.Left = Screen.AllScreens[x].WorkingArea.Left;
                        form.Top = Screen.AllScreens[x].WorkingArea.Top;
                        found = true;
                    }
                }
            }
        }

        // Store sensor data (including energy)
        private void setupSensorDataStorage()
        {
            sensorDataTable = new DataTable();

            sensorDataTable.TableName = sensorDataTableName;
            sensorDataTable.Columns.Add(dataTableRowTimeName, typeof(DateTime));
            sensorDataTable.Columns.Add(dataTableRowLatitudeName, typeof(double));
            sensorDataTable.Columns.Add(dataTableRowLongitudeName, typeof(double));
            sensorDataTable.Columns.Add(dataTableRowElevationName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowSpeedName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowOrientationName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowHugsName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowSlapsName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowDropsName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowNudgesName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowSoundLevelName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowLuminosityName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowTemperatureName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowRssiName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowRssidBmName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowChargeStateName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowBatterymVName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowEnergyuWhName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowEnergymWPerHourName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowEnergyTotalmWhName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowUplinkBytesName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowUplinkDatagramsName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowDownlinkBytesName, typeof(Int32));
            sensorDataTable.Columns.Add(dataTableRowDownlinkDatagramsName, typeof(Int32));
        }

        // Store traffic data
        private void setupTrafficDataStorage()
        {
            trafficDataTable = new DataTable();

            trafficDataTable.TableName = trafficDataTableName;
            trafficDataTable.Columns.Add(dataTableRowTimeName, typeof(DateTime));
            trafficDataTable.Columns.Add(dataTableRowUplinkBytesName, typeof(Int32));
            trafficDataTable.Columns.Add(dataTableRowUplinkDatagramsName, typeof(Int32));
            trafficDataTable.Columns.Add(dataTableRowDownlinkBytesName, typeof(Int32));
            trafficDataTable.Columns.Add(dataTableRowDownlinkDatagramsName, typeof(Int32));
        }

        // Tick callback for the tick timer
        private void tickTimerCallback(object state)
        {
            updateControlText(labelTimeNow, ("Time Now: " + DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.ToLocalTime().ToLongTimeString()));
            tickTimer.Change (tickTimerDurationSeconds * 1000, 0);
        }

        // Setup the common stuff for the various graphs
        private void setupCommonGraph(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.Series.FirstOrDefault().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.ChartAreas.FirstOrDefault().AxisX.LabelAutoFitMaxFontSize = 6;
            chart.ChartAreas.FirstOrDefault().AxisY.LabelAutoFitMaxFontSize = 6;
            chart.ChartAreas.FirstOrDefault().AxisY2.LabelAutoFitMaxFontSize = 6;
            chart.ChartAreas.FirstOrDefault().AxisY.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas.FirstOrDefault().AxisY2.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas.FirstOrDefault().AxisX.MajorGrid.LineColor = Color.LightGray;
            chart.Paint += new PaintEventHandler(paintUpdateTimeOnChart);
        }

        // Setup the various graphs
        private void setupElevationGraph(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.Titles["TitleChartElevation"].Text = "Elevation";
            chart.Series.Add("elevation");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().YValueMembers = dataTableRowElevationName;
            chart.Series.Last().Color = Color.Red;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.FirstOrDefault().AxisX.LabelStyle.Format = "HH:mm:ss";
            chart.ChartAreas.FirstOrDefault().AxisY.Title = "metres";
            setupCommonGraph(chart);
            chart.DataSource = sensorDataTable;
            chart.DataBind();
        }
        private void setupSpeedGraph(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.Titles["TitleChartSpeed"].Text = "Speed";
            chart.Series.Add("speed");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().YValueMembers = dataTableRowSpeedName;
            chart.Series.Last().Color = Color.Red;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.FirstOrDefault().AxisX.LabelStyle.Format = "HH:mm:ss";
            chart.ChartAreas.FirstOrDefault().AxisY.Title = "km/h";
            setupCommonGraph(chart);
            chart.DataSource = sensorDataTable;
            chart.DataBind();
        }
        private void setupOrientationGraph(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.Titles["TitleChartOrientation"].Text = "Orientation";
            chart.Series.Add("orientation");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().YValueMembers = dataTableRowOrientationName;
            chart.Series.Last().Color = Color.Red;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.FirstOrDefault().AxisX.LabelStyle.Format = "HH:mm:ss";
            chart.ChartAreas.FirstOrDefault().AxisY.Interval = 1;
            setupCommonGraph(chart);
            chart.DataSource = sensorDataTable;
            chart.DataBind();
        }
        private void setupInteractionGraph(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.Titles["TitleChartInteraction"].Text = "Interaction";
            chart.Series.Add("hugs");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowHugsName;
            chart.Series.Last().Color = Color.DarkGreen;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.Last().AxisY.Interval = 1;
            chart.Series.Add("slaps");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowSlapsName;
            chart.Series.Last().Color = Color.DarkRed;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.Last().AxisY.Interval = 1;
            chart.Series.Add("drops");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowDropsName;
            chart.Series.Last().Color = Color.MediumPurple;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.Last().AxisY.Interval = 1;
            chart.Series.Add("nudges");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowNudgesName;
            chart.Series.Last().Color = Color.Red;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.FirstOrDefault().AxisX.LabelStyle.Format = "HH:mm:ss";
            chart.ChartAreas.Last().AxisY.Interval = 1;
            setupCommonGraph(chart);
            chart.DataSource = sensorDataTable;
            chart.DataBind();
        }
        private void setupRssiGraph(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.Titles["TitleChartRssi"].Text = "RSSI";
            chart.Series.Add("rssi");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowRssidBmName;
            chart.Series.Last().BorderWidth = 1;
            chart.Series.Last().Color = Color.Red;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.FirstOrDefault().AxisX.LabelStyle.Format = "HH:mm:ss";
            chart.ChartAreas.FirstOrDefault().AxisY.Title = "dBm";
            setupCommonGraph(chart);
            chart.DataSource = sensorDataTable;
            chart.DataBind();
        }
        private void setupPowerGraph(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.Titles["TitleChartPower"].Text = "Power";
            chart.Series.Add("energy (mWh)");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers =  dataTableRowEnergymWPerHourName;
            chart.Series.Last().BorderWidth = 1;
            chart.Series.Last().BorderColor = Color.DarkBlue;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.Series.Last().YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            chart.Series.Add("battery (mV)");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowBatterymVName;
            chart.Series.Last().BorderWidth = 1;
            chart.Series.Last().BorderColor = Color.DarkGreen;
            chart.Series.Last().YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Primary;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.FirstOrDefault().AxisX.LabelStyle.Format = "HH:mm:ss";
            chart.ChartAreas.FirstOrDefault().AxisY.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chart.ChartAreas.FirstOrDefault().AxisY.Title = "mV";
            chart.ChartAreas.FirstOrDefault().AxisY.LineColor = Color.DarkBlue;
            chart.ChartAreas.FirstOrDefault().AxisY.MajorTickMark.LineColor = Color.DarkBlue;
            chart.ChartAreas.FirstOrDefault().AxisY.TitleForeColor = Color.DarkBlue;
            chart.ChartAreas.FirstOrDefault().AxisY.LabelStyle.ForeColor = Color.DarkBlue;
            chart.ChartAreas.FirstOrDefault().AxisY2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chart.ChartAreas.FirstOrDefault().AxisY2.Title = "mWh";
            chart.ChartAreas.FirstOrDefault().AxisY2.LineColor = Color.DarkGreen;
            chart.ChartAreas.FirstOrDefault().AxisY2.MajorTickMark.LineColor = Color.DarkGreen;
            chart.ChartAreas.FirstOrDefault().AxisY2.TitleForeColor = Color.DarkGreen;
            chart.ChartAreas.FirstOrDefault().AxisY2.LabelStyle.ForeColor = Color.DarkGreen;
            setupCommonGraph(chart);
            chart.DataSource = sensorDataTable;
            chart.DataBind();
        }
        private void setupTrafficGraph(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.Titles["TitleChartTraffic"].Text = "Traffic Totals Since Reset";
            chart.Series.Add("uplink bytes");
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowUplinkBytesName;
            chart.Series.Last().YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Primary;
            chart.Series.Last().Color = Color.Red;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.Series.Add("uplink datagrams");
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowUplinkDatagramsName;
            chart.Series.Last().Color = Color.DarkRed;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.Series.Last().YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            chart.Series.Add("downlink bytes");
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowDownlinkBytesName;
            chart.Series.Last().Color = Color.LightBlue;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.Series.Last().YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Primary;
            chart.Series.Add("downlink datagrams");
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowDownlinkDatagramsName;
            chart.Series.Last().Color = Color.DarkBlue;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.Series.Last().YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            chart.ChartAreas.FirstOrDefault().AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            chart.ChartAreas.FirstOrDefault().AxisY.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chart.ChartAreas.FirstOrDefault().AxisY.Title = "bytes";
            chart.ChartAreas.FirstOrDefault().AxisY2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chart.ChartAreas.FirstOrDefault().AxisY2.Title = "datagrams";
            setupCommonGraph(chart);
            chart.DataSource = trafficDataTable;
            chart.DataBind();
        }
        private void setupTemperatureGraph(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.Titles["TitleChartTemperature"].Text = "Temperature";
            chart.Series.Add("temperature");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowTemperatureName;
            chart.Series.Last().BorderWidth = 1;
            chart.Series.Last().BorderColor = Color.DarkRed;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.FirstOrDefault().AxisX.LabelStyle.Format = "HH:mm:ss";
            chart.ChartAreas.FirstOrDefault().AxisY.Title = "Celsius";
            setupCommonGraph(chart);
            chart.DataSource = sensorDataTable;
            chart.DataBind();
        }
        private void setupLuminosityGraph(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.Titles["TitleChartLuminosity"].Text = "Luminosity";
            chart.Series.Add("luminosity");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowLuminosityName;
            chart.Series.Last().BorderWidth = 1;
            chart.Series.Last().BorderColor = Color.DarkBlue;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.FirstOrDefault().AxisX.LabelStyle.Format = "HH:mm:ss";
            chart.ChartAreas.FirstOrDefault().AxisY.Title = "lux";
            setupCommonGraph(chart);
            chart.DataSource = sensorDataTable;
            chart.DataBind();
        }
        private void setupSoundLevelGraph(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.Titles["TitleChartSoundLevel"].Text = "SoundLevel";
            chart.Series.Add("soundLevel");
            chart.Series.Last().XValueMember = dataTableRowTimeName;
            chart.Series.Last().XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            chart.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series.Last().YValueMembers = dataTableRowSoundLevelName;
            chart.Series.Last().BorderWidth = 1;
            chart.Series.Last().BorderColor = Color.DarkOrange;
            chart.Series.Last().MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            chart.ChartAreas.FirstOrDefault().AxisX.LabelStyle.Format = "HH:mm:ss";
            setupCommonGraph(chart);
            chart.DataSource = sensorDataTable;
            chart.DataBind();
        }

        ///
        /// Methods to handle interaction with the objects on the form directly
        ///
        private void updateMap(double latitude, double longitude)
        {
            StringBuilder webPage = new StringBuilder();
            webPage.Append(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");
            webPage.Append(@"<html>");
            webPage.Append(@"<head>");
            webPage.Append(@"<meta name=""viewport"" content=""initial-scale=1.0, user-scalable=no"" />");
            webPage.Append(@"<title>Google Map</title>");
            webPage.Append(@"<script type=""text/javascript""");
            webPage.Append(@"src=""http://maps.google.com.mx/maps/api/js?sensor=true&language=""es""></script>");
            webPage.Append(@"<script src='http://google-maps-utility-library-v3.googlecode.com/svn/trunk/markerclusterer/src/markerclusterer.js'>");
            webPage.Append(@"</script><script type=""text/javascript"">");
            webPage.Append(@"  function initialize()");
            webPage.Append(@"  {");
            webPage.Append(string.Format("    var latlng = new google.maps.LatLng({0}, {1});", latitude, longitude));
            webPage.Append(@"    var myOptions =");
            webPage.Append(@"    {");
            webPage.Append(@"      zoom: 15,");
            webPage.Append(@"      center: latlng,");
            webPage.Append(@"      mapTypeId: google.maps.MapTypeId.HYBRID");
            webPage.Append(@"    };");
            webPage.Append(@"    var map = new google.maps.Map (document.getElementById(""map_canvas""), myOptions);");
            webPage.Append(@"    map.setTilt (45);");
            webPage.Append(@"    var marker = new google.maps.Marker ({");
            webPage.Append(@"      position: latlng,");
            webPage.Append(string.Format ("      title: \"Where was tedI @ {0}?\"", String.Format("Updated {0:D2}:{1:D2}:{2:D2}", DateTime.UtcNow.ToLocalTime().Hour, DateTime.UtcNow.ToLocalTime().Minute, DateTime.UtcNow.ToLocalTime().Second)));
            webPage.Append(@"    });");
            webPage.Append(@"    marker.setMap (map);");
            webPage.Append(@"  }");
            webPage.Append(@"</script>");
            webPage.Append(@"</head>");
            webPage.Append(@"<body onload=""initialize()"">");
            webPage.Append(@"  <div id=""map_canvas"" style=""width:100%; height:100%""></div>");
            map.DocumentText = webPage.ToString();

            map.Invalidate();
            updateControlVisibility (map, true);
        }

        // Bring up the comms form
        private void pictureBoxCommsForm_Click(object sender, EventArgs e)
        {
            commsForm.TopMost = true;
            commsForm.Show();
            commsForm.WindowState = FormWindowState.Normal;
        }

        ///
        ///  Methods to allow values to be updated on this form based
        ///  on the comms conducted by the CommsForm.
        ///

        // Update sensor data
        public void _updateSensorDataCallback(DataRow value)
        {
            sensorDataTable.Rows.Add(value);
            sensorDataTable.WriteXml(sensorDataTableFilename);

            if (value[dataTableRowLatitudeName] != DBNull.Value && value[dataTableRowLongitudeName] != DBNull.Value)
            {
                updateMap(Convert.ToDouble(value[dataTableRowLatitudeName]), Convert.ToDouble(value[dataTableRowLongitudeName]));
            }

            if (value[dataTableRowElevationName] != DBNull.Value)
            {
                updateRefreshChart(chartElevation);
            }
            if (value[dataTableRowSpeedName] != DBNull.Value)
            {
                updateRefreshChart(chartSpeed);
            }
            if (value[dataTableRowOrientationName] != DBNull.Value)
            {
                updateRefreshChart(chartOrientation);
            }
            if (value[dataTableRowHugsName] != DBNull.Value || value[dataTableRowSlapsName] != DBNull.Value || value[dataTableRowDropsName] != DBNull.Value || value[dataTableRowNudgesName] != DBNull.Value)
            {
                updateRefreshChart(chartInteraction);
            }
            if (value[dataTableRowRssiName] != DBNull.Value)
            {
                updateRefreshChart(chartRssi);
            }
            if (value[dataTableRowBatterymVName] != DBNull.Value && value[dataTableRowEnergymWPerHourName] != DBNull.Value)
            {
                updateRefreshChart(chartPower);
            }
            if (value[dataTableRowTemperatureName] != DBNull.Value)
            {
                updateRefreshChart(chartTemperature);
            }
            if (value[dataTableRowLuminosityName] != DBNull.Value)
            {
                updateRefreshChart(chartLuminosity);
            }
            if (value[dataTableRowSoundLevelName] != DBNull.Value)
            {
                updateRefreshChart(chartSoundLevel);
            }
        }

        // Update traffic data
        public void _updateTrafficDataCallback(DataRow value)
        {
            trafficDataTable.Rows.Add(value);
            trafficDataTable.WriteXml(trafficDataTableFilename);

            updateRefreshChart(chartTraffic);
        }

        // Clear the sensor data
        public void _clearSensorDataCallback()
        {
            if (System.IO.File.Exists(sensorDataTableFilename))
            {
                System.IO.File.Delete(sensorDataTableFilename);
            }
            sensorDataTable.Clear();
            updateRefreshChart(chartElevation);
            updateRefreshChart(chartSpeed);
            updateRefreshChart(chartOrientation);
            updateRefreshChart(chartInteraction);
            updateRefreshChart(chartRssi);
            updateRefreshChart(chartPower);
            updateRefreshChart(chartTemperature);
            updateRefreshChart(chartLuminosity);
            updateRefreshChart(chartSoundLevel);
        }

        // Clear the traffic data
        public void _clearTrafficDataCallback()
        {
            if (System.IO.File.Exists(trafficDataTableFilename))
            {
                System.IO.File.Delete(trafficDataTableFilename);
            }
            trafficDataTable.Clear();
            updateRefreshChart(chartTraffic);
        }

        // Update the reporting interval wherever relevant
        public void _updateReportingIntervalCallback(UInt32 reportingInterval)
        {
            updateControlText(labelReportingInterval, String.Format("ReportingInterval: {0} minute(s).", reportingInterval));
        }

        // Update the reading interval wherever relevant
        public void _updateHeartbeatCallback(UInt32 heartbeat)
        {
            updateControlText(labelHeartbeat, String.Format("Heartbeat: {0} second(s).", heartbeat));
        }

        // Update the connection state
        public void _updateConnectionCallback(Boolean connected)
        {
        }

        // Update when an InitInd occurs
        public void _updateInitIndCallback(String str)
        {
            updateControlText(labelInitInd, String.Format("Last InitInd @ {0:D2}:{1:D2}:{2:D2}, reason \"{3}\".",
                                                           DateTime.UtcNow.ToLocalTime().Hour, DateTime.UtcNow.ToLocalTime().Minute, DateTime.UtcNow.ToLocalTime().Second, str));
        }

        // Update when a PollInd occurs
        public void _updatePollIndCallback()
        {
            updateControlText(labelPollInd, String.Format("Last PollInd @ {0:D2}:{1:D2}:{2:D2}.",
                                                           DateTime.UtcNow.ToLocalTime().Hour, DateTime.UtcNow.ToLocalTime().Minute, DateTime.UtcNow.ToLocalTime().Second));
        }

        // Write the time a chart was updated
        private void paintUpdateTimeOnChart(object sender, PaintEventArgs e)
        {
            string text = String.Format("Updated {0:D2}:{1:D2}:{2:D2}", DateTime.UtcNow.ToLocalTime().Hour, DateTime.UtcNow.ToLocalTime().Minute, DateTime.UtcNow.ToLocalTime().Second);

            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 8);
            System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(Color.Black);
            e.Graphics.DrawString(text, drawFont, drawBrush, 0, 0);
            drawFont.Dispose();
            drawBrush.Dispose();
        }

        // Update the text on a control
        private void updateControlText(Control control, String text)
        {
            if (!control.IsDisposed)
            {
                if (control.InvokeRequired)
                {
                    SetControlTextCallback update = new SetControlTextCallback(_updateControlText);
                    control.Invoke(update, new object[] { control, text });
                }
                else
                {
                    _updateControlText(control, text);
                }
            }
        }
        private void _updateControlText(Control control, String text)
        {
            control.Text = text;
        }

        // Update the background image on a control
        private void updateControlBackgroundImage(Control control, Bitmap image)
        {
            if (!control.IsDisposed)
            {
                if (control.InvokeRequired)
                {
                    SetControlBackgroundImageCallback update = new SetControlBackgroundImageCallback(_updateControlBackgroundImage);
                    control.Invoke(update, new object[] { control, image });
                }
                else
                {
                    _updateControlBackgroundImage(control, image);
                }
            }
        }
        private void _updateControlBackgroundImage(Control control, Bitmap image)
        {
            control.BackgroundImage = image;
        }

        // Update the visibility of a control
        private void updateControlVisibility(Control control, Boolean onNotOff)
        {
            if (!control.IsDisposed)
            {
                if (control.InvokeRequired)
                {
                    SetControlVisibilityCallback update = new SetControlVisibilityCallback(_updateControlVisibility);
                    control.Invoke(update, new object[] { control, onNotOff });
                }
                else
                {
                    _updateControlVisibility(control, onNotOff);
                }
            }
        }
        private void _updateControlVisibility(Control control, Boolean onNotOff)
        {
            control.Visible = onNotOff;
        }

        // Refresh a control
        private void updateRefreshControl(Control control)
        {
            if (!control.IsDisposed)
            {
                if (control.InvokeRequired)
                {
                    RefreshControlCallback update = new RefreshControlCallback(_updateRefreshControl);
                    control.Invoke(update, new object[] { control });
                }
                else
                {
                    _updateRefreshControl(control);
                }
            }
        }
        private void _updateRefreshControl(Control control)
        {
            control.Refresh();
        }

        // Update a chart
        private void updateRefreshChart(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            if (!chart.IsDisposed)
            {
                if (chart.InvokeRequired)
                {
                    RefreshChartCallback update = new RefreshChartCallback(_updateRefreshChart);
                    chart.Invoke(update, new object[] { chart });
                }
                else
                {
                    _updateRefreshChart(chart);
                }
            }
        }
        private void _updateRefreshChart(System.Windows.Forms.DataVisualization.Charting.Chart chart)
        {
            chart.DataBind();
        }

        // Update the map
        private void updateRefreshMap(double latitude, double longitude)
        {
            if (!map.IsDisposed)
            {
                if (map.InvokeRequired)
                {
                    RefreshMapCallback update = new RefreshMapCallback(updateMap);
                    map.Invoke(update, new object[] { latitude, longitude });
                }
                else
                {
                    updateMap(latitude, longitude);
                }
            }
        }
    }
}
