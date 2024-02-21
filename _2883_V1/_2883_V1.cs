using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    /// <summary dllName="_2883_V1">
    /// USB 2883 电压采集器
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        Dictionary<string, object> keys;
        #region 程序接口
        //主程序所使用的变量
        public object Interface(Dictionary<string, object> keys) => this.keys = keys;
        double AddNumber = 0;
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：USB 2883";
            string dllfunction = "Dll功能说明 ：SUB2883功能模块";
            string dllHistoryVersion = "历史Dll版本：";
            string dllVersion = "当前Dll版本：23.11.1.2";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.8.10.0：开发共享到后台，同时获取正玄波有效值";
            string dllChangeInfo2 = "23.11.2.0：开发取电流值";

            string[] info = { dllname,
                dllfunction,
                dllHistoryVersion,
                dllVersion,
                dllChangeInfo, dllChangeInfo1
            };
            return info;
        }
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();

        Dictionary<int, StartUSB2883> _2883Dic = new Dictionary<int, StartUSB2883>();
        #endregion
        public string Run(object[] Command)
        {
            SplitCMD(Command, out int DeviceID, out string[] cmd);
            switch (cmd[1])
            {
                case "StartUSB2883":
                    return StartUSB2883(DeviceID, int.Parse(cmd[3]));
                case "GetSqrtValue":
                    return GetSqrtValue(DeviceID, int.Parse(cmd[3]), cmd[4]);
                case "GetAvgValue":
                    return GetAvgValue(DeviceID, int.Parse(cmd[3]), cmd[4]);
                case "GetAvgCurrent":
                    return GetAvgCurrent(DeviceID, int.Parse(cmd[3]), cmd[4], double.Parse(cmd[5]));
                default: return "Command Error False";
            }
        }
        void SplitCMD(object[] Command, out int DeviceID, out string[] CMD)
        {
            List<string> listCMD = new List<string>();
            string TestName = "";
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
            try
            {
                if (OnceConfig.ContainsKey("SN"))
                {
                    DeviceID = int.Parse(OnceConfig[CMD[2]].ToString());
                    string TestID = OnceConfig["TestID"].ToString();
                    double.TryParse(WindowsAPI.GetValue($"{TestID}#_2883", TestName, ".\\AllDLL\\MenuStrip\\MoreAddDeploy.ini"), out AddNumber);

                }
                else
                {
                    DeviceID = int.Parse(CMD[2]);
                    double.TryParse(WindowsAPI.GetValue("_2883", $"{TestName}", ".\\AllDLL\\MenuStrip\\AddDeploy.ini"),  out AddNumber);
                }
            }
            catch (Exception ex)
            {
                DeviceID = -1;
                MessageBox.Show($"ID转换失败\r\nID:{CMD[2]}\r\n" + ex.ToString());
            }




        }

        /// <summary isPublicTestItem="true">
        /// USB 2883 开始测试
        /// </summary>
        /// <param name="DeviceID">单板设设备ID 如“200” 连扳设配置的字段如“2883ID_A”</param>
        /// <param name="ReadCount">读取次数 400组大约2.5秒</param>
        /// <returns></returns>
        public string StartUSB2883(int DeviceID, int ReadCount)
        {
            if (!_2883Dic.ContainsKey(DeviceID))
                _2883Dic[DeviceID] = new StartUSB2883();
            return _2883Dic[DeviceID].Start(DeviceID, ReadCount);

        }
        /// <summary isPublicTestItem="true">
        /// 取值 正弦波有效值
        /// </summary>
        /// <param name="DeviceID">单板设设备ID 如“200” 连扳设配置的字段如“2883ID_A”</param>
        /// <param name="Channel">读取的通道</param>
        /// <param name="Unit" options="V,mV">取值单位</param>
        /// 
        /// <returns></returns>
        public string GetSqrtValue(int DeviceID, int Channel, string Unit)
        {
            if (!_2883Dic.ContainsKey(DeviceID))
                return "Not Test False";
            double sqrt = 1 / (Math.Sqrt(2));
            double Max = _2883Dic[DeviceID].Allvoltage[Channel].Max();
            double Min = _2883Dic[DeviceID].Allvoltage[Channel].Min();
            double value = (Max + Min) / 2 * sqrt;
            if (Max <= 0 && Min <= 0 || Max >= 0 && Min >= 0)
            {
                List<double> absList = new List<double>()
                {
                      Math.Abs(Max),
                      Math.Abs(Min)
                };
                double diff = absList.Max() - absList.Min();
                value = (diff / 2) * sqrt;
            }
            else
            {
                value = (Math.Abs(Max) + Math.Abs(Min)) / 2 * sqrt;
            }
            if (Unit == "V")
            {
                value /= 1000;
            }
            else if (Unit == "mV")
            {
            }
            return Math.Round(value + AddNumber, 2).ToString();
        }

        /// <summary isPublicTestItem="true">
        /// 取值 取电压平均值
        /// </summary>
        /// <param name="DeviceID">单板设设备ID 如“200” 连扳设配置的字段如“2883ID_A”</param>
        /// <param name="Channel">读取的通道</param>
        /// <param name="Unit" options="V,mV">取值单位</param>
        /// 
        /// <returns></returns>
        public string GetAvgValue(int DeviceID, int Channel, string Unit)
        {
            if (!_2883Dic.ContainsKey(DeviceID))
                return "Not Test False";
            double sqrt = 1 / (Math.Sqrt(2));
            double Avg = _2883Dic[DeviceID].Allvoltage[Channel].Average();
            if (Unit == "V")
            {
                Avg /= 1000;
            }
            else if (Unit == "mV")
            {
            }
            return Math.Round(Avg+AddNumber, 2).ToString();
        }
        /// <summary isPublicTestItem="true">
        /// 取值 取电流平均值
        /// </summary>
        /// <param name="DeviceID">单板设设备ID 如“200” 连扳设配置的字段如“2883ID_A”</param>
        /// <param name="Channel">读取的通道</param>
        /// <param name="Unit" options="A,mA,uA">取值单位</param>
        /// <param name="resistance">电阻 mA输入500 uA输入 200</param>
        /// <returns></returns>
        public string GetAvgCurrent(int DeviceID, int Channel, string Unit, double resistance)
        {
            //   "Compute" 设定计算公式 mA是输入“Avg / 1000 / 500 * 1000” 
            //string formula = "20*(30-10)";
            //var result = new System.Data.DataTable().Compute(formula, "");
            //Console.WriteLine(result);
            if (!_2883Dic.ContainsKey(DeviceID))
                return "Not Test False";
            double Avg = _2883Dic[DeviceID].Allvoltage[Channel].Average();

            switch (Unit)
            {
                case "mA":
                    double mA = Avg / 1000 / resistance * 1000;
                    return Math.Round(mA + AddNumber, 2).ToString();
                case "uA":   //  mV 变 V  除以100被放大  除200电阻等于A     ✖2个1000等于 uA
                    double uA = Avg / 1000 / resistance / 200 * 1000 * 1000;
                    return Math.Round(uA + AddNumber, 2).ToString();
                case "A":


                default:
                    return "这个功能还没写 False";
            }

        }
    }
    class WindowsAPI
    {
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVar, int size, string INIpath);
        public static string GetValue(string section, string key, string _path = ".\\AllDLL\\MenuStrip\\AddDeploy.ini")
        {
            try
            {
                StringBuilder var = new StringBuilder(512);
                GetPrivateProfileString(section, key, "", var, 512, _path);
                return var.ToString().Trim();
            }
            catch
            {
                return "0";
            }

        }
    }

}
