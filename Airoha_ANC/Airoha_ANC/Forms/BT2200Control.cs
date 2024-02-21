using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Airoha_ANC.Forms
{

    /// <summary ClassType="UserControl">
    /// BT2200Control
    /// </summary>
    public partial class BT2200Control : UserControl
    {
        public BT2200Control()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void BT2200Control_Load(object sender, EventArgs e)
        {
        }
        MerryDllFramework.MerryDll dll = new MerryDllFramework.MerryDll();
        private void bt_OpenPort_Click(object sender, EventArgs e)
        {
            if (MerryDllFramework.MerryDll.PORT == null || !MerryDllFramework.MerryDll.PORT.IsOpen)
            {
                bt_OpenPort.Text = "Close";
                this.bt_OpenPort.BackColor = System.Drawing.Color.Coral;
                dll.BT2200_OpenPort();
            }
            else
            {
                this.bt_OpenPort.BackColor = System.Drawing.Color.Lime;

                bt_OpenPort.Text = "Open";
                dll.BT2200_ColsePort();
            }
        }

        private void bt_AutoMaticPair_Click(object sender, EventArgs e)
        {
            bt_AutoMaticPair.Enabled = false;
            Task.Run(() =>
            {
                try
                {
                    MerryDllFramework.MerryDll.PortLog.Add("正在进行配对");

                    tb_BDAddress.Text = dll.SPP_Connect("False", 10);
                }
                finally
                {
                    bt_AutoMaticPair.Enabled = true;

                }

            });

        }



        private void bt_Reset_Click(object sender, EventArgs e)
        {
            dll.WriteLog(">RST");
            dll.ReadLog(out _, out _);
        }

        private void bt_Firmware_Version_Click(object sender, EventArgs e)
        {
            dll.WriteLog(">SYS_INFO_GET=?");
            dll.ReadLog(out _, out _);
        }


        private void bt_SendCMD_Click(object sender, EventArgs e)
        {
            try
            {
                if (cb_HEXButton.Checked)
                {
                    string str = tb_CMD.Text.Replace("0X", "").Replace("0x", "");
                    List<Byte> bytes = new List<Byte>();

                    foreach (var item in str.Split(' '))
                    {
                        if (item.Trim().Length <= 0)
                            continue;

                        bytes.Add(Convert.ToByte(item.Trim(), 16));
                    }
                    dll.WriteLog(bytes.ToArray());

                }
                else
                {
                    dll.WriteLog(">SYS_INFO_GET=?");
                }

                dll.ReadLog(out _, out _);
            }
            catch (Exception ex)
            {
                tb_log.Text = ex.ToString();
            }
        }
        private void bt_A2DP_Click(object sender, EventArgs e)
        {
            dll.WriteLog(">OPEN A2DP");
            dll.ReadLog(out _, out _);
        }

        private void bt_HFP_Click(object sender, EventArgs e)
        {
            dll.WriteLog(">OPEN HFP");
            dll.ReadLog(out _, out _);
        }

        private void bt_SPPDisconnect_Click(object sender, EventArgs e)
        {
            dll.WriteLog(">SPP_DISC");
            dll.ReadLog(out _, out _);
        }

        private void bt_Disconnect_Click(object sender, EventArgs e)
        {
            MerryDllFramework.MerryDll.PortLog.Add("正在断开连接");

            MerryDllFramework.MerryDll.PortLog.Add(dll.BT2200_Disconnect());
            tb_BDAddress.Text = "";
        }

        private void tb_log_TextChanged(object sender, EventArgs e)
        {
            this.tb_log.SelectionStart = this.tb_log.Text.Length;
            this.tb_log.ScrollToCaret();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            string[] str = MerryDllFramework.MerryDll.PortLog.ToArray();
            MerryDllFramework.MerryDll.PortLog.Clear();
            if (str.Length > 0)
            {
                StringBuilder @string = new StringBuilder(tb_log.Text);
                @string.AppendLine("");
                foreach (var item in str)
                    @string.AppendLine(item);
                tb_log.Text = @string.ToString();
                if (MerryDllFramework.MerryDll.PORT == null || !MerryDllFramework.MerryDll.PORT.IsOpen)
                {
                    bt_OpenPort.Text = "Close";
                    this.bt_OpenPort.BackColor = System.Drawing.Color.Coral;
                }
                else
                {
                    this.bt_OpenPort.BackColor = System.Drawing.Color.Lime;
                    bt_OpenPort.Text = "Open";
                }

            }

        }
    }
}
