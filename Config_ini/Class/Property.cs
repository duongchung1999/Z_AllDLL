using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Config_ini.Class.HttpPostAPI;

namespace Config_ini.Class
{
    public class Property
    {
        Dictionary<string, object> Config;

        public Property(Dictionary<string, object> Config)
        {
            this.Config = Config;
            config._path = $"{Config["adminPath"]}\\Config\\CONFIG.ini";
        }
        #region 内部调用方法
        public void MesConfig()
        {
            foreach (var section in INIOperationClass.INIGetAllSectionNames(config._path))
            {
                foreach (var item in INIOperationClass.INIGetAllItems(config._path, section))
                {
                    string[] strs = item.Split('=');
                    if (strs.Length < 2) continue;
                    Config[strs[0]] = strs[1];
                }
            }
            NetworkFlag = config.GetValue("Flag", "NetworkFlag") != "0";
            try
            {
                GetMysqlConfig(config.GetValue("Config", "Name"), config.GetValue("Config", "Station"));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            #region [Flag]
            int value;
            int.TryParse(config.GetValue("Flag", "MesFlag"), out value);
            MesFlag = value;
            TE_MesFlag = config.GetValue("Flag", "TE_MesFlag") == "1";
            int.TryParse(config.GetValue("Mes", "PassCount"), out value);
            PassCount = value;
            int.TryParse(config.GetValue("Mes", "FailCount"), out value);
            FailCount = value;
            int.TryParse(config.GetValue("Mes", "WorkOrderLength"), out value);
            WorkOrderLength = value;
            int.TryParse(config.GetValue("Mes", "ErrorCodeLength"), out value);
            ErrorCodeLength = value == 0 ? 7 : value;
            Name = config.GetValue("Config", "Name");
            GetBDFlag = config.GetValue("Flag", "GetBDFlag") == "1";
            Station = config.GetValue("Config", "Station");
            CustomerName = config.GetValue("Mes", "CustomerName");
            SystematicName = config.GetValue("Mes", "SystematicName");

            GetEvent();
            GetTestControlConfig();
            foreach (System.Reflection.FieldInfo info in this.GetType().GetFields())
                Config[info.Name] = info.GetValue(this);
            #endregion

        }
        public string GetMysqlPartNumberInfo(string OrderNumber)
        {
            if (NetworkFlag)
            {
                string Sql = $"SELECT n3.config,n2.`name`  FROM part_no n1  ,model n2 ,part_no_config n3 where n1.`no` LIKE \"{OrderNumber}\" and  n1.model_id=n2.id AND n1.part_no_config_id=n3.id ;";
                string[][] table = null;
                MySqlHelper.GetDataList(Sql, out table);
                if (table.Length < 1)
                {
                    Task.Run(() => MessageBox.Show($"料号未维护在TE系统“{OrderNumber}”\r\n拍照将信息传回软件工程\r\n点击OK继续", "TE系统提示", MessageBoxButtons.OK));
                    return "该料号并未维护在系统 False";
                }
                PartNumberInfoStr = table[0][0];

                string[] partInfo = PartNumberInfoStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                this.Config["Name"] = Name = table[0][1];
                foreach (var item in partInfo)
                {
                    string[] info = item.Split('=');
                    if (info.Length > 1)
                        PartNumberInfos[info[0]] = info[1];
                }
                Config["PartNumberInfos"] = PartNumberInfos;
                return PartNumberInfoStr;
            }
            return "True";
        }

        public void GetTestControlConfig()
        {
            try
            {
                TestControl.Clear();
                string path = $"{Config["adminPath"]}/TestItem/{Name}/0_TestControl/TestControl.ini";
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (!File.Exists(path)) return;
                foreach (var item in INIOperationClass.INIGetAllSectionNames(path))
                {
                    if (int.TryParse(item, out int result))
                    {
                        Dictionary<string, string> section = new Dictionary<string, string>();
                        foreach (var sections in INIOperationClass.INIGetAllItems(path, item))
                        {
                            string[] sec = sections.Split('=');
                            section.Add(sec[0], sec[1]);

                        }
                        if (!section.ContainsKey("HidInfo")) section.Add("HidInfo", "");
                        if (!section.ContainsKey("HidInfo")) section.Add("Number", "");
                        TestControl.Add(result, section);
                    }
                }
                #region [TestModel]

                TestControlFlag = int.Parse(INIOperationClass.INIGetStringValue(path, "TestModel", "TestControlFlag", "0"));
                #endregion
            }
            catch (Exception ex)
            {
                Config["BugLog"] = $"TestControl.ini文件配置异常，请检查配置\r\n{ex}";
                MessageBox.Show("TestControl.ini文件配置异常，请检查配置\r\n" + ex.ToString(), "TE系统提示");
            }
        }


        //加载方法
        public void LoadConfig()
        {
            int value;
            // Name = config.GetValue("Config", "Name");
            Station = config.GetValue("Config", "Station");
            NetworkFlag = config.GetValue("Flag", "NetworkFlag") != "0";
            EngineerMode = config.GetValue("Flag", "EngineerMode") != "0";
            int.TryParse(config.GetValue("Flag", "MesFlag"), out value);
            MesFlag = value;
            GetMysqlConfig((string)Config["Name"], (string)Config["Station"]);
            try
            {

                #region [Comport] 
                _8342Comport = config.GetValue("Comport", "_8342Comport");
                _2303SComport = config.GetValue("Comport", "_2303SComport");
                SwitchComport = config.GetValue("Comport", "SwitchComport");
                MB1205CPort = config.GetValue("Comport", "MB1205CPort");
                Dm5dPort = config.GetValue("Comport", "Dm5dPort");
                NXP_RF_Comport = config.GetValue("Comport", "NXP_RF_Comport");
                #endregion

                #region [Delay]
                int.TryParse(config.GetValue("Delay", "StopDelay"), out value);
                StopDelay = value <= 1000 ? 1000 * 50 : value;
                int.TryParse(config.GetValue("Delay", "StartDelay"), out value);
                StartDelay = value <= 0 ? 1000 : value;
                #endregion

                #region [Config]
                int.TryParse(config.GetValue("Config", "Barcode"), out value);
                Barcode = value;
                int.TryParse(config.GetValue("Config", "WireCountEnd"), out value);
                WireCountEnd = value;
                int.TryParse(config.GetValue("Config", "WireIncrease"), out value);
                WireIncrease = value;
                TE_BZPLimit = int.TryParse(config.GetValue("Config", "TE_BZPLimit"), out value) ? value : 10;
                #endregion

                #region [Flag]

                TestLogUploadMySQL = config.GetValue("Flag", "TestLogUploadMySQL") != "0";
                UploadFTP = config.GetValue("Flag", "UploadFTP") == "1";
                SoundcheckDataUploadFTP = config.GetValue("Flag", "SoundcheckDataUploadFTP") == "1";

                #endregion

                #region [TestModel]
                TestModelFlag = config.GetValue("TestModel", "TestModelFlag") == "1";
                CheckDevice = config.GetValue("TestModel", "CheckDevice") == "1";
                Pid = config.GetValue("TestModel", "Pid");
                Vid = config.GetValue("TestModel", "Vid");
                _Pid = config.GetValue("TestModel", "_Pid");
                _Vid = config.GetValue("TestModel", "_Vid");
                #endregion

                #region [Loop]
                //LoopFlag = config.GetValue("Loop", "LoopFlag") == "1";
                //if (MesFlag > 0) LoopFlag = false;
                //int.TryParse(config.GetValue("Loop", "TestNumbers"), out value);
                //TestNumbers = value;
                #endregion

                #region [SoundCheck]
                //SoundCheckFlag = config.GetValue("SoundCheck", "SoundCheckFlag") == "1";
                SoundCheckPath = config.GetValue("SoundCheck", "SoundCheckPath");
                SqcPath = config.GetValue("SoundCheck", "SqcPath");
                #endregion

                #region [Visa]
                RT550_IP = config.GetValue("Visa", "RT550_IP");
                RT550Port = config.GetValue("Visa", "RT550Port");
                MT8852BGPIB = config.GetValue("Visa", "MT8852BGPIB");

                #endregion

                #region [Print]
                Print = config.GetValue("Print", "Print");
                PrintFileName = config.GetValue("Print", "PrintFileName");

                #endregion

                #region [Program]
                int.TryParse(config.GetValue("Program", "WireCount"), out value);
                WireCount = value;
                TopMostFlag = config.GetValue("Program", "TopMostFlag") == "1";
                Maximized = config.GetValue("Program", "Maximized") == "1";
                MinimumSize = config.GetValue("Program", "MinimumSize") == "1";
                ComputerName = (config.GetValue("Program", "ComputerName"));
                int.TryParse(config.GetValue("Program", "TestPassCount"), out value);
                TestPassCount = value;
                int.TryParse(config.GetValue("Program", "DayOfYear"), out value);
                DayOfYear = value;
                int.TryParse(config.GetValue("Program", "TE_BZPCount"), out value);
                TE_BZPCount = value;

                TE_BZP1 = config.GetValue("Program", "TE_BZP1");
                TE_BZP1 = TE_BZP1 == "" ? "MF_TEST" : TE_BZP1;
                TE_BZP2 = config.GetValue("Program", "TE_BZP2");
                TE_BZP2 = TE_BZP2 == "" ? "QC_TEST" : TE_BZP2;
                TE_BZP3 = config.GetValue("Program", "TE_BZP3");
                TE_BZP3 = TE_BZP3 == "" ? "EN_TEST" : TE_BZP3;
                #endregion

                #region [LogiTestLog]
                LogiLogFlag = config.GetValue("LogiTestLog", "LogiLogFlag") == "1";
                LogiLogUpLoadMysql = config.GetValue("LogiTestLog", "LogiLogUpLoadMysql") != "0";
                LogiBU = config.GetValue("LogiTestLog", "LogiBU");
                LogiProject = config.GetValue("LogiTestLog", "LogiProject");
                LogiStation = config.GetValue("LogiTestLog", "LogiStation");
                LogioemSource = config.GetValue("LogiTestLog", "LogioemSource");
                LogiStage = config.GetValue("LogiTestLog", "LogiStage");
                #endregion

                parameter();

                foreach (System.Reflection.PropertyInfo info in this.GetType().GetProperties())
                    Config[info.Name] = info.GetValue(this);



            }
            catch (Exception ex)
            {

                MessageBox.Show($"Config 读取失败\r\n\r\n{ex.Message}", "TE系统提示");
                Config["BugLog"] = $"{ex}";

            }

        }
        /// <summary>
        /// 从TE数据库拿Config
        /// </summary>
        /// <param name="TypeName"></param>
        /// <param name="Station"></param>
        void GetMysqlConfig(string TypeName, string Station)
        {
            if (NetworkFlag && !EngineerMode)
            {
                string Sql = $"SELECT a1.config FROM station a1, model  a2 WHERE a1.model_id = a2.id  AND a2.`name`= '{TypeName}' AND a1.`name`= '{Station}'";
                string[][] table = null;
                MySqlHelper.GetDataList(Sql, out table);
                if (table.Length > 0)
                {
                    string[] sections = table[0][0].Split('\n');
                    Dictionary<string, string> Dict = new Dictionary<string, string>();
                    string section = "";
                    //将数据库上的Config配置参数读取整合
                    foreach (var item in sections)
                    {
                        if (item.Contains("[") && item.Contains("]"))
                        {
                            int start = item.IndexOf("[") + 1;
                            int End = item.IndexOf("]") - 1;
                            section = item.Substring(start, End);
                            continue;
                        }
                        if (section == "") continue;
                        if (item.Trim().Length <= 0) continue;
                        string[] keyValue = item.Split('=');
                        Dict.Add($"{section}#{keyValue[0]}", keyValue[1]);
                    }
                    //将整合的数据进行写入Config
                    foreach (var item in Dict)
                    {
                        string[] sec = item.Key.Split('#');
                        if (item.Value.Trim().Length <= 0)
                        {
                            config.SetValue(sec[0], sec[1], config.GetValue(sec[0], sec[1]));
                            continue;
                        }
                        config.SetValue(sec[0], sec[1], item.Value);
                    }
                }
            }
            List<string> configData = new List<string>();
            foreach (var item in File.ReadAllLines(config._path))
            {
                if (item.Trim().Length > 1)
                {
                    if (item.Contains("[") && item.Contains("]"))
                    {
                        configData.Add("");
                    }
                    configData.Add(item.Trim());
                }
            }
            File.WriteAllLines(config._path, configData.ToArray());
        }
        /// <summary>
        /// 获取8342以及其他路径参数
        /// </summary>
        void parameter()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                var portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
                var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();
                foreach (string s in portList)
                {
                    if (s.Contains("834X")) _8342Comport = s.Substring(0, s.IndexOf(" "));
                }
            }
            if (SoundCheckPath.Trim().Length < 4)
            {
                foreach (var item in Directory.GetDirectories(@"C:\"))
                    if (item.Contains("SoundCheck"))
                    {
                        foreach (var exePath in Directory.GetFiles(item, "*.exe"))
                        {
                            if (Path.GetFileName(exePath).Contains("SoundCheck") && !Path.GetFileName(exePath).Contains("Demo") && !Path.GetFileName(exePath).Contains("Rene"))
                            {
                                SoundCheckPath = exePath;
                                break;
                            }
                        }
                        break;
                    }
            }
            if (this.DayOfYear != DateTime.Now.DayOfYear)
            {
                TE_BZPCount = 0;
                TestPassCount = 0;
                config.SetValue("Program", "TE_BZPCount", "0");
                this.DayOfYear = DateTime.Now.DayOfYear;

                config.SetValue("Program", "DayOfYear", DateTime.Now.DayOfYear.ToString());
                config.SetValue("Program", "TestPassCount", "0");
            }
            if (Environment.MachineName != ComputerName)
            {
                config.SetValue("Program", "ComputerName", Environment.MachineName);
                config.SetValue("Program", "WireCount", "0");
                config.SetValue("Program", "TestPassCount", "0");
                ComputerName = Environment.MachineName;
                WireCount = 0;
                TestPassCount = 0;
            }
        }
        /// <summary>
        /// 获取加载事件
        /// </summary>
        void GetEvent()
        {
            if (LoadEvent.Count > 0) return;
            LoadEvent.Clear();
            StartTestEvent.Clear();
            TestEndEvent.Clear();
            ClosedEvent.Clear();
            StartTestsEvent.Clear();
            foreach (var item in File.ReadAllLines($@"{Config["adminPath"]}\AllDLL\LoadEvent.txt", Encoding.UTF8))
                if (item.Trim().Length > 0)
                    if (!item.Substring(0, 2).Equals("//"))
                        LoadEvent.Add(item.Split('&'));
            foreach (var item in File.ReadAllLines($@"{Config["adminPath"]}\AllDLL\StartTestEvent.txt", Encoding.UTF8))
                if (item.Trim().Length > 0)
                    if (!item.Substring(0, 2).Equals("//"))
                        StartTestEvent.Add(item.Split('&'));
            foreach (var item in File.ReadAllLines($@"{Config["adminPath"]}\AllDLL\TestEndEvent.txt", Encoding.UTF8))
                if (item.Trim().Length > 0)
                    if (!item.Substring(0, 2).Equals("//"))
                        TestEndEvent.Add(item.Split('&'));
            foreach (var item in File.ReadAllLines($@"{Config["adminPath"]}\AllDLL\ClosedEvent.txt", Encoding.UTF8))
                if (item.Trim().Length > 0)
                    if (!item.Substring(0, 2).Equals("//"))
                        ClosedEvent.Add(item.Split('&'));
            if (File.Exists(".\\AllDLL\\StartTestsEvent.txt"))
                foreach (var item in File.ReadAllLines(".\\AllDLL\\StartTestsEvent.txt", Encoding.UTF8))
                {
                    if (item.Trim().Length > 0)
                        if (!item.Substring(0, 2).Equals("//"))
                            StartTestsEvent.Add(item.Split('&'));
                }
        }

        #endregion

        #region 参数区
        INIHelper config = new INIHelper();
        /// <summary>
        /// SN
        /// </summary>
        public string SN { get; set; } = "";
        public string UID { get; set; } = "";
        public string UID_D { get; set; } = "";

        /// <summary>
        /// 副条码的SN
        /// </summary>
        public string BarcodeSN { get; set; } = "";
        public string BindingSN { get; set; } = "";
        public string BitAddressByBindingSN { get; set; } = "";
        public string BitAddressByBarcodeSN { get; set; } = "";
        public string CustomerSNByBarcodeSN { get; set; } = "";
        public string CustomerSNByBindingSN { get; set; } = "";
        public string BarcodeLeftSN { get; set; } = "";
        public string BarcodeRigthSN { get; set; } = "";
        /// <summary>
        /// 搜索BingdingSN的返回值
        /// </summary>
        public string QueryKeyPartInfo { get; set; } = "";

        //BCCode_SN
        public string SN_BCCode { get; set; } = "";
        //CVC
        public string LincenseKey { get; set; } = "";
        //由搜索条码获取的BD号

        //BD号
        public string BitAddress { get; set; } = "";
        //通过流水码获取客户SN
        public string CustomerSN { get; set; } = "";
        //单项测试指令
        public string Command { get; set; } = "";
        //所有项测试结果
        public bool TestResultFlag { get; set; }
        public ListView TestTable { get; set; } = new ListView();
        public List<string[]> TestItem { get; set; } = new List<string[]>();
        //测试框数据
        public string TestLogging { get; set; }
        //单次测试Logging
        public string OnceTestLogging { get; set; } = "";
        //窗体最大化
        public bool Maximized { get; set; }
        //窗体缩小化
        public bool MinimumSize { get; set; }
        //检测Device标志位
        public bool CheckDevice { get; set; }
        //以下在Config获取的参数
        #region 标志位
        /// <summary>
        /// 测试模式
        /// </summary>
        public bool TestModelFlag { get; internal set; }

        ///// <summary>
        ///// 循环测试
        ///// </summary>
        //public bool LoopFlag { get; internal set; }



        /// <summary>
        /// SoundCheck 
        /// </summary>
        public bool SoundCheckFlag { get; internal set; }
        /// <summary>
        /// 上传测试数据给Mysql
        /// </summary>
        public bool TestLogUploadMySQL { get; set; }
        //上传TestLog到FTP
        public bool UploadFTP { get; set; }
        //上传Soundcheck测试数据到FTP
        public bool SoundcheckDataUploadFTP { get; set; }
        #endregion

        /// <summary>
        /// 测试一轮开始延时
        /// </summary>
        public int StartDelay { get; set; }




        /// <summary>
        /// 线材使用次数
        /// </summary>
        public int WireCount { get; set; }
        /// <summary>
        /// 线材寿命
        /// </summary>
        public int WireCountEnd { get; set; }
        /// <summary>
        /// 线材递增
        /// </summary>
        public int WireIncrease { get; set; }
        /// <summary>
        /// 程序上次打开的电脑名称
        /// </summary>
        public string ComputerName { get; set; }
        /// <summary>
        /// 程序本地记录测试Pass次数
        /// </summary>
        public int TestPassCount { get; set; }


        /// <summary>
        /// 测试一轮结束界面停滞时间
        /// </summary>
        public int StopDelay { get; set; }
        public string RT550_IP { get; set; }
        public string RT550Port { get; set; }
        public string MT8852BGPIB { get; set; }
        /// <summary>
        /// 打印机名
        /// </summary>
        public string Print { get; set; }


        /// <summary>
        /// 打印文件名
        /// </summary>
        public string PrintFileName { get; set; }

        //打印机打印内容
        public string PrintString { get; set; }
        /// <summary>
        /// SoundCheck EXE路径
        /// </summary>
        public string SoundCheckPath { get; set; }
        /// <summary>
        /// SoundCheck Sqc文件路径
        /// </summary>
        public string SqcPath { get; set; }
        /// <summary>
        /// 8342电流表串口
        /// </summary>
        public string _8342Comport { get; internal set; }
        public string MB1205CPort { get; set; }
        /// <summary>
        /// 电源供给器
        /// </summary>
        public string _2303SComport { get; internal set; }
        /// <summary>
        /// Switch开关控制器串口
        /// </summary>
        public string SwitchComport { get; internal set; }
        /// <summary>
        /// Dm5d电流表串口
        /// </summary>
        public string Dm5dPort { get; internal set; }
        /// <summary>
        /// 校准值
        /// </summary>
        public string FreqTrim { get; set; } = "";
        /// <summary>
        /// NXP串口
        /// </summary>
        public string NXP_RF_Comport { get; internal set; }
        /// <summary>
        /// 设备PID
        /// </summary>
        public string Pid { get; internal set; }
        /// <summary>
        /// 设备Vid
        /// </summary>
        public string Vid { get; internal set; }
        /// <summary>
        /// 设备PID
        /// </summary>
        public string _Pid { get; internal set; }
        /// <summary>
        /// 设备Vid
        /// </summary>
        public string _Vid { get; internal set; }
        ///// <summary>
        ///// 循环测试次数
        ///// </summary>
        //public int TestNumbers { get; internal set; }
        /// <summary>
        /// 是否联网
        /// </summary>
        public bool NetworkFlag { get; set; } = true;

        /// <summary>
        /// 工程模式
        /// </summary>
        public bool EngineerMode { get; set; }
        /// <summary>
        /// 是否置顶主窗体
        /// </summary>
        public bool TopMostFlag { get; set; }
        /// <summary>
        /// 以年为单位的天数
        /// </summary>
        public int DayOfYear { get; set; }

        public string TE_BZP1 { get; set; }
        public string TE_BZP2 { get; set; }
        public string TE_BZP3 { get; set; }


        public int TE_BZPCount { get; set; }
        public int TE_BZPLimit { get; set; }



        #region Logi

        public bool LogiLogFlag { get; set; }
        public bool LogiLogUpLoadMysql { get; set; }
        public string LogiBU { get; set; }
        public string LogiProject { get; set; }
        public string LogiStation { get; set; }
        public string LogioemSource { get; set; }
        public string LogiStage { get; set; }


        #endregion

        #region 窗体登录触发事件
        /// <summary>
        /// 加载事件
        /// </summary>
        public List<string[]> LoadEvent = new List<string[]>();
        /// <summary>
        /// 开始测试事件
        /// </summary>
        public List<string[]> StartTestEvent = new List<string[]>();
        /// <summary>
        /// 测试结束事件
        /// </summary>
        public List<string[]> TestEndEvent = new List<string[]>();
        /// <summary>
        /// 窗体关闭事件
        /// </summary>
        public List<string[]> ClosedEvent = new List<string[]>();
        /// <summary>
        /// 所有线程测试结束事件
        /// </summary>
        public List<string[]> StartTestsEvent = new List<string[]>();
        #endregion




        #endregion

        #region MES使用字段
        /// <summary>
        /// 连扳特有的线程参数
        /// </summary>
        public Dictionary<int, Dictionary<string, string>> TestControl = new Dictionary<int, Dictionary<string, string>>();
        /// <summary>
        /// 连扳测试模式
        /// </summary>
        public int TestControlFlag;
        /// <summary>
        /// 错误代码长度
        /// </summary>
        public int ErrorCodeLength;
        /// <summary>
        /// 测试机型
        /// </summary>
        public string Name = "";
        /// <summary>
        /// 测试站别
        /// </summary>
        public string Station = "";
        /// <summary>
        /// 条码长度
        /// </summary>
        public int Barcode { get; set; }
        /// <summary>
        /// 工单长度
        /// </summary>
        public int WorkOrderLength;
        /// <summary>
        /// 测试失败后需要重复测试成功次数
        /// </summary>
        public int PassCount;
        /// <summary>
        ///  测试失败后需要重复测试失败次数
        /// </summary>
        public int FailCount;
        /// <summary>
        /// 工单工号
        /// </summary>
        public string UserID = "";
        /// <summary>
        /// 工单
        /// </summary>
        public string Works = "";
        /// <summary>
        /// 工单信息
        /// </summary>
        public string OrderNumberInformation = "";
        /// <summary>
        /// MES系统名称
        /// </summary>
        public string SystematicName = "";
        /// <summary>
        /// TE数据库带出相关料号的信息
        /// </summary>
        public string PartNumberInfoStr = "";
        /// <summary>
        /// TE数据库带出相关料号的信息
        /// </summary>
        public Dictionary<string, string> PartNumberInfos = new Dictionary<string, string>();
        /// <summary>
        /// MES
        /// </summary>
        public int MesFlag;
        /// <summary>
        /// 是否连接MES测试库
        /// </summary>
        public bool TE_MesFlag = false;

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName = "";
        /// <summary>
        /// 抓取BD号
        /// </summary>
        public bool GetBDFlag;

        #endregion

    }

