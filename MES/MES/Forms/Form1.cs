using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MerryDllFramework;
using System.IO;

namespace MES
{
    public partial class Form1 : Form
    {
        int WorkOrderLength = 0;
        public Form1(int workOrderLength)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;

            WorkOrderLength = workOrderLength;
            //tb_Works.Focus();
            txtPassword.Focus();

        }
        
        private void btn_close_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            this.Close();
        }
        string path = ".\\AllDLL\\MES\\MES.txt";
        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            int x = System.Windows.Forms.SystemInformation.WorkingArea.Width / 2 - this.Size.Width / 2;
            int y = System.Windows.Forms.SystemInformation.WorkingArea.Height / 2 - this.Size.Height / 2;
            this.StartPosition = FormStartPosition.Manual; //窗体的位置由Location属性决定
            this.Location = (Point)new Size(x, y);         //窗体的起始位置为(x,y)
            tb_Works.Text = GetWorkOrder();
            tb_User.Focus();
        }
        private string GetWorkOrder()
        {
            if (!File.Exists(path)) File.Create(path);
            var array_item = File.ReadAllText(path).Split('\r', '\n');
            foreach(var item in array_item)
            {
                if (item.Contains("工单"))
                {
                    return item.Split('=')[1];
                }
            }
            return "";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tb_Works_KeyDown(sender, new KeyEventArgs(Keys.Enter));

        }

        private void tb_Works_TextChanged(object sender, EventArgs e)
        {

        }

       
        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            Login();

        }

        private void tb_Works_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            Login();
        }

        private void tb_User_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            Login();
        }

        string[] TeUser = { "91204127", "91204140", "91202346" };
        private void Login()
        {
            if (tb_User.Text == "91204127")
            {
                if (txtPassword.Text != "TETEST")
                {
                    MessageBox.Show("密码错误", "提醒");
                    tb_Works.Focus();
                    return;
                }
            }
            if (tb_Works.Text.Trim().Length != WorkOrderLength)
            {
                MessageBox.Show($"工单长度不合法,工单正确长度是{WorkOrderLength}位/Độ dài của công đơn không đúng, độ dài công đơn là {WorkOrderLength}");
                tb_Works.Clear();
                tb_Works.Focus();
                return;
            };
            MerryDll.userID = tb_User.Text.Trim();
            MerryDll.Works = tb_Works.Text.Trim();
            if (MerryDll.MesFlag >= 1)
            {
                if (!MerryDll.login()) return;
                if (MerryDll.BDFlag) new MerryDll().UpdateInfo();
            }
            else if (MerryDll.BDFlag)
            {
                new MerryDll().UpdateInfo();
            }
            var Workorder = $"工单={tb_Works.Text}";
            File.WriteAllText(path, Workorder);
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cbxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
        }
    }
    
}
