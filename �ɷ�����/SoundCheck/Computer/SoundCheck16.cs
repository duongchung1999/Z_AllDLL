using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Windows.Forms;
using System.Net;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Data;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace MerryDllFramework
{
    public class SoundCheck16
    {
        public static string Msg;

        private string _IPAddress = "127.0.0.1";

        private string Port = "4444";

        public static TcpClient client = new TcpClient();

        public static StreamReader STR;

        public static StreamWriter STW;

        private static dynamic json;

        private static JArray stepsList;
        public int RunCount = 0;

        public void ClearMsg()
        {
            SoundCheck16.Msg = "";
        }

        public string GetMsg()
        {
            string arg_0F_0 = SoundCheck16.Msg;
            SoundCheck16.Msg = "";
            return arg_0F_0;
        }

        public void RunSoundCheck(string SoundCheckPath)
        {
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(SoundCheckPath)).Length == 0)
            {
                Process.Start(SoundCheckPath, "");
                SoundCheck16.Msg = SoundCheckPath + " started.";
                return;
            }
            SoundCheck16.Msg = Path.GetFileName(SoundCheckPath) + " is already running.";
        }

        public void ConnectToServer()
        {
            SoundCheck16.Msg = "Connecting to SoundCheck...";
            client = new TcpClient();
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(this._IPAddress), int.Parse(this.Port));
            try
            {
                client.Connect(remoteEP);
                if (client.Connected)
                {
                    STR = new StreamReader(client.GetStream());
                    STW = new StreamWriter(client.GetStream());
                    STW.AutoFlush = true;
                    this.ReadLineFromStream();
                    this.SendCommandAndGetResponse("SoundCheck.SetFloatStrings('NaN','Infinity','-Infinity')");
                    SoundCheck16.Msg = "Connected to SoundCheck ok.";
                }
            }
            catch (Exception)
            {
                SoundCheck16.Msg = "Failed to connect to SoundCheck.";
                SoundCheck16.Msg = "Could not connect to SoundCheck because the target machine refused it." + Environment.NewLine + "Please make sure that TCP/IP server is enabled in SoundCheck Preferences dialog and try again.";
            }
        }

        public void closeServer()
        {

            if (client.Connected)
            {
                client.Close();
                SoundCheck16.Msg = "disonnect server";
            }
        }
        public void SendSN(string SN)
        {

            if (this.SendCommandAndGetResponse("SoundCheck.SetSerialNumber('" + SN + "')"))
            {
                SoundCheck16.Msg = "Serial number set ok.";
                return;
            }
            if (this.GetErrorType() == "Timeout")
            {
                SoundCheck16.Msg += "Command failed; timed out!";
            }
        }

        public void OpenSequence(string Path)
        {
            if (this.SendCommandAndGetResponse("Sequence.Open('" + Path + "')"))
            {
                if (this.GetReturnDataBoolean())
                {
                    SoundCheck16.Msg = "Sequence Opened ok. Ready to run!";
                    if (!this.SendCommandAndGetResponse("Sequence.GetStepsList"))
                    {
                        return;
                    }
                    string[] array = new string[]
                    {
                        "Step Name",
                        "Step Type",
                        "Input Channel",
                        "Output Channel"
                    };
                    DataTable dataTable = this.InitializeDataTable(array.Length);


                    stepsList = json.Value<JArray>("returnData"); // Convert return data to dynamic objects array
                    using (IEnumerator<JObject> enumerator = stepsList.Children<JObject>().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            JObject current = enumerator.Current;
                            DataRow dataRow = dataTable.NewRow();
                            dataRow[0] = current.Value<string>("Name");
                            dataRow[1] = current.Value<string>("Type");
                            dataRow[2] = this.FormatChannelNames(current.Value<JArray>("InputChannelNames"));
                            dataRow[3] = this.FormatChannelNames(current.Value<JArray>("OutputChannelNames"));
                            dataTable.Rows.Add(dataRow);
                        }
                        return;
                    }
                }
                SoundCheck16.Msg = "Sequence failed to open.";
                return;
            }
            if (this.GetErrorType() == "Timeout")
            {
                SoundCheck16.Msg = "Sequence failed to open. Command timed out!";
            }
        }

        public void RunSequence()
        {
            SoundCheck16.Msg = "Running sequence ...";
            if (this.Sendstart("Sequence.Run"))
            {
                SoundCheck16.Msg = "Running sequence ok.";
                return;
            }
            SoundCheck16.Msg = "Running sequence fail";
        }

        public void ExitSoundCheck()
        {
            if (this.SendCommandAndGetResponse("SoundCheck.Exit"))
            {
                SoundCheck16.Msg = "SoundCheck exited.";
                return;
            }
            // Command did not complete successfully
            if (json.Value<string>("errorType") == "Timeout") // Check if command timed out
            {

                SoundCheck16.Msg = "Command failed; timed out!";
            }
        }

        public bool GetResult()
        {
            if (this.GetResponseJSON())
            {
                RunCount--;


                if (this.GetCommandCompleted())
                {
                    return GetReturnDataBoolean("Pass?");
                }
            }
            if (!client.Connected)
            {
                SoundCheck16.Msg = "You are not connected to SoundCheck." + Environment.NewLine + "Please connect and try again!";
            }
            return false;
        }

        public bool GetResponseJSON()
        {
            string text = this.ReadLineFromStream();
            if (text != null)
            {
                json = JToken.Parse(text);
                return true;
            }
            return false;
        }

        private string ReadLineFromStream()
        {
            string result;
            try
            {
                string text = null;
                while (text == null)
                {
                    text = STR.ReadLine();
                    if (text == null)
                    {
                        Thread.Sleep(100);
                    }
                }
                result = text;
            }
            catch (Exception ex)
            {
                if (client.Connected)
                {
                    SoundCheck16.Msg = ex.Message.ToString();
                }
                result = null;
            }
            return result;
        }

        public bool SendCommandAndGetResponse(string SCCommand)
        {
            if (!client.Connected)
            {
                SoundCheck16.Msg = "You are not connected to SoundCheck." + Environment.NewLine + "Please connect and try again!";
                return false;
            }
            STR.DiscardBufferedData();
            STW.WriteLine(SCCommand + "\r\n");
            if (this.GetResponseJSON())
            {
                return this.GetCommandCompleted();
            }
            if (!client.Connected)
            {
                SoundCheck16.Msg = "You are not connected to SoundCheck." + Environment.NewLine + "Please connect and try again!";
            }
            return false;
        }

        public void SequenceGetStepsList()
        {

            if (SendCommandAndGetResponse("Sequence.GetStepsList"))
            {
                stepsList = json.Value<JArray>("returnData"); // Convert return data to dynamic objects array
            }
        }

        private bool Sendstart(string SCCommand)
        {
            if (!client.Connected)
            {
                SoundCheck16.Msg = "You are not connected to SoundCheck." + Environment.NewLine + "Please connect and try again!";
                return false;
            }
            STW.WriteLine(SCCommand + "\r\n");
            return true;
        }

        private string FormatChannelNames(JArray channelNames)
        {
            return string.Join(", ", channelNames.ToObject<string[]>());
        }

        private DataTable InitializeDataTable(int numOfColumns)
        {
            DataTable dataTable = new DataTable();
            for (int i = 0; i < numOfColumns; i++)
            {
                DataColumn column = new DataColumn();
                dataTable.Columns.Add(column);
            }
            return dataTable;
        }

        private bool GetCommandCompleted()
        {
            return json.Value<Boolean>("cmdCompleted");
        }

        private string GetReturnType()
        {
            return json.Value<string>("returnType");
        }

        private string GetErrorType()
        {
            return json.Value<string>("errorType");
        }

        private string GetErrorDescription()
        {
            return json.Value<string>("errorDescription");
        }

        public bool GetReturnDataBoolean()
        {
            return json.returnData.Value<Boolean>("Value");

        }

        public bool GetReturnDataBoolean(string dataName)
        {
            return json.returnData.Value<Boolean>(dataName);
        }     // Overload: Boolean data by name, where the return data contains more than just the boolean field

        private int GetReturnDataInteger()
        {
            return json.returnData.Value<Int32>("Value");
        }

        private int GetReturnDataInteger(string dataName)
        {

            return json.returnData.Value<Int32>(dataName);
        }

        private double GetReturnDataDouble()
        {

            return json.returnData.Value<Double>("Value");
        }

        private double GetReturnDataDouble(string dataName)
        {
            return json.returnData.Value<Double>(dataName);
        }

        private string GetReturnDataString()
        {
            return json.returnData.Value<String>("Value");
        }

        private string[] GetReturnDataStringArray()
        {
            return json.returnData.Value<JArray>("Value").ToObject<string[]>();
            //return arg_153_0(arg_153_1, arg_14E_0(arg_14E_1, arg_149_0(arg_149_1, SoundCheck16.<>o__33.<>p__0.Target(SoundCheck16.<>o__33.<>p__0, this.json), "Value")));
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
        public string GetStepResult(string StepName)
        {


            string cmd = $"MemoryList.Get('Result', '{StepName}')";

            if (SendCommandAndGetResponse(cmd))
            {
                JToken stepResults = (JToken)json;

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
            return "Send CMD False";

            //JArray stepResults = json.returnData.Value<JArray>("StepResults"); // Convert return data to dynamic objects array
            //Dictionary<string, StepResultStruct> StepList = new Dictionary<string, StepResultStruct>();
            //for (int i = 0; i < stepResults.Count; i++)
            //{
            //    StepResultStruct stepResultStruct = new StepResultStruct();
            //    stepResultStruct.StepName = stepsList[i].Value<string>("Name");
            //    stepResultStruct.StepType = stepsList[i].Value<string>("Type");
            //    stepResultStruct.InputChannel = FormatChannelNames(stepsList[i].Value<JArray>("InputChannelNames"));
            //    stepResultStruct.OutputChannel = FormatChannelNames(stepsList[i].Value<JArray>("OutputChannelNames"));
            //    stepResultStruct.Verdict = stepResults[i].Value<Boolean>("Verdict");
            //    stepResultStruct.Margin = stepResults[i].Value<Double>("Margin").ToString();
            //    stepResultStruct.Limit = stepResults[i].Value<string>("Limit");
            //    stepResultStruct.Max_Min = stepResults[i].Value<string>("Max/Min");
            //    StepList[stepResultStruct.StepName] = stepResultStruct;
            //}
            //int index = 0;
            //foreach (var item in StepList)
            //{
            //    index += 1;
            //    if (item.Key == StepName)
            //    {
            //        string resultStr = StepList[StepName].Verdict ? "" : " False";
            //        return $"Step {index} : { StepList[StepName].Margin}{resultStr}";
            //    }
            //}


            //return "No Step Name was found False";
        }

    }
}