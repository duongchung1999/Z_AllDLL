using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ANC
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
         

            string dirPaht = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (File.Exists($"{dirPaht}\\Path.txt"))
            {
                string[] paths = File.ReadAllLines($"{dirPaht}\\Path.txt");
                if (paths.Length >= 2)
                    Form1.AllDllPath = $@"{paths[1]}";
                Form1.ConfigPath = $@"{paths[0]}\ANC_Parameter.ini";

            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
