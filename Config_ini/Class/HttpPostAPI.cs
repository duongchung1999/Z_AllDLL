using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Config_ini.Class
{
    public static class HttpPostAPI
    {    
        /// <summary>
             /// 调用API 数据库
             /// </summary>
             /// <param name="url">连接地址</param>
             /// <param name="data">数据及格式</param>
             /// <returns></returns>
        public static string HttpPost(string url, string Writedata, string Method)
        {
            string str = HttpPost(url, Writedata, Method, out string error);
            if (str.Contains("False")) MessageBox.Show(error, "TE系统提示");
            return str;
        }

        public static bool HttpPost(string url, string Writedata, string Method, out string data, out string error)
        {

            data = HttpPost(url, Writedata, Method, out error);
            return !data.Contains("False");
        }
        public static string HttpPost(string url, string Writedata, string Method, out string error)
        {
            error = "";
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(Writedata);
                //参数类型，这里是json类型
                //还有别的类型如"application/x-www-form-urlencoded"，不过我没用过(逃
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "text/plain";
                //参数数据长度
                httpWebRequest.ContentLength = bs.Length;
                //设置请求类型 
                httpWebRequest.Method = Method;
                //设置超时时间
                httpWebRequest.Timeout = 4000;
                //将参数写入请求地址中
                if (Writedata != null) httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
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
                error=ex.Message.Contains("無法連接至遠端伺服器")?"网络断开连接，请连接网络" : ex.ToString();
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
                httpWebRequest.Timeout = 2000000;
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
