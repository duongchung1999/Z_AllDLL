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
            string dllVersion = "当前Dll版本：22.4.14.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo2 = "21.8.1.0：2021/8/13：初步开发版";
            string dllChangeInfo3 = "22.4.14.0：增加专业针对性测颜色测LED灯模式";


            string[] info = { dllname,
                dllfunction,
                dllHistoryVersion,
                dllVersion,
                dllChangeInfo,
                dllChangeInfo2,dllChangeInfo3
            };
            return info;
        }

        public object Interface(Dictionary<string, object> keys) => keys;

        public string Run(object[] Command)
        {
            string[] cmd = new string[20];
            string AddName = "";
            double add;
            foreach (object item in Command)
            {
                if (item.GetType().ToString().Contains("TestitemEntity")) AddName = item.GetType().GetProperty("测试项目").GetValue(item, null).ToString();
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1].Trim();
            }
            add = double.TryParse(GetValue("HanOpticSens", AddName), out double num) ? num : 0;
            if (cmd.Length >= 4) cmd[3] = cmd[3].PadLeft(2, '0');

            switch (cmd[1])
            {

                case "r_doWave": return (add + double.Parse(CryTransparentMode(cmd[2], ":001r_dowave01-16\\n", cmd[3]))).ToString();

                case "r_lux": return (add + double.Parse(CryTransparentMode(cmd[2], ":001r_lux01-16\\n", cmd[3]))).ToString();

                case "r_cd": return (add + double.Parse(CryTransparentMode(cmd[2], ":001r_cd_mm01-16\\n", cmd[3]))).ToString();

                case "ColorCoordinateX": return (add + double.Parse(CryTransparentMode(cmd[2], ":001r_xy01-16\\n", cmd[3]))).ToString();

                case "ColorCoordinateY": return (add + double.Parse(CryTransparentMode(cmd[2], ":001r_xy01-16\\n", cmd[3]))).ToString();
                case "Send_Read": return (add + double.Parse(CryTransparentMode(cmd[2], cmd[3]))).ToString();

                case "WhiteWaveLength": return CryTransparentMode(cmd[2], ":001w_target_type01-16=0\\n").Contains(":001w_target_type").ToString();
                case "WhiteCCT": return CryTransparentMode(cmd[2], ":001w_target_type01-16=3\\n").Contains(":001w_target_type").ToString();
                case "MixtureWaveLength": return CryTransparentMode(cmd[2], ":001w_target_type01-16=4\\n").Contains(":001w_target_type").ToString();
                case "RGB_WaveLength": return CryTransparentMode(cmd[2], ":001w_target_type01-16=5\\n").Contains(":001w_target_type").ToString();
                case "RedWaveLength": return CryTransparentMode(cmd[2], ":001w_target_type01-16=6\\n").Contains(":001w_target_type").ToString();
                case "lux":
                    return (add + double.Parse(CryTransparentMode(cmd[2], $":001r_chroma{cmd[3]}-{cmd[3]}\\n", "1"))).ToString();
                case "X":
                    return (add + double.Parse(CryTransparentMode(cmd[2], $":001r_chroma{cmd[3]}-{cmd[3]}\\n", "2"))).ToString();
                case "Y":
                    return (add + double.Parse(CryTransparentMode(cmd[2], $":001r_chroma{cmd[3]}-{cmd[3]}\\n", "3"))).ToString();
                case "dowave":
                    return (add + double.Parse(CryTransparentMode(cmd[2], $":001r_chroma{cmd[3]}-{cmd[3]}\\n", "4"))).ToString();
                case "duty":
                    return (add + double.Parse(CryTransparentMode(cmd[2], $":001r_chroma{cmd[3]}-{cmd[3]}\\n", "5"))).ToString();
                case "cct":
                    return (add + double.Parse(CryTransparentMode(cmd[2], $":001r_chroma{cmd[3]}-{cmd[3]}\\n", "6"))).ToString();
                case "cd_mm":
                    return (add + double.Parse(cd_mm(cmd[2], $":001r_chroma{cmd[3]}-{cmd[3]}\\n", "1", double.Parse(cmd[4])))).ToString();

            }
            return "Command Error Fasle";

        }
        SerialPort sp;

        public string CryTransparentMode(string ComPort, string cmd, string Channel)
        {
            string result = CryTransparentMode(ComPort, cmd);
            if (result.Contains("False")) return "False";
            string[] results = result.Split('=');
            string[] resultss = results[1].Split(',');
            return resultss[int.Parse(Channel) - 1];


        }
        public string cd_mm(string ComPort, string cmd, string Channel, double Coefficient)
        {
            string result = CryTransparentMode(ComPort, cmd);
            if (result.Contains("False")) return "False";
            string[] results = result.Split('=');
            string[] resultss = results[1].Split(',');
            double cd = double.Parse(resultss[int.Parse(Channel) - 1]) * Coefficient;

            return cd.ToString("f2");
        }



        public string CryTransparentMode(string ComPort, string cmd)
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
                return Encoding.ASCII.GetString(recData);//将byte数组转换为ASCll OK = 4F 4B
            }
            catch (Exception ex)
            {
                sp = null;
                return $"{ex}Error False";
            }
            finally
            {
                if (sp.IsOpen) sp.Close();
                sp.Dispose();
                Thread.Sleep(50);
            }


        }







        string _path = ".\\AllDLL\\MenuStrip\\AddDeploy.ini";
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
