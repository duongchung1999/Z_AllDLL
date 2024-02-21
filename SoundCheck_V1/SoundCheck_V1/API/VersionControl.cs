using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using SoundCheck_V1.TemplateANC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ICSharpCode.SharpZipLib.Zip.FastZip;
using static MerryDllFramework.CurvesInfo;

namespace SoundCheck_V1.API
{
    internal class VersionControl
    {

        public static string URL_GetQueryALLSoundCheckSQC = INIClass.GetValue("URL", "GetQueryALLSoundCheckSQC", "http://10.55.2.25:20005/api/GetQueryALLSoundCheckSQC", $"{SoundCheck_V1.TemplateANC.Method.dllPath}/URL_Config.ini");
        public static string URL_PostDownloadZIP = INIClass.GetValue("URL", "PostDownloadZIP", "http://10.55.2.25:20005/api/PostDownloadZIP", $"{SoundCheck_V1.TemplateANC.Method.dllPath}/URL_Config.ini");
        public static string URL_UploadLGAcousticData = INIClass.GetValue("URL", "UploadLGAcousticData", "http://10.55.2.25:8088/api/TestDataUpload/UploadLGAcousticData", $"{SoundCheck_V1.TemplateANC.Merry_CalibrationDataFF.dllPath}/URL_Config.ini");


        public static string CheckSequenceVersion(FileInfo info, out bool isDownload)
        {
            isDownload = false;
            //"http://10.55.2.25:20005/api/GetQueryALLSoundCheckSQC";
            //"http://10.55.2.25:20005/api/PostDownloadZIP"

            string response = PostClass.HttpPost(URL_GetQueryALLSoundCheckSQC, "", "GET", out string Error);
            if (response.Contains("False"))
            {
                MessageBox.Show($"GetQueryALLSoundCheckSQC HttpPost Response False\r\n {Error}", "Sound Check_V1 提示");
                MessageBox.Show("");
                return $"GetQueryALLSoundCheckSQC HttpPost Response False";

            }

            DateTime APIVersion = new DateTime();
            JArray objects = JArray.Parse(response);
            bool flag = false;
            foreach (JToken obj in objects)
            {
                string model_Nmae = obj["modelsqc_name"].ToString();
                if (model_Nmae.Equals(info.Name))
                {
                    flag = true;
                    APIVersion = DateTime.Parse(obj["sqc_cratetime"].ToString());

                    TimeSpan time = (APIVersion - info.LastWriteTime);
                    bool result = Math.Abs(time.TotalSeconds) < 60;
                    if (result)
                    {
                        return "True";
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (!flag)
            {
                MessageBox.Show($"未在版本管控系统上传SQC文件 未找到{info.Name}", "Sound Check_V1 提示");
                return $"Not Found Sqc {info.Name} Info  False";
            }
            string name = Path.GetFileNameWithoutExtension(info.Name);
            string DownLoadPath = $"{Environment.CurrentDirectory}\\AllDll\\Download\\{name}.zip";

            string strContent = "{\"TestName\":\"" + "Soundcheck_SQC" + "\",\"DownloadName\":\"" + name + "\"}";
            if (!Directory.Exists(Path.GetDirectoryName(DownLoadPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(DownLoadPath));
            if (!PostClass.HttpPost(URL_PostDownloadZIP, strContent, "POST", DownLoadPath, out Error))
                return $"PostDownloadZIP HttpPost Response False";
            if (!Compress($@"{Environment.CurrentDirectory}\AllDll\Download", DownLoadPath, null))
                return $"Compress False";
            if (!UpdateEachFile($"{info.Directory}", $"{Environment.CurrentDirectory}\\AllDll\\Download\\{name}"))
                return "UpdateEachFile False";
            isDownload = true;
            return "True";
        }
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
        /// <summary>
        /// 检测程序文件
        /// </summary>
        /// <param name="P_本地包路径"></param>
        /// <param name="P_更新包路径"></param>
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
                            if (更新包文件属性.Extension.Equals(".dat") || 更新包文件属性.Extension.Equals(".txt") || 更新包文件属性.Extension.Equals(".ini"))
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
    }
    class PostClass
    {
        public static string HttpPost(string url, string Writedata, string Method, out string error)
        {
            error = "";
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                //参数类型，这里是json类型
                //还有别的类型如"application/x-www-form-urlencoded"，不过我没用过(逃
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "text/plain";


                //设置请求类型 
                httpWebRequest.Method = Method;
                //设置超时时间
                httpWebRequest.Timeout = 4000;
                //将参数写入请求地址中
                if (Method == "Post")
                {
                    //字符串转换为字节码
                    byte[] bs = Encoding.UTF8.GetBytes(Writedata);
                    //参数数据长度
                    httpWebRequest.ContentLength = bs.Length;
                    httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                }

                //发送请求
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //读取返回数据
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                httpWebRequest.Abort();
                return responseContent;
            }
            catch (Exception ex)
            {
                error = ex.Message.Contains("無法連接至遠端伺服器") ? "网络断开连接，请连接网络" : ex.ToString();
                File.AppendAllText($@".\Log\错误信息{DateTime.Now:MM_dd}.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                return $"{ex} False";
            }
        }

        /// <summary>
        /// 调用API 版本控制系统
        /// </summary>
        /// <param name="url"></param>
        /// <param name="TestName"></param>
        /// <param name="DownloadName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool HttpPost(string url, string Writedata, string Method, string Path, out string Error)
        {
            Error = "";
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(Writedata);
                //参数类型，这里是json类型
                //还有别的类型如"application/x-www-form-urlencoded"，不过我没用过(逃
                httpWebRequest.ContentType = "application/json";
                //参数数据长度
                httpWebRequest.ContentLength = bs.Length;
                //设置请求类型
                httpWebRequest.Method = Method;
                //设置超时时间
                httpWebRequest.Timeout = 20000;
                //将参数写入请求地址中
                httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                //发送请求
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //流对象使用完后自动关闭
                using (Stream stream = httpWebResponse.GetResponseStream())
                {
                    //文件流，流信息读到文件流中，读完关闭
                    using (FileStream fs = File.Create(Path))
                    {
                        //建立字节组，并设置它的大小是多少字节
                        byte[] bytes = new byte[102400];
                        int n = 1;
                        while (n > 0)
                        {
                            //一次从流中读多少字节，并把值赋给Ｎ，当读完后，Ｎ为０,并退出循环
                            n = stream.Read(bytes, 0, 10240);
                            fs.Write(bytes, 0, n); //将指定字节的流信息写入文件流中
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Error = ex.ToString();
                File.AppendAllText($@".\Log\错误信息{DateTime.Now:MM_dd}.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                return false;
            }
        }
    }

}
