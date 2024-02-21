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


namespace MerryDllFramework
{
    public class SoundCheck16
    {
        public static string Msg;

        private string _IPAddress = "127.0.0.1";

        private string Port = "4444";

        public  TcpClient client = new TcpClient();

        public StreamReader STR;

        public StreamWriter STW;
        public int i = 0;

        private dynamic json;

        private JArray stepsList;
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
            this.client = new TcpClient();
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(this._IPAddress), int.Parse(this.Port));
            try
            {
                this.client.Connect(remoteEP);
                if (this.client.Connected)
                {
                    this.STR = new StreamReader(this.client.GetStream());
                    this.STW = new StreamWriter(this.client.GetStream());
                    this.STW.AutoFlush = true;
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

            if (this.client.Connected)
            {
                this.client.Close();
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
                    using (IEnumerator<JObject> enumerator = this.stepsList.Children<JObject>().GetEnumerator())
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
            if (!this.client.Connected)
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
                this.json = JToken.Parse(text);
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
                    text = this.STR.ReadLine();
                    if (text == null)
                    {
                        Thread.Sleep(100);
                    }
                }
                result = text;
            }
            catch (Exception ex)
            {
                if (this.client.Connected)
                {
                    SoundCheck16.Msg = ex.Message.ToString();
                }
                result = null;
            }
            return result;
        }

        private bool SendCommandAndGetResponse(string SCCommand)
        {
            if (!this.client.Connected)
            {
                SoundCheck16.Msg = "You are not connected to SoundCheck." + Environment.NewLine + "Please connect and try again!";
                return false;
            }
            this.STR.DiscardBufferedData();
            this.STW.WriteLine(SCCommand + "\r\n");
            if (this.GetResponseJSON())
            {
                return this.GetCommandCompleted();
            }
            if (!this.client.Connected)
            {
                SoundCheck16.Msg = "You are not connected to SoundCheck." + Environment.NewLine + "Please connect and try again!";
            }
            return false;
        }
        private bool Sendstart(string SCCommand)
        {
            if (!this.client.Connected)
            {
                SoundCheck16.Msg = "You are not connected to SoundCheck." + Environment.NewLine + "Please connect and try again!";
                return false;
            }
            this.STW.WriteLine(SCCommand + "\r\n");
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

        private bool GetReturnDataBoolean()
        {
            return json.returnData.Value<Boolean>("Value");

        }

        private bool GetReturnDataBoolean(string dataName)
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
    }


}