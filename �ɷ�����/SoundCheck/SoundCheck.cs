using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        Dictionary<string, object> Config;
        #region 程序接口
        //主程序所使用的变量
        public object Interface(Dictionary<string, object> keys) => this.Config = keys;

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：SoundCheck";
            string dllfunction = "Dll功能说明 ：SoundCheck控制模块";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllVersion = "当前Dll版本：21.7.29.12";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo2 = "0.0.1.2：2021 /7/15：修复控制窗体失灵，增加启动sc测试标志位，获取sc结果检测标志位";
            string dllChangeInfo3 = "21.7.29.12：2021 /7/29：增加";
            string dllChangeInfo4 = "21.8.30.0：获取sound check单步的测试结果";
            string dllChangeInfo5 = "22.12.6.0：修复重复链接问题";



            string[] info = { dllname,
                dllfunction,
                dllHistoryVersion,
                dllVersion,
                dllChangeInfo, dllChangeInfo2,dllChangeInfo3,dllChangeInfo4,dllChangeInfo5};
            return info;
        }
        public string Run(object[] Command)
        {
            string[] cmd = new string[0];
            string SN = (string)this.Config["SN"];
            foreach (var item in Command)
            {
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            switch (cmd[1].ToLower())
            {
                case "start": return StartSoundCheck(SN).ToString();
                case "opensoundcheck": return OpenSoundCheck((string)this.Config["SoundCheckPath"]).ToString();
                case "connecttoserver":
                    if (!SoundCheck16.client.Connected)
                    {
                        if (!ConnectToServer()) return "建立连接失败False";
                        Thread.Sleep(500);
                        if (!OpenSequence((string)this.Config["SqcPath"])) return "运行Sqc失败False";
                        Thread.Sleep(500);
                    }
                    return true.ToString();
                case "getresult": return GetSoundCheckResult().ToString();
                case "forms":
                    return fw.SendKeyToWindow(cmd[2], false, cmd[3], out string text).ToString();
                case "checkforms":
                    return fw.SendKeyToWindow(cmd[2], false);
                case "isconnentandrunsequence":
                    return isConnentAndRunSequence();
                case "getstepresult":
                    return GetStepResult(cmd[2]);
                case "discardbuffereddata":
                    return DiscardBufferedData();
                default:
                    return "Command Error False";
            }
        }

        #endregion






        private SoundCheck16 sc = new SoundCheck16();
        private WindowControl fw = new WindowControl();
        private string Msg = "";

        /// <summary>
        /// 计时器弹窗
        /// </summary>
        /// <param name="sleep"></param>
        /// <returns></returns>
        bool messagebox(double sleep)
        {
            SoundCheck.scForm scform = new SoundCheck.scForm(sleep);
            scform.TopMost = true;
            scform.ShowDialog();
            return scform.DialogResult == DialogResult.Yes;
        }
        /// <summary>
        /// 打开SC程序
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool OpenSoundCheck(string path)
        {
            foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses()) if (thisProc.ProcessName.Contains("SoundCheck")) return true;
            try
            {

                Thread.Sleep(500);
                Process.Start(path, ""); // Start SoundCheck with commandline arguments
                return messagebox(65000);
            }
            catch (Exception)
            {
                return false;
            }


        }
        /// <summary>
        /// 与SC建立链接
        /// </summary>
        /// <returns></returns>
        private bool ConnectToServer()
        {
            if (SoundCheck16.client.Connected) return true;
            sc.ConnectToServer();
            this.Msg = this.sc.GetMsg();
            if (this.Msg.Contains("Connected to SoundCheck ok."))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 运行SQC文件
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        private bool OpenSequence(string Path)
        {
            sc.OpenSequence(Path);
            this.Msg = sc.GetMsg();
            if (this.Msg.Contains("Sequence Opened ok."))
            {
                return true;
            }
            return false;
        }
        bool SCresult = false;
        bool StartRunSoundCheck;
        //写入SN 并开始测试
        public string StartSoundCheck(string SN)
        {
            if (!SoundCheck16.client.Connected)
            {
                if (!OpenSoundCheck((string)this.Config["SoundCheckPath"]))
                    return "OpenSoundCheck False";
                if (!ConnectToServer())
                {
                    Msg = "与soundcheck建立连接失败，请检查端口是否一致";
                    System.Windows.Forms.MessageBox.Show(Msg);
                    return "建立连接失败，请检查端口是否一致 False";
                }
                if (!OpenSequence((string)this.Config["SqcPath"]))
                    return "Run Sqc False";
                Thread.Sleep(1000);
            }
            for (int i = 0; i < sc.RunCount; i++)
            {
                sc.GetResponseJSON();
                sc.RunCount--;
            }
            sc.SendSN(SN);
            Msg = sc.GetMsg();
            if (!this.Msg.Contains("Serial number set ok.")) return $"Serial number set False";
            SCresult = false;
            Thread.Sleep(500);
            sc.RunSequence();
            Msg = sc.GetMsg();
            if (Msg.Contains("ok"))
            {
                StartRunSoundCheck = SCresult = true;
                sc.RunCount++;

            }
            return SCresult.ToString();
        }
        /// <summary>
        /// 获取SC的最总测试结果
        /// </summary>
        /// <returns></returns>
        public bool GetSoundCheckResult()
        {
            if (!SCresult) return false;
            SCresult = false;
            return sc.GetResult();
        }
        /// <summary>
        /// 关闭SC程序
        /// </summary>
        /// <returns></returns>
        public string CloseSoundCheck()
        {
            //断开连接
            try
            {
                sc.closeServer();
                sc.ExitSoundCheck();
            }
            catch { }
            //关闭程序
            foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                if (thisProc.ProcessName.Contains("SoundCheck"))
                    if (System.Windows.Forms.MessageBox.Show("是否关闭sound check", "提示", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        try
                        {
                            Thread.Sleep(3000);
                            thisProc.Kill();
                        }
                        catch { }

                    }
            return "";
        }

        string isConnentAndRunSequence()
        {
            if (!SoundCheck16.client.Connected)
            {
                if (!ConnectToServer())
                {
                    SoundCheck16.client.Close();
                    return "Connect Server False";
                }
                Thread.Sleep(500);
                if (!OpenSequence((string)this.Config["SqcPath"]))
                {
                    SoundCheck16.client.Close();
                    return "Run Sequence False";
                }
                Thread.Sleep(500);
            }
            // sc.SequenceGetStepsList();
            return $"{true}";


        }
        string DiscardBufferedData()
        {
            StartRunSoundCheck = false;
            return "True";
        }

        string GetStepResult(string StepNamse)
        {

            if (!StartRunSoundCheck) return "No Data False";
            GetSoundCheckResult();
            return sc.GetStepResult(StepNamse);
        }
    }
}
