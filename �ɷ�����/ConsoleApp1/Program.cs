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
            MerryDllFramework.MerryDll merry = new MerryDllFramework.MerryDll();
          string ss=  merry.Run(new object[] { "dllname=HanOpticSens&CMD=cd_mm&Com=COM22&channel=3" });
        }
    }
}
