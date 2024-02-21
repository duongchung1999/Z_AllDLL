using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ANC
{
    internal class InvorkDll
    {
        public static Dictionary<string, object> Config = new Dictionary<string, object>();
        public static Dictionary<string, Type> DllType_Namespace = new Dictionary<string, Type>();
        public static Dictionary<string, object> DllObject_Namespace = new Dictionary<string, object>();
        public static Dictionary<string, Assembly> DllAssembly = new Dictionary<string, Assembly>();

        public static string DirectoryPath = "";
        public static bool LoadDll(string keys, string assemblyFile, string _namespace)
        {
            try
            {
                //string _namespace = "MerryDllFramework";
                //string _class = "MerryDll";
                //根据路径读取Dll

                if (!DllAssembly.ContainsKey(keys))
                {
                    DllAssembly[keys] = Assembly.LoadFrom(assemblyFile);
                }

                //根据抓的dll执行该命名空间及类

                if (!DllType_Namespace.ContainsKey(_namespace))
                {
                    DllType_Namespace[_namespace] = DllAssembly[keys].GetType(_namespace);
                }
                //抓取该类构造函数并且抓取改方法
                if (!DllObject_Namespace.ContainsKey(_namespace))
                {
                    DllObject_Namespace[_namespace] = DllType_Namespace[_namespace].GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                    var mi = DllType_Namespace[_namespace].GetMethod("Interface");
                    if (mi != null)
                    {
                        mi.Invoke(DllObject_Namespace[_namespace], new object[] { Config }).ToString();//方法有参数时，需要把null替换为参数的集合
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"动态引用Dll失败{keys} \r\n{ex}");
                return false;
            }

        }
        public static string CallMethod(string keys, string methods, object[] parameter)
        {
            if (!LoadDll(keys, "", "MerryDllFramework.MerryDll")) return "Load Dll False";
            try
            {
                var mi = DllType_Namespace[keys].GetMethod(methods);
                return mi.Invoke(DllObject_Namespace[keys], parameter).ToString();//方法有参数时，需要把null替换为参数的集合
            }
            catch (Exception ex)
            {
                return "Invoke False";
            }
        }
    }
}
