using System;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Forms;

namespace Common
{
    public partial class Box : Form
    {
        public Box(string msg)
        {
            InitializeComponent();
            Thread.Sleep(50);
            this.TopMost = true;
            label2.Text = msg;
        }

        private void YES1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Box_Load(object sender, EventArgs e)
        {

        }
    }
}
