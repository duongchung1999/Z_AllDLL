using MerryTest.testitem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MerryDllFramework
{
    /// <summary dllName="CRY574">
    /// CRY574 蓝牙适配器
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        #region MyRegion

        Command cmd = new Command();
        GetHandle getH = new GetHandle();
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：CRY574";
            string dllfunction = "Dll功能说明 ：蓝牙适配器";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllVersion = "当前Dll版本：23.7.12.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.6.15.5：初版开发";
            string dllChangeInfo2 = "23.7.12.0：循环2下进入HFP";

            string[] info = { dllname, dllfunction,
                dllHistoryVersion,
                dllVersion, dllChangeInfo,
                dllChangeInfo1 };
            return info;
        }
        Dictionary<string, object> Config = new Dictionary<string, object>();
        public object Interface(Dictionary<string, object> keys) => Config = keys;


        #endregion

        public string Run(object[] Command)
        {
            try
            {
                SplitCMD(Command, out string[] CMD);
                switch (CMD[1])
                {
                    case "Connect": return Connect(bool.Parse(CMD[2]), int.Parse(CMD[3]));
                    case "EnterHFP": return EnterHFP().ToString();
                    case "EnterA2DP": return EnterA2DP().ToString();
                    case "Disconnect": return Disconnect().ToString();


                    case "ComPortConnect": return ComPortConnect((CMD[2]), CMD[3]);
                    case "ComPortEnterA2DP": return ComPortEnterA2DP().ToString();
                    case "ComPortEnterHFP": return ComPortEnterHFP().ToString();
                    case "ComPortGetBattery": return ComPortGetBattery();
                    case "ComPortDisconnect": return ComPortDisconnect().ToString();

                    default: return $"Command Error {false}";
                }
            }

            finally
            {
                getH.CloseHandle(ref getH.Handle);
            }

        }
        void SplitCMD(object[] Command, out string[] CMD)
        {
            string TestName = "";
            List<string> listCMD = new List<string>();

            foreach (var item in Command)
            {
                Type type = item.GetType();
                if (type.ToString().Contains("TestitemEntity"))
                {
                    PropertyInfo property = type.GetProperty("测试项目");
                    TestName = property.GetValue(item, null).ToString();
                }
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            CMD = listCMD.ToArray();
        }

        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 分割线 HID区 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string Cut_OffRule()
        {
            return "True";
        }

        /// <summary isPublicTestItem="false">
        /// HID 建立连接 标准品默认盲连
        /// </summary>
        /// <param name="ManualFlag" options="True,False">True 为指定连接地址在Config["BitAddress"] 或 False 盲连</param>
        /// <param name="TimeOut">配对超时 /S  常规“13”</param>
        /// <returns></returns>
        public string Connect(bool ManualFlag, int TimeOut)
        {
            getH.gethandle("5750", "0483", "mi_03");
            //盲连
            if (!ManualFlag || Config["SN"].ToString().Contains("TE_BZP"))
            {

                bool isInquiryFlag = false;
                string BDAddress = "";
                try
                {
                    if (!this.cmd.StartReadHIDValue(getH.Path, 65))
                        return "Connect CRT574 False";
                    //搜索正在配对设备
                    for (int i = 0; i < 2; i++)
                    {
                        if (!this.cmd.WriteSend("00 69 6e 71 75 69 72 79 20 35 0d 0a", 65, getH.Handle))
                            return $"Send CMD {getH.Handle} False";
                        for (int j = 0; j < 5; j++)
                        {
                            string[] value = this.cmd.CMDlog.ToArray();
                            foreach (var item in value)
                            {
                                if (item.Contains("INQUIRY_PARTIAL"))
                                {
                                    BDAddress = item.Substring(16, 17);
                                    isInquiryFlag = true;
                                    break;
                                }
                            }
                            if (isInquiryFlag)
                                break;
                            Thread.Sleep(1000);

                        }
                        if (isInquiryFlag)
                            break;

                    }

                }
                finally
                {
                    this.cmd.StopReadHIDValue();

                }

                if (!isInquiryFlag)
                    return "Inquiry Partial False";
                List<byte> bytes = new List<byte>();



                return SpecifiedConnect(BDAddress, TimeOut);

            }
            //指定配对
            else
            {
                string CryBDAddress = Config["BitAddress"].ToString();
                string BD = $"{CryBDAddress.Substring(0, 2)}:{CryBDAddress.Substring(2, 2)}:{CryBDAddress.Substring(4, 2)}:{CryBDAddress.Substring(6, 2)}:{CryBDAddress.Substring(8, 2)}:{CryBDAddress.Substring(10, 2)}".ToUpper();//"f4:b6:88:5d:a2:39";

                return SpecifiedConnect(BD, TimeOut);
            }


        }
        string SpecifiedConnect(string BD, int TimeOut)
        {
            #region 指定配对
            if (!this.cmd.StartReadHIDValue(getH.Path, 65))
                return "Connect CRT574 False";
            string cmd1 = "ic";
            string cmd2 = "pair " + BD;
            string cmd3 = "call " + BD + " 111e hfp-ag";
            string cmd4 = "call " + BD + " 19 a2dp";
            string cmd5 = "call " + BD + " 17 avrcp";
            string[] ConnectCMD = new string[] { cmd1, cmd2, cmd3, cmd4, cmd5 };
            DateTime StartTestTime = DateTime.Now;//记录时间
            bool status = false;
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    if (!Connects(ConnectCMD))
                        return $"Send Cry574 CMD {getH.Handle} False";
                    for (int j = 0; j < 6; j++)
                    {
                        string[] strs = this.cmd.CMDlog.ToArray();
                        foreach (var item in strs)
                        {
                            if (item.Contains("OK") && item.Contains(BD))
                            {
                                status = true;
                                break;
                            }
                        }
                        if (status)
                            break;
                        if ((DateTime.Now - StartTestTime).TotalSeconds >= TimeOut)
                        {
                            return $"Connect Timeout {false}";
                        }
                        Thread.Sleep(940);
                    }
                    if (status)
                        break;
                }

            }
            finally
            {
                this.cmd.StopReadHIDValue();
            }
            string cmd7 = "@0 +XAPL=iPhone ,7";
            string cmd8 = "@0 ok.@0 ok.@0ok.@0 ok.@0 ok.@0 ok";
            string cmdh = $"name {BD}";
            string cmd9 = "rssi 1";
            string cmd10 = "@0 +vgs = 15";
            string cmd12 = "@2 avrcp pdu 50 100";
            string cmd13 = "@2 avrcp pdu 31 d";
            string[] cmd = new string[] { cmd7, cmd8, cmdh, cmd9, cmd10, cmd12, cmd13 };
            for (int i = 0; i < cmd.Length; i++)
            {
                byte[] bytecmd = System.Text.Encoding.ASCII.GetBytes("." + cmd[i] + "..");
                for (int j = 0; j < bytecmd.Length; j++)
                {
                    if (bytecmd[j] == 0x2e)
                        bytecmd[j] = 0x0d;
                }
                bytecmd[0] = 0x00;
                bytecmd[bytecmd.Length - 2] = 0X0d;
                bytecmd[bytecmd.Length - 1] = 0X0a;

                //将上面得到的byte[] 10进制转16进制得到str
                string str = "";
                foreach (var bycmd in bytecmd)
                {
                    str += Convert.ToString(bycmd, 16) + " ";
                }
                Thread.Sleep(100);
                if (!this.cmd.WriteSend(str.Trim(), 65, getH.Handle))
                {
                    return false.ToString();
                }
            }
            return status ? BD.Replace(":", "") : $"Connect Timeout {false}";
            #endregion
        }
        bool Connects(string[] cmds)
        {
            for (int i = 0; i < cmds.Length; i++)
            {

                string _cmd = "." + cmds[i] + "..";
                byte[] bytecmd = System.Text.Encoding.ASCII.GetBytes(_cmd);
                for (int j = 0; j < bytecmd.Length; j++)
                {
                    if (bytecmd[j] == 0x2e) bytecmd[j] = 0x0d;
                }
                bytecmd[0] = 0x00;
                bytecmd[bytecmd.Length - 2] = 0X0d;
                bytecmd[bytecmd.Length - 1] = 0X0a;

                //将上面得到的byte[] 10进制转16进制得到str
                string str = "";
                foreach (var bycmd in bytecmd)
                {
                    str += Convert.ToString(bycmd, 16) + " ";
                }
                Thread.Sleep(50);
                if (!this.cmd.WriteSend(str.Trim(), 65, getH.Handle))
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary isPublicTestItem="false">
        /// HID 进入A2DP
        /// </summary>
        /// <returns></returns>
        public bool EnterA2DP()
        {
            getH.gethandle("5750", "0483", "mi_03");
            try
            {
                if (!this.cmd.StartReadHIDValue(getH.Path, 65))
                    return false;
                string[] cmds = new string[] {
                    "00 63 6c 6f 73 65 20 34 0d 0a",
                    "00 40 31 20 61 32 64 70 20 73 74 72 65 61 6d 69 6e 67 20 73 74 61 72 74 20 31 0d 0a",
                    "00 40 32 20 61 76 72 63 70 20 6e 66 79 20 63 68 61 6e 67 65 64 20 31 20 31 20 31 0d 0a"
                    };
                foreach (var item in cmds)
                {
                    if (!this.cmd.WriteSend(item, 65, getH.Handle)) return false;
                    Thread.Sleep(250);
                }
                Thread.Sleep(1000);
                return true;

            }
            finally
            {
                this.cmd.StopReadHIDValue();
            }

        }
        /// <summary isPublicTestItem="false">
        /// HID 进入HFP
        /// </summary>
        /// <returns></returns>
        public bool EnterHFP()
        {

            getH.gethandle("5750", "0483", "mi_03");
            try
            {
                if (!this.cmd.StartReadHIDValue(getH.Path, 65))
                    return false;
                string[] cmds = new string[] {
                    "00 40 32 20 61 76 72 63 70 20 6e 66 79 20 63 68 61 6e 67 65 64 20 31 20 31 20 32 0d 0a",
                    "00 40 31 20 61 32 64 70 20 73 74 72 65 61 6d 69 6e 67 20 73 74 6f 70 20 20 31 0d 0a",
                    "00 73 63 6f 20 6f 70 65 6e 20 30 0d 0a"
                    };
                foreach (var item in cmds)
                {
                    if (!this.cmd.WriteSend(item, 65, getH.Handle)) return false;
                    Thread.Sleep(250);
                }
                Thread.Sleep(1000);
                return true;

            }
            finally
            {
                this.cmd.StopReadHIDValue();
            }

        }
        /// <summary isPublicTestItem="false">
        /// HID 断开连接
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            getH.gethandle("5750", "0483", "mi_03");
            string cmd1 = "close 0. close 1. close 2. close 3. close 4. close 5.";
            string cmd2 = ".close 0 .close 1 .close 2 .close 3 .close 4 .close 5";
            string[] cmd = new string[] { cmd1, cmd2 };
            foreach (string item in cmd)
            {
                byte[] bytecmd = System.Text.Encoding.ASCII.GetBytes(item);
                for (int j = 0; j < bytecmd.Length; j++)
                {
                    if (bytecmd[j] == 0x2e) bytecmd[j] = 0x0d;
                }
                bytecmd[0] = 0x00;
                bytecmd[bytecmd.Length - 2] = 0X0d;
                bytecmd[bytecmd.Length - 1] = 0X0a;

                //将上面得到的byte[] 10进制转16进制得到str
                string str = "";
                foreach (var bycmd in bytecmd)
                {
                    str += Convert.ToString(bycmd, 16) + " ";
                }
                if (!this.cmd.WriteSend(str.Trim(), 65, getH.Handle)) return false;
            }
            return true;
        }







        static SerialPort CRY574Port;

        /// <summary isPublicTestItem="true">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 分割线 首选串口区 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <returns></returns>
        public string _Cut_OffRule()
        {
            return "True";
        }
        string Read574Port(int TimeOut = 2000)
        {
            TimeOut /= 100;
            for (int i = 0; i < TimeOut; i++)
            {
                if (CRY574Port.BytesToRead > 0)
                {
                    byte[] readByte = new byte[CRY574Port.BytesToRead];
                    CRY574Port.Read(readByte, 0, readByte.Length);
                    string readStr = Encoding.ASCII.GetString(readByte);
                    Console.WriteLine(readStr);
                    return readStr;
                }
                Thread.Sleep(100);
            }
            return "TimeOut";
        }
        string DllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        bool onceStart = false;
        /// <summary isPublicTestItem="true">
        /// 串口 CRT574 蓝牙配对
        /// </summary>
        /// <param name="ManualFlag" options="True,False,TE_Box">True 为指定连接地址在Config["BitAddress"] 或 False 盲连</param>
        /// <param name="ComPortName">CRY574的虚拟串口 常规 “COM201”</param>
        /// <returns></returns>
        public string ComPortConnect(string ManualFlag, string ComPortName)
        {
            try
            {
                bool CheckProcessName(string Namse)
                {
                    if (!onceStart)
                    {
                        foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                            if (thisProc.ProcessName.Contains(Namse))
                            {
                                thisProc.Kill();
                                break;
                            }
                        onceStart = true;
                        Thread.Sleep(1000);
                    }
                    foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                        if (thisProc.ProcessName.Contains(Namse)) return true;
                    return false;
                }
                if (!CheckProcessName("CRY574PRO"))
                {
                    Process.Start($"{DllPath}\\CRY574Pro(20190213-1.0.25)\\CRY574PRO.exe");
                    Thread.Sleep(3000);
                }
                if (CRY574Port == null)
                {
                    CRY574Port = new SerialPort() { PortName = ComPortName };
                    Thread.Sleep(50);
                }
                if (!CRY574Port.IsOpen) CRY574Port.Open();
            }
            catch (Exception ex)
            {
                CRY574Port.Dispose();
                CRY574Port = null;
                return $"{ex.Message} False";
            }
            Thread.Sleep(5);
            string readStr = "";
            string ConnectCMD;
            string BDAddress = "";
            bool TE_BZP = Config["SN"].ToString().Contains("TE_BZP");
            if (ManualFlag.Contains("False") || TE_BZP)
            {
                ConnectCMD = "AUTOCONNECT";

            }
            else
            {
                BDAddress = Config["BitAddress"].ToString();
                if (BDAddress.Length != 12) return $"{BDAddress} BD Length !=12 False";
                ConnectCMD = $"MACCONNECT:{BDAddress}";
            }
            if (TE_BZP && ManualFlag.Contains("TE_Box"))
            {
                MessageBoxs.BarCodeBox("请扫描BD Address", 12, out BDAddress);
                if (BDAddress.Length != 12) return $"{BDAddress} BD Length !=12 False";
                ConnectCMD = $"MACCONNECT:{BDAddress}";

            }

            for (int i = 0; i < 2; i++)
            {

                CRY574Port.WriteLine(ConnectCMD);
                Thread.Sleep(50);
                readStr = Read574Port(10000);
                if (readStr.Contains("OK"))
                {
                    return true.ToString();

                }

            }

            return $"{readStr} False";
        }
        /// <summary isPublicTestItem="true">
        /// 串口 进入A2DP
        /// </summary>
        /// <returns></returns>
        public string ComPortEnterA2DP()
        {
            string readStr = $"Not Send CMD {false}";
            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(5);
                CRY574Port.WriteLine($"A2DP");
                readStr = Read574Port();

                if (readStr.Contains("OK")) return "True";
                Thread.Sleep(200);
            }
            return $"{readStr} {false}";
        }
        /// <summary isPublicTestItem="true">
        /// 串口 进入HFP
        /// </summary>
        /// <returns></returns>
        public string ComPortEnterHFP()
        {
            Thread.Sleep(5);
            string readStr = $"Not Send CMD {false}";
            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(5);
                CRY574Port.WriteLine($"HFP");
                readStr = Read574Port();
                if (readStr.Contains("OK")) return "True";
                Thread.Sleep(200);
            }

            return $"{readStr} False";
        }
        /// <summary isPublicTestItem="true">
        /// 串口 获取耳机电量
        /// </summary>
        /// <returns></returns>
        public string ComPortGetBattery()
        {
            Thread.Sleep(5);
            CRY574Port.DiscardInBuffer();
            Thread.Sleep(5);
            CRY574Port.WriteLine($"BATTERY");
            string readStr = Read574Port(1000);
            if (readStr == "TimeOut")
                return "TimeOut False";
            return $"{readStr}";

        }
        /// <summary isPublicTestItem="true">
        /// 串口 断开连接
        /// </summary>
        /// <returns></returns>
        public string ComPortDisconnect()
        {

            if (CRY574Port != null && CRY574Port.IsOpen)
            {
                CRY574Port?.WriteLine($"DISCONNECT");
                Thread.Sleep(1000);
                CRY574Port?.WriteLine($"RESET");
                CRY574Port.Close();
            }
            CRY574Port?.Dispose();
            return "True";

        }
    }
}
