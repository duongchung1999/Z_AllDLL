using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace MerryDllFramework
{
    internal partial class userLogin : Form
    {
        /// <summary>
        /// 将该指针的窗口设为活动窗口
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);//设置此窗体为活动窗体
        public userLogin()
        {
            InitializeComponent();
        }
        private void btn_Enter_Click(object sender, EventArgs e)
        {
            string UserName = cb_UserName.Text;
            string Password = tb_Password.Text;
            if (UserName == "Engineer")
            {
                if (Password == "admin")
                {
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    tb_Password.Clear();
                    return;
                }

            }
        }

        private void tb_Password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            btn_Enter_Click(null, null);
        }

        private void userLogin_Load(object sender, EventArgs e)
        {
            if ("CH200001".Contains(Environment.UserName))
            {
                DialogResult = DialogResult.OK;
                this.Close();

            }


            SetForegroundWindow(Handle);
            List<string> UserNames = new List<string>();
            UserNames.Contains(Environment.UserName);

        }
    }
}
