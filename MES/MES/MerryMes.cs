using BD;
using MES;
using MESDLL;
using SwATE_Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    /// <summary dllName="MES">
    /// MES
    /// </summary>
    public class MerryDll : MesInterface
    {
        #region 参数区
        enum ChormeID
        {
            CheckUser = 1,
            CheckSN = 2,
            UploadSN = 3,
            UploadData = 5,
            GetBDofMes = 6,
            UplongBDResultToMes = 7,
            GetBitAddressMes = 8,
            QueryBDofSN = 10,
            Query = 26,
            Binding = 44,
            ExistsSN = 47,
            CustomerSN = 48


        }

        private static _MES mMes;
        private struct _MES
        {
            internal int enable;
            internal string userName;
            internal string station;
            internal SwATE mesInst;
            internal string sLastSN;
            internal int iFailCount;
        }
        
        ECClientDLL.MyEAP myeap = new ECClientDLL.MyEAP();
        MESBDA mESBDA = new MESBDA();
        private static string sMESUrl = @"http://10.175.5.101:9111/Service1.asmx"; //@"http://10.175.5.55:8008/Service1.asmx";
        private string TypeName; //机型名称
        private static string Station;//站别
        internal static int MesFlag;//上传MES标志位
        internal static bool BDFlag;//获取BD标志位
        private string CustomerName;//客户名称
        private int WorkOrderLength;//工单长度
        private string ErrorCode = "T30000";//不良代码
        public static string Works;//工单
        public static string userID;//登录工单的工号
        private int PassFlag;//卡MES
        private int FailFlag;//卡MES
        private string TestSN;//卡MES
        private int PassCount;//卡MES
        private int FailCount;//卡MES
        private static string BDStation;//站别
        static string SystematicName;//系统名称
        bool ChormeFlag = false;
        #endregion
        #region 接口方法
        //Graphics currentGraphics = Graphics.FromHwnd(new WindowInteropHelper(new System.Windows.Window()).Handle);
        /// <summary>
        /// 平台共享参数
        /// </summary>
        static Dictionary<string, object> Config = new Dictionary<string, object>();
        /// <summary>
        /// 连扳程序的参数
        /// </summary>
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        //接口
        public object Interface(Dictionary<string, object> keys) => (Config = keys);
        public void OnceConfigInterface(Dictionary<string, object> onceConfig) => OnceConfig = onceConfig;
        bool MoreTest = false;
        //DLL信息获取
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：MES";
            string dllfunction = "Dll功能说明 ：Chorme，翔威，生产辅助系统";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllHistoryVersion2 = "                     ：0.0.2.1";
            string dllHistoryVersion3 = "                     ：0.0.2.2";
            string dllHistoryVersion4 = "                     ：21.7.31.09";
            string dllHistoryVersion5 = "                     ：21.8.2.3";
            string dllHistoryVersion6 = "                     ：21.8.4.20";
            string dllHistoryVersion7 = "                     ：21.8.30.0";
            string dllHistoryVersion8 = "                     ：21.9.8.0";
            string dllHistoryVersion9 = "                     ：21.10.18.0";
            string dllHistoryVersion10 = "                     ：23.11.23.0";
            string dllVersion = "当前Dll版本：23.12.11.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo2 = "0.0.2.1：2021/6/15：增加Chorme系统,测试标准品不会绑定sn和获取BCCode";
            string dllChangeInfo3 = "0.0.2.2：2021/7/15：测试标准品不会绑定sn和获取BCCode，更改BTAD获取流程，根据SN获取BD号";
            string dllChangeInfo4 = "21.7.31.09：2021/7/31：修复chorme系统重启程序无法连接问题，条码与工单检测，获取BD号问题,每次查询BD号刷新";
            string dllChangeInfo5 = "21.8.2.3：2021/8/2：修改获取BD号方法：SN长度小于4无法获取";
            string dllChangeInfo6 = "21.8.4.20：2021 /8/14：GetBD方法增加刷新BD号信息，显示BD数量窗体若无BD号90秒后自动关闭，增加BD或上传失败报提示,Chorme系统BD号信息查询";
            string dllChangeInfo7 = "21.8.30.0：2021 /8/30：标准品条码赋值BD号，SN，BCCode";
            string dllChangeInfo8 = "21.9.8.0：2021 /9/8：标准品条码赋值BD号，SN，BCCode";
            string dllChangeInfo9 = "21.10.11.1：2021 /10/11：Chorme系统返回值NG增加False字段";
            string dllChangeInfo10 = "21.10.18.2：2021 /10/18：增加金士顿T3.0，T4.0过站接口，获取料号内容信息";
            string dllChangeInfo11 = "22.12.28.0：2022 /12/28：防呆绑定SN条码错误，添加检查绑定SN信息的指令";
            string dllChangeInfo12 = "23.3.24.5：2023 /03/24：调整登录界面，调整绑定指令，调整MES上传 UpdateMes";
            string dllChangeInfo13 = "23.3.24.8：2023/05/20：调整CheckSN";
            string dllChangeInfo14 = "23.3.24.11：2023/06/06：把MES报错的信息翻译过越南语（CheckSN方法，BindingSN方法）";
            string dllChangeInfo15 = "23.6.26.0：2023/06/26：增加根据韶音条码获取产品MAC地址";
            string dllChangeInfo16 = "23.7.22.0：2023/07/22：调整上传MES的测试结果（UpdateMES），NG结果也上传/ QPM要求的";
            string dllChangeInfo17 = "23.8.21.0：2023/07/22：调整GetBDFromMes的方法";
            string dllChangeInfo18 = "23.10.9.0：2023/10/9：增加获取UID号码的方法";
            string dllChangeInfo19 = "23.10.19.0：2023/10/19：BarcodeWindow 的扫描SN方法增加设定内容";
            string dllChangeInfo20 = "23.10.20.0：2023/10/20：调整Run方法";
            string dllChangeInfo21 = "23.10.25.0：2023/10/25：GetBd 号增加 调用 Bluetooth_ID";
            string dllChangeInfo22 = "23.10.26.0：2023/10/26：上传不良代码的格式修改";
            string dllChangeInfo23 = "23.11.10.0：2023/11/10：MesFlag = 2 ---> CheckSN的方法 卡 工单";
            string dllChangeInfo24 = "23.11.14.0：2023/11/10：不良的代码优化调整，克服之前的不良代码序号错误";
            string dllChangeInfo25 = "23.11.20.0：2023/11/20：上传不良代码的弹框提示调整，使用Form来提示";
            string dllChangeInfo26 = "23.12.11.0：MesFlag = 2 ---> CheckUser 的方法Update，Station = Config[Name]_Config[Station]";

            string[] info = { dllname,
                dllfunction,dllHistoryVersion,dllHistoryVersion2, dllHistoryVersion3,
                dllHistoryVersion4,dllHistoryVersion5,dllHistoryVersion6,dllHistoryVersion7,dllHistoryVersion8,dllHistoryVersion9,dllHistoryVersion10,

                dllVersion,
                dllChangeInfo, dllChangeInfo2,  dllChangeInfo3,dllChangeInfo4 ,

                dllChangeInfo5,dllChangeInfo6,dllChangeInfo7,dllChangeInfo8,dllChangeInfo9,dllChangeInfo10,dllChangeInfo11,dllChangeInfo12,dllChangeInfo13
            };
            return info;
        }
        List<string> formdata = new List<string>();
        public string Run(object[] Command)
        {
            SplitCMD(Command, out string SN, out string[] cmd);
            if (SN.Contains("TE_BZP"))
                return TE_BZP();
            switch (cmd[1])
            {
                
                case "Binding": return Binding((string)Config["SN"], (string)Config["BindingSN"]); //userID, (string)Config["SN"]

                case "BindingSN": return BindingSN();

                case "GetSN_BCCodeFromMES": formdata[3] = GetSN_BCCode(); return formdata[3];
                case "GetBDFromMES":
                    switch (SystematicName.ToString().ToLower())
                    {
                        case "chorme":
                            try
                            {
                                if (GerBDFromMesChorme() != "False")
                                {
                                    formdata[1] = Config["LincenseKey"].ToString();
                                    formdata[2] = Config["BitAddress"].ToString();

                                    return formdata[2];

                                }
                                else { return false.ToString(); }
                            }
                            finally
                            {

                                string info = ChormeUpdataInfo();
                                form.Msg = info;
                                form.ShowInformation_Load(null, null);
                            }
                        default:
                            if (GetBDFromMES())
                            {
                                formdata[1] = Config["LincenseKey"].ToString();
                                formdata[2] = Config["BitAddress"].ToString();
                                return formdata[2];
                            }
                            else
                            {
                                return false.ToString();
                            }
                    }

                case "GetBDforBarcodeWindow": return GetBDforBarcodeWindowSN().ToString();
                case "BarCodeWindow":
                    string[] Length = new string[3];
                    if (cmd.Length >= 3)
                    {
                        Length[2] = cmd[2];
                    }
                    else
                    {
                        Length[2] = Config["Barcode"].ToString();
                    }
                    return BarCodeWindow(Length[2],(cmd.Length<=3||cmd[3].ToUpper()=="NULL")?"":cmd[3]).ToString();
                case "BarCode3":
                    string[] Length2 = new string[3];
                    if (cmd.Length >= 3)
                    {
                        Length2[2] = cmd[2];
                    }
                    else
                    {
                        Length2[2] = Config["Barcode"].ToString();
                    }
                    return BarCodeWindow2(Length2[2]).ToString();

                case "BarCodeWindowDongle":
                    string[] Length1 = new string[3];
                    if (cmd.Length >= 3)
                    {
                        Length1[2] = cmd[2];
                    }
                    else
                    {
                        Length1[2] = Config["Barcode"].ToString();
                    }
                    return BarCodeWindowDongle(Length1[2]).ToString();
                case "QueryBDNoBySN":
                    return QueryBDNoBySN();
             
                case "QueryBDNoBySN0":
                    return QueryBDNoBySN0();
                //case "QueryBDNoBySNForm":
                //    return QueryBDNoBySNForm();
                //// Get binding bd via SN by using swate
                //case "QueryBindingBDNoBySN": return QueryBindingBDNoBySN();
                //case "QueryBindingBDNoBySNForm": return QueryBindingBDNoBySNForm();
                case "CustomerSNFromMES": return CustomerSNFromMES();
                case "QueryKeyPart": return QueryKeyPart();
                case "CheckBinding": return CheckBinding();
                case "CheckRxBinding": return CheckRXBinding();
                case "CheckTxBinding": return CheckTXBinding();
                case "GetBindingSN":
                    MessageBox.Show("123");
                    return GetBindingSN();
                case "GetBDfromBindingSN":
                    return GetBDfromBindingSN();
                case "GetMAC_SY": return GetMAC_SY();

                case "UID_Master": return UID_Master(bool.Parse(cmd[2]), cmd[3]);
                case "UID_Slave": return UID_Slave(bool.Parse(cmd[2]), cmd[3]);
                case "LogiUIDForIndex": return LogiUIDForIndex(bool.Parse(cmd[2]), int.Parse(cmd[3]));
                default: return "指令错误 False";

            }
        }
        void SplitCMD(object[] Command, out string SN, out string[] CMD)
        {
            List<string> listCMD = new List<string>();
            foreach (var item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>)) OnceConfig = (Dictionary<string, object>)item;
                if (type == typeof(List<string>)) formdata = (List<string>)item;
                if (type == typeof(string))
                {
                    listCMD = new List<string>(item.ToString().Split('&'));
                    for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
                }

            }
            CMD = listCMD.ToArray();
            MoreTest = OnceConfig.ContainsKey("SN");
            SN = MoreTest ? (string)OnceConfig["SN"] : (string)Config["SN"];

        }
        public string TE_BZP()
        {

            Config["LincenseKey"] = formdata[1] = "TE_BZP";
            Config["BitAddress"] = formdata[2] = "TE_BZP";
            Config["SN_BCCode"] = formdata[3] = "TE_BZP";
            Config["CustomerSN"] = "TE_BZP";
            Config["CustomerSNByBindingSN"] = "TE_BZP";
            Config["BitAddressByBarcodeSN"] = "TE_BZP";
            Config["CustomerSNByBarcodeSN"] = "TE_BZP";
            Config["BitAddressByBindingSN"] = "TE_BZP";
            Config["CustomerSNByBarcodeSN"] = "TE_BZP";
            Config["UID_D"] = Config["UID"] = Config["SN"];
            Config["PairID"] = Config["PairID"] = "TE_BZP";

            if (OnceConfig.Count > 0)
            {
                OnceConfig["LincenseKey"] = "TE_BZP";
                OnceConfig["BitAddress"] = "TE_BZP";
                OnceConfig["SN_BCCode"] = "TE_BZP";
                OnceConfig["CustomerSN"] = "TE_BZP";
                OnceConfig["CustomerSNByBindingSN"] = "TE_BZP";
                OnceConfig["BitAddressByBarcodeSN"] = "TE_BZP";
                OnceConfig["CustomerSNByBindingSN"] = "TE_BZP";
                OnceConfig["CustomerSNByBarcodeSN"] = "TE_BZP";
                OnceConfig["PairID"] = "TE_BZP";

            }
            return (string)Config["SN"];

        }
        #endregion
        //T3用的过站接口
        //public bool IsBindSecurityCode()
        //{
        //    return mESBDA.IsBindSecurityCode(Config["SN"].ToString(), false);
        //}
        //T4.0
        //public bool UploadKingStonSN()
        //{
        //    return mESBDA.UploadKingStonSN(Config["Works"].ToString(), Config["SN"].ToString());
        //}


        /// <summary isPublicTestItem="true">
        /// 输入Config[BarCodeSN]条码
        /// </summary>
        /// <param name="Length" options="10,12">条码的长度</param>
        /// <param name="Forms" options="NULL,请输入条码/Vui lòng quét mã SN,请扫描Bincode 、Vui lòng quét mã Bincode">弹框内容</param>
        /// <returns>info</returns>
        string BarCodeWindow(string Length, string Forms)
        {

            bool flag = MerryTest.MessageBoxs.BarCodeBox((Forms.Length)==0?"请输入条码/Vui lòng quét mã SN": Forms, int.Parse(Length), out string BarCode);
            BarCode = BarCode.ToUpper();
            Config["BindingSN"] = BarCode;
            return flag ? BarCode : "False";

        }
        /// <summary isPublicTestItem="true">
        /// 输入Config[SN]条码
        /// </summary>
        /// <param name="Length" options="10,12">条码的长度</param>
        /// <returns>info</returns>
        string BarCodeWindow2(string Length)
        {

            bool flag = MerryTest.MessageBoxs.BarCodeBox("请输入条码/Vui lòng quét mã SN", int.Parse(Length), out string BarCode);
            BarCode = BarCode.ToUpper();
            Config["SN"] = BarCode;
            return flag ? BarCode : "False";

        }

        string BarCodeWindowDongle(string Length)
        {

            bool flag = MerryTest.MessageBoxs.BarCodeBox("请输入条码/Vui lòng quét mã SN", int.Parse(Length), out string BarCode);
            BarCode = BarCode.ToUpper();
            if (BarCode.Contains("TX"))
                Config["BindingSN"] = BarCode;
            else return "False SN 错误";
            return flag ? BarCode : "False";

        }
        /// <summary isPublicTestItem="true">
        /// 根据BarCodeSN条码获取BD号
        /// </summary>
        /// <returns>info</returns>
        string QueryBDNoBySN()
        {
            if(SystematicName.ToString().ToLower() == "chorme")
            {
                string Value;
                string sn = (string)Config["BindingSN"];
                string BD = "";
                Value = CmdMes((int)ChormeID.QueryBDofSN, $"{sn};", out string msg);
                if (Value != "True") return $"{Value}  False";
                Config["BitAddressByBindingSN"] = BD = msg.Split(';')[1];
                return BD;
            }
            else
            {
                string SN = (string)Config["BindingSN"];
                Config["BitAddressByBindingSN"] = form.MESBD.GetBDNoBysn(SN);
                return Config["BitAddressByBindingSN"].ToString();
            }
        }
        /// <summary isPublicTestItem="true">
        /// 根据SN条码获取BD号
        /// </summary>
        /// <returns>info</returns>
        string QueryBDNoBySN0()
        {
            if (SystematicName.ToString().ToLower() == "chorme")
            {
                string Value;
                string sn = (string)Config["BindingSN"];
                string BD = "";
                Value = CmdMes((int)ChormeID.QueryBDofSN, $"{sn};", out string msg);
                if (Value != "True") return $"{Value}  False";
                Config["BitAddressByBindingSN"] = BD = msg.Split(';')[1];
                return BD;
            }
            else
            {
                string SN = (string)Config["SN"];
                Config["BitAddress"] = form.MESBD.GetBDNoBysn(SN);
                //formdata[1] = Config["LincenseKey"].ToString();
                formdata[4] = Config["BitAddress"].ToString();
                return Config["BitAddress"].ToString();
            }
        }


        /// <summary isPublicTestItem="false">
        /// 检查SN合法性
        /// </summary>
        /// <returns>info</returns>
        public string CheckSN()
        {
            string SN = (string)Config["SN"];
            MesFlag = (int)Config["MesFlag"];
            if (SN.Contains("TE_BZP")) return true.ToString();

            switch (SystematicName.ToString().ToLower())
            {
                case "chorme": goto Chorme;
                default: goto 翔威;
            }
        翔威:
            if (MesFlag < 1) return true.ToString();
            string message = "";
            if (MesFlag >= 2)
            {
                try
                {
                    string SNconfirm = mMes.mesInst.GetWOANDPartNo(SN);
                    if (!(SNconfirm.Contains((Config["Works"]).ToString())))
                    {
                        Config["TestResultFlag"] = false;
                        MessageBox.Show($"此条码不属于当前工单/ Mã SN không thuộc công đơn.  'SN：{SN}，Return Value: {SNconfirm}'");
                        return false.ToString();
                    }
                    message = mMes.mesInst.checkSN_Station(SN, Station);
                    //生存log给Burnin程序使用
                    string log = $"SN: {Config["SN"]} Time: {DateTime.Now:yy-MM-dd  HH:mm:ss} \n{message} \n\n";
                    Config["MesError"] = log;
                    //结束生存log
                    if (message.Contains("OK"))
                    {
                        return true.ToString();
                    }
                    Config["TestResultFlag"] = false;
                    if (message.Contains("ROUTE END"))
                    {
                        string[] msg = message.Split('(', '/');
                        message = $"{message} \n此条码的途程正在 { msg[1]}, 当测试站的途程是 {msg[2]} \nMã SN này đang ở trạm test {msg[1]}, Khác với trạm test hiện tại {msg[2]}";
                    }
                    else if (message.Contains("Route NG"))
                    {
                        string[] msg = message.Split(':', '/');
                        message = $"{message} \n此条码的途程正在 { msg[1]}, 当测试站的途程是 {msg[2]} \nMã SN này đang ở trạm test {msg[1]}, Khác với trạm test hiện tại {msg[2]}";
                    }
                    MessageBoxShow(message);
                    return message;
                    //}
                    //return message;
                }
                catch (Exception e)
                {
                    Config["TestResultFlag"] = false;
                    MessageBox.Show(e.StackTrace);
                    return false.ToString();
                }
            }
            // 下面的是MES Flag = 1
            try
            {
                message = mMes.mesInst.checkSN_Station(SN, Station);
                //生存log给Burnin程序使用
                string log = $"SN: {Config["SN"]} Time: {DateTime.Now:yy-MM-dd  HH:mm:ss} \n{message} \n\n";
                Config["MesError"] = log;
                //结束生存log
                if (message.Contains("OK"))
                {
                    return true.ToString();
                }
                Config["TestResultFlag"] = false;
                if(message.Contains("ROUTE END"))
                {
                    string[] msg = message.Split('(', '/');
                    message = $"{message} \n此条码的途程正在 { msg[1]}, 当测试站的途程是 {msg[2]} \nMã SN này đang ở trạm test {msg[1]}, Khác với trạm test hiện tại {msg[2]}";
                }
                else if (message.Contains("Route NG"))
                {
                    string[] msg = message.Split(':', '/');
                    message = $"{message} \n此条码的途程正在 { msg[1]}, 当测试站的途程是 {msg[2]} \nMã SN này đang ở trạm test {msg[1]}, Khác với trạm test hiện tại {msg[2]}";
                }
                MessageBoxShow(message);
                return message;
                //}
                //return message;
            }
            catch (Exception e)
            {
                Config["TestResultFlag"] = false;
                MessageBox.Show(e.StackTrace);
                return false.ToString();
            }
        Chorme:
            if (MesFlag <= 0) return true.ToString();
            string Value;
            Value = CmdMes((int)ChormeID.ExistsSN, $"{ SN};{Works};");
            if (Value != "True")
            {
                return Value;
            }

            return CmdMes((int)ChormeID.CheckSN, SN + ";");
        }
        public void MessageBoxShow(string MEG, MessageBoxButtons Buttons = MessageBoxButtons.OK)
        {
            MessageBox.Show(MEG, "MES提示", Buttons, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1);
        }

        /// <summary isPublicTestItem="false">
        /// 检查用户权限
        /// </summary>
        /// <returns>info</returns>
        public string CheckUser()
        {
            try
            {
                //userID = "91203868";
                MesFlag = (int)Config["MesFlag"];
                BDFlag = (bool)Config["GetBDFlag"];
                FailCount = (int)Config["FailCount"];
                PassCount = (int)Config["PassCount"];
                WorkOrderLength = (int)Config["WorkOrderLength"];
                if (MesFlag >= 2)
                {
                    var name = Config["Name"].ToString().Split('-')[0];
                    var station = ((string)Config["Station"]);
                    Station = $"{name}_{station}";
                }
                else Station = ((string)Config["Station"]);
                TypeName = (string)Config["Name"];
                BDStation = Station;
                CustomerName = (string)Config["CustomerName"];
                SystematicName = Config["SystematicName"].ToString().ToLower();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                System.Windows.Forms.MessageBox.Show("MES 加载失败请重启程序");
                return "MES 加载失败请重启程序";
            }
            switch (SystematicName.ToString().ToLower())
            {
                case "chorme": goto Chorme;
                case "xiangwei": goto 翔威;
                default: goto 翔威;
            }

            翔威:
            #region 翔威
            if (MesFlag == 0 && !BDFlag) return "未启动";
            Form1 form2 = new Form1(WorkOrderLength);
            form2.ShowDialog();
            Config["Works"] = Works;
            Config["UserID"] = userID;
            //Config["OrderNumberInfomation"] = GetOrderNumberInfomation();
            return (form2.DialogResult == DialogResult.OK).ToString();
            #endregion


            Chorme:
            #region Chorme

            if (MesFlag <= 0) return "Chorme系统：未连接";
            try
            {
                ChormeFlag = true;
                MES.SajetTransStart();
                Thread.Sleep(1500);


            }
            catch (System.Exception ex)
            {
                MES.SajetTransClose();
                MessageBox.Show(ex.Message + "启动连接MES出问题了，请重启程序");
            }
            try
            {
                ChormeLogin form1 = new ChormeLogin(WorkOrderLength);
                while (true)
                {

                    form1.ShowDialog();
                    Config["UserID"] = userID = form1.tb_User.Text;
                    //Config["Works"] = Works = form1.tb_OrderWorks.Text;
                    Config["Works"] = Works = form1.tb_OrderWorks.Text;
  
                    string ReceiveDATA = CmdMes((int)ChormeID.CheckUser, form1.tb_User.Text);
                    if (ReceiveDATA == true.ToString())
                    {
                        return "Chorme系统：已连接";
                    }
                    form1.tb_ReadInfo.Text = ReceiveDATA;
                    if (form1.DialogResult == DialogResult.No)
                    {
                        CloseMes();
                        return "Chorme系统：连接失败";
                    }
                }
            }
            finally
            {
                if (BDFlag)
                {
                    form = ShowInformation.Forms(Works, Station, userID, SystematicName);
                    string info = ChormeUpdataInfo();
                    form.Msg = info;
                    form.Show();

                }
            }

            #endregion
        }

        /// <summary isPublicTestItem="false">
        /// 上传测试结果到MES系统
        /// </summary>
        /// <returns>info</returns>
        public bool UpdataMes()
        {
            bool Flag = (bool)Config["TestResultFlag"];
            bool MoreTest = OnceConfig.ContainsKey("SN");
            string SN = MoreTest?(string)OnceConfig["SN"]:(string)Config["SN"];
            MesFlag = (int)Config["MesFlag"];
            if (SN.Contains("TE_BZP") )
            {
                return Flag;
            }

            switch (SystematicName.ToString().ToLower())
            {
                case "chorme": goto Chorme;
                default: goto 翔威;
            }
            翔威:
            #region 翔威
            
            if (MesFlag >= 1)
            {
                //if (Flag) Flag = SendTestResult(Flag, SN) ? Flag : false;
                SendTestResult(Flag, SN);
                Config["TestResultFlag"] = Flag;
                return Flag;
            }
            else
            {
                Config["TestResultFlag"] = Flag;
                return Flag;
            }
            #endregion

            Chorme:
            #region Chorme

            string IsOK = Flag ? "OK;" : "NG;";
            string cmd = $@"{userID};{SN};{IsOK}";
            if (MesFlag >= 1)
            {
                string UpdataCmd = $@"{userID};{SN};{updataChormeString((ListView)Config["TestTable"])};";
                string a = CmdMes((int)ChormeID.UploadData, UpdataCmd);
                bool Result = a == "True";
                Flag = Result ? Flag : false;

                if (Flag)
                {
                    string msg = CmdMes((int)ChormeID.UploadSN, cmd);
                    Flag = msg == "True" ? Flag : false;
                    if (!Flag) MessageBox.Show(msg);
                }
                else
                {
                    //Messagebox mes = new Messagebox();
                    //mes.Text = "Chorme MES";
                    //mes.ShowDialog();
                    //if (mes.DialogResult == DialogResult.OK)
                    //{
                    cmd = $@"{cmd} {ErrorCode};N;";
                    CmdMes((int)ChormeID.UploadSN, cmd);
                    // }
                }
                return Flag;
            }
            else
            {
                return Flag;
            }
            #endregion
        }
       
        //窗体关闭事件
        public string CloseMes()
        {
            if (ChormeFlag) Process.GetCurrentProcess().Kill();
            return "";
        }
        #region 翔威

        public bool GetBD()
        {
            string DeviceBD = (string)Config["DeviceBD"];
            string SN = (string)Config["SN"];
            form.MESBD.GetBTADFromMES(SN,DeviceBD, out string BTAD, out string msg, out string LicenseKey);
            Config["LincenseKey"] = LicenseKey;
            if (BTAD.Length != 12) return false;
            if (msg.Contains("END")) return false;
            if (!msg.Contains("EXIST"))
            {
                if (!form.MESBD.UploadBTADResultToMES(SN, false))
                {
                    MessageBox.Show("上传BDA错误，请停止测试，联系工程师修复");
                    return false;

                }
            }
            else
            {
                Config["LincenseKey"] = "EXIST";
            }
            Config["BitAddress"] = BTAD;
            try
            {
                form.ShowInformation_Load(null, null);
            }
            catch
            {
            }


            return true;

        }
        public bool SetBD()
        {
            bool Flag = (bool)Config["TestResultFlag"];
            string SN = (string)Config["SN"];
            if (Flag) return form.MESBD.UploadBTADResultToMES(SN, Flag);
            form.MESBD.UploadBTADResultToMES(SN, Flag);
            return false;
        }


        //public string GetOrderNumberInfomation() => mESBDA.CheckMOInfo((string)Config["Works"]);

        
        /// <summary isPublicTestItem="true">
        /// 从用工单在MES捞BD
        /// </summary>
        /// <returns>info</returns>
        public bool GetBDFromMES()
        {
            string SN = (string)Config["SN"];
            if (SN.Trim().Length < 8)
            {
                Task.Run(() => MessageBox.Show("获取BD号的SN长度不能小于8"));
                return false;
            }
            string MESBD = form.MESBD.GetBDNoBysn(SN);
            string lincensekey = "EXIST";
            if (MESBD.Length != 12)
            {
                if (!form.MESBD.GetBTADFromMES(SN,"123456789012", out MESBD, out string msg, out lincensekey))
                {
                    MessageBox.Show("获取BD号失败");
                    return false;
                }
                if (msg.Contains("END"))
                {
                    MessageBox.Show(msg);
                    return false;
                }

                if (!Bluetooth_ID(SN, MESBD))
                {
                    MessageBox.Show("MES--Bluetooth_ID: 綁定SN和BDA錯誤");
                    return false;
                }


                if (!form.MESBD.UploadBTADResultToMES(SN, true))
                {
                    MessageBox.Show("上传BDA错误，请停止测试，联系工程师修复");
                    return false;
                }


                MESBD = form.MESBD.GetBDNoBysn(SN);
                if (MESBD.Length != 12)
                {
                    MessageBox.Show($"MES通过SN获取BD号失败: {MESBD}");
                    return false;
                }

            }
            try
            {
                form.ShowInformation_Load(null, null);
            }
            catch { }
            Config["BitAddress"] = MESBD;
            Config["LincenseKey"] = lincensekey;
            return true;
        }

        //Begin GetBDforDongle
        public string GetBDforBarcodeWindowSN()
        {
            string SN = (string)Config["BindingSN"];
            if (SN.Trim().Length < 8)
            {
                Task.Run(() => MessageBox.Show("获取BD号的SN长度不能小于8"));
                return false.ToString();
            }
            string MESBD = form.MESBD.GetBDNoBysn(SN);
            string lincensekey = "EXIST";
            if (MESBD.Length != 12)
            {
                if (!form.MESBD.GetBTADFromMES(SN, "123456789012", out MESBD, out string msg, out lincensekey))
                {
                    MessageBox.Show("获取BD号失败");
                    return false.ToString();
                }
                if (msg.Contains("END"))
                {
                    MessageBox.Show(msg);
                    return false.ToString();
                }

                if (!Bluetooth_ID(SN, MESBD))
                {
                    MessageBox.Show("MES--Bluetooth_ID: 綁定SN和BDA錯誤");
                    return false.ToString();
                }


                if (!form.MESBD.UploadBTADResultToMES(SN, true))
                {
                    MessageBox.Show("上传BDA错误，请停止测试，联系工程师修复");
                    return false.ToString();
                }


                MESBD = form.MESBD.GetBDNoBysn(SN);
                if (MESBD.Length != 12)
                {
                    MessageBox.Show("MES通过SN获取BD号失败");
                    return false.ToString();
                }

            }
            try
            {
                form.ShowInformation_Load(null, null);
            }
            catch { }
            Config["BitAddressByBindingSN"] = MESBD;
            //Config["LincenseKey"] = lincensekey;
            return MESBD;
        }
        //End GetBDforDongle

        // BD写入成功上传MES
        public bool UploadBTADResultToMES(bool flag)
        {
            string SN = (string)Config["SN"];
            return form.MESBD.UploadBTADResultToMES(SN, flag);
        }

        //获取SN_BCCode
        public string GetSN_BCCode()
        {
            //string SN = (string)Config["SN"];
            //string SN_BCCode = mESBDA.GetPolyMX681SN(Works, SN, out string message);
            //if (SN_BCCode == null)
            //{
            //    MessageBox.Show($@"无SN返回信息：{message}");
            //    return $"{message} False";
            //}
            //Config["SN_BCCode"] = SN_BCCode;
            //return SN_BCCode;
            return "";

        }
        
        /// <summary>
        /// 过站
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="SN"></param>
        /// <returns></returns>
        private bool SendTestResult(bool flag, string SN)
        {

            try
            {
                //begin repair 不良代码名称的规格：机型_站别编号_测试项目编号 2023/26/10
                if (MesFlag >= 2 && !flag)
                {
                    int item_indexs = 0;
                    bool MoreTest = OnceConfig.ContainsKey("SN");
                    if (MoreTest)
                    {
                        System.Data.DataTable Datatgridview = (System.Data.DataTable)OnceConfig["TestTable"];
                        var items_count = Datatgridview.Rows.Count;
                        for (int i = 0; i < items_count; i++)
                        {
                            var a = (Datatgridview.Rows[i][5]).ToString();
                            item_indexs= item_indexs==items_count ? items_count : i + 1;
                            bool NGflag = CompareItemResult((Datatgridview.Rows[i][5]).ToString(), (Datatgridview.Rows[i][3]).ToString(), (Datatgridview.Rows[i][4]).ToString());
                            if ((Datatgridview.Rows[i][5]).ToString().Contains("False") || (Datatgridview.Rows[i][5]).ToString() == ""|| !NGflag)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        System.Windows.Forms.ListView tabel = (ListView)Config["TestTable"];
                        var items_count = tabel.Items.Count;
                        for (int item_index = 0; item_index < items_count; item_index++)
                        {
                            item_indexs = item_index + 1;
                            if (tabel.Items[item_index].SubItems[6].Text == "" || tabel.Items[item_index].SubItems[6].Text.Contains("False"))
                            {
                                break;
                            }
                        }

                        if (item_indexs > items_count) item_indexs = items_count;

                    }

                    var model = Config["Name"].ToString().Split('-')[0];
                    var stationName = "";
                    if ((Config["Station"].ToString()).Split(' ', '_').Length >= 2)
                    {
                            stationName = ((Config["Station"].ToString()).Split(' ', '_')[1]).Replace("T", "").Replace(".", "");
                    }
                    else
                    {
                        stationName = ((Config["Station"].ToString())).Replace("T", "").Replace(".", "");
                    }
                    var TestItemNumber = item_indexs.ToString("D3");
                    ErrorCode = $"{model}_{stationName}_{TestItemNumber}";
                    var str = mMes.mesInst.sendTestResult(userID, SN, Station, flag ? "OK;" : "NG;" + ErrorCode + ";");//过站接口
                    if (str.Contains("OK")) return true;
                    if (MoreTest)
                    {
                        Messagebox message = new Messagebox($"线程 ID: [{OnceConfig["TestID"]}]\nSN: {SN}, ErrorCode: {ErrorCode}\nMes Respond: '{str}'");
                        message.ShowDialog();
                    }
                    else
                    {
                        Messagebox message = new Messagebox($"SN: {SN}, ErrorCode: {ErrorCode}\nMes Respond: '{str}'");
                        message.ShowDialog();
                    }
                    
                }
                else
                {
                    ErrorCode = Config["ErrorCode"].ToString();
                    var str = mMes.mesInst.sendTestResult(userID, SN, Station, flag ? "OK;" : "NG;" + ErrorCode + ";");//过站接口
                    if (str.Contains("OK")) return true;
                    //MessageBox.Show($"SN: {SN}, ErrorCode: {ErrorCode}\nMes Respond: '{str}'");
                    Messagebox message = new Messagebox($"SN: {SN}, ErrorCode: {ErrorCode}\nMes Respond: '{str}'");
                    message.ShowDialog();
                }

                //end Repair
                //ErrorCode = Config["ErrorCode"].ToString();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            return false;
        }
        private bool CompareItemResult(string result, string lowLimit, string UpLimit)
        {
            try
            {
                if (result == lowLimit && result == UpLimit || lowLimit.Contains("N/A")|| UpLimit.Contains("N/A")) return true;
                if (double.Parse(result) <= double.Parse(lowLimit) || double.Parse(result) >= double.Parse(UpLimit)) return false;
            }
            catch
            {

            }
            return true;
        }

        
        private bool Bluetooth_ID(string sn, string bd)
        {
            try
            {
               // mMes.mesInst = new SwATE();
                var str = mMes.mesInst.Bluetooth_ID(sn, bd);//过站接口
                if (str.Contains("OK")) return true;
                MessageBox.Show("MES - Bluetooth_ID(SN, BD) -- " + str);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return false;
        }
        /// <summary isPublicTestItem="true">
        /// 韶音根据SN获取MAC地址
        /// </summary>
        /// <returns>info</returns>
        private string GetMAC_SY()
        {
            try
            {
                string sn = (string)Config["SN"];
                
                mMes = new _MES();
                mMes.mesInst = new SwATE(sMESUrl);
                var str = mMes.mesInst.GetMAC_SY(sn);
                if (str.Contains("NG")) return $"False {str}";
                Config["BitAddress"] = str;
                return str;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            return false.ToString();

        }


        /// <summary isPublicTestItem="true">
        /// 根据SN条码和料件规格获取UID
        /// </summary>
        /// <param name="IsSMT" options="true,false">是否在SMT测试</param>
        /// <param name="PN_Name" options="NULL">料件规格名称</param>
        /// <returns>info</returns>
        private string UID_Master(bool IsSMT,string PN_Name)
        {
            string UID = "";
            try
            {
                bool MoreTest = OnceConfig.ContainsKey("SN");
                string sn = MoreTest ? (string)OnceConfig["SN"] : (string)Config["SN"];
                if (IsSMT) return UID = sn;
                mMes = new _MES();
                mMes.mesInst = new SwATE(sMESUrl);
                var MesReturn = mMes.mesInst.GetKeyPartInfo(sn,PN_Name);
                if (MesReturn.Contains("NG;")) return $"False SN条码没有绑定UID的信息 Mã SN chưa liên kết với UID {MesReturn}";
                UID = MesReturn.Split(';')[2];
                Config["UID"] = UID;
                return UID;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            finally
            {
                OnceConfig["UID"] = Config["UID"] = UID;
            }
            return false.ToString();

        }
        /// <summary isPublicTestItem="true">
        /// 根据BarcodeSN条码和料件规格获取UID
        /// </summary>
        /// <param name="IsSMT" options="true,false">是否在SMT测试</param>
        /// <param name="PN_Name" options="NULL">料件规格名称</param>
        /// <returns>info</returns>
        private string UID_Slave(bool IsSMT, string PN_Name)
        {
            string UID = "";
            try
            {
                bool MoreTest = OnceConfig.ContainsKey("SN");
                string sn = MoreTest ? (string)OnceConfig["BarcodeSN"] : (string)Config["BindingSN"];
                if (IsSMT) return UID = sn;
                mMes = new _MES();
                mMes.mesInst = new SwATE(sMESUrl);
                var MesReturn = mMes.mesInst.GetKeyPartInfo(sn, PN_Name);
                if (MesReturn.Contains("NG;")) return $"False SN条码没有绑定UID的信息 Mã SN chưa liên kết với UID {MesReturn}";
                UID = MesReturn.Split(';')[2];
                Config["UID_D"] = UID;
                return UID;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            finally
            {
                OnceConfig["UID_D"] = Config["UID_D"] = UID;
            }
            return false.ToString();

        }
        /// <summary isPublicTestItem="true">
        /// 根据SN条码和料件Index获取UID
        /// </summary>
        /// <param name="IsSMT" options="true,false">是否在SMT测试</param>
        /// <param name="Index" options="NULL">料件顺序</param>
        /// <returns>info</returns>
        string LogiUIDForIndex(bool IsSMT, int Index)
        {
            string UID = "";
            try
            {
                Index = Index * 2;
                bool MoreTest = OnceConfig.ContainsKey("SN");
                string sn = MoreTest ? (string)OnceConfig["SN"] : (string)Config["SN"];
                if (IsSMT) return UID = sn;
                mMes = new _MES();
                mMes.mesInst = new SwATE(sMESUrl);
                var MesReturn = mMes.mesInst.GetKeyPartInfo(sn, "");
                if (MesReturn.Contains("NG;")) return $"False SN条码没有绑定UID的信息 Mã SN chưa liên kết với UID {MesReturn}";
                string[] strings = MesReturn.Split(';');
                if (strings.Count() < Index) return $"BinSN:{strings.Count()}<{Index} False";
                UID = strings[Index];
                return UID;
            }
            finally
            {
                OnceConfig["UID"] = Config["UID"] = UID;
            }
        }
        /// <summary>
        /// 上传测试数据
        /// </summary>
        /// <param name="SN"></param>
        /// <param name="TestResult"></param>
        /// <param name="Testvalue"></param>
        /// <returns></returns>
        private bool MISSendTestValue(string SN, string TestResult, string Testvalue)
        {
            return myeap.InsertTestProInfo("", "", SN, CustomerName, TypeName, "", Works, Station, "", TestResult, "", "", "", Testvalue, "", "") > 0;
        }

        static ShowInformation form;
        /// <summary>
        /// 浮动的BD窗体
        /// </summary>
        /// <param name="Station"></param>
        internal void UpdateInfo()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            form = ShowInformation.Forms(Works, BDStation, userID, SystematicName);
            form.Show();
        }
        /// <summary>
        /// 窗体登录
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static  bool login()
        {
            try
            {
                mMes = new _MES();
                mMes.mesInst = new SwATE(sMESUrl);
                //弹出界面框 拿到UserID和工单号
                //string str = mMes.mesInst.checkEmpNo(userID, Station);
                //var str = mMes.mesInst.checkEmpNo()
                var str = mMes.mesInst.checkEmpNo(userID, Station);
                InvorkDll.Config = Config;
                string peerInfo = mMes.mesInst.GetPart($"{Works}");
                InvorkDll.Config["OrderNumberInformation"] = peerInfo;
                string WorksIsOK = InvorkDll.CallMethod("Config_ini", "GetMysqlPartNumberInfo", new object[] { $"{peerInfo}" });
                if (str.Contains("OK")) return true;
                else
                {
                    MessageBox.Show(str);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            return false;
        }

        string GerResult(ListView view)
        {
            string result = "";
            for (var j = 0; j < view.Items.Count; j++)
            {
                result += view.Items[j].SubItems[5].Text + ",";
            }
            return result;
        }
        #endregion
        #region Chorme

        unsafe private string CmdMes(short cmdID, string Cmd)//命令格式   傳送內容
        {
            CmdMes(cmdID, Cmd, out string Value);
            return Value.Split(';')[0].Contains("OK") ? true.ToString() : $"{Value} False";
        }
        unsafe private string CmdMes(short cmdID, string cmd, out string msg)//命令格式   傳送內容
        {
            msg = "Flase";
            try
            {
                Boolean bResult = false;
                byte[] sData = System.Text.Encoding.Default.GetBytes(cmd);
                //System.Text.Encoding.Default.GetBytes(f_sData);
                Int32 iLen = sData.Length;
                if (sData.Length < 2048) Array.Resize(ref sData, 2048);

                fixed (byte* sTransData = &sData[0])
                {
                    bResult = MES.SajetTransData(cmdID, sTransData, &iLen);
                }

                Array.Resize(ref sData, iLen);
                //value = System.Text.Encoding.GetEncoding("gb2312").GetString(sData);
                msg = cmd = Encoding.Default.GetString(sData);
                //System.Text.Encoding.Default.GetString(sData);
                return cmd.Split(';')[0].Contains("OK") ? true.ToString() : $"{cmd} False";
            }
            catch (Exception ex)
            {
                File.AppendAllText($".\\TestData\\{DateTime.Now.ToString("MM_dd")}_bug.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                return "False";
            }

        }
        private string updataChormeString(ListView view)
        {
            string @string = "";
            for (var j = 0; j < view.Items.Count; j++)
            {
                string time = DateTime.Now.ToString("yyyy - MM - dd HH:mm:ss");
                string Result = view.Items[j].SubItems[6].Text == true.ToString() ? "P" : "F";
                string testitem = view.Items[j].SubItems[1].Text.Replace(";", "").Replace("#", "").Replace(",", "");
                string ResultData = view.Items[j].SubItems[5].Text.Replace(";", "").Replace("#", "").Replace(",", "");
                string LowLimit = view.Items[j].SubItems[3].Text.Replace(";", "").Replace("#", "").Replace(",", "");
                string UpLimit = view.Items[j].SubItems[4].Text.Replace(";", "").Replace("#", "").Replace(",", "");
                if (view.Items[j].SubItems[5].Text != "True" && view.Items[j].SubItems[5].Text != "False")
                {

                    @string += $@"{testitem},{Result},{ResultData},{LowLimit},{UpLimit},{time},#";
                    continue;
                }
                @string += $@"{testitem},{Result},,,,{time},#";
            }
            return @string;
        }
        //private string Binding(string user, string sn)
        //{
        //    string cmd = $@"{user};{sn};{ Config["BindingSN"] };";
        //    return CmdMes((int)ChormeID.Binding, cmd);

        //}
        private string Binding(string sn, string bindingSN)
        {
            try
            {
                mMes = new _MES();
                mMes.mesInst = new SwATE(sMESUrl);
                //sn = (string)Config["SN"];
                //bindingSN = (string)Config["BindingSN"];

                var str = mMes.mesInst.InsertDCSN(sn, 11, bindingSN);
                if (str.Contains("OK")) return true.ToString();
                MessageBox.Show(str);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            return false.ToString();

        }


        //add binding SN mondaydev 22.8.22
        /// <summary isPublicTestItem="true">
        /// SN和BarcodeSN条码互相绑定
        /// </summary>
        /// <returns>info</returns>
        public string BindingSN()
        {
            
            try
            {
                string HeadsetSN = (string)Config["SN"];
                string DongleSN = (string)Config["BindingSN"];

                mMes = new _MES();
                mMes.mesInst = new SwATE(sMESUrl);
                //获取已与耳机条码绑定的条码
                var str = mMes.mesInst.Query_Link_SN(HeadsetSN, 11);
                if (str.Contains(DongleSN)) return true.ToString();
                if (str.Contains("OK")) 
                {
                    MessageBox.Show($"耳机条码已经绑定：{str} /Mã SN tai nghe đã bangding với Dongle {str} ");
                    return "False" + str;
                } 
                //获取已经与Dongle条码绑定的条码
                var str2 = mMes.mesInst.Query_Link_SN(DongleSN, 10);
                if (str2.Contains("OK"))
                {
                    MessageBox.Show($"Dongle条码已经绑定：{str2} / Mã SN của Dongle đã bangding với tai nghe : {str2}");
                    return "False" + str2;
                }
                //进行绑定
                var str1 = mMes.mesInst.InsertDCSN(HeadsetSN, 11, DongleSN);
                if (str1.Contains("OK")) return true.ToString();
                if(str1.Contains("BOM IS NULL"))
                {
                    str1 = $"{str1} \n此料号在系统没有维护，无法绑定，请拍照图片发给工程PE处理\n Mã liệu này chưa được weihu trên hệ thống, không thể bangding, Vui lòng liên hệ PE công trình xử lí";
                }
                MessageBox.Show(str1);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            return false.ToString();
            
                                                                              
        }
        private string QueryKeyPart()
        {

            if (CmdMes((int)ChormeID.Query, $"{Config["BindingSN"]};", out string msg) == "True")
            {
                if (msg.Contains(Config["SN"].ToString()))
                    return Config["BindingSN"].ToString();
            }
            return $"扫码的SN{Config["BindingSN"]}||返回信息  {msg} False";

        }
        /// <summary isPublicTestItem="true">
        /// 检查Dongle条码和Headset条码是否已绑定
        /// </summary>
        /// <returns>info</returns>
        private string CheckBinding()
        {
            try
            {
                string sn = (string)Config["SN"];
                string bindingSN = (string)Config["BindingSN"];

                mMes = new _MES();
                mMes.mesInst = new SwATE(sMESUrl);
                //sn = (string)Config["SN"];
                //bindingSN = (string)Config["BindingSN"];

                var str = mMes.mesInst.Query_Link_SN(sn, 11);
                if (str.Contains(bindingSN)) return bindingSN;
                return $"False {str.Split(';')[1]}";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            return false.ToString();

        }
        public string GetBindingSN()
        {
            MessageBox.Show("123");
            try
            {
                
                MessageBox.Show((string)Config["SN"]);
                string sn = (string)Config["SN"];
                //string bindingSN = (string)Config["BindingSN"];
                MessageBox.Show(sn);
               // mMes = new _MES();
               SwATE ms= new SwATE();
                //sn = (string)Config["SN"];
                //bindingSN = (string)Config["BindingSN"];
                MessageBox.Show(sn);

                var str = ms.Query_Link_SN(sn, 11);
              //  string str = "";
                if (str.Contains("OK"))
                {
                    string[] bindingSN = str.Split(';');
                    Config["BindingSN"] = bindingSN[1];
                    return bindingSN[1];
                }
                MessageBox.Show(str);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            return false.ToString();

        }

        public string GetBDfromBindingSN()
        {
            string bindingSN = GetBindingSN();
            string SN = (string)Config["BindingSN"];
            if (SN.Trim().Length < 8)
            {
                Task.Run(() => MessageBox.Show("获取BD号的SN长度不能小于8"));
                return false.ToString();
            }
            string MESBD = form.MESBD.GetBDNoBysn(SN);
            string lincensekey = "EXIST";
            if (MESBD.Length != 12)
            {
                if (!form.MESBD.GetBTADFromMES(SN, "123456789012", out MESBD, out string msg, out lincensekey))
                {
                    MessageBox.Show("获取BD号失败");
                    return false.ToString();
                }
                if (msg.Contains("END"))
                {
                    MessageBox.Show(msg);
                    return false.ToString();
                }

                if (!Bluetooth_ID(SN, MESBD))
                {
                    MessageBox.Show("MES--Bluetooth_ID: 綁定SN和BDA錯誤");
                    return false.ToString();
                }


                if (!form.MESBD.UploadBTADResultToMES(SN, true))
                {
                    MessageBox.Show("上传BDA错误，请停止测试，联系工程师修复");
                    return false.ToString();
                }


                MESBD = form.MESBD.GetBDNoBysn(SN);
                if (MESBD.Length != 12)
                {
                    MessageBox.Show($"MES通过SN获取BD号失败: {MESBD}");
                    return false.ToString();
                }

            }
            try
            {
                form.ShowInformation_Load(null, null);
            }
            catch { }
            Config["BitAddressByBindingSN"] = MESBD;
            Config["LincenseKey"] = lincensekey;
            return MESBD;

        }

        private string CheckRXBinding()
        {
            try
            {
                string sn = (string)Config["SN"];
               // string bindingSN = (string)Config["BindingSN"];

                mMes = new _MES();
                mMes.mesInst = new SwATE(sMESUrl);
                //sn = (string)Config["SN"];
                //bindingSN = (string)Config["BindingSN"];

                var str = mMes.mesInst.Query_Link_SN(sn, 11);
                if (str.Contains("OK"))
                {
                    string[] bindingSN = str.Split(';');
                    if (MessageBox.Show($"False {sn} 已经与 {bindingSN[1]} 绑定", "Notification", MessageBoxButtons.OK) == DialogResult.OK)
                        return $"False {sn}已与{bindingSN[1]}绑定";
                }
                else
                {
                    if (MessageBox.Show("Ready Binding", "Notification", MessageBoxButtons.OK) == DialogResult.OK)
                        return true.ToString();
                }
                
            } 
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            return false.ToString();

        }

        private string CheckTXBinding()
        {
            try
            {
                //string sn = (string)Config["SN"];
                string sn = (string)Config["BindingSN"];

                mMes = new _MES();
                mMes.mesInst = new SwATE(sMESUrl);
                //sn = (string)Config["SN"];
                //bindingSN = (string)Config["BindingSN"];

                var str = mMes.mesInst.Query_Link_SN(sn, 11);
                if (str.Contains("OK"))
                {
                    string[] bindingSN = str.Split(';');
                    if (MessageBox.Show($"False {sn} 已经与 {bindingSN[1]} 绑定", "Notification", MessageBoxButtons.OK) == DialogResult.OK)
                        return $"False {sn}已与{bindingSN[1]}绑定";
                }
                else
                {
                    if (MessageBox.Show("Ready Binding", "Notification", MessageBoxButtons.OK) == DialogResult.OK)
                        return true.ToString();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            return false.ToString();

        }
        private string CustomerSNFromMES()
        {
            string sn = (string)Config["SN"];
            string Result = CmdMes((int)ChormeID.CustomerSN, sn, out string data);
            if (Result == "True")
                return (Config["CustomerSN"] = data.Split(';')[1]).ToString();

            return Result;

        }


        string GerBDFromMesChorme()
        {
            string Value;
            string sn = (string)Config["SN"];
            string BD = "";
            if (sn.Length < 8)
            {
                Task.Run(() => MessageBox.Show("获取BD号的SN长度不能小于8"));
                return "SN Length<8 False";
            }
            Config["LincenseKey"] = "EXIST";
            Value = CmdMes((int)ChormeID.QueryBDofSN, $"{sn};", out string msg);
            if (Value == "True")
            {
                BD = msg.Split(';')[1];
            }
            string GetBDfromMes = $"{userID};{sn};{Works};{BD};";
            Value = CmdMes((int)ChormeID.GetBDofMes, GetBDfromMes, out string BD_CVC);
            if (Value == "True")
            {
                string[] bd_cvc = BD_CVC.Split(';');
                Config["BitAddress"] = bd_cvc[1].Split(',')[1];
                Config["LincenseKey"] = bd_cvc[3];
                string BD_Result = "1";
                Value = CmdMes((int)ChormeID.UplongBDResultToMes, $"{userID};{sn};{Works};{Config["BitAddress"]};{bd_cvc[3]};{BD_Result};");
                if (Value != "True") return "False";
                return bd_cvc[1].Split(',')[1];
            }
            else
            {
                Task.Run(() => MessageBox.Show($"工单没有BD号，返回信息：{BD_CVC}"));
            }
            Config["BitAddress"] = "EXIST";
            Config["LincenseKey"] = "EXIST";

            return $"{BD_CVC}  False";

        }
        string ChormeUpdataInfo()
        {
            string cmd = $"{Works};N;";
            CmdMes((int)ChormeID.GetBitAddressMes, cmd, out string msg);
            string[] Msg = msg.Split(';');
            if (Msg[0] == "OK")
            {
                StringBuilder @string = new StringBuilder();
                int count = 15;

                @string.Append("工单".PadRight(count, ' ') + "：" + Works + "\r\n");
                //BD总数减去取用次数
                int surplus = int.Parse(Msg[4].Split(',')[1]) - int.Parse(Msg[8].Split(',')[1]);
                @string.Append("未使用BD数量".PadRight(count, ' ') + "：" + surplus + "\r\n");
                @string.Append("已使用BD数量".PadRight(count, ' ') + "：" + Msg[9].Split(',')[1] + "\r\n");
                @string.Append("已回收BD数量".PadRight(count, ' ') + "：" + Msg[10].Split(',')[1] + "\r\n");
                @string.Append("BD总数".PadRight(count, ' ') + "：" + Msg[4].Split(',')[1] + "\r\n");
                @string.Append("----------------------------".PadRight(count, ' ') + "\r\n");
                @string.Append("前次BD".PadRight(count, ' ') + "：" + Msg[7].Split(',')[1] + "\r\n");
                @string.Append("起始BD".PadRight(count, ' ') + "：" + Msg[5].Split(',')[1] + "\r\n");
                @string.Append("结束BD".PadRight(count, ' ') + "：" + Msg[6].Split(',')[1] + "\r\n");
                @string.Append(Msg[11] + "\r\n");
                return @string.ToString();
            }
            return "當前工單還沒有分配BDA號號段";
        }
        #endregion
    }
}
