using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static ICSharpCode.SharpZipLib.Zip.FastZip;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;

namespace Compress
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string[] dllPaths = File.ReadAllLines(Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.txt")[0]);
            string mainProgramNme = dllPaths[0];
            string mainProgramPath = dllPaths[1];
            string mainProgramDirectory = dllPaths[1];

            for (int i = 4; i > 0; i--)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"等待更新{i}");

            }
            foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                if (thisProc.ProcessName.Contains(Path.GetFileNameWithoutExtension(mainProgramNme)))
                    thisProc.Kill();
            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (dllPaths[0].Split('=')[0] == "True")
            {
                if (!Compress($"{exePath}", dllPaths[0].Split('=')[1]))
                    MessageBox.Show("平台程序解压失败", "更新提示");
                DirectoryInfo Directory = new DirectoryInfo($"{exePath}\\{Path.GetFileNameWithoutExtension(dllPaths[0].Split('=')[1])}");


                var files = Directory.GetFiles();
                var DirectoryFiles = Directory.GetDirectories();

                foreach (var item in files)
                {

                    string pathName = $"{dllPaths[2]}\\{Path.GetFileName(item.FullName)}";
                    try
                    {
                        File.Copy(item.FullName, pathName, true);
                        Console.WriteLine($"复制路径：{item.FullName}");
                        Console.WriteLine($"粘贴路径：{pathName}");
                    }
                    catch (Exception EX)
                    {
                        Console.WriteLine($"{Path.GetFileName(item.FullName)}主文件更新失败");
                        Console.WriteLine($"{EX}");
                    }



                }
                foreach (var item in DirectoryFiles)
                {
                    CopyFolder(item.FullName, dllPaths[2]);
                }



            }

            for (int i = 3; i < dllPaths.Length; i += 3)
            {
                string zidPath = dllPaths[i + 1];
                string FileName = Path.GetFileNameWithoutExtension(dllPaths[i + 1]);
                Compress($@"{exePath}", zidPath);
                CopyFolderAlldll($@"{exePath}\{FileName}", dllPaths[i + 2]);
            }
            for (int i = 2; i > 0; i--)
            {


                Thread.Sleep(1000);
                Console.WriteLine($"等待程序重启{i}");

            }
            Process.Start(mainProgramPath);


        }
        public static FastZip fz;

        /// 解压Zip
        /// </summary>
        /// <param name="DirPath">解压后存放路径</param>
        /// <param name="ZipPath">Zip的存放路径</param>
        /// <param name="ZipPWD">解压密码（null代表无密码）</param>
        /// <returns></returns>
        public static bool Compress(string DirPath, string ZipPath, string ZipPWD = null)
        {
            try
            {



                if (fz == null) fz = new FastZip();
                string DeletePath = $"{DirPath}\\{Path.GetFileNameWithoutExtension(ZipPath)}";
                Console.WriteLine($"删除的文件夹{DeletePath}");
                Console.WriteLine($"压缩包路径{ZipPath}");

                Console.WriteLine($"解压的路径{DirPath}");

                if (Directory.Exists(DeletePath)) Directory.Delete(DeletePath, true);
                Directory.CreateDirectory(DeletePath);
                fz.Password = ZipPWD;
                // fz.ExtractZip( ZipPath, DirPath, ZipPWD);
                fz.ExtractZip(ZipPath, DirPath, Overwrite.Never, null, ZipPWD, null, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                File.AppendAllText($@"错误信息{DateTime.Now:MM_dd}.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                return false;
            }
        }

        /// <summary>
        /// 复制文件夹及文件
        /// </summary>
        /// <param name="sourceFolder">原文件路径</param>
        /// <param name="destFolder">目标文件路径</param>
        /// <returns></returns>
        public static bool CopyFolder(string sourceFolder, string destFolder)
        {
            try
            {
                string folderName = Path.GetFileName(sourceFolder);
                string destfolderdir = Path.Combine(destFolder, folderName);
                string[] filenames = Directory.GetFileSystemEntries(sourceFolder);
                foreach (string file in filenames)// 遍历所有的文件和目录
                {

                    if (Path.GetExtension(file).Contains("ini"))
                        continue;
                    if (Directory.Exists(file))
                    {
                        string currentdir = Path.Combine(destfolderdir, Path.GetFileName(file));
                        if (!Directory.Exists(currentdir))
                        {
                            Directory.CreateDirectory(currentdir);
                        }
                        CopyFolder(file, destfolderdir);
                    }
                    else
                    {
                        string FileName = Path.GetFileName(file);
                        string srcfileName = Path.Combine(destfolderdir, FileName);
                        if (!Directory.Exists(destfolderdir))
                        {
                            Directory.CreateDirectory(destfolderdir);
                        }
                        File.Copy(file, srcfileName, true);
                        Console.WriteLine($"复制路径：{file}");
                        Console.WriteLine($"粘贴路径：{srcfileName}");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{destFolder}：更新失败");
                return false;
            }

        }

        public static bool CopyFolderAlldll(string sourceFolder, string destFolder)
        {
            try
            {
                string folderName = Path.GetFileName(sourceFolder);
                string destfolderdir = Path.Combine(destFolder, folderName);
                string[] filenames = Directory.GetFileSystemEntries(sourceFolder);
                foreach (string file in filenames)// 遍历所有的文件和目录
                {


                    if (Directory.Exists(file))
                    {
                        string currentdir = Path.Combine(destfolderdir, Path.GetFileName(file));
                        if (!Directory.Exists(currentdir))
                        {
                            Directory.CreateDirectory(currentdir);
                        }
                        CopyFolder(file, destfolderdir);
                    }
                    else
                    {


                        string FileName = Path.GetFileName(file);
                        string Extension = Path.GetExtension(file);
                        string srcfileName = Path.Combine(destfolderdir, FileName);

                        if (Extension.Contains("ini") || Extension.Contains("txt"))
                        {
                            if (File.Exists(srcfileName))
                                continue;
                        }
                        if (!Directory.Exists(destfolderdir))
                        {
                            Directory.CreateDirectory(destfolderdir);
                        }
                        File.Copy(file, srcfileName, true);
                        Console.WriteLine($"复制路径：{file}");
                        Console.WriteLine($"粘贴路径：{srcfileName}");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{destFolder}：更新失败");
                return false;
            }

        }



    }
}
