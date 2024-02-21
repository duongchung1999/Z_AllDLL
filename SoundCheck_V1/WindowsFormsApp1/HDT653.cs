using MerryTest.testitem;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using static System.Windows.Forms.MessageBox;
using TestItem;
using HDT653.TestItem;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Windows.Forms;
using static MerryTest.testitem.Data;

namespace MerryDllFramework
{
    public class MerryDll : IMerryDll
    {

        #region 主模板的方法接口
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：HDT653";
            string dllfunction = "Dll功能说明 ：HDT653";
            string dllHistoryVersion = "历史Dll版本：0.0.0.1";
            string dllVersion = "当前Dll版本：MP 0.0.0.2";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo3 = "MP 0.0.0.1：第一版开发程序";
            string[] info = { dllname,
                dllfunction,
                dllHistoryVersion,
                dllVersion,
                dllChangeInfo, dllChangeInfo3
       };
            return info;
        }

        TestMethod test = new TestMethod();
        GetHandle GH = new GetHandle();
        Command SendCMD = new Command();
        Data data = new Data();
        /// <summary>
        /// 平台程序共享的参数
        /// </summary>
        Dictionary<string, object> Config = new Dictionary<string, object>();
        /// <summary>
        /// 连扳程序特有的单线程平台共享参数
        /// </summary>
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        /// <summary>
        /// 用于判断是连扳还是单板程序
        /// </summary>
        bool MoreTestFlag;
        bool TE_BZP
        {
            get
            {
                return this.Config["SN"].ToString().Contains("TE_BZP");
            }
        }
        public object Interface(Dictionary<string, object> Config) => this.Config = Config;


        public bool StartRun()
        {
            /*
               单板开始测试是触发方法
               写下你的代码 
            */
            MoreTestFlag = false;

            return true;
        }

        public bool StartTest(Dictionary<string, object> OnceConfig)
        {
            /*
               连扳程序当开始测试后触发方法  OnceConfig是线程独立参数
               写下你的代码 
               Console.WriteLine("Hello Word");
            */
            this.OnceConfig = OnceConfig;
            MoreTestFlag = true;

            return true;
        }
        public void TestsEnd(object obj)
        {
            /*
                连扳程序当所有线程测试结束后触发方法
                写下你的代码 
                Console.WriteLine("Hello Word");
           */
        }
        #endregion

        public bool Start(List<string> formsData, IntPtr _handel)

        {
            /*
                程序启动是触发方法
                写下你的代码 
                Console.WriteLine("Hello Word");
             */
            //料号
            //string OrderNumberInformation = (string)Config["OrderNumberInformation"];
            ////工单
            //string OrderNumber = (string)Config["Works"];
            ////根据料号索引的后台的参数
            //Dictionary<string, string> PartNumberInfos = (Dictionary<string, string>)Config["PartNumberInfos"];
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                var portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
                var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();
                foreach (string s in portList) if (s.Contains("Prolific USB-to-Serial Comm Port")) data.BT2200Port = s.Substring(0, s.IndexOf(" "));
            }

            if (data.DynamicAdjustANC)
            {
                Stopwatch sp = new Stopwatch();
                sp.Start();
                bool FBLFile = File.Exists($@"{data.dllpath}\ANC_AdjustCardinal\L_FB_Curve_data.txt");
                bool FFLFile = File.Exists($@"{data.dllpath}\ANC_AdjustCardinal\L_FF_Curve_data.txt");
                bool FBRFile = File.Exists($@"{data.dllpath}\ANC_AdjustCardinal\R_FB_Curve_data.txt");
                bool FFRFile = File.Exists($@"{data.dllpath}\ANC_AdjustCardinal\R_FF_Curve_data.txt");
                if (!FBLFile)
                    Show("校准基数丢失 未找到 ：L_FB_Curve_data.txt");
                if (!FFLFile)
                    Show("校准基数丢失 未找到 ：L_FF_Curve_data.txt");
                if (!FBRFile)
                    Show("校准基数丢失 未找到 ：R_FB_Curve_data.txt");
                if (!FFRFile)
                    Show("校准基数丢失 未找到 ：R_FF_Curve_data.txt");
                if (!FBLFile || !FFLFile || !FBRFile || !FFRFile)
                {

                    return false;
                }
                string[] FBLData = File.ReadAllLines($@"{data.dllpath}\ANC_AdjustCardinal\L_FB_Curve_data.txt");
                string[] FFLData = File.ReadAllLines($@"{data.dllpath}\ANC_AdjustCardinal\L_FF_Curve_data.txt");
                string[] FBRData = File.ReadAllLines($@"{data.dllpath}\ANC_AdjustCardinal\R_FB_Curve_data.txt");
                string[] FFRData = File.ReadAllLines($@"{data.dllpath}\ANC_AdjustCardinal\R_FF_Curve_data.txt");
                Cardinal.L_FB_CurveGianCardinal = adjustSplit(FBLData);
                Cardinal.L_FF_CurveGianCardinal = adjustSplit(FFLData);
                Cardinal.R_FB_CurveGianCardinal = adjustSplit(FBRData);
                Cardinal.R_FF_CurveGianCardinal = adjustSplit(FFRData);


            }
            if (Config != null)
            {

                Func<string, string> FB_ForCurves = (ANCAdjustFB_ForCurves);
                Config["ANCAdjustFB_ForCurves"] = FB_ForCurves;
                Func<string, string> FF_ForCurves = (ANCAdjustFF_ForCurves);
                Config["ANCAdjustFF_ForCurves"] = FF_ForCurves;

            }

