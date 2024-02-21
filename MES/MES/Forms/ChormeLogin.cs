using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    public partial class ChormeLogin : Form
    {
        int OrderWorksLength;
        public ChormeLogin(int OrderWorksLength)
        {

            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.OrderWorksLength = OrderWorksLength;


        }
        private void Form1_Load(object sender, EventArgs e)
        {

            int x = System.Windows.Forms.SystemInformation.WorkingArea.Width / 2 - this.Size.Width / 2;
            int y = System.Windows.Forms.SystemInformation.WorkingArea.Height / 2 - this.Size.Height / 2;
            this.StartPosition = FormStartPosition.Manual; //窗体的位置由Location属性决定
            this.Location = (Point)new Size(x, y);         //窗体的起始位置为(x,y)
            tb_User.Focus();
        }

        private void tb_User_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tb_OrderWorks.Text.Length != OrderWorksLength)
                {
                    MessageBox.Show($"工单条码长度：{OrderWorksLength}");
                    tb_OrderWorks.Clear();
                    return;
                }
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tb_User_KeyDown(sender, new KeyEventArgs(Keys.Enter));
        }
    }
}
