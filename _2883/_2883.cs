using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        Dictionary<string, object> keys;
        #region 程序接口
        //主程序所使用的变量
        public object Interface(Dictionary<string, object> keys) => this.keys = keys;
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：_2883";
            string dllfunction = "Dll功能说明 ：SUB2883功能模块";
            string dllHistoryVersion = "历史Dll版本：21.8.4.10";
            string dllHistoryVersion2 = "                     ：21.8.4.10";
            string dllVersion = "当前Dll版本：21.8.4.10";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo2 = "21.8.4.10：2021/8/4：测试版";
            string dllChangeInfo3 = "21.7.29.12：2021 /7/29：增加";

            string[] info = { dllname,
                dllfunction,
                dllHistoryVersion,
                dllVersion,
                dllChangeInfo, dllChangeInfo2
            };
            return info;
        }
        Dictionary<string, StartUSB2883> _2883 = new Dictionary<string, StartUSB2883>();
        #endregion
        public string Run(object[] Command)
        {
            string[] cmd = new string[0];
            foreach (var item in Command)
            {
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            switch (cmd[1].ToLower())
            {
                case "start":
                    if (!_2883.ContainsKey(cmd[2])) _2883.Add(cmd[2], new StartUSB2883());
                    return _2883[cmd[2]].Start(cmd[3], cmd[4]).ToString();
                case "getvalue":
                    if (!_2883.ContainsKey(cmd[2])) return $"ID:{cmd[2]}：需要先进行测试 +False";
                    return _2883[cmd[2]].avg[int.Parse(cmd[3])].ToString();
                default: return "指令错误 False";
            }
        }
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string Section, string key, string def, StringBuilder refVal, int size, string INIpath);
        [DllImport("kernel32.dll")]
        private static extern int WritePrivateProfileString(string Section, string key, string Val, string INIpath);

    }
}
