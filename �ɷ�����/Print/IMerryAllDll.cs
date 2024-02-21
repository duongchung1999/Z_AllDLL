

using System;
using System.Collections.Generic;

namespace MerryDllFramework
{
    internal interface IMerryAllDll
    {

        /// <summary>
        /// 调用内部方法
        /// </summary>
        /// <param name="message">指令，决定调用哪个方法</param>
        /// <returns>方法调用后回传值</returns>
        string Run(object[] Command);
        //主程序所使用的变量
        void Interface(Dictionary<string, object> keys);
        string[] GetDllInfo();

    }
}
