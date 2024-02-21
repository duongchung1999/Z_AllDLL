using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;


namespace MerryDllFramework
{
    /// <summary dllName="STM8">
    /// STM8
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        public string Run(object[] Command)
        {
            string[] cmd = new string[20];
            
            //addRfVolt = double.TryParse(GetValue("MECL_Board", "Rf_Volt"), out double num) ? num : 0;

            foreach (var item in Command)
            {
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            switch (cmd[1])
            {
                case "ReadVoltage": return ReadVoltage(cmd[2]).ToString();
                //电流区域
                case "ReadCurrent": return ReadCurrent(cmd[2]).ToString();
                //继电器区域
                case "Relay":
                    return Relay(cmd[2]);
                default: return "False Command Error False";
            }
        }

        Dictionary<string, object> dic;
        

        public string Interface(Dictionary<string, object> keys)
        {
            dic = keys;
            STM8_COM = dic["STM8Comport"].ToString();
            return "";
        }
        string STM8_COM = "";
     

        SerialPort Comport = new SerialPort();
        string[] Value;

        /// <summary isPublicTestItem="true">
        /// 读取电压
        /// </summary>
        /// <param name="channel" options="1,2,3,4,5,6,7,8,9,10">通道</param>
        /// <returns>浮点数或报错信息</returns>
        string ReadVoltage(string channel)
        {
            int i = 0;
            
            string Command = "start";
            Value = null;
            try
            {
                Comport.PortName = STM8_COM;
                Comport.BaudRate = 9600;
                if (!Comport.IsOpen) Comport.Open();
                Comport.Write(Command);
                Thread.Sleep(1000);
                Value = Comport.ReadExisting().Split('\n');
                switch (channel)
                {
                    case "1": return Value[10] != null ? (Math.Round(Convert.ToDouble(Value[10]), 2)).ToString("0.00") : "False null";
                    case "2": return Value[9] != null ? (Math.Round(Convert.ToDouble(Value[9]), 2)).ToString("0.00") : "False null";
                    case "3": return Value[1] != null ? (Math.Round(Convert.ToDouble(Value[1]), 2)).ToString("0.00") : "False null";
                    case "4": return Value[2] != null ? (Math.Round(Convert.ToDouble(Value[2]), 2)).ToString("0.00") : "False null";
                    case "5": return Value[3] != null ? (Math.Round(Convert.ToDouble(Value[3]), 2)).ToString("0.00") : "False null";
                    case "6": return Value[4] != null ? (Math.Round(Convert.ToDouble(Value[4]), 2)).ToString("0.00") : "False null";
                    case "7": return Value[5] != null ? (Math.Round(Convert.ToDouble(Value[5]), 2)).ToString("0.00") : "False null";
                    case "8": return Value[6] != null ? (Math.Round(Convert.ToDouble(Value[6]), 2)).ToString("0.00") : "False null";
                    case "9": return Value[7] != null ? (Math.Round(Convert.ToDouble(Value[7]), 2)).ToString("0.00") : "False null";
                    case "10": return Value[8] != null ? (Math.Round(Convert.ToDouble(Value[8]), 2)).ToString("0.00") : "False null";
                    default: return "False channel error";
                }
                //return Value[i].ToString();
            }
            catch (Exception ex)
            {
                return $"False COMPORT ERROR： {STM8_COM}  {ex.Message}";
            }
            finally
            {
                if (Comport.IsOpen) Comport.Close();
                Comport.Dispose();
            }
        }
        /// <summary isPublicTestItem="true">
        /// 读取电流
        /// </summary>
        /// <param name="channel" options="0,1,2,3,4,5,6,7,8,9">通道</param>
        /// <returns>浮点数或报错信息</returns>
        string ReadCurrent(string channel)
        {
            try
            {
                string command = "";
                switch (channel)
                {
                    case "0": command = "test0"; break;
                    case "1": command = "test1"; break;
                    case "2": command = "test2"; break;
                    case "3": command = "test3"; break;
                    case "4": command = "test4"; break;
                    case "5": command = "test5"; break;
                    case "6": command = "test6"; break;
                    case "7": command = "test7"; break;
                    case "8": command = "test8"; break;
                    case "9": command = "test9"; break;
                    default: return "False channel error";
                }
                Comport.PortName = STM8_COM;
                Comport.BaudRate = 9600;
                if (!Comport.IsOpen) Comport.Open();
                Comport.Write(command);
                Thread.Sleep(1000);
                var result = Math.Round(Convert.ToDouble(Comport.ReadExisting()), 4);//(Math.Round(Convert.ToDouble(Comport.ReadExisting()), 2)).ToString()
                return result.ToString("0.0000");
            }
            catch (Exception ex)
            {
                return $"False COMPORT ERROR: {STM8_COM} {ex.Message}";
            }
            finally
            {
                if (Comport.IsOpen) Comport.Close();
                Comport.Dispose();
            }
        }

        /// <summary isPublicTestItem="true">
        /// 控制继电器 1 ~ 24
        /// </summary>
        /// <param name="cmd" options="0,1,2,1.2.3.4">通道</param>
        /// <returns>浮点数或报错信息</returns>
        string Relay(string cmd)
        {
            try
            {
                byte b0 = 0x00;
                byte b1 = 0x00;
                byte b2 = 0x00;
                byte b3 = 0x00;
                byte b4 = 0x00;
                var relays = cmd.Split('.');
                //初始化继电器的Byte地址
                byte[] relay_Address = { 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x10, 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x20, 0x02, 0x01, 0x08, 0x04, 0x01, 0x01, 0x08, 0x02 };
                //关闭所有继电器的指令
                byte[] s0 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x23 };

                Comport.PortName = STM8_COM;
                Comport.BaudRate = 9600;
                Comport.DataBits = 8;
                if (!Comport.IsOpen) Comport.Open();
                //Comport.Write(s0, 0, s0.Length);
                //Thread.Sleep(50);
                foreach (var relay_number in relays)
                {
                    var number = Convert.ToInt32(relay_number) - 1;
                    if (number <= 5 && number >= 0)
                    {
                        b0 = (byte)(b0 + relay_Address[number]);
                    }
                    if (number >= 8 && number <= 14)
                    {
                        b1 = (byte)(b1 + +relay_Address[number]);
                    }
                    if ((number == 6 || number == 7) || (number >= 18 && number <= 20) || number == 23)
                    {
                        b2 = (byte)(b2 + +relay_Address[number]);
                    }
                    if (number == 15 || number == 21 || number == 22)
                    {
                        b3 = (byte)(b3 + +relay_Address[number]);
                    }
                    if (number == 16 || number == 17)
                    {
                        b4 = (byte)(b4 + +relay_Address[number]);
                    }

                }
                var B0 = $"0x{b0.ToString("X2")}";
                var B1 = $"0x{b1.ToString("X2")}";
                var B2 = $"0x{b2.ToString("X2")}";
                var B3 = $"0x{b3.ToString("X2")}";
                var B4 = $"0x{b4.ToString("X2")}";
                string[] send = { B0, B1, B2, B3, B4, "0x23" };
                byte[] byteArray = new byte[send.Length];
                for (int i = 0; i < send.Length; i++)
                {
                    byteArray[i] = Convert.ToByte(send[i], 16);
                }
                Comport.Write(byteArray, 0, byteArray.Length);
                Thread.Sleep(50);


                //Thread.Sleep(1000);
                return true.ToString();
            }
            catch (Exception ex)
            {
                return $"False COMPORT ERROR: {STM8_COM} {ex.Message}";
            }
            finally
            {
                if (Comport.IsOpen) Comport.Close();
                Comport.Dispose();
            }
        }

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：STM8";
            string dllfunction = "Dll功能说明 ：STM8 MEVN Board";
            string dllHistoryVersion = "历史Dll版本：23.8.17.7";
            string dllVersion = "当前Dll版本：23.8.28.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.8.28.0：23.8.28.0：无";
            string[] info = { dllname, dllfunction, dllHistoryVersion,
                dllVersion, dllChangeInfo,dllChangeInfo1
            };

            return info;
        }
        string _path = ".\\AllDLL\\MenuStrip\\STM8.ini";
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