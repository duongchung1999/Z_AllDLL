using FluentFTP;
using JSON_DATA;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Config_ini.Class.HttpPostAPI;

namespace Config_ini.Class
{
    /// <summary>
    /// 该类用于处理测试数据，储存测试数据
    /// </summary>
    class TestData
    {
        #region 构造函数
        public TestData(Dictionary<string, object> Config) =>
            this.Config = Config;
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        public void OnceConfigInterface(Dictionary<string, object> OnceConfig) => this.OnceConfig = OnceConfig;
        public TestData()
        {
        }
        string SoundCheckLogPath = ".\\TestData\\SoundcheckDataUploadFTP";
        public Dictionary<string, object> Config;
        #endregion


        public void ClearSoundCheckLogging(bool flag)
        {
            if (flag)
            {
                if (Directory.Exists(SoundCheckLogPath)) Directory.Delete(SoundCheckLogPath, true);
                Directory.CreateDirectory(SoundCheckLogPath);

            }
        }


        #region 获取测试项目
        /// <summary>
        /// 获取测试项目
        /// </summary>
        public void ReadTestItem(bool NetworkFlag)
        {
            //机型
            string TypeName = (string)Config["Name"];
            //站别
            string Station = (string)Config["Station"];
            //是否连接数据库
            //本地txt
            string Path = $@".\TestItem\{TypeName}\{Station}.txt";
            List<string[]> TestItem = new List<string[]>();
            if (NetworkFlag)
            {
                Config["LoadMessagebox"] = $"连接TE数据库获取测试计划";

                string[][] testitems = null;

                if (GetTestItems(TypeName, Station, out testitems))
                {

                    string[] str = new string[testitems.Length];
                    for (int i = 0; i < testitems.Length; i++)
                    {
                        string col = "";
                        foreach (var items in testitems[i])
                        {
                            col += "," + items;
                        }

                        str[i] = col.Remove(0, 1);
                    }
                    //将内容的全部TXT写入
                    if (!Directory.Exists($@".\TestItem\{TypeName}")) Directory.CreateDirectory($@".\TestItem\{TypeName}");
                    File.WriteAllLines($@".\TestItem\{TypeName}\{Station}.txt", str, Encoding.UTF8);
                    TestItem.AddRange(testitems);
                    Config["TestItem"] = TestItem;
                    return;
                }

            }
            Config["LoadMessagebox"] = $"获取本地测试计划";

            foreach (var item in File.ReadAllLines(Path))
                TestItem.Add(item.Split(','));
            Config["TestItem"] = TestItem;
            return;
        }

        /// <summary>
        /// 查询测试项目
        /// </summary>
        bool GetTestItems(string TypeName, string Station, out string[][] TestItem)
        {
            var sql = $@"SELECT testitem.`name`, testitem.unit,testitem.lower_value,testitem.upper_value,testitem.`no` , testitem.cmd
                from model 
                inner join station on model.id = station.model_id and model.`name` = '{TypeName}' 
                inner join station_testitem on station.`name` =  '{Station}' and station.id = station_testitem.station_id 
                inner join testitem on station_testitem.testitem_id = testitem.id ORDER BY station_testitem.`sort_index`;";
            string[][] tableStr = TestItem = null;
            //MessageBox.Show("进入查询方法");
            if (!MySqlHelper.GetDataList(sql, out tableStr)) return false;
            int delaynums = 1;
            List<string[]> TestItems = new List<string[]>();
            foreach (var item in tableStr)
            {
                string[] str = new string[7];
                str[1] = item[0].Trim(); //测试项目  
                str[2] = item[1].Trim(); //单位
                str[3] = item[2].Trim(); //数值下限  
                str[4] = item[3].Trim(); //数值上限
                str[5] = item[4].Trim(); //耳机指令 
                str[6] = item[5].Trim(); //方法
                if (str[1].Contains("-"))
                {
                    str[0] = "0";
                }
                else
                {
                    str[0] = delaynums.ToString();
                    delaynums++;
                }
                TestItems.Add(str);
            }
            TestItem = TestItems.ToArray();
            return true;
        }
        #endregion