    class GetInkPath
    {

        [Flags()]
        public enum SLR_FLAGS
        {
            SLR_NO_UI = 0x1,
            SLR_ANY_MATCH = 0x2,
            SLR_UPDATE = 0x4,
            SLR_NOUPDATE = 0x8,
            SLR_NOSEARCH = 0x10,
            SLR_NOTRACK = 0x20,
            SLR_NOLINKINFO = 0x40,
            SLR_INVOKE_MSI = 0x80
        }

        [Flags()]
        public enum SLGP_FLAGS
        {
            SLGP_SHORTPATH = 0x1,
            SLGP_UNCPRIORITY = 0x2,
            SLGP_RAWPATH = 0x4
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        // Unicode version
        public struct WIN32_FIND_DATA
        {
            public int dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
            private const int MAX_PATH = 260;
        }

        [
        ComImport(),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("000214F9-0000-0000-C000-000000000046")
        ]

        // Unicode version
        public interface IShellLink
        {
            void GetPath(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cchMaxPath,
            out WIN32_FIND_DATA pfd,
            SLGP_FLAGS fFlags);

            void GetIDList(
            out IntPtr ppidl);

            void SetIDList(
            IntPtr pidl);

            void GetDescription(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName,
            int cchMaxName);

            void SetDescription(
            [MarshalAs(UnmanagedType.LPWStr)] string pszName);

