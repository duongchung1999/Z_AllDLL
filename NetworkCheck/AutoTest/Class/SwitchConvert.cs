using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest
{
    public class SwitchConvert
    {
        private static byte[] bits1 = { 0xFE, 0x05, 0x00, 0x00, 0xFF, 0x00, 0x98, 0x35 };
        private static byte[] bits2 = { 0xFE, 0x05, 0x00, 0x01, 0xFF, 0x00, 0xC9, 0xF5 };
        private static byte[] bits3 = { 0xFE, 0x05, 0x00, 0x02, 0xFF, 0x00, 0x39, 0xF5 };
        private static byte[] bits4 = { 0xFE, 0x05, 0x00, 0x03, 0xFF, 0x00, 0x68, 0x35 };
        private static byte[] bits5 = { 0xFE, 0x05, 0x00, 0x04, 0xFF, 0x00, 0xD9, 0xF4 };
        private static byte[] bits6 = { 0xFE, 0x05, 0x00, 0x05, 0xFF, 0x00, 0x88, 0x34 };
        private static byte[] bits7 = { 0xFE, 0x05, 0x00, 0x06, 0xFF, 0x00, 0x78, 0x34 };
        private static byte[] bits8 = { 0xFE, 0x05, 0x00, 0x07, 0xFF, 0x00, 0x29, 0xF4 };
        private static byte[] bits9 = { 0xFE, 0x05, 0x00, 0x08, 0xFF, 0x00, 0x19, 0xF7 };
        private static byte[] bits10 = { 0xFE, 0x05, 0x00, 0x09, 0xFF, 0x00, 0x48, 0x37 };
        private static byte[] bits11 = { 0xFE, 0x05, 0x00, 0x0A, 0xFF, 0x00, 0xB8, 0x37 };
        private static byte[] bits12 = { 0xFE, 0x05, 0x00, 0x0B, 0xFF, 0x00, 0xE9, 0xF7 };
        private static byte[] bits13 = { 0xFE, 0x05, 0x00, 0x0C, 0xFF, 0x00, 0x58, 0x36 };
        private static byte[] bits14 = { 0xFE, 0x05, 0x00, 0x0D, 0xFF, 0x00, 0x09, 0xF6 };
        private static byte[] bits15 = { 0xFE, 0x05, 0x00, 0x0E, 0xFF, 0x00, 0xF9, 0xF6 };
        private static byte[] bits16 = { 0xFE, 0x05, 0x00, 0x0F, 0xFF, 0x00, 0xA8, 0x36 };

        //Off
        private static byte[] bitoff1 = { 0xFE, 0x05, 0x00, 0x00, 0x00, 0x00, 0xD9, 0xC5 };
        private static byte[] bitoff2 = { 0xFE, 0x05, 0x00, 0x01, 0x00, 0x00, 0x88, 0x05 };
        private static byte[] bitoff3 = { 0xFE, 0x05, 0x00, 0x02, 0x00, 0x00, 0x78, 0x05 };
        private static byte[] bitoff4 = { 0xFE, 0x05, 0x00, 0x03, 0x00, 0x00, 0x29, 0xC5 };
        private static byte[] bitoff5 = { 0xFE, 0x05, 0x00, 0x04, 0x00, 0x00, 0x98, 0x04 };
        private static byte[] bitoff6 = { 0xFE, 0x05, 0x00, 0x05, 0x00, 0x00, 0xC9, 0xC4 };
        private static byte[] bitoff7 = { 0xFE, 0x05, 0x00, 0x06, 0x00, 0x00, 0x39, 0xC4 };
        private static byte[] bitoff8 = { 0xFE, 0x05, 0x00, 0x07, 0x00, 0x00, 0x68, 0x04 };
        private static byte[] bitoff9 = { 0xFE, 0x05, 0x00, 0x08, 0x00, 0x00, 0x58, 0x07 };
        private static byte[] bitoff10 = { 0xFE, 0x05, 0x00, 0x09, 0x00, 0x00, 0x09, 0xC7 };
        private static byte[] bitoff11 = { 0xFE, 0x05, 0x00, 0x0A, 0x00, 0x00, 0xF9, 0xC7 };
        private static byte[] bitoff12 = { 0xFE, 0x05, 0x00, 0x0B, 0x00, 0x00, 0xA8, 0x07 };
        private static byte[] bitoff13 = { 0xFE, 0x05, 0x00, 0x0C, 0x00, 0x00, 0x19, 0xC6 };
        private static byte[] bitoff14 = { 0xFE, 0x05, 0x00, 0x0D, 0x00, 0x00, 0x48, 0x06 };
        private static byte[] bitoff15 = { 0xFE, 0x05, 0x00, 0x0E, 0x00, 0x00, 0xB8, 0x06 };
        private static byte[] bitoff16 = { 0xFE, 0x05, 0x00, 0x0F, 0x00, 0x00, 0xE9, 0xC6 };

        /// <summary>
        /// 判断存储的容器
        /// </summary>
        static private Dictionary<int, string[]> vessel = new Dictionary<int, string[]>();

        /// <summary>
        /// 启动指定模板-32
        /// </summary>
        /// <param name="port">通讯口</param>
        /// <param name="NamePort">COM口</param>
        /// <param name="number">打开通道的名字用“.”隔开 例子“1.2.3.4.5”</param>
        /// <returns></returns>
        /// 
        private static void RelayACT16(SerialPort port, byte[] relayx)
        {

            port.Write(relayx, 0, relayx.Length);
            Console.WriteLine($"{DateTime.Now.ToString("MM/dd-HH:mm:ss")}|{port.PortName}|Send Switch byte|{relayx.ToString()}");
        }
        private static void SwitchOn16(SerialPort port, string number)
        {
            byte[] relayx= { };
            switch (number)
            {
                case "1": 
                    relayx = bits1;
                    break;
                case "2": 
                    relayx = bits2;
                    break;
                case "3":
                    relayx = bits3;
                    break;
                case "4":
                    relayx = bits4;
                    break;
                case "5":
                    relayx = bits5;
                    break;
                case "6":
                    relayx = bits6;
                    break;
                case "7":
                    relayx = bits7;
                    break;
                case "8":
                    relayx = bits8;
                    break;
                case "9":
                    relayx = bits9;
                    break;
                case "10":
                    relayx = bits10;
                    break;
                case "11":
                    relayx = bits11;
                    break;
                case "12":
                    relayx = bits12;
                    break;
                case "13":
                    relayx = bits13;
                    break;
                case "14":
                    relayx = bits14;
                    break;
                case "15":
                    relayx = bits15;
                    break;
                case "16":
                    relayx = bits16;
                    break;
                default: break;

            }
            try
            {

                port.Write(relayx, 0, relayx.Length);
            }
            catch { }

            
        }
        private static void SwitchOff16(SerialPort port, string number)
        {
            byte[] relayx = { };
            switch (number)
            {
                case "1":
                    relayx = bitoff1;
                    break;
                case "2":
                    relayx = bitoff2;
                    break;
                case "3":
                    relayx = bitoff3;
                    break;
                case "4":
                    relayx = bitoff4;
                    break;
                case "5":
                    relayx = bitoff5;
                    break;
                case "6":
                    relayx = bitoff6;
                    break;
                case "7":
                    relayx = bitoff7;
                    break;
                case "8":
                    relayx = bitoff8;
                    break;
                case "9":
                    relayx = bitoff9;
                    break;
                case "10":
                    relayx = bitoff10;
                    break;
                case "11":
                    relayx = bitoff11;
                    break;
                case "12":
                    relayx = bitoff12;
                    break;
                case "13":
                    relayx = bitoff13;
                    break;
                case "14":
                    relayx = bitoff14;
                    break;
                case "15":
                    relayx = bitoff15;
                    break;
                case "16":
                    relayx = bitoff16;
                    break;
                default: break;

            }
            try
            {
                port.Write(relayx, 0, relayx.Length);
            }
            catch { }


        }
        
        private static void OffSwitch(SerialPort port)
        {
            try
            {
                RelayACT16(port, bitoff1);
                RelayACT16(port, bitoff2);
                RelayACT16(port, bitoff3);
                RelayACT16(port, bitoff4);
                RelayACT16(port, bitoff5);
                RelayACT16(port, bitoff6);
                RelayACT16(port, bitoff7);
                RelayACT16(port, bitoff8);
                RelayACT16(port, bitoff9);
                RelayACT16(port, bitoff10);
                RelayACT16(port, bitoff11);
                RelayACT16(port, bitoff12);
                RelayACT16(port, bitoff13);
                RelayACT16(port, bitoff14);
                RelayACT16(port, bitoff15);
                RelayACT16(port, bitoff16);
            }
            catch
            {
                Console.WriteLine($"{DateTime.Now.ToString("MM/dd-HH:mm:ss")}|Switch Off Fail!");
            }
           
        }
        public static string Switch16(string NamePort, string number)
        {
            SerialPort port = new SerialPort(NamePort);
            try 
            {
                port.BaudRate = 9600; port.Parity = Parity.None; port.DataBits = 8; 
                if (!port.IsOpen) port.Open();
                string[] ch = number.Split('.');
                OffSwitch(port);
                Console.WriteLine($"{DateTime.Now.ToString("MM/dd-HH:mm:ss")}|OffAllSwitch");
                foreach (var item in ch)
                {
                    
                    SwitchOn16(port, item);
                    Console.WriteLine($"{DateTime.Now.ToString("MM/dd-HH:mm:ss")}|Send Switch|{item}");

                }
                port.Close();
                Console.WriteLine($"{DateTime.Now.ToString("MM/dd-HH:mm:ss")}|{port.PortName}|Send Switch|{number}");

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
        public static string Command(string NamePort, string number, string SwitchIp = "1")
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

                foreach (var item in ch)
                {
                    Thread.Sleep(2);
                    on[6] = (byte)(Convert.ToInt16(item));
                    on[7] = (byte)(on[0] + on[1] + on[2] + on[3] + on[4] + on[5] + on[6]);
                    port.Write(on, 0, on.Length);
                }
                port.Close();
                vessel[ip] = number.Split('.');
                Console.WriteLine($"{DateTime.Now.ToString("MM/dd-HH:mm:ss")}|{port.PortName}|Send Switch|{number}");

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
