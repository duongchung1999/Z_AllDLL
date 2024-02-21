using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {

            MerryDllFramework.MerryDll dll = new MerryDllFramework.MerryDll();
            dll.Interface(new Dictionary<string, object>()
            {
                { "SN","TE_BZP"}

            });
            Console.WriteLine(dll.Run(new object[] { "dllname=BT2200&method=SendBT2200CMD&method=>SPP_CONN" }));

            Console.WriteLine(dll.Run(new object[] { "dllname=BT2200&method=A2DP" }));
            Console.WriteLine(dll.Run(new object[] { "dllname=BT2200&method=HFP" }));

            Console.WriteLine(dll.Run(new object[] { "dllname=BT2200&method=SPPSendCMD&method=05 5A 05 00 06 0E 00 0B 01&method=05 5B 08" }));
            Console.WriteLine(dll.Run(new object[] { "dllname=BT2200&method=SPPSendCMD&method=05 5A 07 00 06 0E 00 0A 01 00 01&method=05 5B 08 00" }));
            Console.WriteLine(dll.Run(new object[] { "dllname=BT2200&method=SendBT2200CMD&method=>DISC" }));

        }
    }
}
