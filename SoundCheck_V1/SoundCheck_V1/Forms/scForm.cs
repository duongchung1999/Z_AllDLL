using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    public partial class scForm : Form
    {
        double sleep;
        double EnabledButtonDelay;
        public scForm(double sleep)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.sleep = sleep;
            progressBar1.Maximum = (int)sleep + 1;
            EnabledButtonDelay = 10*1000;
            la_time.Text = $"{sleep / 1000}";
            NO.Enabled = false;
            YES.Enabled = false;
        }
        bool bettonFlag = true;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow([In] IntPtr hWnd);

        private void scForm_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(1000);
                    SetForegroundWindow(Handle);
                }
            });
            TopMost = true;

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (bettonFlag)
            {
                if (progressBar1.Value >= EnabledButtonDelay)
                {
                    NO.Enabled = true;
                    YES.Enabled = true;
                    bettonFlag = false;
                    YES.Focus();
                }
            }

            if (progressBar1.Value >= sleep)
            {
                YES_Click(sender, e);
                return;
            }
            progressBar1.Value += 500;
            la_time.Text = (double.Parse(la_time.Text) - 0.5).ToString();
        }
        private void NO_Click(object sender, EventArgs e)
        {
            this.timer1.Enabled = false;
            DialogResult = DialogResult.No;
            this.Close();
        }

        private void YES_Click(object sender, EventArgs e)
        {

            this.timer1.Enabled = false;
            DialogResult = DialogResult.Yes;
            this.Close();

        }
        private void scForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.timer1.Enabled = false;
        }
    }
}
