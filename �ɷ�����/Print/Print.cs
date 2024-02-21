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

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法
        public void Interface(Dictionary<string, object> keys)
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
        /// <summary>
        /// 主模板用于搜索打印机方法
        /// </summary>
        /// <returns></returns>
        public string GetPrinter()
        {
            string strList = "";
            System.Drawing.Printing.PrinterSettings.StringCollection PrinterList = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
            foreach (var item in PrinterList)
            {
                strList += item + ",";
            }
            return strList;
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
                    case "printsn":
                    case "PrintSN": return PrintSN();
                    case "printbd":
                    case "PrintBD": return PrintBD();

                    case "PrintDictionary": return PrintDictionary(cmd[2]);
                    case "prints": return Prints();
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


        /// <summary>
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

        /// <summary>
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
        /// <summary>
        /// 将所打印的内容写成 Dictionary string string 字典赋值给Config["PrintDictionary"] 程序遍历打
        /// </summary>
        /// <param name="PrintFilePath">TestItem机型下面的打印文件 *.btw</param>
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
            //MessageBox.Show(PrintFilePath);
            //string str = "";
            //foreach (var item in PrintKeys)
            //{
            //    str = $"{item.Key}:{item.Value}";
            //}
            //MessageBox.Show(str);
            LabelFormatDocument btFormat = btEngine.Documents.Open(PrintFilePath);
            btFormat.PrintSetup.PrinterName = Config["Print"].ToString();
            foreach (var item in PrintKeys)
                btFormat.SubStrings[item.Key].Value = item.Value;
            Result nResult1 = btFormat.Print("标签打印软件", 10000, out Messages messages);
            btFormat.PrintSetup.Cache.FlushInterval = CacheFlushInterval.PerSession;
            return nResult1.ToString() == "Success" ? "True" : $"{nResult1} {false}";
        }
        /// <summary>
        /// 已过时
        /// </summary>
        /// <returns></returns>
        public string Prints()
        {
            try
            {
                LabelFormatDocument INFO = btEngine.Documents.Open($"{Config["PrintFileName"]}");
                INFO.PrintSetup.PrinterName = Config["Print"].ToString();//打印机
                INFO.SubStrings[Config["PrintSection1"].ToString()].Value = Config["PrintValus1"].ToString();
                INFO.SubStrings[Config["PrintSection2"].ToString()].Value = Config["PrintValus2"].ToString();
                Result nResult1 = INFO.Print("标签打印软件", 10000, out Messages messages);
                INFO.PrintSetup.Cache.FlushInterval = CacheFlushInterval.PerSession;
                return (nResult1.ToString() == "Success").ToString();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
                return false.ToString();
            }

        }


        #endregion
    }
}
