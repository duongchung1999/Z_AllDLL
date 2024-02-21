using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
                {"SN","TE_BZP" },
                {"BitAddress","c18400c28435" }
            });
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.WriteLine(dll.Run(new object[] { $"dllname=CRY574&延时=ComPortConnect&=True&=COM201" }));
            Console.WriteLine(dll.Run(new object[] { $"dllname=CRY574&延时=ComPortConnect&=True&=COM201" }));

            sw.Restart();
            Console.WriteLine(dll.Run(new object[] { $"dllname=CRY574&延时=ComPortEnterA2DP" }));
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Restart();

            Console.WriteLine(dll.Run(new object[] { $"dllname=CRY574&延时=ComPortEnterHFP" }));
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Restart();

            Console.WriteLine(dll.Run(new object[] { $"dllname=CRY574&延时=ComPortDisconnect" }));
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }
}
