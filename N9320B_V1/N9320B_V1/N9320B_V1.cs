using Agilent.CommandExpert.ScpiNet.AgSCPI99_1_0;
using N9320B_V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    /// <summary dllName="N9320B_V1">
    /// N9320B频谱仪
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法

        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        Dictionary<string, object> Config;
        AgSCPI99 COMM;
        string _number = "";
        double AddNumber = 0;
        string number
        {
            get
            {
                if (_number == "")
                {
                    foreach (var item in Hardware.AllUsbDevices)
                    {
                        if (!item.Name.Contains("IVI")) continue;
                        if (!item.PNPDeviceID.Contains("FFEF")) continue;
                        _number = item.PNPDeviceID.Substring(22, 10);
                        break;
                    }

                }
                return _number;
            }
            set
            {
                _number = value;
            }
        }
        public object Interface(Dictionary<string, object> Config) => this.Config = Config;

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：N9320B_V1";
            string dllfunction = "Dll功能说明 ：频谱仪测试";
            string dllVersion = "当前Dll版本：23.9.21";
            string dllChangeInfo = "Dll改动信息：";
            string[] info = { dllname, dllfunction,
                dllVersion, dllChangeInfo};


            return info;
        }
        #endregion
        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] cmd);
            bool MoreTest = OnceConfig.ContainsKey("SN");
            switch (cmd[1])
            {
                case "ReadPeak": return ReadPeak(cmd[2], bool.Parse(cmd[3]));
                case "MaxPeak": return MaxPeak(cmd[2], int.Parse(cmd[3]));
                case "ReadFrequency": return ReadFrequency(cmd[2], bool.Parse(cmd[3]));
                case "ReadOffset": return ReadOffset(cmd[2], bool.Parse(cmd[3]));

                case "OptionsOffset": return OptionsOffset(cmd[2], cmd[3], cmd[4], bool.Parse(cmd[5]));
                case "OptionsMaxHoldPeak": return OptionsMaxHoldPeak(cmd[2], cmd[3], cmd[4], Convert.ToInt32(cmd[5]), bool.Parse(cmd[6]));
                default:
                    return $"Command Error {false}";
            }

        }

        void SplitCMD(object[] Command, out string[] CMD)
        {
            List<string> listCMD = new List<string>();
            string TestName = "";
            foreach (var item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>))
                    OnceConfig = (Dictionary<string, object>)item;
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
            double.TryParse(GetValue("N9320B", $"{TestName}"), out AddNumber);
        }


        /// <summary isPublicTestItem="true">
        /// 读取峰值 一般用于读取Power
        /// </summary>
        /// <param name="X_HZ">M/hz 比如 2402</param>
        /// <param name="isInit" options="True,False">是否初始化 常规“False”</param>
        /// <returns></returns>
        public string ReadPeak(string X_HZ, bool isInit)
        {
            return Read(X_HZ, isInit, "Peak");
        }


        /// <summary isPublicTestItem="true">
        /// 可选 读取最大峰值 读取多次取最大值
        /// </summary>
        /// <param name="X_HZ">M/hz 比如 2402</param>
        /// <param name="Count">读取次数 默认 5</param>
        /// <returns></returns>
        public string MaxPeak(string X_HZ, int Count)
        {
            string ResultStr = "Not Connect Device False";
            List<double> Peaks = new List<double>();
            for (int i = 0; i < Count; i++)
            {
                ResultStr = Read(X_HZ, i == 0, "Peak");
                if (!double.TryParse(ResultStr, out double Result))
                    return ResultStr;
                Peaks.Add(Result);
            }
            if (Peaks.Count > 0) return Peaks.Max().ToString();
            return ResultStr;
        }


        /// <summary isPublicTestItem="true">
        /// 读取频率 这不是频偏
        /// </summary>
        /// <param name="X_HZ">M/hz 比如 2402</param>
        /// <param name="isInit" options="True,False">是否初始化 常规“False”</param>
        /// <returns></returns>
        public string ReadFrequency(string X_HZ, bool isInit)
        {
            return Read(X_HZ, isInit, "Frequency");
        }


        /// <summary isPublicTestItem="true">
        /// 读取频偏
        /// </summary>
        /// <param name="X_HZ">M/hz 比如 2402</param>
        /// <param name="isInit" options="True,False">是否初始化 常规“False”</param>
        /// <returns></returns>
        public string ReadOffset(string X_HZ, bool isInit)
        {
            return Read(X_HZ, isInit, "Offset");
        }


        /// <summary isPublicTestItem="true">
        /// 可选 读取频偏
        /// </summary>
        /// <param name="X_HZ">M/hz 比如 2402</param>
        /// <param name="SPAN">SPAN 跨距 常规“2.5e6”</param>
        /// <param name="RBW">RBW “30e3”</param>
        /// <param name="isInit" options="True,False">是否初始化 常规“False”</param>
        /// <returns></returns>
        public string OptionsOffset(string X_HZ, string SPAN, string RBW, bool isInit)
        {
            string[] cmds = new string[] {
            $"SENS:FREQ:SPAN {SPAN}",
            $"Sense:Band:Res {RBW}"};
            return Read(X_HZ, isInit, "Offset", 200, cmds);
        }



        //

        /// <summary isPublicTestItem="true">
        /// 特殊 读取峰值 方法MAX Hold
        /// </summary>
        /// <param name="X_HZ">M/hz 比如 2402</param>
        /// <param name="SPAN">SPAN 跨距 常规“2.5e6”</param>
        /// <param name="RBW">RBW 常规 “30e3”</param>
        /// <param name="SleepTime">Max Hold后等待时间 /毫秒 常规“2000” </param>
        /// <param name="isInit" options="True,False">是否初始化 常规“False”</param>
        /// <returns></returns>
        public string OptionsMaxHoldPeak(string X_HZ, string SPAN, string RBW, int SleepTime, bool isInit)
        {
            string[] cmds = new string[] {
            $"SENS:FREQ:SPAN {SPAN}",
            $"Sense:Band:Res {RBW}",
            $"CAL:SOUR:STAT ON",
            $"INIT:CONT 1",
            ":TRACe1:MODE MAXHold",
             };
            return MaxHoldRead(X_HZ, isInit, "Peak", SleepTime, cmds);
        }

        string Read(string X_HZ, bool isInit, string Morde, int Sleep = 200, string[] Commands = null)
        {
            string Result = "";
            double Value = 0;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (COMM == null)
                    {
                        COMM = new AgSCPI99("USB0::0x0957::0xFFEF::" + number + "::0::INSTR");
                        COMM.SCPI.WAI.Command();
                        COMM.SCPI.RST.Command();
                        Thread.Sleep(100);
                        COMM.SCPI.WAI.Command();

                    }
                    else if (isInit)
                    {
                        COMM.SCPI.RST.Command();
                        COMM.SCPI.WAI.Command();
                        Thread.Sleep(100);
                    }

                    COMM.Transport.Command.Invoke("UNIT:POW DBM");
                    COMM.Transport.Command.Invoke("SENS:FREQ:CENT " + X_HZ + "e6");
                    COMM.Transport.Command.Invoke("SENS:FREQ:SPAN 2.5e6");
                    COMM.SCPI.WAI.Command();
                    if (Commands != null)
                    {
                        foreach (var item in Commands)
                            COMM.Transport.Command.Invoke(item);
                        COMM.SCPI.WAI.Command();
                        Thread.Sleep(250);
                    }
                    Thread.Sleep(Sleep);
                    //COMM.Transport.Query.Invoke("*IDN?", out string ReturnValue);
                    COMM.Transport.Command.Invoke("CAL:SOUR:STAT ON");
                    COMM.Transport.Command.Invoke("INIT:CONT 0");
                    COMM.Transport.Command.Invoke("INIT:IMM");
                    COMM.SCPI.WAI.Command();
                    COMM.Transport.Command.Invoke("CALC:MARK:MAX");
                    COMM.SCPI.WAI.Command();
                    string MARK_X = double.MinValue.ToString();
                    switch (Morde)
                    {

                        case "Offset":
                            COMM.Transport.Query.Invoke("CALC:MARK:X?", out MARK_X);
                            double diff = double.Parse(MARK_X) - (double.Parse(X_HZ) * 1000 * 1000);
                            Value = Math.Round(diff / 1000, 2);
                            break;
                        case "Frequency":
                            COMM.Transport.Query.Invoke("CALC:MARK:X?", out MARK_X);
                            Value = Math.Round(double.Parse(MARK_X), 2);
                            break;
                        case "Peak":
                        default:
                            COMM.Transport.Query.Invoke("CALC:MARK:Y?", out string MARK_Y);
                            Value = Math.Round(double.Parse(MARK_Y), 2);
                            break;
                    }

                    Result = (Value + AddNumber).ToString();
                    return Result;
                }
                catch (Exception ex)
                {
                    COMM?.Disconnect();
                    COMM = null;
                    number = "";
                    Result = $"{ex.Message} False";
                    Thread.Sleep(750);

                }
                Thread.Sleep(250);
            }
            return Result;
        }
        string MaxHoldRead(string X_HZ, bool isInit, string Morde, int Sleep = 200, string[] Commands = null)
        {
            string Result = "";
            double Value = 0;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (COMM == null)
                    {
                        COMM = new AgSCPI99("USB0::0x0957::0xFFEF::" + number + "::0::INSTR");
                        COMM.SCPI.RST.Command();
                        COMM.SCPI.WAI.Command();
                    }
                    else if (isInit)
                    {
                        COMM.SCPI.RST.Command();
                        COMM.SCPI.WAI.Command();
                        Thread.Sleep(100);
                    }
                    COMM.Transport.Command.Invoke("UNIT:POW DBM");
                    COMM.Transport.Command.Invoke("SENS:FREQ:CENT " + X_HZ + "e6");
                    COMM.SCPI.WAI.Command();
                    if (Commands != null)
                    {
                        foreach (var item in Commands)
                            COMM.Transport.Command.Invoke(item);
                        COMM.SCPI.WAI.Command();
                        Thread.Sleep(250);
                    }
                    //COMM.Transport.Query.Invoke("*IDN?", out string ReturnValue);
                    //COMM.Transport.Command.Invoke("CAL:SOUR:STAT ON");
                    //COMM.Transport.Command.Invoke("INIT:CONT 0");
                    //COMM.Transport.Command.Invoke("INIT:IMM");
                    COMM.SCPI.WAI.Command();
                    Thread.Sleep(Sleep);
                    COMM.Transport.Command.Invoke("CALC:MARK:MAX");
                    COMM.SCPI.WAI.Command();
                    string MARK_X = double.MinValue.ToString();
                    switch (Morde)
                    {

                        case "Offset":
                            COMM.Transport.Query.Invoke("CALC:MARK:X?", out MARK_X);
                            double diff = double.Parse(MARK_X) - (double.Parse(X_HZ) * 1000 * 1000);
                            Value = Math.Round(diff / 1000, 2);
                            break;
                        case "Frequency":
                            COMM.Transport.Query.Invoke("CALC:MARK:X?", out MARK_X);
                            Value = Math.Round(double.Parse(MARK_X), 2);
                            break;
                        case "Peak":
                        default:
                            COMM.Transport.Query.Invoke("CALC:MARK:Y?", out string MARK_Y);
                            Value = Math.Round(double.Parse(MARK_Y), 2);
                            break;
                    }

                    Result = (Value + AddNumber).ToString();
                    return Result;
                }
                catch (Exception ex)
                {
                    COMM?.Disconnect();
                    COMM = null;
                    number = "";
                    Result = $"{ex.Message} False";
                    Thread.Sleep(750);

                }
                Thread.Sleep(250);
            }
            return Result;
        }








        string _path = ".\\AllDLL\\MenuStrip\\AddDeploy.ini";
        [DllImport("kernel32.dll")]
        static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVar, int size, string INIpath);
        string GetValue(string section, string key)
        {
            try
            {
                StringBuilder var = new StringBuilder(512);
                GetPrivateProfileString(section, key, "null", var, 512, _path);
                return var.ToString().Trim();
            }
            catch
            {
                return "0";
            }

        }

    }

}
