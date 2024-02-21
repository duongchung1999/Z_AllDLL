using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    /// <summary dllName="_8342_V1">
    /// _8342_V1 电流表
    /// </summary>
    public class MerryDll : IMerryAllDll
    {

        #region 接口方法

        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        Dictionary<string, object> Config;
        _8342 port = new _8342();
        string _8342Comport = "";
        string oldRange = "";
        int IniType = 0;
        int oldCurrent = -1;
        public object Interface(Dictionary<string, object> Config) => this.Config = Config;

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：_8342_V1";
            string dllfunction = "Dll功能说明 ：弹出窗体，串口调试";
            string dllVersion = "当前Dll版本：23.10.6.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "22.9.28.0：第一版";
            string dllChangeInfo2 = "23.6.12.0: 目前可以自动获取8342串口、可以在本地设定";
            string dllChangeInfo4 = "23.10.6.0: 可设定保存几位小数";


            string[] info = { dllname, dllfunction,
                dllVersion,
                dllChangeInfo,
                dllChangeInfo1,
            };


            return info;
        }
        #endregion

        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] cmd);
            bool MoreTest = OnceConfig.ContainsKey("SN");
            switch (cmd[1])
            {

                case "A_DC":
                    return A_DC(cmd[2], int.Parse(cmd[3]));
                case "A_AC":
                    return A_AC(cmd[2], int.Parse(cmd[3]));
                case "V_DC":
                    return V_DC(cmd[2], int.Parse(cmd[3]));
                case "V_AC":
                    return V_AC(cmd[2], int.Parse(cmd[3]));
                case "RES":
                    return RES(cmd[2], int.Parse(cmd[3]));
                case "Hz_P":
                    return Hz_P(cmd[2], int.Parse(cmd[3]));
                case "DIODe":
                    return DIODe(int.Parse(cmd[2]));


                case "_A_DC":return _A_DC(cmd[2], int.Parse(cmd[3]), int.Parse(cmd[4]));
                case "_A_AC": return _A_AC(cmd[2], int.Parse(cmd[3]), int.Parse(cmd[4]));
                case "_V_DC": return _V_DC(cmd[2], int.Parse(cmd[3]), int.Parse(cmd[4]));
                case "_V_AC": return _V_AC(cmd[2], int.Parse(cmd[3]), int.Parse(cmd[4]));

                default:
                    return "Command Error False";
            }

        }

        void SplitCMD(object[] Command, out string[] CMD)
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
            CMD = listCMD.ToArray();
        }
        string GetComPort()
        {
            if (_8342Comport == "")
            {
                _8342Comport = Config.ContainsKey("_8342Comport") ? Config["_8342Comport"].ToString() : "COM1";
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
                {
                    var portnames = SerialPort.GetPortNames();
                    var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
                    var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();
                    foreach (string s in portList)
                    {
                        if (s.Contains("834X"))
                        {
                            _8342Comport = s.Substring(0, s.IndexOf(" "));
                        }

                    }
                }
            }


            return _8342Comport;
        }

        /// <summary isPublicTestItem="true">
        /// 电流直流电 A_DC
        /// </summary>
        /// <param name="NewRange" options="5,0.5,0.0005">量程</param>
        /// <param name="Count">读取次数 取平均值 常规 “1”</param>
        /// <returns>浮点数或报错信息</returns>
        public string A_DC(string NewRange, int Count)
            => CurrentTests(NewRange, Count, 1);

        /// <summary isPublicTestItem="true">
        /// 电流交流电 A_AC
        /// </summary>
        /// <param name="NewRange" options="5,0.5,0.0005">量程</param>
        /// <param name="Count">读取次数 取平均值 常规 “1”</param>
        /// <returns>浮点数或报错信息</returns>
        public string A_AC(string NewRange, int Count)
            => CurrentTests(NewRange, Count, 2);

        /// <summary isPublicTestItem="true">
        /// 电压直流电 V_DC
        /// </summary>
        /// <param name="NewRange" options="5,0.5,0.0005">量程</param>
        /// <param name="Count">读取次数 取平均值 常规 “1”</param>
        /// <returns>浮点数或报错信息</returns>
        public string V_DC(string NewRange, int Count)
            => CurrentTests(NewRange, Count, 3);

        /// <summary isPublicTestItem="true">
        /// 电压交流电 V_AC
        /// </summary>
        /// <param name="NewRange" options="5,0.5,0.0005">量程</param>
        /// <param name="Count">读取次数 取平均值 常规 “1”</param>
        /// <returns>浮点数或报错信息</returns>
        public string V_AC(string NewRange, int Count)
            => CurrentTests(NewRange, Count, 4);

        /// <summary isPublicTestItem="true">
        /// 电阻 RES
        /// </summary>
        /// <param name="NewRange" options="5,0.5,0.0005">量程</param>
        /// <param name="Count">读取次数 取平均值 常规 “1”</param>
        /// <returns>浮点数或报错信息</returns>
        public string RES(string NewRange, int Count)
            => CurrentTests(NewRange, Count, 5);

        /// <summary isPublicTestItem="true">
        /// 频率 Hz_P
        /// </summary>
        /// <param name="NewRange" options="5,0.5,0.0005">量程</param>
        /// <param name="Count">读取次数 取平均值 常规 “1”</param>
        /// <returns>浮点数或报错信息</returns>
        public string Hz_P(string NewRange, int Count)
            => CurrentTests(NewRange, Count, 6);

        /// <summary isPublicTestItem="true">
        /// 二极管测量 Diode
        /// </summary>
        /// <param name="Conut">读取次数 取平均值 常规 “1”</param>
        /// <returns></returns>
        public string DIODe(int Conut)
            => CurrentTests("xxx", Conut, 7);

        string CurrentTests(string NewRange, int Count, int Current)
        {
            port.PortName = OnceConfig.ContainsKey("_8342Comport") ? (string)OnceConfig["_8342Comport"] : GetComPort();
            string Result = "Note Test False";
            double Value = 0;
            double avgValue = 0;
            int avgCount = 0;
            bool ReadFlag = false;
            if ((oldRange != NewRange) || (Current != oldCurrent))
            {
                string cmd = "";
                switch (Current)
                {
                    case 1: cmd = ($":CONF:CURR:DC {NewRange}\r\n"); break;
                    case 2: cmd = ($":CONF:CURR:AC {NewRange}\r\n"); break;
                    case 3: cmd = ($":CONF:VOLT:DC {NewRange}\r\n"); break;
                    case 4: cmd = ($":CONF:VOLT:AC {NewRange}\r\n"); break;
                    case 5: cmd = ($":CONF:RES {NewRange}\r\n"); break;
                    case 6: cmd = ($":CONF:FREQ {NewRange}\r\n"); break;
                    case 7: cmd = ($"CONFigure:DIODe\r\n"); break;
                    default:
                        MessageBox.Show("指令错了");
                        break;
                }
                Result = port.WriteLineText(cmd);
                if (Result.Contains("False"))
                    return Result;

                oldRange = NewRange;
                oldCurrent = Current;
                Thread.Sleep(2000);
            }

            for (int C = 0; C < Count; C++)
            {
                ReadFlag = false;
                for (int i = 0; i < 4; i++)
                {

                    port.WriteLineText(":VAL1?");
                    Thread.Sleep(250);
                    Result = port.ReadLine();
                    if (Result.Contains("False"))
                        return Result;
                    if (double.TryParse(Result, out Value))
                    {
                        ReadFlag = true;
                        break;
                    }

                }
                if (ReadFlag)
                {
                    avgCount++;
                    avgValue += Value;
                }
                Thread.Sleep(50);
            }
            Value = avgValue / avgCount;


            switch (NewRange)
            {

                case "5":
                    if (Current == 1)
                        Value *= 1000;
                    break;

                case "0.5":
                    Value *= 1000;
                    break;

                case "0.0005":
                    if (Current <= 2) Value = Value * 1000 * 1000;
                    if (Current >= 3) Value *= 1000;
                    break;
            }

            Result = Value.ToString("f2");
            return Result;
        }





        /// <summary isPublicTestItem="true">
        /// 进阶 电流直流电 A_DC Rount
        /// </summary>
        /// <param name="NewRange" options="5,0.5,0.0005">量程</param>
        /// <param name="Count">读取次数 取平均值 常规 “1”</param>
        /// <param name="Rount">四舍五入法 保留几位小数 常规“2”</param>
        /// <returns>浮点数或报错信息</returns>
        public string _A_DC(string NewRange, int Count, int Rount)
            => _CurrentTests(NewRange, Count, 1, Rount);

        /// <summary isPublicTestItem="true">
        /// 进阶 电流交流电 A_AC Rount
        /// </summary>
        /// <param name="NewRange" options="5,0.5,0.0005">量程</param>
        /// <param name="Count">读取次数 取平均值 常规 “1”</param>
        /// <param name="Rount">四舍五入法 保留几位小数 常规“2”</param>
        /// <returns>浮点数或报错信息</returns>
        public string _A_AC(string NewRange, int Count, int Rount)
            => _CurrentTests(NewRange, Count, 2, Rount);

        /// <summary isPublicTestItem="true">
        /// 进阶 电压直流电 V_DC Rount
        /// </summary>
        /// <param name="NewRange" options="5,0.5,0.0005">量程</param>
        /// <param name="Count">读取次数 取平均值 常规 “1”</param>
        /// <param name="Rount">四舍五入法 保留几位小数 常规“2”</param>
        /// <returns>浮点数或报错信息</returns>
        public string _V_DC(string NewRange, int Count, int Rount)
            => _CurrentTests(NewRange, Count, 3, Rount);

        /// <summary isPublicTestItem="true">
        ///  进阶 电压交流电 V_AC Rount
        /// </summary>
        /// <param name="NewRange" options="5,0.5,0.0005">量程</param>
        /// <param name="Count">读取次数 取平均值 常规 “1”</param>
        /// <param name="Rount">四舍五入法 保留几位小数 常规“2”</param>
        /// <returns>浮点数或报错信息</returns>
        public string _V_AC(string NewRange, int Count, int Rount)
            => _CurrentTests(NewRange, Count, 4, Rount);







        string _CurrentTests(string NewRange, int Count, int Current, int Round)
        {
            port.PortName = OnceConfig.ContainsKey("_8342Comport") ? (string)OnceConfig["_8342Comport"] : GetComPort();
            string Result = "Note Test False";
            double Value = 0;
            double avgValue = 0;
            int avgCount = 0;
            bool ReadFlag = false;
            if ((oldRange != NewRange) || (Current != oldCurrent))
            {
                string cmd = "";
                switch (Current)
                {
                    case 1: cmd = ($":CONF:CURR:DC {NewRange}\r\n"); break;
                    case 2: cmd = ($":CONF:CURR:AC {NewRange}\r\n"); break;
                    case 3: cmd = ($":CONF:VOLT:DC {NewRange}\r\n"); break;
                    case 4: cmd = ($":CONF:VOLT:AC {NewRange}\r\n"); break;
                    case 5: cmd = ($":CONF:RES {NewRange}\r\n"); break;
                    case 6: cmd = ($":CONF:FREQ {NewRange}\r\n"); break;
                    case 7: cmd = ($"CONFigure:DIODe\r\n"); break;
                    default:
                        MessageBox.Show("指令错了");
                        break;
                }
                Result = port.WriteLineText(cmd);
                if (Result.Contains("False"))
                    return Result;

                oldRange = NewRange;
                oldCurrent = Current;
                Thread.Sleep(2000);
            }

            for (int C = 0; C < Count; C++)
            {
                ReadFlag = false;
                for (int i = 0; i < 4; i++)
                {

                    port.WriteLineText(":VAL1?");
                    Thread.Sleep(250);
                    Result = port.ReadLine();
                    if (Result.Contains("False"))
                        return Result;
                    if (double.TryParse(Result, out Value))
                    {
                        ReadFlag = true;
                        break;
                    }

                }
                if (ReadFlag)
                {
                    avgCount++;
                    avgValue += Value;
                }
                Thread.Sleep(50);
            }
            Value = avgValue / avgCount;


            switch (NewRange)
            {

                case "5":
                    if (Current == 1)
                        Value *= 1000;
                    break;

                case "0.5":
                    Value *= 1000;
                    break;

                case "0.0005":
                    if (Current <= 2) Value = Value * 1000 * 1000;
                    if (Current >= 3) Value *= 1000;
                    break;
            }

            Result = Math.Round(Value, Round).ToString();
            return Result;
        }



    }
}
