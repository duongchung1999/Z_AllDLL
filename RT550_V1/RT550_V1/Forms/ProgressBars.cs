using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RT550_V1.Froms
{
    public partial class ProgressBars : Form
    {
        public string lable = "-1:线程正在使用RT550";
        public static bool showDialogFlag = false;
        public ProgressBars()
        {
            InitializeComponent();
            showDialogFlag = true;
            Thread.Sleep(100);
            Control.CheckForIllegalCrossThreadCalls = false;

        }
        private void ProgressBars_Load(object sender, EventArgs e)
        {
            i = 0;
            timer1.Enabled = true;
            label1.Text = lable;
            timeI = 0;
        }
        int i;
        double timeI;
        private void timer1_Tick(object sender, EventArgs e)
        {
            i += 1;
            progressBar1.Value = i;
            if (i >= 199) i = 0;
            timeI += 0.2;
            lb_Time.Text = $"{timeI}";
            if (!showDialogFlag)
                this.Close();

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
