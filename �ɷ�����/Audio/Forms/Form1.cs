using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Audio.Forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            Thread.Sleep(50);
            SetForegroundWindow(Handle);
            this.TopMost = true;

        }
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);//设置此窗体为活动窗体
        private void Yes_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void NO_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Y)
            {
                Yes_Click(null, null);
            }
            else if (e.KeyCode == Keys.N)
            {
                NO_Click(null, null);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
