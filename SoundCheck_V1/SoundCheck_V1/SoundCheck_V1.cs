using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoundCheck_V1.API;
using SoundCheck_V1.TemplateANC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace MerryDllFramework
{
    /// <summary dllName="SoundCheck_V1">
    /// SoundCheck 控制类
    /// </summary>
    public class MerryDll : IMerryAllDll
    {

        #region Dll Info
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        public Dictionary<string, object> Config;
        public WindowControl fw = new WindowControl();

        public bool TE_BZP;

        public object Interface(Dictionary<string, object> Config)
        {

            return anc.Config = this.Config = Config;
        }
        string dllVersion = "当前Dll版本：23.12.06.0";
        string computerName = Environment.MachineName;

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：SoundCheck_V1";
            string dllfunction = "Dll功能说明 ：控制Sound check";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "Dll改动信息：逐渐试跑ANC，开发Airoha芯片搭配SC使用首版";
            string dllChangeInfo2 = "23.8.29.0：优化点击F2弹窗搜索时间长，点击失败概率大问题";
            string dllChangeInfo3 = "23.11.8.0：增加ANC通用校准方法，增加自动上传Logi SQC文件数据";
            string dllChangeInfo4 = "23.11.18.0：SQC文件卡控，标准品不再跳过卡控，只有量产模式卡控";
            string dllChangeInfo5 = "23.12.06.0：SQC文件卡控，工程模式如果没有 .sqc 可以从系统下载一份";





            string[] info = { dllname, dllfunction,
                dllVersion, dllChangeInfo,dllChangeInfo2
            };
            return info;
        }
        #endregion


        //######################################################################## Run方法区 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public string Run(object[] Command)
        {

            try
            {
                SplitCMD(Command, out string[] cmd);
                switch (cmd[1])
                {
                    case "ControlSequence": return ControlSequence();

                    case "OpenSoundCheck": return OpenSoundCheck();
                    case "ConnectSoundCheck": return ConnectSoundCheck();
                    case "EveryRunSequence": return EveryRunSequence(cmd[2], cmd[3], int.Parse(cmd[4]));
                    case "StartTest": return StartTest(int.Parse(cmd[2]));
                    case "StartTestB": return StartTestB(int.Parse(cmd[2]), bool.Parse(cmd[3]));
                    case "GetStepResult": return GetStepResult(cmd[2]);
                    case "GetFinalResults": return GetFinalResults();
                    case "SendKeyToWindow": return SendKeyToWindow(cmd[2], cmd[3]);
                    case "SearchFormsName": return SearchFormsName(cmd[2], int.Parse(cmd[3]));
                    case "SaveLog": return SaveLog();
                    case "RunSequence": return RunSequence();
                    case "ANCAdjust": return ANCAdjust(cmd[2], cmd[3], cmd[4], cmd[5], cmd[6], int.Parse(cmd[7]), int.Parse(cmd[8]));
                    case "ANCAdjust_BySensitivity": return ANCAdjust_BySensitivity(cmd[2], cmd[3], cmd[4], int.Parse(cmd[5]), int.Parse(cmd[6]));
                    case "ANCAdjust_ByCurves": return ANCAdjust_ByCurves(cmd[2], cmd[3], int.Parse(cmd[4]), int.Parse(cmd[5]));
                    case "StartTestC": return StartTestC(int.Parse(cmd[2]));
                    case "Adjust_ANC_ByCurves": return Adjust_ANC_ByCurves(cmd[2], cmd[3], cmd[4]);
                    case "Calibration_ANC_ByCurves": return Calibration_ANC_ByCurves(cmd[2], cmd[3], cmd[4], cmd[5], cmd[6], cmd[7]);
                    case "SearchFromsOrFile": return SearchFromsOrFile(cmd[2], bool.Parse(cmd[3]), cmd[4], int.Parse(cmd[5]), out _, out _);
                    case "AllCurveInfomationIsUploadedToMysql": return AllCurveInfomationIsUploadedToMysql();
                    case "ExistsFileOrForms": return ExistsFileOrForms(cmd[2], bool.Parse(cmd[3]), cmd[4], int.Parse(cmd[5]), out _, out _);
                    case "ExistsFile": return ExistsFile(cmd[2], int.Parse(cmd[3]));
                    case "DeleteAllTxt": return DeleteAllTxt(cmd[2]);
                    case "Calibration_ANC_ByCurves_2": return Calibration_ANC_ByCurves_2(cmd[2], cmd[3], cmd[4], cmd[5], cmd[6], int.Parse(cmd[7]));
                    case "Calibration_Sensitivity": return Calibration_Sensitivity(cmd[2], double.Parse(cmd[3]), cmd[4], cmd[5], cmd[6], int.Parse(cmd[7]));

                    case "UploadLogiAcousticsData": return UploadLogiAcousticsData();
                    case "ANC_Calibration": return ANC_Calibration();

                    default: return "Connend Error False";


                }
            }
            catch (Exception ex)
            {

                return $"Error {ex.Message} False";
            }

        }

        public string SequenceDic
        {
            get
            {
                return Path.GetDirectoryName((string)this.Config["SqcPath"]);
            }
        }

        public string SN
        {
            get
            {
                return this.Config["SN"].ToString();
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
            TE_BZP = Config["SN"].ToString().Contains("TE_BZP");
        }

        #region  常规使用
        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 常规使用 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string _Cut_OffRule()
        {
            return "True";
        }

        /// <summary isPublicTestItem="true">
        ///  0、管控SQC文件
        /// </summary>
        /// <returns></returns>
        public string ControlSequence()
        {

            //if (Convert.ToBoolean(Config["EngineerMode"]))
            //    return "标准品";


            Stopwatch SW = Stopwatch.StartNew();
            string SqcName = $"{Config["Name"]}_{Config["Station"]}";
            string processPath = $@"{Config["adminPath"]}\TestItem\{SqcName}\{SqcName}.sqc";
            FileInfo info = new FileInfo(processPath);
            string result = "";
            bool isDownload = false;
            if ((bool)Config["EngineerMode"])
            {
                if (!File.Exists(processPath))
                    result = VersionControl.CheckSequenceVersion(info, out isDownload);
            }
            else
            {
                result = VersionControl.CheckSequenceVersion(info, out isDownload);
            }
            //string result = VersionControl.CheckSequenceVersion(info, out bool isDownload);
            Console.WriteLine($"CheckSequenceVersion:{SW.ElapsedMilliseconds}");
            if (result.ContainsFalse())
                return result;
            info.Refresh();
            string Receive = OpenSoundCheck();
            if (Receive.Contains("False"))
                return Receive;
            Receive = SoundCheck.ConnectSoundCheck();
            Console.WriteLine($"ConnectSoundCheck:{SW.ElapsedMilliseconds}");
            if (Receive.ContainsFalse())
                return Receive;
            string SequenceName = SoundCheck.GetSequenceName();
            Console.WriteLine($"GetSequenceName:{SW.ElapsedMilliseconds}");
            if (SequenceName.ContainsFalse())
                return SequenceName;
            if (isDownload || SequenceName != Path.GetFileNameWithoutExtension(info.Name))
            {
                Receive = SoundCheck.RunSequence(info.FullName);
                if (Receive.ContainsFalse())
                    return Receive;

            }
            return "True";

        }

        /// <summary isPublicTestItem="false">
        /// 1、Sound Check 开始测试内包含（启动,建立连接，运行Sqc）
        /// </summary>
        /// <param name="Sleep">开始测试前延时 常规 “100” </param>
        /// <returns></returns>
        public string StartTest(int Sleep)
        {
            if (!SoundCheck.IsConnect || SoundCheck.FirstRunSequence)
            {
                string Receive = OpenSoundCheck();
                if (Receive.Contains("False"))
                    return Receive;
                Receive = SoundCheck.ConnectSoundCheck();
                if (Receive.Contains("False"))
                    return Receive;
                Receive = RunSequence();
                if (Receive.Contains("False"))
                    return Receive;
                Thread.Sleep(1000);
            }
            SoundCheck.SetSerialNumber((string)this.Config["SN"]);
            Thread.Sleep(Sleep);
            return SoundCheck.StartTest();
        }
        /// <summary isPublicTestItem="true">
        /// 1、Sound Check 开始测试内包含（启动,建立连接，运行Sqc）
        /// </summary>
        /// <param name="Sleep">开始测试前延时 常规 “100” </param>
        /// <param name="RunSequenceFlag" options="False,True"> 是否运行SQC文件</param>
        /// <returns></returns>
        public string StartTestB(int Sleep, bool RunSequenceFlag)
        {
            if (!SoundCheck.IsConnect || SoundCheck.FirstRunSequence)
            {
                string Receive = OpenSoundCheck();
                if (Receive.Contains("False"))
                    return Receive;
                Receive = SoundCheck.ConnectSoundCheck();
                if (Receive.Contains("False"))
                    return Receive;
                if (RunSequenceFlag)
                {
                    Receive = RunSequence();
                    if (Receive.Contains("False"))
                        return Receive;
                    Thread.Sleep(1000);
                }

            }
            SoundCheck.SetSerialNumber((string)this.Config["SN"]);
            Thread.Sleep(Sleep);
            return SoundCheck.StartTest();
        }

        /// <summary isPublicTestItem="true">
        /// 2、获取指定项目的测试结果 
        /// </summary>
        /// <param name="SetpName"></param>
        /// <returns></returns>
        public string GetStepResult(string SetpName)
        {
            return SoundCheck.GetStepResult(SetpName);
        }

        /// <summary isPublicTestItem="true">
        /// 3、获取总的测试结果
        /// </summary>
        /// <returns>info</returns>
        public string GetFinalResults()
        {
            return SoundCheck.GetFinalResults();
        }



        #endregion


        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ANC 测试特调区 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string Cut_OffRule()
        {
            return "True";
        }

        #region MyRegion

        /// <summary isPublicTestItem="false">
        /// 1、打开Sound Check 程序 已经打开后不会重复打开
        /// </summary>
        /// <returns>info</returns>
        public string OpenSoundCheck()
        {
            return SoundCheck.OpenSoundCheck((string)this.Config["SoundCheckPath"]);
        }

        /// <summary isPublicTestItem="false">
        /// 2、与 Sound Check 建立连接
        /// </summary>
        /// <returns></returns>
        public string ConnectSoundCheck()
        {
            return SoundCheck.ConnectSoundCheck();
        }

        /// <summary isPublicTestItem="false">
        /// 3 .A、执行Sqc文件、程序第一次启动或第一次启动Sound Check执行
        /// </summary>
        /// <returns>info</returns>
        public string RunSequence()
        {
            if (!SoundCheck.FirstRunSequence)
                return "True";
            string Result = SoundCheck.RunSequence((string)this.Config["SqcPath"]);
            if (!Result.Contains("False"))
                Thread.Sleep(1000);
            return Result;
        }

        /// <summary isPublicTestItem="false">
        ///  3 .B、每次执行Sqc文件
        /// </summary>
        /// <param name="Path">绝对路径、 相对路径填入快捷方式名称、 将快捷方式存放在TestItem\机型\SQC文件夹下</param>
        /// <param name="PathType" options="Absolute,Relative">绝对路径：Absolute 相对路径：Relative</param>
        /// <param name="Sleep">运行Sqc后的延时 1000</param>
        /// <returns>info</returns>
        public string EveryRunSequence(string Path, string PathType, int Sleep)
        {
            switch (PathType)
            {
                case "Relative":
                    Path = GetInkPath.GetPath($@"{Config["adminPath"]}\TestItem\{Config["Name"]}\SQC\{Path}");
                    break;
                case "Absolute":
                default:
                    break;
            }
            string Receive = SoundCheck.RunSequence(Path);
            if (!Receive.Contains("False"))
                Thread.Sleep(Sleep);
            return Receive;
        }

        #endregion


        /// <summary isPublicTestItem="true">
        /// 0、ANC用：Sound Check 开始测试内包含（启动，建立连接，运行Sqc，清除txt数据）
        /// </summary>
        /// <param name="Sleep">开始测试前延时 常规 “100” </param>
        /// <returns></returns>
        public string StartTestC(int Sleep)
        {
            if (!SoundCheck.IsConnect || SoundCheck.FirstRunSequence)
            {
                string Receive = OpenSoundCheck();
                if (Receive.Contains("False"))
                    return Receive;
                Receive = SoundCheck.ConnectSoundCheck();
                if (Receive.Contains("False"))
                    return Receive;
                Receive = RunSequence();
                if (Receive.Contains("False"))
                    return Receive;
                Thread.Sleep(1500);
            }
            DeleteAllTxt("default");
            SoundCheck.SetSerialNumber((string)this.Config["SN"]);
            Thread.Sleep(Sleep);
            return SoundCheck.StartTest();
        }
        #region MyRegion
        /// <summary isPublicTestItem="false">
        /// ANC 根据曲线上下限校准校准
        /// </summary>
        /// <param name="FuncName">Config字典上的委托名称</param>
        /// <param name="UpLimitName_L">SC 左项目上限名称</param>
        /// <param name="LowLimitName_L">SC 左项目下限名称</param>
        /// <param name="UpLimitName_R">SC 右项目上限名称</param>
        /// <param name="LowLimitName_R">SC 右项目下限名称</param>
        /// <param name="Sleep">测试前延时 方便等待指令生效时间 建议 300</param>
        /// <param name="TrimCount">校准次数  没想法写 10</param>
        /// <returns>info</returns>
        public string ANCAdjust(string FuncName, string UpLimitName_L, string LowLimitName_L, string UpLimitName_R, string LowLimitName_R, int Sleep, int TrimCount)
        {

            if (!Config.ContainsKey(FuncName))
                return $"Config Dictronary Not Found {FuncName} Func False";
            SoundCheck.SetSerialNumber((string)this.Config["SN"]);
            string SC_Receive;
            Thread.Sleep(100);
            SC_Receive = SoundCheck.StartTest();
            if (SC_Receive.Contains("False"))
                return $"SC_{SC_Receive}";
            SC_Receive = GetFinalResults();
            if (!SC_Receive.Contains("False"))
                return $"Adjust Count:{0} Result:{SC_Receive}";

            for (int i = 0; i < TrimCount; i++)
            {


                SC_Receive = SoundCheck.GetStepResultMargin(UpLimitName_L, out bool L_Up_Passed, out string L_Up_Margin);
                if (SC_Receive.Contains("False"))
                    return $"UpLimitName_L:{SC_Receive}";
                SC_Receive = SoundCheck.GetStepResultMargin(LowLimitName_L, out bool L_Low_Passed, out string L_Low_Margin);
                if (SC_Receive.Contains("False"))
                    return $"LowLimitName_L:{SC_Receive}";
                SC_Receive = SoundCheck.GetStepResultMargin(UpLimitName_R, out bool R_Up_Passed, out string R_Up_Margin);
                if (SC_Receive.Contains("False"))
                    return $"UpLimitName_R:{SC_Receive}";
                SC_Receive = SoundCheck.GetStepResultMargin(LowLimitName_R, out bool R_Low_Passed, out string R_Low_Margin);
                if (SC_Receive.Contains("False"))
                    return $"LowLimitName_R:{SC_Receive}";
                try
                {
                    Func<double, double, double, double, string> ANCAdjustAction = (Func<double, double, double, double, string>)Config[FuncName];
                    string Adjust = ANCAdjustAction(double.Parse(L_Up_Margin), double.Parse(L_Low_Margin), double.Parse(R_Up_Margin), double.Parse(R_Low_Margin));

                    if (Adjust.Contains("False"))
                        return Adjust;
                }
                catch (Exception ex)
                {

                    MessageBox.Show("func Erroe\r\n" + ex.ToString());
                    return $"{ex.Message} func Erroe False";
                }
                Thread.Sleep(Sleep);
                SC_Receive = SoundCheck.StartTest();
                if (SC_Receive.Contains("False"))
                    return $"SC_{SC_Receive}";
                SC_Receive = GetFinalResults();
                if (!SC_Receive.Contains("False"))
                    return $"Adjust Count:{i} Result:{SC_Receive}";


            }
            return $"Adjust Count：{TrimCount} False";

        }

        /// <summary isPublicTestItem="false">
        /// ANC 获取感度校准
        /// </summary>
        /// <param name="FuncName">Config字典上的委托名称</param>
        /// <param name="ResultName_L">SC 左 感度结果名称 </param>
        /// <param name="ResultName_R">SC 右 感度结果名称 </param>
        /// <param name="Sleep">测试前延时 方便等待指令生效时间 建议 300</param>
        /// <param name="TrimCount">校准次数  没想法写 10</param>
        /// <returns>info</returns>
        public string ANCAdjust_BySensitivity(string FuncName, string ResultName_L, string ResultName_R, int Sleep, int TrimCount)
        {

            if (!Config.ContainsKey(FuncName))
                return $"Config Dictronary Not Found {FuncName} Func False";
            SoundCheck.SetSerialNumber((string)this.Config["SN"]);
            string SC_Receive;
            Thread.Sleep(100);
            SC_Receive = SoundCheck.StartTest();
            if (SC_Receive.Contains("False"))
                return $"SC_{SC_Receive}";
            string onceResult = GetFinalResults();
            int i = 0;
            do
            {
                SC_Receive = SoundCheck.GetStepResultMargin(ResultName_L, out bool L_Passed, out string L_Margin, out string L_Limit);
                if (SC_Receive.Contains("False"))
                    return $"ResultName_L:{SC_Receive}";
                SC_Receive = SoundCheck.GetStepResultMargin(ResultName_R, out bool R_Passed, out string R_Margin, out string R_Limit);
                if (SC_Receive.Contains("False"))
                    return $"ResultName_R:{SC_Receive}";
                try
                {
                    Func<double, double, double, double, double, double, string> ANCAdjustAction = (Func<double, double, double, double, double, double, string>)Config[FuncName];
                    string Adjust = ANCAdjustAction(double.Parse(L_Margin), double.Parse(L_Limit.Split('/')[0]), double.Parse(L_Limit.Split('/')[1]), double.Parse(R_Margin), double.Parse(R_Limit.Split('/')[0]), double.Parse(R_Limit.Split('/')[1]));
                    if (Adjust.Contains("False"))
                        return Adjust;
                    if (Adjust.Contains("Not Adjust True") && i == 0)
                    {
                        return $"Adjust Count:{i} Result:{onceResult}";
                    }

                    i++;
                    Thread.Sleep(Sleep);
                    SC_Receive = SoundCheck.StartTest();
                    if (SC_Receive.Contains("False"))
                        return $"SC_{SC_Receive}";
                    SC_Receive = GetFinalResults();
                    if (Adjust.Contains("Not Adjust True"))
                    {
                        return $"Adjust Count:{i} Result:{SC_Receive}";
                    }
                    if (!SC_Receive.Contains("False"))// && i > 0)
                        return $"Adjust Count:{i} Result:{SC_Receive}";
                }
                catch (Exception ex)
                {

                    MessageBox.Show("func Erroe\r\n" + ex.ToString());
                    return $"{ex.Message} False";
                }


            } while (i < TrimCount);
            return $"Adjust Count：{TrimCount} False";

        }

        /// <summary isPublicTestItem="false">
        /// ANC 根据曲线校准旧版
        /// </summary>
        /// <param name="FuncName">Config字典上的委托名称</param>
        /// <param name="FilePath">txt曲线数据完整路径</param>
        /// <param name="Sleep">测试前延时 方便等待指令生效时间 建议 300</param>
        /// <param name="TrimCount">校准次数  没想法写 3</param>
        /// <returns>info</returns>
        public string ANCAdjust_ByCurves(string FuncName, string FilePath, int Sleep, int TrimCount)
        {

            if (!Config.ContainsKey(FuncName))
                return $"Config Dictronary Not Found {FuncName} Func False";
            SoundCheck.SetSerialNumber((string)this.Config["SN"]);
            string SC_Receive;
            int i = 0;
            Thread.Sleep(100);
            SC_Receive = SoundCheck.StartTest();
            if (SC_Receive.Contains("False"))
                return $"SC_{SC_Receive}";
            string onceResult = GetFinalResults();
            do
            {
                try
                {
                    Func<string, string>
                        ANCAdjustAction =
                        (Func<string, string>)Config[FuncName];

                    string Adjust = ANCAdjustAction(FilePath);
                    if (Adjust.Contains("False"))
                        return Adjust;
                    if (Adjust.Contains("Not Adjust True") && i == 0)
                    {
                        return $"Adjust Count:{i} Result:{onceResult}";
                    }
                    i++;
                    Thread.Sleep(Sleep);
                    SC_Receive = SoundCheck.StartTest();
                    if (SC_Receive.Contains("False"))
                        return $"SC_{SC_Receive}";

                    SC_Receive = GetFinalResults();
                    if (Adjust.Contains("Not Adjust True"))
                    {
                        return $"Adjust Count:{i} Result:{onceResult}";
                    }
                    if (!SC_Receive.Contains("False"))
                        return $"Adjust Count:{i} Result:{SC_Receive}";
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Dll里面的方法报错了 func Erroe\r\n" + ex.ToString());
                    return $"{ex.Message} False";
                }


            } while (i < TrimCount);


            return $"Adjust Count：{TrimCount} False";

        }

        /// <summary isPublicTestItem="false">
        /// 保存所有曲线上传到数据库
        /// </summary>
        /// <returns></returns>
        public string AllCurveInfomationIsUploadedToMysql()
        {
            if (!SoundCheck.IsStartTest)
                return "Not Test False";
            string receive = SoundCheck.GetAllCurves(out Dictionary<string, Dictionary<string, object>> tokens).CorrectCharacter();
            if (receive.Contains("False"))
                return receive;
            string SequenceName = SoundCheck.GetSequenceName().CorrectCharacter();

            string SN = Config["SN"].ToString().CorrectCharacter();
            string TypeName = Config["Name"].ToString().CorrectCharacter();
            string TestTime = SoundCheck.GetSequenceDuration().CorrectCharacter();
            string Works = Config["Works"].ToString().CorrectCharacter();
            string Station = Config["Station"].ToString().CorrectCharacter();

            string SQL = "(SN,Sequence,Station,TestTime,CurveName,CurveData,workorders,MachineName,dllver) values ";
            StringBuilder Valus = new StringBuilder();
            foreach (var item in tokens)
            {
                string CurveData = JsonConvert.SerializeObject(item.Value).CorrectCharacter();
                string CurveName = item.Key.CorrectCharacter();
                Valus.Append($@"('{SN}','{SequenceName}','{Station}','{TestTime}','{CurveName}','{CurveData}','{Works}','{computerName}','{dllVersion}'),");
            }

            string result = MySqlTestData.ExecuteSql(TypeName, SQL + Valus.Remove(Valus.Length - 1, 1)).ToString();

            SoundCheck.IsStartTest = false;

            return result;
        }

        /// <summary isPublicTestItem="false">
        /// ANC 根据曲线数据进行校准
        /// </summary>
        /// <param name="FuncName">Func 执行体 赋值给Config字典名</param>
        /// <param name="_FilePath">测试数据 txt 名</param>
        /// <param name="FormsName"></param>
        /// <returns></returns>
        public string Adjust_ANC_ByCurves(string FuncName, string _FilePath, string FormsName)
        {
            string DirectoryPath = SequenceDic;
            string TestData = $@"{DirectoryPath}\{_FilePath}";
            string PassText = $@"{DirectoryPath}\PASS.txt";
            string TestEND = $@"{DirectoryPath}\TEST-END.txt";
            int i = 0;
            string F2 = "";
            string ResultInfo = "";

            for (; i < 3; i++)
            {
                try
                {
                    bool click = i > 0;

                    string FileResult = fw.ExistsFile($"{TestData}|{TestEND}", 20000);
                    if (FileResult.Contains("False"))
                        return $"Not Found {_FilePath} _{i} error => 001 False";
                    FileResult = fw.ExistsFile($"{TestEND}", 100);
                    if (FileResult.Contains("True"))
                        return $"Found TEST-END.txt Test End _{i} False";
                    if (click)
                    {
                        if (fw.ExistsFile($"{PassText}", 200).Contains("True"))
                        {
                            F2 = ExistsFileOrForms(FormsName, "F2");
                            if (F2.Contains("False"))
                                return $"OK Test Count _{i} SendKey F2 error => 003 {F2}";
                            else
                                return $"OK Test Count _{i}";
                        }
                    }
                    Func<string, string> ANCAdjustAction = (Func<string, string>)Config[FuncName];
                    string Adjust = ANCAdjustAction(TestData);
                    if (Adjust.Contains("False"))
                        ResultInfo = $"“_{i} error => 006 {Adjust}”";
                    if (Adjust.Contains("Not Adjust True") && fw.ExistsFile($"{PassText}", 200).Contains("True"))
                    {
                        F2 = ExistsFileOrForms(FormsName, "F2");
                        if (F2.Contains("False"))
                            return $"OK Test Count _{i} SendKey F2 error => 004 {F2}";
                        else
                            return $"OK Test Count _{i}";
                    }
                    DeleteAllTxt(DirectoryPath);
                    string F4 = ExistsFileOrForms(FormsName, "F4");
                    if (F4.Contains("False"))
                        return $"_{i} SendKey F4 error => 005 {F4}";

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dll里面的方法报错了 func Erroe\r\n" + ex.ToString());
                    return $"{ex.Message} False";
                }
            }
            return $"NG Adjust Count _{i} {ResultInfo} False";

        }




        /// <summary isPublicTestItem="true">
        /// 识别弹框  识别到Final Test Result.txt 就Fail 
        /// </summary>
        /// <param name="FormsName">窗体名称</param>
        /// <param name="isbutton" options="True,False">是否点击弹窗</param>
        /// <param name="Key">按键</param>
        /// <param name="TimeOut">超时 千分一秒</param>
        /// <returns></returns>
        public string SearchFromsOrFile(string FormsName, bool isbutton, string Key, int TimeOut, out bool IsFile, out bool IsForms)
        {

            string TestEND = $@"{SequenceDic}\Final Test Result.txt";
            string FileResult = "";
            string FormsResult = "";
            IsFile = false;
            IsForms = false;
            Thread FileThread = new Thread(() =>
            {
                FileResult = fw.ExistsFile(TestEND, TimeOut);
            });
            Thread FormsThread = new Thread(() =>
            {
                if (isbutton)
                {
                    FormsResult = fw.SendKeyToWindow(FormsName, Key, TimeOut);

                }
                else
                {
                    FormsResult = fw.SearchWindow(FormsName, TimeOut);

                }
            });
            FileThread.Start();
            FormsThread.Start();
            double Count = (TimeOut + 3000) / 50;
            for (int i = 0; i < Count; i++)
            {
                if (FileResult != "" || FormsResult != "")
                {
                    break;
                }
                Thread.Sleep(50);
            }

            if (FileThread.IsAlive)
                FileThread.Abort();
            if (FormsThread.IsAlive)
                FormsThread.Abort();
            IsFile = !FileResult.Contains("True");
            IsForms = FormsResult.Contains("True");
            if (FormsResult != "")
                return FormsResult;
            if (FileResult.Contains("True"))
                return $"Found File {Path.GetFileName(TestEND)} False";
            return $"Time Out Null False";
        }


        /// <summary isPublicTestItem="false">
        ///  通用机型开发校准方法 单曲线
        /// </summary>
        /// <param name="FuncName">Func 执行体 赋值给Config字典名</param>
        /// <param name="Frequency">频率</param>
        /// <param name="CurvesName">曲线的名称</param>
        /// <param name="Uplimit">上限的名称</param>
        /// <param name="LowLimit">下限的名称</param>
        /// <param name="TestCount">校准次数 要跟SC 对应</param>
        /// <returns></returns>
        public string Calibration_Sensitivity(
            string FuncName, double Frequency,
            string CurvesName, string Uplimit,
            string LowLimit, int TestCount
            )
        {
            string DirectoryPath = SequenceDic;
            string _Pass = $@"{DirectoryPath}\Calibration Pass.txt";
            string _Data = $@"{DirectoryPath}\Calibration Data.txt";
            string TestEND = $@"{DirectoryPath}\Final Test Result.txt";
            string FormsName = $"Calibration_Forms";
            string FilePath = Path.GetDirectoryName((string)this.Config["SqcPath"]);
            string ResultInfo = "";
            string F2 = "";
            string F4 = "";
            int i = 0;
            TestCount += 1;
            for (; i < TestCount; i++)
            {
                try
                {
                    string scResult = fw.ExistsFile($"{_Data}|{TestEND}", 20000);
                    //if (TE_BZP)
                    //{
                    //    F2 = ExistsFileOrForms(FormsName, "F2", 15000);
                    //    return "标准品";
                    //}


                    if (scResult.ContainsFalse())
                        return $"Not {scResult} _{i} {Path.GetFileName(TestEND)} error => S401 False";
                    scResult = fw.ExistsFile($"{TestEND}", 100);
                    if (scResult.Contains("True"))
                        return $"Found Final {Path.GetFileName(TestEND)} Test End _{i} False";
                    bool PassTxtResult = fw.ExistsFile($"{_Pass}", 100).Contains("True");

                    if (PassTxtResult)
                    {
                        F2 = ExistsFileOrForms(FormsName, "F2", 6000);
                        return F2.ContainsFalse()
                        ? $"OK Test Count _{i} SendKey F2 error => S403 {F2}"
                        : $"OK Test Count _{i}";
                    }
                    if (i <= TestCount - 1)
                    {
                        CurvesInfo.info _CurvesInfo = new CurvesInfo.info();
                        MerryDllFramework.CurvesInfo.ReadFB_SCDataA(_Data, LowLimit, CurvesName, Uplimit, ref _CurvesInfo);
                        int ValueIndex = MerryDllFramework.CurvesInfo.ScreenApproachFigure(_CurvesInfo.L_Curves.Xdata, Frequency);
                        int UplimitIndex = MerryDllFramework.CurvesInfo.ScreenApproachFigure(_CurvesInfo.L_Curves_Uplimit.Xdata, Frequency);
                        int LowlimitIndex = MerryDllFramework.CurvesInfo.ScreenApproachFigure(_CurvesInfo.L_Curves_Lowlimit.Xdata, Frequency);

                        Func<string, string, string, string> ANCAdjustAction = (Func<string, string, string, string>)Config[FuncName];
                        string Adjust = ANCAdjustAction($"{_CurvesInfo.L_Curves_Lowlimit.Ydata[LowlimitIndex]}", $"{_CurvesInfo.L_Curves.Ydata[ValueIndex]}", $"{_CurvesInfo.L_Curves_Uplimit.Ydata[UplimitIndex]}");
                        if (Adjust.Contains("False"))
                            ResultInfo = Adjust;
                    }

                    F4 = ExistsFileOrForms(FormsName, "F4", 6000);
                    if (F4.ContainsFalse())
                    {
                        F4 = ExistsFileOrForms(FormsName, "F4", 6000);
                        if (F4.ContainsFalse())
                            return $"_{i} SendKey F4 error => S406 {F4}";
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dll里面的方法报错了 func Erroe\r\n" + ex.ToString());
                    return $"{ex.Message} False";
                }
                finally
                {
                    DeleteAllTxt(DirectoryPath);
                    Thread.Sleep(100);
                }
            }
            return $"NG Adjust _{i} {ResultInfo} False";

        }

        ANC_CalibrationMethod anc = new ANC_CalibrationMethod();

        /// <summary isPublicTestItem="true">
        /// ANC 通用校准方法
        /// </summary>
        /// <returns></returns>
        public string ANC_Calibration()
        {
            Method.SequenceDic = anc.SequenceDic = this.SequenceDic;
            anc.ANC_ParameterPath = $@"{Config["adminPath"]}\TestItem\{Config["Name"]}\ANC_Parameter.ini"; ;

            anc.ReadANCConfig();
            return anc.Calibration();
        }




        #endregion





        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 辅组方法区 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string __Cut_OffRule()
        {
            return "True";
        }

        //每测覆盖，本地数据不可删 不可改
        /// <summary isPublicTestItem="true">
        /// 上传Logi 声学Execl数据到服务器 注:每测更新
        /// </summary>
        /// <returns></returns>
        public string UploadLogiAcousticsData()
        {
            try
            {


                if (!(bool)Config["LogiLogFlag"])
                {
                    MessageBox.Show($"AE声学曲线Execl数据上传数据服务器 必须启动上传Logi数据，需设定 LogiLogFlag=1", "SoundCheck_V1提示");
                    return "LogiLogFlag NG False";

                }

                string filePath = $@"";

                DateTime _dateTime = new DateTime();

                DirectoryInfo folder = new DirectoryInfo($@"{SequenceDic}\LogiAcousticsData");
                foreach (FileInfo file in folder.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (file.Extension.Equals(".xlsx") && file.Name.Contains("LogiData") && file.Name.Contains($"{Config["Name"]}"))
                    {
                        string[] str = file.Name.Split(' ')[0].Split('-');
                        if (str.Length < 3)
                            continue;
                        int.TryParse(str[0], out int Year);
                        int.TryParse(str[1], out int Month);
                        int.TryParse(str[2], out int Day);
                        if (Year == DateTime.Now.Year && Month == DateTime.Now.Month && Day == DateTime.Now.Day)
                            if (file.LastWriteTimeUtc > _dateTime)
                                filePath = file.FullName;
                    }
                }
                if (!File.Exists(filePath))
                    return "Not Found File False";
                string UploadAcousticsDataRes = UploadAcousticsData(
                     VersionControl.URL_UploadLGAcousticData,
                    $"{Config["Name"]}", $"{Config["Station"]}|{Config["LogiStation"]}",
                    $"{Config["LogiProject"]}_{Config["LogiStage"]}",
                    Environment.MachineName, filePath,
                    Path.GetFileName(filePath));
                //MessageBox.Show(UploadAcousticsDataRes);
                Dictionary<string, string> UploadAcousticsDataInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(UploadAcousticsDataRes);
                if (UploadAcousticsDataInfo["status"] == "200")
                {
                    return "True";
                }
                else
                {
                    MessageBox.Show($"AE声学曲线Execl数据上传服务器失败\r\n返回信息={UploadAcousticsDataRes}", "SoundCheck_V1提示");
                    return "Upload False";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 上传声学xlsx文件
        /// </summary>
        /// <param name="url">API地址</param>
        /// <param name="Model">机型名称</param>
        /// <param name="StationName">站别名称（测试站名称|LG站别名称）</param>
        /// <param name="LGInfo">传入LG文件名称前缀信息（LogiProject_LogiStage）</param>
        /// <param name="MachineName">传入电脑编号</param>
        /// <param name="FilePath">文件绝对路径</param>
        /// <param name="FieName">文件名称</param>
        /// <returns></returns>
        static string UploadAcousticsData(string url, string Model, string StationName, string LGInfo, string MachineName, string FilePath, string FieName)
        {
            try
            {
                string responseContent;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                request.ContentType = "multipart/form-data; boundary=" + boundary;

                using (Stream requestStream = request.GetRequestStream())
                {
                    // 添加FromfromData参数
                    string formData = "--" + boundary + "\r\n";
                    formData += $"Content-Disposition: form-data; name=\"Model\"\r\n\r\n{Model}\r\n";
                    formData += "--" + boundary + "\r\n";
                    formData += $"Content-Disposition: form-data; name=\"StationName\"\r\n\r\n{StationName}\r\n";
                    formData += "--" + boundary + "\r\n";
                    formData += $"Content-Disposition: form-data; name=\"LGInfo\"\r\n\r\n{LGInfo}\r\n";
                    formData += "--" + boundary + "\r\n";
                    formData += $"Content-Disposition: form-data; name=\"MachineName\"\r\n\r\n{MachineName}\r\n";
                    byte[] formDataBytes = Encoding.UTF8.GetBytes(formData);
                    requestStream.Write(formDataBytes, 0, formDataBytes.Length);

                    // 添加文件
                    string fileHeader = "--" + boundary + "\r\n";
                    fileHeader += $"Content-Disposition: form-data; name=\"LGAcousticData\"; filename=\"{FieName}\"\r\n";
                    fileHeader += "Content-Type: application/octet-stream\r\n\r\n";
                    byte[] fileHeaderBytes = Encoding.UTF8.GetBytes(fileHeader);
                    requestStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);

                    using (FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead = 0;
                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            requestStream.Write(buffer, 0, bytesRead);
                        }
                    }
                    // 添加请求体结束标志
                    byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                    requestStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
                return responseContent;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        /// <summary isPublicTestItem="true">
        /// 搜索窗体 模拟点击按键
        /// </summary>
        /// <param name="FormsName">窗体名称</param>
        /// <param name="Key">按键 比如 F2</param>
        /// <returns></returns>
        public string SendKeyToWindow(string FormsName, string Key)
        {
            return fw.SendKeyToWindow(FormsName, false, Key, out string text).ToString();

        }
        /// <summary isPublicTestItem="true">
        /// 搜索窗体
        /// </summary>
        /// <param name="FormsName">窗体名称</param>
        /// <param name="Time">搜索时间  毫秒</param>
        /// <returns></returns>
        public string SearchFormsName(string FormsName, int Time)
        {
            return fw.SearchWindow(FormsName, Time);
        }
        /// <summary isPublicTestItem="false">
        ///   识别或点击弹窗  识别到TEST-END.txt 就Fail 
        /// </summary>
        /// <param name="FormsName">窗体名称</param>
        /// <param name="isbutton" options="True,False">是否点击弹窗</param>
        /// <param name="Key">按键</param>
        /// <param name="TimeOut">超时 千分一秒</param>
        /// <returns></returns>
        public string ExistsFileOrForms(string FormsName, bool isbutton, string Key, int TimeOut, out bool IsFile, out bool IsForms)
        {

            string TestEND = $@"{SequenceDic}\TEST-END.txt";
            string FileResult = "";
            string FormsResult = "";
            IsFile = false;
            IsForms = false;
            Thread FileThread = new Thread(() =>
            {
                FileResult = fw.ExistsFile(TestEND, TimeOut);
            });
            Thread FormsThread = new Thread(() =>
            {
                if (isbutton)
                {
                    FormsResult = fw.SendKeyToWindow(FormsName, Key, TimeOut);

                }
                else
                {
                    FormsResult = fw.SearchWindow(FormsName, TimeOut);

                }
            });
            FileThread.Start();
            FormsThread.Start();
            double Count = (TimeOut + 3000) / 50;
            for (int i = 0; i < Count; i++)
            {
                if (FileResult != "" || FormsResult != "")
                {
                    break;
                }
                Thread.Sleep(50);
            }
            if (FileThread.IsAlive)
                FileThread.Abort();
            if (FormsThread.IsAlive)
                FormsThread.Abort();
            IsFile = !FileResult.Contains("True");
            IsForms = FormsResult.Contains("True");
            if (FormsResult != "")
                return FormsResult;
            if (FileResult.Contains("True"))
                return $"Found File {Path.GetFileName(TestEND)} False";
            return $"Time Out Null False";
        }
        /// <summary isPublicTestItem="false">
        /// 搜索Sequence目录下 指定文件
        /// </summary>
        /// <param name="FileName">文件名</param>
        /// <param name="TimeOut">超时 毫秒</param>
        /// <returns></returns>
        public string ExistsFile(string FileName, int TimeOut)
        {
            return fw.ExistsFile($@"{SequenceDic}\{FileName}", TimeOut);
        }

        /// <summary isPublicTestItem="false">
        /// 清除所有txt文件
        /// </summary>
        /// <param name="FilePath">输入指定路径清除 或 使用Sqc路径 输入“default”</param>
        public string DeleteAllTxt(string FilePath)
        {
            switch (FilePath)
            {
                case "default":
                    foreach (var item in Directory.GetFiles(SequenceDic, "*.txt"))
                    {
                        File.Delete(item);
                    }
                    break;

                default:
                    foreach (var item in Directory.GetFiles(FilePath, "*.txt"))
                    {
                        File.Delete(item);
                    }
                    break;
            }
            return "True";
        }
        /// <summary isPublicTestItem="true">
        /// 生成Sound Tcp指令日记
        /// </summary>
        /// <returns>True</returns>
        public string SaveLog()
              => SoundCheck.SaveLog();

        //######################################     ANC部分隐藏方法     ######################################

        /// <summary isPublicTestItem="false">
        ///  被洛达芯片方法调用 第一次使用的方法
        /// </summary>
        /// <param name="FuncName">Func 执行体 赋值给Config字典名</param>
        /// <param name="PassFileName">Pass文件名</param>
        /// <param name="CurvesDataFileName">曲线数据文件名</param>
        /// <param name="FormsName">判断窗体名</param>
        /// <param name="ShowCalibration" options="False,True">显示校准算法</param>
        /// <param name="FinalTestResult">测试结束</param>
        /// <returns></returns>
        public string Calibration_ANC_ByCurves(string FuncName,
            string PassFileName, string CurvesDataFileName,
            string FormsName, string ShowCalibration, string FinalTestResult)
        {
            string DirectoryPath = SequenceDic;
            string TestData = $@"{DirectoryPath}\{CurvesDataFileName}";
            string PassText = $@"{DirectoryPath}\{PassFileName}";
            string TestEND = $@"{DirectoryPath}\{FinalTestResult}";
            int i = 0;
            string F2 = "";
            string SN = Config["SN"].ToString();
            string ResultInfo = "";


            for (; i < 3; i++)
            {
                try
                {
                    bool 第一次测试 = i > 0;
                    string SC测试结果 = fw.ExistsFile($"{TestData}|{TestEND}", 20000);
                    if (SN.Contains("TE_BZP"))
                    {
                        F2 = ExistsFileOrForms(FormsName, "F2", 15000);
                        return "标准品";
                    }
                    if (SC测试结果.Contains("False"))
                        return $"Not {SC测试结果} _{i} error => 001 False";
                    SC测试结果 = fw.ExistsFile($"{TestEND}", 100);
                    if (SC测试结果.Contains("True"))
                        return $"Found Final {FinalTestResult} Test End _{i} False";
                    if (第一次测试)
                    {
                        if (fw.ExistsFile($"{PassText}", 200).Contains("True"))
                        {
                            Thread.Sleep(500);
                            F2 = ExistsFileOrForms(FormsName, "F2");

                            if (F2.Contains("False"))
                                return $"OK Test Count _{i} SendKey F2 error => 003 {F2}";
                            else
                                return $"OK Test Count _{i}";
                        }
                    }
                    Func<bool, string> ANCAdjustAction = (Func<bool, string>)Config[FuncName];
                    string Adjust = ANCAdjustAction(bool.Parse(ShowCalibration));
                    if (Adjust.Contains("False"))
                    {
                        ResultInfo = $"“_{i} error => 006 {Adjust}”";
                    }
                    if (Adjust.Contains("Not Adjust True") && fw.ExistsFile($"{PassText}", 300).Contains("True"))
                    {
                        Thread.Sleep(500);
                        F2 = ExistsFileOrForms(FormsName, "F2");


                        if (F2.Contains("False"))
                            return $"OK Test Count _{i} SendKey F2 error => 004 {F2}";
                        else
                            return $"OK Test Count _{i}";
                    }
                    DeleteAllTxt(DirectoryPath);
                    string F4 = ExistsFileOrForms(FormsName, "F4");
                    if (F4.Contains("False"))
                    {
                        Thread.Sleep(1000);
                        F4 = ExistsFileOrForms(FormsName, "F4");
                        if (F4.Contains("False"))
                            return $"_{i} SendKey F4 error => 005 {F4}";

                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dll里面的方法报错了 func Erroe\r\n" + ex.ToString());
                    return $"{ex.Message} False";
                }
            }
            return $"NG Adjust Count _{i} {ResultInfo} False";

        }

        /// <summary isPublicTestItem="false">
        ///  被洛达芯片方法调用 第二次使用的方法
        /// </summary>
        /// <param name="FuncName">Func 执行体 赋值给Config字典名</param>
        /// <param name="PassFileName">Pass文件名</param>
        /// <param name="CurvesDataFileName">曲线数据文件名</param>
        /// <param name="FormsName">判断窗体名</param>
        /// <param name="TestCount">校准次数</param>
        /// <param name="FinalTestResult">测试结束</param>
        /// <returns></returns>
        public string Calibration_ANC_ByCurves_2(string FuncName,
            string PassFileName, string CurvesDataFileName,
            string FormsName, string FinalTestResult, int TestCount)
        {
            string DirectoryPath = SequenceDic;
            string TestData = $@"{DirectoryPath}\{CurvesDataFileName}";
            string PassText = $@"{DirectoryPath}\{PassFileName}";
            string TestEND = $@"{DirectoryPath}\{FinalTestResult}";
            string Adjust = "";
            string ResultInfo = "";
            string F2 = "";
            string F4 = "";
            int i = 0;
            for (; i < TestCount; i++)
            {
                try
                {
                    string scResult = fw.ExistsFile($"{TestData}|{TestEND}", 20000);
                    if (TE_BZP)
                    {
                        F2 = ExistsFileOrForms(FormsName, "F2");
                        return "标准品";
                    }

                    if (scResult.ContainsFalse())
                        return $"Not {scResult} _{i} {CurvesDataFileName} error => S201 False";
                    scResult = fw.ExistsFile($"{TestEND}", 100);
                    if (scResult.Contains("True"))
                        return $"Found Final {FinalTestResult} Test End _{i} False";
                    if (i < TestCount - 1)
                    {
                        Func<string> ANCAdjustAction = (Func<string>)Config[FuncName];
                        Adjust = ANCAdjustAction();
                    }

                    if (Adjust.Contains("Continue Adjust"))
                        ResultInfo = $"“_{i} error => S202 {Adjust}”";
                    if (Adjust.Contains("Not Adjust"))
                    {
                        F2 = ExistsFileOrForms(FormsName, "F2");
                        return F2.ContainsFalse()
                        ? $"OK Test Count _{i} SendKey F2 error => S205 {F2}"
                        : $"OK Test Count _{i}";
                    }
                    bool PassTxtResult = fw.ExistsFile($"{PassText}", 200).Contains("True");
                    if (Adjust.Contains("Adjust True") && PassTxtResult)
                    {
                        F2 = ExistsFileOrForms(FormsName, "F2");
                        return F2.ContainsFalse()
                        ? $"OK Test Count _{i} SendKey F2 error => S203 {F2}"
                        : $"OK Test Count _{i}";
                    }
                    if (TestCount - 1 == i && PassTxtResult)
                    {

                        F2 = ExistsFileOrForms(FormsName, "F2");
                        return F2.ContainsFalse()
                        ? $"OK Test Count _{i} SendKey F2 error => S204 {F2}"
                        : $"OK Test Count _{i}";
                    }

                    DeleteAllTxt(DirectoryPath);
                    F4 = ExistsFileOrForms(FormsName, "F4");
                    if (F4.ContainsFalse())
                    {
                        F4 = ExistsFileOrForms(FormsName, "F4");
                        if (F4.ContainsFalse())
                            return $"_{i} SendKey F4 error => S206 {F4}";

                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dll里面的方法报错了 func Erroe\r\n" + ex.ToString());
                    return $"{ex.Message} False";
                }
            }
            return $"NG Adjust _{i} {ResultInfo} False";

        }

        public string ExistsFileOrForms(string FormsName, string Key, int TimeOut = 3500)
        {
            string str = "Note Search Forms False";
            Thread.Sleep(50);
            // Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 2; i++)
            {
                str = ExistsFileOrForms(FormsName, true, Key, TimeOut, out _, out _);
                if (!str.Contains("False"))
                    break;
                Thread.Sleep(1000);
                str = $"Send {Key} Error {str}";
            }
            return str;
        }


        public string CloseSoundCheck()
        {

            //断开连接
            try
            {
                SoundCheck.SoundCheckExit();
            }
            catch { }
            //关闭程序

            return "";
        }
    }
}
