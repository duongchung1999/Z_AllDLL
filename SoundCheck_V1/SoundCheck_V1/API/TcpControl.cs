using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundCheck_V1.API
{
    internal class TcpControl
    {
        public TcpControl(string ipv4, string port)
        {
            Ipv4 = ipv4;
            Port = port;
        }
        ~TcpControl()
        {
            Disconnect();
        }

        public static TcpClient client = new TcpClient();
        public static StreamReader STR;
        public static StreamWriter STW;
        public JToken jToken; // JSON array to store steps list from Sequence.GetStepsList
        public JArray jArray
        {
            get
            {
                return json.Value<JArray>("returnData");
            }
        }
        public dynamic json; // Dynamic object for working with JSON response from SoundCheck

        string Ipv4 = "";
        string Port = "";
        public List<string> TcpLog = new List<string>();
        public bool IsConnect
        {
            get
            {
                if (!client.Connected)
                {
                    Disconnect();
                    json = "";
                    jToken = "";
                    return false;
                }
                return client.Connected;
            }
        }

        public bool Connect()
        {
            if (IsConnect) return true;

            client = new TcpClient();
            client.SendTimeout = 60000;
            client.ReceiveTimeout = 60000;
            IPEndPoint IP_End = new IPEndPoint(IPAddress.Parse(Ipv4), int.Parse(Port));
            try
            {
                client.Connect(IP_End);
                STR = new StreamReader(client.GetStream());
                STW = new StreamWriter(client.GetStream());
                STW.AutoFlush = true;
                WriteCmdCount++;
                return true;
            }
            catch (Exception ex)
            {

                MessageBox.Show("建立连接失败" + Environment.NewLine +
                    "Could not connect to SoundCheck because the target machine refused it." + Environment.NewLine +
                    "Please make sure that TCP/IP server is enabled in SoundCheck Preferences dialog and try again.");
                Disconnect();
                return false;
            }


        }

        public string WriteCommandAndGetResponse(string Command)
        {

            string Receive = WriteLine(Command);
            if (Receive.Contains("False"))
                return Receive;
            return ReadCommandCompleted();
        }
        public int WriteCmdCount = 0;
        public string WriteLine(string Command)
        {
            if (!IsConnect) return "Not Connect False";
            TcpLog.Add($"{DateTime.Now} | Write | {Command}");
            ReadLine();
            cmd_Lock();
            STW.WriteLine(Command + "\r\n"); //Send command to server, with CRLF termination
            WriteCmdCount++;
            return Command;
        }

        public string ReadCommandCompleted()
        {
            string Receive = ReadLine();
            if (Receive.Contains("False"))
                return Receive;
            json = jToken = JValue.Parse(Receive);
            return GetCommandCompleted();
        }
        public string ReadLine()
        {
            try
            {
                string receive = "Not ReadLine False";
                for (; WriteCmdCount > 0; WriteCmdCount--)
                {
                    if (!IsConnect) return "Not Connect False";
                    receive = STR.ReadLine(); //Read line from stream
                    TcpLog.Add($"{DateTime.Now} | Read | {receive}");
                }

                return receive;

            }
            finally
            {
                cmd_Ulock();
            }


        }

        public void Disconnect()
        {
            WriteCmdCount = 0;
            client?.Close();
            STR?.Dispose();
            STW?.Dispose();
            client = new TcpClient();
        }


        // These methods help parse the response from SoundCheck 

        // The following methods are common for every command's response
        public string GetCommandCompleted() { return json.Value<Boolean>("cmdCompleted") ? "True" : "Send Command False"; }      // Whether or not the command completed
        public string GetReturnType() { return json.Value<string>("returnType"); }             // Get the data type of the data returned by the command
        public string GetErrorType() { return json.Value<string>("errorType"); }               // If command did not complete, what error type caused the failure
        public string GetErrorDescription() { return json.Value<string>("errorDescription"); } // Error description

        // The following methods help get command specific data from the response JSON

        // Boolean Data
        public bool GetReturnDataBoolean() { return json.returnData.Value<Boolean>("Value"); }     // Boolean Data
        public bool GetReturnDataBoolean(string dataName) { return json.returnData.Value<Boolean>(dataName); }     // Overload: Boolean data by name, where the return data contains more than just the boolean field

        // Integer Data
        public int GetReturnDataInteger() { return json.returnData.Value<Int32>("Value"); }        // Integer Data
        public int GetReturnDataInteger(string dataName) { return json.returnData.Value<Int32>(dataName); }     // Overload: Integer data by name, where the return data contains more than just the integer field

        // Double Date
        public double GetReturnDataDouble() { return json.returnData.Value<Double>("Value"); }     // Double Data
        public double GetReturnDataDouble(string dataName) { return json.returnData.Value<Double>(dataName); }     // Overload: Double data by name, where the return data contains more than just the double field

        // String Data
        public string GetReturnDataString() { return json.returnData.Value<String>("Value"); }     // String Data
        public string[] GetReturnDataStringArray() { return json.returnData.Value<JArray>("Value").ToObject<string[]>(); }     // String Array Data


        object lock_obj = new object();
        bool LockFlag = false;
        void cmd_Lock()
        {
            lock (lock_obj)
            {
                while (LockFlag)
                {
                    Thread.Sleep(5);
                }
            }
            LockFlag = true;
        }
        void cmd_Ulock()
        {
            LockFlag = false;
        }
    }
}
