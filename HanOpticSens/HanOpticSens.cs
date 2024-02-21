using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：HanOpticSens";
            string dllfunction = "Dll功能说明 ：涵测光电";
            string dllHistoryVersion = "历史Dll版本：21.8.1.0";
            string dllHistoryVersion2 = "                     ：0.0.2.1";
            string dllVersion = "当前Dll版本：21.8.1.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo2 = "21.8.1.0：2021/8/13：初步开发版";

            string[] info = { dllname,
                dllfunction,
                dllHistoryVersion,
                dllVersion,
                dllChangeInfo,
                dllChangeInfo2,
            };
            return info;
        }

    //    public object Interface(Dictionary<string, object> keys) => keys;
        Dictionary<string, object> Config;
        public object Interface(Dictionary<string, object> keyValues) => Config = keyValues;
        public string Run(object[] Command)
        {
            string[] cmd = new string[20];
            string AddName = "";
            double add;
            double luxOffset = 0;
            double[] luxRate = { 1,1,1,1};
            double value1 = 0;
            double value2 = 0;
            double value3 = 0;
            double value4 = 0;
            double value = 0;
            double R_value = 10;
            double G_value = 115;
            double B_value = 255;


            luxOffset = double.TryParse(GetValue("HanOpticSens", "luxOffset"), out double num) ? num : 0;
            luxRate[0] = double.TryParse(GetValue("HanOpticSens", "luxRate1"), out num) ? num : 1;
            luxRate[1] = double.TryParse(GetValue("HanOpticSens", "luxRate2"), out num) ? num : 1;
            luxRate[2] = double.TryParse(GetValue("HanOpticSens", "luxRate3"), out num) ? num : 1;
            luxRate[3] = double.TryParse(GetValue("HanOpticSens", "luxRate4"), out num) ? num : 1;
            R_value = double.TryParse(GetValue("HanOpticSens", "R"), out num) ? num : 10;
            G_value = double.TryParse(GetValue("HanOpticSens", "G"), out num) ? num : 115;
            B_value = double.TryParse(GetValue("HanOpticSens", "B"), out num) ? num : 255;
            foreach (object item in Command)
            {
                if (item.GetType().ToString().Contains("TestitemEntity")) AddName = item.GetType().GetProperty("测试项目").GetValue(item, null).ToString();
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1].Trim();
            }
            // add = double.TryParse(GetValue("HanOpticSens", AddName), out double num) ? num : 0;
            switch (cmd[1])
            {
                case "w_TargetType": return SendCommand(cmd[2], $":001w_target_type01-16={cmd[3]}\\n");

                case "r_doWave": return (double.Parse(CryTransparentMode(cmd[2], ":001r_dowave01-16\\n", cmd[3]))).ToString();

                case "r_lux":
                    Thread.Sleep(300);
                    value1 = double.Parse(CryTransparentMode(cmd[2], ":001r_lux01-16\\n", cmd[3]));
                    Thread.Sleep(300);
                    value2 = double.Parse(CryTransparentMode(cmd[2], ":001r_lux01-16\\n", cmd[3]));
                    Thread.Sleep(300);
                    value3 = double.Parse(CryTransparentMode(cmd[2], ":001r_lux01-16\\n", cmd[3]));
                    Thread.Sleep(300);
                    value4 = double.Parse(CryTransparentMode(cmd[2], ":001r_lux01-16\\n", cmd[3]));
                    Thread.Sleep(300);
                    value = (double.Parse(CryTransparentMode(cmd[2], ":001r_lux01-16\\n", cmd[3])) + value1 + value2 + value3 + value4) / 5;
                    Config["_TestValue"] = (luxOffset + value * luxRate[Convert.ToInt32(cmd[3]) - 1]).ToString();
                    //       Config["_TestValue"] = (luxOffset + double.Parse(CryTransparentMode(cmd[2], ":001r_lux01-16\\n", cmd[3]))*luxRate[Convert.ToInt32(cmd[3]) - 1]).ToString();
                    return Config["_TestValue"].ToString();
                case "r_cd": return (double.Parse(CryTransparentMode(cmd[2], ":001r_cd_mm01-16\\n", cmd[3]))).ToString();

                case "ColorCoordinateX":
                    Thread.Sleep(300);
                    value1 = double.Parse(CryTransparentMode(cmd[2], ":001r_xy01-16\\n", cmd[3]));
                    Thread.Sleep(300);
                    value2 = double.Parse(CryTransparentMode(cmd[2], ":001r_xy01-16\\n", cmd[3]));
                    Thread.Sleep(300);
                    value3 = double.Parse(CryTransparentMode(cmd[2], ":001r_xy01-16\\n", cmd[3]));
                    Thread.Sleep(300);
                    value4 = double.Parse(CryTransparentMode(cmd[2], ":001r_xy01-16\\n", cmd[3]));
                    Thread.Sleep(300);
                    value = (double.Parse(CryTransparentMode(cmd[2], ":001r_xy01-16\\n", cmd[3])) + value1 + value2 + value3 + value4) / 5;
                    return value.ToString();

                case "ColorCoordinateY": return (double.Parse(CryTransparentMode(cmd[2], ":001r_xy01-16\\n", cmd[3]))).ToString();

                case "Send_Read": return (double.Parse(CryTransparentMode(cmd[2], cmd[3], cmd[3]))).ToString();

                //case "RGB": return (double.Parse(CryTransparentMode(cmd[2], ":001r_rgbi01-02\\n", cmd[3]))).ToString();
            }
            return "Command Error Fasle";

        }

        SerialPort sp;

        public string SendCommand(string comPort, string cmd)
        {
            try
            {
                if (sp == null)
                {
                    sp = new SerialPort();
                    sp.PortName = comPort;
                    sp.BaudRate = 115200;
                    sp.DataBits = 8;
                }
                if (!sp.IsOpen) sp.Open();
                sp.Write(cmd);
                sp.Write(new Byte[] { 0X0A }, 0, 1);
                Thread.Sleep(50);
                var recData = new byte[sp.BytesToRead];
                sp.Read(recData, 0, recData.Length);
                string value = Encoding.ASCII.GetString(recData);
                if (value.Contains(cmd))
                {
                    return true.ToString();
                }
                return false.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"HanOpticSensDLL: {ex.Message}");
                return false.ToString();
            }
            finally
            {
                if (sp.IsOpen) sp.Close();
                sp.Dispose();
                Thread.Sleep(50);
            }
        }

        public string CryTransparentMode(string ComPort, string cmd, string Channel)
        {
            try
            {
                if (sp == null)
                {
                    sp = new SerialPort();
                    sp.PortName = ComPort;
                    sp.BaudRate = 115200;
                    sp.DataBits = 8;

                }
                if (!sp.IsOpen) sp.Open();
                sp.Write(cmd);
                sp.Write(new Byte[] { 0X0A }, 0, 1);
                Thread.Sleep(50); //毫秒内数据接收完毕，可根据实际情况调整
                var recData = new byte[sp.BytesToRead];
                sp.Read(recData, 0, recData.Length);
                string Value = Encoding.ASCII.GetString(recData);//将byte数组转换为ASCll OK = 4F 4B
                string[] result = Value.Split('=');
                if (result.Length > 1)
                {
                    return result[1].Split(',')[int.Parse(Channel) - 1];
                }
                return Value;
            }
            catch (Exception ex)
            {
                sp = null;
                MessageBox.Show(ex.ToString());
                return "Error";
            }
            finally
            {
                if (sp.IsOpen) sp.Close();
                sp.Dispose();
                Thread.Sleep(50);
            }


        }


        //string _path = ".\\AllDLL\\MenuStrip\\AddDeploy.ini";
        string _path = ".\\AllDLL\\MenuStrip\\HanOpticSens.ini";
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVar, int size, string INIpath);
        public string GetValue(string section, string key)
        {
            try
            {
                StringBuilder var = new StringBuilder(512);
                GetPrivateProfileString(section, key, "0", var, 512, _path);
                return var.ToString().Trim();
            }
            catch
            {
                return "0";
            }

        }

    }
}
