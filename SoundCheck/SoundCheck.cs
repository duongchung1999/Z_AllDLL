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
        Dictionary<string, object> keys;
        #region 程序接口
        //主程序所使用的变量
        public string Interface(Dictionary<string, object> keys)
        {
            this.keys = keys;
            scFlag = (bool)this.keys["SoundCheckFlag"];
            scPath = (string)this.keys["SoundCheckPath"];
            scSqcPaht = (string)this.keys["SqcPath"];
            return "OK";

        }
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：SoundCheck";
            string dllfunction = "Dll功能说明 ：SoundCheck控制模块";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllHistoryVersion2 = "                     ：21.7.29.12";
            string dllVersion = "当前Dll版本：21.7.29.12";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo2 = "0.0.1.2：2021 /7/15：修复控制窗体失灵，增加启动sc测试标志位，获取sc结果检测标志位";
            string dllChangeInfo3 = "21.7.29.12：2021 /7/29：增加";

            string[] info = { dllname,
                dllfunction,
                dllHistoryVersion,dllHistoryVersion2,
                dllVersion,
                dllChangeInfo, dllChangeInfo2,dllChangeInfo3};
            return info;
        }
        public string Run(object[] Command)
        {
            string[] cmd = new string[0];
            string SN = (string)this.keys["SN"];
            foreach (var item in Command)
            {
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            switch (cmd[1].ToLower())
            {
                case "start": return StartSoundCheck(SN).ToString();
                case "opensoundcheck": return OpenSoundCheck(scPath).ToString();
                case "connecttoserver":
                    if (!sc.client.Connected)
                    {
                        if (!ConnectToServer()) return "建立连接失败False";
                        Thread.Sleep(500);
                        if (!OpenSequence(scSqcPaht)) return "运行Sqc失败False";
                        Thread.Sleep(500);
                    }
                    return true.ToString();
                case "getresult": return GetSoundCheckResult().ToString();
                case "forms":
                    return fw.SendKeyToWindow(cmd[2], false, cmd[3], out string text).ToString();
                case "checkforms":
                    return fw.SendKeyToWindow(cmd[2], false);
                default: return GetSoundCheckResult().ToString();
            }
        }

        #endregion

        private SoundCheck16 sc = new SoundCheck16();
        private WindowControl fw = new WindowControl();
        private string Msg = "";
        bool scFlag;
        string scPath;
        string scSqcPaht;

        bool OpenSoundCheck(string path)
        {
            foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses()) if (thisProc.ProcessName.Contains("SoundCheck")) return true;
            try
            {

                Process process = new Process();
                process.StartInfo.FileName = path;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                return messagebox(65000);
            }
            catch (Exception)
            {
                return false;
            }


        }
        bool messagebox(double sleep)
        {
            SoundCheck.scForm scform = new SoundCheck.scForm(sleep);
            scform.TopMost = true;
            scform.ShowDialog();
            return scform.DialogResult == DialogResult.Yes;
        }
        //启动程序 
        public string OpenSoundCheck()
        {
            if (!scFlag) return "";
            Task.Run(() =>
            {
                bool isOpenSC = false;
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses()) if (thisProc.ProcessName.Contains("SoundCheck")) isOpenSC = true;
                try
                {
                    if (!isOpenSC)
                    {

                        if (!OpenSoundCheck(scPath))
                        {
                            MessageBox.Show("SC启动失败");
                            return "启动失败";
                        }


                    }
                    if (!ConnectToServer())
                    {
                        Msg = "与soundcheck建立连接失败，请检查端口是否一致";
                        return Msg;
                    }
                    if (!OpenSequence(scSqcPaht))
                    {
                        Msg = "开启sqc文件失败,请检查文件路径是否正确";
                        return Msg;
                    }
                    return "启动成功";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return "启动 sound check 错误";
                }
            });
            return "";
        }

        //关闭
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
        bool SCresult = false;
        //写入SN 并开始测试
        public bool StartSoundCheck(string SN)
        {
            if (!sc.client.Connected)
            {
                bool scFlag = false;
                //检测sound check 线程
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses()) if (thisProc.ProcessName.Contains("SoundCheck")) scFlag = true;
                if (!scFlag) return false;
                if (!ConnectToServer())
                {
                    Msg = "与soundcheck建立连接失败，请检查端口是否一致";
                    System.Windows.Forms.MessageBox.Show(Msg);
                    return false;
                }
                Thread.Sleep(1000);
            }
            for (int i = 0; i < sc.RunCount; i++)
            {
                sc.GetResponseJSON();
                sc.RunCount--;
            }
            sc.SendSN(SN);
            Msg = sc.GetMsg();
            if (!this.Msg.Contains("Serial number set ok.")) return false;
            SCresult = false;
            Thread.Sleep(500);
            sc.RunSequence();
            Msg = sc.GetMsg();
            if (Msg.Contains("ok"))
            {
                SCresult = true;
                sc.RunCount++;

            }
            return SCresult;
        }

        //获取结果

        public bool GetSoundCheckResult()
        {
            if (!SCresult) return false;
            SCresult = false;
            return sc.GetResult();
        }

        //打开Sqc
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
        //建立连接
        private bool ConnectToServer()
        {
            if (sc.client.Connected) return true;
            sc.ConnectToServer();
            this.Msg = this.sc.GetMsg();
            if (this.Msg.Contains("Connected to SoundCheck ok."))
            {
                return true;
            }
            return false;
        }
    }
}
