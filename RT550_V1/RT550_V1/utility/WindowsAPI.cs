using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RT550_V1.utility
{

    class WindowsAPI
    {
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVar, int size, string INIpath);
        public static string GetValue(string section, string key, string _path = ".\\AllDLL\\MenuStrip\\AddDeploy.ini")
        {
            try
            {
                StringBuilder var = new StringBuilder(512);
                GetPrivateProfileString(section, key, "", var, 512, _path);
                return var.ToString().Trim();
            }
            catch
            {
                return "0";
            }

        }
    }
}
