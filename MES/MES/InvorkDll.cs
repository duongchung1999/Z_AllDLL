using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MES
{
    internal class InvorkDll
    {
        public static Dictionary<string, object> Config = new Dictionary<string, object>();
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
                var mi = dllType[keys].GetMethod("Interface");
                mi.Invoke(MagicClassObject[keys], new object[] { Config }).ToString();//方法有参数时，需要把null替换为参数的集合
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询数据库料号报错{keys}.Dll失败 \r\n{ex}");
                dllType[keys] = null;
                MagicClassObject[keys] = null;
                return false;
            }

        }
        public static string CallMethod(string keys, string methods, object[] parameter)
        {
            if (!LoadDll(keys)) return "Load Dll False";
            try
            {
                var mi = dllType[keys].GetMethod(methods);
                return mi.Invoke(MagicClassObject[keys], parameter).ToString();//方法有参数时，需要把null替换为参数的集合
            }
            catch (Exception ex)
            {
                return "Invoke False";
            }
        }
    }
}
