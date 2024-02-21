

using System;
using System.Collections.Generic;

namespace MerryDllFramework
{
    internal interface IMerryAllDll
    {


        //主程序所使用的变量
        string Interface(Dictionary<string, object> keys);
        string[] GetDllInfo();

    }
}
