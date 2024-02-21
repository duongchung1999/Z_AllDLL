using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DownloadAPI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DownloadTestDll("HDT647", out string ERROR);
        }
        public static bool DownloadTestDll(string TypeName, out string Error)
        {

            string DownLoadPath = $@".\{TypeName}_160.zip";
            //string linkAPI = "http://10.55.22.160:20005/api/PostDownloadZIP";
            string linkAPI = "http://10.175.5.59:20005/api/PostDownloadZIP";
            string strContent = "{\"TestName\":\"" + "TestItem" + "\",\"DownloadName\":\"" + TypeName + "\"}";
            if (!Directory.Exists(Path.GetDirectoryName(DownLoadPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(DownLoadPath));
            if (!HttpPost(linkAPI, strContent, "POST", DownLoadPath, out Error))
                return false;

            Error = "更新机型模块解压失败";
            return false;

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
