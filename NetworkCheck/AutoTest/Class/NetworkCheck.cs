using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.Class
{
    internal class NetworkCheck
    {
        public NetworkCheck(Dictionary<string, object> Config)
             => this.Config = Config;
        Dictionary<string, object> Config;
        string computer="";
        string type_mode ="";
        string WO = "";
        string PN = "";
        string Model = "";
        public void Network()
        {
            Thread.Sleep(2000);
            computer = $"{Config["Name"]} {Config["Station"]} CPU:{Config["ComputerName"]}";
            type_mode = TypeMode();
            WO = "";//工单
            PN = "";//料号
            Model = $"{Config["Name"]}";
            if ((int)Config["MesFlag"] > 0|| (bool)Config["GetBDFlag"])
            {
                WO = $"{ Config["Works"]}";
                PN = $"{Config["OrderNumberInformation"]}";
            }
            type_mode = $"{type_mode} 工单：{WO} , 料号：{PN}";
            Config["testtimes"] = Convert.ToDouble(0);//使用给评估测试效率
            Config["passtimes"] = Convert.ToDouble(0);
            Thread t = new Thread(formConnectSQL);
            t.Start();

        }
        private void formConnectSQL()
        {
            fNetworkMonitor f = new fNetworkMonitor(computer, Model, type_mode, Config);
            f.ShowDialog();
        }
        public string TypeMode()
        {
            string type_mode = "单机模式";
            string SystematicName = (string)Config["SystematicName"];

            if ((int)Config["MesFlag"] > 0)
            {
                type_mode = $"MES模式:{SystematicName}";
            }
            else if ((bool)Config["GetBDFlag"])
            {
                type_mode = $"BD模式:{SystematicName}";
            }
            string EngineerMode = (bool)Config["EngineerMode"] ? "工程" : "量产";
            type_mode = $"{EngineerMode}{type_mode}";
            return type_mode;
        }

        public void PerformencyUpdate()
        {
            try
            {
                if (((string)Config["SN"]).Contains("TE_BZP")) return;
                //MessageBox.Show($"{Config["testtimes"]}_{Config["passtimes"]}");
                Config["testtimes"] = (double)Config["testtimes"] + 1;
                if (((bool)Config["TestResultFlag"]))
                {
                    Config["passtimes"] = (double)Config["passtimes"] + 1;
                }
                double performency = (Convert.ToDouble(Config["passtimes"] )/ Convert.ToDouble(Config["testtimes"]) )* 100;
                performency = Math.Round(performency, 2);
                string cmd2 = $"Update Network  set performency = '{performency}%',test_times='{Config["testtimes"]}',pass_times={Config["passtimes"]} where computer = '{Config["Name"]} {Config["Station"]} CPU:{Config["ComputerName"]}'";
                MySqlHelper.Query(cmd2);

            }
            catch
            {

            }
            
        }
    }
}
