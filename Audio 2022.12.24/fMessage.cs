using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

namespace Audio
{
    public partial class fMessage : Form
    {
        private string Message1 { get; set; }
        private int Interval { get; set; }
        private int TimeDisplay { get; set; }
        public fMessage(string mesage, int interval)
        {
            InitializeComponent();
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    if (process.MainModule.FileName == current.MainModule.FileName)
                    {
                        System.Environment.Exit(0);
                        return;
                    }
                }
            }
            Message1 = mesage;
            Interval = interval;
            //TimeDisplay = timeDisplay;
        }

        private void fMessage_Load(object sender, EventArgs e)
        {
            //button1.Focus();
            //button2.Focus();
            this.KeyPreview = true;
            lblMes.Text = Message1;
            timerMs.Interval = Interval;
            timerMs.Enabled = true;
            //timerCloseMsg.Interval = TimeDisplay;
            this.btnMsYES.Enabled = false;
            this.btnMsNO.Enabled = false;
           
            //timerCloseMsg.Enabled = true;
        }

        private void timerMs_Tick(object sender, EventArgs e)
        {
            btnMsNO.Enabled = true;
            btnMsYES.Enabled = true;
            btnMsYES.Focus();
            timerMs.Enabled = false;
        }

        private void btnMsYES_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
        }

        private void btnMsNO_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
        }

        private void fMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnMsYES_Click(null, null);
            }
            else if (e.KeyCode == Keys.N)
            {
                btnMsNO_Click(null, null);
            }
        }
    }
}