            void GetWorkingDirectory(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
            int cchMaxPath);

            void SetWorkingDirectory(
            [MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            void GetArguments(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
            int cchMaxPath);

            void SetArguments(
            [MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            void GetHotkey(
            out short pwHotkey);

            void SetHotkey(
            short wHotkey);

            void GetShowCmd(
            out int piShowCmd);

            void SetShowCmd(
            int iShowCmd);

            void GetIconLocation(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
            int cchIconPath,
            out int piIcon);

            void SetIconLocation(
            [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
            int iIcon);

            void SetRelativePath(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
            int dwReserved);

            void Resolve(
            IntPtr hwnd,
            SLR_FLAGS fFlags);

            void SetPath(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport(), Guid("00021401-0000-0000-C000-000000000046")]
        public class ShellLink
        {
        }

        public static string GetPath(string path)
        {
            IShellLink vShellLink = (IShellLink)new ShellLink();
            UCOMIPersistFile vPersistFile = vShellLink as UCOMIPersistFile;
            vPersistFile.Load(path, 0);
            StringBuilder vStringBuilder = new StringBuilder(260);
            WIN32_FIND_DATA vWIN32_FIND_DATA;
            vShellLink.GetPath(vStringBuilder, vStringBuilder.Capacity,
            out vWIN32_FIND_DATA, SLGP_FLAGS.SLGP_RAWPATH);
            return vStringBuilder.ToString();
        }
    }
    class INIHelper
    {
        public string _path;
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVar, int size, string INIpath);
        public string GetValue(string section, string key)
        {

            StringBuilder var = new StringBuilder(512);
            int length = GetPrivateProfileString(section, key, "", var, 512, _path);
            if (length <= 0) SetValue(section, key, "");
            return var.ToString().Trim();
        }

        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        public long SetValue(string section, string Key, string value)
        {
            return WritePrivateProfileString(section, Key, value, _path);
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]     //可以没有此行
        public static extern bool WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);


    }
}
