using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace MerryDllFramework
{
    class MES
    {
        const string path = @".\AllDLL\MES\SajetConnect.dll";
        [DllImport(path, EntryPoint = "SajetTransStart")]
        public static extern bool SajetTransStart();

        [DllImport(path, EntryPoint = "SajetTransClose")]
        public static extern bool SajetTransClose();

        //[DllImport("SajetConnect.dll", EntryPoint = "SajetTransData")]
        //public static extern bool SajetTransData(short f_iCommandNo, ref byte f_pData, ref int f_pLen);

        //[DllImport("SajetConnect.dll", EntryPoint = "SajetTransData")]
        //public static extern bool SajetTransData(short f_iCommandNo, ref string f_pData, ref int f_pLen);

        [DllImport(path, EntryPoint = "SajetTransData", CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern bool SajetTransData(int f_iCommandNo, byte* f_pData, int* f_pLen);
    }

    class MES_click
    {
        private void Write_LOG(string stytle, string data)
        {
            string test_time = DateTime.Now.ToString("yyyy-MM-dd-hh");
            string test_DATA = DateTime.Now.ToString("yyyy-MM-dd");
            string file_path = @"E:\" + "ATE_PLC_log" + "\\" + test_DATA + "\\";
            if (System.IO.Directory.Exists(file_path))
            { }
            else
                System.IO.Directory.CreateDirectory(file_path);
            file_path = file_path + test_time + ".txt";
            StreamWriter sw_log = new StreamWriter(file_path, true);
            sw_log.WriteLine(stytle + "," + data);
            sw_log.Close();
        }
    }
}
