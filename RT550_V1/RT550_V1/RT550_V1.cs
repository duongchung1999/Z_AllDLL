using RT550_V1.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    /// <summary dllName="RT550_V1">
    /// RT550仪器
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：RT550_V1";
            string dllfunction = "Dll功能说明 ：RT550仪器功能模块";
            string dllVersion = "当前Dll版本：23.7.3.0";

            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "22.9.29.0： 重构RT550兼容连扳测试";
            string dllChangeInfo2 = "22.12.15.0： 新增读取特性偏移可选值";
            string dllChangeInfo3 = "23.1.6： 增加BLE 1M modulation index 可选";
            string dllChangeInfo4 = "23.6.8： 增加DUT modulation index 可选";
            string dllChangeInfo5 = "23.6.13.0： 增加可以用指令调节通道";
            string dllChangeInfo6 = "23.7.3.0： 增加可以用指令调节通道";
            string[] info = { dllname, dllfunction,
                dllVersion,
                dllChangeInfo, dllChangeInfo1,dllChangeInfo2,dllChangeInfo3,dllChangeInfo4
            };
            return info;
        }

        #region 字段，构造函数
        public MerryDll()
        {

        }
        /// <summary>
        /// 新式的分享全局变量
        /// </summary>
        /// <param name="Config"></param>
        public MerryDll(Dictionary<string, object> Config)
         => this.Config = Config;
        /// <summary>
        /// 老式的分享全局变量
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public object Interface(Dictionary<string, object> keys)
           => this.Config = keys;
        double AddNumber = 0;
        string Address = "";
        ControlRT550 RT550 = new ControlRT550();
        Dictionary<string, object> Config;
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();

        #endregion


        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] CMD);

            switch (CMD[1])
            {
                #region DUT Test

                case "DUT_Test":
                    return DUT_Test(
                        ConnectSleepTime: CMD.Length >= 3 ? int.Parse(CMD[2]) : 16,
                        TimeOut: CMD.Length >= 4 ? int.Parse(CMD[3]) : 16,
                    TXP: CMD.Length >= 5 ? CMD[4] : "-40",
                    ExtraLoss: CMD.Length >= 6 ? CMD[5] : "0",
                    ManualFlag: CMD[6] == "True");

                case "DUT_Optional_Test": 
                    return DUT_Optional_Test(int.Parse(CMD[2]), CMD[3], CMD[4], CMD[5], CMD[6], CMD[7], CMD[8], CMD[9]);

                //功率
                case "DUT_Out_Put_Power_Low": return DUT_Out_Put_Power_Low();
                case "DUT_Out_Put_Power_Middle": return DUT_Out_Put_Power_Middle();
                case "DUT_Out_Put_Power_High": return DUT_Out_Put_Power_High();
                //载波
                case "DUT_Initial_Carrier_Low":
                case "DUT_Carrier_Drift_Low": return DUT_Carrier_Drift_Low();

                case "DUT_Initial_Carrier_Middle":
                case "DUT_Carrier_Drift_Middle": return DUT_Carrier_Drift_Middle();

                case "DUT_Initial_Carrier_High":
                case "DUT_Carrier_Drift_High": return DUT_Carrier_Drift_High();
                //灵敏度
                case "DUT_Sensitivity_Low": return DUT_Sensitivity_Low();
                case "DUT_Sensitivity_Middle": return DUT_Sensitivity_Middle();
                case "DUT_Sensitivity_High": return DUT_Sensitivity_High();
                //调制指数
                case "DUT_Modulation_Index_Low": return DUT_Modulation_Index_Low();
                case "DUT_Modulation_Index_Middle": return DUT_Modulation_Index_Middle();
                case "DUT_Modulation_Index_High": return DUT_Modulation_Index_High();
                case "DUT_Modulation_Index_dfDiff": return DUT_Modulation_Index_dfDiff();

                case "DUT_Optional_Modulation_Index_Low": return DUT_Optional_Modulation_Index_Low(CMD[2], CMD[3]);
                case "DUT_Optional_Modulation_Index_Middle": return DUT_Optional_Modulation_Index_Middle(CMD[2], CMD[3]);
                case "DUT_Optional_Modulation_Index_High": return DUT_Optional_Modulation_Index_High(CMD[2], CMD[3]);



                #endregion

                #region CW

                case "CWReadFreqOffset": return CWReadFreqOffset(CMD[2], CMD.Length >= 4 ? bool.Parse(CMD[3]) : true);
                case "CWReadFreqOffset_Khz": return CWReadFreqOffset_Khz(CMD[2], CMD.Length >= 4 ? bool.Parse(CMD[3]) : true);
                case "CWReadPower": return CWReadPower(CMD[2], CMD.Length >= 4 ? bool.Parse(CMD[3]) : true);
                #endregion

                #region BLE Test
                case "BLE_Optional_Rx_Test": return BLE_Optional_Rx_Test(CMD[2], CMD[3], CMD[4], CMD[5], CMD[6], CMD[7], CMD[8], CMD[9]);

                case "BLERxTest": return BLERxTest(CMD[2], CMD[3]);



                case "BLE_Tx_PowerTest": return BLE_Tx_PowerTest(CMD[2]);
                case "BLE_CarrierOffset": return BLE_CarrierOffset();
                case "BLE_Modulation": return BLE_Modulation();
                case "BLE_MaxDrift": return BLE_MaxDrift();
                case "BLE_InitialDriftRate": return BLE_InitialDriftRate();
                case "BLE_DriftRate": return BLE_DriftRate();

                case "BLE_Tx_PorwerTest_2M": return BLE_Tx_PorwerTest_2M(CMD[2], CMD[3], CMD[4], CMD[5], CMD[6]);
                case "BLE_CarrierOffset_2M": return BLE_CarrierOffset_2M(CMD[2], CMD[3], CMD[4]);
                case "BLE_Modulation_2M": return BLE_Modulation_2M(CMD[2], CMD[3], CMD[4], CMD[5], CMD[6]);
                case "BLE_Tx_PowerTest_1M": return BLE_Tx_PowerTest_1M(CMD[2], CMD[3], CMD[4], CMD[5], CMD[6]);
                case "BLE_CarrierOffset_1M": return BLE_CarrierOffset_1M(CMD[2], CMD[3], CMD[4]);
                case "BLE_Modulation_1M": return BLE_Modulation_1M(CMD[2], CMD[3], CMD[4], CMD[5], CMD[6]);
                #endregion

                #region 辅助方法
                case "Set_RF_Port": return Set_RF_Port(CMD[2]);
                case "StopTesting": return StopTesting();
                case "SaveLog": return SaveLog();

                //仪器锁
                case "LockRT550": return LockRT550();
                case "UnlockRT550": return UnlockRT550();
                default:
                    return "Command Error False";
                    #endregion


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
                if (type == typeof(Dictionary<string, object>))
                    OnceConfig = (Dictionary<string, object>)item;
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            CMD = listCMD.ToArray();
            if (OnceConfig.ContainsKey("SN"))
            {
                string TestID = OnceConfig["TestID"].ToString();
                double.TryParse(WindowsAPI.GetValue($"{TestID}#RT550", TestName, ".\\AllDLL\\MenuStrip\\MoreAddDeploy.ini"), out AddNumber);
            }
            else
            {
                double.TryParse(WindowsAPI.GetValue("RT550", $"{TestName}"), out AddNumber);

            }
            RT550.ComPort = (OnceConfig.ContainsKey("RT550Port")) ? (string)OnceConfig["RT550Port"] : (string)Config["RT550Port"];
            Address = (OnceConfig.ContainsKey("BitAddress")) ? (string)OnceConfig["BitAddress"] : (string)Config["BitAddress"];
            RT550.IPv4 = (string)Config["RT550_IP"];
        }




        //#####################################  DUT测试区  !!!!!!!!!!!!!!!!!!!!!!!!!!
        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 分割线 DUT区 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string Cut_OffRule____()
        {
            return "True";
        }
        #region DUT测试方法
        /// <summary isPublicTestItem="true">
        /// DUT _常规 连接及测试
        /// </summary>
        /// <param name="ConnectSleepTime">连接成功延时测试 常规1</param>
        /// <param name="TimeOut">超时 /秒 常规 “16”</param>
        /// <param name="TXP">TX功率 常规 “-40”</param>
        /// <param name="ExtraLoss">连接功率 常规 “0”</param>
        /// <param name="ManualFlag" options="False,True">是否指定配对 True 为指定 连接地址在Config或onceConfig ["BitAddress"]</param>
        /// 
        /// <returns>True</returns>
        public string DUT_Test(int ConnectSleepTime, int TimeOut, string TXP, string ExtraLoss, bool ManualFlag)
          => RT550.DUT_Test(ConnectSleepTime, TimeOut, TXP, ExtraLoss, ManualFlag, Address);

        /// <summary isPublicTestItem="true">
        /// DUT _进阶 连接及测试
        /// </summary>
        /// <param name="TimeOut">超时 /秒 常规 “20”</param>
        /// <param name="TXP">TX功率 常规 “-40”</param>
        /// <param name="ExtraLoss">连接功率 常规 “0”</param>
        /// <param name="PathEditLow">低频线损 传导常规“-3” 耦合常规 “-15”</param>
        /// <param name="PathEditMiddle">中频线损 传导常规“-3” 耦合常规 “-15”</param>
        /// <param name="PathEditHigh">高频线损 传导常规“-3” 耦合常规 “-15”</param>
        /// <param name="Packaging">丢包率 包的数量 常规“500”</param>
        /// <param name="PathEdit_P">丢包率 线损 常规“-70”</param>
        /// <returns></returns>
        public string DUT_Optional_Test(int TimeOut, string TXP, string ExtraLoss, string PathEditLow, string PathEditMiddle, string PathEditHigh, string Packaging, string PathEdit_P)
            => RT550.DUT_Optional_Test(TimeOut, TXP, ExtraLoss, PathEditLow, PathEditMiddle, PathEditHigh, Packaging, PathEdit_P);



        /// <summary isPublicTestItem="true">
        /// DUT 读取测试低功率值 2402 “Oouput Power”
        /// </summary>
        /// <returns>数值</returns>
        public string DUT_Out_Put_Power_Low()
            => RT550.DUTGetTestResult("XResult OP,HopOffL", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取测试中功率值 2441 “Oouput Power”
        /// </summary>
        /// <returns>数值</returns>
        public string DUT_Out_Put_Power_Middle()
            => RT550.DUTGetTestResult("XResult OP,HopOffM", 5, AddNumber);


        /// <summary isPublicTestItem="true">
        /// DUT 读取测试高功率值 2480 “Oouput Power”
        /// </summary>
        /// <returns>数值</returns>
        public string DUT_Out_Put_Power_High()
            => RT550.DUTGetTestResult("XResult OP,HopOffH", 5, AddNumber);








        /// <summary isPublicTestItem="true">
        /// DUT 读取载波偏移低 2402 “Initial Carrier”
        /// </summary>
        /// <returns></returns>
        public string DUT_Initial_Carrier_Low()
            => DUT_Carrier_Drift_Low();
        //方法名写错了,实际读得是Initial_Carrier。
        string DUT_Carrier_Drift_Low()
          => RT550.DUTGetTestResult_Khz("XResult IC,HopOffL", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取载波偏移中 2441 “Initial Carrier”
        /// </summary>
        /// <returns></returns>
        public string DUT_Initial_Carrier_Middle()
            => DUT_Carrier_Drift_Middle();
        //方法名写错了,实际读得是Initial_Carrier。
        string DUT_Carrier_Drift_Middle()
          => RT550.DUTGetTestResult_Khz("XResult IC,HopOffM", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取载波偏移高 2480 “Initial Carrier”
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string DUT_Initial_Carrier_High()
            => DUT_Carrier_Drift_High();

        //方法名写错了,实际读得是Initial_Carrier。

        string DUT_Carrier_Drift_High()
            => RT550.DUTGetTestResult_Khz("XResult IC,HopOffH", 5, AddNumber);









        /// <summary isPublicTestItem="true">
        /// DUT 读取灵敏度低 2402 “Single Sensitivity”
        /// </summary>
        /// <returns>数值</returns>
        public string DUT_Sensitivity_Low()
            => RT550.DUTGetTestResult("XResult SS,HopOffL", 3, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取灵敏度中 2441 “Single Sensitivity”
        /// </summary>
        /// <returns>数值</returns>
        public string DUT_Sensitivity_Middle()
            => RT550.DUTGetTestResult("XResult SS,HopOffM", 3, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 读取灵敏度高 2480 “Single Sensitivity”
        /// </summary>
        /// <returns>数值</returns>
        public string DUT_Sensitivity_High()
            => RT550.DUTGetTestResult("XResult SS,HopOffH", 3, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 常规 特调指数低 2402 “Modulation Index”
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string DUT_Modulation_Index_Low()
            => RT550.DUTGetTestResult_Khz("XResult MI,HopOffL", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT 常规 特调指数中 2441 “Modulation Index”
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string DUT_Modulation_Index_Middle()
            => RT550.DUTGetTestResult_Khz("XResult MI,HopOffM", 5, AddNumber);

        /// <summary isPublicTestItem="false">
        /// DUT 常规 特调指数 系数  “Modulation Index”
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string DUT_Modulation_Index_dfDiff()
            => RT550.DUTGetTestResult("XResult MI,HopOffM", 7, AddNumber);
        /// <summary isPublicTestItem="true">
        /// DUT 常规 特调指数高 2480 “Modulation Index”
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string DUT_Modulation_Index_High()
            => RT550.DUTGetTestResult_Khz("XResult MI,HopOffH", 5, AddNumber);

        /// <summary isPublicTestItem="true">
        /// DUT _可选 特调指数低 2402  “Modulation Index”
        /// </summary>
        /// <param name="index" options="F1_Max,F1_Average,F2_Max,F2_Average,F2_Avg/Flavg,F2_Max_Failed,F2_Max_Count_(Total),Failed,Tested,PassRate">取哪个值</param>
        /// <param name="units" options="default,/1000_Khz">值的单位 Khz会将值÷1000</param>
        /// <returns></returns>
        public string DUT_Optional_Modulation_Index_Low(string index, string units)
        {
            return RT550.DUTSwitchUnitsGetTestResult(units, "XResult MI,HopOffL", index.DUTSwictModulationPattern(), AddNumber);
        }

        /// <summary isPublicTestItem="true">
        /// DUT _可选 特调指数低 2441 “Modulation Index”
        /// </summary>
        /// <param name="index" options="F1_Max,F1_Average,F2_Max,F2_Average,F2_Avg/Flavg,F2_Max_Failed,F2_Max_Count_(Total),Failed,Tested">取哪个值</param>
        /// <param name="units" options="default,/1000_Khz">值的单位 Khz会将值÷1000</param>
        /// <returns></returns>
        public string DUT_Optional_Modulation_Index_Middle(string index, string units)
        {
            return RT550.DUTSwitchUnitsGetTestResult(units, "XResult MI,HopOffM", index.DUTSwictModulationPattern(), AddNumber);

        }

        /// <summary isPublicTestItem="true">
        /// DUT _可选 特调指数低 2480 “Modulation Index”
        /// </summary>
        /// <param name="index" options="F1_Max,F1_Average,F2_Max,F2_Average,F2_Avg/Flavg,F2_Max_Failed,F2_Max_Count_(Total),Failed,Tested">取哪个值</param>
        /// <param name="units" options="default,/1000_Khz">值的单位 Khz会将值÷1000</param>
        /// <returns></returns>
        public string DUT_Optional_Modulation_Index_High(string index, string units)
        {
            return RT550.DUTSwitchUnitsGetTestResult(units, "XResult MI,HopOffM", index.DUTSwictModulationPattern(), AddNumber);
        }





        #endregion

        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 分割线 CW区 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string Cut_OffRule()
        {
            return "True";
        }

        //#####################################  CW测试区   !!!!!!!!!!!!!!!!!!!!!!!!!!
        #region CW 

        /// <summary isPublicTestItem="true">
        /// CW 用9320B类似方式读取频偏hz
        /// </summary>
        /// <param name="Channel">蓝牙通道</param>
        /// <param name="InitFlag">是否初始化</param>
        /// <returns>数值 hz</returns>
        public string CWReadFreqOffset(string Channel, bool InitFlag)
         => RT550.CWReadFreqOffset(Channel, InitFlag);

        /// <summary isPublicTestItem="true">
        /// CW 用9320B类似方式读取频偏
        /// </summary>
        /// <param name="FreqMhzStr">举个例子2401.35</param>
        /// <param name="init">是否初始化(不会就输True)</param>
        /// <returns>数值 Khz</returns>
        public string CWReadFreqOffset_Khz(string FreqMhzStr, bool init)
        {
            //FreqMhzStr=2402
            double FreqMhz = double.Parse(FreqMhzStr);
            double TrunCateMhz = Math.Truncate(FreqMhz);
            string channel = ((int)TrunCateMhz - 2402).ToString();
            string ReadStrVisa = RT550.CWReadFreqOffset(channel, init);
            if (double.TryParse(ReadStrVisa, out double Freqoff))
            {
                //将写入的频偏转换成Khz
                double SendFreqMhzToKhz = (FreqMhz * 1000);
                // （因为频段是0-76表示需要转译） 写入的频段*1000转换成Khz  +   读取的频偏=产品发射的频段
                double ReadFreqhzToKhz = (TrunCateMhz * 1000) + (Freqoff / 1000);
                return ((ReadFreqhzToKhz - SendFreqMhzToKhz) + AddNumber).ToString("f3");
            }
            return ReadStrVisa;
        }

        /// <summary isPublicTestItem="true">
        /// CW 用9320B类似方式读取功率
        /// </summary>
        /// <param name="FreqMhzStr">举个例子2401.35</param>
        /// <param name="init">是否初始化(不会就输True)</param>
        /// <returns>数值</returns>
        public string CWReadPower(string FreqMhzStr, bool init)
            => RT550.CWReadPower(FreqMhzStr, init);


        #endregion

        //#########################################  BLE 测试区  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        #region BLE Test

        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 分割线 BLE RX 区 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string Cut_OffRule_____()
        {
            return "True";
        }
        /// <summary isPublicTestItem="true">
        /// BLE 2M 测试模块 RX测试模式
        /// </summary>
        /// <param name="Channel">通道 比如19 </param>
        /// <param name="Packets">数据包 比如1000</param>
        /// <returns>True</returns>
        public string BLERxTest(string Channel, string Packets)
            => RT550.BLERxTest(Channel, Packets);

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
        public string BLE_Optional_Rx_Test(string Channel, string Standard, string PowerLevel, string Payload, string SyncWord, string Spacing, string PayloadLength, string Packets)
            => RT550.BLE_Optional_Rx_Test(int.Parse(Channel), Standard, PowerLevel, Payload, SyncWord, Spacing, PayloadLength, Packets);



        //#########################################  2M  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        #region 2M

        #region 已经抛弃的内容
        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 分割线 BLE 2M 区 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string Cut_OffRule_()
        {
            return "True";
        }
        [Obsolete]
        /// <summary isPublicTestItem="false">
        /// BLE 2M 功率 Power 测试
        /// </summary>
        /// <param name="Channel">通道 比如 0~39</param>
        /// <returns>数值 dBm</returns>
        public string BLE_Tx_PowerTest(string Channel)
            => RT550.BLE_Tx_PowerTest(Channel, AddNumber);

        [Obsolete]
        /// <summary isPublicTestItem="false">
        /// BLE 2M 频偏系列测试 返回平均偏移
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string BLE_CarrierOffset()
            => RT550.BLE_CarrierOffset(AddNumber);

        [Obsolete]
        /// <summary isPublicTestItem="false">
        /// BLE 2M 最大漂移 频偏系列 要先测频偏
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string BLE_MaxDrift()
              => RT550.BLEGetTestResult_Khz("ORESULT TEST,1,LEICD2M", 6, AddNumber, 1);
        [Obsolete]
        /// <summary isPublicTestItem="false">
        /// BLE 2M Carrier 频偏系列 要先测频偏
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string BLE_InitialDriftRate()
            => RT550.BLEGetTestResult_Khz("XRESULT LEICD2M,HOPOFFL,1", 12, AddNumber, 2);

        [Obsolete]
        /// <summary isPublicTestItem="false">
        /// BLE 2M Carrier 频偏系列 要先测频偏
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string BLE_DriftRate()
            => RT550.BLEGetTestResult_Khz("XRESULT LEICD2M,HOPOFFL,1", 6, AddNumber, 2);

        [Obsolete]
        /// <summary isPublicTestItem="false">
        /// BLE 2M Modulation Index f2 Avg
        /// </summary>
        /// <returns>数值 Khz</returns>
        public string BLE_Modulation()
                    => RT550.BLE_Modulation(AddNumber);


        #endregion



        /// <summary isPublicTestItem="true">
        /// BLE 2M Output Power 功率
        /// </summary>
        /// <param name="Channel">Channel 范围 0~39</param>
        /// <param name="FIXEDOFF">FIXEDOFF 常规 0</param>
        /// <param name="SyncWord">SyncWord 常规 71764129</param>
        /// <param name="PayloadLength">Payload Length 常规 37</param>
        /// <param name="GetResult" options="PeakToAvg,AvgPower">取哪个值</param>
        /// <returns>数值 dBm</returns>

        public string BLE_Tx_PorwerTest_2M(string Channel, string FIXEDOFF, string SyncWord, string PayloadLength, string GetResult)
            => RT550.BLE_Tx_PowerTest_2M(Channel, FIXEDOFF, SyncWord, PayloadLength, GetResult.SwictPowerResult(), AddNumber);

        /// <summary isPublicTestItem="true">
        /// BLE 2M Carrier Frequency 频率
        /// </summary>
        /// <param name="_SyncWord">SyncWord 常规 71764129></param>
        /// <param name="_PayloadLength">Payload Length 常规 37</param>
        /// <param name="_GetResult" options="Max+veOffset,Max-veOffset,AverageOffset,MaxDrift,AvgDrift,InitialDriftRate,DriftRate">取哪个值</param>
        /// <returns></returns>
        public string BLE_CarrierOffset_2M(string _SyncWord, string _PayloadLength, string _GetResult)
            => RT550.BLE_CarrierOffset_2M(_SyncWord, _PayloadLength, _GetResult.SwictCarrierFrequencyResult(), AddNumber);

        /// <summary isPublicTestItem="true">
        /// BLE 2M Modulation Characteristics 调制特性
        /// </summary>
        /// <param name="SyncWord">SyncWord 常规 71764129</param>
        /// <param name="Payload" options="MOD10101010,MOD11110000,PRBS9,ONES,ZEROS">Payload 测试模式</param>
        /// <param name="PayloadLength">Payload Length 常规 37</param>
        /// <param name="GetResult" options="F1_Max,F1_Average,F2_Max,F2_Average,F2_Avg/Flavg,F2_Max_Failed,F2_Max_Count_(Total),Failed,Tested,PassRate">取哪个值</param>
        /// <param name="units" options="default,/1000_Khz">值的单位 Khz会将值÷1000</param>
        /// <returns>数值 Khz</returns>
        public string BLE_Modulation_2M(string SyncWord, string Payload, string PayloadLength, string GetResult, string units)
                    => RT550.BLE_Modulation_2M(SyncWord, Payload, PayloadLength, GetResult.BLESwictModulationPattern(), units, AddNumber);

        #endregion

        //#########################################  1M  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 分割线 BLE 1M 区 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string Cut_OffRule__()
        {
            return "True";
        }
        #region 1M

        /// <summary isPublicTestItem="true">
        /// BLE 1M Output Power 功率
        /// </summary>
        /// <param name="Channel">Channel 范围 0~39</param>
        /// <param name="FIXEDOFF">FIXEDOFF 常规 0</param>
        /// <param name="SyncWord">SyncWord 常规 71764129</param>
        /// <param name="PayloadLength">Payload Length 常规 37</param>
        /// <param name="GetResult" options="PeakToAvg,AvgPower">取哪个值</param>
        /// <returns>数值 dBm</returns>
        public string BLE_Tx_PowerTest_1M(string Channel, string FIXEDOFF, string SyncWord, string PayloadLength, string GetResult)
            => RT550.BLE_Tx_PowerTest_1M(Channel, FIXEDOFF, SyncWord, PayloadLength, GetResult.SwictPowerResult(), AddNumber);

        /// <summary isPublicTestItem="true">
        /// BLE 1M Carrier Frequency 频率
        /// </summary>
        /// <param name="_SyncWord">SyncWord 常规 71764129></param>
        /// <param name="_PayloadLength">Payload Length 常规 37</param>
        /// <param name="_GetResult" options="Max+veOffset,Max-veOffset,AverageOffset,MaxDrift,AvgDrift,InitialDriftRate,DriftRate">取哪个值</param>
        /// <returns></returns>
        public string BLE_CarrierOffset_1M(string _SyncWord, string _PayloadLength, string _GetResult)
            => RT550.BLE_CarrierOffset_1M(_SyncWord, _PayloadLength, _GetResult.SwictCarrierFrequencyResult(), AddNumber);

        /// <summary isPublicTestItem="true">
        /// BLE 1M Modulation Characteristics 调制特性
        /// </summary>
        /// <param name="SyncWord">SyncWord 常规 71764129</param>
        /// <param name="Payload" options="MOD10101010,MOD11110000,PRBS9,ONES,ZEROS">Payload 测试模式</param>
        /// <param name="PayloadLength">Payload Length 常规 37</param>
        /// <param name="GetResult" options="F1_Max,F1_Average,F2_Max,F2_Average,F2_Avg/Flavg,F2_Max_Failed,F2_Max_Count_(Total),Failed,Tested,PassRate">取哪个值</param>
        /// <param name="units" options="default,/1000_Khz">值的单位 Khz会将值÷1000</param>
        /// <returns>数值 Khz</returns>
        public string BLE_Modulation_1M(string SyncWord, string Payload, string PayloadLength, string GetResult, string units)
                    => RT550.BLE_Modulation_1M(SyncWord, Payload, PayloadLength, GetResult.BLESwictModulationPattern(), units, AddNumber);

        #endregion




        #endregion

        //#########################################  辅组方法  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 分割线 辅组 区 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string Cut_OffRule___()
        {
            return "True";
        }
        #region 辅组方法


        /// <summary isPublicTestItem="true">
        ///  设置通道
        /// </summary>
        /// <param name="ComPort" options="COM1,COM2">设置通道 正在DUT或BLE测试中 禁止下</param>
        /// <returns></returns>
        public string Set_RF_Port(string ComPort)
        {
            Config["RT550Port"] = ComPort;
            return RT550.Set_RF_Port(ComPort);

        }

        /// <summary isPublicTestItem="true">
        /// 停止测试
        /// </summary>
        /// <returns>True</returns>
        public string StopTesting()
        => RT550.StopTesting();

        /// <summary isPublicTestItem="true">
        /// 锁定RT550方法
        /// </summary>
        /// <returns>True</returns>
        public string LockRT550()
            => _LockRT550.Lock(OnceConfig.ContainsKey("TestID") ? OnceConfig["TestID"].ToString() : "-1").ToString();

        /// <summary isPublicTestItem="true">
        /// 解锁RT550方法
        /// </summary>
        /// <returns>True</returns>
        public string UnlockRT550()
            => _LockRT550.UnLock().ToString();

        /// <summary isPublicTestItem="true">
        /// 生成RT550指令日记
        /// </summary>
        /// <returns>True</returns>
        public string SaveLog()
              => RT550.SaveLog();
        #endregion


    }

}
