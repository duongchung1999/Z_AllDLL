using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {

        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();

        public string Run(object[] Command)
        {

            string[] cmd = new string[20];
            foreach (var item in Command)
            {
                if (item.GetType().ToString() != "System.String") continue;
                if (item.GetType() == typeof(Dictionary<string, object>)) OnceConfig = (Dictionary<string, object>)item;

                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            bool MoreTest = OnceConfig.ContainsKey("SN");

            return SET2303s(MoreTest?(string) OnceConfig["_2303SComport"] : (string)Config["_2303SComport"], cmd[1], cmd[2], cmd[3]).ToString();
        }

        Dictionary<string, object> Config;
        private SerialPort serialPort1 = null;
        string _2303SComport;

        /// <summary>
        /// 调节2303电源供给器
        /// </summary>
        /// <param name="comportName">串口名</param>
        /// <param name="commands">指令</param>
        /// <returns></returns>
        private bool SET2303s(string comportName, string volt, string current, string channel)
        {
            bool returnflag = false;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (serialPort1 == null)
                    {

                        serialPort1 = new SerialPort();
                        serialPort1.PortName = comportName;
                        serialPort1.BaudRate = 9600;
                        Thread.Sleep(50);
                        if (!serialPort1.IsOpen) serialPort1.Open();
                    }

                    var command = new
                    {
                        volt = volt,
                        current = current,
                        ch = channel
                    };
                    serialPort1.WriteLine("OUT0");
                    serialPort1.WriteLine("ISET" + command.ch + ":" + command.current);
                    serialPort1.WriteLine("VSET" + command.ch + ":" + command.volt);
                    serialPort1.WriteLine("OUT1");
                    returnflag = true;
                }
                catch (Exception ex)
                {
                    returnflag = false;
                    if (serialPort1.IsOpen) serialPort1.Close();
                    serialPort1.Dispose();
                    serialPort1 = null;
                }
                if (returnflag) break;
                Thread.Sleep(500);
            }
            return returnflag;
        }

        #region 接口方法
        public object Interface(Dictionary<string, object> config) => Config = config;



        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：_2303";
            string dllfunction = "Dll功能说明 ：电源供给器控制";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllVersion = "当前Dll版本：0.0.1.0";
            string dllChangeInfo = "Dll改动信息：";
            string[] info = { dllname, dllfunction, dllHistoryVersion, dllVersion, dllChangeInfo };

            return info;
        }
        #endregion

    }

}
