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
            int type = 1;
            switch (cmd[1])
            {

                case "A_DC": type = 1; break;
                case "A_AC": type = 2; break;
                case "V_DC": type = 3; break;
                case "V_AC": type = 4; break;
                case "RES": type = 5; break;
                default:
                    break;
            }

            return ReadValue_CDC(_8342Comport, cmd[2], type);
        }

        Dictionary<string, object> dic;
        private SerialPort serialPort1 = null;
        string _8342Comport;

        public string Interface(Dictionary<string, object> keys)
        {
            dic = keys;
            _8342Comport = dic["_8342Comport"].ToString();
            return "";
        }

        #region 8342电流表
        /// <summary>
        /// 读取电流表的值
        /// </summary>
        /// <param name="port">8342串口</param>
        /// <returns></returns>
        private string ReadValue(string comportName, string range, int type)
        {

            if (CurrentInit_CDC(comportName, range, type))
            {
                serialPort1.WriteLine(":VAL1?");
                Thread.Sleep(200);

                var buffer = new byte[serialPort1.BytesToRead];
                var data = serialPort1.Read(buffer, 0, serialPort1.BytesToRead);
                var datavalue = ASCIIEncoding.ASCII.GetString(buffer);

                switch (range)
                {
                    case "5":
                        datavalue = datavalue;
                        break;
                    case "0.5":
                        datavalue = Convert.ToString(Convert.ToDouble(datavalue) * 1000);
                        break;
                    case "0.0005":
                        datavalue = Convert.ToString(Convert.ToDouble(datavalue) * 1000 * 1000);
                        break;
                }

                return datavalue;
            }
            return "READ ERR";

            

        }

        private string ReadValue_CDC(string comportName, string range, int type)
        {
            bool returnflag = false;
            string currentValue = "";
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (serialPort1 == null)
                    {

                        serialPort1 = new SerialPort();
                        serialPort1.PortName = comportName;
                        serialPort1.BaudRate = 9600;
                        serialPort1.Parity = Parity.None;
                        serialPort1.DataBits = 8;
                        serialPort1.StopBits = StopBits.One;

                        Thread.Sleep(50);
                        if (!serialPort1.IsOpen) serialPort1.Open();
                    }

                    var command = new
                    {
                        rangeSet = range,
                        typeSet = type,
                    };

                    switch (command.typeSet)
                    {
                        case 1: serialPort1.WriteLine($":CONF:CURR:DC {command.rangeSet}"); break;
                        case 2: serialPort1.WriteLine($":CONF:CURR:AC {command.rangeSet}"); break;
                        case 3: serialPort1.WriteLine($":CONF:VOLT:DC {command.rangeSet}"); break;
                        case 4: serialPort1.WriteLine($":CONF:VOLT:AC {command.rangeSet}"); break;
                        case 5: serialPort1.WriteLine($":CONF:RES {command.rangeSet}"); break;
                    }

                    Thread.Sleep(500);

                    serialPort1.WriteLine("VAL1?");

                    Thread.Sleep(200);

                    var buffer = new byte[serialPort1.BytesToRead];
                    var data = serialPort1.Read(buffer, 0, serialPort1.BytesToRead);
                    var datavalue = ASCIIEncoding.ASCII.GetString(buffer);

                    switch (command.rangeSet)
                    {
                        case "10":
                            datavalue = Convert.ToString(Convert.ToDouble(datavalue) * 1);
                            break;
                        case "5":
                            datavalue = Convert.ToString(Convert.ToDouble(datavalue) * 1);
                            break;
                        case "0.5":
                            datavalue = Convert.ToString(Convert.ToDouble(datavalue) * 1000);
                            break;
                        case "0.0005":
                            datavalue = Convert.ToString(Convert.ToDouble(datavalue) * 1000 * 1000);
                            break;
                    }

                    returnflag = true;

                    currentValue = Convert.ToString(datavalue);
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
            return currentValue;

        }

        /// <summary>
        /// 初始化电流表
        /// </summary>
        /// <param name="sPortName">串口名</param>
        /// <param name="range">档位</param>
        /// <param name="Type">类别</param>
        /// <returns></returns>

        private bool CurrentInit_CDC(string comportName, string range, int Type)
        {
            try
            {
                serialPort1.PortName = comportName;
                serialPort1.BaudRate = 9600;
                serialPort1.Parity = Parity.None;
                serialPort1.DataBits = 8;
                serialPort1.StopBits = StopBits.One;
                //serialPort1.ReadTimeout = 500;

                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                }
                serialPort1.Open();

                serialPort1.WriteLine("*IDN?");
                Thread.Sleep(100);
                switch (Type)
                {
                    case 1: serialPort1.Write(":CONF:CURR:DC " + range + "\r\n"); break;
                    case 2: serialPort1.Write(":CONF:CURR:AC " + range + "\r\n"); break;
                    case 3: serialPort1.Write(":CONF:VOLT:DC " + range + "\r\n"); break;
                    case 4: serialPort1.Write(":CONF:VOLT:AC " + range + "\r\n"); break;
                    case 5: serialPort1.Write($":CONF:RES {range}\r\n"); break;
                }


            }
            catch
            {

                if (serialPort1.IsOpen) serialPort1.Close();
                serialPort1.Dispose();
                return false;
            }
            return true;
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
 

        #endregion

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：_8342";
            string dllfunction = "Dll功能说明 ：电流表控制";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllHistoryVersion1 = "                     ：21.8.14.0";
            string dllHistoryVersion2 = "                     ：21.8.26.0";
            string dllVersion = "当前Dll版本：21.8.26.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "21.8.14.0：2021/8/14：修复电压测试值返回错误问题";
            string dllChangeInfo2 = "21.8.26.0：2021/8/26：增加电阻返回值";
            string[] info = { dllname, dllfunction, dllHistoryVersion,dllHistoryVersion1,dllHistoryVersion2,
                dllVersion, dllChangeInfo,dllChangeInfo1,dllChangeInfo2
            };

            return info;
        }



    }
}