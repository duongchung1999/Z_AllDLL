using NXPRF_V1;
using NXPRF_V1.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    /// <summary dllName="NXPRF_V1">
    /// NXPRF_V1测试类
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法
        Dictionary<string, object> Config;
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        ControlNXP NXP = new ControlNXP();
        public object Interface(Dictionary<string, object> Config)
        {
            NXP.invoke = new Invoke(Config);
            return this.Config = Config;
        }
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：NXPRF_V1";
            string dllfunction = "Dll功能说明 ：初版NXP使用连扳测试";
            string dllHistoryVersion = "历史Dll版本：0.0.0.0";
            string dllVersion = "当前Dll版本：22.10.28.0";
            string dllChangeInfo = "Dll改动信息：";
            string[] info = { dllname, dllfunction, dllHistoryVersion, dllVersion, dllChangeInfo };

            return info;
        }
        #endregion


        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] CMD);
            try
            {
                switch (CMD[1])
                {
                    case "NXPCrystalTrim_MT8852B": return NXPCrystalTrim_MT8852B(CMD[2], int.Parse(CMD[3])); // NXP RF 校准方法
                    case "NXPCrystalTrim_RT550": return NXPCrystalTrim_RT550(CMD[2], int.Parse(CMD[3]));
                    case "NXPRFTestPrepare": return NXPRFTestPrepare(); // NXP RF测试前置
                    case "OpenTx2402DTMCH0": return OpenTx2402DTMCH0(); // 开启TX2402 Power测试通道
                    case "OpenTx2440DTMCH19": return OpenTx2440DTMCH19(); // 开启TX2440 Power测试通道
                    case "OpenTx2480DTMCH39": return OpenTx2480DTMCH39(); // 开启TX2480 Power测试通道
                    case "OpenRx2402DTMCH0": return OpenRx2402DTMCH0(); // 开启RX2402 Power测试通道
                    case "OpenRx2440DTMCH19": return OpenRx2440DTMCH19(); // 开启RX2440 Power测试通道
                    case "OpenRx2480DTMCH39": return OpenRx2480DTMCH39(); // 开启RX2480 Power测试通道
                    case "EndPacketRateTest": return EndPacketRateTest(); // 关闭RX测试通道并返回测试结果
                    case "CloseDTM": return CloseDTM(); // 关闭Power 测试通道
                    case "ShowFreqTrim": return ShowFreqTrim();//显示校准值
                    case "Test": return Test();
                }
                return "Command Error False";

            }
            finally
            {
                NXP.CloseCOM();
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
                    PropertyInfo Lowproperty = type.GetProperty("数值下限");
                    NXP.LowLimit = Lowproperty.GetValue(item, null).ToString();
                    PropertyInfo Upproperty = type.GetProperty("数值上限");
                    NXP.UpLimit = Upproperty.GetValue(item, null).ToString();
                }
                if (type == typeof(Dictionary<string, object>))
                    NXP.OnceConfig = OnceConfig = (Dictionary<string, object>)item;
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            CMD = listCMD.ToArray();
            NXP.PortName = OnceConfig.ContainsKey("NXP_RF_Comport") ? OnceConfig["NXP_RF_Comport"].ToString() : Config["NXP_RF_Comport"].ToString();
        }


        /// <summary isPublicTestItem="true">
        /// 用MT8852B 仪器进行校准NXP芯片
        /// </summary>
        /// <param name="InitTrim">初始校准值</param>
        /// <param name="TrimCount">校准次数 一般 15</param>
        /// <returns>频偏 Khz</returns>
        public string NXPCrystalTrim_MT8852B(string InitTrim, int TrimCount)
            => NXP.NXPCrystalTrim_MT8852B(InitTrim, TrimCount);
        /// <summary isPublicTestItem="true">
        /// 用RT550 仪器进行校准NXP芯片
        /// </summary>
        /// <param name="InitTrim">初始校准值</param>
        /// <param name="TrimCount">校准次数 一般 15</param>
        /// <returns>频偏 Khz</returns>

        public string NXPCrystalTrim_RT550(string InitTrim, int TrimCount)
            => NXP.NXPCrystalTrim_RT550(InitTrim, TrimCount);


        /// <summary isPublicTestItem="true">
        /// 显示校准值
        /// </summary>
        /// <returns></returns>
        public string ShowFreqTrim()
            => (this.OnceConfig["FreqTrim"] = Config["FreqTrim"] = NXP.Trim).ToString();

        /// <summary isPublicTestItem="true">
        /// RF测试前置动作
        /// </summary>
        /// <returns>info</returns>
        public string NXPRFTestPrepare()
            => NXP.NXPRFTestPrepare();

        /// <summary isPublicTestItem="true">
        /// 释放2402通道信号
        /// </summary>
        /// <returns></returns>
        public string OpenTx2402DTMCH0()
            => NXP.OpenTx2402DTMCH0();

        /// <summary isPublicTestItem="true">
        /// 释放2440通道信号
        /// </summary>
        /// <returns></returns>
        public string OpenTx2440DTMCH19()
            => NXP.OpenTx2440DTMCH19();

        /// <summary isPublicTestItem="true">
        /// 释放2480通道信号
        /// </summary>
        /// <returns></returns>
        public string OpenTx2480DTMCH39()
            => NXP.OpenTx2480DTMCH39();

        /// <summary isPublicTestItem="true">
        /// 开启RX2402 Power测试通道
        /// </summary>
        /// <returns></returns>
        public string OpenRx2402DTMCH0()
            => NXP.OpenRx2402DTMCH0();

        /// <summary isPublicTestItem="true">
        /// 开启RX2440 Power测试通道
        /// </summary>
        /// <returns></returns>
        public string OpenRx2440DTMCH19()
            => NXP.OpenRx2440DTMCH19();

        /// <summary isPublicTestItem="true">
        /// 开启RX2480 Power测试通道
        /// </summary>
        /// <returns></returns>
        public string OpenRx2480DTMCH39()
            => NXP.OpenRx2480DTMCH39();

        /// <summary isPublicTestItem="true">
        /// 关闭RX测试通道并返回测试结果
        /// </summary>
        /// <returns></returns>
        public string EndPacketRateTest()
            => NXP.EndPacketRateTest();
        /// <summary isPublicTestItem="true">
        /// 关闭Power 测试通道
        /// </summary>
        /// <returns></returns>
        public string CloseDTM()
            => NXP.CloseDTM();

        string Test()
         => NXP.Test();



    }
}
