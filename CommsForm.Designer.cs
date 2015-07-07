using System.ComponentModel;

namespace Teddy
{
    partial class CommsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose ();
            }
            base.Dispose (disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommsForm));
            this.labelUsername = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.textBoxDebug = new System.Windows.Forms.TextBox();
            this.labelDebug = new System.Windows.Forms.Label();
            this.buttonReboot = new System.Windows.Forms.Button();
            this.textBoxHeartbeatSeconds = new System.Windows.Forms.TextBox();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.labelHeartbeat = new System.Windows.Forms.Label();
            this.textBoxUuid = new System.Windows.Forms.TextBox();
            this.labelUuid = new System.Windows.Forms.Label();
            this.labelReportingInterval = new System.Windows.Forms.Label();
            this.textBoxReportingIntervalMinutes = new System.Windows.Forms.TextBox();
            this.labelReportingIntervalMinutes = new System.Windows.Forms.Label();
            this.labelRssi = new System.Windows.Forms.Label();
            this.textBoxRssi = new System.Windows.Forms.TextBox();
            this.buttonSetReportingInterval = new System.Windows.Forms.Button();
            this.labelHost = new System.Windows.Forms.Label();
            this.textBoxHostname = new System.Windows.Forms.TextBox();
            this.labelTimePassedSeconds = new System.Windows.Forms.Label();
            this.labelTxCount = new System.Windows.Forms.Label();
            this.textBoxTxCount = new System.Windows.Forms.TextBox();
            this.labelBatterySizemWh = new System.Windows.Forms.Label();
            this.textBoxBatterySizemWh = new System.Windows.Forms.TextBox();
            this.labelTRxPerDay = new System.Windows.Forms.Label();
            this.textBoxTRxPerDay = new System.Windows.Forms.TextBox();
            this.labelBitsPerTx = new System.Windows.Forms.Label();
            this.textBoxBitsPerTx = new System.Windows.Forms.TextBox();
            this.textBoxTimePassedSeconds = new System.Windows.Forms.TextBox();
            this.labelSeconds = new System.Windows.Forms.Label();
            this.labelmWh = new System.Windows.Forms.Label();
            this.labelNorm1 = new System.Windows.Forms.Label();
            this.labelNorm2 = new System.Windows.Forms.Label();
            this.groupBoxEnergyParameters = new System.Windows.Forms.GroupBox();
            this.labelCsqAdvice = new System.Windows.Forms.Label();
            this.buttonResetEnergyUsage = new System.Windows.Forms.Button();
            this.labelCsqValueAtRefdBm = new System.Windows.Forms.Label();
            this.textBoxBitsProtocolOverhead = new System.Windows.Forms.TextBox();
            this.textBoxCsqValueAtRefdBm = new System.Windows.Forms.TextBox();
            this.labelBitsProtocolOverhead = new System.Windows.Forms.Label();
            this.textBoxBitsPerAck = new System.Windows.Forms.TextBox();
            this.labelBitsPerAck = new System.Windows.Forms.Label();
            this.labelNorm3 = new System.Windows.Forms.Label();
            this.textBoxBitsPerRx = new System.Windows.Forms.TextBox();
            this.labelBitsPerRx = new System.Windows.Forms.Label();
            this.textBoxBitsRxCount = new System.Windows.Forms.TextBox();
            this.labelBitsRxCount = new System.Windows.Forms.Label();
            this.textBoxBitsTxCount = new System.Windows.Forms.TextBox();
            this.labelBitsTxCount = new System.Windows.Forms.Label();
            this.textBoxRxCount = new System.Windows.Forms.TextBox();
            this.labelRxCount = new System.Windows.Forms.Label();
            this.groupBoxConnectionParameters = new System.Windows.Forms.GroupBox();
            this.buttonExitAll = new System.Windows.Forms.Button();
            this.groupBoxDevice = new System.Windows.Forms.GroupBox();
            this.buttonSetHeartbeat = new System.Windows.Forms.Button();
            this.labelHeartbeatSeconds = new System.Windows.Forms.Label();
            this.labelOpenHelp = new System.Windows.Forms.Label();
            this.groupBoxEnergyParameters.SuspendLayout();
            this.groupBoxConnectionParameters.SuspendLayout();
            this.groupBoxDevice.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelUsername
            // 
            this.labelUsername.AutoSize = true;
            this.labelUsername.Location = new System.Drawing.Point(23, 44);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(58, 13);
            this.labelUsername.TabIndex = 2;
            this.labelUsername.Text = "Username:";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(84, 41);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(99, 20);
            this.textBoxUsername.TabIndex = 3;
            this.textBoxUsername.WordWrap = false;
            this.textBoxUsername.TextChanged += new System.EventHandler(this.textBoxUsername_TextChanged);
            this.textBoxUsername.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxUsername_KeyPress);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(266, 41);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(99, 20);
            this.textBoxPassword.TabIndex = 5;
            this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxPassword_TextChanged);
            this.textBoxPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxPassword_KeyPress);
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(204, 43);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(56, 13);
            this.labelPassword.TabIndex = 4;
            this.labelPassword.Text = "Password:";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(388, 41);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(64, 20);
            this.buttonConnect.TabIndex = 8;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // textBoxDebug
            // 
            this.textBoxDebug.Location = new System.Drawing.Point(87, 124);
            this.textBoxDebug.Multiline = true;
            this.textBoxDebug.Name = "textBoxDebug";
            this.textBoxDebug.ReadOnly = true;
            this.textBoxDebug.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDebug.Size = new System.Drawing.Size(458, 182);
            this.textBoxDebug.TabIndex = 19;
            this.textBoxDebug.TabStop = false;
            this.textBoxDebug.WordWrap = false;
            // 
            // labelDebug
            // 
            this.labelDebug.AutoSize = true;
            this.labelDebug.Location = new System.Drawing.Point(39, 124);
            this.labelDebug.Name = "labelDebug";
            this.labelDebug.Size = new System.Drawing.Size(42, 13);
            this.labelDebug.TabIndex = 18;
            this.labelDebug.Text = "Debug:";
            // 
            // buttonReboot
            // 
            this.buttonReboot.Location = new System.Drawing.Point(447, 94);
            this.buttonReboot.Name = "buttonReboot";
            this.buttonReboot.Size = new System.Drawing.Size(89, 23);
            this.buttonReboot.TabIndex = 17;
            this.buttonReboot.Text = "Reboot";
            this.buttonReboot.UseVisualStyleBackColor = true;
            this.buttonReboot.Click += new System.EventHandler(this.buttonRebootDevice_Click);
            // 
            // textBoxHeartbeatSeconds
            // 
            this.textBoxHeartbeatSeconds.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxHeartbeatSeconds.Location = new System.Drawing.Point(86, 45);
            this.textBoxHeartbeatSeconds.Name = "textBoxHeartbeatSeconds";
            this.textBoxHeartbeatSeconds.Size = new System.Drawing.Size(97, 20);
            this.textBoxHeartbeatSeconds.TabIndex = 5;
            this.textBoxHeartbeatSeconds.TabStop = false;
            this.textBoxHeartbeatSeconds.TextChanged += new System.EventHandler(this.textBoxHeartbeatSeconds_TextChanged);
            this.textBoxHeartbeatSeconds.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxHeartbeatSeconds_KeyPress);
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Location = new System.Drawing.Point(473, 41);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(69, 20);
            this.buttonDisconnect.TabIndex = 9;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // labelHeartbeat
            // 
            this.labelHeartbeat.AutoSize = true;
            this.labelHeartbeat.Location = new System.Drawing.Point(27, 47);
            this.labelHeartbeat.Name = "labelHeartbeat";
            this.labelHeartbeat.Size = new System.Drawing.Size(57, 13);
            this.labelHeartbeat.TabIndex = 4;
            this.labelHeartbeat.Text = "Heartbeat:";
            // 
            // textBoxUuid
            // 
            this.textBoxUuid.Location = new System.Drawing.Point(85, 67);
            this.textBoxUuid.Name = "textBoxUuid";
            this.textBoxUuid.Size = new System.Drawing.Size(280, 20);
            this.textBoxUuid.TabIndex = 7;
            this.textBoxUuid.WordWrap = false;
            // 
            // labelUuid
            // 
            this.labelUuid.AutoSize = true;
            this.labelUuid.Location = new System.Drawing.Point(49, 70);
            this.labelUuid.Name = "labelUuid";
            this.labelUuid.Size = new System.Drawing.Size(32, 13);
            this.labelUuid.TabIndex = 6;
            this.labelUuid.Text = "Uuid:";
            // 
            // labelReportingInterval
            // 
            this.labelReportingInterval.AutoSize = true;
            this.labelReportingInterval.Location = new System.Drawing.Point(4, 22);
            this.labelReportingInterval.Name = "labelReportingInterval";
            this.labelReportingInterval.Size = new System.Drawing.Size(80, 13);
            this.labelReportingInterval.TabIndex = 0;
            this.labelReportingInterval.Text = "Report Interval:";
            // 
            // textBoxReportingIntervalMinutes
            // 
            this.textBoxReportingIntervalMinutes.Location = new System.Drawing.Point(86, 19);
            this.textBoxReportingIntervalMinutes.Name = "textBoxReportingIntervalMinutes";
            this.textBoxReportingIntervalMinutes.Size = new System.Drawing.Size(97, 20);
            this.textBoxReportingIntervalMinutes.TabIndex = 1;
            this.textBoxReportingIntervalMinutes.TabStop = false;
            this.textBoxReportingIntervalMinutes.TextChanged += new System.EventHandler(this.textBoxReportingIntervalMinutes_TextChanged);
            this.textBoxReportingIntervalMinutes.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxReportingIntervalMinutes_KeyPress);
            // 
            // labelReportingIntervalMinutes
            // 
            this.labelReportingIntervalMinutes.AutoSize = true;
            this.labelReportingIntervalMinutes.Location = new System.Drawing.Point(192, 22);
            this.labelReportingIntervalMinutes.Name = "labelReportingIntervalMinutes";
            this.labelReportingIntervalMinutes.Size = new System.Drawing.Size(43, 13);
            this.labelReportingIntervalMinutes.TabIndex = 2;
            this.labelReportingIntervalMinutes.Text = "minutes";
            // 
            // labelRssi
            // 
            this.labelRssi.AutoSize = true;
            this.labelRssi.Location = new System.Drawing.Point(46, 75);
            this.labelRssi.Name = "labelRssi";
            this.labelRssi.Size = new System.Drawing.Size(35, 13);
            this.labelRssi.TabIndex = 8;
            this.labelRssi.Text = "RSSI:";
            // 
            // textBoxRssi
            // 
            this.textBoxRssi.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxRssi.Location = new System.Drawing.Point(86, 72);
            this.textBoxRssi.Name = "textBoxRssi";
            this.textBoxRssi.ReadOnly = true;
            this.textBoxRssi.Size = new System.Drawing.Size(97, 20);
            this.textBoxRssi.TabIndex = 9;
            this.textBoxRssi.TabStop = false;
            // 
            // buttonSetReportingInterval
            // 
            this.buttonSetReportingInterval.Location = new System.Drawing.Point(242, 18);
            this.buttonSetReportingInterval.Name = "buttonSetReportingInterval";
            this.buttonSetReportingInterval.Size = new System.Drawing.Size(60, 20);
            this.buttonSetReportingInterval.TabIndex = 3;
            this.buttonSetReportingInterval.Text = "Set";
            this.buttonSetReportingInterval.UseVisualStyleBackColor = true;
            this.buttonSetReportingInterval.Click += new System.EventHandler(this.buttonSetReportingInterval_Click);
            // 
            // labelHost
            // 
            this.labelHost.AutoSize = true;
            this.labelHost.Location = new System.Drawing.Point(49, 19);
            this.labelHost.Name = "labelHost";
            this.labelHost.Size = new System.Drawing.Size(32, 13);
            this.labelHost.TabIndex = 0;
            this.labelHost.Text = "Host:";
            // 
            // textBoxHostname
            // 
            this.textBoxHostname.Location = new System.Drawing.Point(84, 15);
            this.textBoxHostname.Name = "textBoxHostname";
            this.textBoxHostname.Size = new System.Drawing.Size(280, 20);
            this.textBoxHostname.TabIndex = 1;
            this.textBoxHostname.WordWrap = false;
            this.textBoxHostname.TextChanged += new System.EventHandler(this.textBoxHostname_TextChanged);
            this.textBoxHostname.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxHostname_KeyPress);
            // 
            // labelTimePassedSeconds
            // 
            this.labelTimePassedSeconds.AutoSize = true;
            this.labelTimePassedSeconds.Location = new System.Drawing.Point(10, 128);
            this.labelTimePassedSeconds.Name = "labelTimePassedSeconds";
            this.labelTimePassedSeconds.Size = new System.Drawing.Size(71, 13);
            this.labelTimePassedSeconds.TabIndex = 8;
            this.labelTimePassedSeconds.Text = "Time Passed:";
            // 
            // labelTxCount
            // 
            this.labelTxCount.AutoSize = true;
            this.labelTxCount.Location = new System.Drawing.Point(28, 24);
            this.labelTxCount.Name = "labelTxCount";
            this.labelTxCount.Size = new System.Drawing.Size(53, 13);
            this.labelTxCount.TabIndex = 0;
            this.labelTxCount.Text = "Tx Count:";
            // 
            // textBoxTxCount
            // 
            this.textBoxTxCount.Location = new System.Drawing.Point(86, 22);
            this.textBoxTxCount.Name = "textBoxTxCount";
            this.textBoxTxCount.Size = new System.Drawing.Size(99, 20);
            this.textBoxTxCount.TabIndex = 1;
            this.textBoxTxCount.WordWrap = false;
            this.textBoxTxCount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxTxCount_KeyPress);
            // 
            // labelBatterySizemWh
            // 
            this.labelBatterySizemWh.AutoSize = true;
            this.labelBatterySizemWh.Location = new System.Drawing.Point(285, 21);
            this.labelBatterySizemWh.Name = "labelBatterySizemWh";
            this.labelBatterySizemWh.Size = new System.Drawing.Size(66, 13);
            this.labelBatterySizemWh.TabIndex = 12;
            this.labelBatterySizemWh.Text = "Battery Size:";
            // 
            // textBoxBatterySizemWh
            // 
            this.textBoxBatterySizemWh.Location = new System.Drawing.Point(355, 19);
            this.textBoxBatterySizemWh.Name = "textBoxBatterySizemWh";
            this.textBoxBatterySizemWh.Size = new System.Drawing.Size(99, 20);
            this.textBoxBatterySizemWh.TabIndex = 13;
            this.textBoxBatterySizemWh.WordWrap = false;
            this.textBoxBatterySizemWh.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxBatterySizemWh_KeyPress);
            // 
            // labelTRxPerDay
            // 
            this.labelTRxPerDay.AutoSize = true;
            this.labelTRxPerDay.Location = new System.Drawing.Point(283, 47);
            this.labelTRxPerDay.Name = "labelTRxPerDay";
            this.labelTRxPerDay.Size = new System.Drawing.Size(68, 13);
            this.labelTRxPerDay.TabIndex = 15;
            this.labelTRxPerDay.Text = "TRx per day:";
            // 
            // textBoxTRxPerDay
            // 
            this.textBoxTRxPerDay.Location = new System.Drawing.Point(355, 45);
            this.textBoxTRxPerDay.Name = "textBoxTRxPerDay";
            this.textBoxTRxPerDay.Size = new System.Drawing.Size(99, 20);
            this.textBoxTRxPerDay.TabIndex = 16;
            this.textBoxTRxPerDay.WordWrap = false;
            this.textBoxTRxPerDay.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxTRxPerDay_KeyPress);
            // 
            // labelBitsPerTx
            // 
            this.labelBitsPerTx.AutoSize = true;
            this.labelBitsPerTx.Location = new System.Drawing.Point(291, 73);
            this.labelBitsPerTx.Name = "labelBitsPerTx";
            this.labelBitsPerTx.Size = new System.Drawing.Size(60, 13);
            this.labelBitsPerTx.TabIndex = 18;
            this.labelBitsPerTx.Text = "Bits per Tx:";
            // 
            // textBoxBitsPerTx
            // 
            this.textBoxBitsPerTx.Location = new System.Drawing.Point(355, 71);
            this.textBoxBitsPerTx.Name = "textBoxBitsPerTx";
            this.textBoxBitsPerTx.Size = new System.Drawing.Size(99, 20);
            this.textBoxBitsPerTx.TabIndex = 19;
            this.textBoxBitsPerTx.WordWrap = false;
            this.textBoxBitsPerTx.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxBitsPerTx_KeyPress);
            // 
            // textBoxTimePassedSeconds
            // 
            this.textBoxTimePassedSeconds.Location = new System.Drawing.Point(87, 126);
            this.textBoxTimePassedSeconds.Name = "textBoxTimePassedSeconds";
            this.textBoxTimePassedSeconds.Size = new System.Drawing.Size(99, 20);
            this.textBoxTimePassedSeconds.TabIndex = 9;
            this.textBoxTimePassedSeconds.WordWrap = false;
            this.textBoxTimePassedSeconds.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxTimePassedSeconds_KeyPress);
            // 
            // labelSeconds
            // 
            this.labelSeconds.AutoSize = true;
            this.labelSeconds.Location = new System.Drawing.Point(192, 129);
            this.labelSeconds.Name = "labelSeconds";
            this.labelSeconds.Size = new System.Drawing.Size(47, 13);
            this.labelSeconds.TabIndex = 10;
            this.labelSeconds.Text = "seconds";
            // 
            // labelmWh
            // 
            this.labelmWh.AutoSize = true;
            this.labelmWh.Location = new System.Drawing.Point(460, 22);
            this.labelmWh.Name = "labelmWh";
            this.labelmWh.Size = new System.Drawing.Size(32, 13);
            this.labelmWh.TabIndex = 14;
            this.labelmWh.Text = "mWh";
            // 
            // labelNorm1
            // 
            this.labelNorm1.AutoSize = true;
            this.labelNorm1.Location = new System.Drawing.Point(460, 48);
            this.labelNorm1.Name = "labelNorm1";
            this.labelNorm1.Size = new System.Drawing.Size(87, 13);
            this.labelNorm1.TabIndex = 17;
            this.labelNorm1.Text = "[in normal usage]";
            // 
            // labelNorm2
            // 
            this.labelNorm2.AutoSize = true;
            this.labelNorm2.Location = new System.Drawing.Point(460, 74);
            this.labelNorm2.Name = "labelNorm2";
            this.labelNorm2.Size = new System.Drawing.Size(87, 13);
            this.labelNorm2.TabIndex = 20;
            this.labelNorm2.Text = "[in normal usage]";
            // 
            // groupBoxEnergyParameters
            // 
            this.groupBoxEnergyParameters.Controls.Add(this.labelCsqAdvice);
            this.groupBoxEnergyParameters.Controls.Add(this.buttonResetEnergyUsage);
            this.groupBoxEnergyParameters.Controls.Add(this.labelCsqValueAtRefdBm);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxBitsProtocolOverhead);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxCsqValueAtRefdBm);
            this.groupBoxEnergyParameters.Controls.Add(this.labelBitsProtocolOverhead);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxBitsPerAck);
            this.groupBoxEnergyParameters.Controls.Add(this.labelBitsPerAck);
            this.groupBoxEnergyParameters.Controls.Add(this.labelNorm3);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxBitsPerRx);
            this.groupBoxEnergyParameters.Controls.Add(this.labelBitsPerRx);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxBitsRxCount);
            this.groupBoxEnergyParameters.Controls.Add(this.labelBitsRxCount);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxBitsTxCount);
            this.groupBoxEnergyParameters.Controls.Add(this.labelBitsTxCount);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxRxCount);
            this.groupBoxEnergyParameters.Controls.Add(this.labelRxCount);
            this.groupBoxEnergyParameters.Controls.Add(this.labelNorm2);
            this.groupBoxEnergyParameters.Controls.Add(this.labelNorm1);
            this.groupBoxEnergyParameters.Controls.Add(this.labelmWh);
            this.groupBoxEnergyParameters.Controls.Add(this.labelSeconds);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxTimePassedSeconds);
            this.groupBoxEnergyParameters.Controls.Add(this.labelTimePassedSeconds);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxBitsPerTx);
            this.groupBoxEnergyParameters.Controls.Add(this.labelBitsPerTx);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxTRxPerDay);
            this.groupBoxEnergyParameters.Controls.Add(this.labelTRxPerDay);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxBatterySizemWh);
            this.groupBoxEnergyParameters.Controls.Add(this.labelBatterySizemWh);
            this.groupBoxEnergyParameters.Controls.Add(this.textBoxTxCount);
            this.groupBoxEnergyParameters.Controls.Add(this.labelTxCount);
            this.groupBoxEnergyParameters.Location = new System.Drawing.Point(16, 135);
            this.groupBoxEnergyParameters.Name = "groupBoxEnergyParameters";
            this.groupBoxEnergyParameters.Size = new System.Drawing.Size(555, 206);
            this.groupBoxEnergyParameters.TabIndex = 46;
            this.groupBoxEnergyParameters.TabStop = false;
            this.groupBoxEnergyParameters.Text = "Energy Parameters (when changing one value press Enter in that field to save it)";
            // 
            // labelCsqAdvice
            // 
            this.labelCsqAdvice.AutoSize = true;
            this.labelCsqAdvice.Location = new System.Drawing.Point(460, 178);
            this.labelCsqAdvice.Name = "labelCsqAdvice";
            this.labelCsqAdvice.Size = new System.Drawing.Size(85, 13);
            this.labelCsqAdvice.TabIndex = 30;
            this.labelCsqAdvice.Text = "[\'0\' for simulated]";
            // 
            // buttonResetEnergyUsage
            // 
            this.buttonResetEnergyUsage.Location = new System.Drawing.Point(87, 149);
            this.buttonResetEnergyUsage.Name = "buttonResetEnergyUsage";
            this.buttonResetEnergyUsage.Size = new System.Drawing.Size(99, 23);
            this.buttonResetEnergyUsage.TabIndex = 11;
            this.buttonResetEnergyUsage.Text = "Reset To Zero";
            this.buttonResetEnergyUsage.UseVisualStyleBackColor = true;
            this.buttonResetEnergyUsage.Click += new System.EventHandler(this.buttonResetEnergyUsage_Click);
            // 
            // labelCsqValueAtRefdBm
            // 
            this.labelCsqValueAtRefdBm.AutoSize = true;
            this.labelCsqValueAtRefdBm.Location = new System.Drawing.Point(264, 178);
            this.labelCsqValueAtRefdBm.Name = "labelCsqValueAtRefdBm";
            this.labelCsqValueAtRefdBm.Size = new System.Drawing.Size(88, 13);
            this.labelCsqValueAtRefdBm.TabIndex = 28;
            this.labelCsqValueAtRefdBm.Text = "CSQ @ -80 dBm:";
            // 
            // textBoxBitsProtocolOverhead
            // 
            this.textBoxBitsProtocolOverhead.Location = new System.Drawing.Point(355, 149);
            this.textBoxBitsProtocolOverhead.Name = "textBoxBitsProtocolOverhead";
            this.textBoxBitsProtocolOverhead.Size = new System.Drawing.Size(99, 20);
            this.textBoxBitsProtocolOverhead.TabIndex = 27;
            this.textBoxBitsProtocolOverhead.WordWrap = false;
            this.textBoxBitsProtocolOverhead.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxBitsProtocolOverhead_KeyPress);
            // 
            // textBoxCsqValueAtRefdBm
            // 
            this.textBoxCsqValueAtRefdBm.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxCsqValueAtRefdBm.Location = new System.Drawing.Point(355, 175);
            this.textBoxCsqValueAtRefdBm.Name = "textBoxCsqValueAtRefdBm";
            this.textBoxCsqValueAtRefdBm.Size = new System.Drawing.Size(99, 20);
            this.textBoxCsqValueAtRefdBm.TabIndex = 29;
            this.textBoxCsqValueAtRefdBm.WordWrap = false;
            this.textBoxCsqValueAtRefdBm.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxCsqValueAtRefdBm_KeyPress);
            // 
            // labelBitsProtocolOverhead
            // 
            this.labelBitsProtocolOverhead.AutoSize = true;
            this.labelBitsProtocolOverhead.Location = new System.Drawing.Point(232, 151);
            this.labelBitsProtocolOverhead.Name = "labelBitsProtocolOverhead";
            this.labelBitsProtocolOverhead.Size = new System.Drawing.Size(119, 13);
            this.labelBitsProtocolOverhead.TabIndex = 26;
            this.labelBitsProtocolOverhead.Text = "Bits Protocol Overhead:";
            // 
            // textBoxBitsPerAck
            // 
            this.textBoxBitsPerAck.Location = new System.Drawing.Point(355, 123);
            this.textBoxBitsPerAck.Name = "textBoxBitsPerAck";
            this.textBoxBitsPerAck.Size = new System.Drawing.Size(99, 20);
            this.textBoxBitsPerAck.TabIndex = 25;
            this.textBoxBitsPerAck.WordWrap = false;
            this.textBoxBitsPerAck.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxBitsPerAck_KeyPress);
            // 
            // labelBitsPerAck
            // 
            this.labelBitsPerAck.AutoSize = true;
            this.labelBitsPerAck.Location = new System.Drawing.Point(284, 125);
            this.labelBitsPerAck.Name = "labelBitsPerAck";
            this.labelBitsPerAck.Size = new System.Drawing.Size(67, 13);
            this.labelBitsPerAck.TabIndex = 24;
            this.labelBitsPerAck.Text = "Bits per Ack:";
            // 
            // labelNorm3
            // 
            this.labelNorm3.AutoSize = true;
            this.labelNorm3.Location = new System.Drawing.Point(460, 99);
            this.labelNorm3.Name = "labelNorm3";
            this.labelNorm3.Size = new System.Drawing.Size(87, 13);
            this.labelNorm3.TabIndex = 23;
            this.labelNorm3.Text = "[in normal usage]";
            // 
            // textBoxBitsPerRx
            // 
            this.textBoxBitsPerRx.Location = new System.Drawing.Point(355, 97);
            this.textBoxBitsPerRx.Name = "textBoxBitsPerRx";
            this.textBoxBitsPerRx.Size = new System.Drawing.Size(99, 20);
            this.textBoxBitsPerRx.TabIndex = 22;
            this.textBoxBitsPerRx.WordWrap = false;
            this.textBoxBitsPerRx.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxBitsPerRx_KeyPress);
            // 
            // labelBitsPerRx
            // 
            this.labelBitsPerRx.AutoSize = true;
            this.labelBitsPerRx.Location = new System.Drawing.Point(290, 99);
            this.labelBitsPerRx.Name = "labelBitsPerRx";
            this.labelBitsPerRx.Size = new System.Drawing.Size(61, 13);
            this.labelBitsPerRx.TabIndex = 21;
            this.labelBitsPerRx.Text = "Bits per Rx:";
            // 
            // textBoxBitsRxCount
            // 
            this.textBoxBitsRxCount.Location = new System.Drawing.Point(87, 100);
            this.textBoxBitsRxCount.Name = "textBoxBitsRxCount";
            this.textBoxBitsRxCount.Size = new System.Drawing.Size(99, 20);
            this.textBoxBitsRxCount.TabIndex = 7;
            this.textBoxBitsRxCount.WordWrap = false;
            this.textBoxBitsRxCount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxBitsRxCount_KeyPress);
            // 
            // labelBitsRxCount
            // 
            this.labelBitsRxCount.AutoSize = true;
            this.labelBitsRxCount.Location = new System.Drawing.Point(6, 102);
            this.labelBitsRxCount.Name = "labelBitsRxCount";
            this.labelBitsRxCount.Size = new System.Drawing.Size(74, 13);
            this.labelBitsRxCount.TabIndex = 6;
            this.labelBitsRxCount.Text = "Bits Rx Count:";
            // 
            // textBoxBitsTxCount
            // 
            this.textBoxBitsTxCount.Location = new System.Drawing.Point(87, 48);
            this.textBoxBitsTxCount.Name = "textBoxBitsTxCount";
            this.textBoxBitsTxCount.Size = new System.Drawing.Size(99, 20);
            this.textBoxBitsTxCount.TabIndex = 3;
            this.textBoxBitsTxCount.WordWrap = false;
            this.textBoxBitsTxCount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxBitsTxCount_KeyPress);
            // 
            // labelBitsTxCount
            // 
            this.labelBitsTxCount.AutoSize = true;
            this.labelBitsTxCount.Location = new System.Drawing.Point(8, 50);
            this.labelBitsTxCount.Name = "labelBitsTxCount";
            this.labelBitsTxCount.Size = new System.Drawing.Size(73, 13);
            this.labelBitsTxCount.TabIndex = 2;
            this.labelBitsTxCount.Text = "Bits Tx Count:";
            // 
            // textBoxRxCount
            // 
            this.textBoxRxCount.Location = new System.Drawing.Point(87, 74);
            this.textBoxRxCount.Name = "textBoxRxCount";
            this.textBoxRxCount.Size = new System.Drawing.Size(99, 20);
            this.textBoxRxCount.TabIndex = 5;
            this.textBoxRxCount.WordWrap = false;
            this.textBoxRxCount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxRxCount_KeyPress);
            // 
            // labelRxCount
            // 
            this.labelRxCount.AutoSize = true;
            this.labelRxCount.Location = new System.Drawing.Point(27, 76);
            this.labelRxCount.Name = "labelRxCount";
            this.labelRxCount.Size = new System.Drawing.Size(54, 13);
            this.labelRxCount.TabIndex = 4;
            this.labelRxCount.Text = "Rx Count:";
            // 
            // groupBoxConnectionParameters
            // 
            this.groupBoxConnectionParameters.Controls.Add(this.buttonExitAll);
            this.groupBoxConnectionParameters.Controls.Add(this.labelHost);
            this.groupBoxConnectionParameters.Controls.Add(this.textBoxHostname);
            this.groupBoxConnectionParameters.Controls.Add(this.labelUuid);
            this.groupBoxConnectionParameters.Controls.Add(this.textBoxUuid);
            this.groupBoxConnectionParameters.Controls.Add(this.buttonDisconnect);
            this.groupBoxConnectionParameters.Controls.Add(this.buttonConnect);
            this.groupBoxConnectionParameters.Controls.Add(this.textBoxPassword);
            this.groupBoxConnectionParameters.Controls.Add(this.labelPassword);
            this.groupBoxConnectionParameters.Controls.Add(this.textBoxUsername);
            this.groupBoxConnectionParameters.Controls.Add(this.labelUsername);
            this.groupBoxConnectionParameters.Location = new System.Drawing.Point(18, 38);
            this.groupBoxConnectionParameters.Name = "groupBoxConnectionParameters";
            this.groupBoxConnectionParameters.Size = new System.Drawing.Size(552, 92);
            this.groupBoxConnectionParameters.TabIndex = 47;
            this.groupBoxConnectionParameters.TabStop = false;
            this.groupBoxConnectionParameters.Text = "Connection Parameters";
            // 
            // buttonExitAll
            // 
            this.buttonExitAll.Location = new System.Drawing.Point(473, 15);
            this.buttonExitAll.Name = "buttonExitAll";
            this.buttonExitAll.Size = new System.Drawing.Size(69, 20);
            this.buttonExitAll.TabIndex = 10;
            this.buttonExitAll.Text = "Exit All";
            this.buttonExitAll.UseVisualStyleBackColor = true;
            this.buttonExitAll.Click += new System.EventHandler(this.buttonExitAll_Click);
            // 
            // groupBoxDevice
            // 
            this.groupBoxDevice.Controls.Add(this.buttonSetHeartbeat);
            this.groupBoxDevice.Controls.Add(this.labelHeartbeatSeconds);
            this.groupBoxDevice.Controls.Add(this.buttonSetReportingInterval);
            this.groupBoxDevice.Controls.Add(this.labelRssi);
            this.groupBoxDevice.Controls.Add(this.textBoxRssi);
            this.groupBoxDevice.Controls.Add(this.labelReportingIntervalMinutes);
            this.groupBoxDevice.Controls.Add(this.labelReportingInterval);
            this.groupBoxDevice.Controls.Add(this.textBoxReportingIntervalMinutes);
            this.groupBoxDevice.Controls.Add(this.labelHeartbeat);
            this.groupBoxDevice.Controls.Add(this.textBoxHeartbeatSeconds);
            this.groupBoxDevice.Controls.Add(this.buttonReboot);
            this.groupBoxDevice.Controls.Add(this.labelDebug);
            this.groupBoxDevice.Controls.Add(this.textBoxDebug);
            this.groupBoxDevice.Location = new System.Drawing.Point(16, 340);
            this.groupBoxDevice.Name = "groupBoxDevice";
            this.groupBoxDevice.Size = new System.Drawing.Size(555, 315);
            this.groupBoxDevice.TabIndex = 48;
            this.groupBoxDevice.TabStop = false;
            this.groupBoxDevice.Text = "Device";
            // 
            // buttonSetHeartbeat
            // 
            this.buttonSetHeartbeat.Location = new System.Drawing.Point(241, 43);
            this.buttonSetHeartbeat.Name = "buttonSetHeartbeat";
            this.buttonSetHeartbeat.Size = new System.Drawing.Size(60, 20);
            this.buttonSetHeartbeat.TabIndex = 21;
            this.buttonSetHeartbeat.Text = "Set";
            this.buttonSetHeartbeat.UseVisualStyleBackColor = true;
            this.buttonSetHeartbeat.Click += new System.EventHandler(this.buttonSetHeartbeat_Click);
            // 
            // labelHeartbeatSeconds
            // 
            this.labelHeartbeatSeconds.AutoSize = true;
            this.labelHeartbeatSeconds.Location = new System.Drawing.Point(191, 46);
            this.labelHeartbeatSeconds.Name = "labelHeartbeatSeconds";
            this.labelHeartbeatSeconds.Size = new System.Drawing.Size(47, 13);
            this.labelHeartbeatSeconds.TabIndex = 20;
            this.labelHeartbeatSeconds.Text = "seconds";
            // 
            // labelOpenHelp
            // 
            this.labelOpenHelp.AutoSize = true;
            this.labelOpenHelp.Location = new System.Drawing.Point(78, 9);
            this.labelOpenHelp.Name = "labelOpenHelp";
            this.labelOpenHelp.Size = new System.Drawing.Size(456, 13);
            this.labelOpenHelp.TabIndex = 49;
            this.labelOpenHelp.Text = "[Click in the vicinity of the top left of the main screen to get back here after " +
    "this form has closed]";
            // 
            // CommsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 658);
            this.Controls.Add(this.labelOpenHelp);
            this.Controls.Add(this.groupBoxDevice);
            this.Controls.Add(this.groupBoxConnectionParameters);
            this.Controls.Add(this.groupBoxEnergyParameters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CommsForm";
            this.Text = "CommsForm";
            this.TopMost = true;
            this.groupBoxEnergyParameters.ResumeLayout(false);
            this.groupBoxEnergyParameters.PerformLayout();
            this.groupBoxConnectionParameters.ResumeLayout(false);
            this.groupBoxConnectionParameters.PerformLayout();
            this.groupBoxDevice.ResumeLayout(false);
            this.groupBoxDevice.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.TextBox textBoxDebug;
        private System.Windows.Forms.Label labelDebug;
        private System.Windows.Forms.Button buttonReboot;
        private System.Windows.Forms.TextBox textBoxHeartbeatSeconds;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.Label labelHeartbeat;
        private System.Windows.Forms.TextBox textBoxUuid;
        private System.Windows.Forms.Label labelUuid;
        private System.Windows.Forms.Label labelReportingInterval;
        private System.Windows.Forms.TextBox textBoxReportingIntervalMinutes;
        private System.Windows.Forms.Label labelReportingIntervalMinutes;
        private System.Windows.Forms.Label labelRssi;
        private System.Windows.Forms.TextBox textBoxRssi;
        private System.Windows.Forms.Button buttonSetReportingInterval;
        private System.Windows.Forms.Label labelHost;
        private System.Windows.Forms.TextBox textBoxHostname;
        private System.Windows.Forms.Label labelTimePassedSeconds;
        private System.Windows.Forms.Label labelTxCount;
        private System.Windows.Forms.TextBox textBoxTxCount;
        private System.Windows.Forms.Label labelBatterySizemWh;
        private System.Windows.Forms.TextBox textBoxBatterySizemWh;
        private System.Windows.Forms.Label labelTRxPerDay;
        private System.Windows.Forms.TextBox textBoxTRxPerDay;
        private System.Windows.Forms.Label labelBitsPerTx;
        private System.Windows.Forms.TextBox textBoxBitsPerTx;
        private System.Windows.Forms.TextBox textBoxTimePassedSeconds;
        private System.Windows.Forms.Label labelSeconds;
        private System.Windows.Forms.Label labelmWh;
        private System.Windows.Forms.Label labelNorm1;
        private System.Windows.Forms.Label labelNorm2;
        private System.Windows.Forms.GroupBox groupBoxEnergyParameters;
        private System.Windows.Forms.GroupBox groupBoxConnectionParameters;
        private System.Windows.Forms.GroupBox groupBoxDevice;
        private System.Windows.Forms.TextBox textBoxRxCount;
        private System.Windows.Forms.Label labelRxCount;
        private System.Windows.Forms.TextBox textBoxBitsTxCount;
        private System.Windows.Forms.Label labelBitsTxCount;
        private System.Windows.Forms.TextBox textBoxBitsRxCount;
        private System.Windows.Forms.Label labelBitsRxCount;
        private System.Windows.Forms.Label labelNorm3;
        private System.Windows.Forms.TextBox textBoxBitsPerRx;
        private System.Windows.Forms.Label labelBitsPerRx;
        private System.Windows.Forms.TextBox textBoxBitsPerAck;
        private System.Windows.Forms.Label labelBitsPerAck;
        private System.Windows.Forms.TextBox textBoxBitsProtocolOverhead;
        private System.Windows.Forms.Label labelBitsProtocolOverhead;
        private System.Windows.Forms.Button buttonResetEnergyUsage;
        private System.Windows.Forms.Button buttonExitAll;
        private System.Windows.Forms.Label labelOpenHelp;
        private System.Windows.Forms.Label labelCsqValueAtRefdBm;
        private System.Windows.Forms.TextBox textBoxCsqValueAtRefdBm;
        private System.Windows.Forms.Label labelCsqAdvice;
        private System.Windows.Forms.Label labelHeartbeatSeconds;
        private System.Windows.Forms.Button buttonSetHeartbeat;
    }
}

