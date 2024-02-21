using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryTest.testitem
{
    public partial class ProgressBars : Form
    {

        public static bool CountDown(Func<bool> func, string MSG, string title = "标题", int TimeOut_S = 20)
        {
            var flag = true;
            ProgressBars box = new ProgressBars();
            box.Text = title;
            box.label1.Text = MSG;
            box.TopMost = true;
            box.progressBar1.Maximum = TimeOut_S * 1000;
            box.timer1.Interval = 500;

            Task.Run(() =>
             {
                 Thread.Sleep(50);
                 while (flag)
                 {
                     try
                     {
                         bool Result = func.Invoke();
                         if (box.IsDisposed || box.DialogResult == DialogResult.Cancel || box.DialogResult == DialogResult.No)
                             return;

                         if (Result)
                         {
                             box.DialogResult = DialogResult.Yes;
                         }
                         Thread.Sleep(150);
                     }
                     catch
                     {
                         return;
                     }

                 }
             });
            box.ShowDialog();
            bool result = box.DialogResult == DialogResult.Yes;
            box.Dispose();
            flag = false;
            return result;
        }
        #region 窗体部分

        public ProgressBars(string title = "标题")
        {
            InitializeComponent();
            Text = title;

        }
        private void ProgressBars_Load(object sender, EventArgs e)
        {
            Span = timer1.Interval;
            Max = progressBar1.Maximum - Span;
            timer1.Enabled = true;
        }
        int Span = 0;
        int Max = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {

            progressBar1.Value += Span;
            //如果執行時間超過，則this.DialogResult = DialogResult.No;
            if (progressBar1.Value >= Max)
            {
                DialogResult = DialogResult.No;
                timer1.Stop();
                timer1.Enabled = false;
                this.Close();
            }
        }



        /// <summary>
        /// 控制窗体
        /// </summary>
        /// <param name="lpClassName">传null</param>
        /// <param name="lpWindowName">需要控制窗体得名字</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        /// <summary>
        /// 结束窗体
        /// </summary>
        /// <param name="hWnd">信息发往的窗口的句柄</param>
        /// <param name="Msg">消息ID ：0x10</param>
        /// <param name="wParam">参数1 ：</param>
        /// <param name="lParam">参数2 ：0</param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        extern static int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        #endregion

    }
}
