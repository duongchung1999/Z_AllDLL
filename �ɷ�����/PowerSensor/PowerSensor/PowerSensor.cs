using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法
        Dictionary<string, object> Config;
        public object Interface(Dictionary<string, object> Config) => this.Config = Config;

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：PowerSensor";
            string dllfunction = "Dll功能说明 ：电流表控制";
            string dllHistoryVersion = "历史Dll版本：0.0.0.0";
            string dllVersion = "当前Dll版本：0.0.0.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "0.0.0.0： 初测试功率计";
            string[] info = { dllname, dllfunction, dllHistoryVersion,
                dllVersion, dllChangeInfo,dllChangeInfo1
            };

            return info;
        }

        #endregion


        public string Run(object[] Command)
        {
            string mechod = Command.Where(item => item.GetType() == typeof(string)).First().ToString();
            List<string> cmd = new List<string>();
            foreach (var item in mechod.Split('&'))
                cmd.Add(item.Split('=')[1]);
            switch (cmd[1])
            {
                case "Peak":
                    return ReadPeak(double.Parse(cmd[2]));
                case "Freq":
                default:
                    return "Command Error False";
            }
        }
        mcl_pm_NET45.usb_pm pm1;
        string InitPowerSensor()
        {

            if (pm1 != null)
                pm1 = new mcl_pm_NET45.usb_pm();
            string SN = "";

            int openResult = pm1.Open_Sensor(ref SN);
            if (openResult < 1) return $"{openResult} Open_Sensor False";
            string GetSensorModelName = pm1.GetSensorModelName();
            string GetSensorSN = pm1.GetSensorSN();
            pm1.AVG = 1;  //  1 to actiate average ; 0 no average
            pm1.AvgCount = 4;  // average of 4 power readings  range 1..16
            pm1.Freq = 2441; // frequency= 1000 MHz 
                             //pm1.OffsetValue=10;  // offset of 10 dB
                             //pm1.OffsetValue_Enable=1;  // 1 for enable; 0 - no offset
                             //pm1.Close_Sensor();

            return "True";
        }
        string ReadPeak(double Freq)
        {
            try
            {
                mcl_pm_NET45.usb_pm pm1;
                pm1 = new mcl_pm_NET45.usb_pm();
                string SN = "";
                int OpenResult = pm1.Open_Sensor(ref SN);
                if (OpenResult < 1)
                    return $"{OpenResult} Open_Sensor False";
                pm1.AVG = 1;  //  1 to actiate average ; 0 no average
                pm1.AvgCount = 4;  // average of 4 power readings  range 1..16
                pm1.Freq = Freq; // frequency= 1000 MHz 
                //pm1.OffsetValue = 10;  // offset of 10 dB
                //pm1.OffsetValue_Enable = 0;  // 1 for enable; 0 - no offset
                float ReadPwr = pm1.ReadPower();
                return ReadPwr.ToString();
            }
            finally
            {
                pm1.Close_Sensor();
            }

        }
        string ReadFreq(double Freq)
        {
            try
            {
                mcl_pm_NET45.usb_pm pm1;
                pm1 = new mcl_pm_NET45.usb_pm();
                string SN = "";
                int OpenResult = pm1.Open_Sensor(ref SN);
                if (OpenResult < 1)
                    return $"{OpenResult} Open_Sensor False";
                pm1.AVG = 1;  //  1 to actiate average ; 0 no average
                pm1.AvgCount = 4;  // average of 4 power readings  range 1..16
                pm1.Freq = Freq; // frequency= 1000 MHz 
                //pm1.OffsetValue = 10;  // offset of 10 dB
                //pm1.OffsetValue_Enable = 0;  // 1 for enable; 0 - no offset
                double ReadPwr = pm1.FC_ReadFreq();
                Console.WriteLine(ReadPwr);
                return ReadPwr.ToString();
            }
            finally
            {
                pm1.Close_Sensor();
            }
        }
    }
}
