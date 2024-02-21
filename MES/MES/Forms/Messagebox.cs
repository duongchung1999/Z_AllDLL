using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MES
{
    public partial class Messagebox : Form
    {
        public string dysSN;
        public string msg;
        public Messagebox(string msg)
        {
            InitializeComponent();
            //Control.CheckForIllegalCrossThreadCalls = false;
            this.msg = msg;
        }
        

        //private void textBox1_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Home)
        //    {
        //        DialogResult = DialogResult.No;
        //        this.Close();
        //    }
        //    if (e.KeyCode != Keys.Enter) return;
        //    if (textBox1.Text.Trim().Length != Length)
        //    {
        //        MessageBox.Show($"不良条码长度为 {Length}/ Độ dài của mã SN lỗi là {Length}");
        //        textBox1.Clear();
        //        textBox1.Focus();
        //        return;
        //    }
        //    dysSN = textBox1.Text.Trim();
        //    DialogResult = DialogResult.OK;
        //    this.Close();
        //}

        private void Messagebox_Load(object sender, EventArgs e)
        {
            int x = System.Windows.Forms.SystemInformation.WorkingArea.Width / 2 - this.Size.Width / 2;
            int y = System.Windows.Forms.SystemInformation.WorkingArea.Height / 2 - this.Size.Height / 2;
            this.StartPosition = FormStartPosition.Manual; //窗体的位置由Location属性决定
            this.Location = (Point)new Size(x, y);         //窗体的起始位置为(x,y)
            btnClose.Focus();
            label2.Text = msg;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Messagebox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            btnClose_Click(null, null);
        }
    }
}
