using CommonUtil;
using Ivi.Visa;
using lvi_Visa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VISAInstrument.Port;


namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：RT550";
            string dllfunction = "Dll功能说明 ：RT550仪器功能模块";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllHistoryVersion1 = "历史Dll版本：21.8.14.0";
            string dllHistoryVersion2 = "                     ：21.11.18";
            string dllVersion = "当前Dll版本：22.8.15.0";

            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "21.11.18：2021/11/18：优化测试速度及功能";
            string dllChangeInfo2 = "22.5.17.1： 更新建立连接为2次，每次10秒超时,增加读取CW";
            string dllChangeInfo3 = "22.8.15.0： DUT开始测试增加测试时长";


            string[] info = { dllname, dllfunction, dllHistoryVersion,dllHistoryVersion1,
                dllHistoryVersion2,
                dllVersion,
                dllChangeInfo,dllChangeInfo1,dllChangeInfo2,dllChangeInfo3
            };
            return info;
        }
        static VISA visa = new VISA();
        static Dictionary<string, object> Config;

        public object Interface(Dictionary<string, object> keys) => VISA.Config = Config = keys;

        public string Run(object[] Command)
        {
            List<string> cmd = null;
            string TestName = "";
            foreach (var item in Command)
            {
                Type type = item.GetType();
                if (type.ToString().Contains("TestitemEntity"))
                {
                    PropertyInfo property = type.GetProperty("测试项目");
                    TestName = property.GetValue(item, null).ToString();
                }
                if (type != typeof(string)) continue;
                cmd = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < cmd.Count; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            string add = "0";
            add = GetValue("RT550", $"{TestName}");
            visa.Port = (Config.ContainsKey("RT550Port")) ? Config["RT550Port"].ToString() : "COM1";
            #region MyRegion
            switch (cmd[1])
            {
                case "Connect":
                    return Connect();
                case "StartTest":
                    string TXPC = GetValue("RT550", $"{TestName}");
                    if (TXPC.Length <= 0) TXPC = "-70";
                    return StartTest(TXPC, cmd.Count >= 3 ? int.Parse(cmd[2]) : 16, cmd.Count >= 4 ? cmd[3] : "0");
                case "output_power_low":
                    return output_power_low(add);
                case "output_power_mid":
                    return output_power_mid(add);
                case "output_power_high":
                    return output_power_high(add);
                case "s_slot_sensitivitv":
                    return s_M_sensitivitv(add);
                case "s_H_sensitivitv":
                    return s_H_sensitivitv(add);
                case "s_L_sensitivitv":
                    return s_L_sensitivitv(add);
                case "carrier_drift_low":
                    return carrier_drift_low(add);
                case "carrier_drift_mid":
                    return carrier_drift_mid(add);
                case "carrier_drift_high":
                    return carrier_drift_high(add);

                case "Df1Avg": return Df1Avg(add);
                case "Df1LowAvg": return Df1LowAvg(add);
                case "Df1MiddleAvg": return Df1MiddleAvg(add);
                case "Df1HighAvg": return Df1HighAvg(add);
                case "DfDiff": return DfDiff(add);

                case "NXPCrystalTrimCWTest": return NXPCrystalTrimCWTest(cmd[2], int.Parse(cmd[3]), cmd[4]);
                case "Qcc514XCrystalTrimCWTest": return Qcc514XCrystalTrimCWTest("39", int.Parse(cmd[2]));
                case "ShowFreqTrim": return Config["FreqTrim"].ToString();

                case "ReadFreCqoffCW": return ReadFreqoffCW(cmd[2], cmd.Count >= 4 ? bool.Parse(cmd[3]) : false);

                case "ReadPowerCW": return ReadPowerCW(cmd[2], add);
                case "ReadFreqoffCW_Khz": return ReadFreqoffCW_Khz(cmd[2], cmd.Count >= 4 ? bool.Parse(cmd[3]) : false, add);



                case "BLERxTest": return BLERxTest(cmd[2], cmd[3]);
                case "BLETxTest": return BLETxTest(cmd[2]);
                case "BLE_AvgPower": return BLE_AvgPower(add);//第二部分
                case "BLE_AverageOffset": return BLE_AverageOffset(add);
                case "BLE_MaxDrift": return BLE_MaxDrift(add);
                case "BLE_InitialDriftRate": return BLE_InitialDriftRate(add);
                case "BLE_DriftRate": return BLE_DriftRate(add);
                case "BLE_DeltaF2Avg": return BLE_DeltaF2Avg(add);//第三部分
                default: return "Command Error False";
            }
            #endregion
        }
        string ReadFreqoffCW(string channel, bool init) => visa.ReadFreqoffCW(channel, (string)Config["RT550_IP"], init);
        string ReadFreqoffCW_Khz(string FreqMhzStr, bool init, string Add)
        {
            //FreqMhzStr=2402
            double FreqMhz = double.Parse(FreqMhzStr);
            double TrunCateMhz = Math.Truncate(FreqMhz);
            string channel = ((int)TrunCateMhz - 2402).ToString();
            string ReadStrVisa = visa.ReadFreqoffCW(channel, (string)Config["RT550_IP"], init);
            if (double.TryParse(ReadStrVisa, out double Freqoff))
            {
                double.TryParse(Add, out double i);
                //将写入的频偏转换成Khz
                double SendFreqMhzToKhz = (FreqMhz * 1000);
                // （因为频段是0-76表示需要转译） 写入的频段*1000转换成Khz  +   读取的频偏=产品发射的频段
                double ReadFreqhzToKhz = (TrunCateMhz * 1000) + (Freqoff / 1000);
                double DIFF = Math.Round((ReadFreqhzToKhz - SendFreqMhzToKhz) + i, 3);
                return DIFF.ToString("f3");
            }
            return ReadStrVisa;
        }
        string ReadPowerCW(string FreqMhzStr, string add)
        {

            //FreqMhzStr=2402
            double FreqMhz = double.Parse(FreqMhzStr);
            double TrunCate = Math.Truncate(FreqMhz);
            string channel = ((int)TrunCate - 2402).ToString();
            double.TryParse(add, out double i);
            string str = visa.ReadPowerCW(channel, (string)Config["RT550_IP"]);
            if (str.Contains("False"))
            {
                return str;
            }
            if (double.TryParse(str, out double Power))
            {
                return Math.Round(Power + i, 3).ToString("f3");
            };
            return str;


        }
        string NXPCrystalTrimCWTest(string channel, int Count, string InitiaTriml) => visa.NXPCrystalTrimCWTest(channel, Count, InitiaTriml, (string)Config["RT550_IP"]);
        string Qcc514XCrystalTrimCWTest(string channel, int Count) => visa.Qcc514XCrystalTrimCWTest(channel, Count, (string)Config["RT550_IP"]);









        #region BLETest
        /*
         因为BLE测试会使用几个测试模块，每个测试模块开始测试会刷新测试值，
         所以测试过程中需要储存测试值
        */
        string BLERxTest(string channel, string Packets) => visa.BLERxTest(channel, Packets, (string)Config["RT550_IP"]);
        string BLETxTest(string channel) => visa.BLETxTest(channel, (string)Config["RT550_IP"]);
        string BLE_AvgPower(string Add)
        {
            double.TryParse(Add, out double i);
            double Result = visa._BLE_AvgPower + i;
            return Result.ToString();
        }
        string BLE_AverageOffset(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("ORESULT TEST,1,LEICD2M");
            string[] read = visa.ReadString().Split(',');
            double Reslt = double.Parse(read[2]) / 1000 + i;
            return Reslt.ToString();
        }

        string BLE_MaxDrift(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("ORESULT TEST,1,LEICD2M");
            string[] read = visa.ReadString().Split(',');
            double Reslt = double.Parse(read[6]) / 1000 + i;
            return Reslt.ToString();
        }
        string BLE_InitialDriftRate(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XRESULT LEICD2M,HOPOFFL,1");
            string[] read = visa.ReadString().Split(',');
            double Reslt = double.Parse(read[12]) / 1000 + i;
            return Reslt.ToString();
        }

        string BLE_DriftRate(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XRESULT LEICD2M,HOPOFFL,1");
            if (!ReadResult(6, out double result)) return $"Test False {result}";
            double Reslt = result / 1000 + i;
            return Reslt.ToString();
        }
        string BLE_DeltaF2Avg(string Add)
        {

            double.TryParse(Add, out double i);
            double Result = visa._BLE_DeltaF2Avg / 1000 + i;
            return Result.ToString();

        }

        #endregion

















        #region Dut测试
        string Connect() => visa.DUTConnect((string)Config["RT550_IP"]);
        string StartTest(String TXP, int timeout, string PATHEDIT) => visa.StartTest(TXP, timeout, PATHEDIT);
        string output_power_low(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult OP,HopOffL");
            if (!ReadResult(5, out double result)) return $"Test False {result}";
            double Reslt = result + i;
            return Reslt.ToString("F3");

        }
        string output_power_mid(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult OP,HopOffM");
            if (!ReadResult(5, out double result)) return $"Test False {result}";
            double Reslt = result + i;
            return Reslt.ToString("F3");

        }
        string output_power_high(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult OP,HopOffH");
            if (!ReadResult(5, out double result)) return $"Test False {result}";
            double Reslt = result + i;
            return Reslt.ToString("F3");
        }
        string s_H_sensitivitv(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult SS,HopOffH");
            if (!ReadResult(3, out double result)) return $"Test False {result}";
            double Reslt = result + i;
            return Reslt.ToString("F3");
        }
        string s_M_sensitivitv(string add)
        {

            double.TryParse(add, out double i);
            visa.WriteCommand("XResult SS,HopOffM");
            if (!ReadResult(3, out double result)) return $"Test False {result}";
            double Reslt = result + i;
            return Reslt.ToString("F3");
        }
        string s_L_sensitivitv(string add)
        {

            double.TryParse(add, out double i);
            visa.WriteCommand("XResult SS,HopOffL");
            if (!ReadResult(3, out double result)) return $"Test False {result}";
            double Reslt = result + i;
            return Reslt.ToString("F3");
        }
        string carrier_drift_low(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult IC,HopOffL");
            if (!ReadResult(5, out double result)) return $"Test False {result}";
            double Reslt = result / 1000 + i;
            return Reslt.ToString("F3");
        }
        string carrier_drift_mid(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult IC,HopOffM");
            if (!ReadResult(5, out double result)) return $"Test False {result}";
            double Reslt = result / 1000 + i;
            return Reslt.ToString("F3");
        }
        string carrier_drift_high(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult IC,HopOffH");
            if (!ReadResult(5, out double result)) return $"Test False {result}";
            double Reslt = result / 1000 + i;
            return Reslt.ToString("F3");
        }
        string Df1Avg(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult MI, HopOffM");
            if (!ReadResult(5, out double result)) return $"Test False {result}";
            double Reslt = result / 1000 + i;
            return Reslt.ToString("F3");
        }

        string Df1LowAvg(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult MI,HopOffL");
            if (!ReadResult(5, out double result)) return $"Test False {result}";
            double Reslt = result / 1000 + i;
            return Reslt.ToString("F3");
        }
        string Df1MiddleAvg(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult MI,HopOffM");
            if (!ReadResult(5, out double result)) return $"Test False {result}";
            double Reslt = result / 1000 + i;
            return Reslt.ToString("F3");
        }
        string Df1HighAvg(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult MI,HopOffH");
            if (!ReadResult(5, out double result)) return $"Test False {result}";
            double Reslt = result / 1000 + i;
            return Reslt.ToString("F3");
        }
        string DfDiff(string Add)
        {
            double.TryParse(Add, out double i);
            visa.WriteCommand("XResult MI,HopOffM");
            if (!ReadResult(7, out double dif1Avg)) return $"1、Test False {dif1Avg}";
            return (dif1Avg + i).ToString();
        }

        #endregion
        bool ReadResult(int index, out double result)
        {
            string[] read = visa.ReadString().Split(',');
            result = double.Parse(read[index]);
            if (read[2] != "TRUE") return false;
            return true;
        }


        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVar, int size, string INIpath);
        public string GetValue(string section, string key, string _path = ".\\AllDLL\\MenuStrip\\AddDeploy.ini")
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