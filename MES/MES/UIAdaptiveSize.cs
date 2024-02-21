using System;
using System.Drawing;
using System.Windows.Forms;

namespace MerryTest.Entity
{
    /// <summary>
    /// 记录最初始控件大小类
    /// </summary>
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
