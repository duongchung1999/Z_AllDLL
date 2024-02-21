using MerryDllFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace MerryDllFramework
{
    public partial class CurvesForms : Form
    {
        public UIAdaptiveSize uias;

        public CurvesForms(int time, string _TextName)
        {
            InitializeComponent();


            if (uias == null)
            {
                uias = new UIAdaptiveSize
                {
                    Width = Width,
                    Height = Height,
                    FormsName = this.Text,
                    X = Width,
                    Y = Height,

                };
                uias.SetInitSize(this);
            }
            Balance.Series.Clear();
            Balance.ChartAreas.Clear();
            L_R.Series.Clear();
            L_R.ChartAreas.Clear();
            this.CloseTime = time;
            TextName = this.Text = _TextName;
            btn_Close.Text = $"关闭 ：{time}";
            this.Text = $"{TextName}  “{time}”";
        }



        string TextName = "";
        bool isClose = false;
        bool RefreshingFlag = true;
        bool isAddDiffCurves = false;
        List<_CurvesData> curvesDiffDatas = new List<_CurvesData>();

        bool isAddCurves_ = false;
        List<_CurvesData> curvesDatas_ = new List<_CurvesData>();


        bool isLog = false;
        List<string> Log = new List<string>();


        public void AddBalanceCurves(_CurvesData data)
        {
            lock (curvesDiffDatas)
            {
                curvesDiffDatas.Add(data);
                isAddDiffCurves = true;
            }
        }
        public void AddCurves_(_CurvesData data)
        {
            lock (curvesDatas_)
            {
                curvesDatas_.Add(data);
                isAddCurves_ = true;
            }
        }
        public void AddLog(string str)
        {
            lock (Log)
            {
                Log.Add(str);
                isLog = true;
            }
        }



        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);//设置此窗体为活动窗体
        private async void CurvesForms_Load(object sender, EventArgs e)
        {

            await Task.Run(() => Thread.Sleep(50));
            WindowState = FormWindowState.Normal;
            int x = System.Windows.Forms.SystemInformation.WorkingArea.Width - this.Size.Width;
            int y = System.Windows.Forms.SystemInformation.WorkingArea.Height - this.Size.Height;
            this.StartPosition = FormStartPosition.Manual; //窗体的位置由Location属性决定
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Location = (Point)new Size(x, y);         //窗体的起始位置为(x,y)
            CurvesTimer__Tick(null, null);
            CurvesDiffTimer_Tick(null, null);
            SetForegroundWindow(Handle);
            this.TopMost = true;
            RefreshingFlag = false;
            timer1.Enabled = true;
            CurvesTimer_.Enabled = true;
            CurvesDiffTimer.Enabled = true;

            ChartArea Low = new ChartArea("Low");
            Low.AxisX.Minimum = 10;
            Low.AxisX.Maximum = 1000;
            Low.AxisX.IsLogarithmic = true;
            Low.AxisX.LogarithmBase = 10;
            Balance.ChartAreas.Add(Low);

            ChartArea DiffX = new ChartArea("Balance");
            DiffX.AxisX.Minimum = 10;
            DiffX.AxisX.Maximum = 1000;
            DiffX.AxisX.IsLogarithmic = false;
            DiffX.AxisX.LogarithmBase = 10;
            L_R.ChartAreas.Add(DiffX);


        }

        private void CurvesForms_Resize(object sender, EventArgs e)
        {

            if (RefreshingFlag) return;

            var newX = Width;
            var newY = Height;
            uias.UpdateSize(Width, Height, this);
            uias.X = newX;
            uias.Y = newY;
        }
        int CloseTime = 5;
        private void timer1_Tick(object sender, EventArgs e)
        {

            if (CloseTime <= 0)
                CloseThis();
            if (isClose)
                CloseThis();

            CloseTime--;
            this.Text = $"{TextName}  “{CloseTime}”";
            btn_Close.Text = $"关闭 ：{CloseTime}";
        }

        private void CurvesDiffTimer_Tick(object sender, EventArgs e)
        {

            lock (curvesDiffDatas)
            {
                if (!isAddDiffCurves)
                    return;
                isAddDiffCurves = false;
                foreach (var data in curvesDiffDatas)
                {
                    bool SeriesExist = false;
                    for (int i = 0; i < Balance.Series.Count; i++)
                    {
                        if (Balance.Series[i].Name.Contains(data.CurveName))
                            SeriesExist = true;
                    }
                    if (SeriesExist)
                        continue;
                    Series Series1 = new Series(data.CurveName)
                    {
                        ChartType = SeriesChartType.Line,
                        Color = data._Color
                    };
                    for (int i = 0; i < data.Xdata.Length; i++)
                        Series1.Points.AddXY(data.Xdata[i], data.Ydata[i]);
                    Balance.Series.Add(Series1);

                }
            }
            lock (Log)
            {
                if (isLog)
                {
                    foreach (var item in Log)
                        textBox1.Text += $"{item}\r\n";
                    Log.Clear();
                }
            }
        }


        private void CurvesTimer__Tick(object sender, EventArgs e)
        {

            lock (curvesDatas_)
            {
                if (!isAddCurves_)
                    return;
                isAddCurves_ = false;
                foreach (var data in curvesDatas_)
                {
                    bool SeriesExist = false;
                    for (int i = 0; i < L_R.Series.Count; i++)
                    {
                        if (L_R.Series[i].Name.Contains(data.CurveName))
                            SeriesExist = true;
                    }
                    if (SeriesExist)
                        continue;
                    Series Series1 = new Series(data.CurveName)
                    {
                        ChartType = SeriesChartType.Line,
                        Color = data._Color
                    };
                    for (int i = 0; i < data.Xdata.Length; i++)
                        Series1.Points.AddXY(data.Xdata[i], data.Ydata[i]);

                    L_R.Series.Add(Series1);
                }
            }



        }

        private void button1_Click(object sender, EventArgs e)
        {
            CloseTime += 600;
            Application.DoEvents();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            CloseThis();

        }
        void CloseThis()
        {
            this.Close();
        }
    }
    public class UIAdaptiveSize
    {
        public int X;
        public int Y;
        /// <summary>
        /// 窗体初始宽度
        /// </summary>
        public int Width;
        /// <summary>
        /// 窗体初始高度
        /// </summary>
        public int Height;
        /// <summary>
        /// 窗体名字
        /// </summary>
        public string FormsName;
        /// <summary>
        /// 存储窗体控件初始大小
        /// </summary>
        /// <param name="cons">窗体对象（一般为this）</param>
        public void SetInitSize(Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;
                if (con.Controls.Count > 0) SetInitSize(con);
            }
        }
        /// <summary>
        /// 修改窗体控件大小
        /// </summary>
        /// <param name="x">改变后宽度</param>
        /// <param name="y">改变后高度</param>
        /// <param name="cons"></param>
        public void UpdateSize(float x, float y, Control cons)
        {

            var newx = x / Width;
            var newy = y / Height;
            foreach (Control con in cons.Controls)
            {
                if (con.Tag == null) continue;
                string[] mytag = con.Tag.ToString().Split(new char[] { ':' });
                float a = Convert.ToSingle(mytag[0]) * newx;
                con.Width = (int)a;
                a = Convert.ToSingle(mytag[1]) * newy;
                con.Height = (int)(a);
                a = Convert.ToSingle(mytag[2]) * newx;
                con.Left = (int)(a);
                a = Convert.ToSingle(mytag[3]) * newy;
                con.Top = (int)(a);
                Single currentSize = Convert.ToSingle(mytag[4]) * Math.Min(newx, newy);
                con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                if (con.Controls.Count > 0)
                {
                    UpdateSize(x, y, con);
                }
            }
        }



    }
}
