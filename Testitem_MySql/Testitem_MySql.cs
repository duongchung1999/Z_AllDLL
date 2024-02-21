using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentFTP;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using JSON_DATA;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法
        string SoundCheckLogPath = ".\\TestData\\SoundcheckDataUploadFTP";
        Dictionary<string, object> keys;
        public string Interface(Dictionary<string, object> keys) => (this.keys = keys).ToString();
        public string ReadTestItem()
        {

            //机型
            string TypeName = (string)keys["Name"];
            //站别
            string Station = (string)keys["Station"];
            //是否连接数据库
            bool NetworkFlag = (bool)keys["NetworkFlag"];
            //本地txt
            string Path = $@".\TestItem\{TypeName}\{Station}.txt";
            List<string[]> testitems = new List<string[]>();
            if (NetworkFlag)
            {
                testitems = GetTestItems(TypeName, Station);
                if (testitems != null)
                {

                    string[] str = new string[testitems.Count];
                    for (int i = 0; i < testitems.Count; i++)
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
                    WretelineTxt($@".\TestItem\{TypeName}\{Station}.txt", str);
                    keys.Add("TestItem", testitems);
                    return "读数据库项目";
                }

            }
            testitems = ReadStationTxt(Path);
            keys.Add("TestItem", testitems);
            return "读本地项目";
        }
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Testitem_MySql";
            string dllfunction = "Dll功能说明 ：加载测试项目";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllHistoryVersion2 = "                     ：0.0.1.2";
            string dllHistoryVersion3 = "                     ：2021.7.31.09";
            string dllHistoryVersion4 = "                     ：2021.8.2.10";
            string dllHistoryVersion5 = "                     ：2021.10.20.3";
            string dllHistoryVersion6 = "                     ：21.12.9.5";


            string dllVersion = "当前Dll版本：22.3.14.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo2 = "0.0.1.2：2021/7/15：增加测试数据上传MySql";
            string dllChangeInfo3 = "2021.7.31.00：2021/7/31：更改读取测试项的数据库";
            string dllChangeInfo4 = "2021.8.2.10：2021 /8/10：增加创建测试项目文件夹,增加存储程序运行测试时间，增加TestLog上传FTP";
            string dllChangeInfo5 = "2021.10.20.3：2021 /10/20：增加计算检测程序突发闪退功能，检测程序打开次数，修复上下限问题，当MES标志位打开时也会上传数据";
            string dllChangeInfo6 = "2021.12.9.5：2021 /12/9：当MES标志位打开时也会上传数据";
            string dllChangeInfo7 = "22.3.14.0：当MES标志位打开时也会上传测试数据到数据库，FTP部分可以自定义上传";

            string[] info = { dllname,
                dllfunction,
                dllHistoryVersion,dllHistoryVersion2,dllHistoryVersion3,dllHistoryVersion4,dllHistoryVersion5,dllHistoryVersion6,
                dllVersion,
                dllChangeInfo,
            dllChangeInfo2,dllChangeInfo3,dllChangeInfo4,dllChangeInfo5,dllChangeInfo6,dllChangeInfo7
            };
            return info;
        }
        public string Run(object[] Command) => "";
        bool Flag = true;
        public string UpLoadProgramStart()
        {
            if (!Flag) return "";
            if ((bool)keys["NetworkFlag"])
            {
                Flag = false;
                List<string> _Program = MySqlHelper.GetData($"SELECT Count ,ProgramOpenCount FROM ProgramDeadBusy " +
                    $"WHERE TypeName = '{keys["Name"]}' " +
                    $"AND Station = '{keys["Station"]}' " +
                    $"AND ComputerName = '{ComputerName}' " +
                    $"AND DateTime = '{_DayOfYear}' ;").Result;
                if (_Program == null) return "";


                if (_Program.Count <= 0)
                {
                    bool a = MySqlHelper.ExecuteSql("INSERT INTO ProgramDeadBusy " +
                                  $"(TypeName,Station,ComputerName,Count,ProgramOpenCount,DateTime)VALUES('{keys["Name"]}', '{keys["Station"]}' ,'{ComputerName}',1,1,{_DayOfYear}) ; ").Result;
                }
                else
                {
                    bool b = MySqlHelper.ExecuteSql($"UPDATE ProgramDeadBusy SET Count={Convert.ToInt32(_Program[0]) + 1} " +
                    $",ProgramOpenCount={Convert.ToInt32(_Program[1]) + 1}  " +
                    $"WHERE TypeName = '{keys["Name"]}' " +
                    $"AND Station = '{keys["Station"]}' " +
                    $"AND ComputerName = '{ComputerName}' " +
                    $"AND DateTime = '{_DayOfYear}' ;").Result;

                }

            }

            return "";
        }
        public string UpLoadProgramClose()
        {
            List<string> _Program = MySqlHelper.GetData($"SELECT Count FROM ProgramDeadBusy " +
                   $"WHERE TypeName = '{keys["Name"]}' " +
                    $"AND Station = '{keys["Station"]}' " +
                    $"AND ComputerName = '{ComputerName}' " +
                    $"AND DateTime = '{_DayOfYear}' ;").Result;
            if (_Program == null) return "";
            if (_Program.Count > 0)
            {
                bool b = MySqlHelper.ExecuteSql($"UPDATE ProgramDeadBusy SET Count={Convert.ToInt32(_Program[0]) - 1} " +
                   $"WHERE TypeName = '{keys["Name"]}' " +
                   $"AND Station = '{keys["Station"]}' " +
                   $"AND ComputerName = '{ComputerName}' " +
                   $"AND DateTime = '{_DayOfYear}' ;").Result;
            }
            return "";
        }
        #endregion

        #region 读测试项目
        /// <summary>
        /// 查询测试站别
        /// </summary>
        private List<string> GetTestStations(string TypeName)
        {
            if (!MySqlHelper.IsOpen()) return new List<string>(null);
            string sql = $@"SELECT a1.`name`
FROM station a1 ,model  a2
WHERE a1.model_id=a2.id  AND a2.`name`='{TypeName}'";
            return MySqlHelper.GetData(sql).Result;
        }
        /// <summary>
        /// 查询测试项目
        /// </summary>
        private List<string[]> GetTestItems(string TypeName, string Station)
        {
            var sql = $@"SELECT testitem.`name`, testitem.unit,testitem.lower_value,testitem.upper_value,testitem.`no` , testitem.cmd
                from model 
                inner join station on model.id = station.model_id and model.`name` = '{TypeName}' 
                inner join station_testitem on station.`name` =  '{Station}' and station.id = station_testitem.station_id 
                inner join testitem on station_testitem.testitem_id = testitem.id ORDER BY station_testitem.`sort_index`;";
            List<string[]> itemdata = MySqlHelper.GetDataList(sql).Result;
            if (itemdata == null || itemdata.Count == 0) return null;
            int delaynums = 1;
            List<string[]> TestItems = new List<string[]>();
            itemdata.ForEach(item =>
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
            });
            return TestItems;
        }
        /// <summary>
        /// 读取本地Txt
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<string[]> ReadStationTxt(string path)
        {
            List<string[]> StationTxt = new List<string[]>();
            try
            {
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                {
                    int i = 1;
                    while (!sr.EndOfStream)
                    {
                        var str = sr.ReadLine().Trim();
                        if (str == null || str == "") break;
                        string[] strs = str.Split(',');

                        StationTxt.Add(new string[] { strs[1].Contains("-") ? "0" : $"{i++}", strs[1], strs[2], strs[3], strs[4], strs[5], strs[6] });

                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }
            return StationTxt;

        }
        /// <summary>
        /// 写出Txt
        /// </summary>
        /// <param name="path"></param>
        /// <param name="str"></param>
        private void WretelineTxt(string path, string[] str)
        {
            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(file))
                {
                    foreach (var item in str) writer.WriteLine(item);
                }
            }
        }
        #endregion
        public string ClearSoundCheckLogging()
        {
            if ((bool)keys["SoundcheckDataUploadFTP"])
            {
                try
                {

                    if (Directory.Exists(SoundCheckLogPath)) Directory.Delete(SoundCheckLogPath, true);
                    Thread.Sleep(400);
                    Directory.CreateDirectory(SoundCheckLogPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"清空Sound Check Data失败，请关闭文件夹并重启程序\r\n\r\n{ex}");
                }


            };
            return "";
        }

        #region 保存测试数据
        int _DayOfYear = DateTime.Now.DayOfYear;
        string ComputerName = Environment.MachineName;
        FtpClient ftp = null;

        public string SaveTestLogging()
        {
            string path = $@".\TestData\{DateTime.Now.ToString("yyyy-MM-dd")} Test Logging.txt";
            string[] str = { (string)keys["TestLogging"] };
            Writer(str, path);
            _ = Task.Run(() =>
              {
                  try
                  {
                      MySqlTestData.Closed();
                  }
                  catch (Exception ex)
                  {
                      File.AppendAllText(@".\TestData\bug.txt", $"{DateTime.Now.ToString()}\r\n{ex}\r\n");
                  }
              });
            return "True";
        }
        /*生成本地Log*/

        public string SaveTestData()
        {
            try
            {
                ListView view = (ListView)keys["TestTable"];
                string SN = (string)keys["SN"];
                bool TestFlag = (bool)keys["TestResultFlag"];
                string Station = (string)keys["Station"];
                string TypeName = (string)keys["Name"];
                string WorkOrder = (string)keys["Works"];
                if (SN.Contains("TE_BZP")) WorkOrder = "000000000000";
                bool TestLogUploadMySQL = (bool)keys["TestLogUploadMySQL"] || ((int)keys["MesFlag"]) > 0;
                string TestTime = ((TimeSpan)keys["TestTime"]).TotalSeconds.ToString("0.00");
                string OnceTestLogging = (string)keys["OnceTestLogging"];
                bool UploadFTP = (bool)keys["UploadFTP"];
                bool SC_FTP = (bool)keys["SoundcheckDataUploadFTP"];
                bool LogiTestLog = (bool)keys["LogiTestLog"];
                string build = (string)keys["Build"];
                #region 生成Logi的测试数据
                if (LogiTestLog)
                {
                    if (!SN.Contains("TE_BZP"))
                        if (!LogiTestLogCSV(SN, TestFlag, TypeName, "T2", build, TestTime, view))
                            keys["TestResultFlag"] = false;
                }
                #endregion   

                #region 上传数据库
                if (TestLogUploadMySQL)
                {
                    string SQL = $@"(SN, Result, Testlog, Station, workorders,MachineName,TestTime)VALUES( '{SN}','{TestFlag}','{TestLog(view)}','{Station}','{WorkOrder}','{ComputerName }','{TestTime}');";
                    bool MysqlFlag = false;
                    for (int i = 0; i < 6; i++)
                    {
                        if (MySqlTestData.ExecuteSql(TypeName, SQL).Result)
                        {
                            MysqlFlag = true;
                            break;
                        }
                    }
                    if (!MysqlFlag)
                    {
                        keys["TestResultFlag"] = false;
                        MessageBox.Show("与MySql失去连接");
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
                        keys["TestResultFlag"] = false;
                        MessageBox.Show("TestLog上传FTP失去连接");
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
                        keys["TestResultFlag"] = false;
                        MessageBox.Show($"{str}");
                    }
                }

                #endregion

                #region 本地数据
                SaveTestData_Csv(view, SN, TestFlag);

                #endregion


                return "Save TestData True";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return "Save TestData False";
            }
        }
        string ipv4 = null;

        bool LogiTestLogCSV(string SN, bool TestFlag, string TypeName, string Station, string Stage, string TestTime, ListView TestTable)
        {
            try
            {
                string LogiStation = Station.Replace(".", "").ToUpper();
                string BU = "VC-Headset".ToUpper();
                string Project = "Mulberries".ToUpper();
                string oemSource = "MERRY_HZ".ToUpper();

                string processPath = $@".\TestItem\{TypeName}\{TypeName}.dll";
                FileInfo info = new FileInfo(processPath);
                string FilePathCsv = $@"D:\MerryTestLog\LogiTestLog\{TypeName}\{Station}\{Project}_{Stage}_{LogiStation}__{DateTime.Now:yyMMdd}_{ComputerName}.csv";
                string FilePathJson = $@"{Path.GetDirectoryName(FilePathCsv)}\{Project}_{Stage}_{LogiStation}__{DateTime.Now:yyMMdd}_{ComputerName}.json";
                if (!File.Exists(FilePathCsv) || !File.Exists(FilePathJson))
                {

                    #region 生成CSV文件
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePathCsv));
                    string processTime = $"{info.LastWriteTime:MM-dd-yyyy HH:mm:ss}";
                    //以下是客户指定的格式，不做解释，填充即可
                    string col = $@"SeqNum,Status,Comment,Test_Start_Time,Test_Duration,Failed_Tests,BU,Project,Station,Stage,CSVFileName,MAC_Addr,IP_Addr,oemSource,";
                    string T = $@",T,{DateTime.Now:MM-dd-yyyy HH:mm:ss},,";
                    string U = @",U,,,,,,,,,,,,,";
                    string L = $@",L,,,,,{BU},{Project},{LogiStation.Replace(".", "")},{Stage},,,,{oemSource},";
                    string ErrorItem = "";
                    for (int i = 0; i < TestTable.Items.Count; i++)
                    {
                        col += $"{TestTable.Items[i].SubItems[1].Text.Replace(",", "")},";
                        L += $"{TestTable.Items[i].SubItems[3].Text.Replace(",", "")},";
                        U += $"{TestTable.Items[i].SubItems[4].Text.Replace(",", "")},";
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
                    File.WriteAllLines($@"{FilePathCsv}", logs.ToArray(), Encoding.UTF8);
                    Thread.Sleep(100);
                    #endregion


                    #region Json

                    string JsonTable = JSON_DataClass.TestTableToJSON(TypeName, BU, Project, LogiStation, Stage, oemSource, TestTable);

                    File.WriteAllText(FilePathJson, JsonTable, Encoding.UTF8);

                    #endregion

                }
                if (ipv4 == null)
                    foreach (var ipa in Dns.GetHostAddresses(ComputerName)) if (ipa.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipv4 = ipa.ToString();
                            break;
                        }

                string[] strs = File.ReadAllLines(FilePathCsv);
                int commacount = strs[0].Length - strs[0].Replace(",", "").Length;
                List<string> TESTNAMES = new List<string>();
                string ErrorCode = "";
                bool OneFalsg = true;
                for (int i = 0; i < TestTable.Items.Count; i++)
                {
                    TESTNAMES.Add(TestTable.Items[i].SubItems[1].Text);
                    if (!TestFlag)
                        if (OneFalsg)
                            if (TestTable.Items[i].SubItems[6].Text != "True")
                            {
                                string error = LogiStation.Replace("T", "").Replace(".", "") + $"{(i + 1).ToString().PadLeft(2, '0')}";
                                ErrorCode = $"{TestTable.Items[i].SubItems[1].Text.Replace(".", "")}->{error}:";
                                OneFalsg = false;
                            }
                }

                if (!TESTNAMES.Contains("UID"))
                {
                    MessageBox.Show($"生成客户测试日记必须包含UID测试项目");
                    return false;
                }
                string RST = TestFlag ? "P" : "F";
                string Result = $"{strs.Length - 8},{RST},{ErrorCode},{DateTime.Now.ToString("HH:mm:ss")},{TestTime},,{BU},{Project},{LogiStation},{Stage},{Path.GetFileName(FilePathCsv)},,{ipv4},{oemSource},";
                for (int i = 0; i < TestTable.Items.Count; i++)
                    Result += $"{TestTable.Items[i].SubItems[5].Text.Replace(".", "")},";
                int resultLength = Result.Length - Result.Replace(",", "").Length;
                if (commacount != resultLength)
                {
                    MessageBox.Show($"(请勿增减测试项目)\r\n已储存客户Log的测试项目格式长度{commacount}与现在的测试项格式长度{resultLength}不等 False");
                    return false;
                }
                File.AppendAllText($"{FilePathCsv}", $"{Result}\r\n", Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成客户测试日记报错了\r\n{ex}");
                return false;
            }

        }
        bool SaveTestData_FTP(string SN, string TypeName, string Station, string TestLoad)
        {
            try
            {
                if (ftp == null) ftp = new FtpClient("ftp://10.55.2.19", "merryte", "merry@TE");

                //指定命令
                string DirectoryPath = $".\\{TypeName}\\{Station}\\TestLogging";
                string saveTxtName = $"{SN}_{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}_{ComputerName}.txt";
                if (!ftp.DirectoryExists(DirectoryPath)) ftp.CreateDirectory(DirectoryPath);
                Directory.CreateDirectory($".\\TestData\\LogToFTP");
                File.WriteAllText($".\\TestData\\LogToFTP\\{saveTxtName}", TestLoad);
                using (FileStream fileS = new FileStream($".\\TestData\\LogToFTP\\{saveTxtName}", FileMode.Open))
                {
                    ftp.Upload(fileS, $"{DirectoryPath}\\{saveTxtName}", FtpRemoteExists.NoCheck, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText($".\\TestData\\{DateTime.Now.ToString("MM_dd")}_bug.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                ftp.Dispose();
                ftp = null;
                return false;
            }
        }

        string SaveSoundCheckData_FTP(string SN, string TypeName, string Station)
        {
            try
            {
                if (ftp == null) ftp = new FtpClient("ftp://10.55.2.19", "merryte", "merry@TE");
                string DirectoryPath = $".\\{TypeName}\\{Station}\\SoundCheckLogging";
                //判断有没有这个文件夹
                if (!ftp.DirectoryExists(DirectoryPath)) ftp.CreateDirectory(DirectoryPath);
                if (!Directory.Exists(".\\TestData\\loggingFTP")) Directory.CreateDirectory(".\\TestData\\loggingFTP");
                string saveTxtName = "";
                //本地数据路径
                try
                {
                    saveTxtName = $"{ Directory.GetFiles(SoundCheckLogPath, "*.txt")[0]}";
                }
                catch
                {
                    return $"{ SoundCheckLogPath} 路径下没有检测到SoundCheck的txt数据";
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
                File.AppendAllText($".\\TestData\\{DateTime.Now.ToString("MM_dd")}_bug.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                ftp.Dispose();
                ftp = null;
                return $"SoundcheckData上传FTP失去连接 {ex}";
            }
        }

        void SaveTestData_Csv(ListView view, string SN, bool TestFlag)
        {
            string path = ".//TestData/" + DateTime.Now.ToString("yyyy-MM-dd") + "-Log.csv";
            string result = "";
            string name = "SN,Time,Result,";
            Directory.CreateDirectory(".\\TestData");
            for (var j = 0; j < view.Items.Count; j++) result += view.Items[j].SubItems[5].Text + ",";
            //测试项内容
            for (int j = 1; j < view.Columns.Count - 2; j++)
            {
                name += $"\r,,{ view.Columns[j].Text},";
                for (var i = 0; i < view.Items.Count; i++)
                {
                    name += view.Items[i].SubItems[j].Text + ",";
                }
            }
            string info = SN + "," + DateTime.Now.ToString("hh:mm:ss") + "," + TestFlag + "," + result;
            //Log文件不存在则创建
            if (!File.Exists(path)) Writer(new string[] { name }, path);
            Writer(new string[] { info }, path, true);
        }
        private string TestLog(ListView view)
        {
            string @string = "";
            for (var j = 0; j < view.Items.Count; j++)
            {
                string Result = view.Items[j].SubItems[6].Text == true.ToString() ? "P" : "F";
                string testitem = view.Items[j].SubItems[1].Text.Replace(";", "").Replace("#", "").Replace(",", "").Replace("'", "");
                string LowLimit = view.Items[j].SubItems[3].Text.Replace(";", "").Replace("#", "").Replace(",", "").Replace("'", "");
                string UpLimit = view.Items[j].SubItems[4].Text.Replace(";", "").Replace("#", "").Replace(",", "").Replace("'", "");
                string TestValue = view.Items[j].SubItems[5].Text.Replace(";", "").Replace("#", "").Replace(",", "").Replace("'", "");
                if (view.Items[j].SubItems[5].Text != "False")
                {
                    string Lolimit = LowLimit == "N/A" ? "" : LowLimit;
                    string uplimit = UpLimit == "N/A" ? "" : UpLimit;
                    @string += $@"{testitem},{Result},{TestValue},{Lolimit },{uplimit}#";
                    continue;
                }
                @string += $@"{testitem},{Result},,,#";
            }
            return @string;
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
        #endregion

    }
    public static class class1
    {
        public static string CommaRecover(this string str, int length)
        {
            int l = str.Length - str.Replace(",", "").Length;
            int diff = l - length;

            for (int i = 0; i < diff; i++)
                str += ",";
            return str;

        }


    }
}