using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MerryDllFramework
{
    /// <summary>
    /// 存放Config数据
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        internal INIHelper config = new INIHelper(path, out flag);
        const string path = @".\Config\CONFIG.INI";
        static bool flag = false;
        #region 接口方法
        Dictionary<string, object> DicConfig;
        public string Run(object[] Command)
        {
            throw new NotImplementedException();
        }
        public object Interface(Dictionary<string, object> keys) => this.DicConfig = keys;
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Config_ini";
            string dllfunction = "Dll功能说明 ：Config获取内容";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllHistoryVersion1 = "                     ：21.8.2.09";
            string dllHistoryVersion2 = "                     ：21.8.4.20";
            string dllHistoryVersion3 = "                     ：21.10.19.3";
            string dllVersion = "当前Dll版本：21.11.17.5";

            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "21.8.2.09：2021 /8/2：测试数据倒数据库标志位增加";
            string dllChangeInfo2 = "21.8.4.20：2021 /8/13：增加电流表串口搜索功能，Sound Check路径搜索功能，增加UploadFTP标志位";
            string dllChangeInfo3 = "21.10.19.3：2021 /10/28：模板Config从数据库上获取，增加获取料号字段，增加打印机字段";
            string dllChangeInfo4 = "21.11.17.5：2021 /11/17：增加_Pid _Vid字段，修复计数测试成功次数";


            string[] info = { dllname, dllfunction,
                dllHistoryVersion1,dllHistoryVersion2,dllHistoryVersion3,
                dllHistoryVersion, dllVersion, dllChangeInfo,
                dllChangeInfo1,dllChangeInfo2,dllChangeInfo3,dllChangeInfo4
            };

            return info;
        }

        //外部调用接口
        public string GetConfig()
        {
            foreach (var section in INIOperationClass.INIGetAllSectionNames(path))
            {
                foreach (var item in INIOperationClass.INIGetAllItems(path, section))
                {
                    string[] strs = item.Split('=');
                    if (strs.Length < 2) continue;
                    DicConfig[strs[0]] = strs[1];
                }
            }
            try
            {
                Name = config.GetValue("Config", "Name");
                Station = config.GetValue("Config", "Station");
                NetworkFlag = (config.GetValue("Flag", "NetworkFlag")) == "0" ? false : true;
                if (NetworkFlag)
                {
                    try
                    {
                        List<string[]> CongfigList = MySqlHelper.GetDataList($"SELECT a1.config FROM station a1, model  a2 WHERE a1.model_id = a2.id  AND a2.`name`= '{Name}' AND a1.`name`= '{Station}'").Result;
                        if (CongfigList != null)
                        {
                            string[] ss = CongfigList[0][0].Split('\n');
                            Dictionary<string, string> Dict = new Dictionary<string, string>();
                            string section = "";
                            foreach (var item in ss)
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

                    catch
                    {
                    }
                }
                LoadConfig();
                parameter();
                Task.Run(() =>
                {
                    try
                    {
                        string sta = "";
                        foreach (var item in File.ReadAllLines(path))
                        {
                            if (item.Trim().Length < 1) continue;
                            sta += $"{item}\r\n";
                        }
                        File.WriteAllText(path, sta);
                    }
                    catch { }
                });
                List<string> configData = new List<string>();
                foreach (var item in File.ReadAllLines(path))
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
                File.WriteAllLines(path, configData.ToArray());
                foreach (System.Reflection.PropertyInfo info in this.GetType().GetProperties()) DicConfig[info.Name]= info.GetValue(this, null);
                return "Config读取完毕";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return "Config读取异常";
            }
        }


        #endregion
        #region MyRegion

        public void GetTE_BZPinfo()
        {
            if (this.Day != DateTime.Now.Day)
            {
                config.SetValue("TE_BZP", "TE_BZPCount", "0");
                config.SetValue("Config", "Day", $"{DateTime.Now.Day}");
                this.Day = DateTime.Now.Day;
                TE_BZPCount = 0;
            }
        }
        public bool GetData()
        {
            var path = @"./Config/Count.txt";
            if (!File.Exists(path)) return false;
            var itemdata = Reader(path);
            if (itemdata == null || itemdata.Count == 0) return false;
            测试成功 = Convert.ToInt32(itemdata[0].Split('=')[1]);
            测试失败 = Convert.ToInt32(itemdata[1].Split('=')[1]);
            数据机型 = itemdata[2].Split('=')[1];
            当前时间 = itemdata[3].Split('=')[1];


            if (数据机型 != Station || 当前时间 != DateTime.Now.DayOfYear.ToString())
            {
                测试成功 = 0;
                测试失败 = 0;
                数据机型 = Station;
                当前时间 = DateTime.Now.DayOfYear.ToString();
                string[] str = { $"测试成功={ 测试成功}\r测试失败={ 测试失败}\r数据机型={ Station}\r当前时间={DateTime.Now.DayOfYear.ToString()}" };
                Writer(str, path);
            }
            DicConfig["测试成功"] = 测试成功;
            DicConfig["测试失败"] = 测试失败;
            DicConfig["数据机型"] = 数据机型;
            DicConfig["当前时间"] = 当前时间;


            return true;
        }
        public bool SetData()
        {
            try
            {
                string path = @"./Config/Count.txt";
                string[] str = { $"测试成功={ DicConfig["测试成功"]}\r测试失败={ DicConfig["测试失败"]}\r数据机型={ DicConfig["Station"]}\r当前时间={DateTime.Now.DayOfYear.ToString()}" };
                Writer(str, path);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
        private void Writer(string[] str, string path, bool append = false)
        {
            using (StreamWriter writer = new StreamWriter(path, append, Encoding.UTF8))
            {
                foreach (var item in str)
                {
                    writer.WriteLine(item);
                }
            }

        }
        private List<string> Reader(string path)
        {
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                List<string> readstr = new List<string>();
                while (!reader.EndOfStream)
                {
                    var st = reader.ReadLine();
                    if (st == null || st == "") break;
                    readstr.Add(st);
                }
                return readstr;
            }
        }
        //加载方法
        private void LoadConfig()
        {
            if (!flag) return;
            #region [Comport] 
            _8342Comport = config.GetValue("Comport", "_8342Comport");
            _2303SComport = config.GetValue("Comport", "_2303SComport");
            SwitchComport = config.GetValue("Comport", "SwitchComport");
            StartRunComport = config.GetValue("Comport", "StartRunComport");
            Dm5dPort = config.GetValue("Comport", "Dm5dPort");
            NXP_RF_Comport = config.GetValue("Comport", "NXP_RF_Comport");

            #endregion
            #region [Delay]
            StopDelay = Convert.ToInt32(config.GetValue("Delay", "StopDelay"));
            StartDelay = Convert.ToInt32(config.GetValue("Delay", "StartDelay"));
            #endregion
            #region [Config]


            Title = config.GetValue("Config", "Title");
            Barcode = int.Parse(config.GetValue("Config", "Barcode"));
            WireCount = int.Parse(config.GetValue("Config", "WireCount"));
            WireCountEnd = int.Parse(config.GetValue("Config", "WireCountEnd"));
            int.TryParse(config.GetValue("Config", "Day"), out int i);
            Day = i;
            int.TryParse(config.GetValue("Config", "WireIncrease"), out int result);
            if (result <= 0) result = 1;
            WireIncrease = result;
            #endregion
            #region [Flag]
            MesFlag = int.Parse(config.GetValue("Flag", "MesFlag"));
            GetBDFlag = (config.GetValue("Flag", "GetBDFlag")) == "1" ? true : false;
            TopMostFlag = (config.GetValue("Flag", "TopMostFlag")) == "1" ? true : false;
            Maximized = (config.GetValue("Flag", "Maximized")) == "1" ? true : false;
            MinimumSize = (config.GetValue("Flag", "MinimumSize")) == "1" ? true : false;
            TestLogUploadMySQL = config.GetValue("Flag", "TestLogUploadMySQL") != "0" ? true : false;

            LogiTestLog = config.GetValue("Flag", "LogiTestLog") != "0" ? true : false;
            UploadFTP = (config.GetValue("Flag", "UploadFTP")) == "1" ? true : false;
            SoundcheckDataUploadFTP = config.GetValue("Flag", "SoundcheckDataUploadFTP") == "1" ? true : false;

            #endregion
            #region [TestModel]
            TestModelFlag = (config.GetValue("TestModel", "TestModelFlag")) == "1" ? true : false;
            CheckDevice = (config.GetValue("TestModel", "CheckDevice")) == "1" ? true : false;
            Pid = config.GetValue("TestModel", "Pid");
            Vid = config.GetValue("TestModel", "Vid");
            _Pid = config.GetValue("TestModel", "_Pid");
            _Vid = config.GetValue("TestModel", "_Vid");
            #endregion
            #region [Loop]
            LoopFlag = (config.GetValue("Loop", "LoopFlag")) == "1" ? true : false;
            if (MesFlag > 0) LoopFlag = false;
            TestNumbers = Convert.ToInt32(config.GetValue("Loop", "TestNumbers"));
            #endregion
            #region [Mes]
            ErrorCode = config.GetValue("Mes", "ErrorCode");
            PassCount = int.Parse(config.GetValue("Mes", "PassCount"));
            FailCount = int.Parse(config.GetValue("Mes", "FailCount"));
            WorkOrderLength = int.Parse(config.GetValue("Mes", "WorkOrderLength"));
            CustomerName = config.GetValue("Mes", "CustomerName");
            Build = config.GetValue("Mes", "Build");


            BDstation = config.GetValue("Mes", "BDstation");
            SystematicName = config.GetValue("Mes", "SystematicName");
            int.TryParse(config.GetValue("Mes", "ErrorCodeLength"), out int ss);
            ErrorCodeLength = ss == 0 ? 7 : ss;
            #endregion
            #region [SoundCheck]
            SoundCheckFlag = (config.GetValue("SoundCheck", "SoundCheckFlag")) == "1" ? true : false;
            Soundchckpath = config.GetValue("SoundCheck", "Soundchckpath");
            Sqcpath = config.GetValue("SoundCheck", "Sqcpath");
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
            #region  [TE_BZP]
            int.TryParse(config.GetValue("TE_BZP", "TE_BZPCount"), out i);
            TE_BZPCount = i;
            if (int.TryParse(config.GetValue("TE_BZP", "TE_BZPLimit"), out i))
            {
                TE_BZPLimit = i;
            }
            else
            {
                TE_BZPLimit = 50;
            }
            #endregion
            GetTE_BZPinfo();

        }
        void parameter()
        {
            string[] serialport = GetHarewareInfo(HardwareEnum.Win32_SerialPort, "Name");
            foreach (var item in serialport)
            {
                if (item.Contains("834X"))
                {

                    int start = item.IndexOf("(");
                    int end = item.IndexOf(")");
                    int index = (end - start);
                    _8342Comport = item.Substring(start + 1, index - 1);
                }
                if (_2303SComport.Length < 3)
                {
                    if (item.Contains("USB Serial Port"))
                    {
                        int start = item.IndexOf("(");
                        int end = item.IndexOf(")");
                        int index = (end - start);
                        _2303SComport = item.Substring(start + 1, index);
                    }

                }
            }
            if (Soundchckpath.Trim().Length < 4)
            {
                foreach (var item in Directory.GetDirectories(@"C:\"))
                {
                    if (item.Contains("SoundCheck"))
                    {

                        foreach (var exePath in Directory.GetFiles(item, "*.exe"))
                        {
                            if (Path.GetFileName(exePath).Contains("SoundCheck") && !Path.GetFileName(exePath).Contains("Demo") && !Path.GetFileName(exePath).Contains("Rene"))
                            {
                                Soundchckpath = exePath;
                                break;
                            }
                        }
                        break;
                    }

                }
            }
            if (Sqcpath.Trim().Length < 4)
            {
                Sqcpath = sqcPath(".\\SoundCheck_sqc");
            }
        }
        string sqcPath(string path)
        {
            string sqcpath = "";
            if (!Directory.Exists(path)) return "";
            foreach (var item in Directory.GetFileSystemEntries(path))
            {
                if (!Directory.Exists(item))
                {
                    if (Path.GetExtension(item).Contains("sqc"))
                    {
                        sqcpath = Path.GetFullPath(item);
                        return sqcpath;
                    }
                }
                else
                {
                    sqcpath = sqcPath(item);
                    if (sqcpath != "") return sqcpath;

                }


            }
            return sqcpath;
        }

        /// <summary>
        /// Get the system devices information with windows api.
        /// </summary>
        /// <param name="hardType">Device type.</param>
        /// <param name="propKey">the property of the device.</param>
        /// <returns></returns>
        string[] GetHarewareInfo(HardwareEnum hardType, string propKey)
        {

            List<string> strs = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        if (hardInfo.Properties[propKey].Value != null)
                        {
                            String str = hardInfo.Properties[propKey].Value.ToString();
                            strs.Add(str);
                        }

                    }
                }
                return strs.ToArray();
            }
            catch
            {
                return null;
            }
            finally
            {
                strs = null;
            }
        }
        #endregion

        #region 参数区

        //不在config的公共参数
        public string SN { get; set; } = "";
        public int TE_BZPCount { get; set; }
        public int TE_BZPLimit { get; set; }

        public int ErrorCodeLength { get; set; } = 7;
        public int Day { get; set; }
        //barCode的SN
        public string BindingSN { get; set; } = "";

        //搜索BingdingSN的返回值
        public string QueryKeyPartInfo { get; set; } = "";

        //BCCode_SN
        public string SN_BCCode { get; set; } = "";
        //CVC
        public string LineseKey { get; set; } = "";
        //由搜索条码获取的BD号
        public string BitAddressByBindingSN { get; set; } = "";
        //BD号
        public string BitAddress { get; set; } = "";
        //通过流水码获取客户SN
        public string CustomerSN { get; set; } = "";
        //单项测试指令
        public string Command { get; set; } = "";
        //单项测试值
        public string TestValue { get; set; } = "";
        //测试结果的数据
        public string Result { get; set; } = "";
        //工单工号
        public string UserID { get; set; } = "";
        //工单
        public string Works { get; set; } = "";
        //工单信息
        public string OrderNumberInformation { get; set; } = "";

        public string PairID { get; set; }
        //MES系统名称
        public string SystematicName { get; set; } = "";
        //所有项测试结果
        public bool TestResultFlag { get; set; }
        public ListView TestTable { get; set; } = new ListView();
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
        /// <summary>
        /// 循环测试
        /// </summary>
        public bool LoopFlag { get; internal set; }
        /// <summary>
        /// MES
        /// </summary>
        public int MesFlag { get; internal set; }
        /// <summary>
        /// 抓取BD号
        /// </summary>
        public bool GetBDFlag { get; internal set; }
        /// <summary>
        /// SoundCheck 
        /// </summary>
        public bool SoundCheckFlag { get; internal set; }
        /// <summary>
        /// 上传测试数据给Mysql
        /// </summary>
        public bool TestLogUploadMySQL { get; set; }
        public bool LogiTestLog { get; set; }
        //上传TestLog到FTP
        public bool UploadFTP { get; set; }
        //上传Soundcheck测试数据到FTP
        public bool SoundcheckDataUploadFTP { get; set; }
        #endregion

        public string BDstation { get; set; } = "T2.3";
        /// <summary>
        /// 测试一轮开始延时
        /// </summary>
        public int StartDelay { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }
        public string Build { get; set; }
        /// <summary>
        /// 测试机型
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 测试站别
        /// </summary>
        public string Station { get; set; }
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
        /// 测试程序标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 条码长度
        /// </summary>
        public int Barcode { get; set; }
        /// <summary>
        /// 工单长度
        /// </summary>
        public int WorkOrderLength { get; set; }
        /// <summary>
        /// 测试失败后需要重复测试成功次数
        /// </summary>
        public int PassCount { get; set; }
        /// <summary>
        ///  测试失败后需要重复测试失败次数
        /// </summary>
        public int FailCount { get; set; }
        /// <summary>
        /// 测试一轮结束界面停滞时间
        /// </summary>
        public int StopDelay { get; set; }
        /// <summary>
        /// 打印机名
        /// </summary>
        public string Print { get; set; }
        public string RT550_IP { get; set; }
        public string RT550Port { get; set; }
        public string MT8852BGPIB { get; set; }
        public string FreqTrim { get; set; } = "null";


        /// <summary>
        /// 打印文件名
        /// </summary>
        public string PrintFileName { get; set; }


        /// <summary>
        /// 打印机打印内容
        /// </summary>
        public string PrintString { get; set; }
        public string PrintSection1 { get; set; }
        public string PrintSection2 { get; set; }
        public string PrintValus1 { get; set; }
        public string PrintValus2 { get; set; }






        /// <summary>
        /// SoundCheck EXE路径
        /// </summary>
        public string Soundchckpath { get; set; }
        /// <summary>
        /// SoundCheck Sqc文件路径
        /// </summary>
        public string Sqcpath { get; set; }
        /// <summary>
        /// 8342电流表串口
        /// </summary>
        public string _8342Comport { get; internal set; }
        public string StartRunComport { get; internal set; }
        /// <summary>
        /// 电源供给器
        /// </summary>
        public string _2303SComport { get; internal set; }
        /// <summary>
        /// Switch开关控制器串口
        /// </summary>
        public string SwitchComport { get; internal set; }
        /// <summary>
        /// 8342电流表串口
        /// </summary>
        public string Dm5dPort { get; internal set; }

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
        /// <summary>
        /// 循环测试次数
        /// </summary>
        public int TestNumbers { get; internal set; }
        /// <summary>
        /// MESErrorCode
        /// </summary>
        public string ErrorCode { get; internal set; }


        /// <summary>
        /// 是否联网
        /// </summary>
        public bool NetworkFlag { get; set; } = true;
        /// <summary>
        /// 是否置顶主窗体
        /// </summary>
        public bool TopMostFlag { get; set; }


        public int 测试成功 { get; set; }
        public int 测试失败 { get; set; }
        public string 数据机型 { get; set; }
        public string 当前时间 { get; set; }
        #endregion

        enum HardwareEnum
        {
            // 硬件
            Win32_Processor, // CPU 处理器
            Win32_PhysicalMemory, // 物理内存条
            Win32_Keyboard, // 键盘
            Win32_PointingDevice, // 点输入设备，包括鼠标。
            Win32_FloppyDrive, // 软盘驱动器
            Win32_DiskDrive, // 硬盘驱动器
            Win32_CDROMDrive, // 光盘驱动器
            Win32_BaseBoard, // 主板
            Win32_BIOS, // BIOS 芯片
            Win32_ParallelPort, // 并口
            Win32_SerialPort, // 串口
            Win32_SerialPortConfiguration, // 串口配置
            Win32_SoundDevice, // 多媒体设置，一般指声卡。
            Win32_SystemSlot, // 主板插槽 (ISA & PCI & AGP)
            Win32_USBController, // USB 控制器
            Win32_NetworkAdapter, // 网络适配器
            Win32_NetworkAdapterConfiguration, // 网络适配器设置
            Win32_Printer, // 打印机
            Win32_PrinterConfiguration, // 打印机设置
            Win32_PrintJob, // 打印机任务
            Win32_TCPIPPrinterPort, // 打印机端口
            Win32_POTSModem, // MODEM
            Win32_POTSModemToSerialPort, // MODEM 端口
            Win32_DesktopMonitor, // 显示器
            Win32_DisplayConfiguration, // 显卡
            Win32_DisplayControllerConfiguration, // 显卡设置
            Win32_VideoController, // 显卡细节。
            Win32_VideoSettings, // 显卡支持的显示模式。

            // 操作系统
            Win32_TimeZone, // 时区
            Win32_SystemDriver, // 驱动程序
            Win32_DiskPartition, // 磁盘分区
            Win32_LogicalDisk, // 逻辑磁盘
            Win32_LogicalDiskToPartition, // 逻辑磁盘所在分区及始末位置。
            Win32_LogicalMemoryConfiguration, // 逻辑内存配置
            Win32_PageFile, // 系统页文件信息
            Win32_PageFileSetting, // 页文件设置
            Win32_BootConfiguration, // 系统启动配置
            Win32_ComputerSystem, // 计算机信息简要
            Win32_OperatingSystem, // 操作系统信息
            Win32_StartupCommand, // 系统自动启动程序
            Win32_Service, // 系统安装的服务
            Win32_Group, // 系统管理组
            Win32_GroupUser, // 系统组帐号
            Win32_UserAccount, // 用户帐号
            Win32_Process, // 系统进程
            Win32_Thread, // 系统线程
            Win32_Share, // 共享
            Win32_NetworkClient, // 已安装的网络客户端
            Win32_NetworkProtocol, // 已安装的网络协议
            Win32_PnPEntity,//all device
        }
    }
    public class INIOperationClass
    {

        #region INI文件操作

        /*
         * 针对INI文件的API操作方法，其中的节点（Section)、键（KEY）都不区分大小写
         * 如果指定的INI文件不存在，会自动创建该文件。
         * 
         * CharSet定义的时候使用了什么类型，在使用相关方法时必须要使用相应的类型
         *      例如 GetPrivateProfileSectionNames声明为CharSet.Auto,那么就应该使用 Marshal.PtrToStringAuto来读取相关内容
         *      如果使用的是CharSet.Ansi，就应该使用Marshal.PtrToStringAnsi来读取内容
         *      
         */

        #region API声明

        /// <summary>
        /// 获取所有节点名称(Section)
        /// </summary>
        /// <param name="lpszReturnBuffer">存放节点名称的内存地址,每个节点之间用\0分隔</param>
        /// <param name="nSize">内存大小(characters)</param>
        /// <param name="lpFileName">Ini文件</param>
        /// <returns>内容的实际长度,为0表示没有内容,为nSize-2表示内存大小不够</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, uint nSize, string lpFileName);

        /// <summary>
        /// 获取某个指定节点(Section)中所有KEY和Value
        /// </summary>
        /// <param name="lpAppName">节点名称</param>
        /// <param name="lpReturnedString">返回值的内存地址,每个之间用\0分隔</param>
        /// <param name="nSize">内存大小(characters)</param>
        /// <param name="lpFileName">Ini文件</param>
        /// <returns>内容的实际长度,为0表示没有内容,为nSize-2表示内存大小不够</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

        /// <summary>
        /// 读取INI文件中指定的Key的值
        /// </summary>
        /// <param name="lpAppName">节点名称。如果为null,则读取INI中所有节点名称,每个节点名称之间用\0分隔</param>
        /// <param name="lpKeyName">Key名称。如果为null,则读取INI中指定节点中的所有KEY,每个KEY之间用\0分隔</param>
        /// <param name="lpDefault">读取失败时的默认值</param>
        /// <param name="lpReturnedString">读取的内容缓冲区，读取之后，多余的地方使用\0填充</param>
        /// <param name="nSize">内容缓冲区的长度</param>
        /// <param name="lpFileName">INI文件名</param>
        /// <returns>实际读取到的长度</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, [In, Out] char[] lpReturnedString, uint nSize, string lpFileName);

        //另一种声明方式,使用 StringBuilder 作为缓冲区类型的缺点是不能接受\0字符，会将\0及其后的字符截断,
        //所以对于lpAppName或lpKeyName为null的情况就不适用
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        //再一种声明，使用string作为缓冲区的类型同char[]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnedString, uint nSize, string lpFileName);

        /// <summary>
        /// 将指定的键值对写到指定的节点，如果已经存在则替换。
        /// </summary>
        /// <param name="lpAppName">节点，如果不存在此节点，则创建此节点</param>
        /// <param name="lpString">Item键值对，多个用\0分隔,形如key1=value1\0key2=value2
        /// <para>如果为string.Empty，则删除指定节点下的所有内容，保留节点</para>
        /// <para>如果为null，则删除指定节点下的所有内容，并且删除该节点</para>
        /// </param>
        /// <param name="lpFileName">INI文件</param>
        /// <returns>是否成功写入</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]     //可以没有此行
        private static extern bool WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);

        /// <summary>
        /// 将指定的键和值写到指定的节点，如果已经存在则替换
        /// </summary>
        /// <param name="lpAppName">节点名称</param>
        /// <param name="lpKeyName">键名称。如果为null，则删除指定的节点及其所有的项目</param>
        /// <param name="lpString">值内容。如果为null，则删除指定节点中指定的键。</param>
        /// <param name="lpFileName">INI文件</param>
        /// <returns>操作是否成功</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        #endregion

        #region 封装

        /// <summary>
        /// 读取INI文件中指定INI文件中的所有节点名称(Section)
        /// </summary>
        /// <param name="iniFile">Ini文件</param>
        /// <returns>所有节点,没有内容返回string[0]</returns>
        public static string[] INIGetAllSectionNames(string iniFile)
        {
            uint MAX_BUFFER = 32767;    //默认为32767

            string[] sections = new string[0];      //返回值

            //申请内存
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));
            uint bytesReturned = INIOperationClass.GetPrivateProfileSectionNames(pReturnedString, MAX_BUFFER, iniFile);
            if (bytesReturned != 0)
            {
                //读取指定内存的内容
                string local = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned).ToString();

                //每个节点之间用\0分隔,末尾有一个\0
                sections = local.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }

            //释放内存
            Marshal.FreeCoTaskMem(pReturnedString);

            return sections;
        }

        /// <summary>
        /// 获取INI文件中指定节点(Section)中的所有条目(key=value形式)
        /// </summary>
        /// <param name="iniFile">Ini文件</param>
        /// <param name="section">节点名称</param>
        /// <returns>指定节点中的所有项目,没有内容返回string[0]</returns>
        public static string[] INIGetAllItems(string iniFile, string section)
        {
            //返回值形式为 key=value,例如 Color=Red
            uint MAX_BUFFER = 32767;    //默认为32767

            string[] items = new string[0];      //返回值

            //分配内存
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));

            uint bytesReturned = INIOperationClass.GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, iniFile);

            if (!(bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
            {

                string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);
                items = returnedString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }

            Marshal.FreeCoTaskMem(pReturnedString);     //释放内存

            return items;
        }

        /// <summary>
        /// 获取INI文件中指定节点(Section)中的所有条目的Key列表
        /// </summary>
        /// <param name="iniFile">Ini文件</param>
        /// <param name="section">节点名称</param>
        /// <returns>如果没有内容,反回string[0]</returns>
        public static string[] INIGetAllItemKeys(string iniFile, string section)
        {
            string[] value = new string[0];
            const int SIZE = 1024 * 10;

            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            char[] chars = new char[SIZE];
            uint bytesReturned = INIOperationClass.GetPrivateProfileString(section, null, null, chars, SIZE, iniFile);

            if (bytesReturned != 0)
            {
                value = new string(chars).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }
            chars = null;

            return value;
        }

        /// <summary>
        /// 读取INI文件中指定KEY的字符串型值
        /// </summary>
        /// <param name="iniFile">Ini文件</param>
        /// <param name="section">节点名称</param>
        /// <param name="key">键名称</param>
        /// <param name="defaultValue">如果没此KEY所使用的默认值</param>
        /// <returns>读取到的值</returns>
        public static string INIGetStringValue(string iniFile, string section, string key, string defaultValue)
        {
            string value = defaultValue;
            const int SIZE = 1024 * 10;

            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("必须指定键名称(key)", "key");
            }

            StringBuilder sb = new StringBuilder(SIZE);
            uint bytesReturned = INIOperationClass.GetPrivateProfileString(section, key, defaultValue, sb, SIZE, iniFile);

            if (bytesReturned != 0)
            {
                value = sb.ToString();
            }
            sb = null;

            return value;
        }

        /// <summary>
        /// 在INI文件中，将指定的键值对写到指定的节点，如果已经存在则替换
        /// </summary>
        /// <param name="iniFile">INI文件</param>
        /// <param name="section">节点，如果不存在此节点，则创建此节点</param>
        /// <param name="items">键值对，多个用\0分隔,形如key1=value1\0key2=value2</param>
        /// <returns></returns>
        public static bool INIWriteItems(string iniFile, string section, string items)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            if (string.IsNullOrEmpty(items))
            {
                throw new ArgumentException("必须指定键值对", "items");
            }

            return INIOperationClass.WritePrivateProfileSection(section, items, iniFile);
        }

        /// <summary>
        /// 在INI文件中，指定节点写入指定的键及值。如果已经存在，则替换。如果没有则创建。
        /// </summary>
        /// <param name="iniFile">INI文件</param>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>操作是否成功</returns>
        public static bool INIWriteValue(string iniFile, string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("必须指定键名称", "key");
            }

            if (value == null)
            {
                throw new ArgumentException("值不能为null", "value");
            }

            return INIOperationClass.WritePrivateProfileString(section, key, value, iniFile);

        }

        /// <summary>
        /// 在INI文件中，删除指定节点中的指定的键。
        /// </summary>
        /// <param name="iniFile">INI文件</param>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <returns>操作是否成功</returns>
        public static bool INIDeleteKey(string iniFile, string section, string key)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("必须指定键名称", "key");
            }

            return INIOperationClass.WritePrivateProfileString(section, key, null, iniFile);
        }

        /// <summary>
        /// 在INI文件中，删除指定的节点。
        /// </summary>
        /// <param name="iniFile">INI文件</param>
        /// <param name="section">节点</param>
        /// <returns>操作是否成功</returns>
        public static bool INIDeleteSection(string iniFile, string section)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            return INIOperationClass.WritePrivateProfileString(section, null, null, iniFile);
        }

        /// <summary>
        /// 在INI文件中，删除指定节点中的所有内容。
        /// </summary>
        /// <param name="iniFile">INI文件</param>
        /// <param name="section">节点</param>
        /// <returns>操作是否成功</returns>
        public static bool INIEmptySection(string iniFile, string section)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            return INIOperationClass.WritePrivateProfileSection(section, string.Empty, iniFile);
        }


        private void TestIniINIOperation()
        {

            string file = "F:\\TestIni.ini";

            //写入/更新键值
            INIWriteValue(file, "Desktop", "Color", "Red");
            INIWriteValue(file, "Desktop", "Width", "3270");

            INIWriteValue(file, "Toolbar", "Items", "Save,Delete,Open");
            INIWriteValue(file, "Toolbar", "Dock", "True");

            //写入一批键值
            INIWriteItems(file, "Menu", "File=文件\0View=视图\0Edit=编辑");

            //获取文件中所有的节点
            string[] sections = INIGetAllSectionNames(file);

            //获取指定节点中的所有项
            string[] items = INIGetAllItems(file, "Menu");

            //获取指定节点中所有的键
            string[] keys = INIGetAllItemKeys(file, "Menu");

            //获取指定KEY的值
            string value = INIGetStringValue(file, "Desktop", "color", null);

            //删除指定的KEY
            INIDeleteKey(file, "desktop", "color");

            //删除指定的节点
            INIDeleteSection(file, "desktop");

            //清空指定的节点
            INIEmptySection(file, "toolbar");

        }
        #endregion

        #endregion
    }
}
public class INIHelper
{
    string _path;
    public INIHelper(string path, out bool flag)
    {
        if (!File.Exists(path)) flag = false;
        _path = path;
        flag = true;
    }
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

