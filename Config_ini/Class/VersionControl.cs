using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
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
using static Config_ini.Class.HttpPostAPI;

namespace MerryDllFramework
{
    /// <summary>
    /// 这个类用于下载接口信息的类
    /// </summary>
    static class VersionControl
    {
        /// <summary>
        /// 检测机型的版本信息
        /// </summary>
        /// <param name="TypeName"></param>
        /// <param name="ModeVersion"></param>
        /// <returns></returns>
        public static bool CheckTypeNameDllVersion(string TypeName, DateTime ModeVersion, out string Error)
        {

            bool flag = false;
            if (!HttpPost("http://10.175.5.59:20005/api/GetALLModelControl", "", "POST", out string APIDllInfo, out Error))
                return false;

            DateTime APIVersion = new DateTime();
            JArray objects = JArray.Parse(APIDllInfo);
            foreach (JToken obj in objects)
            {
                string model_Nmae = obj["model_name"].ToString().Replace(".dll", "");
                if (model_Nmae.Equals(TypeName))
                {
                    APIVersion = DateTime.Parse(obj["model_createtime"].ToString());
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                Error = $"{TypeName}机型未上传记录，程序即将关闭 Close";
                return false;
            }
            TimeSpan time = (APIVersion - ModeVersion);
            return Math.Abs(time.TotalSeconds) < 60;

        }

        /// <summary>
        /// 检查所有机型的版本信息
        /// </summary>
        /// <param name="AllDLL"></param>
        /// <returns></returns>
        public static Dictionary<string, bool> CheckAllDLLVersion(Dictionary<string, DateTime> AllDLL)
        {
            string APIDllInfo = HttpPost("http://10.175.5.59:20005/api/GetALLModelALLDLL", "", "POST");
            string strs = "";
            Dictionary<string, bool> UpLoadLog = new Dictionary<string, bool>();
            Dictionary<string, DateTime> UpLoadTime = new Dictionary<string, DateTime>();
            Dictionary<string, bool> updateResult = new Dictionary<string, bool>();

            JArray objects = JArray.Parse(APIDllInfo);
            foreach (var item in AllDLL)
            {
                UpLoadLog[item.Key] = false;
                updateResult[item.Key] = false;
                foreach (JToken obj in objects)
                {
                    if (obj["alldll_name"].ToString().Contains(item.Key))
                    {
                        string alldll_createTime = obj["alldll_createtime"].ToString();
                        UpLoadTime[item.Key] = DateTime.Parse(alldll_createTime);
                        UpLoadLog[item.Key] = true;
                        break;
                    }
                }
            }
            if (UpLoadLog.ContainsValue(false))
                foreach (var item in UpLoadLog)
                    if (!item.Value)
                        strs += $"{item.Key}测试组件没有上传记录\r\n";
            if (UpLoadLog.ContainsValue(false))
            {
                MessageBox.Show(strs);
                Process.GetCurrentProcess().Kill();
            }
            foreach (var item in AllDLL)
            {
                TimeSpan time = item.Value - UpLoadTime[item.Key];
                updateResult[item.Key] = Math.Abs(time.TotalSeconds) < 60;
            }

            return updateResult;
        }
        /// <summary>
        /// 获取系统Alldll的版本信息
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, DateTime> GetAllDllInfo()
        {
            string APIDllInfo = HttpPost("http://10.175.5.59:20005/api/GetALLModelALLDLL", "", "POST");
            Dictionary<string, DateTime> UpLoadTime = new Dictionary<string, DateTime>();
            JArray objects = JArray.Parse(APIDllInfo);
            foreach (JToken obj in objects)
            {
                string alldll_createTime = obj["alldll_createtime"].ToString();
                UpLoadTime[$"{obj["alldll_name"]}"] = DateTime.Parse(alldll_createTime);
            }
            return UpLoadTime;
        }

        /// <summary>
        /// 用于下载机型Dll的接口
        /// </summary>
        /// <param name="TypeName"></param>
        /// <param name="DirectoryPath"></param>
        /// <returns></returns>
        public static bool DownloadTestDll(string TypeName, string DirectoryPath, out string Error)
        {

            string DownLoadPath = $"{DirectoryPath}\\AllDll\\Download\\{TypeName}.zip";

            string strContent = "{\"TestName\":\"" + "TestItem" + "\",\"DownloadName\":\"" + TypeName + "\"}";
            if (!Directory.Exists(Path.GetDirectoryName(DownLoadPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(DownLoadPath));
            if (!HttpPost("http://10.175.5.59:20005/api/PostDownloadZIP", strContent, "POST", DownLoadPath, out Error))
                return false;
            if (Compress($@"{DirectoryPath}\AllDll\\Download", DownLoadPath, null))
                return true;
            Error = "更新机型模块解压失败";
            return false;

        }
        /// <summary>
        /// 用于下载工程模式机型Dll的接口
        /// </summary>
        /// <param name="TypeName"></param>
        /// <param name="DirectoryPath"></param>
        /// <returns></returns>
        public static bool DownloadEngineerTestDll(string TypeName, string DirectoryPath, out string Error)
        {

            string DownLoadPath = $"{DirectoryPath}\\AllDll\\Download\\{TypeName}.zip";

            string strContent = "{\"TestName\":\"" + "EngineeringMode" + "\",\"DownloadName\":\"" + TypeName + "\"}";
            if (!Directory.Exists(Path.GetDirectoryName(DownLoadPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(DownLoadPath));
            if (!HttpPost("http://10.175.5.59:20005/api/PostDownloadZIP", strContent, "POST", DownLoadPath, out Error))
                return false;
            return true;
        }
        /// <summary>
        /// 用于下载AllDLL的接口，主要是下载
        /// </summary>
        /// <param name="AllDLLname"></param>
        /// <param name="DirectoryPath"></param>
        /// <returns></returns>
        public static bool DownloadAllDLL(string AllDLLname, string DirectoryPath)
        {

            string strContent = "{\"TestName\":\"" + "ALLDLL" + "\",\"DownloadName\":\"" + AllDLLname + "\"}";
            string DownLoadPath = $"{DirectoryPath}\\{AllDLLname}.zip";
            Directory.CreateDirectory(DirectoryPath);
            string ErrorMessage = "";
            for (int i = 0; i < 2; i++)
            {
                if (HttpPost("http://10.175.5.59:20005/api/PostDownloadZIP", strContent, "POST", DownLoadPath, out ErrorMessage))
                    return true;
            }
            MessageBox.Show($"Download:{AllDLLname} 失败+\r\n{ErrorMessage}");
            return false;

        }
        /// <summary>
        /// 用于下载测试项目的接口
        /// </summary>
        /// <param name="TypeName"></param>
        /// <param name="Station"></param>
        /// <param name="DirectoryPath"></param>
        /// <returns></returns>
        public static bool DownloadTestTxt(string TypeName, string Station, string DirectoryPath)
        {


            string DownLoadPath = $"{DirectoryPath}\\TestItem\\{TypeName}\\{Station}.txt";
            string strContent = "{\"TestName\":\"" + "Station" + "\",\"DownloadName\":\"" + TypeName + "\",\"StationAlias\":\"" + Station + "\"}";
            return (HttpPost("http://10.175.5.59:20005/api/PostDownloadZIP", strContent, "POST", DownLoadPath, out string Error));

        }
        /// <summary>
        /// 检测程序文件
        /// </summary>
        /// <param name="P_本地包路径"></param>
        /// <param name="P_测试包路径"></param>
        /// <param name="UpdataPath"></param>
        /// <returns></returns>
        public static bool UpdateEachFile(string P_本地包路径, string P_更新包路径)
        {
            try
            {

                //  获取文件夹名字
                string 获取文件夹名称 = Path.GetFileName(P_更新包路径);
                //判断本地有没有文件夹
                if (!Directory.Exists(P_本地包路径))
                    //没有就创建
                    Directory.CreateDirectory(P_本地包路径);
                string[] F_本地包 = Directory.GetFileSystemEntries(P_本地包路径);
                string[] F_更新包 = Directory.GetFileSystemEntries(P_更新包路径);

                Dictionary<string, DateTime> 本地包文件属性s = new Dictionary<string, DateTime>();
                foreach (var item in F_本地包)
                {
                    FileInfo fi = new FileInfo(item);
                    本地包文件属性s.Add(fi.Name, fi.LastWriteTime);
                }
                foreach (string 更新包文件 in F_更新包)
                {
                    //判断是文件夹
                    if (Directory.Exists(更新包文件))
                    {

                        //是文件夹就获取文件夹名称
                        string 更新包文件夹名称 = Path.GetFileName(更新包文件);
                        //组合起来看看本地有没有这个文件夹
                        string 本地包文件夹路径 = Path.Combine(P_本地包路径, Path.GetFileName(更新包文件夹名称));
                        //本地有这个文件夹就递归更新
                        if (Directory.Exists(本地包文件夹路径))
                        {
                            UpdateEachFile(本地包文件夹路径, 更新包文件);
                        }
                        else
                        {
                            //没有这个文件夹就直接将文件夹复制过来
                            if (!CopyFolder(更新包文件, P_本地包路径))
                                return false;
                        }
                    }
                    //是文件
                    else
                    {
                        //更新包的文件属性
                        FileInfo 更新包文件属性 = new FileInfo(更新包文件);
                        //文件需要粘贴的路径
                        string paste = Path.Combine(P_本地包路径, Path.GetFileName(更新包文件属性.Name));

                        //更本地存在新包文件一样的文件
                        if (本地包文件属性s.ContainsKey(更新包文件属性.Name))
                        {
                            //判断是ini和txt直接出去不修改本地参数文件。
                            if (更新包文件属性.Extension.Equals(".txt") || 更新包文件属性.Extension.Equals(".ini"))
                                continue;

                            //如果不是本地参数文件
                            if (本地包文件属性s[更新包文件属性.Name] != 更新包文件属性.LastWriteTime)
                            {
                                Console.WriteLine($"更新包：{更新包文件属性.Name}={更新包文件属性.LastWriteTime}||本地包：{更新包文件属性.Name}={本地包文件属性s[更新包文件属性.Name]}");
                                File.Copy(更新包文件, paste, true);
                            }
                        }
                        else
                        {
                            //如果本地没有更新包的文件就直接粘贴
                            File.Copy(更新包文件, paste, true);
                        }
                    }



                }
                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
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
                        string srcfileName = Path.Combine(destfolderdir, Path.GetFileName(file));
                        if (!Directory.Exists(destfolderdir))
                        {
                            Directory.CreateDirectory(destfolderdir);
                        }
                        File.Copy(file, srcfileName, true);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }
        /// <summary>
        /// 实例化FastZip
        /// </summary>
        /// <summary>
        public static FastZip fz;
        /// 解压Zip
        /// </summary>
        /// <param name="DirPath">解压后存放路径</param>
        /// <param name="ZipPath">Zip的存放路径</param>
        /// <param name="ZipPWD">解压密码（null代表无密码）</param>
        /// <returns></returns>
        public static bool Compress(string DirPath, string ZipPath, string ZipPWD)
        {
            try
            {
                if (fz == null) fz = new FastZip();
                string DeletePath = $"{DirPath}\\{Path.GetFileNameWithoutExtension(ZipPath)}";
                Directory.CreateDirectory(DeletePath);
                fz.Password = ZipPWD;
                fz.ExtractZip(ZipPath, DirPath, Overwrite.Never, null, ZipPWD, null, true);
                return true;
            }
            catch (Exception ex)
            {

                File.AppendAllText($@".\Log\错误信息{DateTime.Now:MM_dd}.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                return false;
            }
        }
    }
}
