using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MessageCodec;
using Neul.ServiceProvider;
using System.Data;

namespace Teddy
{
    public partial class CommsForm : Form
    {
        // Create object used for the DLL 
        MessageCodec_dll messageCodec = new MessageCodec_dll();

        // Place to store reference to parent so that we can talk to it
        MainForm parent;

        // Delegates to receiveTimer asynchronous calls for messing
        // with the text property on the debugTextBox control
        // and boolean values in a checkbox.
        delegate void SetTextCallback(string text);
        delegate void SetControlTextCallback(Control control, string text);
        delegate void SetControlVisibilityCallback(Control control, Boolean onNotOff);
        delegate void SetCheckBoxCallback(CheckBox checkBox, Boolean onNotOff);
        delegate void SetButtonEnabledCallback(Button button, Boolean onNotOff);

        // Declare functions to communicate with MainForm
        public UpdateDataRowDelegate UpdateSensorDataCallback;
        public UpdateDataRowDelegate UpdateTrafficDataCallback;
        public UpdateUInt32Delegate UpdateReportingIntervalCallback;
        public UpdateUInt32Delegate UpdateHeartbeatCallback;
        public UpdateBooleanDelegate UpdateConnectionCallback;
        public UpdateStringDelegate UpdateInitIndCallback;
        public UpdateVoidDelegate UpdatePollIndCallback;

        // Neul connection stuff
        private Connection connection = null;
        private System.Threading.Timer receiveTimer;
        private const UInt32 receiveTimerDurationMs = 1000; // Not less than one second
        private Object receiveLock = new Object();
        BackgroundWorker disconnectThread = new BackgroundWorker();

        // Message handler stuff
        private bool messageCodecReady;
        private int uartEndpoint;
        private static byte[] txDatagramBuffer;

        // Some strings
        private const string commsFormTitleDisconnected = "Setup Form";
        private const string commsFormTitleSyncing = commsFormTitleDisconnected + " SYNCHRONISING";
        private const string commsFormTitleConnected = commsFormTitleDisconnected + " CONNECTED AND SYNCHRONISED";
        private const string rebootButtonText = "Reboot";

        // Constants to do with dynamic data calculated from John Haine's spreadsheet
        public const Int64 perBitRxnWh = 1850;  // Rx Energy for 20 byte packet is 296 uWh
        public const Int64 perBitTxnWh = 1983;  // Tx Energy for 20 byte packet is 317 uWh
        public const Int64 perTRxnWh = 16566;  //  This represents the idle time, which is 17 uWh
        public const Int64 leakagenWh = 3000;

        // Some flags
        private Boolean gotReportingIntervalMinutes = false;
        private Boolean gotHeartbeatSeconds = false;

        // Some variables
        private Int32 rebootDurationS = 10;
        private Int32 rebootTickS = 1;
        public const Int32 rssidBmMax = -120;
        public const Int32 rssidBmMin = -135;
        private Int32 rssidBm = ((rssidBmMax - rssidBmMin) / 2) + rssidBmMin;
        private UInt32 datagramsSent = 0;
        private UInt32 datagramsReceived = 0;
        private UInt64 powerUWHTotal = 0;
        private UInt64 powerLastReportTime = 0;

        ///
        /// PUBLIC methods
        ///

        // Save and close stuff
        public void close()
        {
            if (connection != null)
            {
                if (disconnectThread.IsBusy != true)
                {
                    disconnectThread.RunWorkerAsync();
                }
            }
        }

        ///
        /// PRIVATE methods
        ///

        ///
        /// Configuration methods called at start/end of day
        ///

        // Constructor
        public CommsForm(MainForm parent)
        {
            InitializeComponent();
            this.parent = parent;

            // Hook in the event handlers for the form
            this.Load += new System.EventHandler(this.CommsForm_Load);
            this.FormClosing += commsForm_FormClosing;

            // Hook in the handlers for the thread that does disconnections
            disconnectThread.DoWork += new DoWorkEventHandler(disconnectThreadDo);
            disconnectThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(disconnectThreadRunWorkerCompleted);

            // Hook in-delegates on parent form
            this.UpdateSensorDataCallback += new UpdateDataRowDelegate(parent._updateSensorDataCallback);
            this.UpdateTrafficDataCallback += new UpdateDataRowDelegate(parent._updateTrafficDataCallback);
            this.UpdateReportingIntervalCallback += new UpdateUInt32Delegate(parent._updateReportingIntervalCallback);
            this.UpdateHeartbeatCallback += new UpdateUInt32Delegate(parent._updateHeartbeatCallback);
            this.UpdateConnectionCallback += new UpdateBooleanDelegate(parent._updateConnectionCallback);
            this.UpdateInitIndCallback += new UpdateStringDelegate(parent._updateInitIndCallback);
            this.UpdatePollIndCallback += new UpdateVoidDelegate(parent._updatePollIndCallback);

            messageCodecReady = false;
            uartEndpoint = 4;
            buttonConnect.Enabled = false;
            setObjectsThatNeedConnection(false);

            buttonReboot.Text = rebootButtonText;
            textBoxHostname.Text = Properties.Settings.Default.Host;
            textBoxUsername.Text = Properties.Settings.Default.Username;
            textBoxPassword.Text = Properties.Settings.Default.Password;
            textBoxUuid.Text = Properties.Settings.Default.Uuid;
            textBoxBatterySizemWh.Text = Properties.Settings.Default.BatterySizemWh.ToString();
            textBoxTxCount.Text = Properties.Settings.Default.TxCount.ToString();
            textBoxBitsTxCount.Text = Properties.Settings.Default.BitsTxCount.ToString();
            textBoxRxCount.Text = Properties.Settings.Default.RxCount.ToString();
            textBoxBitsRxCount.Text = Properties.Settings.Default.BitsRxCount.ToString();
            textBoxTimePassedSeconds.Text = Properties.Settings.Default.TimePassedSeconds.ToString();
            textBoxTRxPerDay.Text = Properties.Settings.Default.TRxPerDay.ToString();
            textBoxBitsPerTx.Text = Properties.Settings.Default.BitsPerTx.ToString();
            textBoxBitsPerRx.Text = Properties.Settings.Default.BitsPerRx.ToString();
            textBoxBitsPerAck.Text = Properties.Settings.Default.BitsPerAck.ToString();
            textBoxBitsProtocolOverhead.Text = Properties.Settings.Default.BitsProtocolOverhead.ToString();
            textBoxCsqValueAtRefdBm.Text = Properties.Settings.Default.CsqValueAtRefdBm.ToString();

            if (parent.noTamperingAllowed)
            {
                textBoxBatterySizemWh.Enabled = false;
                textBoxBitsPerAck.Enabled = false;
                textBoxBitsPerRx.Enabled = false;
                textBoxBitsPerTx.Enabled = false;
                textBoxBitsProtocolOverhead.Enabled = false;
                textBoxBitsRxCount.Enabled = false;
                textBoxBitsTxCount.Enabled = false;
                textBoxTRxPerDay.Enabled = false;
                textBoxRxCount.Enabled = false;
                textBoxTxCount.Enabled = false;
                textBoxTimePassedSeconds.Enabled = false;
                textBoxHostname.Enabled = false;
                textBoxUsername.Enabled = false;
                textBoxPassword.Enabled = false;
                textBoxUuid.Enabled = false;
                textBoxReportingIntervalMinutes.Enabled = false;
                textBoxHeartbeatSeconds.Enabled = false;
                textBoxCsqValueAtRefdBm.Enabled = false;
                buttonSetReportingInterval.Enabled = false;
                buttonSetHeartbeat.Enabled = false;
                buttonResetEnergyUsage.Enabled = false;
                buttonReboot.Enabled = false;
                buttonDisconnect.Enabled = false;
                buttonExitAll.Enabled = false;
            }
        }

