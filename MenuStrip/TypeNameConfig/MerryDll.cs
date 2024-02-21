using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string Type = keys["Name"].ToString().Split('-')[0];
            string ConfigPath = $@".\TestItem\{Type}\config.txt";
            string MessagePath = $@".\TestItem\{Type}\Message.txt";
            Form1.GetForm1(ConfigPath, MessagePath).Show();
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
