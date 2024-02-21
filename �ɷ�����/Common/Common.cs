using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法

        bool MessageBox(string msg)
        {
            Common.Box boxs = new Common.Box(msg);
            boxs.ShowDialog();
            var result = boxs.DialogResult;//先关闭会获取不到值
            return result == DialogResult.Yes;
        }

        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        Dictionary<string, object> Config;
        public object Interface(Dictionary<string, object> Config) => this.Config = Config;


        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Common";
            string dllfunction = "Dll功能说明 ：弹出窗体，串口调试";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllVersion = "当前Dll版本：22.9.23";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo2 = "21.8.14.0：2021/8/14：改动串口错误返回值";
            string dllChangeInfo3 = "22.8.4：修复弹窗不置顶问题";
            string dllChangeInfo4 = "22.9.23：对串口下指令可以调节波特率";

            string[] info = { dllname, dllfunction, dllHistoryVersion, 
                dllVersion, dllChangeInfo,dllChangeInfo2,dllChangeInfo3,dllChangeInfo4
            };


            return info;
        }
        public string Run(object[] Command)
        {
            List<string> cmd = new List<string>();
            foreach (object item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>))
                    OnceConfig = (Dictionary<string, object>)item;
                if (type == typeof(string))
                {
                    cmd.AddRange(item.ToString().Split('&'));
                    for (int i = 0; i < cmd.Count; i++) cmd[i] = cmd[i].Split('=')[1].Trim();
                }
            }
            switch (cmd[1].ToLower())
            {
                case "forms": return MessageBox(cmd[2]).ToString();
                case "sleep": Thread.Sleep(int.TryParse(cmd[2], out int i) ? i : 1000); return true.ToString();
                case "send_read":
                    return Send_Read
                        (OnceConfig.ContainsKey("ComPort") ? (string)OnceConfig["ComPort"] : cmd[2],
                        cmd[3],
                      cmd.Count >= 5 ? int.Parse(cmd[4]):9600
                       );

                case "powerfrequency":
                    string[] str = new string[7];
                    str[2] = cmd[2];
                    str[3] = cmd[3];
                    str[4] = cmd[4];
                    if (cmd.Count < 6)
                    {
                        str[5] = cmd[5];
                    }
                    else
                    {
                        str[5] = "500";
                    }
                    return Power(cmd[2], cmd[3], cmd[4], cmd[5]);
                case "lock": return LockThread_.Lock().ToString();
                case "unlock": return LockThread_.UnLock().ToString();
                case "awaitthread": return AwaitThread();
                case "locknumberthread":
                    return LockThread_.LockNumberThread(cmd[2]).ToString();
                case "unlocknumberthread":
                    return LockThread_.UnLockNumberThread(cmd[2]).ToString();

                default: return "Connend Error False";

            }
        }
        #endregion

        SerialPort Comport = new SerialPort();
        string Send_Read(string ComPort, string Command, int baudRate)
        {

            try
            {
                Comport.PortName = ComPort;
                Comport.BaudRate = baudRate;
                Command = Command.Replace(@"\r", "\r").Replace(@"\n", "\n");
                Comport.Open();
                if (Command.Contains('?'))
                {
                    Comport.WriteLine(Command);
                    Thread.Sleep(500);
                    return Comport.ReadLine();
                }
                else
                {
                    Comport.WriteLine(Command);
                    return true.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"{ComPort} {ex.Message} Error";
            }
            finally
            {
                if (Comport.IsOpen) Comport.Close();
                Comport.Dispose();
            }
        }
        string Power(string ComPort, string Command, string Signal, string Sleep)
        {

            using (SerialPort Comport = new SerialPort(ComPort))
            {
                try
                {
                    bool signal = bool.TryParse(Signal, out bool result) && result;
                    Comport.Open();
                    switch (Command)
                    {
                        //  Com口 4脚设置是否高电频
                        case "DtrEnable": Comport.DtrEnable = signal; break;
                        //  Com口 7脚设置是否高电频
                        case "RtsEnable": Comport.RtsEnable = signal; break;
                        default: return "指令错误,请查看Command.xlsx False";
                    }
                    return true.ToString();
                }
                catch (Exception ex)
                {
                    return $"{ex.Message} Error";
                }
                finally
                {
                    Thread.Sleep(int.Parse(Sleep));
                    if (Comport.IsOpen) Comport.Close();
                }
            }
        }
        string AwaitThread()
        {
            lock (obj_lock)
            {
                if (TestFlags == null)
                {
                    foreach (KeyValuePair<int, Dictionary<string, string>> item in (Dictionary<int, Dictionary<string, string>>)Config["TestControl"])
                    {
                        testID.Add(item.Key);
                    }

                    TestFlags = new List<int>(new int[500]);
                }

                if (first)
                {
                    foreach (int item2 in testID)
                    {
                        TestFlags[item2] = 1;
                    }

                    first = false;
                }
            }

            do
            {
                TestFlags[Convert.ToInt32(OnceConfig["TestID"])] = 0;
                Thread.Sleep(100);
            }
            while (TestFlags.Contains(1));
            first = true;
            return true.ToString();
        }


        private static object obj_lock = new object();
        private static List<int> TestFlags = null;
        private static List<int> testID = new List<int>();
        private static bool first = true;

    }
    internal static class LockThread_
    {
        private static bool Flag;

        public static bool Lock()
        {
            while (Flag)
            {
                Thread.Sleep(100);
            }

            Flag = true;
            return true;
        }

        public static bool UnLock()
        {
            Flag = false;
            return true;
        }
        static Dictionary<string, bool> lockbool = new Dictionary<string, bool>();
        static Dictionary<string, object> lockObj = new Dictionary<string, object>();

        public static bool LockNumberThread(string number)
        {
            if (!lockObj.ContainsKey(number))
            {
                lockObj[number] = new object();
                lockbool[number] = false;
            }
            lock (lockObj[number])
            {

                while (lockbool[number])
                {
                    Thread.Sleep(100);
                }
                lockbool[number] = true;
            }
            return true;
        }

        public static bool UnLockNumberThread(string number)
        {
            lockbool[number] = false;
            return true;
        }
    }
}

