using System;
using System.Collections.Generic;

namespace MerryDllFramework
{
    /// <summary dllName="Shizuku_V1">
    /// USB电流表
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        //copy "$(TargetDir)$(ProjectName).dll" "C:\Users\ch200001\source\repos\MerryTest\MerryTest\bin\Debug\AllDLL\$(ProjectName)\$(ProjectName).dll"
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        Dictionary<string, object> Config;
        public object Interface(Dictionary<string, object> Config) => this.Config = Config;

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Shizuku_V1";
            string dllfunction = "Dll功能说明 ：替代8342的仪器";
            string dllVersion = "当前Dll版本：23.10.10.0";
            string dllChangeInfo = "Dll改动信息：";

            string[] info = { dllname, dllfunction,
                dllVersion, dllChangeInfo,
            };
            return info;
        }
        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] cmd);
            switch (cmd[1])
            {
                case "CurrentToA": return CurrentToA();
                case "CurrentTomA": return CurrentTomA();
                case "CurrentTouA": return CurrentTouA();
                case "CurrentToV": return CurrentToV(int.Parse(cmd[2]));
                case "CurrentTomV": return CurrentTomV(int.Parse(cmd[2]));
                case "ReadCurrent": return ReadCurrent(cmd[2], cmd[3], int.Parse(cmd[4]));
                case "ReadVoltage": return ReadVoltage(cmd[2], cmd[3], int.Parse(cmd[4]));

                default: return "Connend Error False";
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

        #region 已经抛弃

        Shizuku_V1.Shizuku sk;

        /// <summary isPublicTestItem="false">
        /// 读取电流 以安为单位
        /// </summary>
        /// <returns>浮点数</returns>
        public string CurrentToA()
            => Current("", 1);
        /// <summary isPublicTestItem="false">
        /// 读取电流 以毫安为单位
        /// </summary>
        /// <returns>浮点数</returns>
        public string CurrentTomA()
            => Current("", 1000);
        /// <summary isPublicTestItem="false">
        /// 读取电流 以微安为单位
        /// </summary>
        /// <returns>浮点数</returns>
        public string CurrentTouA()
            => Current("", 1000 * 1000);
        /// <summary isPublicTestItem="false">
        /// 读取电压 以伏特为单位
        /// </summary>
        /// <param name="Round">保留几位小数 常规“2”</param>
        /// <returns></returns>
        public string CurrentToV(int Round)
            => Voltage("", 1, Round);
        /// <summary isPublicTestItem="false">
        /// 读取电压 以豪伏为单位
        /// </summary>
        /// <param name="Round">保留几位小数 常规“2”</param>
        /// <returns>浮点数</returns>
        public string CurrentTomV(int Round)
            => Voltage("", 1000, Round);
        string Current(string nulber, int Modulation)
        {
            if (sk == null)
                sk = new Shizuku_V1.Shizuku(true);
            string Result = sk.Current(nulber, Modulation);
            if (Result.Contains("False"))
            {
                sk.Dispose();
                sk = null;
                GC.Collect();

            }
            return Result;

        }
        string Voltage(string nulber, int Modulation, int Round)
        {
            if (sk == null)
                sk = new Shizuku_V1.Shizuku(true);
            string Result = sk.Voltage(nulber, Modulation, Round);
            if (Result.Contains("False"))
            {
                sk.Dispose();
                sk = null;
                GC.Collect();

            }
            return Result;

        }
        #endregion

        /// <summary isPublicTestItem="true">
        /// 单扳 连扳 读取电流
        /// </summary>
        /// <param name="Number">单板输入Null 连扳在ShizukuID字段输入 串口内容、事件、资讯 第三段</param>
        /// <param name="unit" options="A,mA,uA">单位</param>
        /// <param name="Round">保留几位小数 常规“2”</param>
        /// <returns></returns>
        public string ReadCurrent(string Number, string unit, int Round)
        {
            int Modulation = 1;
            if (unit == "A")
                Modulation = 1;
            else if (unit == "mA")
                Modulation = 1000;
            else if (unit == "uA")
                Modulation = 1000 * 1000;
            if (OnceConfig.ContainsKey("SN"))
            {
                return ReadValue("Current", OnceConfig["ShizukuID"].ToString(), Modulation, Round);
            }
            else
            {
                Number = Number == "Null" ? "" : Number;
                return ReadValue("Current", Number, Modulation, Round);

            }



        }


        /// <summary isPublicTestItem="true">
        /// 单扳 连扳 读取电压
        /// </summary>
        /// <param name="Number">单板输入Null 连扳在ShizukuID字段输入 串口内容、事件、资讯 第三段</param>
        /// <param name="unit" options="V,mV,uV">单位</param>
        /// <param name="Round">保留几位小数 常规“2”</param>
        /// <returns></returns>
        public string ReadVoltage(string Number, string unit, int Round)
        {
            int Modulation = 1;
            if (unit == "V")
                Modulation = 1;
            else if (unit == "mV")
                Modulation = 1000;
            else if (unit == "uV")
                Modulation = 1000 * 1000;
            if (OnceConfig.ContainsKey("SN"))
            {
                return ReadValue("Voltage", OnceConfig["ShizukuID"].ToString(), Modulation, Round);
            }
            else
            {
                Number = Number == "Null" ? "" : Number;
                return ReadValue("Voltage", Number, Modulation, Round);

            }



        }
        string ReadValue(string Mode,string Number, int Modulation, int Round)
        {
            if (sk == null)
                sk = new Shizuku_V1.Shizuku(true);
            string Result = sk.ReadValue(Mode, Number.ToUpper(), Modulation, Round);
            if (Result.Contains("False"))
            {
                sk.Dispose();
                sk = null;
                GC.Collect();

            }
            return Result;

        }
    }
}
