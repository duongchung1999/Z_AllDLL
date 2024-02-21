using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QCCRF
{
    internal class Invoke
    {
        public Invoke(Dictionary<string, object> Config)
            => this.Config = Config;

        public Invoke()
        {

        }
        public Dictionary<string, object> Config = new Dictionary<string, object>();
        static Dictionary<string, Type> dllType = new Dictionary<string, Type>();
        static Dictionary<string, object> MagicClassObject = new Dictionary<string, object>();
        public bool LoadDll(string keys)
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
        public string CallMethod(string cmd)
        {
            string keys = cmd.Split('&')[0].Split('=')[1];
            if (!LoadDll(keys)) return "Load Dll False";
            try
            {
                var mi = dllType[keys].GetMethod("Run");
                return mi.Invoke(MagicClassObject[keys], new object[] { new object[] { cmd } }).ToString();//方法有参数时，需要把null替换为参数的集合
            }
            catch (Exception ex)
            {
                MessageBox.Show($"调用{keys}方法失败{"Run"}\r\n{ex}");
                return "Invoke False";
            }
        }
    }
}
