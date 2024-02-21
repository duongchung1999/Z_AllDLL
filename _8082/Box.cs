using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
            label1.Text = msg;
        }

        private void Box_Load(object sender, EventArgs e)
        {
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
