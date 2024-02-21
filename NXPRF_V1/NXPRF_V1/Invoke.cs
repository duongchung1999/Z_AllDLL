using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NXPRF_V1.API
{
    public class Invoke
    {
        public Invoke(Dictionary<string, object> Config)
            => this.Config = Config;

        public Invoke()
        {

        }
        public Dictionary<string, object> Config = new Dictionary<string, object>();
        Dictionary<string, Type> dllType = new Dictionary<string, Type>();
        Dictionary<string, object> MagicClassObject = new Dictionary<string, object>();
        public bool LoadDll(string keys)
        {
            try
            {
                if (dllType.ContainsKey(keys)) return true;
                string _namespace = "MerryDllFramework";
                string _class = "MerryDll";
                string dllPath = $".\\AllDLL\\{keys}\\{keys}.dll";
                //根据路径读取Dll
                if (!File.Exists(dllPath))
                {
                    MessageBox.Show($"未找到Dll，{dllPath}", "QCCRF提示", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    return false;
                }
                var ass = Assembly.LoadFrom(dllPath);
                //根据抓的dll执行该命名空间及类
                dllType[keys] = ass.GetType($"{_namespace}.{_class}");
                //抓取该类构造函数并且抓取改方法
                MagicClassObject[keys] = dllType[keys].GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                if (MagicClassObject[keys] == null)
                {
                    MagicClassObject.Remove(keys);
                    MessageBox.Show($"初始化{keys}，运行构造函数失败", "QCCRF提示", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    return false;
                }
                var mi = dllType[keys].GetMethod("Interface");
                mi.Invoke(MagicClassObject[keys], new object[] { Config });//方法有参数时，需要把null替换为参数的集合
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"调用{keys}.Dll失败 \r\n{ex}");
                dllType[keys] = null;
                MagicClassObject[keys] = null;
                return false;
            }

        }
        public string CallMethod(string cmd, Dictionary<string, object> onceConfig)
        {
            Thread.Sleep(300);
            string keys = cmd.Split('&')[0].Split('=')[1];
            if (onceConfig == null) onceConfig = new Dictionary<string, object>();
            if (!LoadDll(keys)) return "Load Dll False";
            try
            {
                var mi = dllType[keys].GetMethod("Run");
                return mi.Invoke(MagicClassObject[keys], new object[] { new object[] { cmd, onceConfig } }).ToString();//方法有参数时，需要把null替换为参数的集合
            }
            catch (Exception ex)
            {
                MessageBox.Show($"调用{keys}方法失败{"Run"}:\r\nCMD{cmd}\r\n{ex}");
                return "Invoke False";
            }
        }
    }
}
