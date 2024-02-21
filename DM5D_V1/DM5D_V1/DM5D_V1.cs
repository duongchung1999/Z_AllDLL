using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    /// <summary dllName="DM5D_V1">
    /// 电流表 DM5D/6D
    /// </summary>
    public class MerryDll : IMerryAllDll
    {

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：DM5D_V1";
            string dllfunction = "Dll功能说明 ：控制打印机";
            string dllHistoryVersion = "历史Dll版本： ";
            string dllVersion = "当前Dll版本：23.11.3.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.4.18.0：开发改成支持后台插入项目";
            string dllChangeInfo2 = "23.11.3.0：增加补偿功能，之前的连扳一直有，改了从后台插入项目就漏写了这个功能";
            string[] info = { dllname, dllfunction,
                dllHistoryVersion,
                dllVersion, dllChangeInfo,
                dllChangeInfo1 };
            return info;
        }
        Dictionary<string, object> Config = new Dictionary<string, Object>();
        public object Interface(Dictionary<string, object> keys) => Config = keys;
        double AddNumber = 0;
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();

        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] CMD);
            switch (CMD[1])
            {
                case "ReadDM5D": return ReadDM5D(CMD[2], CMD[3], int.Parse(CMD[4]), CMD[5]);
                case "Opention_ReadDM5D": return Opention_ReadDM5D(CMD[2], CMD[3], int.Parse(CMD[4]), CMD[5], int.Parse(CMD[6]));
                default: return $"Command Error False";


            }
        }

        void SplitCMD(object[] Command, out string[] CMD)
        {
            List<string> listCMD = new List<string>();

            string TestName = "";
            foreach (var item in Command)
            {
                Type type = item.GetType();

                if (type.ToString().Contains("TestitemEntity"))
                {
                    PropertyInfo property = type.GetProperty("测试项目");
                    TestName = property.GetValue(item, null).ToString();
                }
                if (type == typeof(Dictionary<string, object>))
                    OnceConfig = (Dictionary<string, object>)item;
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            CMD = listCMD.ToArray();
            if (OnceConfig.ContainsKey("SN"))
            {
                string TestID = OnceConfig["TestID"].ToString();
                double.TryParse(WindowsAPI.GetValue($"{TestID}#DM5D", TestName, ".\\AllDLL\\MenuStrip\\MoreAddDeploy.ini"), out AddNumber);

            }
            else
            {
                double.TryParse(WindowsAPI.GetValue("DM5D", $"{TestName}", ".\\AllDLL\\MenuStrip\\AddDeploy.ini"), out AddNumber);
            }
        }



        /// <summary isPublicTestItem="false"> 
        /// 读取DM5D
        /// </summary>
        /// <param name="NamePort">单板设串口如“COM1” 连扳设字段“DM5DPort” </param>
        /// <param name="id">物理id 默认是 1</param>
        /// <param name="Count">读取次数 取平均值</param>
        /// <param name="units" options="default,*1000,/1000">值转换 选“default”值不变</param>
        /// <returns></returns>
        public string ReadDM5D(string NamePort, string id, int Count, string units)
        {
            string STR = OnceConfig.ContainsKey("DM5DPort") ? OnceConfig["DM5DPort"].ToString() : NamePort; ;

            SerialPort port = new SerialPort(STR);
            try
            {
                if (!port.IsOpen) port.Open();
                byte[] TxBuf = new byte[100];
                ushort LuW_Crc;
                byte[] LAB_Tmp = new byte[4];
                byte[] LAB_ReadTmp = new byte[50];
                string Value = Int16.MinValue.ToString();
                TxBuf[0] = byte.Parse(id); TxBuf[1] = 0x03; TxBuf[2] = 0x00; TxBuf[3] = 0x2a; TxBuf[4] = 0x00; TxBuf[5] = 0x02;
                LuW_Crc = Get_CRC(TxBuf, 6);
                LAB_Tmp = BitConverter.GetBytes(LuW_Crc);
                TxBuf[6] = LAB_Tmp[0]; TxBuf[7] = LAB_Tmp[1];
                Thread.Sleep(2);
                port.Write(TxBuf, 0, 8);
                Thread.Sleep(50);
                List<double> readValues = new List<double>();
                for (byte i = 0; i < Count; i++)
                {
                    Thread.Sleep(5);
                    if (port.BytesToRead >= 9)
                    {
                        port.Read(LAB_ReadTmp, 0, port.BytesToRead);
                        Value = ArraySingle2String(LAB_ReadTmp);//将接收的字符串传入后转换
                        if (double.TryParse(Value, out double Result))
                            readValues.Add(Result);
                        break;
                    }
                }
                if (units == "*1000")
                {
                    return Math.Round(readValues.Average() * 1000, 2).ToString();

                }
                else if (units == "/1000")
                {
                    return Math.Round(readValues.Average() / 1000, 2).ToString();

                }
                else
                {
                    return Math.Round(readValues.Average(), 2).ToString();

                }

            }
            catch (Exception ex)
            {
                return $"{ex.Message} Error False";
            }
            finally
            {
                if (port.IsOpen) port.Close();
                port?.Dispose();
                Thread.Sleep(50);

            }

        }


        /// <summary isPublicTestItem="true"> 
        /// 读取DM5D
        /// </summary>
        /// <param name="NamePort">单板设串口如“COM1” 连扳设字段“DM5DPort” </param>
        /// <param name="id">物理id 默认是 1</param>
        /// <param name="Count">读取次数 取平均值</param>
        /// <param name="units" options="default,*1000,/1000">值转换 选“default”值不变</param>
        /// <param name="Rount">读保留几位小数 常规输入“2”</param>
        /// <returns></returns>
        public string Opention_ReadDM5D(string NamePort, string id, int Count, string units, int Rount)
        {
            string STR = OnceConfig.ContainsKey("DM5DPort") ? OnceConfig["DM5DPort"].ToString() : NamePort; ;

            SerialPort port = new SerialPort(STR);
            try
            {
                if (!port.IsOpen) port.Open();
                byte[] TxBuf = new byte[100];
                ushort LuW_Crc;
                byte[] LAB_Tmp = new byte[4];
                byte[] LAB_ReadTmp = new byte[50];
                string Value = Int16.MinValue.ToString();
                TxBuf[0] = byte.Parse(id); TxBuf[1] = 0x03; TxBuf[2] = 0x00; TxBuf[3] = 0x2a; TxBuf[4] = 0x00; TxBuf[5] = 0x02;
                LuW_Crc = Get_CRC(TxBuf, 6);
                LAB_Tmp = BitConverter.GetBytes(LuW_Crc);
                TxBuf[6] = LAB_Tmp[0]; TxBuf[7] = LAB_Tmp[1];
                List<double> readValues = new List<double>();
                for (int i = 0; i < Count; i++)
                {
                    port.Write(TxBuf, 0, 8);
                    Thread.Sleep(50);
                    for (byte b = 0; b < 20; b++)
                    {
                        if (port.BytesToRead >= 9)
                        {
                            port.Read(LAB_ReadTmp, 0, port.BytesToRead);
                            Value = ArraySingle2String(LAB_ReadTmp);//将接收的字符串传入后转换
                            if (double.TryParse(Value, out double Result))
                                readValues.Add(Result);
                            break;
                        }
                        Thread.Sleep(5);
                    }
                }




                if (units == "*1000")
                    return Math.Round((readValues.Average() * 1000)+AddNumber, Rount).ToString();
                else if (units == "/1000")
                    return Math.Round((readValues.Average() / 1000)+AddNumber, Rount).ToString();
                else
                    return Math.Round((readValues.Average())+AddNumber, Rount).ToString();

            }
            catch (Exception ex)
            {
                return $"{ex.Message} Error False";
            }
            finally
            {
                if (port.IsOpen) port.Close();
                port?.Dispose();
                Thread.Sleep(50);

            }

        }
        ushort Get_CRC(byte[] pBuf, byte num)
        {
            byte uIndex;
            byte i, uchCRCHi, uchCRCLo;
            uchCRCHi = 0xFF;
            uchCRCLo = 0xFF;
            for (i = 0; i < num; i++)
            {
                uIndex = (byte)(uchCRCLo ^ pBuf[i]);
                uchCRCLo = (byte)(uchCRCHi ^ auchCRCHi[uIndex]);
                uchCRCHi = (byte)(auchCRCLo[uIndex]);
            }
            return (ushort)(((ushort)(uchCRCHi) << 8) | (ushort)(uchCRCLo));

        }
        readonly byte[] auchCRCLo = new byte[]{
        0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,0x05, 0xC5, 0xC4,0x04,
        0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB,0x0B, 0xC9, 0x09,0x08, 0xC8,
        0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE,0xDF, 0x1F, 0xDD,0x1D, 0x1C, 0xDC,
        0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2,0x12, 0x13, 0xD3,0x11, 0xD1, 0xD0, 0x10,

        0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,0x36, 0xF6, 0xF7,0x37, 0xF5, 0x35, 0x34, 0xF4,
        0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E,0xFE, 0xFA, 0x3A,0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38,
        0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B,0x2A, 0xEA, 0xEE,0x2E, 0x2F, 0xEF, 0x2D, 0xED, 0xEC, 0x2C,
        0xE4, 0x24, 0x25, 0xE5, 0x27,0xE7, 0xE6, 0x26,0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0,

        0xA0, 0x60, 0x61, 0xA1,0x63, 0xA3, 0xA2,0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
        0x6C, 0xAC, 0xAD,0x6D, 0xAF, 0x6F,0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
        0x78, 0xB8,0xB9, 0x79, 0xBB,0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C,
        0xB4,0x74, 0x75, 0xB5,0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,

        0x50, 0x90, 0x91,0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94,0x54,
        0x9C, 0x5C,0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59,0x58, 0x98,
        0x88,0x48, 0x49, 0x89, 0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D,0x4D, 0x4C, 0x8C,
        0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,0x41, 0x81, 0x80,0x40
        };
        readonly byte[] auchCRCHi = new byte[]{
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,0x00, 0xC1, 0x81,0x40,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81,0x40, 0x01, 0xC0,0x80, 0x41,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1,0x81, 0x40, 0x01,0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,0xC0, 0x80, 0x41,0x00, 0xC1, 0x81, 0x40,

        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,0x00, 0xC1, 0x81,0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80,0x41, 0x01, 0xC0,0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,0x80, 0x41, 0x01,0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x00,0xC1, 0x81, 0x40,0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,

        0x01, 0xC0, 0x80, 0x41,0x00, 0xC1, 0x81,0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81,0x40, 0x01, 0xC0,0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1,0x81, 0x40, 0x01,0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x01,0xC0, 0x80, 0x41,0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,

        0x00, 0xC1, 0x81,0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,0x40,
        0x01, 0xC0,0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,0x80, 0x41,
        0x01,0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,0x00, 0xC1, 0x81,0x40
        };
        static string ArraySingle2String(byte[] PAuB_Dat, ushort PuW_Offset = 3)
        {
            byte[] LAB_Tmp = new byte[4];
            LAB_Tmp[0] = PAuB_Dat[PuW_Offset + 3]; LAB_Tmp[1] = PAuB_Dat[PuW_Offset + 2];
            LAB_Tmp[2] = PAuB_Dat[PuW_Offset + 1]; LAB_Tmp[3] = PAuB_Dat[PuW_Offset];
            return BitConverter.ToSingle(LAB_Tmp, 0).ToString();
        }



    }
    class WindowsAPI
    {
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVar, int size, string INIpath);
        public static string GetValue(string section, string key, string _path = ".\\AllDLL\\MenuStrip\\AddDeploy.ini")
        {
            try
            {
                StringBuilder var = new StringBuilder(512);
                GetPrivateProfileString(section, key, "", var, 512, _path);
                return var.ToString().Trim();
            }
            catch
            {
                return "0";
            }

        }
    }

}
