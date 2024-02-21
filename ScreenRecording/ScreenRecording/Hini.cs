using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Airoha_ANC_Debug_Tool
{
    internal class Hini
    {
      
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVar, int size, string INIpath);
    
        public static string GetValue(string section, string key, string _path= @".\RecordingTool.ini")
        {

            StringBuilder var = new StringBuilder(512);
            int length = GetPrivateProfileString(section, key, "", var, 512, _path);
            if (length <= 0) SetValue(section, key, "", _path);
            return var.ToString().Trim();
        }

        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

       
        public static long SetValue(string section, string Key, string value, string _path = @".\RecordingTool.ini")
        {
            return WritePrivateProfileString(section, Key, value, _path);
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]     //可以没有此行
        public static extern bool WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);

        /// <summary>
        /// 获取所有节点名称(Section)
        /// </summary>
        /// <param name="lpszReturnBuffer">存放节点名称的内存地址,每个节点之间用\0分隔</param>
        /// <param name="nSize">内存大小(characters)</param>
        /// <param name="lpFileName">Ini文件</param>
        /// <returns>内容的实际长度,为0表示没有内容,为nSize-2表示内存大小不够</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, uint nSize, string lpFileName);
        /// <summary>
        /// 获取某个指定节点(Section)中所有KEY和Value
        /// </summary>
        /// <param name="lpAppName">节点名称</param>
        /// <param name="lpReturnedString">返回值的内存地址,每个之间用\0分隔</param>
        /// <param name="nSize">内存大小(characters)</param>
        /// <param name="lpFileName">Ini文件</param>
        /// <returns>内容的实际长度,为0表示没有内容,为nSize-2表示内存大小不够</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

        /// <summary>
        /// 读取INI文件中指定INI文件中的所有节点名称(Section)
        /// </summary>
        /// <param name="iniFile">Ini文件</param>
        /// <returns>所有节点,没有内容返回string[0]</returns>
        public static string[] INIGetAllSectionNames(string iniFile = @".\RecordingTool.ini")
        {
            uint MAX_BUFFER = 32767;    //默认为32767

            string[] sections = new string[0];      //返回值

            //申请内存
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));
            uint bytesReturned =  GetPrivateProfileSectionNames(pReturnedString, MAX_BUFFER, iniFile);
            if (bytesReturned != 0)
            {
                //读取指定内存的内容
                string local = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned).ToString();

                //每个节点之间用\0分隔,末尾有一个\0
                sections = local.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }

            //释放内存
            Marshal.FreeCoTaskMem(pReturnedString);

            return sections;
        }

        /// <summary>
        /// 获取INI文件中指定节点(Section)中的所有条目(key=value形式)
        /// </summary>
        /// <param name="iniFile">Ini文件</param>
        /// <param name="section">节点名称</param>
        /// <returns>指定节点中的所有项目,没有内容返回string[0]</returns>
        public static string[] INIGetAllItems( string section, string iniFile = @".\RecordingTool.ini")
        {
            //返回值形式为 key=value,例如 Color=Red
            uint MAX_BUFFER = 32767;    //默认为32767

            string[] items = new string[0];      //返回值

            //分配内存
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));

            uint bytesReturned = GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, iniFile);

            if (!(bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
            {

                string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);
                items = returnedString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }

            Marshal.FreeCoTaskMem(pReturnedString);     //释放内存

            return items;
        }
    }
}
