using MerryDllFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SoundCheck_V1.API
{
    public static class SoundCheck
    {
        //string IP_Address = "127.0.0.1"; // loopback address (also known as localhost)
        //string PortNumber = "4444"; // Default port ;
        static TcpControl tcp = new TcpControl("127.0.0.1", "4444");
        //static TcpControl tcp = new TcpControl("10.55.96.197", "4444");

        static JArray stepsList;
        public static bool IsStartTest;
        public static bool IsConnect
        {
            get { return tcp.IsConnect; }
        }
        /// <summary>
        /// 表示程序第一次启动或者第一次启动sound check 才会执行Sqc
        /// </summary>
        public static bool FirstRunSequence = true;

        public static string OpenSoundCheck(string SoundCheckPath)
        {
            //检测SC是否启动 已经启动就没必要再执行了，直接出去
            foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                if (thisProc.ProcessName.Contains("SoundCheck"))
                    return "True";
            //没有启动就检测路径下有没有文件
            if (!File.Exists(SoundCheckPath))
            {
                MessageBox.Show($"没有在指定路径下找到文件\r\n{SoundCheckPath}", "SoundCheck_V1提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "Not Found File False";
            }
            FirstRunSequence = true;
            //启动                        最大化 ，如果不输入参数面对 SC 17 很大概率报错
            Process.Start(SoundCheckPath, "-m"); // Start SoundCheck with commandline arguments
            // 搞个进度条 等待SC 正常启动
            scForm scform = new scForm(65000);
            scform.TopMost = true;
            scform.ShowDialog();
            return (scform.DialogResult == DialogResult.Yes).ToString();
        }

        public static string ConnectSoundCheck()
        {
            try
            {
                if (tcp.IsConnect)
                    return "True";
                if (!tcp.Connect())
                    return "Connect False";
                string receive = tcp.ReadLine();
                // SoundCheck will send an acknowledgement on connection. Read it.
                // Send command to SoundCheck to set the strings to receive for 'NaN','Infinity','-Infinity' float values.
                // C# uses the same strings as SoundCheck, so this step wouldn't be necessary if this were the 
                // only application connecting to SoundCheck.

                return tcp.WriteCommandAndGetResponse("SoundCheck.SetFloatStrings('NaN','Infinity','-Infinity')");

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString() + "\r\n" + "Could not connect to SoundCheck because the target machine refused it." + Environment.NewLine +
                    "Please make sure that TCP/IP server is enabled in SoundCheck Preferences dialog and try again.");
                Disconnect();
                return $"{ex.Message} False";
            }


        }

        public static string RunSequence(string SequencePath)
        {
            string Receive = tcp.WriteCommandAndGetResponse("Sequence.Open('" + SequencePath + "')");
            if (Receive.Contains("Flase")) // Send Sequence.Open command and wait for result.
                return Receive;
            Receive = tcp.WriteCommandAndGetResponse("Sequence.GetStepsList");
            if (Receive.Contains("Flase")) // Send Sequence.GetStepsList command and wait for result.
                return Receive;
            //Column Headers
            string[] colHeaders = new string[] { "Step Name", "Step Type", "Input Channel", "Output Channel" };

            DataTable dataTable = InitializeDataTable(colHeaders.Length);

            // Populate Data Table
            stepsList = tcp.jArray; // Convert return data to dynamic objects array

            foreach (JObject row in stepsList.Children<JObject>())
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow[0] = row.Value<string>("Name");
                dataRow[1] = row.Value<string>("Type");
                dataRow[2] = FormatChannelNames(row.Value<JArray>("InputChannelNames"));
                dataRow[3] = FormatChannelNames(row.Value<JArray>("OutputChannelNames"));
                dataTable.Rows.Add(dataRow);
            }
            FirstRunSequence = false;
            // Update DataGridView
            return "True";
        }

        public static string SetLotNumber(string LotNumber)
        {
            return tcp.WriteCommandAndGetResponse("SoundCheck.SetLotNumber('" + LotNumber + "')");
        }

        public static string SetSerialNumber(string SerialNumber)
        {
            return tcp.WriteCommandAndGetResponse("SoundCheck.SetSerialNumber('" + SerialNumber + "')"); // Send SoundCheck.SetSerialNumber command and wait for result.

        }

        public static string StartTest()
        {
            string Receive = tcp.WriteLine("Sequence.Run"); // Send Sequence.Run command and wait for result.
            IsStartTest = true;
            StepNames.ForEach(item => StepResultDic[item] = true);
            StepNamesMargin.ForEach(item => StepResultMargin[item] = true);
            return Receive;
        }
        public static string GetFinalResults()
        {
            string Receive = tcp.ReadCommandCompleted();
            if (Receive.Contains("False"))
                return Receive;
            if (!tcp.GetReturnDataBoolean("Pass?"))
                return "Result False";
            #region MyRegion
            ////获取一个边缘值
            //string labelMargin = tcp.GetReturnDataDouble("Margin").ToString();
            //// Update Data Table
            //// Column Headers
            //string[] colHeaders = new string[] { "Step Name", "Step Type", "Input Channel", "Output Channel", "Verdict", "Margin", "Limit", "Max/Min" };
            //DataTable dataTable = InitializeDataTable(colHeaders.Length);
            //// Populate Data Table
            //JArray stepResults = tcp.json.returnData.Value<JArray>("StepResults"); // Convert return data to dynamic objects array
            //for (int i = 0; i < stepResults.Count; i++)
            //// JObject row in stepResults.Children<JObject>()
            //{
            //    DataRow dataRow = dataTable.NewRow();
            //    //dataRow[0] = stepsList[i].Value<string>("Name");
            //    //dataRow[1] = stepsList[i].Value<string>("Type");
            //    //dataRow[2] = FormatChannelNames(stepsList[i].Value<JArray>("InputChannelNames"));
            //    //dataRow[3] = FormatChannelNames(stepsList[i].Value<JArray>("OutputChannelNames"));

            //    if (stepResults[i].Value<Boolean>("Evaluated"))
            //    {
            //        if (stepResults[i].Value<Boolean>("Verdict"))
            //        { dataRow[4] = "Pass"; }
            //        else
            //        { dataRow[4] = "Fail"; }

            //        dataRow[5] = stepResults[i].Value<Double>("Margin").ToString();
            //        dataRow[6] = stepResults[i].Value<string>("Limit");
            //        dataRow[7] = stepResults[i].Value<string>("Max/Min");
            //    }
            //    dataTable.Rows.Add(dataRow);
            //}
            //获取所有的曲线
            // Send MemoryList.GetAllNames command and wait for result.
            //if (!tcp.WriteCommandAndGetResponse("MemoryList.GetAllNames").Contains("False"))
            //{
            //    // Command completed successfully
            //    JArray jarray = tcp.json.returnData.Value<JArray>("Curves"); // Convert return data to dynamic objects array 
            //}
            #endregion
            return "True";
        }

        public static string GetCurve(string CurveName, out Dictionary<String, double[]> Curves)
        {

            Curves = new Dictionary<string, double[]>();
            string cmd = $"MemoryList.Get('Curve','{CurveName}')";
            string Receive = tcp.WriteCommandAndGetResponse(cmd);
            if (Receive.Contains("False") || !tcp.jToken["returnData"].Value<Boolean>("Found"))
            {
                MessageBox.Show($"获取曲线：{CurveName}\r\n失败了");
                return "Get Curve False";
            }
            string Name = tcp.jToken["returnData"]["Curve"]["Name"].ToObject<string>();
            double[] XDatas = tcp.json.returnData.Curve.Value<JArray>("XData").ToObject<double[]>();
            double[] YDatas = tcp.json.returnData.Curve.Value<JArray>("YData").ToObject<double[]>();
            double[] XData = new double[XDatas.Length];
            double[] YData = new double[YDatas.Length];
            //保留两位小数是为了节约数据库上的内存，同时也让数据更方便查看
            for (int i = 0; i < XData.Length; i++)
                XData[i] = Math.Round(XDatas[i], 2);
            for (int i = 0; i < YData.Length; i++)
                YData[i] = Math.Round(YDatas[i], 2);
            Curves["X"] = XData;
            Curves["Y"] = YData;
            return "True";



        }
        /// <summary>
        /// 获取所有曲线的名称
        /// </summary>
        /// <param name="Names"></param>
        /// <returns></returns>
        public static string GetAllNames(out string[] Names)
        {
            Names = null;
            string Receive = tcp.WriteCommandAndGetResponse("MemoryList.GetAllNames");
            if (Receive.Contains("False"))
                return Receive;

            JArray jarray = tcp.json.returnData.Value<JArray>("Curves");
            string[] Curves = jarray.ToObject<string[]>();
            List<string> ListNames = new List<string>();
            foreach (var item in Curves)
            {
                //判断名字太长存在一些其他设定的名字
                if (item.Length > 10)
                {
                    string name = item.Substring(item.Length - 10, 10);
                    if (name.Contains("corr-in") || name.Contains("corr-out") || name.Contains("eq-out"))
                        continue;
                }
                //只保留曲线的名字
                ListNames.Add(item);
            }
            Names = ListNames.ToArray();
            return "True";


        }

        /// <summary>
        /// 获取所有的曲线
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static string GetAllCurves(out Dictionary<string, Dictionary<string, object>> tokens)
        {
            string Receive;
            tokens = new Dictionary<string, Dictionary<string, object>>();
            Receive = GetAllNames(out string[] Namse);
            if (Receive.Contains("False"))
                return Receive;
            foreach (var item in Namse)
            {
                string cmd = $"MemoryList.Get('Curve','{item}')";
                Receive = tcp.WriteCommandAndGetResponse(cmd);
                if (Receive.Contains("False") || !tcp.jToken["returnData"].Value<Boolean>("Found"))
                {
                    MessageBox.Show($"获取曲线：{item}\r\n失败了，指令");
                    return "Get Curve False";
                }
                string Name = tcp.jToken["returnData"]["Curve"]["Name"].ToObject<string>();
                double[] XDatas = tcp.json.returnData.Curve.Value<JArray>("XData").ToObject<double[]>();
                double[] YDatas = tcp.json.returnData.Curve.Value<JArray>("YData").ToObject<double[]>();
                double[] XData = new double[XDatas.Length];
                double[] YData = new double[YDatas.Length];
                //保留两位小数是为了节约数据库上的内存，同时也让数据更方便查看
                for (int i = 0; i < XData.Length; i++)
                    XData[i] = Math.Round(XDatas[i], 2);
                for (int i = 0; i < YData.Length; i++)
                    YData[i] = Math.Round(YDatas[i], 2);
                tokens.Add(Name, new Dictionary<string, object>() { { "XData", XData }, { "YData", YData } });
            }
            return $"Curves {Namse.Length}";
        }
        public static string GetSequenceName()
        {
            string Receive = tcp.WriteCommandAndGetResponse("Sequence.GetName");
            if (Receive.Contains("False"))
                return Receive;
            Receive = tcp.GetCommandCompleted();
            if (Receive.Contains("False"))
                return Receive;
            return tcp.jToken["returnData"]["Value"].ToString();

        }
        public static string GetSequenceDuration()
        {
            string Receive = tcp.WriteCommandAndGetResponse("Sequence.GetDuration");
            if (Receive.Contains("False"))
                return Receive;
            Receive = tcp.GetCommandCompleted();
            if (Receive.Contains("False"))
                return Receive;
            return tcp.jToken["returnData"]["Value"].ToString();

        }





        public struct StepResultStruct
        {
            public string StepName;
            public Boolean Passed;
            public string Limit;
            public string Unit;
            public string Scale;
            public string Max_Min;
            public string Margin;
            public bool Protected;

        }
        static List<string> StepNames = new List<string>();
        static List<string> StepNamesMargin = new List<string>();

        static Dictionary<string, bool> StepResultDic = new Dictionary<string, bool>();
        static Dictionary<string, bool> StepResultMargin = new Dictionary<string, bool>();

        public static string GetStepResult(string StepName)
        {

            if (!StepResultDic.ContainsKey(StepName))
            {
                StepResultDic[StepName] = true;
                StepNames.Add(StepName);
            }
            if (!StepResultDic[StepName]) return "Not Test False";
            string cmd = $"MemoryList.Get('Result', '{StepName}')";
            string Receive = tcp.WriteCommandAndGetResponse(cmd);

            if (!Receive.Contains("False"))
            {
                StepResultDic[StepName] = false;
                JToken stepResults = (JToken)tcp.json;
                JToken returnData = stepResults["returnData"];
                if (returnData == null) return $"returnData Not To Search {false}";
                bool Found = returnData.Value<Boolean>("Found");
                if (!Found) return $"({StepName}) No Step Name  was found False";
                JToken Result = returnData["Result"];
                StepResultStruct stepResultStruct = new StepResultStruct();
                stepResultStruct.StepName = Result.Value<string>("Name");
                stepResultStruct.Passed = Result.Value<Boolean>("Passed");
                stepResultStruct.Limit = Result.Value<string>("Limit");
                stepResultStruct.Unit = Result.Value<string>("Unit");
                stepResultStruct.Scale = Result.Value<string>("Scale");
                stepResultStruct.Max_Min = Result.Value<string>("Max/Min");
                stepResultStruct.Margin = Result.Value<string>("Margin");
                stepResultStruct.Protected = Result.Value<Boolean>("Protected");

                return stepResultStruct.Passed ? "True" : $"Step {false}";
            }
            return Receive;

        }
        public static string GetStepResultMargin(string StepName, out bool Passed, out string Margin, out string Limit)
        {
            Passed = false; Margin = ""; Limit = "";
            if (!StepResultMargin.ContainsKey(StepName))
            {
                StepResultMargin[StepName] = true;
                StepNamesMargin.Add(StepName);
            }
            if (!StepResultMargin[StepName]) return "Not Test False";
            string cmd = $"MemoryList.Get('Result', '{StepName}')";
            string Receive = tcp.WriteCommandAndGetResponse(cmd);
            if (!Receive.Contains("False"))
            {
                StepResultMargin[StepName] = false;
                JToken stepResults = (JToken)tcp.json;
                JToken returnData = stepResults["returnData"];
                if (returnData == null) return $"returnData Not To Search {false}";
                bool Found = returnData.Value<Boolean>("Found");
                if (!Found) return $"({StepName}) No Step Name  was found False";
                JToken Result = returnData["Result"];
                StepResultStruct stepResultStruct = new StepResultStruct
                {
                    StepName = Result.Value<string>("Name"),
                    Passed = Passed = Result.Value<Boolean>("Passed"),
                    Margin = Margin = Result.Value<string>("Margin"),
                    Limit = Result.Value<string>("Limit"),
                    Unit = Result.Value<string>("Unit"),
                    Scale = Result.Value<string>("Scale"),
                    Max_Min = Result.Value<string>("Max/Min"),
                    Protected = Result.Value<Boolean>("Protected")
                };
                Limit = stepResultStruct.Max_Min;
                return "True";
            }
            return Receive;
        }
        public static string GetStepResultMargin(string StepName, out bool Passed, out string Margin)
        {
            return GetStepResultMargin(StepName, out Passed, out Margin, out _);
        }


        public static string SoundCheckExit()
        {
            try
            {

                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                    if (thisProc.ProcessName.Contains("SoundCheck"))
                    {
                        if (System.Windows.Forms.MessageBox.Show("是否关闭Sound Check", "Sound Check 提示", System.Windows.Forms.MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                        {
                            if (tcp.IsConnect)
                                tcp.WriteCommandAndGetResponse("SoundCheck.Exit"); // Send SoundCheck.Exit command and wait for result.
                            Thread.Sleep(3000);
                            thisProc.Kill();
                        }
                    }


                return "True";
            }
            finally
            {
                tcp.Disconnect();
            }

        }

        public static void Disconnect() => tcp.Disconnect();

        static DataTable InitializeDataTable(int numOfColumns) // Create new data table and set number of columns
        {
            // Create data table to hold data read from JSON
            DataTable dataTable = new DataTable();
            DataColumn dataColumn;

            for (int i = 0; i < numOfColumns; i++)
            {
                dataColumn = new DataColumn();
                dataTable.Columns.Add(dataColumn);
            }
            return dataTable;
        }
        static string FormatChannelNames(JArray channelNames)
        {
            return string.Join(", ", channelNames.ToObject<string[]>()); // Convert JArray to string array and created comma separated string
        }

        public static string SaveLog()
        {
            lock (SoundCheck.tcp.TcpLog)
            {
                try
                {
                    string D = "D:/MerryTestLog/Sound Check_TCP_MT8852BLog";
                    string root = "./LOG";
                    string pathName = $"Sound Check_TCP_CMD_{DateTime.Now:yyMMdd}.txt";
                    if (!Directory.Exists(D))
                        Directory.CreateDirectory(D);
                    if (!Directory.Exists(root))
                        Directory.CreateDirectory(root);
                    File.AppendAllLines($"{D}/{pathName}", SoundCheck.tcp.TcpLog.ToArray());
                    File.AppendAllLines($"{root}/{pathName}", SoundCheck.tcp.TcpLog.ToArray());
                    SoundCheck.tcp.TcpLog.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("生成TCP日记失败");
                    Task.Run(() => MessageBox.Show(ex.ToString()));
                }
            }


            return "True";
        }

        public static string CorrectCharacter(this string str)
        {
            //匹配所有要替换掉的字符和符号的正则表达式
            string strPattern = "‘|’|“|”|\'";
            //输出带有各种字符符号的字符串
            return System.Text.RegularExpressions.Regex.Replace(str, strPattern, " ");
        }




    }
}
