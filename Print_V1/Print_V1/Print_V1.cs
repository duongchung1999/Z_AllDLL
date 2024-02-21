using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Seagull.BarTender.Print;
using System.IO;
using System.Reflection;
using Print;
using System.Threading.Tasks;
using Print_V1;

namespace MerryDllFramework
{
    /// <summary dllName="Print_V1">
    /// Print_V1
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法
        public object Interface(Dictionary<string, object> keys)
          => Config = keys;
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Print";
            string dllfunction = "Dll功能说明 ：控制打印机";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllHistoryVersion1 = "                     ：21.8.2.3";
            string dllHistoryVersion2 = "                     ：22.3.14.0";
            string dllVersion = "当前Dll版本：22.9.16.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "21.8.2.3：2021/8/26：btw文件自动获取，文件跟dll放一起，增加打印机打印内容";
            string dllChangeInfo2 = "22.9.16.0：重构打印方法";

            string[] info = { dllname, dllfunction, dllHistoryVersion, dllHistoryVersion1, dllHistoryVersion2,
                    dllVersion, dllChangeInfo, dllChangeInfo1,dllChangeInfo2
            };
            return info;
        }
        Dictionary<string, object> Config;
        Engine btEngine = null;


        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] cmd);
            try
            {
                switch (cmd[1])
                {
                    case "PrintSN": return PrintSN();
                    case "PrintBD": return PrintBD();
                    case "PrintDictionary": return PrintDictionary(cmd[2]);
                    default: return "Command Error False";
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
                return $"{ex.Message} False";
            }

        }

        void SplitCMD(object[] Command, out string[] CMD)
        {
            List<string> listCMD = new List<string>();
            foreach (var item in Command)
            {
                Type type = item.GetType();
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            CMD = listCMD.ToArray();
            try
            {
                if (btEngine == null)
                {
                    string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    Config["PrintFilePath"] = Directory.GetFiles(dllPath, "*.btw")[0];
                    btEngine = new Engine();
                    btEngine.Start();

                    if (Config["Print"].ToString().Length < 3)
                    {
                        Form1 form = new Form1();
                        form.ShowDialog();
                        Config["Print"] = form.printName;
                        windowsAPI.WritePrivateProfileString("Print", "Print", form.printName, $@"{Config["adminPath"]}\Config\CONFIG.ini");
                        form.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                btEngine?.Dispose();
                btEngine = null;
            }
        }


        /// <summary isPublicTestItem="true">
        /// 打印BD Config["BitAddress"]字段的BD
        /// </summary>
        /// <returns></returns>
        public string PrintBD()
        {
            Dictionary<string, string> SNKey = new Dictionary<string, string>();
            SNKey["BD"] = (string)Config["BitAddress"];
            SNKey["Name"] = (string)Config["Name"];
            return Prints(SNKey, (string)Config["PrintFilePath"]);
        }

        /// <summary isPublicTestItem="true">
        /// 打印主条码 SN
        /// </summary>
        /// <returns></returns>
        public string PrintSN()
        {
            Dictionary<string, string> SNKey = new Dictionary<string, string>();
            SNKey["BD"] = (string)Config["SN"];
            SNKey["Name"] = (string)Config["Name"];
            return Prints(SNKey, (string)Config["PrintFilePath"]);
        }
        /// <summary isPublicTestItem="true">
        /// 将所打印的内容写成 Dictionary string string 字典赋值给Config["PrintDictionary"] 程序遍历打
        /// </summary>
        /// <param name="PrintFilePath">TestItem机型下面的打印文件 文件名包括后缀</param>
        /// <returns></returns>
        public string PrintDictionary(string PrintFilePath)
        {
            string Path = $@"{Config["adminPath"]}\TestItem\{Config["Name"]}\{PrintFilePath}";

            return Prints((Dictionary<string, string>)Config["PrintDictionary"], Path);
        }
        /// <summary>
        /// 打印机方法
        /// </summary>
        /// <param name="PrintKeys"></param>
        /// <param name="PrintFilePath"></param>
        /// <returns></returns>
        string Prints(Dictionary<string, string> PrintKeys, string PrintFilePath)
        {

            LabelFormatDocument btFormat = btEngine.Documents.Open(PrintFilePath);
            btFormat.PrintSetup.PrinterName = Config["Print"].ToString();
            foreach (var item in PrintKeys)
                btFormat.SubStrings[item.Key].Value = item.Value;
            Result nResult1 = btFormat.Print("标签打印软件", 10000, out Messages messages);
            btFormat.PrintSetup.Cache.FlushInterval = CacheFlushInterval.PerSession;
            return "True";
            //return nResult1.ToString() == "Success" ? "True" : $"{nResult1} {false}";
        }
        #endregion



    }
}
