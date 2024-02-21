namespace Airoha_ANC.Forms
{
    partial class BT2200Control
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.bt_Reset = new System.Windows.Forms.Button();
            this.bt_A2DP = new System.Windows.Forms.Button();
            this.bt_Firmware_Version = new System.Windows.Forms.Button();
            this.bt_AutoMaticPair = new System.Windows.Forms.Button();
            this.bt_Disconnect = new System.Windows.Forms.Button();
            this.bt_HFP = new System.Windows.Forms.Button();
            this.bt_SPPDisconnect = new System.Windows.Forms.Button();
            this.bt_OpenPort = new System.Windows.Forms.Button();
            this.tb_log = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.tb_CMD = new System.Windows.Forms.TextBox();
            this.bt_SendCMD = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.cb_HEXButton = new System.Windows.Forms.CheckBox();
            this.tb_BDAddress = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // bt_Reset
            // 
            this.bt_Reset.BackColor = System.Drawing.Color.LightCyan;
            this.bt_Reset.Location = new System.Drawing.Point(11, 11);
            this.bt_Reset.Name = "bt_Reset";
            this.bt_Reset.Size = new System.Drawing.Size(95, 40);
            this.bt_Reset.TabIndex = 0;
            this.bt_Reset.Text = "重置";
            this.bt_Reset.UseVisualStyleBackColor = false;
            this.bt_Reset.Click += new System.EventHandler(this.bt_Reset_Click);
            // 
            // bt_A2DP
            // 
            this.bt_A2DP.BackColor = System.Drawing.Color.LightCyan;
            this.bt_A2DP.Location = new System.Drawing.Point(118, 59);
            this.bt_A2DP.Name = "bt_A2DP";
            this.bt_A2DP.Size = new System.Drawing.Size(95, 40);
            this.bt_A2DP.TabIndex = 0;
            this.bt_A2DP.Text = "A2DP";
            this.bt_A2DP.UseVisualStyleBackColor = false;
            this.bt_A2DP.Click += new System.EventHandler(this.bt_A2DP_Click);
            // 
            // bt_Firmware_Version
            // 
            this.bt_Firmware_Version.BackColor = System.Drawing.Color.LightCyan;
            this.bt_Firmware_Version.Location = new System.Drawing.Point(11, 59);
            this.bt_Firmware_Version.Name = "bt_Firmware_Version";
            this.bt_Firmware_Version.Size = new System.Drawing.Size(95, 40);
            this.bt_Firmware_Version.TabIndex = 0;
            this.bt_Firmware_Version.Text = "固件版本";
            this.bt_Firmware_Version.UseVisualStyleBackColor = false;
            this.bt_Firmware_Version.Click += new System.EventHandler(this.bt_Firmware_Version_Click);
            // 
            // bt_AutoMaticPair
            // 
            this.bt_AutoMaticPair.BackColor = System.Drawing.Color.LawnGreen;
            this.bt_AutoMaticPair.Location = new System.Drawing.Point(238, 11);
            this.bt_AutoMaticPair.Name = "bt_AutoMaticPair";
            this.bt_AutoMaticPair.Size = new System.Drawing.Size(102, 40);
            this.bt_AutoMaticPair.TabIndex = 0;
            this.bt_AutoMaticPair.Text = "自动配对";
            this.bt_AutoMaticPair.UseVisualStyleBackColor = false;
            this.bt_AutoMaticPair.Click += new System.EventHandler(this.bt_AutoMaticPair_Click);
            // 
            // bt_Disconnect
            // 
            this.bt_Disconnect.BackColor = System.Drawing.Color.LightCyan;
            this.bt_Disconnect.Location = new System.Drawing.Point(238, 59);
            this.bt_Disconnect.Name = "bt_Disconnect";
            this.bt_Disconnect.Size = new System.Drawing.Size(105, 40);
            this.bt_Disconnect.TabIndex = 0;
            this.bt_Disconnect.Text = "断开连接";
            this.bt_Disconnect.UseVisualStyleBackColor = false;
            this.bt_Disconnect.Click += new System.EventHandler(this.bt_Disconnect_Click);
            // 
            // bt_HFP
            // 
            this.bt_HFP.BackColor = System.Drawing.Color.LightCyan;
            this.bt_HFP.Location = new System.Drawing.Point(118, 11);
            this.bt_HFP.Name = "bt_HFP";
            this.bt_HFP.Size = new System.Drawing.Size(95, 40);
            this.bt_HFP.TabIndex = 0;
            this.bt_HFP.Text = "HFP";
            this.bt_HFP.UseVisualStyleBackColor = false;
            this.bt_HFP.Click += new System.EventHandler(this.bt_HFP_Click);
            // 
            // bt_SPPDisconnect
            // 
            this.bt_SPPDisconnect.BackColor = System.Drawing.Color.PeachPuff;
            this.bt_SPPDisconnect.Location = new System.Drawing.Point(238, 105);
            this.bt_SPPDisconnect.Name = "bt_SPPDisconnect";
            this.bt_SPPDisconnect.Size = new System.Drawing.Size(105, 35);
            this.bt_SPPDisconnect.TabIndex = 0;
            this.bt_SPPDisconnect.Text = "SPP断开连接";
            this.bt_SPPDisconnect.UseVisualStyleBackColor = false;
            this.bt_SPPDisconnect.Click += new System.EventHandler(this.bt_SPPDisconnect_Click);
            // 
            // bt_OpenPort
            // 
            this.bt_OpenPort.BackColor = System.Drawing.Color.Lime;
            this.bt_OpenPort.Location = new System.Drawing.Point(118, 105);
            this.bt_OpenPort.Name = "bt_OpenPort";
            this.bt_OpenPort.Size = new System.Drawing.Size(95, 35);
            this.bt_OpenPort.TabIndex = 12;
            this.bt_OpenPort.Text = "Open";
            this.bt_OpenPort.UseVisualStyleBackColor = false;
            this.bt_OpenPort.Click += new System.EventHandler(this.bt_OpenPort_Click);
            // 
            // tb_log
            // 
            this.tb_log.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.tb_log.Font = new System.Drawing.Font("黑体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb_log.ForeColor = System.Drawing.SystemColors.Window;
            this.tb_log.Location = new System.Drawing.Point(4, 269);
            this.tb_log.Multiline = true;
            this.tb_log.Name = "tb_log";
            this.tb_log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb_log.Size = new System.Drawing.Size(718, 143);
            this.tb_log.TabIndex = 13;
            this.tb_log.Text = " ";
            this.tb_log.TextChanged += new System.EventHandler(this.tb_log_TextChanged);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label20.Location = new System.Drawing.Point(346, 20);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(84, 20);
            this.label20.TabIndex = 17;
            this.label20.Text = "Address：";
            // 
            // tb_CMD
            // 
            this.tb_CMD.BackColor = System.Drawing.Color.Linen;
            this.tb_CMD.Location = new System.Drawing.Point(4, 184);
            this.tb_CMD.Multiline = true;
            this.tb_CMD.Name = "tb_CMD";
            this.tb_CMD.Size = new System.Drawing.Size(718, 79);
            this.tb_CMD.TabIndex = 20;
            // 
            // bt_SendCMD
            // 
            this.bt_SendCMD.BackColor = System.Drawing.Color.LightCyan;
            this.bt_SendCMD.Location = new System.Drawing.Point(11, 141);
            this.bt_SendCMD.Name = "bt_SendCMD";
            this.bt_SendCMD.Size = new System.Drawing.Size(95, 40);
            this.bt_SendCMD.TabIndex = 0;
            this.bt_SendCMD.Text = "发送指令";
            this.bt_SendCMD.UseVisualStyleBackColor = false;
            this.bt_SendCMD.Click += new System.EventHandler(this.bt_SendCMD_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // cb_HEXButton
            // 
            this.cb_HEXButton.AutoSize = true;
            this.cb_HEXButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cb_HEXButton.Location = new System.Drawing.Point(118, 155);
            this.cb_HEXButton.Name = "cb_HEXButton";
            this.cb_HEXButton.Size = new System.Drawing.Size(63, 23);
            this.cb_HEXButton.TabIndex = 21;
            this.cb_HEXButton.Text = "HEX";
            this.cb_HEXButton.UseVisualStyleBackColor = true;
            // 
            // tb_BDAddress
            // 
            this.tb_BDAddress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.tb_BDAddress.Location = new System.Drawing.Point(424, 20);
            this.tb_BDAddress.Name = "tb_BDAddress";
            this.tb_BDAddress.Size = new System.Drawing.Size(180, 25);
            this.tb_BDAddress.TabIndex = 22;
            // 
            // BT2200Control
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.tb_BDAddress);
            this.Controls.Add(this.cb_HEXButton);
            this.Controls.Add(this.tb_CMD);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.tb_log);
            this.Controls.Add(this.bt_OpenPort);
            this.Controls.Add(this.bt_SendCMD);
            this.Controls.Add(this.bt_SPPDisconnect);
            this.Controls.Add(this.bt_HFP);
            this.Controls.Add(this.bt_Firmware_Version);
            this.Controls.Add(this.bt_Disconnect);
            this.Controls.Add(this.bt_A2DP);
            this.Controls.Add(this.bt_AutoMaticPair);
            this.Controls.Add(this.bt_Reset);
            this.Name = "BT2200Control";
            this.Size = new System.Drawing.Size(725, 418);
            this.Load += new System.EventHandler(this.BT2200Control_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button bt_Reset;
        private System.Windows.Forms.Button bt_A2DP;
        private System.Windows.Forms.Button bt_Firmware_Version;
        private System.Windows.Forms.Button bt_AutoMaticPair;
        private System.Windows.Forms.Button bt_Disconnect;
        private System.Windows.Forms.Button bt_HFP;
        private System.Windows.Forms.Button bt_SPPDisconnect;
        private System.Windows.Forms.Button bt_OpenPort;
        private System.Windows.Forms.TextBox tb_log;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox tb_CMD;
        private System.Windows.Forms.Button bt_SendCMD;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox cb_HEXButton;
        private System.Windows.Forms.TextBox tb_BDAddress;
    }
}
