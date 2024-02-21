using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

internal class ConnectToserver
{
    public static string url = "";
    public static void UpLoad()
    {
        try
        {
            Thread.Sleep(1000);
            string ComputerName = Environment.MachineName;
            Connect(ComputerName);

        }
        catch (Exception ex)
        {

            Task.Run(() => MessageBox.Show($"{"拿手机拍照把报错信息记录下来发给TE"}\r\n{ex}", "电脑管理系统报错"));
            Log = ex.ToString();
        }


    }

    static void Connect(string ComputerName)
    {
        string HWinfo = "True";
        int i = 0;
        while (true)
        {
            i++;

            string WriteData = "{" + $"\"model\":\"{TypeName}\",\"station\":\"{Station}\",\"machineName\": \"{ComputerName}\",\"mesFlag\": \"{MesFlag}\",\"computer_Configuration\":\"{HWinfo}\"" + "}";
            string STR = HttpPost(url, WriteData, "POST", out string ERROR);
            if (STR == "200")
            {
                HWinfo = "null";
            }
            Log = ($"{i}、{STR}");
            Thread.Sleep(1000 * 60 * 2);

        }
    }
    public static string TypeName = "";
    public static string Station = "";
    public static string MesFlag = "";
    private static string _Log = "";
    public static string Log
    {
        get { return _Log; }
        set
        {
            string path = System.Environment.CurrentDirectory + "\\LOG";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            _Log = value;
            File.AppendAllText($@"{path}\ConnectingToAService_{DateTime.Now:yy_MM_dd}.txt", contents: $"{DateTime.Now:yyyy MM dd HH mm ss}\t{_Log}\r\n");
        }
    }
    static String exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    static string HttpPost(string url, string Writedata, string Method, out string error)
    {
        Log = $"URL:{url}";
        Log = $"Writedata:{Writedata}";
        error = "";
        try
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            //字符串转换为字节码
            byte[] bs = Encoding.UTF8.GetBytes(Writedata);
            //参数类型，这里是json类型
            //还有别的类型如"application/x-www-form-urlencoded"，
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

            Log = ex.ToString();
            return $"{false}";
        }
    }
}
