using Ivi.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VISAInstrument.Port;

namespace MT8852B_V1.Ulitity
{
    internal class Communication
    {

        public static List<string> listLog = new List<string>();
        public static PortOperatorBase _portOperatorBase = null;
        static bool IsRS232 = false;

        public static bool Connect(string GPIB)
        {
            if (_portOperatorBase != null)
            {
                return true;
            }

            try
            {

                if (GPIB.Contains("GPIB"))
                {
                    IsRS232 = false;
                    List<string> listGPIB = new List<string>(PortUltility.FindAddresses(PortType.GPIB));

                    if (!PortUltility.OpenIPAddress(GPIB, out string fullAddress) || !listGPIB.Contains(fullAddress))
                    {
                        MessageBox.Show($"连接地址方式GPIB：{GPIB}:未找到设备!");
                        return false;
                    }
                    _portOperatorBase = new GPIBPortOperator(fullAddress);

                }
                else
                {
                    IsRS232 = true;


                    Dictionary<string, string> Pair = new Dictionary<string, string>();

                    foreach (var item in PortUltility.FindAddresses(PortType.RS232))
                    {
                        GlobalResourceManager.TryParse(item, out ParseResult result);
                        Pair[result.AliasIfExists] = item;
                    }
                    if (!Pair.ContainsKey(GPIB))
                    {
                        MessageBox.Show($"连接地址方式RS232：未能识别到串口{GPIB}");
                    }

                    if (!PortUltility.OpenRS232Address(Pair[GPIB], out string fullAddress))
                    {
                        MessageBox.Show($"连接地址方式RS232：{GPIB}:未找到设备!");
                        return false;
                    }
                    _portOperatorBase = new RS232PortOperator(Pair[GPIB], 57600, SerialParity.None, SerialStopBitsMode.One, 8);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化设备失败:{ex.Message}");
                _portOperatorBase = null;
                return false;
            }



            try
            {
                _portOperatorBase.Timeout = 50;
                _portOperatorBase.Open();
            }
            catch (Exception ex)
            {
                _portOperatorBase = null;
                MessageBox.Show($"连接设备失败:{ex.Message}");
                return false;

            }
            Thread.Sleep(100);
            WriteCommand("OPMD SCRIPT");
            Thread.Sleep(100);
            WriteCommand("DISCONNECT");
            DiscardInBuffer(10);
            Thread.Sleep(1000);

            return true;
        }
        public static void DiscardInBuffer()
        {
            if (_portOperatorBase != null && _portOperatorBase.IsPortOpen)
                for (int i = 0; i < 6; i++)
                    ReadString();
        }
        public static void DiscardInBuffer(int Count)
        {
            if (_portOperatorBase != null && _portOperatorBase.IsPortOpen)
                for (int i = 0; i < Count; i++)
                {
                    string Str = ReadString();
                    if (Str.Length <= 0) break;
                }
        }


        public static string WriteCommand(string CMD)
        {
            if (_portOperatorBase == null) return "Command False";
            Thread.Sleep(2);
            string Write = $"{DateTime.Now:yy年MM月dd日 HH时mm分ss秒}  [Write]   {CMD}";
            Console.WriteLine(Write);
            listLog.Add(Write);

            return IsRS232 ? WriteCommandByte(CMD) : WriteCommandStr(CMD);
        }
        public static string ReadString()
        {
            if (_portOperatorBase == null) return "Visa not connected False ";
            Thread.Sleep(2);
            string result = IsRS232 ? ReadBytes() : ReadStringStr();
            string Read = $"{DateTime.Now:yy年MM月dd日 HH时mm分ss秒}  [Read]   {result}";
            Console.WriteLine(Read);
            listLog.Add(Read);

            return IsRS232 ? result.Remove(0, 1) : result;
        }


        //################################################################ 用字符串 写入 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        static string WriteCommandStr(string cmd)
        {
            try
            {
                _portOperatorBase.WriteLine(cmd);
                return cmd;
            }
            catch (Exception ex)
            {
                _portOperatorBase = null;
            }
            return "Error False";
        }

        static string WriteCommandByte(string CMD)
        {

            try
            {
                _portOperatorBase.Write(Encoding.ASCII.GetBytes(CMD).Concat(new byte[] { 0x0A }).ToArray());
                Thread.Sleep(5);
                return CMD;
            }
            catch (Exception ex)
            {
                _portOperatorBase = null;
            }
            return "Error False";

        }

        static string ReadBytes()
        {
            try
            {
                byte[] ReadsByte = _portOperatorBase.ReadBytes();
                string Result = Encoding.ASCII.GetString(ReadsByte).Replace("\n", "");
                return Result;
            }
            catch (IOTimeoutException ex)
            {

            }
            catch (Exception ex)
            {
            }
            return "Error False";
        }
        static string ReadStringStr()
        {
            try
            {
                var result = _portOperatorBase.Read().Replace("\n", "");
                return result;
            }
            catch (IOTimeoutException ex)
            {

            }
            catch (Exception ex)
            {
            }
            return "Error False";
        }

    }
}
