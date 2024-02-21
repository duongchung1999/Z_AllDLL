using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Config_ini.Forms
{
    public partial class UpdataInfoForms : Form
    {
        public UpdataInfoForms(List<string> listInfo)
        {
            InitializeComponent();
            this.listInfo = listInfo;

        }
        public List<string> listInfo;
        private void ConfirmUpdata_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

        }

        private void CancelUpdata_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void UpdataInfoForms_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox1.Items.AddRange(listInfo.ToArray());

        }

        private void DeleteSelect_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0) return;
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);

        }
    }
}
