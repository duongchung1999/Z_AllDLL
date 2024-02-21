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

        public string Run(object[] Command)
        {
            string[] cmd = new string[20];
            foreach (var item in Command)
            {
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            return SET1205(PowerComport, cmd[1]).ToString();
        }

        Dictionary<string, object> dic;
        private SerialPort serialPort1 = null;
        string PowerComport;

        /// <summary>
        /// 调节2303电源供给器
        /// </summary>
        /// <param name="comportName">串口名</param>
        /// <param name="commands">指令</param>
        /// <returns></returns>
        private string SET1205(string comportName, string volt)
        {
            bool returnflag = false;
            string volSet = "";
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
                      
                    };
                    serialPort1.WriteLine("V0P");// ngat dau vao/ra
                    serialPort1.WriteLine("V" + command.volt + "M");
                    //serialPort1.WriteLine("VSET" + command.ch + ":" + command.volt);
                    //serialPort1.WriteLine("OUT1");
                    returnflag = true;
                    volSet = String.Concat(Convert.ToString(Convert.ToDouble(volt)/1000), "V");
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
            if (!returnflag) { return false.ToString(); }
            return volSet;
        }

        #region 接口方法
        public string Interface(Dictionary<string, object> keys)
        {
            dic = keys;
            PowerComport = dic["PowerComport"].ToString();
            return "";
        }

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：MB1205C";
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
