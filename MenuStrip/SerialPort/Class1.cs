using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    public class MerryDll
    {

        #region 主程序调用接口
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：调试工具";
            string dllfunction = "Dll功能说明 ：";
            string dllHistoryVersion = "历史Dll版本：0.0.0.1";
            string dllVersion = "当前Dll版本：0.0.0.1";
            string dllChangeInfo = "Dll改动信息：";
            string[] info = { dllname, dllfunction, dllHistoryVersion, dllVersion, dllChangeInfo };

            return info;
        }
        Dictionary<string, object> Keys;
        public string Interface(Dictionary<string, object> keys) => (this.Keys = keys).ToString();
        public string Run()
        {

            string dllpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Directory.GetFiles(dllpath, "SerialPortUtility.exe")[0];
            using (Process.Start(path)) { }
            return "";
        }
        #endregion


    }
}

