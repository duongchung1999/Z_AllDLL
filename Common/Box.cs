using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common
{
    public partial class Box : Form
    {
        /// <summary>
        /// 将该指针的窗口设为活动窗口
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);//设置此窗体为活动窗体

        public Box(string msg)
        {
            InitializeComponent();
            label1.Text = msg;
        }

        private async void Box_Load(object sender, EventArgs e)
        {
            await Task.Run(() => Thread.Sleep(200));
            SetForegroundWindow(Handle);
            this.TopMost = true;
            YES.Focus();
        }

        private void Box_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.N) NO_Click(sender, e);
            if (e.KeyCode == Keys.Y) YES_Click(sender, e);

        }


        private void YES_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();

        }

        private void NO_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }
    }
}
