using Airoha.AdjustANC;
using MerryTest.testitem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace MerryDllFramework
{
    /// <summary dllName="Airoha_ANC," ANC_CMD_List="Airoha.ANC">
    /// Airoha_ANC
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Airoha ANC";
            string dllfunction = "Dll功能说明 ：Airoha模块";
            string dllVersion = "当前Dll版本：23.11.3.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.6.5.0：  ANC开发试跑首版";
            string dllChangeInfo2 = "23.7.28.0：  增加固定次数测试，然后从固定测出的结果获取Gain进行筛选，得到认可首版";
            string dllChangeInfo23 = "23.10.12.0：  发现历史校准后的测试数据筛选还是NG的，每次测试完毕后会将筛选的历史内容删除";
            string dllChangeInfo3 = "23.11.3.0：  增加插件窗体";



            string[] info = { dllname, dllfunction,
                dllVersion,
                dllChangeInfo, dllChangeInfo1
            };
            return info;
        }

        #region 字段，构造函数
        /// <summary>
        /// 老式的分享全局变量
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public object Interface(Dictionary<string, object> Config)
        {
            _invoke = new Invoke(Config);
            this.Config = Config;
            Config["Airoha.ANC.SwitchANCOff"] = new Func<string>(() => _SPP_SwitchANC("ANC_OFF"));//切换到FB模式//Airo_Thru
            Config["Airoha.ANC.SwitchAiroThru"] = new Func<string>(() => _SPP_SwitchANC("Airo_Thru"));//切换到FB模式//Airo_Thru
            Config["Airoha.ANC.SwitchFBOnly"] = new Func<string>(() => _SPP_SwitchANC("FB_only"));//切换到FB模式
            Config["Airoha.ANC.SwitchFFOnly"] = new Func<string>(() => _SPP_SwitchANC("FF_only"));//切换到FF模式
            Config["Airoha.ANC.SwitchHybrid"] = new Func<string>(() => _SPP_SwitchANC("Hybrid"));//切换到混合模式
            Config["Airoha.ANC.GetGain"] = new Func<string>(() => _SPP_Get_ANC_Gain());//读取Gain值
            Config["Airoha.ANC.SetGain"] = new Func<string, string>(Gain => _SPP_SetGain(Gain));//设置缓存Gain值到
            Config["Airoha.ANC.SaveGain"] = new Func<string, string>(Gain => _SPP_SaveANC_Gain(Gain));//保存Gain值
            return "";
        }
        Dictionary<string, object> Config;
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        GetHandle GetHD = new GetHandle();
        UsbInfo GetInfo = new UsbInfo();
        Command HidCMD = new Command();
        UsbInfo.Info info = new UsbInfo.Info();
        Invoke _invoke = null;
        Adjust a = new Adjust();

        bool TE_BZP;


        public static double LFB = -100;
        public static double LFF = -100;
        public static double RFB = -100;
        public static double RFF = -100;
        public static double AMBL = -100;
        public static double AMBR = -100;


        #endregion
        public string Run(object[] Command)
        {

            SplitCMD(Command, out string[] CMD);
            if (CMD[1].Contains("HID"))
                OpenDevice();
            try
            {
                switch (CMD[1])
                {
                    //#####################################  HID  !!!!!!!!!!!!!!!!!!!!!!!!!!

                    #region HID Test

                    case "GetDevicePath": return GetDevicePath(CMD[2], CMD[3], int.Parse(CMD[4]));

                    case "HID_Get_BD_Address": return HID_Get_BD_Address(CMD[2]);

                    case "HID_SendReport": return HID_SendReport(CMD[2], CMD[3]);
                    case "HID_Get_ANC_Gain": return HID_Get_ANC_Gain(CMD[2], CMD[3]);
                    case "HID_SwitchANC": return HID_SwitchANC(CMD[2], CMD[3]);
                    case "HID_InitGain": return HID_InitGain(double.Parse(CMD[2]), double.Parse(CMD[3]), CMD[4]);
                    case "HID_SaveANC_Gain": return HID_SaveANC_Gain(CMD[2], CMD[3]);
                    case "HID_CalibrationANC_FB": return HID_CalibrationANC_FB(CMD[2]);
                    case "HID_CalibrationANC_FF": return HID_CalibrationANC_FF(CMD[2]);
                    case "HID_FactoryReset": return HID_FactoryReset();

                    case "HID_SelectVolumeCalibrationANC_FB":
                        return HID_SelectVolumeCalibrationANC_FB(CMD[2], CMD[3]);
                    case "HID_SelectVolumeCalibrationANC_FF":
                        return HID_SelectVolumeCalibrationANC_FF(CMD[2], CMD[3]);

                    case "HID_SelectTargetVolumeAirohaANCAdjustFF_ForCurves":
                        return HID_SelectTargetVolumeAirohaANCAdjustFF_ForCurves(CMD[2], CMD[3], CMD[4], int.Parse(CMD[5]));
                    case "HID_SelectTargetVolumeAirohaANCAdjustFB_ForCurves":
                        return HID_SelectTargetVolumeAirohaANCAdjustFB_ForCurves(CMD[2], CMD[3], CMD[4], int.Parse(CMD[5]));

                    case "HID_STVA_ANCA_FF_FC_Fixed_once":
                        return HID_STVA_ANCA_FF_FC_Fixed_once(CMD[2], CMD[3], CMD[4], int.Parse(CMD[5]));
                    case "HID_STVA_ANCA_FB_FC_Fixed_once":
                        return HID_STVA_ANCA_FB_FC_Fixed_once(CMD[2], CMD[3], CMD[4], int.Parse(CMD[5]));

                    #endregion

                    //#####################################  SPP  !!!!!!!!!!!!!!!!!!!!!!!!!!

                    #region SPP部分

                    case "BT2200_OpenPort": return BT2200_OpenPort();
                    case "BT2200_ColsePort": return BT2200_ColsePort();
                    case "BT2200_SendCMD": return BT2200_SendCMD(CMD[2]);
                    case "SPP_Connect": return SPP_Connect(CMD[2], int.Parse(CMD[3]));
                    case "SPP_SwitchANC": return SPP_SwitchANC(CMD[2]);
                    case "SPP_InitGain": return SPP_InitGain(double.Parse(CMD[2]), double.Parse(CMD[3]));
                    case "SPP_SetGain": return SPP_SetGain();
                    case "SPP_SaveANC_Gain": return SPP_SaveANC_Gain(CMD[2]);
                    case "SPP_Get_ANC_Gain": return SPP_Get_ANC_Gain(CMD[2]);

                    case "SPP_CalibrationANC_FB": return SPP_CalibrationANC_FB(CMD[2]);
                    case "SPP_CalibrationANC_FF": return SPP_CalibrationANC_FF(CMD[2]);

                    case "SPP_SelectVolumeCalibrationANC_FB": return SPP_SelectVolumeCalibrationANC_FB(CMD[2], CMD[3]);
                    case "SPP_SelectVolumeCalibrationANC_FF": return SPP_SelectVolumeCalibrationANC_FF(CMD[2], CMD[3]);

                    case "SPP_STVA_ANCA_FF_FC_Fixed_once": return SPP_STVA_ANCA_FF_FC_Fixed_once(CMD[2], CMD[3], CMD[4], int.Parse(CMD[5]));
                    case "SPP_STVA_ANCA_FB_FC_Fixed_once": return SPP_STVA_ANCA_FB_FC_Fixed_once(CMD[2], CMD[3], CMD[4], int.Parse(CMD[5]));

                    case "SPP_SendCMD": return SPP_SendCMD(CMD[2], CMD[3]);
                    case "BT2200_Disconnect": return BT2200_Disconnect();
                    #endregion




                    default:
                        return "Air Command Error False";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return $"{ex.Message} False";
            }
            finally
            {
                CloseDevice();
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
            string SN = OnceConfig.ContainsKey("SN") ? OnceConfig["SN"].ToString() : Config["SN"].ToString();
            TE_BZP = SN.Contains("TE_BZP");
            CMD = listCMD.ToArray();

        }

        //#####################################  HID  !!!!!!!!!!!!!!!!!!!!!!!!!!
        #region HID


        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ USB HID 测试区 AB156x/AB157x/AB158x ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string Cut_OffRule____()
        {
            return "True";
        }
        #region 指令常规类

        string TargetDevice = "Both";
        /// <summary isPublicTestItem="true">
        /// 连接USB装置
        /// </summary>
        /// <param name="VID">产品VID</param>
        /// <param name="PID">产品PID</param>
        /// <param name="TimeOut">超时 秒 常规“15”</param>
        /// <returns></returns>
        public string GetDevicePath(string VID, string PID, int TimeOut)
        {
            LFB = -100;
            LFF = -100;
            RFB = -100;
            RFF = -100;
            AMBL = -100;
            AMBR = -100;
            Func<bool> func = new Func<bool>(() =>
            {
                return GetHD.GetHidDevicePath(PID, VID, 62, 62);
            });
            bool Result = MerryTest.testitem.ProgressBars.CountDown(func, $"正在识别装置\r\nPID:{PID}  VID:{VID} ", "提示", TimeOut);
            if (Result)
            {
                GetInfo.GetDeviceInfo(GetHD.Path, out info);
                return info.ProductName;

            }

            return "Time Out False";

        }
        /// <summary isPublicTestItem="true">
        /// HID 下指令
        /// </summary>
        /// <param name="CMD">指令</param>
        /// <param name="ContainsReport">判断的返回值 自动判断可输入“null”</param>
        /// 
        /// <returns></returns>
        public string HID_SendReport(string CMD, string ContainsReport)
        {

            if (ContainsReport == "null")
                ContainsReport = "05 5B";
            return SendReport(CMD, ContainsReport);
        }
        /// <summary isPublicTestItem="true">
        /// HID 读取BD号
        /// </summary>
        /// <param name="TargetDevice" options="Master,Slave">选择IC 常规“Master”</param>
        /// <returns></returns>
        public string HID_Get_BD_Address(string TargetDevice)
        {
            string device = "00";
            switch (TargetDevice)
            {
                case "Master":
                    device = "00";
                    break;
                case "Slave":
                    device = "80";
                    break;
                default:
                    return $"Select TargetDevice Error {false}";
            }
            string report = SendReport($"06 07 {device} 05 5A 03 00 D5 0C 00", $"07 0E {device} 05 5B 0A 00 D5 0C 00", "16 15 14 13 12 11");
            if (report.Contains("False")) return report;
            return report.Replace(" ", "");

        }

        /// <summary isPublicTestItem="true">
        /// HID 读取 Gain
        /// </summary>
        /// <param name="TargetDevice" options="Master,Slave">选择IC 常规“Master”</param>
        /// <param name="GainType" options="FB,FF,All">读取全部 Gain 选择“All”</param>
        /// <returns></returns>
        public string HID_Get_ANC_Gain(string TargetDevice, string GainType)
        {
            string device = "00";
            switch (TargetDevice)
            {
                case "Master":
                    device = "00";
                    break;
                case "Slave":
                    device = "80";
                    break;
                default:
                    return $"Select TargetDevice Error {false}";
            }
            string report = SendReport($"06 08 {device} 05 5A 04 00 06 0E 00 0D", "07 10 00 05 5B 0C 00 06 0E 00 0D", "11 12 13 14 15 16 17 18");
            if (report.Contains("False"))
                return report;

            string[] Gain = report.Split(' ');
            LFF = $"{Gain[0]}{Gain[1]}".HEXGainToGain();
            LFB = $"{Gain[2]}{Gain[3]}".HEXGainToGain();
            RFF = $"{Gain[4]}{Gain[5]}".HEXGainToGain();
            RFB = $"{Gain[6]}{Gain[7]}".HEXGainToGain();
            switch (GainType)
            {
                case "FB":
                    return $"L FB：{LFB} R FB：{RFB}";
                case "FF":
                    return $"L FF：{LFF} R FF：{RFF}";
                case "All":
                    return $"L FB：{LFB} R FB：{RFB} _ L FF：{LFF} R FF：{RFF}";
                default:
                    return $"Select GainType Error {false}";
            }
        }

        /// <summary isPublicTestItem="true">
        /// HID 切换 ANC
        /// </summary>
        /// <param name="TargetDevice" options="Master,Slave">选择IC 常规“Master”</param>
        /// <param name="ANCMode" options="ANC_OFF,Hybrid,FF_only,FB_only,Airo_Thru">ANC 的模式</param>
        /// <returns></returns>
        public string HID_SwitchANC(string TargetDevice, string ANCMode)
        {
            string Mode = "";
            string cmd;
            string ResultPort;
            string device = "00";
            string Report = $"Not Send CMD {false}";
            switch (TargetDevice)
            {
                case "Master":
                    device = "00";
                    break;
                case "Slave":
                    device = "80";
                    break;
                default:
                    return $"Select TargetDevice Error {false}";
            }
            switch (ANCMode)
            {
                case "ANC_OFF":
                    cmd = $"06 0B {device} 05 5A 05 00 06 0E 00 0B 01";
                    ResultPort = $"05 5B 08 00 06 0E 00 0B 01";
                    return SendReport(cmd, ResultPort);
                case "Hybrid":
                    Mode = "00";
                    break;
                case "FF_only":
                    Mode = "01";
                    break;
                case "FB_only":
                    Mode = "02";
                    break;
                case "Airo_Thru":
                    cmd = $"06 0B {device} 05 5A 07 00 06 0E 00 0A 09 04 01";
                    ResultPort = $"05 5B 08 00 06 0E 00 0A 09";
                    return SendReport(cmd, ResultPort);
                default:
                    return "ANC Command Error False";
            }
            cmd = $"06 0B {device} 05 5A 07 00 06 0E 00 0A 01 {Mode} 01";
            ResultPort = $"05 5B 08 00 06 0E 00 0A 01 {Mode} 01";
            Report = SendReport(cmd, ResultPort);
            Thread.Sleep(250);

            return Report;

        }

        /// <summary isPublicTestItem="true">
        /// HID 写Gain 写入缓冲区 
        /// </summary>
        /// <param name="FBGain" >初始化 FB Gain 常规0</param>
        /// <param name="FFGain" >初始化 FF Gain 常规0</param>
        /// <param name="TargetDevice" options="Master,Slave,Both">选择IC 常规“Master”</param>
        /// <returns></returns>
        string HID_InitGain(double FBGain, double FFGain, string TargetDevice)
        {
            if (TE_BZP)
                return "标准品";
            this.TargetDevice = TargetDevice;
            string device = "00";
            string Report = $"Not Send CMD {false}";
            LFB = FBGain;
            LFF = FFGain;
            RFB = FBGain;
            RFF = FFGain;
            switch (TargetDevice)
            {
                case "Master":
                    device = "00";
                    break;
                case "Slave":
                    device = "80";
                    break;
                case "Both":
                    SendReport($"06 10 00 05 5A 0C 00 06 0E 00 0C F6 FF F6 FF F6 FF F6 FF", "05 5B 0C 00 06 0E 00");
                    SendReport($"06 10 80 05 5A 0C 00 06 0E 00 0C F6 FF F6 FF F6 FF F6 FF", "05 5B 0C 00 06 0E 00");
                    Thread.Sleep(200);
                    Report = SendReport($"06 10 00 05 5A 0C 00 06 0E 00 0C 00 00 00 00 00 00 00 00", "05 5B 0C 00 06 0E 00");
                    if (Report.Contains("False"))
                        return $"Master {Report}";
                    Report = SendReport($"06 10 80 05 5A 0C 00 06 0E 00 0C 00 00 00 00 00 00 00 00", "05 5B 0C 00 06 0E 00");
                    if (Report.Contains("False"))
                        return $"Slave {Report}";
                    return Report;
                default:
                    return $"Select TargetDevice Error {false}";
            }
            SendReport($"06 10 {device} 05 5A 0C 00 06 0E 00 0C F6 FF F6 FF F6 FF F6 FF", "05 5B 0C 00 06 0E 00");
            Thread.Sleep(200);
            Report = SendReport($"06 10 {device} 05 5A 0C 00 06 0E 00 0C {LFF.VolToHEXGain()} {LFB.VolToHEXGain()} {RFF.VolToHEXGain()} {RFB.VolToHEXGain()}", "05 5B 0C 00 06 0E 00");
            return $"{TargetDevice} {Report}";

        }

        /// <summary isPublicTestItem="false">
        ///  HID 将缓冲区增益“Gain”写值
        /// </summary>
        /// <param name="TargetDevice" options="Master,Slave,Both">选择IC 常规“Master”</param>
        /// <returns></returns>
        public string HID_SetGain(string TargetDevice)
        {
            try
            {
                if (TE_BZP)
                    return "标准品";
                string device = "00";
                string Report = $"Not Send CMD {false}";
                double dLFF = LFF;
                double dLFB = LFB;
                double dRFF = RFF;
                double dRFB = RFB;
                Report = SendReport($"06 10 {device} 05 5A 0C 00 06 0E 00 0C {dLFF.VolToHEXGain()} {dLFB.VolToHEXGain()} {dRFF.VolToHEXGain()} {dRFB.VolToHEXGain()}", "05 5B 0C 00 06 0E 00");
                Thread.Sleep(300);
                if (Report.Contains("False"))
                    return $"{TargetDevice} {Report}";
                return $" LFB：“{LFB}” LFF：“{LFF}” RFB：“{RFB}” RFF：“{RFF}”:{TargetDevice}";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"HID_SetGain LFF:{LFF}| LFB:{LFB}| dRFF:{RFF}|RFB:{RFB} \r\n{ex}");
                return $"LFF:{LFF}| LFB:{LFB}| dRFF:{RFF}|RFB:{RFB} \r\n{ex.Message}";
            }
            finally
            {
                Thread.Sleep(500);

            }

        }

        /// <summary isPublicTestItem="true">
        /// HID 保存Gain 将Gain保存在IC
        /// </summary>
        /// <param name="TargetDevice" options="Master,Slave,Both">选择IC 常规“Master”</param>
        /// <param name="ShowTyoe" options="FB,FF,ALL">FB和FF都会写入 选择显示的Gain</param>
        /// <returns></returns>
        string HID_SaveANC_Gain(string TargetDevice, string ShowTyoe)
        {
            try
            {
                if (TE_BZP)
                    return "标准品";
                double dLFF = LFF;
                double dLFB = LFB;
                double dRFF = RFF;
                double dRFB = RFB;
                string device = "00";
                string Report = $"Not Send CMD {false}";
                switch (TargetDevice)
                {
                    case "Master":
                        device = "00";
                        break;
                    case "Slave":
                        device = "80";
                        break;
                    case "Both":
                        Report = SendReport($"06 10 00 05 5A 0C 00 06 0E 00 0E {dLFF.VolToHEXGain()} {dLFB.VolToHEXGain()} {dRFF.VolToHEXGain()} {dRFB.VolToHEXGain()}", "05 5B 0C 00 06 0E 00 0E");
                        if (Report.Contains("False"))
                        {
                            return $"Master {Report}";
                        }
                        Report = SendReport($"06 10 80 05 5A 0C 00 06 0E 00 0E {dLFF.VolToHEXGain()} {dLFB.VolToHEXGain()} {dRFF.VolToHEXGain()} {dRFB.VolToHEXGain()}", "05 5B 0C 00 06 0E 00 0E");
                        if (Report.Contains("False"))
                        {
                            return $"Slave {Report}";
                        }
                        return Report;

                    default:
                        return $"Select TargetDevice Error {false}";
                }
                Report = SendReport($"06 10 {device} 05 5A 0C 00 06 0E 00 0E {dLFF.VolToHEXGain()} {dLFB.VolToHEXGain()} {dRFF.VolToHEXGain()} {dRFB.VolToHEXGain()}", "05 5B 0C 00 06 0E 00 0E");
                if (Report.Contains("False"))
                {
                    return $"{TargetDevice} {Report}";
                }

                switch (ShowTyoe)
                {
                    case "FB":
                        return $"LFB：“{LFB}” RFB：“{RFB}”:{TargetDevice} ";

                    case "FF":
                        return $"LFF：“{LFF}” RFF：“{RFF}”:{TargetDevice} ";

                    case "ALL":
                    default:
                        return $" LFB：“{LFB}” RFB：“{RFB}” LFF：“{LFF}” RFF：“{RFF}”:{TargetDevice}";

                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"LFF:{LFF}|RFF:{RFF}|LFB:{LFB}|RFB:{RFB}\r\n{ex}");
                return $"{ex.Message} False";


            }


        }

        #endregion


        #region ANC类



        //############################################################ 立讯 已过时   ###########################################################################

        string TargetS = "";
        bool isShowAdjustLog;
        /// <summary isPublicTestItem="false">
        /// HID 校准 ANC FB 首选“Pass” 次级目标感度
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <param name="TargetFreq" >次级 目标频率  "{200:-6}{300:-5.8}{400:-20.8}" </param>
        /// <returns></returns>
        public string HID_SelectVolumeCalibrationANC_FB(string ShowCalibration, string TargetFreq)
        {

            TargetS = TargetFreq;
            //委托方法名
            string FuncName = "SelectVolumeAirohaANCAdjustFB_ForCurves";
            //测试PASS标识
            string PassFileName = "FB Test Result.txt";
            //SC弹窗是否要校准的弹窗
            string CurvesDataFileName = "FB Test Curves.txt";
            //SC弹窗是否要校准的弹窗
            string FormsName = "Correction or not_FB";
            //测试结束标识
            string FinalTestResult = "Final Test Result.txt";
            Func<bool, string> FB_ForCurves = (SelectVolumeAirohaANCAdjustFB_ForCurves);
            Config[FuncName] = FB_ForCurves;

            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves&FuncName={FuncName}&PassFileName={PassFileName}&CurvesDataFileName={CurvesDataFileName}&FormsName={FormsName}&ShowCalibration={ShowCalibration}&FinalTestResult={FinalTestResult}", null);



        }
        /// <summary isPublicTestItem="false">
        /// HID 校准 ANC FF 首选“Pass” 次级目标感度
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <param name="TargetFreq" >次级 目标频率  "{200:-6}{300:-5.8}{400:-20.8}" </param>
        /// <returns></returns>
        public string HID_SelectVolumeCalibrationANC_FF(string ShowCalibration, string TargetFreq)
        {

            TargetS = TargetFreq;


            List<double> freqs = new List<double>();
            foreach (var item in TargetFreq.Split('|'))
                if (double.TryParse(item, out double result))
                    freqs.Add(result.Round(1));
            //委托方法名
            string FuncName = "SelectVolumeAirohaANCAdjustFF_ForCurves";
            //测试PASS标识
            string PassFileName = "ANC Test Result.txt";
            //测试的曲线数据
            string CurvesDataFileName = "ANC Test Curves.txt";
            //SC弹窗是否要校准的弹窗
            string FormsName = "Correction or not_ANC";
            //测试结束标识
            string FinalTestResult = "Final Test Result.txt";
            Func<bool, string> FB_ForCurves = (SelectVolumeAirohaANCAdjustFF_ForCurves);
            Config[FuncName] = FB_ForCurves;
            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves&FuncName={FuncName}&PassFileName={PassFileName}&CurvesDataFileName={CurvesDataFileName}&FormsName={FormsName}&ShowCalibration={ShowCalibration}&FinalTestResult={FinalTestResult}", null);



        }

        string SelectVolumeAirohaANCAdjustFB_ForCurves(bool isShowAdjustLog)
        {

            try
            {
                string result;
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);
                a.ReadCardinal(Config["Name"].ToString(), true);
                result = a.SelectVolumeANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, 1, "FB",
                    TargetS,
                    ref LFB, ref RFB);
                if (result.Contains("False"))
                    return result;
                HID_SetGain(TargetDevice);
                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"002 FB 报错{ex}");
                return $"{ex.Message} False";
            }
        }
        string SelectVolumeAirohaANCAdjustFF_ForCurves(bool isShowAdjustLog)
        {
            try
            {
                string result;
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);
                a.ReadCardinal(Config["Name"].ToString(), false);
                result = a.SelectVolumeANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, 1, "FF",
                    TargetS,
                    ref LFF, ref RFF);
                if (result.Contains("False"))
                    return result;
                HID_SetGain(TargetDevice);
                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"002 FF 报错{ex}");
                return $"{ex.Message} False";
            }


        }




        //############################################################ 立讯 已过时   ###########################################################################

        #region 已经抛弃
        /// <summary isPublicTestItem="false">
        /// HID 校准 ANC FB 首选“Pass” 次级目标平衡
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <returns></returns>
        public string HID_CalibrationANC_FB(string ShowCalibration)
        {
            //委托方法名
            string FuncName = "AirohaANCAdjustFB_ForCurves";
            //测试PASS标识
            string PassFileName = "FB Test Result.txt";
            //SC弹窗是否要校准的弹窗
            string CurvesDataFileName = "FB Test Curves.txt";
            //SC弹窗是否要校准的弹窗
            string FormsName = "Correction or not_FB";
            //测试结束标识
            string FinalTestResult = "Final Test Result.txt";
            Func<bool, string> FB_ForCurves = (AirohaANCAdjustFB_ForCurves);
            Config[FuncName] = FB_ForCurves;

            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves&FuncName={FuncName}&PassFileName={PassFileName}&CurvesDataFileName={CurvesDataFileName}&FormsName={FormsName}&ShowCalibration={ShowCalibration}&FinalTestResult={FinalTestResult}", null);



        }
        /// <summary isPublicTestItem="false">
        /// HID 校准 ANC FF 首选“Pass” 次级目标平衡
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <returns></returns>
        public string HID_CalibrationANC_FF(string ShowCalibration)
        {
            //委托方法名
            string FuncName = "AirohaANCAdjustFF_ForCurves";
            //测试PASS标识
            string PassFileName = "ANC Test Result.txt";
            //测试的曲线数据
            string CurvesDataFileName = "ANC Test Curves.txt";
            //SC弹窗是否要校准的弹窗
            string FormsName = "Correction or not_ANC";
            //测试结束标识
            string FinalTestResult = "Final Test Result.txt";
            Func<bool, string> FB_ForCurves = (AirohaANCAdjustFF_ForCurves);
            Config[FuncName] = FB_ForCurves;
            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves&FuncName={FuncName}&PassFileName={PassFileName}&CurvesDataFileName={CurvesDataFileName}&FormsName={FormsName}&ShowCalibration={ShowCalibration}&FinalTestResult={FinalTestResult}", null);



        }

        string AirohaANCAdjustFB_ForCurves(bool isShowAdjustLog)
        {
            try
            {
                string result;
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);
                a.ReadCardinal(Config["Name"].ToString(), true);
                result = a.ANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, 1, "FB",
                    ref LFB, ref RFB);
                if (result.Contains("False"))
                    return result;
                HID_SetGain(TargetDevice);
                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"001 FB 报错{ex}");
                return $"{ex.Message} False";
            }

        }
        string AirohaANCAdjustFF_ForCurves(bool isShowAdjustLog)
        {
            try
            {

                string result;
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);
                a.ReadCardinal(Config["Name"].ToString(), false);
                result = a.ANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, 1, "FF",
                     ref LFF, ref RFF);
                if (result.Contains("False"))
                    return result;
                HID_SetGain(TargetDevice);
                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"001 FF 报错{ex}");
                return $"{ex.Message} False";
            }


        }

        /// <summary isPublicTestItem="false">
        /// HID 高级 ANC FF 首选“Pass” 次选左耳目标 右耳平衡
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <param name="LeftTarget" >左边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5} {300:-5.4:1} {400:-3.2}"</param>
        /// <param name="RightTarget" >右边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5} {300:-5.4:1} {400:-3.2}"</param>
        /// <param name="TestCount" >校准次数 要跟SC匹配 常规“5”</param>
        /// 
        /// <returns></returns>
        public string HID_SelectTargetVolumeAirohaANCAdjustFF_ForCurves(string ShowCalibration, string LeftTarget, string RightTarget, int TestCount)
        {

            this.LeftTarget = LeftTarget;
            this.RightTarget = RightTarget;
            OnceAdjust = true;
            isShowAdjustLog = bool.Parse(ShowCalibration);
            //委托方法名
            string FuncName = "SelectTargetVolumeAirohaANCAdjustFF_ForCurves";

            Func<string> ForCurves = (SelectTargetVolumeAirohaANCAdjustFF_ForCurves);
            Config[FuncName] = ForCurves;
            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves_2&FuncName={FuncName}&PassFileName={PassFileName_FF}&CurvesDataFileName={CurvesDataFileName_FF}&FormsName={FormsName_FF}&FinalTestResult={FinalTestResult}&FinalTestResult={TestCount}", null);
        }
        string SelectTargetVolumeAirohaANCAdjustFF_ForCurves()
        {
            try
            {
                lastFF_LGain = (LFF);
                lastFF_RGain = (RFF);
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);

                string result = a2.ANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, Config["Name"].ToString(),
                    LeftTarget.TargetStrDic(),
                    RightTarget.TargetStrDic(),
                    "FF", out bool NGCurvesif,
                    ref LFF, ref RFF);
                if (result.Contains("False"))
                    return result;
                if (!NGCurvesif && !OnceAdjust)
                {
                    double LDiff = (lastFF_LGain.abs() - LFF.abs()).abs();
                    double RDiff = (lastFF_RGain.abs() - RFF.abs()).abs();
                    if (LDiff <= 0.2 && RDiff <= 0.2)
                    {
                        LFF = lastFF_LGain;
                        RFF = lastFF_RGain;
                        return "Not Adjust";
                    }
                }
                HID_SetGain(TargetDevice);
                OnceAdjust = false;
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error=> A301 FF 报错{ex}");
                return $"{ex.Message} False";
            }


        }

        /// <summary isPublicTestItem="false">
        /// HID 高级 ANC FB 首选“Pass” 次选左耳目标 右耳平衡
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <param name="LeftTarget" >左边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5} {300:-5.4:1} {400:-3.2}"</param>
        /// <param name="RightTarget" >右边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5} {300:-5.4:1} {400:-3.2}"</param>
        /// <param name="TestCount" >校准次数 要跟SC匹配 常规“5”</param>
        /// 
        /// <returns></returns>
        public string HID_SelectTargetVolumeAirohaANCAdjustFB_ForCurves(string ShowCalibration, string LeftTarget, string RightTarget, int TestCount)
        {
            this.LeftTarget = LeftTarget;
            this.RightTarget = RightTarget;
            OnceAdjust = true;
            isShowAdjustLog = bool.Parse(ShowCalibration);
            //委托方法名
            string FuncName = "SelectTargetVolumeAirohaANCAdjustFB_ForCurves";

            Func<string> ForCurves = (SelectTargetVolumeAirohaANCAdjustFB_ForCurves);
            Config[FuncName] = ForCurves;
            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves_2&FuncName={FuncName}&PassFileName={PassFileName_FB}&CurvesDataFileName={CurvesDataFileName_FB}&FormsName={FormsName_FB}&FinalTestResult={FinalTestResult}&FinalTestResult={TestCount}", null);
        }
        string SelectTargetVolumeAirohaANCAdjustFB_ForCurves()
        {
            try
            {
                lastFF_LGain = (LFF);
                lastFF_RGain = (RFF);
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);

                string result = a2.ANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, Config["Name"].ToString(),
                    LeftTarget.TargetStrDic(),
                    RightTarget.TargetStrDic(),
                    "FB", out bool NGCurvesif,
                    ref LFF, ref RFF);
                if (result.Contains("False"))
                    return result;
                if (NGCurvesif && !OnceAdjust)
                {
                    double LDiff = (lastFF_LGain.abs() - LFF.abs()).abs();
                    double RDiff = (lastFF_RGain.abs() - RFF.abs()).abs();
                    if (LDiff <= 0.2 && RDiff <= 0.2)
                    {
                        LFF = lastFF_LGain;
                        RFF = lastFF_RGain;
                        return "Not Adjust";
                    }
                }
                HID_SetGain(TargetDevice);
                OnceAdjust = false;
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error=> A301 FF 报错{ex}");
                return $"{ex.Message} False";
            }


        }
        #endregion






        #endregion
        //############################################################### 高级方法 ########################################################################

        #region  ANC


        //测试PASS标识
        const string PassFileName_FB = "FB Test Result.txt";
        const string PassFileName_FF = "ANC Test Result.txt";

        //测试的曲线数据
        const string CurvesDataFileName_FF = "ANC Test Curves.txt";
        const string CurvesDataFileName_FB = "FB Test Curves.txt";
        //SC弹窗是否要校准的弹窗

        const string FormsName_FF = "Correction or not_ANC";
        const string FormsName_FB = "Correction or not_FB";

        //测试结束标识
        const string FinalTestResult = "Final Test Result.txt";




        AdjustClass2 a2 = new AdjustClass2();
        //左边的目标值
        string LeftTarget = "";
        //右边的目标值
        string RightTarget = "";
        //左边上一个Gain值
        double lastFF_LGain = 0;
        //右边上一个Gain值
        double lastFF_RGain = 0;
        //左边上一个Gain值
        double lastFB_LGain = 0;
        //右边上一个Gain值
        double lastFB_RGain = 0;
        //第一次开始测试的标识
        bool OnceAdjust;
        int FixedCount;
        int TestCount;


        AdjustClass3 a3 = new AdjustClass3();
        /// <summary isPublicTestItem="true">
        /// HID 校准 FF 立讯 固定次数
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <param name="LeftTarget" >左边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5}"</param>
        /// <param name="RightTarget" >右边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5}"</param>
        /// <param name="FixedCount" >跟SC对应 固定校准次数“4”</param>
        /// <returns></returns>
        public string HID_STVA_ANCA_FF_FC_Fixed_once(string ShowCalibration, string LeftTarget, string RightTarget, int FixedCount)
        //   HID_SelectTargetVolumeAirohaANCAdjustFF_ForCurves_Fixed_once
        {
            this.FixedCount = FixedCount + 1;

            this.TestCount = 0;
            this.LeftTarget = LeftTarget;
            this.RightTarget = RightTarget;
            a3.infos.Clear();
            OnceAdjust = true;
            isShowAdjustLog = bool.Parse(ShowCalibration);
            //委托方法名
            string FuncName = "STVA_ANCAdjustFF_ForCurves_Fixed_once";

            Func<string> ForCurves = (STVA_ANCAdjustFF_ForCurves_Fixed_once);
            Config[FuncName] = ForCurves;
            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves_2&FuncName={FuncName}&PassFileName={PassFileName_FF}&CurvesDataFileName={CurvesDataFileName_FF}&FormsName={FormsName_FF}&FinalTestResult={FinalTestResult}&FinalTestResult={this.FixedCount}", null);
        }
        string STVA_ANCAdjustFF_ForCurves_Fixed_once()
        {
            try
            {

                TestCount++;
                lastFF_LGain = (LFF);
                lastFF_RGain = (RFF);
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);

                string result = a3.ANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, Config["Name"].ToString(),
                    LeftTarget.TargetStrDic(),
                    RightTarget.TargetStrDic(),
                    "FF", out bool NGCurvesif,
                    ref LFF, ref RFF);
                if (result.Contains("False"))
                    return result;

                if (FixedCount - 1 == TestCount)
                {
                    a3.CheckInfos(
                        isShowAdjustLog,
                        LeftTarget.TargetStrDic(),
                        RightTarget.TargetStrDic(),
                        ref LFF, ref RFF);

                }
                HID_SetGain(TargetDevice);
                // MessageBox.Show("停一下");
                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error=> A301 FF 报错{ex}");
                return $"{ex.Message} False";
            }


        }

        /// <summary isPublicTestItem="true">
        /// HID 校准 FB 立讯 固定次数
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <param name="LeftTarget" >左边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5}"</param>
        /// <param name="RightTarget" >右边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5}"</param>
        /// <param name="FixedCount" >跟SC对应 固定校准次数“4”</param>
        /// <returns></returns>
        public string HID_STVA_ANCA_FB_FC_Fixed_once(string ShowCalibration, string LeftTarget, string RightTarget, int FixedCount)
        {
            this.FixedCount = FixedCount + 1;

            this.TestCount = 0;
            this.LeftTarget = LeftTarget;
            this.RightTarget = RightTarget;
            a3.infos.Clear();
            OnceAdjust = true;
            isShowAdjustLog = bool.Parse(ShowCalibration);
            //委托方法名
            string FuncName = "STVA_ANCAdjustFB_ForCurves_Fixed_once";

            Func<string> ForCurves = (STVA_ANCAdjustFB_ForCurves_Fixed_once);
            Config[FuncName] = ForCurves;
            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves_2&FuncName={FuncName}&PassFileName={PassFileName_FB}&CurvesDataFileName={CurvesDataFileName_FB}&FormsName={FormsName_FB}&FinalTestResult={FinalTestResult}&FinalTestResult={this.FixedCount}", null);
        }
        string STVA_ANCAdjustFB_ForCurves_Fixed_once()
        {
            try
            {

                TestCount++;
                lastFB_LGain = (LFB);
                lastFB_RGain = (LFB);
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);

                string result = a3.ANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, Config["Name"].ToString(),
                    LeftTarget.TargetStrDic(),
                    RightTarget.TargetStrDic(),
                    "FB", out bool NGCurvesif,
                    ref LFB, ref RFB);
                if (result.Contains("False"))
                    return result;

                if (FixedCount - 1 == TestCount)
                {
                    a3.CheckInfos(
                        isShowAdjustLog,
                        LeftTarget.TargetStrDic(),
                        RightTarget.TargetStrDic(),
                        ref LFB, ref RFB);

                }
                HID_SetGain(TargetDevice);

                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error=> A302 FB 报错{ex}");
                return $"{ex.Message} False";
            }


        }


        #endregion



        #region 指令辅助类

        /// <summary isPublicTestItem="true">
        /// HID 工厂重置 Factory Reset
        /// </summary>
        /// <returns></returns>
        public string HID_FactoryReset()
        {
            int Count = 6;
            for (int i = 0; i < 1; i++)
            {
                string str = SendReport("06 08 00 05 5a 04 00 01 11 95 00 00 00 00 00 00", "");
                if (str.Contains("False"))
                    return str;

                for (int j = 0; i < Count; j++)
                {
                    if (!GetHD.gethandle(info.ProductID, info.VendorID, ""))
                        return "True";
                    if (i < Count - 1)
                    {
                        Thread.Sleep(500);

                    }
                }
            }
            return "False";
        }
        void OpenDevice()
        {
            GetInfo.GetHandle(ref info);
            //Airoha 芯片使用Get Report 方法会有缓冲区数据的问题， 开始前先读几次 后面下指令会比较正常
            if (info.Handle != IntPtr.Zero)
            {
                for (int i = 0; i < 2; i++)
                {
                    HidCMD.GetReportReturn("07", info.I, info.Handle, "0");
                }
            }


        }
        void CloseDevice()
            => GetInfo.CloseHandle(ref info.Handle);
        string SendReport(string cmd, string ContainsReport)
        {
            return SendReport(cmd, ContainsReport, out _);
        }
        string SendReport(string cmd, string LastIndexOfStr, string ValueIndex)
        {
            string Value = "";
            string Report = SendReport(cmd, LastIndexOfStr, out string AllValue, true);
            if (Report.Contains("False"))
                return Report;
            string[] valus = AllValue.Split(' ');
            foreach (var item in ValueIndex.Split(' '))
            {
                if (item.Trim().Length <= 0)
                    continue;
                Value += $"{valus[int.Parse(item)]} ";
            }
            Value = Value.Trim();
            return Value;


        }
        string SendReport(string cmd, string CheckReportStr, out string AllValue, bool LastIndexOf = false)
        {
            int Count = 5;
            AllValue = "";
            for (int i = 0; i < Count; i++)
            {
                if (HidCMD.SetReportSend(cmd, info.I, info.Handle))
                {
                    HidCMD.GetReportReturn("07", info.I, info.Handle, "0");
                    AllValue = HidCMD.ALLReturnValue;
                    if (LastIndexOf)
                    {
                        string CheckStr = HidCMD.ALLReturnValue.Substring(0, CheckReportStr.Length);
                        if (CheckStr.Trim() == CheckReportStr.Trim())
                            return "True";
                    }
                    else
                    {
                        if (HidCMD.ALLReturnValue.Contains(CheckReportStr))
                            return "True";
                    }

                };
                if (i < Count - 1)
                {
                    Thread.Sleep(100);
                }
            }
            return $"Send CMD {info.Handle} False";
        }
        #endregion


        #endregion

        //#####################################  SPP  !!!!!!!!!!!!!!!!!!!!!!!!!!
        #region BT2200

        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ SPP 测试区 AB156x/AB157x/AB158x ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string Cut_OffRule___()
        {
            return "True";
        }

        public static SerialPort PORT = null;

        /// <summary isPublicTestItem="false">
        /// 切换 ANC模式
        /// </summary>
        /// <param name="ANCMode" options="ANC_OFF,Hybrid,FF_only,FB_only,Airo_Thru">ANC 的模式</param>
        /// <returns></returns>
        public string _SPP_SwitchANC(string ANCMode)
        {

            string Mode = "";
            string Report = $"Not Send CMD {false}";
            switch (ANCMode)
            {
                case "ANC_OFF":
                    return SPP_SendReport($"05 5A 05 00 06 0E 00 0B 01", $"05 5B 08 00 06 0E 00 0B 01");
                case "Hybrid":
                    Mode = "00";
                    break;
                case "FF_only":
                    Mode = "01";
                    break;
                case "FB_only":
                    Mode = "02";
                    break;
                case "Airo_Thru":
                    return SPP_SendReport($"05 5A 07 00 06 0E 00 0A 09 04 01", $"05 5B 08 00 06 0E 00 0A 09");

                default:
                    return "ANC Command Error False";
            }
            string cmd = $"05 5A 07 00 06 0E 00 0A 01 {Mode} 01";
            string ResultPort = $"05 5B 08 00 06 0E 00 0A 01 {Mode} 01";
            Report = SPP_SendReport(cmd, ResultPort);
            Dictionary<string, object> dict = new Dictionary<string, object>();

            if (Report.Contains("False"))
            {
                dict["cmdCompleted"] = false;
                dict["MSG"] = $"切换模式失败 原因“XXXXXX” {Report}";
            }
            else
            {
                dict["cmdCompleted"] = true;
                dict["MSG"] = $"";
                Thread.Sleep(100);

            }
            return $"{JsonConvert.SerializeObject(dict)}";



        }

        /// <summary isPublicTestItem="false">
        /// 读取 Gain
        /// </summary>
        /// <returns></returns>
        public string _SPP_Get_ANC_Gain()
        {

            string report = SPP_SendReport($"05 5A 04 00 06 0E 00 0D", "05 5B 0C 00 06 0E 00 0D", "08 09 10 11 12 13 14 15");
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (report.Contains("False"))
            {
                dict = new Dictionary<string, object>()
                    {
                        {"FB_Gain_L",-100 },
                        {"FB_Gain_R",-100 },
                        {"FF_Gain_L",-100 },
                        {"FF_Gain_R",-100 },
                        {"MSG",$"读取Gain失败  原因“XXXXXX” {report}" },
                        {"cmdCompleted",false },
                    };
            }
            else
            {
                string[] Gain = report.Split(' ');
                double LFF = $"{Gain[0]}{Gain[1]}".HEXGainToGain();
                double LFB = $"{Gain[2]}{Gain[3]}".HEXGainToGain();
                double RFF = $"{Gain[4]}{Gain[5]}".HEXGainToGain();
                double RFB = $"{Gain[6]}{Gain[7]}".HEXGainToGain();
                dict = new Dictionary<string, object>()
                    {
                        {"FB_Gain_L",LFB},
                        {"FB_Gain_R",RFB},
                        {"FF_Gain_L",LFF},
                        {"FF_Gain_R",RFF},
                        {"cmdCompleted",true },

                    };
            }
            return JsonConvert.SerializeObject(dict);
        }

        /// <summary isPublicTestItem="false">
        ///  将缓冲区增益“Gain”写值
        /// </summary>
        /// <returns></returns>
        public string _SPP_SetGain(string JsonGain)
        {
            JToken Resul = JsonConvert.DeserializeObject<JToken>(JsonGain);
            string Report = $"Not Send CMD {false}";
            double LFB = Resul.Value<double>("FB_Gain_L");
            double RFB = Resul.Value<double>("FB_Gain_R");
            double LFF = Resul.Value<double>("FF_Gain_L");
            double RFF = Resul.Value<double>("FF_Gain_R");
            string MSG = Resul.Value<string>("MSG");//Calibration_FB / Calibration_FF / Calibration_Init

            //SPP_SendReport($"05 5A 0C 00 06 0E 00 0c 18 fc 18 fc 18 fc 18 fc", "05 5B 0C 00 06 0E 00");
            Thread.Sleep(50);
            Report = SPP_SendReport($"05 5A 0C 00 06 0E 00 0C {LFF.VolToHEXGain()} {LFB.VolToHEXGain()} {RFF.VolToHEXGain()} {RFB.VolToHEXGain()}", "05 5B 0C 00 06 0E 00");
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (Report.Contains("False"))
            {
                dict["cmdCompleted"] = false;
                dict["MSG"] = $"设置缓存Gain失败 原因 “XXXXX” {Report}";
            }
            else
            {
                dict["cmdCompleted"] = true;
                Thread.Sleep(100);
            }
            return JsonConvert.SerializeObject(dict);

        }

        /// <summary isPublicTestItem="false">
        /// 保存Gain 将Gain保存在IC
        /// </summary>
        /// <returns></returns>
        public string _SPP_SaveANC_Gain(string JsonGain)
        {
            JToken Resul = JsonConvert.DeserializeObject<JToken>(JsonGain);
            string Report = $"Not Send CMD {false}";
            double LFB = Resul.Value<double>("FB_Gain_L");
            double RFB = Resul.Value<double>("FB_Gain_R");
            double LFF = Resul.Value<double>("FF_Gain_L");
            double RFF = Resul.Value<double>("FF_Gain_R");
            string MSG = Resul.Value<string>("MSG");//Calibration_FB  /  Calibration_FF
            Report = SPP_SendReport($"05 5A 0C 00 06 0E 00 0E {LFF.VolToHEXGain()} {LFB.VolToHEXGain()} {RFF.VolToHEXGain()} {RFB.VolToHEXGain()}", "05 5B 0C 00 06 0E 00 0E");
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (Report.Contains("False"))
            {
                dict["cmdCompleted"] = false;
                dict["MSG"] = $"保存Gain失败 原因 “XXXXX” {Report}"; ;
            }
            else
            {
                dict["cmdCompleted"] = true;
            }
            string CMD = JsonConvert.SerializeObject(dict);
            return $"{CMD}";


        }












        /// <summary isPublicTestItem="true">
        /// BT2200 下指令
        /// </summary>
        /// <param name="CMD">指令 例如 >OPEN A2DP</param>
        /// <returns></returns>
        public string BT2200_SendCMD(string CMD)
        {
            ReadLog(out _, out _);
            WriteLog($"{CMD}\r\n");
            ReadLog(out _, out _);
            return "True";
        }

        /// <summary isPublicTestItem="true">
        /// SPP 建立连接 标准品默认盲连
        /// </summary>
        /// <param name="ManualFlag" options="True,False">True 为指定连接地址在Config["BitAddress"] 或 False 盲连</param>
        /// <param name="Count">配对次数 每次大约10秒 建议2次</param>
        /// <returns>配对成功的BD号</returns>
        public string SPP_Connect(string ManualFlag, int Count)
        {
            if (PORT == null)
            {
                BT2200_OpenPort();
                WriteLog(">SET_SPP_UUID=35111C00000000000000000099AABBCCDDEEFF");
                ReadLog(out _, out _);
                Thread.Sleep(100);
                WriteLog(">DISC");
                ReadLog(out _, out _);
                Thread.Sleep(1000);
                WriteLog(">RST");
                ReadLog(out _, out _);
            }
            PortLog.AddRange(new string[] { "", "", "", "#####################SPP_Connect#####################"
            });
            LFB = -100;
            LFF = -100;
            RFB = -100;
            RFF = -100;
            AMBL = -100;
            AMBR = -100;
            //Extension.WriteGain((string)this.Config["SqcPath"], LFB, RFB, LFF, RFF, AMBL, AMBR);
            string ConnectCMD = ">NO_MAC_CON";
            string readStr = "";
            string BD = "True";
            bool IsConnect = false;
            if (ManualFlag == "True")
            {
                string BD_ = Config["BitAddress"].ToString();
                if (BD_.Length != 12)
                    MessageBox.Show($"BD号长度不等于12码  {BD_}", "BT2200类提示");
                ConnectCMD = TE_BZP ? $">CONN={BD_}" : ConnectCMD;
            }
            if (!PORT.IsOpen) PORT.Open();

            for (int d = 0; d < Count; d++)
            {
                WriteLog(ConnectCMD);
                Thread.Sleep(2000);
                for (int i = 0; i < 8; i++)
                {
                    ReadLog(out readStr, out _);

                    if (readStr.Contains("DEVICE="))
                    {
                        string[] strs = readStr.Split(new string[] { "DEVICE=" }, StringSplitOptions.None);
                        BD = strs[1].Substring(0, 12);
                    }
                    if (readStr.Contains("Success") ||
                        readStr.Contains("avrcp_success") ||
                        readStr.Contains("a2dp_connect_success")
                        )
                    {
                        IsConnect = true;
                        break;
                    }
                    Thread.Sleep(1000);
                }
                if (IsConnect)
                    break;
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
            ReadLog(out readStr, out _);
            for (int i = 0; i < 3; i++)
            {
                WriteLog(">SPP_CONN");
                Thread.Sleep(1000);
                ReadLog(out readStr, out _);
                if (readStr.Contains("SPP_CONNECTED"))
                {
                    IsConnect = true;
                    WriteLog(">GET_CONN_INFO");
                    Thread.Sleep(1000);

                    ReadLog(out readStr, out _);
                    if (readStr.Contains("DEVICE="))
                    {
                        string[] strs = readStr.Split(new string[] { "DEVICE=" }, StringSplitOptions.None);
                        BD = strs[1].Substring(0, 12).ToUpper();
                    }
                    return BD;

                }
                Thread.Sleep(1000);
            }
            return $"{readStr} Time Out False";


        }
        /// <summary isPublicTestItem="true">
        /// SPP 下SPP指令
        /// </summary>
        /// <param name="CMD">指令 16进制 例如 ANC OFF 05 5A 05 00 06 0E 00 0B 01</param>
        /// <param name="ContainsValue">检查指令返回值 例如 05 5B 08 00 06 0E 00 0B 01</param>
        /// <returns></returns>
        public string SPP_SendCMD(string CMD, string ContainsValue)
        {
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 2; i++)
            {
                WriteLog($"{CMD}".HexStrToBytes());
                ReadLog(out _, out ByteStr);
                if (ByteStr.Contains(ContainsValue)) return "True";
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }

        /// <summary isPublicTestItem="true">
        /// SPP 读取 Gain
        /// </summary>
        /// <param name="GainType" options="FB,FF,All">读取全部 Gain 选择“All”</param>
        /// <returns></returns>
        public string SPP_Get_ANC_Gain(string GainType)
        {

            string report = SPP_SendReport($"05 5A 04 00 06 0E 00 0D", "05 5B 0C 00 06 0E 00 0D", "08 09 10 11 12 13 14 15");
            if (report.Contains("False"))
                return report;

            string[] Gain = report.Split(' ');
            LFF = $"{Gain[0]}{Gain[1]}".HEXGainToGain();
            LFB = $"{Gain[2]}{Gain[3]}".HEXGainToGain();
            RFF = $"{Gain[4]}{Gain[5]}".HEXGainToGain();
            RFB = $"{Gain[6]}{Gain[7]}".HEXGainToGain();
            Extension.WriteGain((string)this.Config["SqcPath"], LFB, RFB, LFF, RFF, AMBL, AMBR);
            switch (GainType)
            {
                case "FB":
                    return $"LFB：“{LFB}” RFB：“{RFB}”";

                case "FF":
                    return $"LFF：“{LFF}” RFF：“{RFF}”";

                case "ALL":
                default:
                    return $" LFB：“{LFB}” RFB：“{RFB}” LFF：“{LFF}” RFF：“{RFF}”";

            }
        }

        /// <summary isPublicTestItem="true">
        /// SPP 切换 ANC
        /// </summary>
        /// <param name="ANCMode" options="ANC_OFF,Hybrid,FF_only,FB_only,Airo_Thru">ANC 的模式</param>
        /// <returns></returns>
        public string SPP_SwitchANC(string ANCMode)
        {

            string Mode = "";
            string Report = $"Not Send CMD {false}";
            switch (ANCMode)
            {
                case "ANC_OFF":
                    return SPP_SendReport($"05 5A 05 00 06 0E 00 0B 01", $"05 5B 08 00 06 0E 00 0B 01");
                case "Hybrid":
                    Mode = "00";
                    break;
                case "FF_only":
                    Mode = "01";
                    break;
                case "FB_only":
                    Mode = "02";
                    break;
                case "Airo_Thru":
                    return SPP_SendReport($"05 5A 07 00 06 0E 00 0A 09 04 01", $"05 5B 08 00 06 0E 00 0A 09");

                default:
                    return "ANC Command Error False";
            }
            string cmd = $"05 5A 07 00 06 0E 00 0A 01 {Mode} 01";
            string ResultPort = $"05 5B 08 00 06 0E 00 0A 01 {Mode} 01";
            Report = SPP_SendReport(cmd, ResultPort);
            Thread.Sleep(250);

            return Report;


        }
        /// <summary isPublicTestItem="true">
        /// SPP 写Gain 写入缓冲区 
        /// </summary>
        /// <param name="FBGain" >初始化 FB Gain 常规0</param>
        /// <param name="FFGain" >初始化 FF Gain 常规0</param>
        /// <returns></returns>
        public string SPP_InitGain(double FBGain, double FFGain)
        {
            if (TE_BZP)
            {

                return $"标准品 {SPP_Get_ANC_Gain("All")}";

            }
            string Report = $"Not Send CMD {false}";
            LFB = FBGain;
            LFF = FFGain;
            RFB = FBGain;
            RFF = FFGain;
            SPP_SaveANC_Gain("ALL");
            Thread.Sleep(500);
            SPP_SendReport($"05 5A 0C 00 06 0E 00 0C 18 fc 18 fc 18 fc 18 fc", "05 5B 0C 00 06 0E 00");
            Thread.Sleep(50);
            Report = SPP_SendReport($"05 5A 0C 00 06 0E 00 0C {LFF.VolToHEXGain()} {LFB.VolToHEXGain()} {RFF.VolToHEXGain()} {RFB.VolToHEXGain()}", "05 5B 0C 00 06 0E 00");
            return $"{Report}";

        }
        /// <summary isPublicTestItem="false">
        ///  SPP 将缓冲区增益“Gain”写值
        /// </summary>
        /// <returns></returns>
        public string SPP_SetGain()
        {
            if (TE_BZP)
                return "标准品";
            SPP_SendReport($"05 5A 0C 00 06 0E 00 0c 18 fc 18 fc 18 fc 18 fc", "05 5B 0C 00 06 0E 00");
            Thread.Sleep(50);
            string Report = SPP_SendReport($"05 5A 0C 00 06 0E 00 0C {LFF.VolToHEXGain()} {LFB.VolToHEXGain()} {RFF.VolToHEXGain()} {RFB.VolToHEXGain()}", "05 5B 0C 00 06 0E 00");
            Thread.Sleep(200);
            if (Report.Contains("False"))
                return $"{Report}";
            return $" LFB：“{LFB}” RFB：“{RFB}” LFF：“{LFF}” RFF：“{RFF}”";

        }

        /// <summary isPublicTestItem="true">
        /// SPP 保存Gain 将Gain保存在IC
        /// </summary>
        /// <param name="ShowType" options="FB,FF,ALL">FB和FF都会写入 选择显示的Gain</param>
        /// <returns></returns>
        public string SPP_SaveANC_Gain(string ShowType)
        {
            try
            {
                if (TE_BZP)
                    return "标准品";
                double dLFF = LFF;
                double dLFB = LFB; ;


                double dRFF = RFF;
                double dRFB = RFB;

                string Report = $"Not Send CMD {false}";

                Report = SPP_SendReport($"05 5A 0C 00 06 0E 00 0E {dLFF.VolToHEXGain()} {dLFB.VolToHEXGain()} {dRFF.VolToHEXGain()} {dRFB.VolToHEXGain()}", "05 5B 0C 00 06 0E 00 0E");
                if (Report.Contains("False"))
                {
                    return $"{Report}";
                }
                switch (ShowType)
                {
                    case "FB":
                        return $"LFB：“{LFB}” RFB：“{RFB}”";

                    case "FF":
                        return $"LFF：“{LFF}” RFF：“{RFF}”";

                    case "ALL":
                    default:
                        return $" LFB：“{LFB}” RFB：“{RFB}” LFF：“{LFF}” RFF：“{RFF}”";

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"LFF:{LFF}|RFF:{RFF}|LFB:{LFB}|RFB:{RFB}\r\n{ex}");

                return $"{ex.Message} False";

            }


        }

        #region 已经抛弃
        /// <summary isPublicTestItem="false">
        /// SPP 校准 ANC FB 首选“Pass” 次级目标平衡
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <returns></returns>
        public string SPP_CalibrationANC_FB(string ShowCalibration)
        {
            //委托方法名
            string FuncName = "SPP_AirohaANCAdjustFB_ForCurves";
            //测试PASS标识
            string PassFileName = "FB Test Result.txt";
            //SC弹窗是否要校准的弹窗
            string CurvesDataFileName = "FB Test Curves.txt";
            //SC弹窗是否要校准的弹窗
            string FormsName = "Correction or not_FB";
            //测试结束标识
            string FinalTestResult = "Final Test Result.txt";
            Func<bool, string> FB_ForCurves = (SPP_AirohaANCAdjustFB_ForCurves);
            Config[FuncName] = FB_ForCurves;

            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves&FuncName={FuncName}&PassFileName={PassFileName}&CurvesDataFileName={CurvesDataFileName}&FormsName={FormsName}&ShowCalibration={ShowCalibration}&FinalTestResult={FinalTestResult}", null);



        }
        /// <summary isPublicTestItem="false">
        /// SPP 校准 ANC FF 首选“Pass” 次级目标平衡
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <returns></returns>
        public string SPP_CalibrationANC_FF(string ShowCalibration)
        {

            //委托方法名
            string FuncName = "SPP_AirohaANCAdjustFF_ForCurves";
            //测试PASS标识
            string PassFileName = "ANC Test Result.txt";
            //测试的曲线数据
            string CurvesDataFileName = "ANC Test Curves.txt";
            //SC弹窗是否要校准的弹窗
            string FormsName = "Correction or not_ANC";
            //测试结束标识
            string FinalTestResult = "Final Test Result.txt";
            Func<bool, string> FB_ForCurves = (SPP_AirohaANCAdjustFF_ForCurves);
            Config[FuncName] = FB_ForCurves;
            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves&FuncName={FuncName}&PassFileName={PassFileName}&CurvesDataFileName={CurvesDataFileName}&FormsName={FormsName}&ShowCalibration={ShowCalibration}&FinalTestResult={FinalTestResult}", null);



        }
        string SPP_AirohaANCAdjustFB_ForCurves(bool isShowAdjustLog)
        {
            try
            {

                string result;
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);
                a.ReadCardinal(Config["Name"].ToString(), true);

                result = a.ANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, 0.1, "FB",
                    ref LFB, ref RFB);
                if (result.Contains("False"))
                    return result;
                SPP_SetGain();
                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"003 FB 报错{ex}");
                return $"{ex.Message} False";
            }

        }
        string SPP_AirohaANCAdjustFF_ForCurves(bool isShowAdjustLog)
        {
            try
            {

                string result;
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);
                a.ReadCardinal(Config["Name"].ToString(), false);
                result = a.ANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, 0.1, "FF",
                    ref LFF, ref RFF);
                if (result.Contains("False"))
                    return result;
                SPP_SetGain();
                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"003 FF 报错{ex}");
                return $"{ex.Message} False";
            }


        }
        #endregion


        #region   已经抛弃
        //############################################################ 立讯 已过时   ###########################################################################

        /// <summary isPublicTestItem="false">
        /// SPP 校准 ANC FB 首选“Pass” 次级目标感度
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <param name="TargetFreq" >次级 目标频率 用分隔符 “|” </param>
        /// <returns></returns>
        public string SPP_SelectVolumeCalibrationANC_FB(string ShowCalibration, string TargetFreq)
        {
            TargetS = TargetFreq;
            //委托方法名
            string FuncName = "SPP_SelectVolumeAirohaANCAdjustFB_ForCurves";
            //测试PASS标识
            string PassFileName = "FB Test Result.txt";
            //SC弹窗是否要校准的弹窗
            string CurvesDataFileName = "FB Test Curves.txt";
            //SC弹窗是否要校准的弹窗
            string FormsName = "Correction or not_FB";
            //测试结束标识
            string FinalTestResult = "Final Test Result.txt";
            Func<bool, string> FB_ForCurves = (SPP_SelectVolumeAirohaANCAdjustFB_ForCurves);
            Config[FuncName] = FB_ForCurves;

            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves&FuncName={FuncName}&PassFileName={PassFileName}&CurvesDataFileName={CurvesDataFileName}&FormsName={FormsName}&ShowCalibration={ShowCalibration}&FinalTestResult={FinalTestResult}", null);



        }

        /// <summary isPublicTestItem="false">
        /// SPP 校准 ANC FF 首选“Pass” 次级目标感度
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <param name="TargetFreq" >次级 目标频率 用分隔符 “|” </param>
        /// <returns></returns>
        public string SPP_SelectVolumeCalibrationANC_FF(string ShowCalibration, string TargetFreq)
        {

            TargetS = TargetFreq;

            //委托方法名
            string FuncName = "SPP_SelectVolumeAirohaANCAdjustFF_ForCurves";
            //测试PASS标识
            string PassFileName = "ANC Test Result.txt";
            //测试的曲线数据
            string CurvesDataFileName = "ANC Test Curves.txt";
            //SC弹窗是否要校准的弹窗
            string FormsName = "Correction or not_ANC";
            //测试结束标识
            string FinalTestResult = "Final Test Result.txt";
            Func<bool, string> FB_ForCurves = (SPP_SelectVolumeAirohaANCAdjustFF_ForCurves);
            Config[FuncName] = FB_ForCurves;
            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves&FuncName={FuncName}&PassFileName={PassFileName}&CurvesDataFileName={CurvesDataFileName}&FormsName={FormsName}&ShowCalibration={ShowCalibration}&FinalTestResult={FinalTestResult}", null);



        }


        string SPP_SelectVolumeAirohaANCAdjustFB_ForCurves(bool isShowAdjustLog)
        {
            try
            {
                int FB_Calibration = 0;

                string result;
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);
                a.ReadCardinal(Config["Name"].ToString(), true);
                result = a.SelectVolumeANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, 1, "FB",
                    TargetS,
                  ref LFB, ref RFB);
                if (result.Contains("False"))
                    return result;

                SPP_SetGain();
                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"004 FB 报错\r\n{ex}");
                return $"{ex.Message} False";
            }

        }

        string SPP_SelectVolumeAirohaANCAdjustFF_ForCurves(bool isShowAdjustLog)
        {
            try
            {
                int FF_Calibration = 0;

                string result;
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);
                a.ReadCardinal(Config["Name"].ToString(), false);
                result = a.SelectVolumeANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, 1, "FF",
                    TargetS,
                   ref LFF, ref RFF);
                if (result.Contains("False"))
                    return result;
                SPP_SetGain();
                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"004 FF 报错\r\n{ex}");
                return $"{ex.Message} False";
            }
        }

        #endregion

        /// <summary isPublicTestItem="true">
        /// SPP 校准 FB 立讯 固定次数
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <param name="LeftTarget" >左边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5}"</param>
        /// <param name="RightTarget" >右边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5}"</param>
        /// <param name="FixedCount" >固定校准次数“4”</param>
        /// <returns></returns>
        public string SPP_STVA_ANCA_FB_FC_Fixed_once(string ShowCalibration, string LeftTarget, string RightTarget, int FixedCount)

        {
            this.FixedCount = FixedCount + 1;

            this.TestCount = 0;
            this.LeftTarget = LeftTarget;
            this.RightTarget = RightTarget;
            a3.infos.Clear();
            OnceAdjust = true;
            isShowAdjustLog = bool.Parse(ShowCalibration);
            //委托方法名
            string FuncName = "SPP_STVA_ANCAdjustFB_ForCurves_Fixed_once";

            Func<string> ForCurves = (SPP_STVA_ANCAdjustFB_ForCurves_Fixed_once);
            Config[FuncName] = ForCurves;
            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves_2&FuncName={FuncName}&PassFileName={PassFileName_FB}&CurvesDataFileName={CurvesDataFileName_FB}&FormsName={FormsName_FB}&FinalTestResult={FinalTestResult}&FinalTestResult={this.FixedCount}", null);
        }
        string SPP_STVA_ANCAdjustFB_ForCurves_Fixed_once()
        {
            try
            {

                TestCount++;
                lastFB_LGain = (LFB);
                lastFB_RGain = (RFB);
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);

                string result = a3.ANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, Config["Name"].ToString(),
                    LeftTarget.TargetStrDic(),
                    RightTarget.TargetStrDic(),
                    "FB", out bool NGCurvesif,
                    ref LFB, ref RFB);
                if (result.Contains("False"))
                    return result;

                if (FixedCount - 1 == TestCount)
                {
                    a3.CheckInfos(
                        isShowAdjustLog,
                        LeftTarget.TargetStrDic(),
                        RightTarget.TargetStrDic(),
                        ref LFB, ref RFB);
                }
                SPP_SetGain();

                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error=> A301 FB 报错{ex}");
                return $"{ex.Message} False";
            }


        }

        /// <summary isPublicTestItem="true">
        /// SPP 校准 FF 立讯 固定次数
        /// </summary>
        /// <param name="ShowCalibration" options="False,True">是否显示算法过程 想看选“True”</param>
        /// <param name="LeftTarget" >左边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5}"</param>
        /// <param name="RightTarget" >右边目标 {频率:感度:容错范围} "{530:-3.5:0.5} {200:-5.5:0.5}"</param>
        /// <param name="FixedCount" >固定校准次数“5”</param>
        /// <returns></returns>
        public string SPP_STVA_ANCA_FF_FC_Fixed_once(string ShowCalibration, string LeftTarget, string RightTarget, int FixedCount)
        {
            this.FixedCount = FixedCount + 1;
            this.TestCount = 0;
            this.LeftTarget = LeftTarget;
            this.RightTarget = RightTarget;
            a3.infos.Clear();

            OnceAdjust = true;
            isShowAdjustLog = bool.Parse(ShowCalibration);
            //委托方法名
            string FuncName = "SPP_STVA_ANCAdjustFF_ForCurves_Fixed_once";

            Func<string> ForCurves = (SPP_STVA_ANCAdjustFF_ForCurves_Fixed_once);
            Config[FuncName] = ForCurves;
            return _invoke.CallMethod
                ($"dllname=SoundCheck_V1&method=Calibration_ANC_ByCurves_2&FuncName={FuncName}&PassFileName={PassFileName_FF}&CurvesDataFileName={CurvesDataFileName_FF}&FormsName={FormsName_FF}&FinalTestResult={FinalTestResult}&FinalTestResult={this.FixedCount}", null);
        }
        string SPP_STVA_ANCAdjustFF_ForCurves_Fixed_once()
        {
            try
            {

                TestCount++;
                lastFF_LGain = (LFF);
                lastFF_RGain = (RFF);
                string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);

                string result = a3.ANCAdjust_ForCurves(
                    FilePath,
                    isShowAdjustLog, Config["Name"].ToString(),
                    LeftTarget.TargetStrDic(),
                    RightTarget.TargetStrDic(),
                    "FF", out bool NGCurvesif,
                    ref LFF, ref RFF);
                if (result.Contains("False"))
                    return result;

                if (FixedCount - 1 == TestCount)
                {
                    a3.CheckInfos(
                        isShowAdjustLog,
                        LeftTarget.TargetStrDic(),
                        RightTarget.TargetStrDic(),
                        ref LFF, ref RFF);
                }

                SPP_SetGain();
                return "True";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error=> A301 FF 报错{ex}");
                return $"{ex.Message} False";
            }


        }










        /// <summary isPublicTestItem="true">
        /// 断开连接 重置仪器 释放缓存 生成Log
        /// </summary>
        /// <returns></returns>
        public string BT2200_Disconnect()
        {

            string readStr = "";
            bool BT2200Error1 = false;
            bool BT2200Error2 = false;
            for (int i = 0; i < 4; i++)
            {
                WriteLog(">DISC");
                Thread.Sleep(250);
                ReadLog(out readStr, out _);
                if (readStr.Contains("IDLE"))
                {
                    BT2200Error1 = false;
                    break;
                }
                BT2200Error1 = true;
            }
            Thread.Sleep(100);
            for (int i = 0; i < 4; i++)
            {
                WriteLog(">RST");
                Thread.Sleep(250);

                ReadLog(out readStr, out _);
                if (readStr.Contains("OK"))
                {
                    BT2200Error2 = false;
                    break;
                }
                Thread.Sleep(250);
                BT2200Error2 = true;

            }
            ReadLog(out _, out _);
            PortLog.AddRange(new string[] { "", "", "#####################Disconnect#####################", "" });
            Directory.CreateDirectory(".\\LOG");
            File.AppendAllLines($".\\LOG\\时间{DateTime.Now.ToString("MM月dd日")}SPP Log.txt", PortLog.ToArray());
            PortLog.Clear();
            if (PORT.IsOpen) PORT.Close();
            PORT.Dispose();
            if (BT2200Error1 || BT2200Error2)
            {
                PORT.Dispose();
                PORT = null;
                MessageBox.Show("BT2200 蓝牙适配器异常，插拔适配器USB线需要3次，每次插拔间隔2秒", "Airoha提示");
                return "Error True";
            }
            return "True";
        }


        /// <summary isPublicTestItem="true">
        /// BT2200 打开串口
        /// </summary>
        /// <returns></returns>
        public string BT2200_OpenPort()
        {
            if (PORT == null)
            {
                string PortName = "COM256";
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
                {
                    var portnames = SerialPort.GetPortNames();
                    var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
                    var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();
                    foreach (string s in portList) if (s.Contains("Prolific USB-to-Serial Comm Port")) PortName = s.Substring(0, s.IndexOf(" "));
                }
                if (PortName == "COM256")
                    throw new Exception("通訊埠 'COM256' 不存在。");
                PORT = new SerialPort
                {
                    PortName = PortName,
                    BaudRate = 921600,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Parity = Parity.None
                };

            }
            if (!PORT.IsOpen) PORT.Open();
            return PORT.IsOpen.ToString();
        }
        /// <summary isPublicTestItem="true">
        /// BT2200 关闭串口
        /// </summary>
        /// <returns></returns>
        public string BT2200_ColsePort()
        {
            if (PORT.IsOpen) PORT.Close();
            return (!PORT.IsOpen).ToString();
        }


        public static List<string> PortLog = new List<string>();
        public void WriteLog(string Command)
        {
            BT2200_OpenPort();
            PORT.WriteLine($@"{Command}");
            PORT.NewLine = "\r\n";
            PortLog.Add($"########{DateTime.Now}  |  Write  |  {Command}\r\n");
            Thread.Sleep(150);
        }
        public void WriteLog(byte[] Command)
        {
            BT2200_OpenPort();
            PORT.Write(Command, 0, Command.Length);
            string log = "";
            foreach (var item in Command) log += $"{item.ToString("X2").PadLeft(2, '0')}";
            PortLog.Add($"{DateTime.Now}  |  Write  |  HEX：{log}\r\n");
            Thread.Sleep(250);
        }
        public void ReadLog(out string ReadStr, out string byteStr)
        {
            BT2200_OpenPort();
            ReadStr = "";
            byteStr = "";
            if (PORT.BytesToRead > 0)
            {
                byte[] ReadByte = null;
                byteStr = "";
                ReadByte = new byte[PORT.BytesToRead];
                PORT.Read(ReadByte, 0, ReadByte.Length);
                List<byte> asciiList = new List<byte>();
                foreach (var item in ReadByte)
                {
                    byteStr += $"{item.ToString("X2").PadLeft(2, '0')} ";
                    if ((item >= 33&& item <= 125)||item==10)
                        asciiList.Add(item);

                }
                ReadStr = Encoding.ASCII.GetString(asciiList.ToArray());
                PortLog.Add($"{DateTime.Now}  |  Read  |  ASCII：{ReadStr}  |  HEX： {byteStr}\r\n");
            }
        }
        string SPP_SendReport(string Command, string ContainsReport)
        {
            return SPP_SendReport(Command, ContainsReport, out _);


        }
        string SPP_SendReport(string Command, string LastIndexOfStr, string ValueIndex)
        {
            string Value = "";
            string Report = SPP_SendReport(Command, LastIndexOfStr, out string AllValue);
            if (Report.Contains("False"))
                return Report;
            string[] valus = AllValue.Split(' ');
            foreach (var item in ValueIndex.Split(' '))
            {
                if (item.Trim().Length <= 0)
                    continue;
                Value += $"{valus[int.Parse(item)]} ";
            }
            Value = Value.Trim();
            return Value;

        }
        string SPP_SendReport(string Command, string ContainsReport, out string AllValue)
        {
            ReadLog(out _, out _);
            AllValue = "";
            for (int i = 0; i < 5; i++)
            {
                WriteLog(Command.HexStrToBytes());
                ReadLog(out _, out AllValue);
                if (AllValue.Contains(ContainsReport))
                {
                    return "True";
                }
                Thread.Sleep(50);

            }
            return $"{AllValue} False";

        }
        #endregion




    }
    public static class Extension
    {
        public static byte[] HexStrToBytes(this string str)
        {
            List<byte> bytes = new List<byte>();
            foreach (var item in str.Trim().Split(' '))
            {
                bytes.Add(Convert.ToByte(item, 16));
            }
            return bytes.ToArray();
        }

        public static double HEXGainToGain(this string _HEXGain)
        {
            string HEXGain = "";
            _HEXGain = _HEXGain.Replace(" ", "");
            for (int i = _HEXGain.Length - 2; i >= 0; i -= 2)
            {
                HEXGain += _HEXGain.Substring(i, 2);
            }

            int GainValue = Convert.ToInt32(HEXGain, 16);
            if (GainValue <= 0x258)
            {
                double dB = GainValue / 100.0;
                return Math.Round(dB, 1);
            }
            else
            {
                double dB = (GainValue - 0x10000) / 100.0;
                return Math.Round(dB, 1);
            }


        }
        public static string VolToHEXGain(this double GainValue)
        {
            string HEXGain = "";
            string _HEXGain = "";
            if (GainValue >= 0)
            {
                HEXGain = Convert.ToString((int)(GainValue * 100.0), 16).PadLeft(4, '0');
            }
            else
            {
                int g = (int)((GainValue * 100.0) + 0x10000);
                HEXGain = Convert.ToString(g, 16).PadLeft(4, '0');
            }
            _HEXGain = $"{HEXGain.Substring(2, 2)} {HEXGain.Substring(0, 2)}";
            return _HEXGain;

        }

        public static void WriteGain(string DicPath, double FBL, double FBR, double FFL, double FFR, double AMBL, double AMBR)
        {
            Task.Run(() =>
            {
                string FilePath = $"{Path.GetDirectoryName(DicPath)}\\Gain.csv";
                string Gain = $"100,200,300,400,500,600,\r\n{FBL},{FBR},{FFL},{FFR},{AMBL},{AMBR}";
                for (int i = 0; i < 6; i++)
                {
                    try
                    {
                        File.WriteAllText(FilePath, Gain);
                        return;
                    }
                    catch { Thread.Sleep(500); }
                }
            });

        }


    }
}
