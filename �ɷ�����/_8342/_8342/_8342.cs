using MerryDllFramework;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MerryDllFramework
{
    public class MerryDll
    {
        public void OnceConfigInterface(Dictionary<string, object> onceConfig) => OnceConfig = onceConfig;
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        string _8342Comport = "";

        public string Run(object[] Command)
        {
            string[] cmd = new string[0];
            foreach (object item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>)) OnceConfig = (Dictionary<string, object>)item;
                if (type == typeof(string))
                {
                    cmd = item.ToString().Split('&');
                    for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1].Trim();
                }
            }
            bool MoreTest = OnceConfig.ContainsKey("SN");
            string sgtr = "";
            foreach (var item in OnceConfig)
            {
                sgtr += $"{item.Value} {item.Key}";
            }

            switch (cmd[1])
            {

                case "A_DC":
                    return CurrentTest(MoreTest ? (string)OnceConfig["_8342Comport"] : GetComPort(), cmd[2], 1, cmd.Length >= 4 ? int.Parse(cmd[3]) : 1);
                case "A_AC":
                    return CurrentTest(MoreTest ? (string)OnceConfig["_8342Comport"] : GetComPort(), cmd[2], 2, cmd.Length >= 4 ? int.Parse(cmd[3]) : 1);
                case "V_DC":
                    return CurrentTest(MoreTest ? (string)OnceConfig["_8342Comport"] : GetComPort(), cmd[2], 3, cmd.Length >= 4 ? int.Parse(cmd[3]) : 1);
                case "V_AC":
                    return CurrentTest(MoreTest ? (string)OnceConfig["_8342Comport"] : GetComPort(), cmd[2], 4, cmd.Length >= 4 ? int.Parse(cmd[3]) : 1);
                case "RES":
                    return CurrentTest(MoreTest ? (string)OnceConfig["_8342Comport"] : GetComPort(), cmd[2], 5, cmd.Length >= 4 ? int.Parse(cmd[3]) : 1);
                case "Hz/P":
                    return CurrentTest(MoreTest ? (string)OnceConfig["_8342Comport"] : GetComPort(), cmd[2], 6, cmd.Length >= 4 ? int.Parse(cmd[3]) : 1);
                case "DIODe":
                    return DIODe(MoreTest ? (string)OnceConfig["_8342Comport"] : GetComPort());
                default:
                    return "Command Error Fasle";
            }

        }
        string GetComPort()
        {
            if (_8342Comport == "")
            {
                _8342Comport = "COM1";
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
                {
                    var portnames = SerialPort.GetPortNames();
                    var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
                    var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();
                    foreach (string s in portList)
                    {

                        if (s.Contains("834X")) _8342Comport = s.Substring(0, s.IndexOf(" "));
                    }
                }
            }
            return _8342Comport;


        }
        SerialPort port = new SerialPort();

        #region 8342电流表
        /// <summary>
        /// 读取电流表的值
        /// </summary>
        /// <param name="port">8342串口</param>
        /// <returns></returns>
        private double ReadValue(SerialPort port)
        {
            string Value = "";
            double V = 0;
            port.WriteLine(":VAL1?");
            Thread.Sleep(200);
            try
            {
                while (port.BytesToRead > 0)
                {

                    Value = port.ReadLine();
                    Thread.Sleep(50);

                }
                double.TryParse(Value, out V);
                return V;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 初始化电流表
        /// </summary>
        /// <param name="sPortName">串口名</param>
        /// <param name="range">档位</param>
        /// <param name="Type">类别</param>
        /// <returns></returns>
        private string CurrentInit(string sPortName, string range, int Type)
        {
            try
            {
                if (!port.IsOpen)
                {
                    port.Dispose();
                    port.PortName = sPortName;
                    port.BaudRate = 9600;
                    port.Parity = Parity.None;
                    port.DataBits = 8;
                    port.StopBits = StopBits.One;
                    port.ReadTimeout = 500;
                    port.Open();
                }
                if (!port.IsOpen) port.Open();
                port.WriteLine("*IDN?");
                Thread.Sleep(50);
                switch (Type)
                {
                    case 1: port.Write(":CONF:CURR:DC " + range + "\r\n"); break;
                    case 2: port.Write(":CONF:CURR:AC " + range + "\r\n"); break;
                    case 3: port.Write(":CONF:VOLT:DC " + range + "\r\n"); break;
                    case 4: port.Write(":CONF:VOLT:AC " + range + "\r\n"); break;
                    case 5: port.Write($":CONF:RES {range}\r\n"); break;
                    case 6: port.Write($":CONF:FREQ {range}\r\n"); break;
                }
            }
            catch (Exception ex)
            {

                if (port.IsOpen) port.Close();
                port.Dispose();
                _8342Comport = "";
                return $"{ex.Message} False";
            }
            return "True";
        }
        string oldrange = "";
        int oldcurrent = 0;
        private string CurrentTests(string sPortName, string range, int current)
        {
            string Value = "err";
            double value = 0;
            var newrange = range;
            var delay = "2000";
            //去掉延时后面的数据
            if (range.Contains("延时"))
            {
                int a = range.IndexOf("延时");
                newrange = range.Substring(0, a);
                delay = range.Substring(a + 2, range.Length - a - 2);
            }

            if ((oldrange != newrange) || (current != oldcurrent))
            {
                Value = CurrentInit(sPortName, newrange, current);
                if (Value.Contains("False")) return Value;

                oldrange = newrange;
                oldcurrent = current;
                Thread.Sleep(Convert.ToInt32(delay));
            }
            if (!port.IsOpen) port.Open();
            for (int i = 0; i < 5; i++)
            {
                value = ReadValue(port);
                if (value != 0) break;
                Thread.Sleep(100);
            }


            switch (newrange)
            {

                case "5": if (current == 1) value = value * 1000; break;
                case "0.5": value = value * 1000; break;
                case "0.0005":
                    if (current <= 2) value = value * 1000 * 1000;
                    if (current >= 3) value = value * 1000;
                    break;
            }
            Value = value.ToString("f2");
            return Value;
        }
        #endregion
        #region 8342电流表对外方法
        /// <summary>
        /// 8342电流表对外方法
        /// </summary>
        /// <param name="sPortName">串口名</param>
        /// <param name="lowerlimit">电流最大限定值</param>
        /// <param name="upperlimit">电流最小限定值</param>
        /// <param name="range">指令</param>
        /// <param name="type">类别（ 1.电流测试 2.直流电压测试 3.交流电压测试 ）</param>
        /// <param name="Value">实测测试电流值</param>
        /// <returns></returns>
        private string CurrentTest(string sPortName, string range, int type, int Count)
        {
            double Avg = 0;
            Count = Count <= 0 ? 1 : Count;
            bool ReadResult = false;
            try
            {
                //仅切换档位
                if (range.Contains("切换"))
                {
                    return CurrentInit(sPortName, range.Replace("切换", ""), type).ToString();
                }
                //测试电流
                for (int i = 0; i < Count; i++)
                {
                    string Value = "err";
                    Value = CurrentTests(sPortName, range, type);
                    if (!double.TryParse(Value, out double result))
                        return Value;
                    ReadResult = true;
                    Avg += result;
                    if (Count > 1) Thread.Sleep(500);
                }
                return ReadResult ? (Avg / (double)Count).ToString() : "Avg Error False";
            }
            catch (Exception EX)
            {
                _8342Comport = "";
                port.Dispose();
                return $"{EX.Message} False";
            }
        }
        string DIODe(string sPortName)
        {
            if (!port.IsOpen)
            {
                port.Dispose();
                port.PortName = sPortName;
                port.BaudRate = 9600;
                port.Parity = Parity.None;
                port.DataBits = 8;
                port.StopBits = StopBits.One;
                port.ReadTimeout = 500;
                port.Open();
            }
            if (!port.IsOpen) port.Open();
            port.WriteLine("*IDN?");
            port.Write("CONFigure:DIODe\r\n");
            port.DiscardInBuffer();
            Thread.Sleep(400);
            port.WriteLine(":VAL1?");
            Thread.Sleep(200);
            string Value = "";
            try
            {
                while (port.BytesToRead > 0)
                {

                    Value = port.ReadLine();
                    if (double.TryParse(Value, out double V))
                    {
                        return V.ToString();
                    }

                    Thread.Sleep(50);

                }
                return "Read Error False";
            }
            catch (Exception ex)
            {
                _8342Comport = "";
                port.Dispose();
                return $"{ex.Message} False";

            }

        }

        #endregion


        #region 接口方法
        Dictionary<string, object> Config;
        public object Interface(Dictionary<string, object> Config) => this.Config = Config;

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：_8342";
            string dllfunction = "Dll功能说明 ：电流表控制";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllVersion = "当前Dll版本：22.7.21.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "21.8.14.0：2021/8/14：修复电压测试值返回错误问题";
            string dllChangeInfo2 = "21.8.26.0：2021/8/26：增加电阻返回值";
            string dllChangeInfo3 = "22.7.21.0：增加电阻返回值";

            string[] info = { dllname, dllfunction, dllHistoryVersion ,
                dllVersion, dllChangeInfo,dllChangeInfo1,dllChangeInfo2,dllChangeInfo3
            };

            return info;
        }

        #endregion


    }
}