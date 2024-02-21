using MT8852B_V1;
using RT550_V1.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    /// <summary dllName="MT8852B_V1">
    /// MT8852B_V1测试类
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        #region  字段，构造函数
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：MT8852B_V1";
            string dllfunction = "Dll功能说明 ：MT8852B仪器功能模块";
            string dllVersion = "当前Dll版本：23.6.12.8";

            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "22.10.10.0： 重构8852测试类";
            string dllChangeInfo2 = "22.12.15.0： 新增读取特性偏移可选值";
            string dllChangeInfo3 = "23.6.12.5： MEVN不知道什么原因抓不到TXT指令。更改不用读取txt指令";

            string[] info = { dllname, dllfunction,
                dllVersion,
                dllChangeInfo, dllChangeInfo1,dllChangeInfo2
            };
            return info;
        }
        double AddNumber = 0;
        ControlMT8852B MT8852B = new ControlMT8852B();
        Dictionary<string, object> Config;
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        public object Interface(Dictionary<string, object> keys)
           => this.Config = keys;
        #endregion


        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] CMD);
            switch (CMD[1])
            {
                #region DUT Test
                case "DUT_Test":
                    return DUT_Test(int.Parse(CMD[2]), int.Parse(CMD[3]));
                //功率
                case "DUT_Out_Put_Power_Low": return DUT_Out_Put_Power_Low();
                case "DUT_Out_Put_Power_Middle": return DUT_Out_Put_Power_Middle();
                case "DUT_Out_Put_Power_High": return DUT_Out_Put_Power_High();
                //载波
                case "DUT_Carrier_Drift_Low": return DUT_Carrier_Drift_Low();
                case "DUT_Carrier_Drift_Middle": return DUT_Carrier_Drift_Middle();
                case "DUT_Carrier_Drift_High": return DUT_Carrier_Drift_High();
                //灵敏度
                case "DUT_Sensitivity_Low": return DUT_Sensitivity_Low();
                case "DUT_Sensitivity_Middle": return DUT_Sensitivity_Middle();
                case "DUT_Sensitivity_High": return DUT_Sensitivity_High();
                //调制指数
                case "DUT_Modulation_Index_Low": return DUT_Modulation_Index_Low();
                case "DUT_Modulation_Index_Middle": return DUT_Modulation_Index_Middle();
                case "DUT_Modulation_Index_High": return DUT_Modulation_Index_High();

                case "DUT_Optional_Modulation_Index_Low": return DUT_Optional_Modulation_Index_Low(CMD[2], CMD[3]);
                case "DUT_Optional_Modulation_Index_Middle": return DUT_Optional_Modulation_Index_Middle(CMD[2], CMD[3]);
                case "DUT_Optional_Modulation_Index_High": return DUT_Optional_Modulation_Index_High(CMD[2], CMD[3]);

                #endregion

                #region CW 读频偏
                case "CWReadFreqOffset": return CWReadFreqoff(CMD[2], CMD.Length < 4 || bool.Parse(CMD[3]));
                //case "CWReadFreqOffset_Khz": return CWReadFreqOffset_Khz(CMD[2], CMD.Length >= 4 ? bool.Parse(CMD[3]) : true);
                //case "CWReadPower": return CWReadPower(CMD[2], CMD.Length >= 4 ? bool.Parse(CMD[3]) : true);
                #endregion

                #region BLE Test
                case "BLE_TX_LowPowerTest": return BLE_TX_LowPowerTest(bool.Parse(CMD[2]));
                case "BLE_TX_MiddlePowerTest": return BLE_TX_MiddlePowerTest(bool.Parse(CMD[2]));
                case "BLE_TX_HighPowerTest": return BLE_TX_HighPowerTest(bool.Parse(CMD[2]));
                case "BLE_TX_CarrierOffset": return BLE_TX_CarrierOffset();
                case "BLE_TX_Modulation": return BLE_TX_Modulation();
                case "BLE_RX_Sensitivity": return BLE_RX_Sensitivity();
                case "BLE_Optional_RX_Sensitivity":return BLE_Optional_RX_Sensitivity(CMD[2], CMD[3], CMD[4], CMD[5], CMD[6], CMD[7], CMD[8], CMD[9]);


                #endregion
                case "SaveLog": return SaveLog();
                default:
                    return "Command Error False";

            }
        }

        void SplitCMD(object[] Command, out string[] CMD)
        {
            string TestName = "";
            List<string> listCMD = new List<string>();

            foreach (var item in Command)
            {
                Type type = item.GetType();
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
            {
                double.TryParse(WindowsAPI.GetValue("MT8852B", $"{TestName}"), out AddNumber);
            }
            MT8852B.GPIB = (string)Config["MT8852BGPIB"];
        }
        //#####################################  DUT测试区  !!!!!!!!!!!!!!!!!!!!!!!!!!
        #region DUT测试方法
        /// <summary isPublicTestItem="true">
        /// DUT 连接及测试
        /// </summary>
        /// <param name="ConnectSleepTime">连接成功延时测试 常规 1 </param>
        /// <param name="TimeOut">DUT测试超时 常规 16 </param>
        /// <returns>BD 或 失败信息</returns>
        public string DUT_Test(int ConnectSleepTime, int TimeOut)
          => MT8852B.DUT_Test(ConnectSleepTime, TimeOut);

        /// <summary isPublicTestItem="true">
        /// DUT 读取测试低功率值 2402
        /// </summary>
        /// <returns></returns>
        public string DUT_Out_Put_Power_Low()
            => MT8852B.DUTGetTestResult("XRESULT OP,HOPOFFL", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取测试中功率值 2442
        /// </summary>
        /// <returns></returns>
        public string DUT_Out_Put_Power_Middle()
            => MT8852B.DUTGetTestResult("XRESULT OP,HOPOFFM", 5, AddNumber);


        /// <summary isPublicTestItem="true">
        /// DUT 读取测试高功率值 2480
        /// </summary>
        /// <returns></returns>
        public string DUT_Out_Put_Power_High()
            => MT8852B.DUTGetTestResult("XRESULT OP,HOPOFFH", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取载波偏移低 2402
        /// </summary>
        /// <returns></returns>
        public string DUT_Carrier_Drift_Low()
            => MT8852B.DUTGetTestResult_Khz("XRESULT IC,HOPOFFL", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取载波偏移中 2442
        /// </summary>
        /// <returns></returns>
        public string DUT_Carrier_Drift_Middle()
            => MT8852B.DUTGetTestResult_Khz("XRESULT IC,HOPOFFM", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取载波偏移高 2480
        /// </summary>
        /// <returns></returns>
        public string DUT_Carrier_Drift_High()
            => MT8852B.DUTGetTestResult_Khz("XRESULT IC,HOPOFFH", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取灵敏度低 2402
        /// </summary>
        /// <returns></returns>
        public string DUT_Sensitivity_Low()
            => MT8852B.DUTGetTestResult("XRESULT SS,HOPOFFL", 3, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取灵敏度中 2442
        /// </summary>
        /// <returns></returns>
        public string DUT_Sensitivity_Middle()
            => MT8852B.DUTGetTestResult("XRESULT SS,HOPOFFM", 3, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取灵敏度高 2480
        /// </summary>
        /// <returns></returns>
        public string DUT_Sensitivity_High()
            => MT8852B.DUTGetTestResult("XRESULT SS,HOPOFFH", 3, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 常规 特调指数低 2402
        /// </summary>
        /// <returns></returns>
        public string DUT_Modulation_Index_Low()
            => MT8852B.DUTGetTestResult_Khz("XRESULT MI,HOPOFFL", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 常规 特调指数中 2442
        /// </summary>
        /// <returns></returns>
        public string DUT_Modulation_Index_Middle()
            => MT8852B.DUTGetTestResult_Khz("XRESULT MI,HOPOFFM", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 常规 特调指数高 2480
        /// </summary>
        /// <returns></returns>
        public string DUT_Modulation_Index_High()
            => MT8852B.DUTGetTestResult_Khz("XRESULT MI,HOPOFFH", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT _可选 特调指数低 2402 
        /// </summary>
        /// <param name="index" options="F1_Max,F1_Average,F2_Max,F2_Average,F2_Avg/Flavg,F2_Max_Failed,F2_Max_Count_(Total),Failed,Tested">取哪个值</param>
        /// <param name="units" options="default,/1000_Khz">值的单位 Khz会将值÷1000</param>
        /// <returns></returns>
        public string DUT_Optional_Modulation_Index_Low(string index, string units)
        {
            return MT8852B.DUTSwitchUnitsGetTestResult(units, "XRESULT MI,HOPOFFL", index.SwictModulationPattern(), AddNumber);
        }

        /// <summary isPublicTestItem="true">
        /// DUT _可选 特调指数低 2442
        /// </summary>
        /// <param name="index" options="F1_Max,F1_Average,F2_Max,F2_Average,F2_Avg/Flavg,F2_Max_Failed,F2_Max_Count_(Total),Failed,Tested">取哪个值</param>
        /// <param name="units" options="default,/1000_Khz">值的单位 Khz会将值÷1000</param>
        /// <returns></returns>
        public string DUT_Optional_Modulation_Index_Middle(string index, string units)
        {
            return MT8852B.DUTSwitchUnitsGetTestResult(units, "XRESULT MI,HOPOFFM", index.SwictModulationPattern(), AddNumber);

        }

        /// <summary isPublicTestItem="true">
        /// DUT _可选 特调指数低 2480
        /// </summary>
        /// <param name="index" options="F1_Max,F1_Average,F2_Max,F2_Average,F2_Avg/Flavg,F2_Max_Failed,F2_Max_Count_(Total),Failed,Tested">取哪个值</param>
        /// <param name="units" options="default,/1000_Khz">值的单位 Khz会将值÷1000</param>
        /// <returns></returns>
        public string DUT_Optional_Modulation_Index_High(string index, string units)
        {
            return MT8852B.DUTSwitchUnitsGetTestResult(units, "XRESULT MI,HOPOFFH", index.SwictModulationPattern(), AddNumber);
        }


        #endregion

        //#####################################  CW测试区  !!!!!!!!!!!!!!!!!!!!!!!!!!
        #region CW测试区

        /// <summary>
        /// CW模式读取频偏
        /// </summary>
        /// <param name="channel">频偏的通道 0-78</param>
        /// <param name="init">是否初始化</param>
        /// <returns>返回频偏</returns>
        string CWReadFreqoff(string channel, bool init)
            => MT8852B.ReadFreqoffCW(channel, init);
        #endregion

        //#####################################  BLE测试区  !!!!!!!!!!!!!!!!!!!!!!!!!!

        #region BLE 测试

        /// <summary isPublicTestItem="true">
        /// BLE低频2402测试功率
        /// </summary>
        /// <param name="Init" options="True,False">是否初始化仪器 一般使用BLE的第一项初始化即可</param>
        /// <returns>True或信息</returns>
        public string BLE_TX_LowPowerTest(bool Init)
            => MT8852B.BLE_TX_LowPowerTest(Init, AddNumber);

        /// <summary isPublicTestItem="true">
        /// BLE中频2440测试功率
        /// </summary>
        /// <param name="Init" options="True,False">是否初始化仪器 一般使用BLE的第一项初始化即可</param>
        /// <returns>True或信息</returns>
        public string BLE_TX_MiddlePowerTest(bool Init)
            => MT8852B.BLE_TX_MiddlePowerTest(Init, AddNumber);

        /// <summary isPublicTestItem="true">
        /// BLE高频2480测试功率
        /// </summary>
        /// <param name="Init" options="True,False">是否初始化仪器 一般使用BLE的第一项初始化即可</param>
        /// <returns>True或信息</returns>
        public string BLE_TX_HighPowerTest(bool Init)
            => MT8852B.BLE_TX_HighPowerTest(Init, AddNumber);

        /// <summary isPublicTestItem="true">
        /// CarrierOffset 返回数值 需要先测试功率才能测试频偏
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string BLE_TX_CarrierOffset()
            => MT8852B.BLE_TX_CarrierOffset(AddNumber);

        /// <summary isPublicTestItem="true">
        /// Modulation 返回数值  需要先测试功率才能测试调制特性测试
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string BLE_TX_Modulation()
            => MT8852B.BLE_TX_Modulation(AddNumber);

        /// <summary isPublicTestItem="true">
        /// RX灵敏度发射包
        /// </summary>
        /// <returns>True Or False</returns>
        public string BLE_RX_Sensitivity()
            => MT8852B.BLE_RX_Sensitivity();


        /// <summary isPublicTestItem="true">
        /// BLE _可选 RX测试
        /// </summary>
        /// <param name="Channel">通道 比如19</param>
        /// <param name="Standard" options="BLE,2LE,LR8,LR2" >Standard</param>
        /// <param name="PowerLevel">Power Level 常规 -70</param>
        /// <param name="Payload" options="10101010,11110000,PRBS9,ONES,ZEROS">Payload</param>
        /// <param name="SyncWord">SyncWord 常规 71764129</param>
        /// <param name="Spacing">Spacing 常规 625</param>
        /// <param name="PayloadLength">Payload Length 常规 37</param>
        /// <param name="Packets">Packets 常规1500</param>
        /// <returns></returns>
        public string BLE_Optional_RX_Sensitivity(string Channel, string Standard, string PowerLevel, string Payload, string SyncWord, string Spacing, string PayloadLength, string Packets)
            => MT8852B.BLE_Optional_RX_Sensitivity(int.Parse(Channel), Standard, PowerLevel, Payload, SyncWord, Spacing, PayloadLength, Packets);



        #endregion

        /// <summary isPublicTestItem="true">
        /// 生成MT8852B指令日记
        /// </summary>
        /// <returns>True</returns>
        public string SaveLog()
              => MT8852B.SaveLog();



    }



}