        #region 保存测试数据
        string ComputerName = Environment.MachineName;
        FtpClient ftp = null;

        string ipv4 = null;
        static object lock_obj = new object();

        /*生成本地Log*/
        public void SaveTestData(string url)
        {
            lock (lock_obj)
            {
                onlySaveTestLog(url);
            }
            return;
        }
        void onlySaveTestLog(string url)
        {
            bool moreTest = false;
            bool TestResultFlag = false;
            try
            {
                moreTest = OnceConfig.ContainsKey("SN");
                string SN = moreTest ? (string)OnceConfig["SN"] : (string)Config["SN"];
                TimeSpan span = moreTest ? (TimeSpan)OnceConfig["TestTime"] : (TimeSpan)Config["TestTime"];
                string TestTime = span.TotalSeconds.ToString("0.00");
                TestResultFlag = moreTest ? (bool)OnceConfig["TestResultFlag"] : (bool)Config["TestResultFlag"];
                string TestID = moreTest ? OnceConfig["TestID"].ToString() : "";
                string OnceTestLogging = moreTest ? (string)OnceConfig["OnceTestLogging"] : (string)Config["OnceTestLogging"];
                string Station = (string)Config["Station"];
                string TypeName = (string)Config["Name"];
                string WorkOrder = SN.Contains("TE_BZP") ? "000000000000" : (string)Config["Works"];
                string SystematicName = (string)Config["SystematicName"];
                int MesFlag = (int)Config["MesFlag"];
                bool TestLogUploadMySQL = (bool)Config["TestLogUploadMySQL"];

                bool UploadFTP = (bool)Config["UploadFTP"];
                bool SC_FTP = (bool)Config["SoundcheckDataUploadFTP"];
                string DirPath = $@".\TestData";
                if (!Directory.Exists(DirPath))
                    Directory.CreateDirectory(DirPath);
                DataTable TestTable = moreTest ? (DataTable)OnceConfig["TestTable"] : listViewToTable((ListView)Config["TestTable"]);

                #region Logi生成测试数据
                bool LogiLogFlag = (bool)Config["LogiLogFlag"];
                if (LogiLogFlag && !SN.Contains("TE_BZP"))
                {
                    string LogiBU = (string)Config["LogiBU"];
                    string LogiProject = (string)Config["LogiProject"];
                    string LogiStage = (string)Config["LogiStage"];
                    string LogioemSource = (string)Config["LogioemSource"];
                    string LogiStation = (string)Config["LogiStation"];
                    if (ipv4 == null)
                        foreach (var ipa in Dns.GetHostAddresses(ComputerName)) if (ipa.AddressFamily == AddressFamily.InterNetwork)
                            {
                                ipv4 = ipa.ToString();
                                break;
                            }
                    bool logicsvResult = LogiTestLogCSV(TestResultFlag, TypeName, Station, LogiBU, LogiProject,
                                                               LogiStation, LogiStage, LogioemSource, TestTime, ipv4, TestTable, MesFlag, SystematicName, WorkOrder);
                    if (!logicsvResult)
                    {
                        Config["TestResultFlag"] = TestResultFlag = false;
                        if (moreTest)
                            OnceConfig["TestResultFlag"] = TestResultFlag;
                    }



                }
                #endregion

                #region 上传数据库
                if (TestLogUploadMySQL)
                {

                    bool MysqlFlag = false;
                    string PostResult = "";
                    string error = "";
                    string uploadData = "{" + $"\"modelValue\":\"{TypeName}\",\"SN\":\"{SN}\",\"Result\":\"{TestResultFlag}\",\"Station\":\"{Station}\",\"workorders\":\"{WorkOrder}\",\"MachineName\":\"{ComputerName}\",\"TestTime\":\"{TestTime}\",\"TestLog\":\"{TestLog(TestTable)}\"" + "}";
                    for (int i = 0; i < 6; i++)
                    {

                        PostResult = HttpPost(url,
                                              uploadData,
                                               "Post", out error);
                        if (PostResult == "200")
                        {
                            MysqlFlag = true;
                            break;
                        }
                    }
                    if (!MysqlFlag)
                    {
                        Config["TestResultFlag"] = TestResultFlag = false;
                        if (moreTest)
                            OnceConfig["TestResultFlag"] = TestResultFlag;
                        MessageBox.Show($"上传数据到TE库API异常\r\n{uploadData}\r\n异常信息{PostResult}\\错误信息:\r\n{error}\r\n", "TE系统提示", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        File.AppendAllText($@".\Log\上传数据到TE库API异常.txt", $"{DateTime.Now}\r\n{uploadData}\r\n异常信息{PostResult}\r\n错误内容{error}\r\n");
                    }

                }

                #endregion


                #region 上传至FTP
                if (UploadFTP)
                {
                    bool FTPFlag = false;
                    for (int i = 0; i < 3; i++)
                    {
                        if (SaveTestData_FTP(SN, TypeName, Station, OnceTestLogging))
                        {
                            FTPFlag = true;
                            break;
                        }
                    }
                    if (!FTPFlag)
                    {
                        Config["TestResultFlag"] = TestResultFlag = false;
                        if (moreTest)
                            OnceConfig["TestResultFlag"] = TestResultFlag;
                        MessageBox.Show("TestLog上传FTP失去连接", "TE系统提示", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
                #endregion

                #region Sound Check 数据上传至FTP
                if (SC_FTP)
                {
                    string str = "";
                    bool FTPFlag = false;
                    for (int i = 0; i < 3; i++)
                    {
                        str = SaveSoundCheckData_FTP(SN, TypeName, Station);
                        if (str == "True")
                        {
                            FTPFlag = true;
                            break;
                        }
                    }
                    if (!FTPFlag)
                    {
                        Config["TestResultFlag"] = TestResultFlag = false;
                        if (moreTest)
                            OnceConfig["TestResultFlag"] = TestResultFlag;
                        MessageBox.Show($"{str}", "TE系统提示", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                #endregion
                #region 本地数据
                File.AppendAllText($@"{DirPath}\Test Logging {DateTime.Now:yy年MM月dd日}.txt", OnceTestLogging, Encoding.UTF8);
                SaveTestData_Csv(TestTable, SN, TestResultFlag, TestID);
                #endregion
                return;
            }
            catch (Exception ex)
            {
                TestResultFlag = false;
                MessageBox.Show("数据存储失败\r\n" + ex.ToString(), "TE系统提示", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                File.AppendAllText($@".\Log\错误信息{DateTime.Now:MM_dd}.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                return;
            }
            finally
            {

                Config["TestResultFlag"] = TestResultFlag;
                if (moreTest)
                    OnceConfig["TestResultFlag"] = TestResultFlag;
            }

        }

        /// <summary>
        /// 上传FTP
        /// </summary>
        /// <param name="SN"></param>
        /// <param name="TypeName"></param>
        /// <param name="Station"></param>
        /// <param name="TestLoad"></param>
        /// <returns></returns>
        bool SaveTestData_FTP(string SN, string TypeName, string Station, string TestLoad)
        {
            try
            {
                if (ftp == null) ftp = new FtpClient("ftp://10.175.5.59", "merryte", "merry@TE");

                //指定命令
                string DirectoryPath = $".\\{TypeName}\\{Station}\\TestLogging";
                string saveTxtName = $"{SN}_{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}_{ComputerName}.txt";
                if (!ftp.DirectoryExists(DirectoryPath)) ftp.CreateDirectory(DirectoryPath);
                Directory.CreateDirectory($".\\TestData\\LogToFTP");
                File.WriteAllText($".\\TestData\\LogToFTP\\{saveTxtName}", TestLoad, Encoding.UTF8);
                using (FileStream fileS = new FileStream($".\\TestData\\LogToFTP\\{saveTxtName}", FileMode.Open))
                {
                    ftp.Upload(fileS, $"{DirectoryPath}\\{saveTxtName}", FtpRemoteExists.NoCheck, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText($@".\TestData\错误信息{DateTime.Now:MM_dd}.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                ftp.Dispose();
                ftp = null;
                return false;
            }
        }
        /// <summary>
        /// 上传sound check 的数据
        /// </summary>
        /// <param name="SN"></param>
        /// <param name="TypeName"></param>
        /// <param name="Station"></param>
        /// <returns></returns>
        string SaveSoundCheckData_FTP(string SN, string TypeName, string Station)
        {
            try
            {
                if (ftp == null) ftp = new FtpClient("ftp://10.175.5.59", "merryte", "merry@TE");
                string DirectoryPath = $".\\{TypeName}\\{Station}\\SoundCheckLogging";
                //判断有没有这个文件夹
                if (!ftp.DirectoryExists(DirectoryPath)) ftp.CreateDirectory(DirectoryPath);
                if (!Directory.Exists(".\\TestData\\loggingFTP")) Directory.CreateDirectory(".\\TestData\\loggingFTP");
                string saveTxtName = "";
                //本地数据路径
                try
                {
                    saveTxtName = $"{Directory.GetFiles(SoundCheckLogPath, "*.txt")[0]}";
                }
                catch
                {
                    return $"{SoundCheckLogPath} 路径下没有检测到SoundCheck的txt数据";
                }
                //上传到FTP路径文件名
                string saveFTP_Name = $"{SN}_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.txt";
                //获取本地数据资源
                using (FileStream fileS = new FileStream($"{saveTxtName}", FileMode.Open))
                {
                    //上传到FTP
                    ftp.Upload(fileS, $"{DirectoryPath}\\{saveFTP_Name}", FtpRemoteExists.NoCheck, true);
                }
                //复制本地文件在另一个地方储存
                File.Copy(saveTxtName, $".\\TestData\\loggingFTP\\{saveFTP_Name}");
                Thread.Sleep(100);
                //清除本地文件
                File.Delete(saveTxtName);
                return "True";
            }
            catch (Exception ex)
            {
                File.AppendAllText($@".\TestData\错误信息{DateTime.Now:MM_dd}.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                ftp.Dispose();
                ftp = null;
                return $"SoundcheckData上传FTP失去连接 {ex}";
            }
        }
        /// <summary>
        /// 存储本地CSV
        /// </summary>
        /// <param name="view"></param>
        /// <param name="SN"></param>
        /// <param name="TestFlag"></param>
        void SaveTestData_Csv(DataTable view, string SN, bool TestFlag, string TestID)
        {
            string path = ".//TestData/" + DateTime.Now.ToString("yyyy-MM-dd") + "-Log.csv";
            string result = "";
            string name = "TestID,SN,Time,Result,";
            Directory.CreateDirectory(".\\TestData");
            for (var j = 0; j < view.Rows.Count; j++) result += view.Rows[j][5].ToString() + ",";
            //测试项内容
            for (int j = 1; j < view.Columns.Count - 2; j++)
            {
                name += $"\r,,,{view.Columns[j].ToString()},";
                for (var i = 0; i < view.Rows.Count; i++)
                {
                    name += view.Rows[i][j].ToString() + ",";
                }
            }
            string info = $"{TestID},{SN},{DateTime.Now.ToString("hh: mm: ss")},{TestFlag},{result}";
            //Log文件不存在则创建
            if (!File.Exists(path)) File.WriteAllText(path, name, Encoding.UTF8);
            File.AppendAllText(path, "\r\n" + info, Encoding.UTF8);
        }

        /// <summary>
        /// 罗技存储数据的方法
        /// </summary>
        /// <param name="TestFlag"></param>
        /// <param name="TypeName"></param>
        /// <param name="Station"></param>
        /// <param name="BU"></param>
        /// <param name="Project"></param>
        /// <param name="LogiStation"></param>
        /// <param name="Stage"></param>
        /// <param name="oemSource"></param>
        /// <param name="Test_Duration"></param>
        /// <param name="ipv4"></param>
        /// <param name="TestTable"></param>
        /// <param name="MesFlag"></param>
        /// <param name="SystematicName"></param>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        bool LogiTestLogCSV(bool TestFlag, string TypeName, string Station, string BU, string Project, string LogiStation,
                                            string Stage, string oemSource, string Test_Duration, string ipv4, DataTable TestTable,
                                            int MesFlag, string SystematicName, string orderNumber)
        {
            try
            {
                System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding(false);
                LogiStation = LogiStation.ToUpper();
                BU = BU.ToUpper();
                Project = Project.ToUpper();
                Stage = Stage.ToUpper();
                oemSource = oemSource.ToUpper();
                string processPath = $@".\TestItem\{TypeName}\{TypeName}.dll";
                string FilePathCsv = $@"D:\MerryTestLog\LogiTestLog\{TypeName}\{Station}\{Project}_{Stage}_{LogiStation}__{DateTime.Now:yyMMdd}_{ComputerName}_01.csv";
                string FilePathJson = $@"{Path.GetDirectoryName(FilePathCsv)}\{Project}_{Stage}_{LogiStation}__{DateTime.Now:yyMMdd}_{ComputerName}_01.json";
                FileInfo info = new FileInfo(processPath);
                #region  储存本地

                //检查UID测试项目是否在第一项


                string isUIDname = TestTable.Rows[0][1].ToString().CorrectChineseChar();
                if (!isUIDname.Contains("UID"))
                {
                    MessageBox.Show($"生成客户测试日记必须第一项包含UID测试项目", "TE系统提示", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return false;
                }

                //生成Csv文件
                if (!File.Exists(FilePathCsv) || !File.Exists(FilePathJson))
                {

                    #region 生成CSV文件
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePathCsv));
                    string processTime = $"{info.LastWriteTime:MM-dd-yyyy HH:mm:ss}";
                    //以下是客户指定的格式，不做解释，填充即可
                    string col = $@"SeqNum,Status,Comment,Test_Start_Time,Test_Duration,Failed_Tests,BU,Project,Station,Stage,CSVFileName,MAC_Addr,IP_Addr,oemSource,";
                    string T = $@",T,{DateTime.Now:MM-dd-yyyy HH:mm:ss},,";
                    string U = @",U,,,,,,,,,,,,,";
                    string L = $@",L,,,,,{BU},{Project},{LogiStation},{Stage},,,,{oemSource},";
                    string ErrorItem = "";
                    List<string> names = new List<string>();
                    for (int i = 0; i < TestTable.Rows.Count; i++)
                    {
                        string testName = TestTable.Rows[i][1].ToString().CorrectChineseChar();
                        int colindex = 0;
                        if (names.Contains(testName))
                        {
                            names.ForEach(x => colindex = x.Contains(testName) ? colindex + 1 : colindex);
                            testName += $"-{colindex}";
                        }
                        names.Add(testName);
                        col += $"{testName},";
                        L += $"{TestTable.Rows[i][3].ToString().CorrectChineseChar()},";
                        U += $"{TestTable.Rows[i][4].ToString().CorrectChineseChar()},";
                        ErrorItem += $"E{(i + 1).ToString().PadLeft(3, '0')},";

                    }
                    string P = @",#P,,";
                    string I = $@",#I,,,{processTime},{info.Name}";
                    string _L = $@",#L,,";
                    string M = @",#M,,";
                    string E = $@",#E,,,,,,,,,,,,,{ErrorItem}";
                    int commaCount = col.Length - col.Replace(",", "").Length;
                    List<string> logs = new List<string>
                    {
                        col.CommaRecover(commaCount),
                        T.CommaRecover(commaCount),
                        U.CommaRecover(commaCount),
                        L.CommaRecover(commaCount),
                        P.CommaRecover(commaCount),
                        I.CommaRecover(commaCount),
                        _L.CommaRecover(commaCount),
                        M.CommaRecover(commaCount),
                        E.CommaRecover(commaCount)
                    };

                    Thread.Sleep(100);
                    #endregion


                    string JsonTable = JSON_DataClass.TestTableToJSON(TypeName, BU, Project, LogiStation, Stage, oemSource, TestTable);
                    File.WriteAllText(FilePathJson, JsonTable, utf8);
                    File.WriteAllLines($@"{FilePathCsv}", logs.ToArray(), utf8);

                }
                //将参数写入本地
                List<string> TESTNAMES = new List<string>();
                string[] strs = File.ReadAllLines(FilePathCsv);
                string status = TestFlag ? "P" : "F";
                string Comment = " ";
                string Result;
                bool OneFalsg = true;
                int resultLength;
                int commacount = strs[0].Length - strs[0].Replace(",", "").Length;

                for (int i = 0; i < TestTable.Rows.Count; i++)
                {
                    TESTNAMES.Add(TestTable.Rows[i][1].ToString());
                    if (!TestFlag)
                        if (OneFalsg)
                            if (TestTable.Rows[i][6].ToString() != "True")
                            {
                                string error = $"{(i + 1).ToString().PadLeft(2, '0')}";
                                Comment = $"{TestTable.Rows[i][1].ToString().CorrectChineseChar()}->{error}:";
                                OneFalsg = false;
                            }
                }
                if (!TESTNAMES.Contains("UID"))
                {
                    MessageBox.Show($"生成客户测试日记必须包含UID测试项目", "TE系统提示", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return false;
                }
                Result = $"{strs.Length - 8},{status},{Comment},{DateTime.Now.ToString("HH:mm:ss")},{Test_Duration},,{BU},{Project},{LogiStation},{Stage},{Path.GetFileName(FilePathCsv)},,{ipv4},{oemSource},";

                for (int i = 0; i < TestTable.Rows.Count; i++)
                    Result += $"{TestTable.Rows[i][5].ToString().CorrectChineseChar()},";
                resultLength = Result.Length - Result.Replace(",", "").Length;
                if (commacount != resultLength)
                {
                    MessageBox.Show($"(请勿增减测试项目)\r\n已储存客户Log的测试项目格式长度{commacount}与现在的测试项格式长度{resultLength}不等 False", "TE系统提示", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return false;
                }
                Result = Result.Replace("True", "Pass").Replace("TRUE", "Pass");
                File.AppendAllText($"{FilePathCsv}", $"{Result}\r\n", utf8);
                #endregion

                //数据上传到数据库
                if ((bool)Config["LogiLogUpLoadMysql"])
                {
                    string result = "";
                    string errors = "";
                    string url = "http://10.175.5.59:8088/LGdata/Logi_test_data_upload";
                    bool uploadDataFalg = false;
                    string jsonData = "{\"modelValue\":\"" + TypeName + "\",\"status\":\"" + status + "\",\"comment\":\"" + Comment + "\",\"test_Duration\":\"" + Test_Duration + "\",\"failed_Tests\":\" \"," +
                    "\"bu\":\"" + BU + "\",\"project\":\"" + Project + "\",\"station\":\"" + LogiStation + "\",\"stage\":\"" + Stage + "\",\"maC_Addr\":\" \",\"iP_Addr\":\"" + ipv4 + "\"," +
                    "\"oemSource\":\"" + oemSource + "\",\"dllName\":\"" + info.Name + "\",\"dllTime\":\"" + info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"mesFlag\":\"" + MesFlag + "\"," +
                    "\"mesName\":\"" + SystematicName + "\",\"workorders\":\"" + orderNumber + "\",\"machineName\":\"" + ComputerName + "\"," +
                    "\"TestLog\":\"" + TestLog_CorrectChineseChar(TestTable) + "\"}";
                    for (int i = 0; i < 3; i++)
                    {
                        result = HttpPost(url, jsonData,
                            "POST", out errors);
                        if (result == "200")
                        {
                            uploadDataFalg = true;
                            break;
                        }
                    }
                    if (!uploadDataFalg)
                    {
                        MessageBox.Show($"上传Logi测试数据API异常{result}\r\n{jsonData}\\错误信息:\r\n{errors}\r\n", "TE系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        File.AppendAllText($@".\Log\上传Logi测试数据API异常.txt", $"{DateTime.Now}\r\n{jsonData}\r\n异常信息{result}\r\n错误内容{errors}\r\n");

                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成客户测试日记报错了\r\n{ex}", "TE系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }

        }
        /// <summary>
        /// 获取测试界面的表格
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        string TestLog(DataTable table)
        {
            string @string = "";
            for (var j = 0; j < table.Rows.Count; j++)
            {
                string Result = table.Rows[j][6].ToString() == true.ToString() ? "P" : "F";
                string testitem = table.Rows[j][1].ToString().CorrectCharacter();
                string LowLimit = table.Rows[j][3].ToString().CorrectCharacter();
                string UpLimit = table.Rows[j][4].ToString().CorrectCharacter();
                string TestValue = table.Rows[j][5].ToString().CorrectCharacter();
                @string += $@"{testitem},{Result},{TestValue},{LowLimit},{UpLimit}#";
            }
            return @string;
        }
        string TestLog_CorrectChineseChar(DataTable table)
        {
            string @string = "";
            List<string> names = new List<string>();
            for (var j = 0; j < table.Rows.Count; j++)
            {
                string Result = table.Rows[j][6].ToString() == true.ToString() ? "P" : "F";
                string testName = table.Rows[j][1].ToString().CorrectChineseChar();
                int colindex = 0;
                if (names.Contains(testName))
                {
                    names.ForEach(x => colindex = x.Contains(testName) ? colindex + 1 : colindex);
                    testName += $"-{colindex}";
                }
                names.Add(testName);
                string LowLimit = table.Rows[j][3].ToString().CorrectChineseChar();
                string UpLimit = table.Rows[j][4].ToString().CorrectChineseChar();
                string TestValue = table.Rows[j][5].ToString().CorrectChineseChar();
                @string += $@"{testName},{Result},{TestValue},{LowLimit},{UpLimit}#";
            }
            return @string;
        }

        DataTable listViewToTable(ListView view)
        {
            DataTable table = new DataTable();
            table.Columns.Add("编号");
            table.Columns.Add("测试项目");
            table.Columns.Add("单位");
            table.Columns.Add("数值下限");
            table.Columns.Add("数值上限");
            table.Columns.Add("测试值");
            table.Columns.Add("测试结果");
            for (int i = 0; i < view.Items.Count; i++)
            {
                table.Rows.Add(new object[] {
                    view.Items[i].SubItems[0].Text,
                    view.Items[i].SubItems[1].Text,
                    view.Items[i].SubItems[2].Text,
                    view.Items[i].SubItems[3].Text,
                    view.Items[i].SubItems[4].Text,
                    view.Items[i].SubItems[5].Text,
                    view.Items[i].SubItems[6].Text
                });

            }

            return table;

        }
        #endregion

    }

    public static class class1
    {
        public static string CommaRecover(this string str, int length)
        {
            int l = str.Length - str.Replace(",", "").Length;
            int diff = length - l;
            for (int i = 0; i < diff; i++)
                str += ",";
            return str;

        }


        public static string CorrectChineseChar(this string str)
        {
            if (CheckStringChineseReg(str))
                str = GetNumberAlpha(str);
            //匹配所有要替换掉的字符和符号的正则表达式
            //输出带有各种字符符号的字符串
            return str.CorrectCharacter();

        }
        public static string CorrectCharacter(this string str)
        {
            //匹配所有要替换掉的字符和符号的正则表达式
            string strPattern = "‘|’|“|”|<|>|,|：|#|!|！|；|;|N/A|{|}|&|（|）|\"|\'|\0|\r|\n|\t";
            //输出带有各种字符符号的字符串
            return System.Text.RegularExpressions.Regex.Replace(str, strPattern, " ").Replace("+", "").Replace(@"\", @"/");
        }

        /// <summary>
        /// 判断有没有中文
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static bool CheckStringChineseReg(string text)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(text, @"[\u4e00-\u9fbb]");
        }
        /// <summary>
        /// 只保留数字和英文字符串
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        static string GetNumberAlpha(string source)
        {
            string pattern = @"[A-Za-z0-9\+\-\.\:]";
            string strRet = "";
            MatchCollection results = Regex.Matches(source, pattern);
            foreach (var v in results)
            {
                strRet += v.ToString();
            }
            return strRet;
        }


    }

}
