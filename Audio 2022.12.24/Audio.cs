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
using Un4seen.Bass;
using WindowsFormsApplication1.FunctionalTest.Interface;
using Audio;


namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        IntPtr handle = IntPtr.Zero;
        string TypeName = null;
   

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
                case "PlayTest":
                    return MerryTestFramework.Audio.PlayTest(cmd[2], handle, $@".\Music\{cmd[3]}.wav", true, cmd[4]).ToString();
                case "Play":
                    dic["AudioFlag"] = false;
                    if ((bool)dic["AudioFlag"]) return "True";
                    dic["AudioFlag"] = true;
                    MerryTestFramework.Audio.playmusic2(cmd[2], handle, $@".\Music\{cmd[3]}.wav", true);
                    return "True";
                case "RecordTest":
                    return MerryTestFramework.Audio.RecordTest(cmd[2], handle, cmd[3], cmd[4]).ToString();
                case "RecordTest-0":
                    return WindowsFormsApplication1.FunctionalTest.Interface.plays.RecordTest().ToString();
                case "RecordTest-N":
                    SoundRecordss recorder = new SoundRecordss(cmd[2]);
                    string wavfile = $@"{Directory.GetCurrentDirectory()}\Music\rec.wav";
                    recorder.SetFileName(wavfile);
                    return recorder.Record(cmd[3]).ToString();
                case "PlayMusic":
                    return MerryTestFramework.Audio.playmusic(cmd[2], handle, $@".\TestItem\{TypeName}\{cmd[3]}", true).ToString();//$@".\TestItem\{TypeName.Split('-')[0]}\{cmd[3]}"
                case "StopMusic":
                    MerryTestFramework.Audio.StopPlay();
                    dic["AudioFlag"] = false;
                    return true.ToString();
                case "SpkVolume":
                    return System.Audio.Volume.SistemVolumChanger.SPKVOL(cmd[2], int.TryParse(cmd[3], out int SpkVolume) ? SpkVolume : 100);
                case "MicVolume":
                    return System.Audio.Volume.SistemVolumChanger.MICVOL(cmd[2], int.TryParse(cmd[3], out int MicVolume) ? MicVolume : 100);
                case "HFP": return HFP();
                case "A2DP": return A2DP();
                case "CloseRundll32": return CloseRundll32();
                case "Sidetone": return MerryTestFramework.Audio.Sidetone(cmd[2]).ToString();

                ///
                case "MusicNoPV":
                    return MerryTestFramework.Audio.MusicPlay(cmd[2], cmd[3]).ToString();
                case "RecordNoPV":
                    return MerryTestFramework.Audio.RecordPlayTest(cmd[2], cmd[3]).ToString();

                //case "VolumeUp": return MerryTestFramework.Audio.VolumeUp().ToString();

                //case "VolumeDown": return MerryTestFramework.Audio.VolumeDown().ToString();
                default: return "指令错误 False";
            }

        }
        #region 接口方法
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Audio";
            string dllfunction = "Dll功能说明 ：播放音乐，控制音量";
            string dllHistoryVersion = "历史Dll版本：23.3.16.2";

            string dllVersion = "当前Dll版本：23.8.14.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "21.8.9.1：2021/8/9：增加A2DP和HFP模式";
            string dllChangeInfo2 = "21.11.17.1：2021/11/17：修改播放录音方法";
            string dllChangeInfo3 = "21.11.17.8：2021/11/19：增加程序聆听功能,以及调整音量失败带False";
            string dllChangeInfo4 = "21.11.30.20：2021/12/2：修改录音方法,递增录音方法RecordTest-0";
            string dllChangeInfo5 = "23.3.16.0：2023/3/16：增加播放音乐使用别的功能";
            string dllChangeInfo6 = "23.8.14.0：2023/8/14：调整PlayMusic方法的引用路径";
            string[] info = { dllname, dllfunction,
                dllHistoryVersion,
                dllVersion,
                dllChangeInfo,dllChangeInfo1,dllChangeInfo2,dllChangeInfo3,dllChangeInfo4,dllChangeInfo5,dllChangeInfo6
            };

            return info;
        }
        Dictionary<string, object> dic;
        public string Interface(Dictionary<string, object> keys)
        {
            dic = keys;
            TypeName = dic["Name"].ToString();
            return "";
        }
        //public static bool VolumeUp()
        //{
        //    return VolumeTestPlan.VolumeTest(true, "上调音量/Vui lòng vặn tăng âm lượng");
        //}
        //public static bool VolumeDown()
        //{
        //    return VolumeTestPlan.VolumeTest(false, "下调音量/ Vui lòng vặn giảm âm lượng");
        //}
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
        string CloseRundll32()
        {
            foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                if (thisProc.ProcessName.Contains("rundll32"))
                    if (!thisProc.CloseMainWindow())
                        thisProc.Kill();
            return "True";
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
