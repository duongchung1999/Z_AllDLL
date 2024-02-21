using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest
{
    internal static class Analysis
    {
       /// <summary>
       /// 切割指令
       /// </summary>
       /// <param name="str"></param>
       /// <returns></returns>
        public static string[] CMDSplit(this string str)
        {
            //MemoryList.Get('Result', 'left_passive_margin_result')
            int indexof = str.IndexOf('(');
            int END = str.LastIndexOf(')');
            string CMDSection = str.Substring(indexof + 1, END - indexof - 1);
            int CmdKey1Index = CMDSection.IndexOf("'", 0);
            int CmdKey2Index = CMDSection.IndexOf("'", CmdKey1Index + 1);
            int CmdValue1Index = CMDSection.IndexOf("'", CmdKey2Index + 1);
            int CmdValue2Index = CMDSection.IndexOf("'", CmdValue1Index + 1);
            string CmdHead = str.Substring(0, indexof);
            string Key = CMDSection.Substring(CmdKey1Index + 1, CmdKey2Index - CmdKey1Index - 1);
            string Value = CMDSection.Substring(CmdValue1Index + 1, CmdValue2Index - CmdValue1Index - 1);
            return new string[] { CmdHead, Key, Value };
        }


    }
}
