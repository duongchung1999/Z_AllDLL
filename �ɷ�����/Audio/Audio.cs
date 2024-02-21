using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Un4seen.Bass;
using WindowsFormsApplication1.FunctionalTest.Interface;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {

        IntPtr handle = IntPtr.Zero;
        string TypeName = null;
        readonly string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public string Run(object[] Command)
        {
            string[] cmd = new string[20];
            foreach (var item in Command)
            {
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1].Trim();
            }
            switch (cmd[1])
            {
                //播放音乐，搜索设备超时5秒
                case "PlayTest":
                    return MerryTestFramework.Audio.PlayTest(cmd[2], handle, $@"{dllPath}\Music\{cmd[3]}.wav", true, cmd[4]);

                //常规录音测试
                case "RecordTest":
                    return MerryTestFramework.Audio.RecordTest($@"{dllPath}\Music\rec.wav", cmd[2], handle, cmd[3]);
                //使用默认的录音装置录音，为高版本win10准备。
                case "RecordTest-0":
                    return WindowsFormsApplication1.FunctionalTest.Interface.plays.RecordTest($@"{dllPath}\Music\rec.wav").ToString();
                //一些设备无法搜索到录音装置上面的方法不行只能用这个方法
                case "RecordTest-N":
                    SoundRecordss recorder = new SoundRecordss(cmd[2]);
                    string wavfile = $@"{dllPath}\Music\rec.wav";
                    recorder.SetFileName(wavfile);
                    return recorder.Record(wavfile, cmd[3]);
                //使用聆听的模式取测试录音
                case "Sidetone": return MerryTestFramework.Audio.Sidetone(cmd[2]).ToString();
                //只播放音乐
                case "PlayMusic":
                    return MerryTestFramework.Audio.playmusic(cmd[2], handle, $@"{Config["adminPath"]}\TestItem\{TypeName}\{cmd[3]}", true).ToString();
                //停止播放音乐
                case "StopMusic":
                    MerryTestFramework.Audio.StopPlay();
                    return true.ToString();
                //调节SPK的音量
                case "SpkVolume":
                    return System.Audio.Volume.SistemVolumChanger.SPKVOL(cmd[2], int.TryParse(cmd[3], out int SpkVolume) ? SpkVolume : 100);
                //调节Mic的音量
                case "MicVolume":
                    return System.Audio.Volume.SistemVolumChanger.MICVOL(cmd[2], int.TryParse(cmd[3], out int MicVolume) ? MicVolume : 100);
                case "HFP": return HFP();
                case "A2DP": return A2DP();
                case "CloseRundll32": return CloseRundll32();
                //搜索播放装置设备
                case "SearchPlayDevice":
                    return System.Audio.Volume.SistemVolumChanger.SearchPlayDevice(cmd[2], int.Parse(cmd[3]));
                //搜索录音装置设备
                case "SearchRecordDevice":
                    return System.Audio.Volume.SistemVolumChanger.SearchRecordDevice(cmd[2], int.Parse(cmd[3]));

                default: return "Commend Error False";
            }
        }
        #region 接口方法
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Audio";
            string dllfunction = "Dll功能说明 ：播放音乐，控制音量";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllHistoryVersion1 = "                     ：21.8.9.1";
            string dllHistoryVersion2 = "                     ：21.11.17.1";
            string dllHistoryVersion3 = "                     ：21.11.17.8";

            string dllVersion = "当前Dll版本：21.11.30.20";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "21.8.9.1：2021/8/9：增加A2DP和HFP模式";
            string dllChangeInfo2 = "21.11.17.1：2021/11/17：修改播放录音方法";
            string dllChangeInfo3 = "21.11.17.8：2021/11/19：增加程序聆听功能,以及调整音量失败带False";
            string dllChangeInfo4 = "21.11.30.20：2021/12/2：修改录音方法,递增录音方法RecordTest-0,RecordTest-N,Sidetone";
            string[] info = { dllname, dllfunction,
                dllHistoryVersion, dllHistoryVersion1,dllHistoryVersion2,dllHistoryVersion3,
                dllVersion,
                dllChangeInfo,dllChangeInfo1,dllChangeInfo2,dllChangeInfo3,dllChangeInfo4
            };

            return info;
        }
        Dictionary<string, object> Config;
        public string Interface(Dictionary<string, object> keys)
        {
            Config = keys;
            TypeName = Config["Name"].ToString();
            return "";
        }
        string CloseRundll32()
        {
            foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                if (thisProc.ProcessName.Contains("rundll32"))
                    if (!thisProc.CloseMainWindow())
                        thisProc.Kill();
            return "True";
        }
        string A2DP()
        {
            try
            {
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                    if (thisProc.ProcessName.Contains("rundll32"))
                        if (!thisProc.CloseMainWindow())
                            thisProc.Kill();
                Thread.Sleep(100);

                Process.Start("rundll32.exe", @"C:\WINDOWS\system32\shell32.dll,Control_RunDLL mmsys.cpl,,0");
                Thread.Sleep(500);
                return "True";
            }
            catch
            {
                return "False";
            }
        }
        string HFP()
        {
            try
            {
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                    if (thisProc.ProcessName.Contains("rundll32"))
                        if (!thisProc.CloseMainWindow())
                            thisProc.Kill();
                Thread.Sleep(100);
                Process.Start("rundll32.exe", @"C:\WINDOWS\system32\shell32.dll,Control_RunDLL mmsys.cpl,,1");
                Thread.Sleep(500);
                return "True";
            }
            catch
            {
                return "False";
            }
        }
        #endregion

    }
}
