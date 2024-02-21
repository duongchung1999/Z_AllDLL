using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：MB电源供予器";
            string dllfunction = "Dll功能说明 ：供电";
            string dllHistoryVersion = "历史Dll版本：0.0.0.0";
            string dllVersion = "当前Dll版本：22.9.19.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "0.0.0.0：2021 /1/3：修改符合一对多测试模板";
            string dllChangeInfo2 = "22.9.19.0：修改符合一对多测试模板";


            string[] info = {
                dllname, dllfunction, dllHistoryVersion,
                dllVersion,
                dllChangeInfo,dllChangeInfo1,dllChangeInfo2
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
            List<string> cmd = new List<string>();
            foreach (object item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>))
                    OnceConfig = (Dictionary<string, object>)item;
                if (type == typeof(string))
                {
                    cmd.AddRange(item.ToString().Split('&'));
                    for (int i = 0; i < cmd.Count; i++) cmd[i] = cmd[i].Split('=')[1].Trim();
                }
            }
            string cmdID = cmd.Count >= 4 ? cmd[3].PadLeft(4, '0') : "0003";
            string CommandStr = $"R{cmdID}A V{cmd[2]}m";
            switch (cmd[1])
            {
                case "SetVoltage":
                    return MB125CPort(OnceConfig.ContainsKey("MB1205CPort") ? (string)OnceConfig["MB1205CPort"] : (string)Config["MB1205CPort"], $"{CommandStr}");
                default: return "指令错误 False";
            }

        }
        public string MB125CPort(string NamePort, string command)
        {

            string result = "Error False";
            for (int i = 0; i < 3; i++)
            {
                using (SerialPort port = new SerialPort()
                {
                    BaudRate = 9600,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One
                })
                {
                    try
                    {
                        port.PortName = NamePort;
                        if (!port.IsOpen) port.Open();
                        port.WriteLine(command);
                        Thread.Sleep(100);
                        if (port.BytesToRead > 0)
                        {
                            result = $"{port.ReadLine()} True";
                            break;
                        }
                        result = "Read Str Null False";
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
                }
                Thread.Sleep(1500);

            }
            return result;


        }
    }
}
