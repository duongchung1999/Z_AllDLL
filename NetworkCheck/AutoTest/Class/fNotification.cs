using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.Class
{
    public partial class fNotification : Form
    {
        string Notification = "";
        public fNotification(string Notification)
        {
            InitializeComponent();
            this.Notification = Notification;
        }

        private void fNotification_Load(object sender, EventArgs e)
        {
            label1.Text = Notification;
            btnOK.Focus();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
