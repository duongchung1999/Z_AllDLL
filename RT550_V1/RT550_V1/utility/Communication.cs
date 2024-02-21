using Ivi.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VISAInstrument.Port;

namespace RT550_V1
{
    static class Communication
    {
        public static List<string> listLog = new List<string>();
        public static PortOperatorBase _portOperatorBase = null;
        public static bool Connect(string IP)
        {

            if (_portOperatorBase != null) return true;
            string IpRegex = @"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$";
            if (!Regex.IsMatch(IP, IpRegex))
            {
                MessageBox.Show("IP地址不正确！");
                return false;
            }

            if (!PortUltility.OpenIPAddress(IP, out string fullAddress))
            {
                MessageBox.Show("未找到设备!");
                return false;
            }
            try
            {
                _portOperatorBase = new LANPortOperator(fullAddress);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化设备失败:{ex.Message}");
                _portOperatorBase = null;
                return false;
            }
            try
            {
                _portOperatorBase.Timeout = 2000;
                _portOperatorBase.Open();
            }
            catch (Exception ex)
            {
                _portOperatorBase = null;
                MessageBox.Show($"连接设备失败:{ex.Message}");
                return false;
            }

            return true;
        }
        public static string WriteCommand(string cmd)
        {
            if (!string.IsNullOrEmpty(cmd))
            {
                try
                {
                    if (_portOperatorBase == null) return "Command False";
                    _portOperatorBase.WriteLine(cmd);
                    string Write = $"{DateTime.Now:yy年MM月dd日 HH时mm分ss秒}  [Write]   {cmd}";
                    listLog.Add(Write);
                    Thread.Sleep(3);
#if DEBUG
                    Console.WriteLine(Write);
#endif
                    return cmd;
                }
                catch
                {
                    _portOperatorBase = null;
                }

            }

            return "False";
        }
        public static string ReadString()
        {
            Thread.Sleep(2);
            try
            {
                if (_portOperatorBase == null) return "Visa not connected False ";
                var result = _portOperatorBase.Read();
                string Read = $"{DateTime.Now:yy年MM月dd日 HH时mm分ss秒}  [Read]   {result}";
                listLog.Add(Read);
#if DEBUG
                Console.WriteLine(Read);
#endif
                return result;
            }
            catch (IOTimeoutException)
            {

            }
            catch (Exception)
            {

            }
            return "Error False";
        }

    }
    class AllDLL
    {
        static Dictionary<string, Type> dllType = new Dictionary<string, Type>();
        static Dictionary<string, object> MagicClassObject = new Dictionary<string, object>();
        static bool LoadDll(string keys)
        {
            try
            {
                if (dllType.ContainsKey(keys)) return true;
                string _namespace = "MerryDllFramework";
                string _class = "MerryDll";
                //根据路径读取Dll
                var ass = Assembly.LoadFrom($".\\AllDLL\\{keys}\\{keys}.dll");
                //根据抓的dll执行该命名空间及类
                dllType[keys] = ass.GetType($"{_namespace}.{_class}");
                //抓取该类构造函数并且抓取改方法
                MagicClassObject[keys] = dllType[keys].GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RT550调用{keys}.Dll失败 \r\n{ex}");
                dllType[keys] = null;
                MagicClassObject[keys] = null;
                return false;
            }

        }
        static string CallMethod(string keys, string methods, object[] parameter)
        {
            if (!LoadDll(keys)) return "Load Dll False";
            try
            {
                var mi = dllType[keys].GetMethod(methods);
                return mi.Invoke(MagicClassObject[keys], parameter).ToString();//方法有参数时，需要把null替换为参数的集合
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RT550调用{keys}方法失败{methods}\r\n{ex}");
                return "Invoke False";
            }
        }
    }
}

