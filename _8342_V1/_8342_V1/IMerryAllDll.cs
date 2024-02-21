

using System;
using System.Collections.Generic;

namespace MerryDllFramework
{
    internal interface IMerryAllDll
    {
        string Run(object[] Command);
        //主程序所使用的变量
        object Interface(Dictionary<string, object> keys);
        string[] GetDllInfo();

    }
}
