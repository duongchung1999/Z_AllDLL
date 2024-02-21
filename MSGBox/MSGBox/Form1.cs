using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MSGBox
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = File.ReadAllText($"./xxxx.txt");
            int Y = 140;
            int X = 240;
            if (label1.Height > 140)
            {
                Y = label1.Height + 120;
            }
            if (label1.Width > 240)
            {
                 X= label1.Width + 50;
            }
            this.Height = Y;
            this.Width = X;
            int high = this.Height;
            int width = this.Width;
            button1.Location = new System.Drawing.Point(width / 2 - 50, Height - 85);
            int x = System.Windows.Forms.SystemInformation.WorkingArea.Width / 2 - this.Size.Width / 2;
            int y = System.Windows.Forms.SystemInformation.WorkingArea.Height / 2 - this.Size.Height / 2;
            this.StartPosition = FormStartPosition.Manual; //窗体的位置由Location属性决定
            this.Location = (Point)new Size(x, y);         //窗体的起始位置为(x,y)

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