            return true;
        }


        string GainFBL = "-1";
        string GainFBR = "-1";
        string GainFFL = "-1";
        string GainFFR = "-1";

        public string Run(string Command)
        {
            //string SN = (string)Config["SN"];
            //string BD号 = (string)Config["BitAddress"];
            string[] cmd = Command.Split(' ');
            if (cmd[0].Contains("HID"))
            {
                GH.getHandle(data.HeadsetPID, data.HeadsetVID, "&mi_01#");
            }
            try
            {
                switch (cmd[0])
                {
                    case "SPP_Connect": return SPP_Connect(cmd[1], bool.Parse(cmd[2]));
                    case "SPP_Disconnect": return SPP_Disconnect();

                    case "ANC_Off": return ANC_Off();
                    case "ANC_On": return ANC_On();
                    case "FF_only": return FF_only();
                    case "FB_only": return FB_only();
                    case "Ambient_1_On": return Ambient_1_On();

                    case "Limiter_Off": return Limiter_Off();
                    case "Limiter_On_parameter": return Limiter_On_parameter();

                    //###############  FB  ###############
                    case "Get_gain_FBL": return Get_gain_FBL();
                    case "Get_gain_FBR": return Get_gain_FBR();


                    case "ForSNSet_FBL_Gain": return userSet_FBL_Gain(Config["SN"].ToString());
                    case "ForSNSet_FBR_Gain": return userSet_FBR_Gain(Config["SN"].ToString());
                    case "userSet_FBL_Gain": return userSet_FBL_Gain(cmd[1]);
                    case "userSet_FBR_Gain": return userSet_FBR_Gain(cmd[1]);

                    case "Stored_gain_FBL": return Stored_gain_FBL();
                    case "Stored_gain_FBR": return Stored_gain_FBR();


                    //###############  FF  ###############
                    case "Get_gain_FFL": return Get_gain_FFL();
                    case "Get_gain_FFR": return Get_gain_FFR();

                    case "ForSNSet_FFL_Gain": return Set_FFL_Gain(Config["SN"].ToString());
                    case "ForSNSet_FFR_Gain": return Set_FFR_Gain(Config["SN"].ToString());

                    case "userSet_FFL_Gain": return Set_FFL_Gain(cmd[1]);
                    case "userSet_FFR_Gain": return Set_FFR_Gain(cmd[1]);

                    case "Stored_gain_FFL": return Stored_gain_FFL();
                    case "Stored_gain_FFR": return Stored_gain_FFR();
                    case "OpenBT2200Port": return OpenBT2200Port();
                    case "CloseBT2200Port": return CloseBT2200Port();

                    case "HID_Stored_gain": return HID_Stored_gain();

                    default: return "Command Error False";
                }
            }
            catch (Exception EX)
            {

                return $"{EX.Message} False";
            }

        }



        #region SPP Port
        string OpenBT2200Port()
        {
            is_connect = true;
            try
            {
                if (SPP == null)
                {
                    SPP = new SerialPort
                    {
                        PortName = data.BT2200Port,
                        BaudRate = 921600,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        Parity = Parity.None
                    };
                }
                if (!SPP.IsOpen) SPP.Open();
                return "True";
            }
            catch (Exception ex)
            {

                return $"{ex.Message} False";
            }


        }
        string CloseBT2200Port()
        {
            if (SPP.IsOpen) SPP.Close();
            SPP.Dispose();
            return "True";
        }





        bool is_connect;




        SerialPort SPP = null;
        List<string> SPPLog = new List<string>();
        void SPPWriteLog(string Command)
        {
            SPP.WriteLine($@"{Command}");
            SPP.NewLine = "\r\n";
            SPPLog.Add($"########{DateTime.Now}  |  Write  |  {Command}\r\n");
            Thread.Sleep(250);
        }
        void SPPWriteLog(byte[] Command)
        {
            SPP.Write(Command, 0, Command.Length);
            string log = "";
            foreach (var item in Command) log += $"{item.ToString("X2").PadLeft(2, '0')}";
            SPPLog.Add($"{DateTime.Now}  |  Write  |  HEX：{log}\r\n");
            Thread.Sleep(400);
        }
        void SPPReadLog(out string ReadStr, out string byteStr)
        {
            ReadStr = "";
            byteStr = "";
            if (SPP.BytesToRead > 0)
            {
                byte[] ReadByte = null;
                byteStr = "";
                ReadByte = new byte[SPP.BytesToRead];
                SPP.Read(ReadByte, 0, ReadByte.Length);
                foreach (var item in ReadByte) byteStr += $"{item.ToString("X2").PadLeft(2, '0')} ";
                ReadStr = Encoding.ASCII.GetString(ReadByte);
                Console.WriteLine(ReadStr);
                SPPLog.Add($"{DateTime.Now}  |  Read  |  ASCII：{ReadStr}  |  HEX： {byteStr}\r\n");
            }
        }
        string SPP_Connect(string count, bool isconnect)
        {
            is_connect = false;
            SPPLog.Add("");
            SPPLog.Add("");
            SPPLog.Add("");
            SPPLog.Add("#####################SPP_Connect#####################");

            GainFBL = "-1";
            GainFBR = "-1";
            GainFFL = "-1";
            GainFFR = "-1";
            try
            {
                string readStr = "";
                bool connectFlat = false;
                if (SPP == null)
                {
                    SPP = new SerialPort
                    {
                        PortName = data.BT2200Port,
                        BaudRate = 921600,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        Parity = Parity.None
                    };
                    if (!SPP.IsOpen) SPP.Open();
                    SPPWriteLog(">SET_SPP_UUID=35051A00001101");
                    Thread.Sleep(100);
                    SPPWriteLog(">DISC");
                    Thread.Sleep(100);
                    SPPWriteLog(">RST");
                    SPPReadLog(out _, out _);
                }
                string Address = "";
                string ConnectCMD = "";
                if (this.TE_BZP)
                {
                    ConnectCMD = ">NO_MAC_CON";
                }
                else
                {
                    if (isconnect)
                    {
                        Address = (string)Config["BitAddress"];
                        if (Address.Length != 12)
                        {
                            return "BDAddress Length !=12 False";
                        }
                        ConnectCMD = $">CONN={Address}";
                    }
                    else
                    {
                        ConnectCMD = ">NO_MAC_CON";

                    }
                }
                string BD = "True";
                for (int d = 0; d < 2; d++)
                {

                    if (!SPP.IsOpen) SPP.Open();
                    SPPWriteLog(ConnectCMD);
                    Thread.Sleep(2000);
                    for (int i = 0; i < int.Parse(count); i++)
                    {
                        SPPReadLog(out readStr, out _);

                        if (readStr.Contains("DEVICE="))
                        {
                            string[] strs = readStr.Split(new string[] { "DEVICE=" }, StringSplitOptions.None);
                            BD = strs[1].Substring(0, 12);
                        }

                        if (readStr.Contains("Success") || readStr.Contains("success") || readStr.Contains("a2dp_connecting") || readStr.Contains("AVRCP_CONNECTING_REMOTE") || readStr.Contains("AVRCP_CONNECTING_REMOTE"))
                        {


                            connectFlat = true;
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    if (!connectFlat)
                    {
                        if (d == 2)
                            return $"Connect Timeout False";
                        Thread.Sleep(1000);
                        continue;
                    }
                    Thread.Sleep(2000);
                    for (int i = 0; i < 3; i++)
                    {
                        SPPWriteLog(">SPP_CONN");
                        Thread.Sleep(500);
                        SPPReadLog(out readStr, out _);
                        if (readStr.Contains("SPP_CONNECTED") || readStr.Contains("SPP_PENDING"))
                        {
                            is_connect = true;
                            return BD;

                        }

                    }


                }
                return $"{readStr} Timeout False";

            }
            catch (Exception ex)
            {
                SPP?.Dispose();
                SPP = null;
                return $"{ex.Message} False";
            }

        }
        string SPP_Disconnect()
        {

            GainFBL = "-1";
            GainFBR = "-1";
            GainFFL = "-1";
            GainFFR = "-1";
            is_connect = false;
            SPPWriteLog(">DISC");
            Thread.Sleep(200);
            SPPWriteLog(">RST");
            SPPReadLog(out _, out _);
            if (SPP.IsOpen) SPP.Close();
            SPPLog.Add("");
            SPPLog.Add("");
            SPPLog.Add("#####################SPP_Disconnect#####################");
            // SPP.Dispose();
            File.AppendAllLines($".\\LOG\\时间{DateTime.Now.ToString("MM月dd日")}SPP Log.txt", SPPLog.ToArray());
            SPPLog.Clear();
            return "True";
        }

        #endregion

        //###########################################################################  切换模式  ###########################################################################

        #region 切换模式

        string Limiter_Off()
        {
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog("3E 84 01 02 82 01".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 82 01 01")) return "True";
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        string Limiter_On_parameter()
        {
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog("3E 84 01 02 82 E1".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 82 E1 01")) return "True";
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        string ANC_Off()
        {
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog("3E D5 01 01 00".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 D5 03 01 00 00 01")) return "True";
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        string ANC_On()
        {
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog("3E D5 01 01 01".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 D5 03 01 01 00 01")) return "True";
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        string FF_only()
        {
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog("3E D5 01 01 02".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 D5 03 01 02 00 01")) return "True";
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        string FB_only()
        {
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog("3E D5 01 01 03".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 D5 03 01 03 00 01")) return "True";
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        string Ambient_1_On()
        {
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog("3E D5 01 01 04".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 D5 03 01 04 00 01")) return "True";
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        #endregion


        //###########################################################################  曲线校准  ###########################################################################

        #region 曲线校准
        HDT653.TestItem.CurvesForms forms = null;
        object obj_lock = new object();
        int logIndex = 0;
        string adjustLog
        {
            set
            {
                string str = $"{logIndex}、{value}";
                logIndex++;
                Task.Run(() =>
                {
                    lock (obj_lock)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            if (forms != null)
                                break;
                            Thread.Sleep(500);
                        }
                        forms?.AddLog(str);
                    }

                });

            }
        }

        public string ANCAdjustFB_ForCurves(string FilePath)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                if (!File.Exists(FilePath))
                    return "Not Found Data File False";
                ReadSCData(File.ReadAllLines(FilePath), out Dictionary<string, CurvesData> Curves);
                //不需要校准 符合上下限 感差小于3
                if (ifCorves(Curves[data.Diff_FB.CurvesName], Curves[data.Diff_FB.UppLimitName], 2)
                    && ifCorves(Curves[data.L_FB.CurvesName], Curves[data.L_FB.UppLimitName], Curves[data.L_FB.LowLimitName])
                    && ifCorves(Curves[data.R_FB.CurvesName], Curves[data.R_FB.UppLimitName], Curves[data.R_FB.LowLimitName])
                    )
                {
                    //return "Not Adjust True";
                }

                Task.Run(() =>
                {

                    forms = new CurvesForms(8);
                    forms.AddCurves_(Curves[data.L_FB.UppLimitName]);
                    forms.AddCurves_(Curves[data.L_FB.LowLimitName]);
                    forms.AddDiffCurves(Curves[data.Diff_FB.LowLimitName]);
                    forms.AddDiffCurves(Curves[data.Diff_FB.UppLimitName]);
                    forms.ShowDialog();
                });


                int L_FBGain = 0x0a;//int.Parse(this.GainFBL.VolumeToGain());
                int R_FBGain = 0x0a;//int.Parse(this.GainFBR.VolumeToGain());
                string NexL;
                string NexR;
                CurvesData NexDiffCurves = new CurvesData()
                {
                    CurveName = "NG",
                    Xdata = new double[0],
                    Ydata = new double[0],
                };

                //不插入limit进行筛选，只是单单计算曲线
                __CalculateCurve(data.L_FB.CurvesName, Cardinal.L_FB_CurveGianCardinal[L_FBGain], Curves, false, out Dictionary<int, CurvesData> L_NotScreen);
                __CalculateCurve(data.R_FB.CurvesName, Cardinal.R_FB_CurveGianCardinal[R_FBGain], Curves, false, out Dictionary<int, CurvesData> R_NotScreen);

                //不插入limit进行筛选，感差
                string NotScreen = __CalculateDifference(data.Diff_FB.CurvesName, data.Diff_FB.UppLimitName, L_NotScreen, R_NotScreen, Curves, ref NexDiffCurves);
                NexL = NotScreen.Split('-')[0];
                NexR = NotScreen.Split('-')[1];
                adjustLog = $"不嵌入limit 最优选项{NotScreen}";

                //加入limit进行筛选，只是单单计算曲线
                bool LisPass = CalculateCurve(data.L_FB.CurvesName, data.L_FB.UppLimitName, data.L_FB.LowLimitName, Cardinal.L_FB_CurveGianCardinal[L_FBGain], Curves, false, out Dictionary<int, CurvesData> L_PassCurves);
                bool RisPass = CalculateCurve(data.R_FB.CurvesName, data.R_FB.UppLimitName, data.R_FB.LowLimitName, Cardinal.R_FB_CurveGianCardinal[R_FBGain], Curves, false, out Dictionary<int, CurvesData> R_PassCurves);
                string logStr = "";
                foreach (var item in L_PassCurves)
                    logStr += $"{item.Key.ToString("X2")}，";
                adjustLog = $"嵌入Limit L Pass数量:{L_PassCurves.Count} // :{logStr}";
                logStr = "";
                foreach (var item in R_PassCurves)
                    logStr += $"{item.Key.ToString("X2")}，";
                adjustLog = $"嵌入Limit R Pass数量:{R_PassCurves.Count} // :{logStr}";
                //当左右耳都符合规格的曲线时就符合规格的曲线筛选感差
                if (LisPass && RisPass)
                {
                    adjustLog = $"左右耳都有Pass的曲线";

                    //传入limit 筛选感度感差
                    string DiffGain = CalculateDifference(data.Diff_FB.CurvesName, data.Diff_FB.UppLimitName, data.Diff_FB.LowLimitName, L_PassCurves, R_PassCurves, Curves, ref NexDiffCurves);
                    if (!DiffGain.Contains("False"))
                    {
                        string[] NexGain = DiffGain.Split('-');
                        NexL = NexGain[0];
                        NexR = NexGain[1];
                        adjustLog = $"嵌入感差limit 筛选：{DiffGain}";
                    }
                    //当所有平衡感差超limit时
                    else
                    {
                        //就筛选最平衡的曲线
                        DiffGain = __CalculateDifference(data.Diff_FB.CurvesName, data.Diff_FB.UppLimitName, L_PassCurves, R_PassCurves, Curves, ref NexDiffCurves);
                        string[] NexGain = DiffGain.Split('-');
                        NexL = NexGain[0];
                        NexR = NexGain[1];
                        adjustLog = $"嵌入感差limit筛选无结果，筛选Pass中最平衡 筛选：{DiffGain}";

                    }
                }
                //当只有左耳或者右耳符合规格时
                else
                {
                    adjustLog = $"只有一边耳朵有Pass曲线";
                    string DiffGain = "";
                    //当只有左耳符合规格
                    if (LisPass)
                    {
                        //传入 L符合规格的 ，传入R 全部曲线 筛选最合适的曲线
                        DiffGain = __CalculateDifference(data.Diff_FB.CurvesName, data.Diff_FB.UppLimitName, L_PassCurves, R_NotScreen, Curves, ref NexDiffCurves);
                        string[] NexGain = DiffGain.Split('-');
                        NexL = NexGain[0];
                        NexR = NexGain[1];
                        adjustLog = $"只有左耳Pass 从所有曲线筛选右耳";

                    }
                    //当只有右耳符合规格
                    if (RisPass)
                    {
                        //传入 R符合规格的 ，传入L 全部曲线 筛选最合适的曲线

                        DiffGain = __CalculateDifference(data.Diff_FB.CurvesName, data.Diff_FB.UppLimitName, L_NotScreen, R_PassCurves, Curves, ref NexDiffCurves);
                        string[] NexGain = DiffGain.Split('-');
                        NexL = NexGain[0];
                        NexR = NexGain[1];
                        adjustLog = $"只有右耳Pass 从所有曲线筛选左耳";

                    }
                    if (LisPass && RisPass)
                    {
                        adjustLog = $"左右耳都不合格，取最平衡";

                    }
                }
                Task.Run(() =>
                {
                    Thread.Sleep(500);

                    CurvesData L_Curves = L_NotScreen[Convert.ToInt32(NexL, 16)];
                    CurvesData R_Curves = R_NotScreen[Convert.ToInt32(NexR, 16)];
                    L_Curves.CurveName = $"L_{L_Curves.CurveName}";
                    R_Curves.CurveName = $"R_{R_Curves.CurveName}";
                    L_Curves._Color = Color.Yellow;
                    R_Curves._Color = Color.Blue;
                    NexDiffCurves._Color = Color.Green;


                    forms?.AddCurves_(L_Curves);
                    forms?.AddCurves_(R_Curves);
                    forms?.AddDiffCurves(NexDiffCurves);
                });
                Console.WriteLine("算法耗时："+sw.ElapsedMilliseconds);
                Console.ReadKey();
                string L_SetValue = Set_FBL_Gain(NexL);
                string R_SetValue = Set_FBR_Gain(NexR);

                if (L_SetValue.ContainsFalse() || R_SetValue.ContainsFalse())
                    return $"{L_SetValue}||{R_SetValue} Set Gain False";
                Thread.Sleep(800);
                return "True";

            }
            catch (Exception ex)
            {

                Show("数据异常，算法报错\r\n" + ex.ToString());
                return $"{ex.Message} False";
            }


        }

        public string ANCAdjustFF_ForCurves(string FilePath)
        {
            try
            {
                if (!File.Exists(FilePath))
                    return "Not Found Data File False";
                ReadSCData(File.ReadAllLines(FilePath), out Dictionary<string, CurvesData> Curves);
                //不需要校准 符合上下限 感差小于3
                if (ifCorves(Curves[data.Diff_FF.CurvesName], Curves[data.Diff_FF.UppLimitName], 3.2)
                    && ifCorves(Curves[data.L_FF.CurvesName], Curves[data.L_FF.UppLimitName], Curves[data.L_FF.LowLimitName])
                    && ifCorves(Curves[data.R_FF.CurvesName], Curves[data.R_FF.UppLimitName], Curves[data.R_FF.LowLimitName])
                    )
                {
                    return "Not Adjust True";
                }

                CurvesData diffCurve = new CurvesData();
                int L_FFGain = int.Parse(this.GainFFL.VolumeToGain());
                int R_FFGain = int.Parse(this.GainFFR.VolumeToGain());

                CalculateCurve(data.L_FF.CurvesName, data.L_FF.UppLimitName, data.L_FF.LowLimitName, Cardinal.L_FF_CurveGianCardinal[L_FFGain], Curves, false, out Dictionary<int, CurvesData> L_PassCurves);
                CalculateCurve(data.R_FF.CurvesName, data.R_FF.UppLimitName, data.R_FF.LowLimitName, Cardinal.R_FF_CurveGianCardinal[R_FFGain], Curves, false, out Dictionary<int, CurvesData> R_PassCurves);

                string NexL = "0A";
                string NexR = "0A";
                if (L_PassCurves.Count > 1)
                {
                    int Max = L_PassCurves.Keys.Max();
                    int Min = L_PassCurves.Keys.Min();
                    int dif = (Max - Min) / 2;
                    NexL = (Min + dif).ToString("x2");
                    // MessageBox.Show(NexL);
                }

                if (R_PassCurves.Count > 1)
                {
                    int Max = R_PassCurves.Keys.Max();
                    int Min = R_PassCurves.Keys.Min();
                    int dif = (Max - Min) / 2;
                    NexR = (Min + dif).ToString("x2");
                    // MessageBox.Show(NexR);
                }


                string Result = CalculateDifference(data.Diff_FF.CurvesName, data.Diff_FF.UppLimitName, data.Diff_FF.LowLimitName, L_PassCurves, R_PassCurves, Curves, ref diffCurve);

                if (!Result.Contains("False"))
                {
                    string[] NexGain = Result.Split('-');
                    NexL = NexGain[0];
                    NexR = NexGain[1];
                }


                string L_SetValue = Set_FFL_Gain(NexL);
                string R_SetValue = Set_FFR_Gain(NexR);
                if (L_SetValue.ContainsFalse() || R_SetValue.ContainsFalse())
                {
                    return $"{L_SetValue}||{R_SetValue} Set Gain False";
                }

                Thread.Sleep(800);

                return "True";

            }
            catch (Exception ex)
            {

                Show("数据异常，算法报错\r\n" + ex.ToString());
                return $"{ex.Message} False";
            }


        }


        void __CalculateCurve(string CurvesName, Dictionary<int, double[]> CurveGianCardinal, Dictionary<string, CurvesData> allCurvesData, bool IsADD, out Dictionary<int, CurvesData> NexGainCurves)
        {
            try
            {

                CurvesData CurveData = allCurvesData[CurvesName];

                NexGainCurves = new Dictionary<int, CurvesData>();
                Dictionary<int, CurvesData> NexGainCurvesxxxxx = new Dictionary<int, CurvesData>();

                foreach (KeyValuePair<int, double[]> AdjustValue in CurveGianCardinal)
                {
                    double[] DiffCurves = AdjustValue.Value;
                    double[] NexCurves = new double[DiffCurves.Length];
                    double[] NexXdata = new double[DiffCurves.Length];
                    for (int i = 0; i < DiffCurves.Length; i++)
                    {

                        if (IsADD)
                        {
                            NexCurves[i] = (CurveData.Ydata[i] + DiffCurves[i]).Round(2);

                        }
                        else
                        {
                            NexCurves[i] = (CurveData.Ydata[i] - DiffCurves[i]);
                        }
                        NexXdata[i] = CurveData.Xdata[i];
                    }
                    NexGainCurves.Add(AdjustValue.Key, new CurvesData()
                    {
                        Ydata = NexCurves,
                        Xdata = NexXdata,
                        CurveName = $"Gain:{AdjustValue.Key:X2}"
                    });
                }
            }
            catch (Exception ex)
            {
                NexGainCurves = new Dictionary<int, CurvesData>();
                Show(ex.ToString());
            }


        }
        string __CalculateDifference(string CurvesName, string DiffUppLimitName, Dictionary<int, CurvesData> L_PassCurves, Dictionary<int, CurvesData> R_PassCurves, Dictionary<string, CurvesData> allCurvesData, ref CurvesData curves)
        {
            CurvesData value = allCurvesData[CurvesName];
            CurvesData Uplimit = allCurvesData[DiffUppLimitName];
            int StartCurvesIndex = Array.IndexOf(value.Xdata, Uplimit.Xdata[0]);
            int EndCurvesIndex = Array.IndexOf(value.Xdata, Uplimit.Xdata[1]);

            Dictionary<string, CurvesData> DifferenceCurves = new Dictionary<string, CurvesData>();
            Dictionary<string, double[]> asbDifference = new Dictionary<string, double[]>();
            Dictionary<string, double> diffMaxValue = new Dictionary<string, double>();
            double[] XdataS = new double[0];

            foreach (var LpassData in L_PassCurves)
            {
                double[] L_Ydata = LpassData.Value.Ydata;
                double[] L_Xdata = LpassData.Value.Xdata;
                foreach (var RpassData in R_PassCurves)
                {
                    double[] R_Ydata = RpassData.Value.Ydata;
                    double[] R_Xdata = RpassData.Value.Xdata;
                    string DiffName = $"{LpassData.Key.ToString("X2")}-{RpassData.Key.ToString("X2")}";
                    double[] DiffYdata = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                    double[] DiffXdata = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                    double[] DiffAsb = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                    for (int i = 0; i < DiffYdata.Length; i++)
                    {

                        double L = L_Ydata[StartCurvesIndex + i];
                        double R = R_Ydata[StartCurvesIndex + i];
                        double Xdata = LpassData.Value.Xdata[StartCurvesIndex + i];
                        DiffYdata[i] = (L - R).Round(2);
                        DiffXdata[i] = Xdata;
                        DiffAsb[i] = DiffYdata[i].abs();

                    }

                    DifferenceCurves.Add(DiffName, new CurvesData()
                    {
                        CurveName = DiffName,
                        Ydata = DiffYdata,
                        Xdata = DiffXdata
                    });
                    XdataS = DiffXdata;
                    asbDifference[DiffName] = DiffAsb;
                    diffMaxValue[DiffName] = DiffAsb.Max();


                }
            }
            string Gain = "NG";
            double MinValue = double.MaxValue;
            foreach (var item in diffMaxValue)
            {
                if (item.Value < MinValue)
                {
                    Gain = item.Key;
                    MinValue = item.Value;
                }

            }

            if (DifferenceCurves.ContainsKey(Gain))
            {
                curves = DifferenceCurves[Gain];
            }

            if (Gain == "NG")
                MessageBox.Show(@"算法异常，拍照留下来发给AE工程师", "算法异常");

            return Gain;

        }




        bool CalculateCurve(string CurvesName, string UpLimitName, string LowLimitName, Dictionary<int, double[]> CurveGianCardinal, Dictionary<string, CurvesData> allCurvesData, bool IsADD, out Dictionary<int, CurvesData> NexGainCurves)
        {
            bool isFlag = false;

            try
            {
                CurvesData UpLimitData = allCurvesData[UpLimitName];
                CurvesData LowLimitData = allCurvesData[LowLimitName];
                CurvesData CurveData = allCurvesData[CurvesName];

                int CurvesIndex = Array.LastIndexOf(CurveData.Xdata, UpLimitData.Xdata[0]);
                if (CurvesIndex < 0)
                {
                    Show("曲线或上下限的limit被更改找不到100hz 校准异常");
                }

                NexGainCurves = new Dictionary<int, CurvesData>();
                Dictionary<int, CurvesData> NexGainCurvesxxxxx = new Dictionary<int, CurvesData>();

                foreach (KeyValuePair<int, double[]> AdjustValue in CurveGianCardinal)
                {
                    double[] DiffCurves = AdjustValue.Value;
                    double[] NexCurves = new double[DiffCurves.Length];
                    double[] NexXdata = new double[DiffCurves.Length];
                    bool Pass = true;
                    for (int i = 0; i < DiffCurves.Length; i++)
                    {

                        if (IsADD)
                        {
                            NexCurves[i] = (CurveData.Ydata[i] + DiffCurves[i]).Round(2);

                        }
                        else
                        {
                            NexCurves[i] = (CurveData.Ydata[i] - DiffCurves[i]);
                        }
                        NexXdata[i] = CurveData.Xdata[i];
                    }

                    for (int i = 0; i < UpLimitData.Xdata.Length; i++)
                    {
                        //LimitXdata += $"{UpLimitData.Xdata[i]}".PadLeft(8, ' ');
                        //Uplimit += $"{UpLimitData.Ydata[i]}".PadLeft(8, ' ');
                        //Lowlimit += $"{LowLimitData.Ydata[i]}".PadLeft(8, ' ');
                        //ValueXdata += $"{NexXdata[i + CurvesIndex]}".PadLeft(8, ' ');

                        //Data += $"{NexCurves[i + CurvesIndex]}".PadLeft(8, ' ');
                        //addCalue += $"{DiffCurves[i + CurvesIndex]}".PadLeft(8, ' ');
                        //Valeee += $"{CurveData.Ydata[i + CurvesIndex]}".PadLeft(8, ' ');

                        double nextValue = NexCurves[i + CurvesIndex];
                        double nextXdata = NexXdata[i + CurvesIndex];
                        double UpLimit = UpLimitData.Ydata[i] - 0.2;
                        double LowLimit = LowLimitData.Ydata[i] + 0.2;
                        if (nextValue > UpLimit || nextValue < LowLimit)
                            Pass = false;

                    }

                    //CurvesForms.ShowCurves(new CurvesData()
                    //{
                    //    Ydata = NexCurves,
                    //    Xdata = NexXdata,
                    //    CurveName = $"Gain:{AdjustValue.Key.ToString("X2")}"
                    //});
                    //Show(AdjustValue.Key.ToString("X2"));
                    if (Pass)
                    {
                        isFlag = true;
                        NexGainCurves.Add(AdjustValue.Key, new CurvesData()
                        {
                            Ydata = NexCurves,
                            Xdata = NexXdata,
                            CurveName = $"Gain:{AdjustValue.Key.ToString("X2")}"
                        });
                    }
                    NexGainCurvesxxxxx.Add(AdjustValue.Key, new CurvesData()
                    {
                        Ydata = NexCurves,
                        Xdata = NexXdata,
                        CurveName = $"Gain:{AdjustValue.Key.ToString("X2")}"
                    });
                    //Console.WriteLine(ValueXdata);

                    //Console.WriteLine(LimitXdata);
                    //Console.WriteLine(Uplimit);
                    //Console.WriteLine();
                    //Console.WriteLine(addCalue);
                    //Console.WriteLine(Valeee);
                    //Console.WriteLine();

                    //Console.WriteLine(Data);
                    //Console.WriteLine();

                    //Console.WriteLine(Lowlimit);
                    //Console.WriteLine($"{AdjustValue.Key.ToString("x2")}__{Pass}");
                    //Console.WriteLine();
                    //if (AdjustValue.Key == 26)
                    //{
                    //    Console.ReadKey();
                    //}
                    //  NexGainCurvesL[AdjustValue.Key] = NexCurves;



                }




            }
            catch (Exception ex)
            {
                NexGainCurves = new Dictionary<int, CurvesData>();
                Show(ex.ToString());
            }
            return isFlag;

        }


        string CalculateDifference(string CurvesName, string DiffUppLimitName, string DiffLowLimitName, Dictionary<int, CurvesData> L_PassCurves, Dictionary<int, CurvesData> R_PassCurves, Dictionary<string, CurvesData> allCurvesData, ref CurvesData curves)
        {
            CurvesData value = allCurvesData[CurvesName];
            CurvesData Uplimit = allCurvesData[DiffUppLimitName];
            CurvesData LowLimit = allCurvesData[DiffLowLimitName];
            int StartCurvesIndex = Array.IndexOf(value.Xdata, Uplimit.Xdata[0]);
            int EndCurvesIndex = Array.IndexOf(value.Xdata, Uplimit.Xdata[1]);
            Dictionary<string, CurvesData> DifferenceCurves = new Dictionary<string, CurvesData>();
            Dictionary<string, double[]> asbDifference = new Dictionary<string, double[]>();
            Dictionary<string, double> diffMaxValue = new Dictionary<string, double>();
            double[] XdataS = new double[0];

            foreach (var LpassData in L_PassCurves)
            {
                double[] L_Ydata = LpassData.Value.Ydata;
                double[] L_Xdata = LpassData.Value.Xdata;
                foreach (var RpassData in R_PassCurves)
                {
                    double[] R_Ydata = RpassData.Value.Ydata;
                    double[] R_Xdata = RpassData.Value.Xdata;
                    bool pass = true;
                    string DiffName = $"{LpassData.Key.ToString("X2")}-{RpassData.Key.ToString("X2")}";
                    double[] DiffYdata = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                    double[] DiffXdata = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                    double[] DiffAsb = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                    for (int i = 0; i < DiffYdata.Length; i++)
                    {

                        double L = L_Ydata[StartCurvesIndex + i];
                        double R = R_Ydata[StartCurvesIndex + i];

                        DiffYdata[i] = (L - R).Round(2);
                        DiffAsb[i] = DiffYdata[i].abs();
                        DiffXdata[i] = LpassData.Value.Xdata[StartCurvesIndex + i];

                        if (DiffAsb[i] > Uplimit.Ydata[0].abs())
                        {
                            pass = false;
                        }

                    }
                    if (pass)
                    {

                        DifferenceCurves.Add(DiffName, new CurvesData()
                        {
                            CurveName = DiffName,
                            Ydata = DiffYdata,
                            Xdata = DiffXdata
                        });
                        XdataS = DiffXdata;
                        asbDifference[DiffName] = DiffAsb;
                        diffMaxValue[DiffName] = DiffAsb.Max();

                    }

                }
            }
            string Gain = "NG";
            double MinValue = double.MaxValue;
            foreach (var item in diffMaxValue)
            {
                if (item.Value < MinValue)
                {
                    Gain = item.Key;
                    MinValue = item.Value;
                }

            }
            if (DifferenceCurves.ContainsKey(Gain))
            {
                curves = DifferenceCurves[Gain];
            }
            return Gain;

        }


        #endregion


        //###########################################################################  FB  ###########################################################################
        #region 感度校准
        //string ANCAdjustFB(double L_Margin, double L_Uplimit, double L_Lowlimit, double R_Margin, double R_Uplimit, double R_Lowlimit)
        //{
        //    try
        //    {
        //        //先算出 上限与下限之间的中间值
        //        double L_MiddleLimit = (L_Lowlimit + (L_Uplimit.abs() - L_Lowlimit.abs()).abs() / 2).Round(1);
        //        double R_MiddleLimit = (R_Lowlimit + (R_Uplimit.abs() - R_Lowlimit.abs()).abs() / 2).Round(1);

        //        //算出测试值是不是在limit 值中间 ±1
        //        double L_Diff = ((L_Margin.abs() - L_MiddleLimit.abs()).abs()).Round(1);
        //        bool L_AdjustFlag = L_Diff <= TrimRange;

        //        double R_Diff = ((R_Margin.abs() - R_MiddleLimit.abs()).abs()).Round(1);
        //        bool R_AdjustFlag = R_Diff <= TrimRange;
        //        int L_NextGain = -510000;
        //        int R_NextGain = -510000;
        //        string GainFBL = "";
        //        string GainFBR = "";
        //        //不在±1 就要校准
        //        if (!L_AdjustFlag)
        //        {
        //            GainFBL = this.GainFBL == "-1" ? Get_gain_FBL() : this.GainFBL;
        //            if (GainFBL.ContainsFalse())
        //                return $"Read L Gain {GainFBL}";
        //            L_NextGain = GetNextGainFB(int.Parse(GainFBL.VolumeToGain()), L_Diff, true, L_Margin > L_MiddleLimit);
        //            this.GainFBL = L_NextGain.GainToVolume().ToString();
        //            string L_Result = Set_FBL_Gain(L_NextGain.ToString("x2"));
        //            if (L_Result.ContainsFalse())
        //                return $"Set FB L Gain {L_Result}";
        //        }
        //        if (!R_AdjustFlag)
        //        {
        //            GainFBR = this.GainFBR == "-1" ? Get_gain_FBR() : this.GainFBR;
        //            if (GainFBR.ContainsFalse())
        //                return $"Read R Gain {GainFBR}";
        //            R_NextGain = GetNextGainFB(int.Parse(GainFBR.VolumeToGain()), R_Diff, false, R_Margin > R_MiddleLimit);
        //            this.GainFBR = R_NextGain.GainToVolume().ToString();
        //            string R_Result = Set_FBR_Gain(R_NextGain.ToString("x2"));
        //            if (R_Result.ContainsFalse())
        //                return $"Set FB R Gain {R_Result}";

        //        }
        //        if (L_AdjustFlag && R_AdjustFlag)
        //        {
        //            return "Not Adjust True";
        //        }

        //        return "True";
        //    }
        //    catch (Exception ex)
        //    {

        //        System.Windows.Forms.MessageBox.Show(ex.ToString());
        //        return "dll error False";
        //    }

        //}
        //int GetNextGainFB(int Gain, double Adjust, bool isLeftEar, bool isUpp)
        //{

        //    Dictionary<int, double> CardinalTable = new Dictionary<int, double>();
        //    foreach (var item in isLeftEar ? Cardinal.LGianCardinal_FB[Gain] : Cardinal.RGianCardinal_FB[Gain])
        //    {

        //        if (isUpp)
        //        {
        //            if (item.Key <= Gain) continue;
        //            CardinalTable.Add(item.Key, item.Value);
        //        }
        //        else if (!isUpp)
        //        {
        //            if (item.Key >= Gain) continue;
        //            CardinalTable.Add(item.Key, item.Value);

        //        }
        //    }

        //    double[] diffList = new double[Cardinal.LGianCardinal_FB[Gain].Count];
        //    for (int i = 0; i < diffList.Count(); i++)
        //    {
        //        diffList[i] = 10000;
        //    }
        //    foreach (var item in CardinalTable)
        //    {
        //        double Adjust1 = Adjust.abs();
        //        double Cardinal = item.Value.abs();

        //        diffList[item.Key] = (Adjust1 - Cardinal).abs();
        //        //Console.WriteLine($"{Adjust1}-{Cardinal}={  diffList[item.Key] }  index:{item.Key}");

        //    }

        //    int index = Array.IndexOf(diffList, diffList.Min());
        //    return index;
        //}
        #endregion


        #region MyRegion

        string Get_gain_FBL()
        {
            if (!is_connect) return "Not Connect False";
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 2; i++)
            {
                SPPWriteLog($"3E 84 02 01 01".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 5D"))
                {
                    string[] strs = ByteStr.Split(new string[] { "E3 84 03 01 5D " }, StringSplitOptions.None);

                    return GainFBL = Convert.ToInt32(strs[1].Split(' ')[0], 16).GainToVolume().ToString();
                }
                Thread.Sleep(500);
            }
            return $"{ByteStr} False";
        }
        string Get_gain_FBR()
        {
            if (!is_connect) return "Not Connect False";
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 2; i++)
            {
                SPPWriteLog($"3E 84 02 01 02".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 5E"))
                {
                    string[] strs = ByteStr.Split(new string[] { "E3 84 03 01 5E " }, StringSplitOptions.None);

                    return GainFBR = Convert.ToInt32(strs[1].Split(' ')[0], 16).GainToVolume().ToString();
                }
                Thread.Sleep(500);
            }
            return $"{ByteStr} False";
        }

        string userSet_FBL_Gain(string UserSelectGain)
        {
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {

                SPPWriteLog($"3E 84 01 01 01 {UserSelectGain}".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 5D")) return UserSelectGain;
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        string userSet_FBR_Gain(string UserSelectGain)
        {
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog($"3E 84 01 01 02 {UserSelectGain}".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 5E")) return UserSelectGain;
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }

        string Set_FBL_Gain(String Gain)
        {
            GainFBL = Convert.ToInt32(Gain, 16).GainToVolume().ToString();
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog($"3E 84 01 01 01 {Gain}".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 5D")) return "True";
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        string Set_FBR_Gain(String Gain)
        {
            GainFBR = Convert.ToInt32(Gain, 16).GainToVolume().ToString();

            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog($"3E 84 01 01 02 {Gain}".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 5E")) return "True";
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }

        int Count = 2;

        string Stored_gain_FBL()
        {
            if (!is_connect) return "Not Connect False";

            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < Count; i++)
            {
                string SetGain = GainFBL == "-1" ? Get_gain_FBL() : GainFBL;
                if (SetGain.Contains("False"))
                    return SetGain;
                SPPWriteLog($"3E 84 04 01 01 {double.Parse(SetGain).VolumeToGain()}".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 5D")) return SetGain;
                if (i < Count - 1)
                    Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        string Stored_gain_FBR()
        {
            if (!is_connect) return "Not Connect False";
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < Count; i++)
            {
                string SetGain = GainFBR == "-1" ? Get_gain_FBR() : GainFBR;
                if (SetGain.Contains("False"))
                    return SetGain;
                SPPWriteLog($"3E 84 04 01 02 {double.Parse(SetGain).VolumeToGain()}".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 5E")) return SetGain;
                if (i < Count - 1)
                    Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }
        #endregion


        //#########################  FF  #########################

        #region 感度校准

        //string ANCAdjustFF_ForMargin(double L_Up_Margin, double L_Low_Margin, double R_Up_Margin, double R_Low_Margin)
        //{
        //    string GainFFL = Get_gain_FFL().VolumeToGain();
        //    string GainFFR = Get_gain_FFL().VolumeToGain();
        //    //读取失败就跳出
        //    if (GainFFL.ContainsFalse() || GainFFR.ContainsFalse())
        //        return $"Left:{GainFFL}||Right：{GainFFR}";

        //    //左耳写入校准值
        //    string LeftAdjustResult = SetNextGainFF(L_Up_Margin, L_Low_Margin, double.Parse(GainFFL), true);
        //    //右耳写入校准值
        //    string RightAdjustResult = SetNextGainFF(R_Up_Margin, R_Low_Margin, double.Parse(GainFFR), false);
        //    if (LeftAdjustResult.ContainsFalse() || RightAdjustResult.ContainsFalse())
        //        return $"Left Set Adjust:{LeftAdjustResult}||Right Set Adjust：{RightAdjustResult}";




        //    return "True";


        //}
        //string SetNextGainFF(double Up_Margin, double Low_Margin, double Adjust, bool isLeftEar)
        //{
        //    int NextAdjust = -5100000;
        //    if (Low_Margin < 0)
        //    {
        //        double DiffAdjust = (Low_Margin.abs() / 0.2) * 2;

        //        NextAdjust = (int)Math.Ceiling(Adjust + DiffAdjust).Round(0);
        //    }
        //    else if (Up_Margin < 0)
        //    {
        //        double DiffAdjust = (Up_Margin.abs() / 0.2) * 2;
        //        NextAdjust = (int)Math.Ceiling(Adjust - DiffAdjust).Round(0);
        //    }
        //    else
        //    {
        //        return "True";
        //    }
        //    if (NextAdjust < 0 || NextAdjust > 0x1e)
        //    {
        //        System.Windows.Forms.MessageBox.Show($"校准值计算异常 Next Adjust :{NextAdjust} 值将重置");
        //        NextAdjust = NextAdjust < 0 ? 0 : 30;
        //    }


        //    return isLeftEar
        //        ? Set_FFL_Gain(NextAdjust.ToString("x2"))
        //        : Set_FFR_Gain(NextAdjust.ToString("x2"));
        //}

        //string ANCAdjustFF(double L_Margin, double L_Uplimit, double L_Lowlimit, double R_Margin, double R_Uplimit, double R_Lowlimit)
        //{
        //    try
        //    {
        //        //先算出 上限与下限之间的中间值
        //        double L_MiddleLimit = (L_Lowlimit + (L_Uplimit.abs() - L_Lowlimit.abs()).abs() / 2).Round(1);
        //        double R_MiddleLimit = (R_Lowlimit + (R_Uplimit.abs() - R_Lowlimit.abs()).abs() / 2).Round(1);

        //        //算出测试值是不是在limit 值中间 ±1
        //        double L_Diff = ((L_Margin.abs() - L_MiddleLimit.abs()).abs()).Round(1);
        //        bool L_AdjustFlag = L_Diff <= TrimRange;

        //        double R_Diff = ((R_Margin.abs() - R_MiddleLimit.abs()).abs()).Round(1);
        //        bool R_AdjustFlag = R_Diff <= TrimRange;
        //        int L_NextGain = -510000;
        //        int R_NextGain = -510000;
        //        string GainFFL = "";
        //        string GainFFR = "";
        //        //不在±1 就要校准
        //        if (!L_AdjustFlag)
        //        {
        //            GainFFL = this.GainFFL == "-1" ? Get_gain_FFL() : this.GainFFL;
        //            if (GainFFL.ContainsFalse())
        //                return $"Read L Gain {GainFFL}";
        //            L_NextGain = GetNextGainFF(int.Parse(GainFFL.VolumeToGain()), L_Diff, true, L_Margin < L_MiddleLimit);

        //            this.GainFFL = L_NextGain.GainToVolume().ToString();
        //            string L_Result = Set_FFL_Gain(L_NextGain.ToString("x2"));
        //            if (L_Result.ContainsFalse())
        //                return $"Set FF L Gain {L_Result}";
        //        }
        //        if (!R_AdjustFlag)
        //        {
        //            GainFFR = this.GainFFR == "-1" ? Get_gain_FFR() : this.GainFFR;
        //            if (GainFFR.ContainsFalse())
        //                return $"Read R Gain {GainFFR}";
        //            R_NextGain = GetNextGainFF(int.Parse(GainFFR.VolumeToGain()), R_Diff, false, R_Margin < R_MiddleLimit);
        //            this.GainFFR = R_NextGain.GainToVolume().ToString();
        //            string R_Result = Set_FFR_Gain(R_NextGain.ToString("x2"));
        //            if (R_Result.ContainsFalse())
        //                return $"Set FF R Gain {R_Result}";

        //        }
        //        if (L_AdjustFlag && R_AdjustFlag)
        //        {
        //            return "Not Adjust True";
        //        }

        //        return "True";
        //    }
        //    catch (Exception ex)
        //    {

        //        System.Windows.Forms.MessageBox.Show(ex.ToString());
        //        return "dll error False";
        //    }

        //}
        //int GetNextGainFF(int Gain, double Adjust, bool isLeftEar, bool isUpp)
        //{

        //    Dictionary<int, double> CardinalTable = new Dictionary<int, double>();
        //    foreach (var item in isLeftEar ? Cardinal.LGianCardinal_FF[Gain] : Cardinal.RGianCardinal_FF[Gain])
        //    {

        //        if (isUpp)
        //        {
        //            if (item.Key <= Gain) continue;
        //            CardinalTable.Add(item.Key, item.Value);
        //        }
        //        else if (!isUpp)
        //        {
        //            if (item.Key >= Gain) continue;
        //            CardinalTable.Add(item.Key, item.Value);

        //        }
        //    }

        //    double[] diffList = new double[Cardinal.LGianCardinal_FF[Gain].Count];
        //    for (int i = 0; i < diffList.Count(); i++)
        //    {
        //        diffList[i] = 10000;
        //    }
        //    foreach (var item in CardinalTable)
        //    {
        //        double Adjust1 = Adjust.abs();
        //        double Cardinal = item.Value.abs();

        //        diffList[item.Key] = (Adjust1 - Cardinal).abs();

        //    }

        //    int index = Array.IndexOf(diffList, diffList.Min());
        //    return index;
        //}
        #endregion


        #region MyRegion

        string Get_gain_FFL()
        {
            if (!is_connect) return "Not Connect False";

            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 2; i++)
            {
                SPPWriteLog($"3E 84 02 01 03".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 5F"))
                {
                    string[] strs = ByteStr.Split(new string[] { "E3 84 03 01 5F " }, StringSplitOptions.None);

                    return GainFFL = Convert.ToInt32(strs[1].Split(' ')[0], 16).GainToVolume().ToString();
                }
                Thread.Sleep(500);
            }
            return $"{ByteStr} False";
        }

        string Get_gain_FFR()
        {
            if (!is_connect) return "Not Connect False";

            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 2; i++)
            {
                SPPWriteLog($"3E 84 02 01 04".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 60"))
                {
                    string[] strs = ByteStr.Split(new string[] { "E3 84 03 01 60 " }, StringSplitOptions.None);
                    return GainFFR = Convert.ToInt32(strs[1].Split(' ')[0], 16).GainToVolume().ToString();
                }
                Thread.Sleep(500);
            }
            return $"{ByteStr} False";
        }


        string Set_FFL_Gain(String Gain)
        {
            GainFFL = Convert.ToInt32(Gain, 16).GainToVolume().ToString();

            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog($"3E 84 01 01 03 {Gain}".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 5F")) return Gain;
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }

        string Set_FFR_Gain(String Gain)
        {
            GainFFR = Convert.ToInt32(Gain, 16).GainToVolume().ToString();
            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < 3; i++)
            {
                SPPWriteLog($"3E 84 01 01 04 {Gain}".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 60")) return Gain;
                Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }

        string Stored_gain_FFL()
        {
            if (!is_connect) return "Not Connect False";

            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < Count; i++)
            {
                string SetGain = GainFFL == "-1" ? Get_gain_FFL() : GainFFL;
                if (SetGain.Contains("False"))
                    return SetGain;
                SPPWriteLog($"3E 84 04 01 03 {double.Parse(SetGain).VolumeToGain()}".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 5F")) return SetGain;
                if (i < Count - 1)
                    Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }

        string Stored_gain_FFR()
        {
            if (!is_connect) return "Not Connect False";

            string ByteStr = "Not Send CMD False";
            for (int i = 0; i < Count; i++)
            {
                string SetGain = GainFFR == "-1" ? Get_gain_FFR() : GainFFR;
                if (SetGain.Contains("False"))
                    return SetGain;
                SPPWriteLog($"3E 84 04 01 04 {double.Parse(SetGain).VolumeToGain()}".HexStrToBytes());
                SPPReadLog(out _, out ByteStr);
                if (ByteStr.Contains("E3 84 03 01 60")) return SetGain;
                if (i < Count - 1)
                    Thread.Sleep(1000);
            }
            return $"{ByteStr} False";
        }

        #endregion

        void ReadSCData(string[] curves, out Dictionary<string, CurvesData> Curves)
        {
            // Curves = new Dictionary<string, Dictionary<string, double[]>>();
            Curves = new Dictionary<string, CurvesData>();

            for (int i = 0; i < curves.Length; i += 3)
            {
                string curveName = curves[i].Split(',')[0];
                string X = "X:";
                string[] StrXdata = curves[i + 1].Split(',');
                double[] _Xdata = new double[StrXdata.Length];
                for (int x = 0; x < StrXdata.Length; x++)
                {
                    if (StrXdata[x] == "") continue;
                    _Xdata[x] = double.Parse(StrXdata[x]);
                    X += _Xdata[x] + " ";
                }
                string Y = "Y:";

                string[] StrYdata = curves[i + 2].Split(',');
                double[] _Ydata = new double[StrYdata.Length];
                for (int y = 0; y < StrYdata.Length; y++)
                {
                    if (StrYdata[y] == "") continue;
                    _Ydata[y] = double.Parse(StrYdata[y]);
                    Y += _Ydata[y] + " ";

                }
                Curves.Add(curveName, new CurvesData()
                {
                    CurveName = curveName,
                    Xdata = _Xdata,
                    Ydata = _Ydata,
                    _Color = curveName.Contains("Limit") ? Color.Red : Color.FromArgb(0, 0, 0)

                }); ;
            }
        }

        bool ifCorves(CurvesData Curves, CurvesData UpLimit, double MaxLimit)
        {
            int indexStart = Array.IndexOf(Curves.Xdata, UpLimit.Xdata[0]);
            int indexEnd = Array.IndexOf(Curves.Xdata, UpLimit.Xdata[1]);


            for (int i = indexStart; i < indexEnd; i++)
            {
                if (Curves.Ydata[i].abs() > MaxLimit)
                {
                    return false;
                }
            }

            return true;
        }
        bool ifCorves(CurvesData Curves, CurvesData UpLimit, CurvesData LowLimit)
        {
            int indexStart = Array.IndexOf(Curves.Xdata, UpLimit.Xdata[0]);
            int indexEnd = Array.IndexOf(Curves.Xdata, UpLimit.Xdata[UpLimit.Xdata.Length - 1]);
            for (int i = 0; i < UpLimit.Ydata.Length; i++)
            {
                if (UpLimit.Ydata[i] <= Curves.Ydata[i + indexStart] || LowLimit.Ydata[i] >= Curves.Ydata[i + indexStart])
                {
                    return false;
                }
            }

            return true;
        }
        Dictionary<int, Dictionary<int, double[]>> adjustSplit(string[] Data)
        {
            Dictionary<int, Dictionary<int, double[]>> DicData = new Dictionary<int, Dictionary<int, double[]>>();

            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i].Trim().Length > 1)
                {
                    string[] Str = Data[i].Split(',');
                    double[] Ydata = new double[Str.Length - 2];
                    int ID = Convert.ToInt16(Str[0].Split('-')[0]);
                    int IID = Convert.ToInt16(Str[0].Split('-')[1]);
                    if (!DicData.ContainsKey(ID))
                    {
                        DicData.Add(ID, new Dictionary<int, double[]>());
                    }

                    for (int j = 1; j < Str.Length - 1; j++)
                    {

                        Ydata[j - 1] = Convert.ToDouble(Str[j]).Round(2);

                    }
                    DicData[ID][IID] = Ydata;

                }
            }
            return DicData;

        }

        #region 

        string HID_Stored_gain()
        {
            string CMD = "07 2C 84 04 01";
            bool SendFlag = false;
            MessageBoxs.BarCodeBox("请输入FBL Gain值", 2, out string barcode);
            SendFlag = SendCMD.WriteSend($"{CMD} 01 {barcode}", 64, GH.Handle);
            if (!SendFlag)
                return "False";
            MessageBoxs.BarCodeBox("请输入FBR Gain值", 2, out barcode);
            SendFlag = SendCMD.WriteSend($"{CMD} 02 {barcode}", 64, GH.Handle);
            if (!SendFlag)
                return "False";
            MessageBoxs.BarCodeBox("请输入FFL Gain值", 2, out barcode);
            SendFlag = SendCMD.WriteSend($"{CMD} 03 {barcode}", 64, GH.Handle);
            if (!SendFlag)
                return "False";
            MessageBoxs.BarCodeBox("请输入FFR Gain值", 2, out barcode);
            SendFlag = SendCMD.WriteSend($"{CMD} 04 {barcode}", 64, GH.Handle);
            if (!SendFlag)
                return "False";

            return "True";
        }
        #endregion

    }
    static class Myconvert
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
        public static double abs(this double str)
        {
            return Math.Abs(str);
        }
        public static double Round(this double str, int i)
        {
            return Math.Round(str, i);
        }
        public static double GainToVolume(this int str)
        {
            return Math.Round((double)str * 0.2, 1);
        }
        public static double GainToVolume(this double str)
        {
            return Math.Round((double)Convert.ToInt32(str) * 0.2, 1);
        }

        public static string VolumeToGain(this double str)
        {
            return Convert.ToString((int)Math.Round((str / 0.2)), 16);
        }

        public static string VolumeToGain(this string str)
        {
            return (Convert.ToDouble(str) / 0.2).Round(1).ToString();
        }



        public static bool ContainsFalse(this string str)
        {
            return str.Contains("False");
        }



    }

}
