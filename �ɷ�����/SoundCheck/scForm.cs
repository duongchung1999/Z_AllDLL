using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SoundCheck
{
    public partial class scForm : Form
    {
        double sleep;
        double i;
        public scForm(double sleep)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.sleep = sleep;
            progressBar1.Maximum = (int)sleep + 1;
            i = sleep / 1.2;
            la_time.Text = $"{sleep / 1000}";
            NO.Enabled = false;
            YES.Enabled = false;
            this.TopMost = true;
        }
        bool bettonFlag = true;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow([In] IntPtr hWnd);
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (bettonFlag)
            {
                if (progressBar1.Value >= i)
                {
                    SetForegroundWindow(Handle);
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
            if ((progressBar1.Value % 3000) == 0)
                SetForegroundWindow(Handle);
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
    }
}
