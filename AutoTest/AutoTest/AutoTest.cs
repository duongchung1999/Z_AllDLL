using AutoTest;
using AutoTest.Class;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    /// <summary>
    /// 存放Config数据
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法
        static Dictionary<string, object> Config;
        public string Run(object[] Command) => "";
        public object Interface(Dictionary<string, object> keys)
        {
            Config = keys;
            return Config;
        }
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：AutoTest";
            string dllfunction = "Dll功能说明 ：自动化测试";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllVersion = "当前Dll版本：23.5.18.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.5.18.0：MEVNAutoTest 每次进行测试把Config[SN]净空掉";
            string[] info = { dllname, dllfunction,
                dllHistoryVersion, dllVersion, dllChangeInfo,dllChangeInfo1
            };

            return info;
        }

        #endregion

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern int AttachConsole(IntPtr Handle);
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        public static extern bool ClosePseudoConsole(IntPtr Handle);


        public void AutoTest()
        {

            if (!Config.ContainsKey("AutoTest"))
                return;
            if (Convert.ToInt32(Config["AutoTest"]) < 1)
                return;
            //联动系统控制台 有个BUG  如果在调用之前 进行对控制台的输出（Console.WriteLine），会导致调用后程序的输出不会显示在控制台
            AllocConsole();

            switch (Convert.ToInt32(Config["AutoTest"]))
            {
                case 1:
                    new Thread(new MEVNAutoTest(Config).MEVNTest).Start();
                    break;
                case 2:
                    new Thread(new FileWorking(Config).AutoTest).Start();
                    break;
                case 3:
                    new Thread(new TCP_Control(Config).Test).Start();
                    break;
                default:
                    MessageBox.Show($"自动测试的Case选错了 {Config["AutoTest"]}", "AutoTest 提示");
                    break;
            }


        }



    }
}
