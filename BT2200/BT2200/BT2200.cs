using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net.Security;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    /// <summary dllName="BT2200">
    /// BT2200 蓝牙适配器
    /// </summary>
    public class MerryDll : IMerryAllDll, IDisposable
    {
        ~MerryDll()
        {
            PORT?.Dispose();
            Dispose();
        }
        public void Dispose()
        {
            PORT?.Dispose();
        }
        #region 接口方法
        Dictionary<string, object> Config;
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        bool TE_BZP;
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：BT2200";
            string dllfunction = "Dll功能说明 ：BT2200仪器";
            string dllHistoryVersion = "历史Dll版本：    ";
            string dllVersion = "当前Dll版本：23.7.31.1";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.6.21.0：首写";
            string dllChangeInfo2 = "23.6.30.0：修复用TE_BZP配对失败问题";
            string dllChangeInfo3 = "23.7.31.0：修复下SPP指令错位问题，每次下指令会检测串口状态";


            string[] info = { dllname, dllfunction,
                dllHistoryVersion,
                dllVersion,
                dllChangeInfo,dllChangeInfo1
            };

            return info;
        }
        public object Interface(Dictionary<string, object> Config)
            => this.Config = Config;
        #endregion

        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] cmd);
            switch (cmd[1])
            {
                #region 常规读取
                #endregion

                case "Connect": return Connect(cmd[2], int.Parse(cmd[3]));
                case "ConnectSPP": return ConnectSPP();
                case "A2DP": return A2DP();
                case "HFP": return HFP();
                case "SPPSendCMD": return SPPSendCMD(cmd[2], cmd[3]);
                case "SendBT2200CMD": return SendBT2200CMD(cmd[2]);
                case "Disconnect": return Disconnect();
                case "OpenComPort": return OpenComPort();
                case "ColseComPort": return ColseComPort();

            }
            return $"Command Error {false}";
        }

        void SplitCMD(object[] Command, out string[] CMD)
        {
            List<string> listCMD = new List<string>();
            string TestName = "";

            foreach (var item in Command)
            {
                Type type = item.GetType();

                if (type.ToString().Contains("TestitemEntity"))
                {
                    PropertyInfo property = type.GetProperty("测试项目");
                    TestName = property.GetValue(item, null).ToString();
                }
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            CMD = listCMD.ToArray();
            string SN = OnceConfig.ContainsKey("SN") ? OnceConfig["SN"].ToString() : Config["SN"].ToString();
            TE_BZP = SN.Contains("TE_BZP");
        }
        static SerialPort PORT = null;
        /// <summary isPublicTestItem="true">
        /// 建立连接 标准品默认盲连
        /// </summary>
        /// <param name="ManualFlag" options="True,False">True 为指定连接地址在Config["BitAddress"] 或 False 盲连</param>
        /// <param name="Count">配对次数 每次大约10秒 建议2次</param>
        /// <returns>配对成功的BD号</returns>
        public string Connect(string ManualFlag, int Count)
        {
            OpenComPort();
            PortLog.AddRange(new string[] { "", "", "", "#####################SPP_Connect#####################" });
            string ConnectCMD = ">NO_MAC_CON";
            string readStr = "";
            string BD = "True";
            bool IsConnect = false;

            if (TE_BZP || ManualFlag != "True")
            {
                ConnectCMD = ">NO_MAC_CON";
            }
            else
            {
                string BD_ = Config["BitAddress"].ToString();
                ConnectCMD = $">CONN={BD_}";

            }
            if (!PORT.IsOpen) PORT.Open();

            for (int d = 0; d < Count; d++)
            {
                WriteLog(ConnectCMD);
                Thread.Sleep(2000);
                for (int i = 0; i < 8; i++)
                {
                    ReadLog(out readStr, out _);
                    if (readStr.Contains("Success") ||
                         readStr.Contains("avrcp_success") ||
                         readStr.Contains("a2dp_connect_success")
                        )
                    {
                        IsConnect = true;
                        break;
                    }
                    Thread.Sleep(1000);
                }

                if (IsConnect)
                {
                    WriteLog(">GET_CONN_INFO");
                    Thread.Sleep(500);
                    ReadLog(out readStr, out _);
                    if (readStr.Contains("DEVICE="))
                    {
                        string[] strs = readStr.Split(new string[] { "DEVICE=" }, StringSplitOptions.None);
                        BD = strs[1].Substring(0, 12).ToUpper();
                    }
                    Thread.Sleep(1500);
                    return BD;
                }
                Thread.Sleep(1000);
            }
            return $"{readStr} Time Out False";


        }
        /// <summary isPublicTestItem="true">
        /// 在连接基础上再连接SPP
        /// </summary>
        /// <returns></returns>
        public string ConnectSPP()
        {
            ReadLog(out string readStr, out _);
            for (int i = 0; i < 3; i++)
            {
                WriteLog(">SPP_CONN");
                Thread.Sleep(1000);
                ReadLog(out readStr, out _);
                if (readStr.Contains("SPP_CONNECTED"))
                {
                    return "True";
                }
                Thread.Sleep(1000);
            }

            return "Connect SPP False";
        }
        /// <summary isPublicTestItem="true">
        /// 进入 A2DP
        /// </summary>
        /// <returns></returns>
        public string A2DP()
        {
            string Readstr = "Not Send CMD";
            for (int i = 0; i < 3; i++)
            {
                WriteLog(">OPEN HFP");
                Thread.Sleep(150);
                ReadLog(out Readstr, out _);
                WriteLog(">OPEN A2DP");
                Thread.Sleep(100);
                ReadLog(out Readstr, out _);
                if (Readstr.Contains("A2DP"))
                {
                    return "Switch A2DP True";

                }

            }
            return Readstr;
        }
        /// <summary isPublicTestItem="true">
        /// 进入HFP
        /// </summary>
        /// <returns></returns>
        public string HFP()
        {
            string Readstr = "Not Send CMD";
            for (int i = 0; i < 3; i++)
            {
                WriteLog(">OPEN A2DP");
                Thread.Sleep(150);
                ReadLog(out Readstr, out _);
                WriteLog(">OPEN HFP");
                Thread.Sleep(100);
                ReadLog(out Readstr, out _);
                if (Readstr.Contains("HFP"))
                {
                    return "Switch HFP True";

                }
            }
            return Readstr;

        }
        /// <summary isPublicTestItem="true">
        /// 在SPP连接基础上下指令
        /// </summary>
        /// <param name="CMD">指令 16进制 例如 3E D5 01 01 03</param>
        /// <param name="ContainsValue">检查指令返回值 例如 E3 D5</param>
        /// <returns></returns>
        public string SPPSendCMD(string CMD, string ContainsValue)
        {
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 2; i++)
            {
                WriteLog($"{CMD}".HexStrToBytes());
                ReadLog(out _, out ByteStr);
                if (ByteStr.Contains(ContainsValue)) return "True";
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        /// <summary isPublicTestItem="true">
        /// 对BT2200下指令 
        /// </summary>
        /// <param name="CMD">指令 例如 >OPEN A2DP</param>
        /// <returns></returns>
        public string SendBT2200CMD(string CMD)
        {
            OpenComPort();
            WriteLog(CMD);
            return "True";
        }
        /// <summary isPublicTestItem="true">
        /// 断开连接 重置仪器 释放缓存 生成Log
        /// </summary>
        /// <returns></returns>
        string Disconnect()
        {
            WriteLog(">DISC");
            Thread.Sleep(50);
            WriteLog(">RST");
            ReadLog(out _, out _);
            if (PORT.IsOpen) PORT.Close();
            PortLog.AddRange(new string[] { "","","#####################Disconnect#####################",""
            });
            PORT.Dispose();
            File.AppendAllLines($".\\LOG\\时间{DateTime.Now.ToString("MM月dd日")}SPP Log.txt", PortLog.ToArray());
            PortLog.Clear();
            return "True";
        }



        List<string> PortLog = new List<string>();
        void WriteLog(string Command)
        {
            OpenComPort();
            PORT.Write($"{Command}\r\n");
            PORT.NewLine = "\r\n";
            PortLog.Add($"########{DateTime.Now}  |  Write  |  {Command}\r\n");
            Thread.Sleep(250);
        }
        void WriteLog(byte[] Command)
        {
            OpenComPort();
            PORT.Write(Command, 0, Command.Length);
            string log = "";
            foreach (var item in Command) log += $"{item.ToString("X2").PadLeft(2, '0')}";
            PortLog.Add($"{DateTime.Now}  |  Write  |  HEX：{log}\r\n");
            Thread.Sleep(250);
        }
        void ReadLog(out string ReadStr, out string byteStr)
        {
            OpenComPort();
            ReadStr = "";
            byteStr = "";
            if (PORT.BytesToRead > 0)
            {
                byte[] ReadByte = null;
                byteStr = "";
                ReadByte = new byte[PORT.BytesToRead];
                PORT.Read(ReadByte, 0, ReadByte.Length);
                foreach (var item in ReadByte) byteStr += $"{item.ToString("X2").PadLeft(2, '0')} ";
                ReadStr = Encoding.ASCII.GetString(ReadByte);
                PortLog.Add($"{DateTime.Now}  |  Read  |  ASCII：{ReadStr}  |  HEX： {byteStr}\r\n");
            }
        }

        /// <summary isPublicTestItem="false">
        /// 单单打开串口
        /// </summary>
        /// <returns></returns>
        public string OpenComPort()
        {
            if (PORT == null)
            {
                string PortName = "COM1";
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
                {
                    var portnames = SerialPort.GetPortNames();
                    var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
                    var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();
                    foreach (string s in portList) if (s.Contains("Prolific USB-to-Serial Comm Port")) PortName = s.Substring(0, s.IndexOf(" "));
                }
                PORT = new SerialPort
                {
                    PortName = PortName,
                    BaudRate = 921600,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Parity = Parity.None
                };

            }
            if (!PORT.IsOpen) PORT.Open();
            return PORT.IsOpen.ToString();
        }
        /// <summary isPublicTestItem="true">
        /// 单单关闭串口
        /// </summary>
        /// <returns></returns>
        public string ColseComPort()
        {
            if (PORT != null && PORT.IsOpen)
                PORT.Close();
            return "True";

        }


    }
    static class Myconvert
    {
        public static byte[] HexStrToBytes(this string str)
        {
            List<byte> bytes = new List<byte>();
            String CMD = str.Trim().Replace(" ", "");
            for (int i = 0; i < CMD.Length; i += 2)
            {
                bytes.Add(Convert.ToByte(CMD.Substring(i, 2), 16));
            }

            return bytes.ToArray();
        }
    }


}

