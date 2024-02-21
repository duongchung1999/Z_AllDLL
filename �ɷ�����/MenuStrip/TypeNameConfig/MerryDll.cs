using Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    /// <summary>
    /// 存放Config数据
    /// </summary>
    public class MerryDll : MerryAllDll
    {
        Dictionary<string, object> keys;
        public string Interface(Dictionary<string, object> dic)
        {
            keys = dic;
            return "True";
        }
        public string Run()
        {
            string Type = keys["Name"].ToString();
            string ConfigPath = $@".\TestItem\{Type}\Config.txt";
            string configPath = $@".\TestItem\{Type}\config.txt";
            string ConfigType = $@".\TestItem\{Type}\ConfigType.ini";



            if (File.Exists(ConfigPath))
            {
                Form1.GetForm1(ConfigPath).Show();
                return "";
            }

            if (File.Exists(configPath))
            {
                Form1.GetForm1(configPath).Show();
                return "";

            }
            if (File.Exists(ConfigType))
            {

            }
            else
            {
                File.WriteAllText(ConfigType, "[Config]");
            }
            Form1.GetForm1(ConfigType).Show();
            return "";

        }
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ： ";
            string dllfunction = "Dll功能说明 ： ";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllVersion = "当前Dll版本：21.8.26.0";
            string dllChangeInfo = "Dll改动信息：";
            string[] info = { dllname, dllfunction, dllHistoryVersion,
                dllVersion, dllChangeInfo,
            };

            return info;
        }

    }
}
