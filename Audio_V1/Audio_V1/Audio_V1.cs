using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    /// <summary dllName="Audio_V1">
    /// Audio_V1
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法
        Dictionary<string, object> Config;
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Audio";
            string dllfunction = "Dll功能说明 ：播放音乐，控制音量";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";

            string dllVersion = "当前Dll版本：22.12.15.1";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "22.12.15.0：重构Audio方法 可以上传项目到后台";
            string[] info = { dllname, dllfunction,
                dllHistoryVersion,
                dllVersion,
                dllChangeInfo,dllChangeInfo1
            };

            return info;
        }
        public object Interface(Dictionary<string, object> keys)
        {
            //Config["Name"].ToString();
            return Config = keys;
        }
        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] cmd);
            bool MoreTest = OnceConfig.ContainsKey("SN");
            switch (cmd[1])
            {

                case "PlayTest": return PlayTest(cmd[2], cmd[3], cmd[4]);
                case "RecordTest": return RecordTest(cmd[2], cmd[3]);
                case "Sidetone": return Sidetone(cmd[2]);
                case "PlayMusic": return PlayMusic(cmd[2], cmd[3]);
                case "StopMusic": return StopMusic();
                case "SetSpkVolume": return SetSpkVolume(cmd[2], int.Parse(cmd[3]));
                case "SetMicVolume": return SetMicVolume(cmd[2], int.Parse(cmd[3]));
                case "A2DP": return A2DP();
                case "HFP": return HFP();
                case "CloseRundll32": return CloseRundll32();
                case "SearchPlayDevice": return SearchPlayDevice(cmd[2], int.Parse(cmd[3]));
                case "SearchRecordDevice": return SearchRecordDevice(cmd[2], int.Parse(cmd[3]));
                default:
                    return "Command Error Fasle";
            }

        }



        #endregion


        void SplitCMD(object[] Command, out string[] CMD)
        {
            List<string> listCMD = new List<string>();
            foreach (var item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>))
                    OnceConfig = (Dictionary<string, object>)item;
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            CMD = listCMD.ToArray();
        }

        readonly string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary isPublicTestItem="true">
        /// 播放音乐测试
        /// </summary>
        /// <param name="DeviceName">装置名称</param>
        /// <param name="MuticName" options="加州音馆,敲门声,50-3K扫频音">选择音源</param>
        /// <param name="Message">显示信息</param>
        /// <returns>info</returns>
        public string PlayTest(string DeviceName, string MuticName, string Message)
        {
            A2DP();
            string name = "0";
            switch (MuticName)
            {
                case "加州音馆":
                    name = "0";
                    break;
                case "敲门声":
                    name = "1";
                    break;
                case "50-3K扫频音":
                    name = "2";
                    break;
            }
            string Reverse = MerryTestFramework.Audio.PlayTest(DeviceName, IntPtr.Zero, $@"{dllPath}\Music\{name}.wav", true, Message);
            CloseRundll32();
            return Reverse;
        }
        /// <summary isPublicTestItem="true">
        /// 录音测试
        /// </summary>
        /// <param name="DeviceName">装置名称</param>
        /// <param name="Message">显示信息</param>
        /// <returns>info</returns>
        public string RecordTest(string DeviceName, string Message)
        {
            HFP();
            string Reverse = MerryTestFramework.Audio.RecordTest($@"{dllPath}\Music\rec.wav", DeviceName, IntPtr.Zero, Message);
            CloseRundll32();
            return Reverse;
        }
        /// <summary isPublicTestItem="true">
        /// 聆听模式
        /// </summary>
        /// <param name="Message">显示信息</param>
        /// <returns>info</returns>
        public string Sidetone(string Message)
            => MerryTestFramework.Audio.Sidetone(Message).ToString();

        /// <summary isPublicTestItem="true">
        /// 播放音乐
        /// </summary>
        /// <param name="DeviceName">装置名称</param>
        /// <param name="MusicName">音乐文件名称 带后缀 放在机型下面</param>
        /// <returns>info</returns>
        public string PlayMusic(string DeviceName, string MusicName)
        {
            if (OnceConfig.ContainsKey("AudioName"))
            {
                return MerryTestFramework.Audio.playmusics((string)OnceConfig["AudioName"], IntPtr.Zero, $@"{Config["adminPath"]}\TestItem\{Config["Name"]}\{MusicName}", true, Convert.ToInt32(OnceConfig["TestID"])).ToString();
            }
            else
            {
                return MerryTestFramework.Audio.playmusic((string)OnceConfig["AudioName"], IntPtr.Zero, $@"{Config["adminPath"]}\TestItem\{Config["Name"]}\{MusicName}", true).ToString();
            }

        }
        /// <summary isPublicTestItem="true">
        /// 停止播放音乐
        /// </summary>
        /// <returns>info</returns>
        public string StopMusic()
        {
            return OnceConfig.ContainsKey("AudioName") ? MerryTestFramework.Audio.StopPlay(Convert.ToInt32(OnceConfig["TestID"])) : MerryTestFramework.Audio.StopPlay();
        }
        /// <summary isPublicTestItem="true">
        /// 设置播放装置音量
        /// </summary>
        /// <param name="DeviceName">装置名称</param>
        /// <param name="SpkVolume">音量</param>
        /// <returns>info</returns>
        public string SetSpkVolume(string DeviceName, int SpkVolume)
            => System.Audio.Volume.SistemVolumChanger.SPKVOL(OnceConfig.ContainsKey("AudioName") ? (string)OnceConfig["AudioName"] : DeviceName, SpkVolume);
        /// <summary isPublicTestItem="true">
        /// 设置录音装置音量
        /// </summary>
        /// <param name="DeviceName">装置名称</param>
        /// <param name="MicVolume">音量</param>
        /// <returns>info</returns>
        public string SetMicVolume(string DeviceName, int MicVolume)
            => System.Audio.Volume.SistemVolumChanger.MICVOL(OnceConfig.ContainsKey("AudioName") ? (string)OnceConfig["AudioName"] : DeviceName, MicVolume);

        /// <summary isPublicTestItem="true">
        /// 打开A2DP
        /// </summary>
        /// <returns>info</returns>
        public string A2DP()
        {
            CloseRundll32();
            Process.Start("rundll32.exe", @"C:\WINDOWS\system32\shell32.dll,Control_RunDLL mmsys.cpl,,0");
            for (int i = 0; i < 6; i++)
            {
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                    if (thisProc.ProcessName.Contains("rundll32"))
                        break;
                Thread.Sleep(500);
            }
            return "True";

        }
        /// <summary isPublicTestItem="true">
        /// 打开HFP
        /// </summary>
        /// <returns>info</returns>
        public string HFP()
        {
            CloseRundll32();
            Process.Start("rundll32.exe", @"C:\WINDOWS\system32\shell32.dll,Control_RunDLL mmsys.cpl,,1");
            for (int i = 0; i < 6; i++)
            {
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                    if (thisProc.ProcessName.Contains("rundll32"))
                        break;
                Thread.Sleep(500);
            }
            return "True";

        }
        /// <summary isPublicTestItem="true">
        /// 关闭A2DP和HFP
        /// </summary>
        /// <returns>info</returns>
        public string CloseRundll32()
        {
            foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                if (thisProc.ProcessName.Contains("rundll32"))
                    if (!thisProc.CloseMainWindow())
                    {
                        thisProc.Kill();
                        Thread.Sleep(100);
                    }
            return "True";
        }
        /// <summary isPublicTestItem="true">
        /// 搜索播放装置
        /// </summary>
        /// <param name="DeviceName">装置名称</param>
        /// <param name="TimeOut">超时 秒</param>
        /// <returns>info</returns>
        public string SearchPlayDevice(string DeviceName, int TimeOut)
        {
            return System.Audio.Volume.SistemVolumChanger.SearchPlayDevice(OnceConfig.ContainsKey("AudioName") ? (string)OnceConfig["AudioName"] : DeviceName, TimeOut);

        }

        /// <summary isPublicTestItem="true">
        /// 搜索录音装置
        /// </summary>
        /// <param name="DeviceName">装置名称</param>
        /// <param name="TimeOut">超时 秒</param>
        /// <returns>info</returns>
        public string SearchRecordDevice(string DeviceName, int TimeOut)
        {
            return System.Audio.Volume.SistemVolumChanger.SearchRecordDevice(OnceConfig.ContainsKey("AudioName") ? (string)OnceConfig["AudioName"] : DeviceName, TimeOut);
        }

    }
}
