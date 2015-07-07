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
    public delegate void UpdateVoidDelegate();

    public partial class MainForm : Form
    {
        private CommsForm commsForm;

        // Delegate to enables asynchronous callbacks.
        delegate void SetControlTextCallback(Control control, string text);
        delegate void SetControlVisibilityCallback(Control control, Boolean onNotOff);
        delegate void SetControlBackgroundImageCallback(Control control, Bitmap image);
        delegate void RefreshControlCallback(Control control);
        delegate void RefreshChartCallback(System.Windows.Forms.DataVisualization.Charting.Chart chart);

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

        // Variables to do with dynamic data
        public DataTable energyTable = null;
        public static readonly string energyDataFilename = "teddy_energy_data.xml";
        private static readonly string energyTableName = "teddy";
        private static readonly string energyRowYearsName = "years";
        private static readonly string energyRowEnergymWhName = "energymWh";
        private static readonly string[] numbersInWords = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen", "twenty", "twenty one", "twenty two", "twenty three", "twenty four" };
        private static UInt32 energyDisplayUpdateTimeMs = 10000;
        private System.Threading.Timer energyDisplayUpdateTimer = null;
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

            // Setup the energy usage graph
            setupEnergyUsageGraph();

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
            energyDisplayUpdateTimer = new System.Threading.Timer(updateEnergyDisplayCallback, null, energyDisplayUpdateTimeMs, 0);

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


        // Tick callback for the tick timer
        private void tickTimerCallback(object state)
        {
            updateControlText(labelTimeNow, ("Time Now: " + DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.ToLocalTime().ToLongTimeString()));
            tickTimer.Change (tickTimerDurationSeconds * 1000, 0);
        }
        
        // Set up the energy usage graph
        private void setupEnergyUsageGraph()
        {
            energyTable = new DataTable();

            energyTable.TableName = energyTableName;
            energyTable.Columns.Add(energyRowYearsName, typeof(Double));
            energyTable.Columns.Add(energyRowEnergymWhName, typeof(Int64));

            // Try to read in any existing data
            if (System.IO.File.Exists(energyDataFilename))
            {
                try
                {
                    energyTable.ReadXml(energyDataFilename);
                }
                catch (Exception)
                {
                }
            }

            chartEnergyUsage.Titles["EnergyUsageChartTitle"].Text = "Battery Charge Remaining (based on " + numbersInWords[Properties.Settings.Default.TRxPerDay] + " reports per day)";
            chartEnergyUsage.Series.Add("energy");
            chartEnergyUsage.Series.First().YValueMembers = energyRowEnergymWhName;
            chartEnergyUsage.Series.First().XValueMember = energyRowYearsName;
            chartEnergyUsage.Series.First().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chartEnergyUsage.Series.First().Color = Color.Red;
            chartEnergyUsage.DataSource = energyTable;
            chartEnergyUsage.DataBind();
        }

        ///
        /// Methods to handle interaction with the objects on the form directly
        ///

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

        // Update the time and date
        public void updateTimeAndDate()
        {
            updateControlText(labelSystemDatabaseUpdateTime, ("Updated " + DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.ToLocalTime().ToLongTimeString()));
        }

        // Update the RSSI wherever relevant
        public void _updateRssiCallback(Int32 rssi)
        {
            updateControlText(labelSystemDatabaseRssidBmValue, (rssi.ToString() + " dBm"));
            updateTimeAndDate();
        }

        // Update the reporting interval wherever relevant
        public void _updateReportingIntervalCallback(UInt32 heartbeat)
        {
            updateTimeAndDate();
        }

        // Update the reading interval wherever relevant
        public void _updateHeartbeatCallback(UInt32 heartbeat)
        {
            updateTimeAndDate();
        }

        // 
        // Update the connection state
        public void _updateConnectionCallback(Boolean connected)
        {
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

        // Refresh a chart
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

        // Update the energy representation
        private void updateEnergyDisplayCallback(object state)
        {
            Int64 tRxPerDay = Properties.Settings.Default.TRxPerDay;
            Int64 bitsPerTx = Properties.Settings.Default.BitsPerTx;
            Int64 bitsPerRx = Properties.Settings.Default.BitsPerRx;
            Int64 batterySizemWh = Properties.Settings.Default.BatterySizemWh;
            Int64 txCount = Properties.Settings.Default.TxCount;
            Int64 rxCount = Properties.Settings.Default.RxCount;
            Int64 bitsTxCount = Properties.Settings.Default.BitsTxCount;
            Int64 bitsRxCount = Properties.Settings.Default.BitsRxCount;
            Int64 timePassedSeconds = Properties.Settings.Default.TimePassedSeconds;
            Int64 bitsPerAck = Properties.Settings.Default.BitsPerAck;
            Int64 bitsProtocolOverhead = Properties.Settings.Default.BitsProtocolOverhead;

            // Calculate the normal energy per day, adding in protocol overheads and acks
            Int64 normalEnergyPerDaynWh = tRxPerDay * (CommsForm.perTRxnWh + (((bitsPerTx + bitsProtocolOverhead) * CommsForm.perBitTxnWh) +
                                                       (bitsPerAck * CommsForm.perBitRxnWh)) + (CommsForm.leakagenWh * 24));
            // So the normal battery life in days is...
            Int64 normalBatteryLifeDays = (batterySizemWh * 1000000) / normalEnergyPerDaynWh;
            // Calculate the energy used here (protocol and ack overheads are already included)
            Int64 energyUsednWh = ((txCount + rxCount) * CommsForm.perTRxnWh) + (bitsTxCount * CommsForm.perBitTxnWh) + (bitsRxCount * CommsForm.perBitRxnWh) + (CommsForm.leakagenWh * timePassedSeconds / 3600);
            // So the energy used per second is...
            Int64 energyUsedPerSecondnWh = energyUsednWh / (timePassedSeconds + 1); // +1 to avoid divide by zero exceptions
            // And hence the scale factor between normality and this case is...
            Int64 scaleFactor = (energyUsedPerSecondnWh * 3600 * 24) / normalEnergyPerDaynWh;
            // So the scaled elapsed time in days is...
            Int64 scaledElapsedTimeDays = timePassedSeconds * scaleFactor / (3600 * 24);

            // The battery left, here and now, is
            Int64 batteryLeftnWh = (batterySizemWh * 1000000) - energyUsednWh;
            if (batteryLeftnWh < 0)
            {
                batteryLeftnWh = 0;
            }
            Int64 scaledBatteryLeftDays = batteryLeftnWh / normalEnergyPerDaynWh;

           // Update the "Replace Battery" text
           updateControlText(labelReplaceBattery, "Replace Battery " + scaledBatteryLeftDays / 365 + " Years " + scaledBatteryLeftDays % 365 + " Days");

            // Battery left percentage
            Int64 batteryLeftPercent = (batteryLeftnWh * 100) / (batterySizemWh * 1000000);

            // Update the Counts fields
            updateControlText(labelTxCountValue, txCount.ToString());
            updateControlText(labelRxCountValue, rxCount.ToString());

            // Add a new data point to the collection
            energyTable.Rows.Add((Double)(((Double)scaledElapsedTimeDays) / 365), batteryLeftnWh / 1000000);

            // Update the file: TODO - find a way to append, this is very wasteful
            energyTable.WriteXml(energyDataFilename);

            // Replot the graph
            updateRefreshChart(chartEnergyUsage);

            // Restart the timer
            energyDisplayUpdateTimer.Change(energyDisplayUpdateTimeMs, 0);
        }
    }
}