        // Bind to the Message Handler DLL on form load
        private void CommsForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Enable printfs from dll to be displayed in IDE 
                messageCodec.onConsoleTrace += new MessageCodec_dll.ConsoleTrace(testOnConsoleTrace);
                messageCodec.bindDll(@"Resources\\teddy_msg_codec.dll");
            }

            catch (Exception)
            {
                // Exception is caught in the binding function itself
            }
        }

        // Don't ever close
        private void commsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            ((Control)sender).Visible = false;
        }

        ///
        /// Methods to set the state of objects on the form
        ///

        // Set the state of objects that need a connection to NeulNet
        private void setObjectsThatNeedConnection(Boolean connectionAvailable)
        {
            if (!parent.noTamperingAllowed)
            {
                buttonDisconnect.Enabled = connectionAvailable;
                buttonReboot.Enabled = connectionAvailable;
                buttonSetReportingInterval.Enabled = connectionAvailable;
                buttonSetHeartbeat.Enabled = connectionAvailable;
            }
        }

        // Reset the settings items to defaults
        private void resetSettings()
        {
            gotHeartbeatSeconds = false;
            gotReportingIntervalMinutes = false;
            datagramsSent = 0;
            datagramsReceived = 0;
        }

        // Check if it's OK to enable the connect button
        private Boolean buttonConnectEnableOk()
        {
            Boolean enabled = false;

            if (messageCodecReady &&
                (connection == null) &&
                (textBoxHostname.Text.Trim().Length > 0) &&
                (textBoxUsername.Text.Trim().Length > 0) &&
                (textBoxPassword.Text.Trim().Length > 0))
            {
                enabled = true;
            }

            return enabled;
        }

        // Enable the connect button
        private void buttonConnectEnable()
        {
            buttonConnect.Enabled = buttonConnectEnableOk();
        }

        // Enable or disable the Connect button after a host character is entered
        private void textBoxHostname_TextChanged(object sender, EventArgs e)
        {
            buttonConnectEnable();
        }

        // Enable or disable the Connect button after a username character is entered
        private void textBoxUsername_TextChanged(object sender, EventArgs e)
        {
            buttonConnectEnable();
        }

        // Enable or disable the Connect button after a password character is entered
        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {
            buttonConnectEnable();
        }

        ///
        /// Methods to click things automatically if Enter is pressed
        ///

        // Click the Connect button if enter is pressed on the Host form
        private void textBoxHostname_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (buttonConnect.Enabled && (e.KeyChar == (char)Keys.Enter))
            {
                buttonConnect.PerformClick();
                buttonConnect.Focus();
                e.Handled = true;
            }
        }

        // Click the Connect button if enter is pressed on the Username form
        private void textBoxUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (buttonConnect.Enabled && (e.KeyChar == (char)Keys.Enter))
            {
                buttonConnect.PerformClick();
                buttonConnect.Focus();
                e.Handled = true;
            }
        }

        // Click the Connect button if enter is pressed on the Password form
        private void textBoxPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (buttonConnect.Enabled && (e.KeyChar == (char)Keys.Enter))
            {
                buttonConnect.PerformClick();
                buttonConnect.Focus();
                e.Handled = true;
            }
        }

        // Filter entry to the Battery Size field and
        // save the value if enter is pressed
        private void textBoxBatterySizemWh_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Properties.Settings.Default.BatterySizemWh = Convert.ToUInt32(textBoxBatterySizemWh.Text);
                if (Properties.Settings.Default.TxCount > Properties.Settings.Default.BatterySizemWh)
                {
                    Properties.Settings.Default.TxCount = Properties.Settings.Default.BatterySizemWh;
                    textBoxTxCount.Text = Properties.Settings.Default.TxCount.ToString();
                }

                Properties.Settings.Default.Save();
                // Move the cursor on
                textBoxBatterySizemWh.Enabled = false;
                textBoxBatterySizemWh.Enabled = true;
                e.Handled = true;
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        // Filter entry to the Tx Count field and
        // save the value if enter is pressed
        private void textBoxTxCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Properties.Settings.Default.TxCount = Convert.ToUInt32(textBoxTxCount.Text);

                Properties.Settings.Default.Save();
                // Move the cursor on
                textBoxTxCount.Enabled = false;
                textBoxTxCount.Enabled = true;
                e.Handled = true;
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        // Filter entry to the Bits Tx Count field and
        // save the value if enter is pressed
        private void textBoxBitsTxCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Properties.Settings.Default.BitsTxCount = Convert.ToUInt32(textBoxBitsTxCount.Text);

                Properties.Settings.Default.Save();
                // Move the cursor on
                textBoxTxCount.Enabled = false;
                textBoxTxCount.Enabled = true;
                e.Handled = true;
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        // Filter entry to the Rx Count field and
        // save the value if enter is pressed
        private void textBoxRxCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Properties.Settings.Default.RxCount = Convert.ToUInt32(textBoxRxCount.Text);

                Properties.Settings.Default.Save();
                // Move the cursor on
                textBoxRxCount.Enabled = false;
                textBoxRxCount.Enabled = true;
                e.Handled = true;
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        // Filter entry to the Bits Rx Count field and
        // save the value if enter is pressed
        private void textBoxBitsRxCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Properties.Settings.Default.BitsRxCount = Convert.ToUInt32(textBoxBitsRxCount.Text);

                Properties.Settings.Default.Save();
                // Move the cursor on
                textBoxBitsRxCount.Enabled = false;
                textBoxBitsRxCount.Enabled = true;
                e.Handled = true;
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        // Filter entry to the TRX Per Day field and
        // save the value if enter is pressed
        private void textBoxTRxPerDay_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Properties.Settings.Default.TRxPerDay = Convert.ToUInt32(textBoxTRxPerDay.Text);
                Properties.Settings.Default.Save();
                // Move the cursor on
                textBoxTRxPerDay.Enabled = false;
                textBoxTRxPerDay.Enabled = true;
                e.Handled = true;
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        // Filter entry to the Bits Per Tx field and
        // save the value if enter is pressed
        private void textBoxBitsPerTx_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Properties.Settings.Default.BitsPerTx = Convert.ToUInt32(textBoxBitsPerTx.Text);
                Properties.Settings.Default.Save();
                // Move the cursor on
                textBoxBitsPerTx.Enabled = false;
                textBoxBitsPerTx.Enabled = true;
                e.Handled = true;
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        // Filter entry to the Bits Per Rx field and
        // save the value if enter is pressed
        private void textBoxBitsPerRx_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Properties.Settings.Default.BitsPerRx = Convert.ToUInt32(textBoxBitsPerRx.Text);
                Properties.Settings.Default.Save();
                // Move the cursor on
                textBoxBitsPerRx.Enabled = false;
                textBoxBitsPerRx.Enabled = true;
                e.Handled = true;
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        // Filter entry to the Bits Per Ack field and
        // save the value if enter is pressed
        private void textBoxBitsPerAck_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Properties.Settings.Default.BitsPerAck = Convert.ToUInt32(textBoxBitsPerAck.Text);
                Properties.Settings.Default.Save();
                // Move the cursor on
                textBoxBitsPerAck.Enabled = false;
                textBoxBitsPerAck.Enabled = true;
                e.Handled = true;
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        // Filter entry to the Bits Protocol Overhead field and
        // save the value if enter is pressed
        private void textBoxBitsProtocolOverhead_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Properties.Settings.Default.BitsProtocolOverhead = Convert.ToUInt32(textBoxBitsProtocolOverhead.Text);
                Properties.Settings.Default.Save();
                // Move the cursor on
                textBoxBitsProtocolOverhead.Enabled = false;
                textBoxBitsProtocolOverhead.Enabled = true;
                e.Handled = true;
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        // Filter entry to the reference CSQ value field
        // save the value if enter is pressed
        private void textBoxCsqValueAtRefdBm_KeyPress(object sender, KeyPressEventArgs e)
        {
            UInt32 value;
            if (e.KeyChar == (char)Keys.Enter)
            {
                value = Convert.ToUInt32(textBoxCsqValueAtRefdBm.Text);
                if ((value >= 0) && (value <= 100))
                {
                    Properties.Settings.Default.CsqValueAtRefdBm = value;
                    Properties.Settings.Default.Save();
                    // Move the cursor on
                    textBoxCsqValueAtRefdBm.Enabled = false;
                    textBoxCsqValueAtRefdBm.Enabled = true;
                    e.Handled = true;
                }
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        // Filter entry to the Time Passed field and
        // save the value if enter is pressed
        private void textBoxTimePassedSeconds_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Properties.Settings.Default.TimePassedSeconds = Convert.ToUInt32(textBoxTimePassedSeconds.Text);
                Properties.Settings.Default.Save();
                // Move the cursor on
                textBoxTimePassedSeconds.Enabled = false;
                textBoxTimePassedSeconds.Enabled = true;
                e.Handled = true;
            }
            else
            {
                if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        ///
        /// Methods to do with setting the reporting and reading intervals
        ///

        // Enable or disable the button that sends the reporting interval to the target
        private void buttonSetReportingIntervalEnable()
        {
            if (!parent.noTamperingAllowed)
            {
                try
                {
                    UInt32 reportingIntervalMinutes = Convert.ToUInt32(textBoxReportingIntervalMinutes.Text);
                    if (messageCodecReady && (reportingIntervalMinutes > 0) && (connection != null))
                    {
                        buttonSetReportingInterval.Enabled = true;
                    }
                    else
                    {
                        buttonSetReportingInterval.Enabled = false;
                    }
                }

                catch (System.FormatException)
                {
                }
            }
            else
            {
                buttonSetReportingInterval.Enabled = false;
            }
        }

        // Enable the Set button if a non-zero value is entered in the ReportingIntervalMinutes box
        private void textBoxReportingIntervalMinutes_TextChanged(object sender, EventArgs e)
        {
            buttonSetReportingIntervalEnable();
        }

        // Click the Set button if enter is pressed on the ReportingIntervalMinutes form
        private void textBoxReportingIntervalMinutes_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (buttonSetReportingInterval.Enabled && (e.KeyChar == (char)Keys.Enter))
            {
                buttonSetReportingInterval.PerformClick();
                buttonSetReportingInterval.Focus();
                e.Handled = true;
            }
        }

        // Enable or disable the button that sends the reporting interval to the target
        private void buttonSetHeartbeatEnable()
        {
            if (!parent.noTamperingAllowed)
            {
                try
                {
                    UInt32 heartbeatSeconds = Convert.ToUInt32(textBoxHeartbeatSeconds.Text);
                    if (messageCodecReady && (heartbeatSeconds > 0) && (connection != null))
                    {
                        buttonSetHeartbeat.Enabled = true;
                    }
                    else
                    {
                        buttonSetHeartbeat.Enabled = false;
                    }
                }

                catch (System.FormatException)
                {
                }
            }
            else
            {
                buttonSetHeartbeat.Enabled = false;
            }
        }

        // Enable the Set button if a non-zero value is entered in the HeartbeatSeconds box
        private void textBoxHeartbeatSeconds_TextChanged(object sender, EventArgs e)
        {
            buttonSetHeartbeatEnable();
        }

        // Click the Set button if enter is pressed on the HeartbeatSeconds box
        private void textBoxHeartbeatSeconds_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (buttonSetHeartbeat.Enabled && (e.KeyChar == (char)Keys.Enter))
            {
                buttonSetHeartbeat.PerformClick();
                buttonSetHeartbeat.Focus();
                e.Handled = true;
            }
        }

        ///
        /// Methods to do with making the connection to NeulNet
        ///

        // Connect to NeulNet and send some initial messages
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                updateControlText(this, commsFormTitleDisconnected);
                connection = Connection.Create(textBoxHostname.Text, textBoxUsername.Text, textBoxPassword.Text);
                testOnConsoleTrace(String.Format("Connecting to {0}...", textBoxHostname.Text));
                // testOnConsoleTrace("Purging old messages...");
                // Must do this _before_ opening the connection otherwise the DLL
                // pulls all the messages down locally and the purge is pointless
                // connection.PurgeMessages();
                // testOnConsoleTrace("Purging completed, opening connection...");
                connection.Open();
                testOnConsoleTrace("Connection opened.");

                Properties.Settings.Default.Username = textBoxUsername.Text;
                Properties.Settings.Default.Password = textBoxPassword.Text;
                Properties.Settings.Default.Uuid = textBoxUuid.Text;
                Properties.Settings.Default.Host = textBoxHostname.Text;
                Properties.Settings.Default.Save();

                UpdateConnectionCallback(true);

                receiveTimer = new System.Threading.Timer(receiveCallback, null, receiveTimerDurationMs, 0);

                if (!parent.testMode)
                {
                    sendIntervalsGetReqDlMsg();
                }

                updateControlText(this, commsFormTitleSyncing);
                if (!parent.noTamperingAllowed)
                {
                    textBoxHostname.Enabled = false;
                    textBoxUsername.Enabled = false;
                    textBoxPassword.Enabled = false;

                    textBoxUuid.Enabled = false;
                }

                // Always allow this to be disabled
                buttonConnect.Enabled = false;

                setObjectsThatNeedConnection(true);
                buttonSetReportingIntervalEnable();
                buttonSetHeartbeatEnable();
                updateControlVisibility(this, false);
            }
            catch (Exception exception)
            {
                UpdateConnectionCallback(false);
                setObjectsThatNeedConnection(false);
                testOnConsoleTrace(String.Format("Connection attempt failed: \"{0}\".", exception.Message));
            }
        }

        // Disconnect from NeulNet
        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (connection != null)
            {
                testOnConsoleTrace("Disconnecting...");

                // We cannot disconnect the connection willy-nilly as
                // the receive callback maybe active at the time.  The
                // Neul example uses lock() to protect against this but
                // the receive function here manipulates data on this form
                // and so we'd end up in deadlock if we did that.  Instead
                // launch another thread to handle the disconnection process.
                if (disconnectThread.IsBusy != true)
                {
                    disconnectThread.RunWorkerAsync();
                }
            }
        }

        // Do the disconnect thing, for use in it's own thread
        private void disconnectThreadDo(object sender, DoWorkEventArgs e)
        {
            lock (receiveLock)
            {
                receiveTimer.Dispose();
                connection.Shutdown();
                connection = null;
            }
        }

        // Do what's necessary on completion of the Disconnect operation
        private void disconnectThreadRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Text = commsFormTitleDisconnected;
            testOnConsoleTrace("Disconnected.");
            if (!parent.noTamperingAllowed)
            {
                textBoxHostname.Enabled = true;
                textBoxUsername.Enabled = true;
                textBoxPassword.Enabled = true;
                textBoxUuid.Enabled = true;
            }

            // Always allow this to be enabled
            buttonConnect.Enabled = true;

            setObjectsThatNeedConnection(false);
        }

        ///
        /// Methods to handle interaction with the objects on the form directly
        ///

        // Shut everything down
        private void buttonExitAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit the whole application?", "Careful!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                parent.Close();
            }
        }

        // Set the reporting interval of the device
        private void buttonSetReportingInterval_Click(object sender, EventArgs e)
        {
            try
            {
                UInt32 reportingIntervalMinutes = Convert.ToUInt32(textBoxReportingIntervalMinutes.Text);
                if (reportingIntervalMinutes > 0)
                {
                    sendReportingIntervalSetReqDlMsg(reportingIntervalMinutes);
                }
            }

            catch (System.FormatException)
            {
            }
        }

        // Set the readings interval on the device
        private void buttonSetHeartbeat_Click(object sender, EventArgs e)
        {
            try
            {
                UInt32 heartbeatSeconds = Convert.ToUInt32(textBoxHeartbeatSeconds.Text);
                if (heartbeatSeconds > 0)
                {
                    sendHeartbeatSetReqDlMsg(heartbeatSeconds);
                }
            }

            catch (System.FormatException)
            {
            }
        }

        // Reboot the device
        private void buttonRebootDevice_Click(object sender, EventArgs e)
        {
            UInt32 x;

            setObjectsThatNeedConnection(false);
            this.Refresh();

            sendRebootReqDlMsg(false);

            resetSettings();

            for (x = 0; x < rebootDurationS / rebootTickS; x++)
            {
                Thread.Sleep(rebootTickS * 1000);
                buttonReboot.Text = rebootButtonText + String.Format(" {0}", (rebootDurationS / rebootTickS) - x);
                buttonReboot.Refresh();
            }
            buttonReboot.Text = rebootButtonText;
            setObjectsThatNeedConnection(true);
            buttonSetReportingIntervalEnable();
        }

        // Reset all the energy usage to zero, with a confirmation dialog
        private void buttonResetEnergyUsage_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset the data history to zero?", "Careful!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                Properties.Settings.Default.TxCount = 0;
                Properties.Settings.Default.BitsTxCount = 0;
                Properties.Settings.Default.RxCount = 0;
                Properties.Settings.Default.BitsRxCount = 0;
                Properties.Settings.Default.TimePassedSeconds = 0;
                Properties.Settings.Default.Save();

                textBoxTxCount.Text = Properties.Settings.Default.TxCount.ToString();
                textBoxBitsTxCount.Text = Properties.Settings.Default.BitsTxCount.ToString();
                textBoxRxCount.Text = Properties.Settings.Default.RxCount.ToString();
                textBoxBitsRxCount.Text = Properties.Settings.Default.BitsRxCount.ToString();
                textBoxTimePassedSeconds.Text = Properties.Settings.Default.TimePassedSeconds.ToString();

                if (System.IO.File.Exists(MainForm.energyDataFilename))
                {
                    System.IO.File.Delete(MainForm.energyDataFilename);
                }
                parent.energyTable.Clear();
            }
        }

        // Print to the debug window
        void testOnConsoleTrace(string data)
        {
            // Print debug in a threadsafe way
            if (textBoxDebug.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(_testOnConsoleTrace);
                this.Invoke(d, new object[] { data });
            }
            else
            {
                _testOnConsoleTrace (data);
            }

        }
        private void _testOnConsoleTrace(String text)
        {
            String printString = DateTime.UtcNow.ToLongTimeString() + ": " + text;

            if (textBoxDebug.TextLength + printString.Length + Environment.NewLine.Length > textBoxDebug.MaxLength)
            {
                textBoxDebug.Text = textBoxDebug.Text.Remove(textBoxDebug.TextLength - printString.Length - Environment.NewLine.Length);
            }
            textBoxDebug.Text = printString + Environment.NewLine + textBoxDebug.Text;

            if (text.Trim().Equals("MessageCodec::ready."))
            {
                Boolean enabled;

                messageCodecReady = true;
                enabled = buttonConnectEnableOk();
                // Set up the buffer for sending messages
                txDatagramBuffer = new byte[messageCodec.maxDatagramSizeRaw()];

                // Enable the connect button if it can be (across threads)
                if (buttonConnect.InvokeRequired)
                {
                    SetButtonEnabledCallback update = new SetButtonEnabledCallback(_updateButtonEnabled);
                    buttonConnect.Invoke(update, new object[] { buttonConnect, enabled });
                }
                else
                {
                    _updateButtonEnabled(buttonConnect, enabled);
                }

            }
        }
        private void _updateButtonEnabled(Button button, Boolean onNotOff)
        {
            button.Enabled = onNotOff;
        }

        //
        // Functions to send messages.
        //

        // Send RebootReq
        private void sendRebootReqDlMsg(Boolean devModeOnNotOff)
        {
            unsafe
            {
                fixed (byte* pBuffer = &txDatagramBuffer[0])
                {
                    UInt32 bytesEncoded;
                    Guid guid = new Guid(textBoxUuid.Text);

                    bytesEncoded = messageCodec.encodeRebootReqDlMsg(pBuffer, devModeOnNotOff);
                    Array.Resize(ref txDatagramBuffer, (int)bytesEncoded);
                    connection.Send(guid, uartEndpoint, txDatagramBuffer);
                    datagramsSent++;
                    testOnConsoleTrace(String.Format("Sent to endpoint {0} RebootReq ({1} byte message) [{2}]: 0x{3}.", uartEndpoint, txDatagramBuffer.Length, datagramsSent, BitConverter.ToString(txDatagramBuffer)));
                    updateDataAfterRx(bytesEncoded + 1); // +1 for endpoint byte
                    Array.Resize(ref txDatagramBuffer, (int)messageCodec.maxDatagramSizeRaw());
                }
            }
        }

        // Send IntervalsGetReq
        private void sendIntervalsGetReqDlMsg()
        {
            unsafe
            {
                fixed (byte* pBuffer = &txDatagramBuffer[0])
                {
                    UInt32 bytesEncoded;
                    Guid guid = new Guid(textBoxUuid.Text);

                    bytesEncoded = messageCodec.encodeIntervalsGetReqDlMsg(pBuffer);
                    Array.Resize(ref txDatagramBuffer, (int)bytesEncoded);
                    connection.Send(guid, uartEndpoint, txDatagramBuffer);
                    datagramsSent++;
                    testOnConsoleTrace(String.Format("Sent to endpoint {0} IntervalsGetReq ({1} byte message) [{2}]: 0x{3}.", uartEndpoint, txDatagramBuffer.Length, datagramsSent, BitConverter.ToString(txDatagramBuffer)));
                    updateDataAfterRx(bytesEncoded + 1); // +1 for endpoint byte
                    Array.Resize(ref txDatagramBuffer, (int)messageCodec.maxDatagramSizeRaw());
                }
            }
        }

        // Send ReportingIntervalSetReq
        private void sendReportingIntervalSetReqDlMsg(UInt32 reportingIntervalMinutes)
        {
            unsafe
            {
                fixed (byte* pBuffer = &txDatagramBuffer[0])
                {
                    UInt32 bytesEncoded;
                    Guid guid = new Guid(textBoxUuid.Text);

                    bytesEncoded = messageCodec.encodeReportingIntervalSetReqDlMsg(pBuffer, reportingIntervalMinutes);
                    Array.Resize(ref txDatagramBuffer, (int)bytesEncoded);
                    connection.Send(guid, uartEndpoint, txDatagramBuffer);
                    datagramsSent++;
                    testOnConsoleTrace(String.Format("Sent to endpoint {0} ReportingIntervalSetReq ({1} byte message) [{2}]: 0x{3}.", uartEndpoint, txDatagramBuffer.Length, datagramsSent, BitConverter.ToString(txDatagramBuffer)));
                    updateDataAfterRx(bytesEncoded + 1); // +1 for endpoint byte
                    Array.Resize(ref txDatagramBuffer, (int)messageCodec.maxDatagramSizeRaw());
                }
            }
        }

        // Send HeartbeatSetReq
        private void sendHeartbeatSetReqDlMsg(UInt32 heartbeatSeconds)
        {
            unsafe
            {
                fixed (byte* pBuffer = &txDatagramBuffer[0])
                {
                    UInt32 bytesEncoded;
                    Guid guid = new Guid(textBoxUuid.Text);

                    bytesEncoded = messageCodec.encodeHeartbeatSetReqDlMsg(pBuffer, heartbeatSeconds);
                    Array.Resize(ref txDatagramBuffer, (int)bytesEncoded);
                    connection.Send(guid, uartEndpoint, txDatagramBuffer);
                    datagramsSent++;
                    testOnConsoleTrace(String.Format("Sent to endpoint {0} HeartbeatSetReq ({1} byte message) [{2}]: 0x{3}.", uartEndpoint, txDatagramBuffer.Length, datagramsSent, BitConverter.ToString(txDatagramBuffer)));
                    updateDataAfterRx(bytesEncoded + 1); // +1 for endpoint byte
                    Array.Resize(ref txDatagramBuffer, (int)messageCodec.maxDatagramSizeRaw());
                }
            }
        }

        // Send SensorsReportGetReq
        private void sendSensorsReportGetReqDlMsg()
        {
            unsafe
            {
                fixed (byte* pBuffer = &txDatagramBuffer[0])
                {
                    UInt32 bytesEncoded;
                    Guid guid = new Guid(textBoxUuid.Text);

                    bytesEncoded = messageCodec.encodeSensorsReportGetReqDlMsg(pBuffer);
                    Array.Resize(ref txDatagramBuffer, (int)bytesEncoded);
                    connection.Send(guid, uartEndpoint, txDatagramBuffer);
                    datagramsSent++;
                    testOnConsoleTrace(String.Format("Sent to endpoint {0} SensorsReportGetReq ({1} byte message) [{2}]: 0x{3}.", uartEndpoint, txDatagramBuffer.Length, datagramsSent, BitConverter.ToString(txDatagramBuffer)));
                    updateDataAfterRx(bytesEncoded + 1); // +1 for endpoint byte
                    Array.Resize(ref txDatagramBuffer, (int)messageCodec.maxDatagramSizeRaw());
                }
            }
        }

        // Send TrafficReportGetReq
        private void sendTrafficReportGetReqDlMsg()
        {
            unsafe
            {
                fixed (byte* pBuffer = &txDatagramBuffer[0])
                {
                    UInt32 bytesEncoded;
                    Guid guid = new Guid(textBoxUuid.Text);

                    bytesEncoded = messageCodec.encodeTrafficReportGetReqDlMsg(pBuffer);
                    Array.Resize(ref txDatagramBuffer, (int)bytesEncoded);
                    connection.Send(guid, uartEndpoint, txDatagramBuffer);
                    datagramsSent++;
                    testOnConsoleTrace(String.Format("Sent to endpoint {0} TrafficReportGetReq ({1} byte message) [{2}]: 0x{3}.", uartEndpoint, txDatagramBuffer.Length, datagramsSent, BitConverter.ToString(txDatagramBuffer)));
                    updateDataAfterRx(bytesEncoded + 1); // +1 for endpoint byte
                    Array.Resize(ref txDatagramBuffer, (int)messageCodec.maxDatagramSizeRaw());
                }
            }
        }

        //
        // Function to process received messages.
        //

        // Timer callback for polling NeulNet
        private void receiveCallback(object state)
        {
            lock (receiveLock)
            {
                if (connection != null)
                {
                    var jsonMsg = connection.Receive(100);
                    if (jsonMsg != null)
                    {
                        var rsp = jsonMsg as JsonMessages.AmqpResponse;
                        if (rsp != null)
                        {
                            datagramsReceived++;
                            testOnConsoleTrace(String.Format("Received [{0}]: {1}", datagramsReceived, rsp.ToString()));
                            processReceivedDatagram(ref rsp);
                        }
                    }

                    receiveTimer.Change(receiveTimerDurationMs, 0);
                }
            }

            Properties.Settings.Default.TimePassedSeconds += receiveTimerDurationMs / 1000;
            updateControlText(textBoxTimePassedSeconds, Properties.Settings.Default.TimePassedSeconds.ToString());
            Properties.Settings.Default.Save();
        }

        // Process a received message
        private void processReceivedDatagram(ref JsonMessages.AmqpResponse jsonMsg)
        {
            unsafe
            {
                try
                {
                    fixed (byte* pBuffer = &(jsonMsg.Data[0]))
                    {
                        MessageCodec_dll.CSDecodeResult decodeResult;
                        byte* pNext = pBuffer;
                        byte** ppNext = &pNext;

                        updateDataAfterTx((UInt32)jsonMsg.Data.Length);
                        while (pNext < pBuffer + jsonMsg.Data.Length)
                        {
                            decodeResult = messageCodec.decodeUlMsgType(pNext, (UInt32)jsonMsg.Data.Length);

                            switch (decodeResult)
                            {
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_INIT_IND_UL_MSG):
                                {
                                    UInt32 wakeUpCode;
                                    UInt32 revisionLevel;
                                    decodeResult = messageCodec.decodeUlMsgInitInd(ppNext, (UInt32)jsonMsg.Data.Length, &wakeUpCode, &revisionLevel);
                                    testOnConsoleTrace(String.Format("Message decode: InitIndUlMsg, wake-up code {0}, revisionLevel {1}.", wakeUpCode.ToString(), revisionLevel.ToString()));
                                    if (revisionLevel != messageCodec.revisionLevel())
                                    {
                                        testOnConsoleTrace(String.Format ("!!!!! tedI is at revision level {0} whereas this codec is revision {1}!!!!!", (int) revisionLevel, (int) messageCodec.revisionLevel()));
                                    }

                                    /* Device has (re)started so sync with it's settings */
                                    resetSettings();
                                    if (!parent.testMode)
                                    {
                                        sendIntervalsGetReqDlMsg();
                                    }

                                    // Clear stored data
                                    parent._clearSensorDataCallback();
                                    parent._clearTrafficDataCallback();
                                    powerLastReportTime = 0;

                                    UpdateInitIndCallback(wakeUpCodeString(wakeUpCode));

                                    updateControlText(this, commsFormTitleSyncing);
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_INTERVALS_GET_CNF_UL_MSG):
                                {
                                    UInt32 reportingIntervalMinutes;
                                    UInt32 heartbeatSeconds;

                                    decodeResult = messageCodec.decodeUlMsgIntervalsGetCnf(ppNext, (UInt32)jsonMsg.Data.Length, &reportingIntervalMinutes, &heartbeatSeconds);
                                    testOnConsoleTrace(String.Format("Message decode: IntervalsGetCnfUlMsg, reporting {0} min(s), reading {1} secs.", reportingIntervalMinutes.ToString(), heartbeatSeconds.ToString()));
                                    gotReportingIntervalMinutes = true;
                                    updateControlText(textBoxReportingIntervalMinutes, reportingIntervalMinutes.ToString());
                                    UpdateReportingIntervalCallback(reportingIntervalMinutes);
                                    gotHeartbeatSeconds = true;
                                    updateControlText(textBoxHeartbeatSeconds, heartbeatSeconds.ToString());
                                    UpdateHeartbeatCallback(heartbeatSeconds);
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_REPORTING_INTERVAL_SET_CNF_UL_MSG):
                                {
                                    UInt32 reportingIntervalMinutes;

                                    decodeResult = messageCodec.decodeUlMsgReportingIntervalSetCnf(ppNext, (UInt32)jsonMsg.Data.Length, &reportingIntervalMinutes);
                                    testOnConsoleTrace(String.Format("Message decode: ReportingIntervalSetCnfUlMsg, reporting {0} min(s).", reportingIntervalMinutes.ToString()));
                                    gotReportingIntervalMinutes = true;
                                    updateControlText(textBoxReportingIntervalMinutes, reportingIntervalMinutes.ToString());
                                    UpdateReportingIntervalCallback(reportingIntervalMinutes);
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_HEARTBEAT_SET_CNF_UL_MSG):
                                {
                                    UInt32 heartbeatSeconds;

                                    decodeResult = messageCodec.decodeUlMsgHeartbeatSetCnf(ppNext, (UInt32)jsonMsg.Data.Length, &heartbeatSeconds);
                                    testOnConsoleTrace(String.Format("Message decode: HeartbeatSetCnfUlMsg, reading {0} secs.", heartbeatSeconds.ToString()));
                                    gotHeartbeatSeconds = true;
                                    updateControlText(textBoxHeartbeatSeconds, heartbeatSeconds.ToString());
                                    UpdateHeartbeatCallback(heartbeatSeconds);
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_POLL_IND_UL_MSG):
                                {
                                    decodeResult = messageCodec.decodeUlMsgPollInd(ppNext, (UInt32)jsonMsg.Data.Length);
                                    testOnConsoleTrace(String.Format("Message decode: PollIndUlMsg."));
                                    UpdatePollIndCallback();
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_SENSORS_REPORT_GET_CNF_UL_MSG):
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_SENSORS_REPORT_IND_UL_MSG):
                                {
                                    UInt32 time;
                                    Boolean gpsPositionPresent;
                                    Int32 gpsPositionLatitude;
                                    Int32 gpsPositionLongitude;
                                    Int32 gpsPositionElevation;
                                    Int32 gpsPositionSpeed;
                                    Boolean lclPositionPresent;
                                    UInt32 lclPositionOrientation;
                                    UInt32 lclPositionHugsThisPeriod;
                                    UInt32 lclPositionSlapsThisPeriod;
                                    UInt32 lclPositionDropsThisPeriod;
                                    UInt32 lclPositionNudgesThisPeriod;
                                    Boolean soundLevelPresent;
                                    UInt32 soundLevel;
                                    Boolean luminosityPresent;
                                    UInt32 luminosity;
                                    Boolean temperaturePresent;
                                    Int32 temperature;
                                    Boolean rssiPresent;
                                    UInt32 rssi;
                                    Boolean powerStatePresent;
                                    UInt32 powerStateChargeState;
                                    UInt32 powerStateBatteryMV;
                                    UInt32 powerStateEnergyUWH;

                                    // Add a new data point to the collection
                                    DataRow newRow = parent.sensorDataTable.NewRow();

                                    decodeResult = messageCodec.decodeUlMsgSensorsReportxxx(ppNext, (UInt32)jsonMsg.Data.Length,
                                                                                            &time,
                                                                                            &gpsPositionPresent,
                                                                                            &gpsPositionLatitude,
                                                                                            &gpsPositionLongitude,
                                                                                            &gpsPositionElevation,
                                                                                            &gpsPositionSpeed,
                                                                                            &lclPositionPresent,
                                                                                            &lclPositionOrientation,
                                                                                            &lclPositionHugsThisPeriod,
                                                                                            &lclPositionSlapsThisPeriod,
                                                                                            &lclPositionDropsThisPeriod,
                                                                                            &lclPositionNudgesThisPeriod,
                                                                                            &soundLevelPresent,
                                                                                            &soundLevel,
                                                                                            &luminosityPresent,
                                                                                            &luminosity,
                                                                                            &temperaturePresent,
                                                                                            &temperature,
                                                                                            &rssiPresent,
                                                                                            &rssi,
                                                                                            &powerStatePresent,
                                                                                            &powerStateChargeState,
                                                                                            &powerStateBatteryMV,
                                                                                            &powerStateEnergyUWH);
                                    testOnConsoleTrace(String.Format("Message decode: SensorReportxxxMsg, contents..."));
                                                                        
                                    UInt32 hh = time / 3600;
                                    UInt32 mm = (time % 3600) / 60;
                                    UInt32 ss = (time % 3600) % 60;

                                    newRow[parent.dataTableRowTimeName] = new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(time);

                                    testOnConsoleTrace(String.Format("Time: {0} seconds, UTC {1:D2}:{2:D2}:{3:D2}, date/time {4}.", time, hh, mm, ss, newRow[parent.dataTableRowTimeName]));
                                    
                                    if (gpsPositionPresent)
                                    {
                                        newRow[parent.dataTableRowTimeName] = time;
                                        newRow[parent.dataTableRowLatitudeName] = Convert.ToDouble(gpsPositionLatitude) / 60000;
                                        newRow[parent.dataTableRowLongitudeName] = Convert.ToDouble(gpsPositionLongitude) / 60000;
                                        newRow[parent.dataTableRowElevationName] = gpsPositionElevation;
                                        newRow[parent.dataTableRowSpeedName] = gpsPositionSpeed;

                                        testOnConsoleTrace(String.Format("GPS: lat/long {0:N4}/{1:N4} degrees, elev {2} metres, speed {3} km/h.",
                                                           newRow[parent.dataTableRowLatitudeName], newRow[parent.dataTableRowLongitudeName],
                                                           newRow[parent.dataTableRowElevationName], newRow[parent.dataTableRowSpeedName]));
                                    }
                                    if (lclPositionPresent)
                                    {
                                        newRow[parent.dataTableRowOrientationName] = lclPositionOrientation;
                                        newRow[parent.dataTableRowHugsName] = lclPositionHugsThisPeriod;
                                        newRow[parent.dataTableRowSlapsName] = lclPositionSlapsThisPeriod;
                                        newRow[parent.dataTableRowDropsName] = lclPositionDropsThisPeriod;
                                        newRow[parent.dataTableRowNudgesName] = lclPositionNudgesThisPeriod;
                                        testOnConsoleTrace(String.Format("LCL: orientation {0}, hugs {1}, slaps {2}, drops {3}, nudges {4}.",
                                                           newRow[parent.dataTableRowOrientationName], newRow[parent.dataTableRowHugsName],
                                                           newRow[parent.dataTableRowSlapsName], newRow[parent.dataTableRowDropsName],
                                                           newRow[parent.dataTableRowNudgesName]));
                                    }
                                    if (soundLevelPresent)
                                    {
                                        newRow[parent.dataTableRowSoundLevelName] = soundLevel;
                                        testOnConsoleTrace(String.Format("Sound: {0}.", newRow[parent.dataTableRowSoundLevelName]));
                                    }
                                    if (luminosityPresent)
                                    {
                                        newRow[parent.dataTableRowLuminosityName] = luminosity;
                                        testOnConsoleTrace(String.Format("Luminosity: {0}.", newRow[parent.dataTableRowLuminosityName]));
                                    }
                                    if (temperaturePresent)
                                    {
                                        newRow[parent.dataTableRowTemperatureName] = temperature;
                                        testOnConsoleTrace(String.Format("Temperature: {0} C.", newRow[parent.dataTableRowTemperatureName]));
                                    }
                                    if (rssiPresent)
                                    {
                                        newRow[parent.dataTableRowRssiName] = rssi;
                                        rssidBm = inventRssi(rssi);
                                        newRow[parent.dataTableRowRssidBmName] = rssidBm;
                                        testOnConsoleTrace(String.Format("RSSI: {0}, {1} dBm.", newRow[parent.dataTableRowRssiName], newRow[parent.dataTableRowRssidBmName]));
                                    }
                                    if (powerStatePresent)
                                    {
                                        newRow[parent.dataTableRowChargeStateName] = powerStateChargeState;
                                        newRow[parent.dataTableRowBatterymVName] = powerStateBatteryMV;
                                        newRow[parent.dataTableRowEnergyuWhName] = powerStateEnergyUWH;
                                        if (powerLastReportTime > 0)
                                        {
                                            newRow[parent.dataTableRowEnergymWPerHourName] = (double)powerStateEnergyUWH * 3600 / (double)(time - powerLastReportTime) / 1000;
                                        }
                                        powerLastReportTime = time;
                                        powerUWHTotal += powerStateEnergyUWH;
                                        newRow[parent.dataTableRowEnergyTotalmWhName] = powerUWHTotal / 1000;
                                        testOnConsoleTrace(String.Format("Pwr State: charging {0}, voltage {1} mV, energy {2} uWh, mW in an hour {3}, total since reset {4} uWh.",
                                                           newRow[parent.dataTableRowChargeStateName], newRow[parent.dataTableRowBatterymVName],
                                                           newRow[parent.dataTableRowEnergyuWhName], newRow[parent.dataTableRowEnergymWPerHourName],
                                                           newRow[parent.dataTableRowEnergyTotalmWhName]));
                                    }

                                    // Update the data table
                                    UpdateSensorDataCallback(newRow);
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_TRAFFIC_REPORT_GET_CNF_UL_MSG):
                                {
                                    UInt32 numDatagramsSent;
                                    UInt32 numBytesSent;
                                    UInt32 numDatagramsReceived;
                                    UInt32 numBytesReceived;

                                    // Add a new data point to the collection
                                    DataRow newRow = parent.trafficDataTable.NewRow();

                                    decodeResult = messageCodec.decodeUlMsgTrafficReportGetCnf(ppNext, (UInt32)jsonMsg.Data.Length, &numDatagramsSent, &numBytesSent, &numDatagramsReceived, &numBytesReceived);
                                    newRow[parent.dataTableRowUplinkDatagramsName] = numDatagramsSent;
                                    newRow[parent.dataTableRowUplinkBytesName] = numBytesSent;
                                    newRow[parent.dataTableRowDownlinkDatagramsName] = numDatagramsReceived;
                                    newRow[parent.dataTableRowDownlinkBytesName] = numBytesReceived;

                                    testOnConsoleTrace(String.Format("Message decode: TrafficReportGetCnfUlMsg, reporting {0} bytes sent ({1} datagrams), {2} bytes received ({3} datagrams).",
                                                                     newRow[parent.dataTableRowUplinkBytesName].ToString(), newRow[parent.dataTableRowUplinkDatagramsName].ToString(),
                                                                     newRow[parent.dataTableRowDownlinkBytesName].ToString(), newRow[parent.dataTableRowDownlinkDatagramsName].ToString()));
                                    // Update the data table
                                    UpdateTrafficDataCallback(newRow);
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_TRAFFIC_REPORT_IND_UL_MSG):
                                {
                                    UInt32 numDatagramsSent;
                                    UInt32 numBytesSent;
                                    UInt32 numDatagramsReceived;
                                    UInt32 numBytesReceived;

                                    // Add a new data point to the collection
                                    DataRow newRow = parent.trafficDataTable.NewRow();

                                    decodeResult = messageCodec.decodeUlMsgTrafficReportInd(ppNext, (UInt32)jsonMsg.Data.Length, &numDatagramsSent, &numBytesSent, &numDatagramsReceived, &numBytesReceived);
                                    newRow[parent.dataTableRowUplinkDatagramsName] = numDatagramsSent;
                                    newRow[parent.dataTableRowUplinkBytesName] = numBytesSent;
                                    newRow[parent.dataTableRowDownlinkDatagramsName] = numDatagramsReceived;
                                    newRow[parent.dataTableRowDownlinkBytesName] = numBytesReceived;

                                    testOnConsoleTrace(String.Format("Message decode: TrafficReportIndUlMsg, reporting {0} bytes sent ({1} datagrams), {2} bytes received ({3} datagrams).",
                                                                     newRow[parent.dataTableRowUplinkBytesName].ToString(), newRow[parent.dataTableRowUplinkDatagramsName].ToString(),
                                                                     newRow[parent.dataTableRowDownlinkBytesName].ToString(), newRow[parent.dataTableRowDownlinkDatagramsName].ToString()));
                                    // Update the data table
                                    UpdateTrafficDataCallback(newRow);
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_DEBUG_IND_UL_MSG):
                                {
                                    UInt32 sizeOfString;
                                    byte[] debugString = new byte[messageCodec.maxDebugStringSize()];
                                    fixed (byte * pDebugString = &debugString[0])
                                    {
                                        decodeResult = messageCodec.decodeUlMsgDebugInd(ppNext, (UInt32)jsonMsg.Data.Length, &sizeOfString, pDebugString);
                                        testOnConsoleTrace(String.Format("Message decode: DebugIndUlMsg \"{0}\".", System.Text.Encoding.Default.GetString(debugString, 0, (int) sizeOfString)));
                                    }
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_FAILURE):
                                {
                                    (*ppNext)++;  // Move pNext on.
                                    testOnConsoleTrace("Message decode: failure.");
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_INPUT_TOO_SHORT):
                                {
                                    (*ppNext)++;  // Move pNext on.
                                    testOnConsoleTrace("Message decode: input too short.");
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_OUTPUT_TOO_SHORT):
                                {
                                    (*ppNext)++;  // Move pNext on.
                                    testOnConsoleTrace("Message decode: output too short.");
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_BAD_MSG_FORMAT):
                                {
                                    (*ppNext)++;  // Move pNext on.
                                    testOnConsoleTrace("Message decode: bad message format.");
                                }
                                break;
                                case (MessageCodec_dll.CSDecodeResult.DECODE_RESULT_UNKNOWN_MSG_ID):
                                {
                                    (*ppNext)++;  // Move pNext on.
                                    testOnConsoleTrace("Message decode: unknown message.");
                                }
                                break;
                                default:
                                {
                                    (*ppNext)++;  // Move pNext on.
                                    testOnConsoleTrace("Message decode: unknown decode result.");
                                }
                                break;
                            }
                        }
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    testOnConsoleTrace("JSON contained no data");
                }
            }

            // Check if we've got all the stuff from the device
            if (gotReportingIntervalMinutes && gotHeartbeatSeconds)
            {
                updateControlText (this, commsFormTitleConnected);
            }
        }

        // Return a wakeup string from a code
        private String wakeUpCodeString(UInt32 wakeUpCode)
        {
            String str = wakeUpCode.ToString();
            switch (wakeUpCode)
            {
                case 0:
                    str += " (normal)";
                break;
                case 1:
                    str += " (watchdog timeout?)";
                break;
                case 2:
                    str += " (AT command problem?)";
                break;
                case 3:
                    str += " (network send problem?)";
                break;
                case 4:
                    str += " (memory allocation problem?)";
                break;
                case 5:
                    str += " (protocol problem?)";
                break;
                case 6:
                    str += " (generic failure?)";
                break;
                case 7:
                    str += " (commanded reboot?)";
                break;
                default:
                    str += " (unknown)";
                break;
            }

            return str;
        }

        // Create an RSSI value
        private Int32 inventRssi(UInt32 rssi)
        {
            UInt32 csqAtNeg80dBm = Properties.Settings.Default.CsqValueAtRefdBm;
            if (csqAtNeg80dBm == 0) // If the calibration value is zero, make up an answer
            {
                Random rnd = new Random();
                if (rnd.Next(10) > 8)
                {
                    if (rnd.Next(10) > 5)
                    {
                        rssidBm++;
                        if (rssidBm > rssidBmMax)
                        {
                            rssidBm = rssidBmMax;
                        }
                    }
                    else
                    {
                        rssidBm--;
                        if (rssidBm < rssidBmMin)
                        {
                            rssidBm = rssidBmMin;
                        }
                    }
                }
            }
            else
            {
                // If we've got a calibration value then assume we have a real
                // RSSI value from the module.  Each step is approximately 3dB.
                // We use as a reference point the rssi value for -80 dBm
                // which must be measured and stored by the user (this is
                // the calibration value).
                rssidBm = -80 + ((Int32) rssi - (Int32) csqAtNeg80dBm) * 3;
            }

            return rssidBm;
        }

        //
        // Functions to update forms, text boxes, labels and check boxes on the form either
        // directly or via callbacks since they may be called from the receive message
        // function which is in a different thread.
        //

        // Update the text on a control
        private void updateControlText(Control control, String text)
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
        private void _updateControlText(Control control, String text)
        {
            control.Text = text;
        }

        // Update the visibility of a control on the form
        private void updateControlVisibility(Control control, Boolean onNotOff)
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
        private void _updateControlVisibility(Control control, Boolean onNotOff)
        {
            control.Visible = onNotOff;
        }

        // Update the state of a checkbox on the form
        private void updateCheckBox(CheckBox checkBox, Boolean onNotOff)
        {
            if (checkBox.InvokeRequired)
            {
                SetCheckBoxCallback update = new SetCheckBoxCallback(_updateCheckBox);
                checkBox.Invoke(update, new object[] { checkBox, onNotOff });
            }
            else
            {
                _updateCheckBox(checkBox, onNotOff);
            }
        }
        private void _updateCheckBox(CheckBox checkBox, Boolean onNotOff)
        {
            // IMPORTANT NOTE: Detach the CheckedChanged handlers while doing this so that
            // we don't get stuck in a loop

            if (onNotOff)
            {
                checkBox.CheckState = CheckState.Checked;
            }
            else
            {
                checkBox.CheckState = CheckState.Unchecked;
            }
        }

        //
        // Functions to work out energy consumed
        //

        // Update stored data after a Tx
        private void updateDataAfterTx(UInt32 bytesTx)
        {
            Properties.Settings.Default.TxCount++;
            Properties.Settings.Default.BitsTxCount += (bytesTx * 8) + Properties.Settings.Default.BitsProtocolOverhead;
            Properties.Settings.Default.BitsRxCount += Properties.Settings.Default.BitsPerAck;
            Properties.Settings.Default.Save();
            updateControlText(textBoxTxCount, Properties.Settings.Default.TxCount.ToString());
            updateControlText(textBoxBitsTxCount, Properties.Settings.Default.BitsTxCount.ToString());
            updateControlText(textBoxBitsRxCount, Properties.Settings.Default.BitsRxCount.ToString());
        }

        // Update stored data after an Rx
        private void updateDataAfterRx(UInt32 bytesRx)
        {
            Properties.Settings.Default.RxCount++;
            Properties.Settings.Default.BitsRxCount += (bytesRx * 8) + Properties.Settings.Default.BitsProtocolOverhead;
            Properties.Settings.Default.BitsTxCount += Properties.Settings.Default.BitsPerAck;
            Properties.Settings.Default.Save();
            updateControlText(textBoxRxCount, Properties.Settings.Default.RxCount.ToString());
            updateControlText(textBoxBitsRxCount, Properties.Settings.Default.BitsRxCount.ToString());
            updateControlText(textBoxBitsTxCount, Properties.Settings.Default.BitsTxCount.ToString());
        }
    }
}
