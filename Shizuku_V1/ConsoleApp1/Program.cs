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
            while (true)
            {
                Stopwatch sp = new Stopwatch();
                sp.Restart();
                while (true)
                {
                    string str = dll.Run(new object[] { "dllname=Shizuku_V1&method=CurrentToV&Round=3" });
                     Console.WriteLine($"{str}");
                    Thread.Sleep(100);

                }

            }



        }
    }
}
