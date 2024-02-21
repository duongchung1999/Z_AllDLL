using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    internal class Program
    {
        static void Main(string[] args)
        {

            MerryDllFramework.MerryDll dll = new MerryDllFramework.MerryDll();
            dll.Interface(new Dictionary<string, object> { { "_2303SComport", "COM38" } });
            dll.Run(new object[] { "dllname=_2303&method=SET&电压=8&电流=2&通道=1" });

        }
    }
}
