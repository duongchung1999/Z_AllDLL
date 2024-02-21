using JSON_DATA;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;

namespace SamplePlate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.Items.Add(new ListViewItem(new string[] { "1", "UID", "N/A", "N/A", "N/A", "230000000000", "True" }));
            listView1.Items.Add(new ListViewItem(new string[] { "2", "Current Test", "uA", "0", "100", "50", "True" }));
            listView1.Items.Add(new ListViewItem(new string[] { "3", "Voltage Test", "V", "3", "5", "2.9", "False" }));
            string url = "http://10.55.2.25:8088/LGdata/Logi_test_data_upload";
            string str = @"{""modelValue"":""HDT625"",""status"":""T"",""comment"":"""",""test_Duration"":""0.000"",""failed_Tests"":"""",""bu"":""VC-HEADSET"",""project"":""CYBERMORPH"",""station"":""T3.12"",""stage"":""MP"",""maC_Addr"":""2336MH0010B8"",""iP_Addr"":""10.55.103.18"",""oemSource"":""MERRY_HZ"",""dllName"":"""",""dllTime"":""1904-01-01 08:00:00"",""mesFlag"":""1"",""mesName"":""MECH_Chrome"",""workorders"":""001100425167"",""machineName"":""CFI-FIM3F1BRUP2.mer"",""TestLog"":""Input UID-Serial Number,P,2336MH0010B8,,#ANC-ANC Status,P,P,,#AMB-ANC Status,P,P,,#Burn Playback Gain-Status,P,P,,#ANC OFF FR-Frequency response_Left,P,P,NA,NA#ANC OFF FR-Frequency response_Right,P,P,NA,NA#ANC-Left Ref Spectrum,P,P,NA,NA#ANC-Left ANC Delta Spectrum,P,P,NA,NA#ANC-Right Ref Spectrum,P,P,NA,NA#ANC-Right ANC Delta Spectrum,P,P,NA,NA#ANC-Left FB ANC Delta Spectrum,P,P,NA,NA#ANC-Right FB ANC Delta Spectrum,P,P,NA,NA#ANC ON FR-Frequency response_Left,P,P,NA,NA#ANC ON FR-Frequency response_Right,P,P,NA,NA#AMB-Left ANC Delta Spectrum,P,P,NA,NA#AMB-Right ANC Delta Spectrum,P,P,NA,NA#AMB-ANC Balance,P,P,NA,NA#""}";
            string result = HttpPost(url, str,
                        "POST", out Exception errors);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = "http://10.55.22.160:8088/upload/Test_data_upload";
            string WorkOrder = "001100000000";//工单
            string TypeName = "HDT624"; //机型专案编号
            string Station = "T2.0";//测试工站
            string ComputerName = Environment.MachineName;
            string TestTime = "60";//测试时间 以秒为单位
            string SerialNumber = "MEC012345678"; //条码
            string TestResultFlag = "False"; //最终的测试结果  True or False
            string testdata = TestLog(listViewToTable(listView1));
            string uploadData = "{" + $"\"modelValue\":\"{TypeName}\",\"SN\":\"{SerialNumber}\",\"Result\":\"{TestResultFlag}\",\"Station\":\"{Station}\",\"workorders\":\"{WorkOrder}\",\"MachineName\":\"{ComputerName}\",\"TestTime\":\"{TestTime}\",\"TestLog\":\"{testdata}\"" + "}";
            string response = HttpPost(url, uploadData, "Post", out Exception error);
            if (response == "200")
            {
                label1.Text = "OK";
                label1.BackColor = Color.Green;
            }
            else
            {
                label1.Text = "NG";
                label1.BackColor = Color.Red;
            }
            
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
         
        public static string HttpPost(string url, string Writedata, string Method, out Exception error)
        {
            error = null;
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(Writedata);
                //参数类型，这里是json类型
                //还有别的类型如"application/x-www-form-urlencoded"，不过我没用过(逃
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "text/plain";
                //参数数据长度
                httpWebRequest.ContentLength = bs.Length;
                //设置请求类型 
                httpWebRequest.Method = Method;
                //设置超时时间
                httpWebRequest.Timeout = 4000;
                //将参数写入请求地址中
                if (Writedata != null) httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                //发送请求
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //读取返回数据
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                httpWebRequest.Abort();
                return responseContent;
            }
            catch (Exception ex)
            {
                error = ex;
                return $"{error.Message} False";
            }
        }
        public static string HttpPost(string url, string Writedata, string Method, out string error)
        {
            error = "";
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(Writedata);
                //参数类型，这里是json类型
                //还有别的类型如"application/x-www-form-urlencoded"，不过我没用过(逃
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "text/plain";
                //参数数据长度
                httpWebRequest.ContentLength = bs.Length;
                //设置请求类型 
                httpWebRequest.Method = Method;
                //设置超时时间
                httpWebRequest.Timeout = 4000;
                //将参数写入请求地址中
                if (Writedata != null) httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                //发送请求
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //读取返回数据
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                httpWebRequest.Abort();
                return responseContent;
            }
            catch (Exception ex)
            {
                error = ex.Message.Contains("無法連接至遠端伺服器") ? "网络断开连接，请连接网络" : ex.ToString();
                File.AppendAllText($@".\Log\错误信息{DateTime.Now:MM_dd}.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                return $"{ex} False";
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {


            string LogiBU = this. LogiBU.Text;
            string LogiProject = this.LogiProject.Text;
            string LogiStage = this.LogiStage.Text;
            string LogioemSource = this.LogioemSource.Text;
            string LogiStation = this.LogiStation.Text;
            string WorkOrder = "001100000000";//工单
            string TestResultFlag = "False"; //最终的测试结果  True or False
            string ipv4 = null;
            string TypeName = "HDT624"; //机型专案编号
            string Station = "T2.0";//测试工站
            string TestTime = "60";//测试时间 以秒为单位

            string ComputerName = Environment.MachineName;
            DataTable TestTable =  listViewToTable(listView1);
            int MesFlag = 1;
            string SystematicName = "MECH_Chrome";


            if (ipv4 == null)
                foreach (var ipa in Dns.GetHostAddresses(ComputerName)) if (ipa.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipv4 = ipa.ToString();
                        break;
                    }
            bool logicsvResult = LogiTestLogCSV(TestResultFlag=="True", TypeName, Station, LogiBU, LogiProject,
                                                       LogiStation, LogiStage, LogioemSource, TestTime, ipv4, TestTable, MesFlag, SystematicName, WorkOrder);

            if (logicsvResult  )
            {
                label2.Text = "OK";
                label2.BackColor = Color.Green;
            }
            else
            {
                label2.Text = "NG";
                label2.BackColor = Color.Red;
            }


        }
        string ComputerName = Environment.MachineName;

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
                if (true)
                {
                    string result = "";
                    string errors = "";
                    string url = "http://10.55.2.25:8088/LGdata/Logi_test_data_upload";
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
