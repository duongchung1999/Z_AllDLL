using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MerryDllFramework
{
    /// <summary dllName="_2303_V1">
    /// 电源供给器 3303D/2303S
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        Dictionary<string, object> Config;

        public object Interface(Dictionary<string, object> config)
            => this.Config = config;
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：_2303_V1";
            string dllfunction = "Dll功能说明 ：电源供给器控制";
            string dllHistoryVersion = "历史Dll版本：23.4.12.0";
            string dllVersion = "当前Dll版本：23.10.31.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.10.31.0：增加读取电源值";

            string[] info = { dllname, dllfunction, dllHistoryVersion, dllVersion, dllChangeInfo };

            return info;
        }
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        SerialPort serialPort1 = null;
        static object lock_obj = new object();
        public string Run(object[] Command)
        {
            SplitCMD(Command, out string Comport, out string[] cmd);
            lock (lock_obj)
            {
                try
                {
                    switch (cmd[1])
                    {
                        case "SET":
                            return SET(Comport, cmd[2], cmd[3], cmd[4]);
                        case "GetVoltage": return GetVoltage(Comport, cmd[2]);
                        default:
                            return "Command Error False";
                    }
                }
                finally
                {
                    Thread.Sleep(150);
                    if (serialPort1.IsOpen) serialPort1.Close();
                    serialPort1.Dispose();
                }

            }

        }

        void SplitCMD(object[] Command, out string ComPort, out string[] CMD)
        {
            List<string> listCMD = new List<string>();
            foreach (var item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>))
                    OnceConfig = (Dictionary<string, object>)item;
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            ComPort = OnceConfig.ContainsKey("_2303SComport") ? (string)OnceConfig["_2303SComport"] : (string)Config["_2303SComport"];
            CMD = listCMD.ToArray();
        }

        /// <summary isPublicTestItem="true">
        /// 设置 3303D _ 2303
        /// </summary>
        /// <param name="volt">电压 比如 5</param>
        /// <param name="current">电流 比如 2</param>
        /// <param name="channel">单板设通道比如“1” 连扳 设定字段“_2303SChannel”</param>

        /// <returns></returns>
        public string SET(string comportName, string volt, string current, string channel)
        {
            string returnflag = "Not Send Command False";
            bool isMoreTest = OnceConfig.ContainsKey("SN");
            channel = isMoreTest ? (string)OnceConfig["_2303SChannel"] : channel;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (serialPort1 == null)
                    {
                        serialPort1 = new SerialPort
                        {
                            PortName = comportName,
                            BaudRate = 9600,
                            WriteTimeout = 1000,
                            ReadTimeout = 1000
                        };
                        Thread.Sleep(50);
                    }
                    if (!serialPort1.IsOpen) serialPort1.Open();

                    string[] str = new string[]
                     {
                        "OUT0",
                        $"ISET{ channel}:{current}",
                        $"VSET{channel}:{volt}",
                        "OUT1"
                     };
                    foreach (var item in str)
                        serialPort1.WriteLine(item);
                    return "True";
                }
                catch (Exception ex)
                {
                    returnflag = $"{ex.Message} False";
                    if (serialPort1.IsOpen) serialPort1.Close();
                    serialPort1.Dispose();
                    serialPort1 = null;
                }
                Thread.Sleep(500);
            }
            return returnflag;
        }
        /// <summary isPublicTestItem="true">
        ///  读取仪器电压
        /// </summary>
        /// <param name="channel">单板设通道比如“1” 连扳 设定字段“_2303SChannel”</param>
        /// <returns></returns>
        public string GetVoltage(string comportName, string channel)
        {
            string returnflag = "Not Send Command False";
            bool isMoreTest = OnceConfig.ContainsKey("SN");
            channel = isMoreTest ? (string)OnceConfig["_2303SChannel"] : channel;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (serialPort1 == null)
                    {

                        serialPort1 = new SerialPort
                        {
                            PortName = comportName,
                            BaudRate = 9600,
                            WriteTimeout = 1000,
                            ReadTimeout = 1000
                        };
                        Thread.Sleep(50);
                    }
                    if (!serialPort1.IsOpen) serialPort1.Open();
                    serialPort1.WriteLine($"VOUT{channel}?");
                    Thread.Sleep(250);
                    string str = serialPort1.ReadLine();
                    double value = Convert.ToDouble(str.Replace("V", ""));
                    return Math.Round(value, 2).ToString();
                }
                catch (Exception ex)
                {
                    returnflag = $"{ex.Message} False";
                    if (serialPort1.IsOpen) serialPort1.Close();
                    serialPort1.Dispose();
                    serialPort1 = null;
                }
                Thread.Sleep(500);
            }
            return returnflag;
        }

    }
}
