using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

       

        private void Box_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Y) YES1_Click(sender, e);

        }


        private void YES1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();

        }



        private void Box_Load_1(object sender, EventArgs e)
        {

        }

        private void YES1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Box_Load(object sender, EventArgs e)
        {
            YES1.Focus();
        }

        private void YES1_Click_2(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
