using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MerryDllFramework
{
    /// <summary dllName="MECL_Board">
    /// MECL_Board 多功能版
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        double addRXVolt = 0;
        List<string> formdata = new List<string>();
        public string Run(object[] Command)
        {
            SplitCMD(Command, out string SN, out string[] cmd);
            addRXVolt = double.TryParse(GetValue("MECL_Board", "addRXVolt"), out double num) ? num : 0;
            switch (cmd[1])
            {
                case "SelectChanel": return SelectChanel(cmd[2]);
                case "SetVoltage": return SetVoltage(cmd[2]);
                case "SetMaxCurrent": return SetMaxCurrent(cmd[2]);
                case "SetLoadCurrent": return SetLoadCurrent(cmd[2], cmd[3]);
                case "ReadVoltage": return ReadVoltage();
                case "ReadCurrent": return ReadCurrent();
                case "ReadLoadVoltage": return ReadLoadVoltage(cmd[2]);
                case "ReadLoadCurrent": return ReadLoadCurrent(cmd[2]);
                case "ReadPower": return ReadPower();
                case "ResetBoard": return ResetBoard();
                default: return "False Command Error";
            }
        }
        void SplitCMD(object[] Command, out string SN, out string[] CMD)
        {
            List<string> listCMD = new List<string>();
            foreach (var item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>)) OnceConfig = (Dictionary<string, object>)item;
                if (type == typeof(List<string>)) formdata = (List<string>)item;
                if (type == typeof(string))
                {
                    listCMD = new List<string>(item.ToString().Split('&'));
                    for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
                }

            }
            CMD = listCMD.ToArray();
            MoreTest = OnceConfig.ContainsKey("SN");
            SN = MoreTest ? (string)OnceConfig["SN"] : (string)Config["SN"];

        }

        //Dictionary<string, object> dic;
        string Port;
        public string Interface(Dictionary<string, object> keys)
        {
            Config = keys;
            Port = Config["MECL_Board"].ToString();
            return "";
        }
        /// <summary>
        /// 平台共享参数
        /// </summary>
        static Dictionary<string, object> Config = new Dictionary<string, object>();
        /// <summary>
        /// 连扳程序的参数
        /// </summary>
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        //接口
        public void OnceConfigInterface(Dictionary<string, object> onceConfig) => OnceConfig = onceConfig;
        bool MoreTest = false;

        private SerialPort port1;
        private string OpenPort()
        {
            port1 = new SerialPort();
            
            port1.PortName = Port;
            port1.BaudRate = 9600;
            port1.DataBits = 8;
            port1.StopBits = StopBits.One;
            port1.Parity = Parity.None;
            try
            {
                if (!port1.IsOpen)
                {
                    port1.Open();
                }
                else
                {
                    port1.Close();
                    Thread.Sleep(100);
                    port1.Open();
                }
                return true.ToString();
            }
            catch
            {
                return "Open has not successeed";
            }
        }
        private string SendByte(string[] values, int DelayTime)
        {
            try
            {
                if (OpenPort() != true.ToString()) return false.ToString();
                byte[] byteArray = new byte[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    byteArray[i] = Convert.ToByte(values[i], 16);
                }
                port1.Write(byteArray, 0, byteArray.Length);
                Thread.Sleep(DelayTime);
                byte[] responseData = new byte[port1.BytesToRead];
                port1.Read(responseData, 0, responseData.Length);
                var a = responseData[2].ToString("X");
                var b = responseData[3].ToString("X");
                var hex = string.Concat(a, b);
                return (Convert.ToInt32(hex, 16)).ToString();
            }
            catch
            {
                return "False Send Byte Fail";
            }
            finally
            {
                if (port1.IsOpen) port1.Close();
                port1.Dispose();
                Thread.Sleep(50);
            }

        }
        /// <summary isPublicTestItem="true">
        /// 选择充电通道A或者B
        /// </summary>
        /// <param name="chanel" options="A,B">通道</param>
        /// <returns>浮点数或报错信息</returns>
        private string SelectChanel(string chanel)
        {
            //读取充电电压 //00 AB 01 00 70 54
            string[] valuesA = { "0x00", "0xd1", "0x08", "0x01", "0x96", "0x1d" };
            string[] valuesB = { "0x00", "0xd1", "0x08", "0x00", "0x57", "0xdd" };
            chanel = chanel.ToUpper();
            if(chanel == "B")
            {
                var b = SendByte(valuesB, 100);
                if (!b.Contains("False")) return true.ToString();
                return false.ToString();
            }
            var a = SendByte(valuesA, 100);
            if (!a.Contains("False")) return true.ToString();
            return false.ToString();
        }
        /// <summary isPublicTestItem="true">
        /// 设定充电电流
        /// </summary>
        /// <param name="Volt" options="0,9000,9400">VoltageSet</param>
        /// <returns>浮点数或报错信息</returns>
        private string SetVoltage(string Volt)
        {
            //Setup Charging Voltage = 9.4V ---(因有线损)
            //string[] values = { "0x00", "0x1f", "0x24", "0xb8", "0x2a", "0x90" };//20V "0x00", "0x1f", "0x4e", "0x20", "0x05", "0x9a"
            string[] Startvalue = { "0x00", "0x1f"};
            return SendByte(CalByteSend(Startvalue, Volt), 3000);
        }

        /// <summary isPublicTestItem="true">
        /// 设定最大电流
        /// </summary>
        /// <param name="current" options="1000,3000">MaxCurrent</param>
        /// <returns>浮点数或报错信息</returns>
        private string SetMaxCurrent(string current)
        {
            //Setup max current = 3A
            //string[] values = { "0x00", "0x1c", "0x0b", "0xb8", "0xc6", "0xa0" };
            string[] Startvalue = { "0x00", "0x1c" };
            if (SendByte(CalByteSend(Startvalue, current), 100).Contains("False")) return false.ToString();
            return true.ToString(); ;
        }
        /// <summary isPublicTestItem="true">
        /// 设定拉载电流
        /// </summary>
        /// <param name="current" options="0,100,590,600">拉载电流</param>
        /// <param name="port" options="A,B" >通道</param>
        /// <returns>浮点数或报错信息</returns>
        private string SetLoadCurrent(string current, string port)
        {
            //Setup 负载电流 loadCurrent = 295mA  -->实际值600mA
            //00 a1 01 22 d0 4f  : 290mA
            //00 a1 01 27 10 4c  : 295mA
            //string[] values = { "0x00", "0xa1", "0x01", "0x27", "0x10", "0x4c" };//00 A1 02 58 51 5C
            int valueSet = Convert.ToInt32(current) / 2;
            string[] StartvalueA = { "0x00", "0xa1" };
            string[] StartvalueB = { "0x00", "0xa2" };
            if (port.ToUpper() == "B")
            {
                if (!SendByte(CalByteSend(StartvalueB, valueSet.ToString()), 100).Contains("False"))
                {
                    string[] values1 = { "0x00", "0xd1", "0x02", "0x01", "0x90", "0xbd" };//00 d1 02 01  90 bd 使能负载A
                    if (!SendByte(values1, 100).Contains("False")) return true.ToString();
                }
                return false.ToString();
            }
            if (!SendByte(CalByteSend(StartvalueA, valueSet.ToString()), 100).Contains("False"))
            {
                string[] values1 = { "0x00", "0xd1", "0x02", "0x01", "0x90", "0xbd" };//00 d1 02 01  90 bd 使能负载A
                if (!SendByte(values1, 100).Contains("False")) return true.ToString();
            }
            return false.ToString();
        }



        /// <summary isPublicTestItem="true">
        /// 读取充电电压
        /// </summary>
        /// <returns>浮点数或报错信息</returns>
        private string ReadVoltage()
        {
            //读取充电电压 //00 AB 01 00 70 54
            string[] values = { "0x00", "0xab", "0x01", "0x00", "0x70", "0x54" };
            var a = SendByte(values, 100);
            if (!a.Contains("False")) return (11 * Convert.ToDouble(a)).ToString();
            return false.ToString();
        }

        /// <summary isPublicTestItem="true">
        /// 读取充电电流
        /// </summary>
        /// <returns>浮点数或报错信息</returns>
        private string ReadCurrent()
        {
            //读取充电电流 //00 AB 02 00 70 A4
            string[] values = { "0x00", "0xab", "0x02", "0x00", "0x70", "0xA4" };
            var a = SendByte(values, 100);
            if (!a.Contains("False")) return (2 * Convert.ToInt32(a)).ToString();
            return false.ToString();
        }

        /// <summary isPublicTestItem="true">
        /// 读取拉载电压
        /// </summary>
        /// <param name="port" options="A,B" >通道</param>
        /// <returns>浮点数或报错信息</returns>
        private string ReadLoadVoltage(string port)
        {
            string[] valuesA = { "0x00", "0xab", "0x03", "0x00", "0x71", "0x34" };
            string[] valuesB = { "0x00", "0xab", "0x07", "0x00", "0x73", "0xf4"};
            //读取负载充电电压通道A //00 AB 03 00 71 34
            if (port.ToUpper() == "B")
            {
                var b = SendByte(valuesB, 100);
                if (!b.Contains("False")) return (Math.Round((10 * Convert.ToDouble(b)) / 1000,2) + addRXVolt).ToString();
                return false.ToString();
            }
            var a = SendByte(valuesA, 100);
            if (!a.Contains("False")) return (Math.Round((10 * Convert.ToDouble(a)) / 1000, 2) + addRXVolt).ToString();
            return false.ToString();
        }

        /// <summary isPublicTestItem="true">
        /// 读取拉载电流
        /// </summary>
        /// <param name="port" options="A,B" >通道</param>
        /// <returns>浮点数或报错信息</returns>
        private string ReadLoadCurrent(string port)
        {
            //读取负载充电电压通道A //00 AB 04 00 73 04
            string[] valuesA = { "0x00", "0xab", "0x04", "0x00", "0x73", "0x04" };
            string[] valuesB = { "0x00", "0xab", "0x08", "0x00", "0x76", "0x04" };
            if (port.ToUpper() == "B")
            {
                var b = SendByte(valuesB, 100);
                if (!b.Contains("False")) return (2 * Convert.ToInt32(b)).ToString();
                return false.ToString();
            }
            var a = SendByte(valuesA, 100);
            if (!a.Contains("False")) return (2 * Convert.ToInt32(a)).ToString();
            return false.ToString();
        }

        /// <summary isPublicTestItem="true">
        /// 读取功率
        /// </summary>
        /// <returns>浮点数或报错信息</returns>
        private string ReadPower()
        {
            try
            {
                int i = 0;
                double P = 0;
                while (i <= 5)
                {
                    i++;
                    var V = Convert.ToDouble(ReadVoltage()) / 1000;
                    var A = Convert.ToDouble(ReadCurrent()) / 1000;
                    P = Math.Round(V * A, 2);
                    if (P > 1) break;
                    Thread.Sleep(100);
                }
               
                return P.ToString();
            }
            catch (Exception ex)
            {
                return false.ToString();
            }
        }

        /// <summary isPublicTestItem="true">
        /// 复位板子
        /// </summary>
        /// <returns>浮点数或报错信息</returns>
        private string ResetBoard()
        {
            //读取充电电流 //00 AB 02 00 70 A4
            string[] values = { "0x00", "0xd0", "0x00", "0x00", "0x01", "0xdd" };
            var a = SendByte(values, 100);
            if (!a.Contains("False")) return true.ToString();
            return false.ToString();
        }
        private string[] CalByteSend(string[] StartByte, string SetupValue)
        {
            int Vset = Convert.ToInt32(SetupValue);
            string[] hexArray = new string[2];

            hexArray[0] = "0x" + (Vset >> 8).ToString("X2");
            hexArray[1] = "0x" + (Vset & 0xFF).ToString("X2");
            string[] values = { StartByte[0], StartByte[1], hexArray[0], hexArray[1] };
            var crc = CRC16_ModBus(values, 4).Split(' ');
            string[] send = { StartByte[0], StartByte[1], hexArray[0], hexArray[1], crc[1], crc[0] };
            return send;
        }

        string CRC16_ModBus(string[] values, int usDataLen)
        {
            byte[] puchMsg = new byte[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                puchMsg[i] = Convert.ToByte(values[i], 16);
            }
            uint uCRC = 0xffff;//CRC寄存器

            for (int num = 0; num < usDataLen; num++)
            {
                uCRC = (puchMsg[num]) ^ uCRC;//把数据与16位的CRC寄存器的低8位相异或，结果存放于CRC寄存器。
                for (int x = 0; x < 8; x++)
                {        //循环8次
                    if ((uCRC & 0x0001) == 1)
                    {        //判断最低位为：“1”
                        uCRC = uCRC >> 1;        //先右移
                        uCRC = uCRC ^ 0xA001;        //再与0xA001异或
                    }
                    else
                    {        //判断最低位为：“0”
                        uCRC = uCRC >> 1;        //右移
                    }
                }
            }
            string s = Convert.ToString((uCRC / 256), 16);
            string d = Convert.ToString((uCRC % 256), 16);
            return s + " " + d;//返回CRC校验值

        }
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：MECL_Board";
            string dllfunction = "Dll功能说明 ：MECL多功能板";
            string dllHistoryVersion = "历史Dll版本：无";
            string dllVersion = "当前Dll版本：23.8.17.9";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.8.17.7：2023/8/17：无";

            string[] info = { dllname, dllfunction, dllHistoryVersion,
                dllVersion, dllChangeInfo,dllChangeInfo1
            };

            return info;
        }
        string _path = ".\\AllDLL\\MenuStrip\\MECL_Board.ini";
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