using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MerryDllFramework
{
    /// <summary>
    /// 调节32路继电器类
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法
        Dictionary<string, object> Config;
        public object Interface(Dictionary<string, object> Config) => this.Config = Config;
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Switch";
            string dllfunction = "Dll功能说明 ：继电器跳转";
            string dllHistoryVersion = "历史Dll版本：21.11.1.1";
            string dllVersion = "当前Dll版本：22.10.31.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo2 = "22.4.22.0：可以设定IP";
            string dllChangeInfo3 = "22.10.31.0：单板连扳合并";
            string[] info = {
                dllname, dllfunction, dllHistoryVersion,
                dllVersion,
                dllChangeInfo,dllChangeInfo2,dllChangeInfo3
            };
            return info;
        }
        #endregion

        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        public void OnceConfigInterface(Dictionary<string, object> onceConfig) => OnceConfig = onceConfig;

        string Conport = "null";
        public string Run(object[] Command)
        {

            SplitCMD(Command, out string[] CMD);
            switch (CMD[1])
            {
                default:
                    return command(Conport, CMD[1],
                             CMD.Count() >= 3 ? CMD[2] : "1"
            ).ToString();
            }


        }

        void SplitCMD(object[] Command, out string[] CMD)
        {
            List<string> listCMD = new List<string>();
            foreach (var item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>))
                    OnceConfig = (Dictionary<string, object>)item;
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            Conport = OnceConfig.ContainsKey("SwitchPort") ? (string)OnceConfig["SwitchPort"] : (string)Config["SwitchComport"];
            CMD = listCMD.ToArray();
        }

        #region 
        private static byte[] a = { 0x55, 0x01, 0x13, 0x00, 0x00, 0x00, 0x00, 0x69 };
        private static byte[] s1 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x01, 0x89 };
        private static byte[] s2 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x02, 0x8a };
        private static byte[] s3 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x03, 0x8b };
        private static byte[] s4 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x04, 0x8c };
        private static byte[] s5 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x05, 0x8d };
        private static byte[] s6 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x06, 0x8e };
        private static byte[] s7 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x07, 0x8f };
        private static byte[] s8 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x08, 0x90 };
        private static byte[] s9 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x09, 0x91 };
        private static byte[] s10 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x0a, 0x92 };
        private static byte[] s11 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x0b, 0x93 };
        private static byte[] s12 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x0c, 0x94 };
        private static byte[] s13 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x0d, 0x95 };
        private static byte[] s14 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x0e, 0x96 };
        private static byte[] s15 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x0f, 0x97 };
        private static byte[] s16 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x10, 0x98 };
        private static byte[] s17 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x11, 0x99 };
        private static byte[] s18 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x12, 0x9a };
        private static byte[] s19 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x13, 0x9b };
        private static byte[] s20 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x14, 0x9c };
        private static byte[] s21 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x15, 0x9d };
        private static byte[] s22 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x16, 0x9e };
        private static byte[] s23 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x17, 0x9f };
        private static byte[] s24 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x18, 0xa0 };
        private static byte[] s25 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x19, 0xa1 };
        private static byte[] s26 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x1a, 0xa2 };
        private static byte[] s27 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x1b, 0xa3 };
        private static byte[] s28 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x1c, 0xa4 };
        private static byte[] s29 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x1d, 0xa5 };
        private static byte[] s30 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x1e, 0xa6 };
        private static byte[] s31 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x1f, 0xa7 };
        private static byte[] s32 = { 0x55, 0x01, 0x32, 0x00, 0x00, 0x00, 0x20, 0xa8 };
        /// <summary>
        /// 关闭的指令
        /// </summary>
        private static byte[] off = { 0x55, 0x01, 0x31, 0x00, 0x00, 0x00, 0x00, 0x87 };
        #endregion
        /// <summary>
        /// 判断存储的容器
        /// </summary>
        private Dictionary<int, string[]> vessel = new Dictionary<int, string[]>();

        /// <summary>
        /// 启动指定模板-32
        /// </summary>
        /// <param name="port">通讯口</param>
        /// <param name="NamePort">COM口</param>
        /// <param name="number">打开通道的名字用“.”隔开 例子“1.2.3.4.5”</param>
        /// <returns></returns>
        private string command(string NamePort, string number, string SwitchIp)
        {
            SerialPort port = new SerialPort(NamePort);
            try
            {

                byte ip = byte.Parse(SwitchIp);
                byte[] off = { 0x55, ip, 0x31, 0x00, 0x00, 0x00, 0x00, 0 };
                byte[] on = { 0x55, ip, 0x32, 0x00, 0x00, 0x00, 0x01, 0 };
                string[] ch = number.Split('.');
                port.BaudRate = 9600; port.Parity = Parity.None; port.DataBits = 8; if (!port.IsOpen) port.Open();
                //第一次下指令会关闭所有通道
                if (!vessel.ContainsKey(ip) || number == "0")
                {
                    byte[] alloff = { 0x55, ip, 0x13, 0x00, 0x00, 0x00, 0x00, 0 };
                    for (int i = 0; i < alloff.Length - 1; i++)
                        alloff[7] += alloff[i];
                    port.Write(alloff, 0, alloff.Length);
                    if (!vessel.ContainsKey(ip)) vessel.Add(ip, new string[10]);
                }
                else //第二次下指令将会关闭之前开过的通道
                {
                    //判断已经打开的通道是否包含本次需要打开的通道
                    for (int i = 0; i < vessel[ip].Length; i++)
                        foreach (var item in ch)
                            if (vessel[ip][i].Equals(item)) { vessel[ip][i] = ""; break; }
                    //下指令关闭通道
                    foreach (var item in vessel[ip])
                    {
                        try
                        {
                            if (item == "") continue;
                            Thread.Sleep(5);
                            off[6] = (byte)(Convert.ToInt16(item));
                            off[7] = (byte)(off[0] + off[1] + off[2] + off[3] + off[4] + off[5] + off[6]);
                            port.Write(off, 0, off.Length);
                        }
                        catch { }
                    }
                }

                //下指令开通通道
                Thread.Sleep(15);
                #region 旧下指令方法
                //foreach (var item in ch)//根据输入的序号判断启动模块
                //{
                //    Thread.Sleep(5);
                //    switch (item)
                //    {
                //        case "0": port.Write(a, 0, a.Length); break;
                //        case "1": port.Write(s1, 0, s1.Length); break;
                //        case "2": port.Write(s2, 0, s2.Length); break;
                //        case "3": port.Write(s3, 0, s3.Length); break;
                //        case "4": port.Write(s4, 0, s4.Length); break;
                //        case "5": port.Write(s5, 0, s5.Length); break;
                //        case "6": port.Write(s6, 0, s6.Length); break;
                //        case "7": port.Write(s7, 0, s7.Length); break;
                //        case "8": port.Write(s8, 0, s8.Length); break;
                //        case "9": port.Write(s9, 0, s9.Length); break;
                //        case "10": port.Write(s10, 0, s10.Length); break;
                //        case "11": port.Write(s11, 0, s11.Length); break;
                //        case "12": port.Write(s12, 0, s12.Length); break;
                //        case "13": port.Write(s13, 0, s13.Length); break;
                //        case "14": port.Write(s14, 0, s14.Length); break;
                //        case "15": port.Write(s15, 0, s15.Length); break;
                //        case "16": port.Write(s16, 0, s16.Length); break;
                //        case "17": port.Write(s17, 0, s17.Length); break;
                //        case "18": port.Write(s18, 0, s18.Length); break;
                //        case "19": port.Write(s19, 0, s19.Length); break;
                //        case "20": port.Write(s20, 0, s20.Length); break;
                //        case "21": port.Write(s21, 0, s21.Length); break;
                //        case "22": port.Write(s22, 0, s22.Length); break;
                //        case "23": port.Write(s23, 0, s23.Length); break;
                //        case "24": port.Write(s24, 0, s24.Length); break;
                //        case "25": port.Write(s25, 0, s25.Length); break;
                //        case "26": port.Write(s26, 0, s26.Length); break;
                //        case "27": port.Write(s27, 0, s27.Length); break;
                //        case "28": port.Write(s28, 0, s28.Length); break;
                //        case "29": port.Write(s29, 0, s29.Length); break;
                //        case "30": port.Write(s30, 0, s30.Length); break;
                //        case "31": port.Write(s31, 0, s31.Length); break;
                //        case "32": port.Write(s32, 0, s32.Length); break;
                //        default: break;
                //    }

                //}
                #endregion

                foreach (var item in ch)
                {
                    Thread.Sleep(2);
                    on[6] = (byte)(Convert.ToInt16(item));
                    on[7] = (byte)(on[0] + on[1] + on[2] + on[3] + on[4] + on[5] + on[6]);
                    port.Write(on, 0, on.Length);
                }
                port.Close();
                vessel[ip] = number.Split('.');
                return "True";
            }
            catch (Exception ex)
            {
                return $"{ex.Message} False";
            }
            finally
            {
                if (port.IsOpen) port.Close();
                port.Dispose();
            }
        }

    }
}
