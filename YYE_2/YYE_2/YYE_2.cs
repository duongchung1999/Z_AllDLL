using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法

        bool MessageBox(string msg)
        {
            Common.Box boxs = new Common.Box(msg);
            boxs.ShowDialog();
            var result = boxs.DialogResult;//先关闭会获取不到值
            return result == DialogResult.Yes;
        }

        Dictionary<string, object> dic;

        public object Interface(Dictionary<string, object> keys) => dic = keys;


        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Common";
            string dllfunction = "Dll功能说明 ：弹出窗体，串口调试";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllHistoryVersion2 = "                     ：21.8.14.0";
            string dllVersion = "当前Dll版本：21.8.14.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo2 = "21.8.14.0：2021/8/14：改动串口错误返回值";
            string[] info = { dllname, dllfunction, dllHistoryVersion, dllHistoryVersion2,
                dllVersion, dllChangeInfo,dllChangeInfo2
            };


            return info;
        }
        public string Run(object[] Command)
        {
            string[] cmd = new string[0];
            foreach (var item in Command)
            {
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            return relay(cmd[1], cmd[2]);
         
        }
        #endregion
        SerialPort Comport = new SerialPort();
        string relay(string ComPort, string Command)
        {

            try
            {
                Comport.PortName = ComPort;
                Comport.Open();
                Command = Command.ToUpper();
                switch (Command)
                {
                    case "OPEN1":
                        Comport.WriteLine("A11T");
                        Thread.Sleep(50);
                        return Comport.ReadLine();

                    case "OPEN2":
                        Comport.WriteLine("A21T");
                        Thread.Sleep(50);
                        return Comport.ReadLine();

                    case "CLOSE1":
                        Comport.WriteLine("A10T");
                        Thread.Sleep(50);
                        return Comport.ReadLine();

                    case "CLOSE2":
                        Comport.WriteLine("A20T");
                        Thread.Sleep(50);
                        return Comport.ReadLine();

                    case "OPEN":
                        Comport.WriteLine("A11T");
                        Thread.Sleep(10);
                        Comport.WriteLine("A20T");
                        Thread.Sleep(10);
                        return Comport.ReadLine();

                    case "CLOSE":
                        Comport.WriteLine("A10T");
                        Thread.Sleep(10);
                        Comport.WriteLine("A21T");
                        Thread.Sleep(10);
                        return Comport.ReadLine();
                }
               
            }
            catch (Exception ex)
            {
                MessageBox("vui lòng kiểm tra lại lệnh");
                return $"{ex.Message} sai lenh";
            }
            finally
            {
                if (Comport.IsOpen) Comport.Close();
                Comport.Dispose();
            }
        }
        

    }
}
