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

namespace BD
{
    public partial class ShowInformation : Form
    {
        ShowInformation(string OrderNumber, string Station, string User)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            MESBD.OrderNumber = OrderNumber;
            MESBD.mUser = User;
            MESBD.mStarion = Station;
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual; //窗体的位置由Location属性决定
            this.Location = (Point)new Size(-5, 0);
        }
        private static ShowInformation showInformation = null;
        public static ShowInformation Forms(string OrderNumber, string Station, string User, string systemName)
        {
            SystemName = systemName;
            if (showInformation != null)
            {
                try
                {
                    showInformation.Close();
                }
                catch { }

                Thread.Sleep(500);
            }


            if (showInformation == null || showInformation.IsDisposed)
            {
                showInformation = new ShowInformation(OrderNumber, Station, User);
            }
            return showInformation;
        }
        static string SystemName = "x";
        public string Msg = "";
        public void ShowInformation_Load(object sender, EventArgs e)
        {
      
            string mes = "";
            switch (SystemName.ToLower())
            {
                case "chorme":
                    textBox1.Text = Msg;
                    break;
                case "xiangwei":
                    MESBD.UpdateOrderInfo(out mes);
                    Invoke(new Action(() => textBox1.Text = mes));
                    break;
                default:
                    MESBD.UpdateOrderInfo(out mes);
                    Invoke(new Action(() => textBox1.Text = mes));
                    break;

            }
            if (mes.Contains("當前工單還沒有分配BDA號號段"))
            {
                timer1.Enabled = true;
                timer1.Start();
            }

        }
        public GetMESBD MESBD = new GetMESBD();

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains("當前工單還沒有分配BDA號號段")) this.Close();
        }
    }
}
