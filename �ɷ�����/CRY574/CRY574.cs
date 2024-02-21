using MerryTest.testitem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        Command cmd = new Command();
        GetHandle getH = new GetHandle();
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：DM5D";
            string dllfunction = "Dll功能说明 ：控制打印机";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllVersion = "当前Dll版本：21.8.2.3";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "21.8.2.3：2021/8/26：btw文件自动获取，文件跟dll放一起，增加打印机打印内容";
            string[] info = { dllname, dllfunction,
                dllHistoryVersion,
                dllVersion, dllChangeInfo,
                dllChangeInfo1 };
            return info;
        }
        Dictionary<string, object> Config = new Dictionary<string, object>();
        public object Interface(Dictionary<string, object> keys) => Config = keys;
        public string Run(object[] Command)
        {
            List<string> cmd = new List<string>();
            foreach (object item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(string))
                {
                    cmd.AddRange(item.ToString().Split('&'));
                    for (int i = 0; i < cmd.Count; i++) cmd[i] = cmd[i].Split('=')[1].Trim();
                }
            }
            switch (cmd[1].ToLower())
            {
                case "Connect": return CRYEnterBluetoothAdaptation(cmd.Count <= 2 ? "15" : cmd[2]);
                case "HFP": return CRYEnterHFPmode().ToString();
                case "A2DP": return CRYEnterA2DPmode().ToString();
                case "DisConnect": return CRY574Disconnect().ToString();
                default: return "Command Error Fase";
            }
        }
        private string CRYEnterBluetoothAdaptation(string count = "")
        {
            CRY574Disconnect();
            getH.gethandle("5750", "0483", "mi_03");
            string BD_Address = Config["_TestValue"].ToString();
            if (BD_Address.Length != 12) return "BDAddress Length!=12 False";
            Thread.Sleep(1500);
            string BD = BD_Address;//"f4:b6:88:5d:a2:39";
            string cmd1 = "ic";
            string cmd2 = "pair " + BD;
            string cmd3 = "call " + BD + " 111e hfp-ag";
            string cmd4 = "call " + BD + " 19 a2dp";
            string cmd5 = "call " + BD + " 17 avrcp";
            string cmd6 = "@2 avrcp rsp 1";
            string cmd7 = "@0 +XAPL=iPhone ,7";
            string cmd8 = "@0 ok.@0 ok.@0ok.@0 ok.@0 ok.@0 ok";
            string cmd9 = "rssi 1";
            string cmd10 = "@0 +vgs = 15";
            string cmd11 = "@2 avrcp nfy changed 1 1 1";
            string cmd12 = "@2 avrcp pdu 50 100";
            string cmd13 = "@2 avrcp pdu 31 d";

            string[] cmd = new string[] { cmd1, cmd2, cmd3, cmd4, cmd5, cmd6, cmd7, cmd8, cmd9, cmd10, cmd11, cmd12, cmd13 };

            foreach (string item in cmd)
            {
                byte[] bytecmd = System.Text.Encoding.ASCII.GetBytes("." + item + "..");
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
                    return false.ToString();
                }

                if (item == "@0 ok.@0 ok.@0ok.@0 ok.@0 ok.@0 ok" || item == @"call 00:02:5b:00:ff:01 17 avrcp")
                {
                    for (int i = 0; i < int.Parse(count); i++)
                    {
                        string value = this.cmd.IsReturnValue(getH.Path, "0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27", 100);
                        string st = "00 43 4F 4E 4E 45 43 54";
                        string str2 = "00 41 56 52 43 50 20 32 20 53 45 54 5F 41 42 53 4F 4C 55 54 45";
                        string str3 = "00 41 56 52 43 50 20 32 20 52 45 47 49 53 54 45 52 5F 4E 4F 54";
                        string str4 = "41 56 52 43 50 20 50 41";
                        string str5 = "41 32 44 50";
                        Console.WriteLine(value);
                        if (value.Contains(st) || value.Contains(str2) || value.Contains(str3) || value.Contains(str4) || value.Contains(str5))
                        {
                            Thread.Sleep(3000);
                            return BD;
                        }
                    }
                }
            }
            return $"Connect Timeout False";
        }
        bool CRYEnterHFPmode()
        {
            Thread.Sleep(1000);
            getH.gethandle("5750", "0483", "mi_03");
            ; string cmd14 = "@2 avrcp nfy changed 1 1 2";
            string cmd10 = "@1 a2dp streamig stop 1";
            string cmd16 = "@0 dial";

            string[] cmd = new string[] { cmd14, cmd10, cmd16 };

            int i = 0;
            foreach (string item in cmd)
            {
                i++;
                byte[] bytecmd = System.Text.Encoding.ASCII.GetBytes("." + item + "..");

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
                //00 48 46 50 2D 41 47 20 31 20 43 4F 44 45 43 20 4D 53 42 43 0D
                if (!this.cmd.WriteSend(str.Trim(), 65, getH.Handle)) return false;
            }
            for (int b = 0; b < 5; b++)
            {
                string value = this.cmd.IsReturnValue(getH.Path, "0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27", 150);
                string str1 = "00 43 4F 4E 4E 45 43 54";
                string str2 = "0 48 46 50 2D 41 47";
                string str3 = "00 41 32 44 50 20 53 54 52 45 41 4D 49 4E 47";
                if (value.Contains(str1) || value.Contains(str2) || value.Contains(str3))
                {
                    Thread.Sleep(1000);
                    return true;
                }
            }
            return false;
        }
        bool CRYEnterA2DPmode()
        {
            Thread.Sleep(500);
            getH.gethandle("5750", "0483", "mi_03");
            string cmd4 = "@0 hangup";
            string cmd14 = "close 4";
            string cmd15 = "@1 a2dp streaming start 1";
            string cmd16 = "@2 avrcp nfy changed 1 1 1";
            //进入A2DP
            string[] cmd = new string[] { cmd4, cmd14, cmd15, cmd16 };
            int i = 0;
            foreach (string item in cmd)
            {
                i++;
                byte[] bytecmd = System.Text.Encoding.ASCII.GetBytes("." + item + "..");
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
                Thread.Sleep(1000);
            }
            return true;
        }
        bool CRY574Disconnect()
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

    }
}
