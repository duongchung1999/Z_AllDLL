using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    /// <summary dllName="MB1205C_V1">
    /// MB1205C
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：MB电源供予器";
            string dllfunction = "Dll功能说明 ：供电";
            string dllHistoryVersion = "历史Dll版本：0.0.0.0";
            string dllVersion = "当前Dll版本：23.11.2.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.11.2.0：修改成连扳模板使用";
            string[] info = {
                dllname, dllfunction, dllHistoryVersion,
                dllVersion,
                dllChangeInfo, dllChangeInfo1
            };
            return info;
        }
        Dictionary<string, object> OnceConfig;
        /// <summary>
        /// 平台共享参数
        /// </summary>
        Dictionary<string, object> Config = new Dictionary<string, object>();
        public object Interface(Dictionary<string, object> keys) => Config = keys;

        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] cmd);
            switch (cmd[1])
            {
                case "SetVoltage":
                    return SetVoltage(OnceConfig.ContainsKey("MB1205CPort") ?
                        (string)OnceConfig["MB1205CPort"] :
                        (string)Config["MB1205CPort"], cmd[2], cmd[3]);
                default: return "Command Error False";
            }

        }
        void SplitCMD(object[] Command, out string[] CMD)
        {
            string TestName = "";
            List<string> listCMD = new List<string>();

            foreach (var item in Command)
            {
                Type type = item.GetType();
                if (type.ToString().Contains("TestitemEntity"))
                {
                    PropertyInfo property = type.GetProperty("测试项目");
                    TestName = property.GetValue(item, null).ToString();
                }
                if (type == typeof(Dictionary<string, object>))
                    OnceConfig = (Dictionary<string, object>)item;
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            CMD = listCMD.ToArray();

        }

        /// <summary isPublicTestItem="true">
        /// 设置电压 串口在主程序Config 设置 字段 MB1205CPort
        /// </summary>
        /// <param name="address">物理ID RS485 默认是0001 常规是0003 RS232可以输入0003</param>
        /// <param name="Voltage">电压</param>
        /// <returns>True</returns>
        public string SetVoltage(string NamePort, string address, string Voltage)
        {

            string result = "Error False";
            string CommandStr = $"R{address}A V{Voltage}m";

            SerialPort port = new SerialPort()
            {
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                WriteTimeout = 1500,
                ReadTimeout=1500

            };
            try
            {
                port.PortName = NamePort;
                if (!port.IsOpen) port.Open();
                for (int i = 0; i < 3; i++)
                {
                    port.DiscardInBuffer();
                    port.WriteLine(CommandStr);
                    for (int c = 0; c < 2; c++)
                    {
                        Thread.Sleep(300);

                        if (port.BytesToRead > 0)
                        {
                            result = $"{port.ReadLine()} True";
                            return result;
                        }
                    }
                    result = "Read Str Null False";
                    Thread.Sleep(1500);
                }
            }
            catch (Exception ex)
            {
                result = $"{ex.Message} Error False";
            }
            finally
            {
                if (port.IsOpen) port.Close();
                port?.Dispose();
            }
            return result;

        }


    }
}
