using Airoha_ANC_Debug_Tool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    /// <summary dllName="ScreenRecording">
    /// 录屏
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：ScreenRecording ";
            string dllfunction = "Dll功能说明 ：录屏模块";
            string dllVersion = "当前Dll版本：23.6.6.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.6.6.0：  初步开发录屏软件";
            string[] info = { dllname, dllfunction,
                dllVersion,
                dllChangeInfo, dllChangeInfo1
            };
            return info;
        }

        Dictionary<string, object> Config = new Dictionary<string, object>();
        public object Interface(Dictionary<string, object> Config)
        {
            return this.Config = Config;

        }

        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] cmd);
            switch (cmd[1])
            {
                case "StartRecording":
                    return StartRecording(int.Parse(cmd[2]));

                case "StopRecording":
                    return StopRecording();
                default:
                    return $"Command Error {false}";
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
        static string DirdllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static string iniPath = $@"{DirdllPath}\RecordingTool.ini";
        static string exePath = $"{System.Windows.Forms.Application.StartupPath}";
        /// <summary isPublicTestItem="true">
        /// 开始录屏 录屏开头会少3秒
        /// </summary>
        /// <param name="TimeOut_S">测一个多长时间 就填多长时间</param>
        /// <returns></returns>
        string StartRecording(int TimeOut_S)
        {
            string SavePath = $@"{exePath}\LOG\{Config["Name"]}_ScreenRecording_{DateTime.Now.DayOfYear / 7 + 1}\{Config["SN"]}_{DateTime.Now:dd_HH_mm_ss}.mp4";
            Hini.SetValue("Config", "TimeOut_S", $"{TimeOut_S}", iniPath);
            Hini.SetValue("Config", "SavePath", SavePath, iniPath);
            Hini.SetValue("Config", "RecordStatus", "Start", iniPath);
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
                Thread.Sleep(1000);
            }
            Process.Start($@"{DirdllPath}\_ScreenRecordingProgram.exe");

            //ResponseCallCMD("_ScreenRecordingProgram.exe");
            return "True";
        }
        /// <summary isPublicTestItem="true">
        /// 停止录屏
        /// </summary>
        /// <returns></returns>
        public string StopRecording()
        {
            Hini.SetValue("Config", "RecordStatus", "Stop", iniPath);
            return "True";
        }
        Process program;
        StringBuilder ProcessLog = new StringBuilder();
        List<string> listLog = new List<string>();
        string ResponseCallCMD(string Command)
        {
            string Result = "TimeOut False";
            ProcessLog.Clear();
            listLog.Clear();
            if (program == null)
            {
                program = new Process();
                program.StartInfo.FileName = $"cmd";
                program.StartInfo.UseShellExecute = false;
                program.StartInfo.RedirectStandardOutput = true;
                program.StartInfo.RedirectStandardInput = true;
                program.StartInfo.CreateNoWindow = true;//
                program.StartInfo.RedirectStandardError = true;
                program.Start();
                program.StandardOutput.DiscardBufferedData();
                Thread.Sleep(500);
                Thread ReadThread = new Thread(() =>
                {
                    while (!program.StandardOutput.EndOfStream)
                    {
                        string readstr = program.StandardOutput.ReadLine();
                        ProcessLog.AppendLine(readstr);
                        Console.WriteLine(readstr);
                    };

                });
                ReadThread.Start();
                program.StandardInput.WriteLine($"cd {DirdllPath}");
            }
            program.StandardInput.WriteLine($"{Command}");
            Thread.Sleep(100);
            ProcessLog.Clear();
            return Result;
        }


    }
}

