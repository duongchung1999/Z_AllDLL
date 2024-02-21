using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;

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
        public void OnceConfigInterface(Dictionary<string, object> onceConfig)
           => this.OnceConfig = onceConfig;

        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        Dictionary<string, object> dic;
        //public object Interface(Dictionary<string, object> keys) => dic = keys;
        public object Interface(Dictionary<string, object> keys)
        {
            dic = keys;
            RELAY16 = dic["RELAY16"].ToString();
            return "";
        }
        string RELAY16 = "";
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：RELAY_16";
            string dllfunction = "Dll功能说明 ：弹出窗体，串口调试";
            string dllHistoryVersion = "历史Dll版本：2023.9.8.0";
            string dllHistoryVersion2 = "";
            string dllVersion = "";
            string dllChangeInfo = "";
            string dllChangeInfo2 = "";
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
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>)) OnceConfig = (Dictionary<string, object>)item;
                if (type.ToString() != "System.String") continue;
                //if (item.GetType().ToString() != "System.String") continue;

                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            int types = 0;
            var MoreTest = (string)dic["MoreTestMode"];
            //MessageBox("1");
            return relay(MoreTest=="1" ? (string)OnceConfig["RELAY16"] : (string)dic["RELAY16"], cmd[1]);
        }

        #endregion
        string relay(string PortName,string Command)
        {
            
            int baudRate = 9600; // Thay đổi tốc độ baudrate tương ứng với thiết bị của bạn
            SerialPort serialPort = new SerialPort(PortName, baudRate); 
            try
            {
                try
                {
                    Thread.Sleep(100);
                    serialPort.Open();
                }
                catch
                {
                    return false.ToString();
                }
                string a1 = "0";
                string a2 = "0";
                string a3 = "0";
                string a4 = "0";
                string a5 = "0";
                string a6 = "0";
                string a7 = "0";
                string a8 = "0";
                string a9 = "0";
                string a10 = "0";
                string a11 = "0";
                string a12 = "0";
                string a13 = "0";
                string a14 = "0";
                string a15 = "0";
                string a16 = "0";
                string inputString = Command;
                // Tách chuỗi thành các phần tử bằng dấu chấm
                string[] elements = inputString.Split('.');
                foreach (string element in elements)
                { 
                    if (element == "1")
                    {
                        a8 = "1";
                    }
                    if (element == "2")
                    {
                        a7 = "1";
                    }
                    if (element == "3")
                    {
                        a6 = "1";
                    }
                    if (element == "4")
                    {
                        a5 = "1";
                    }
                    if (element == "5")
                    {
                        a4 = "1";
                    }
                    if (element == "6")
                    {
                        a3 = "1";
                    }
                    if (element == "7")
                    {
                        a2 = "1";
                    }
                    if (element == "8")
                    {
                        a1 = "1";
                    }
                    if (element == "9")
                    {
                        a16 = "1";
                    }
                    if (element == "10")
                    {
                        a15 = "1";
                    }
                    if (element == "11")
                    {
                        a14 = "1";
                    }
                    if (element == "12")
                    {
                        a13 = "1";
                    }
                    if (element == "13")
                    {
                        a12 = "1";
                    }
                    if (element == "14")
                    {
                        a11 = "1";
                    }
                    if (element == "15")
                    {
                        a10 = "1";
                    }
                    if (element == "16")
                    {
                        a9 = "1";
                    }
                    if(element == "0")
                    {
                        Thread.Sleep(100);
                        byte[] img2tz = new byte[] { 0xFE, 0x0F, 0x00, 0x00, 0x00, 0x10, 0x02, 0x00,0x00,0xA7,0XD4 };
                        serialPort.Write(img2tz, 0, img2tz.Length);
                        return true.ToString();
                    }
                }           
                string value = a1 + a2 + a3 + a4 + a5 + a6 + a7 + a8 + a9 + a10 + a11 + a12 + a13 + a14 + a15 + a16;
                string binaryString = value;
                // Chuyển đổi từ hệ nhị phân sang số nguyên
                long decimalValue = Convert.ToInt64(binaryString, 2);
                // Chuyển đổi số nguyên sang mã hex
                string hexValue = decimalValue.ToString("X");
                // Hiển thị kết quả trong Label       
                int length = hexValue.Length;
                string firstPair;
                string secondPair;
                if (length == 1)
                {
                    firstPair = "00";
                    secondPair = "0" + hexValue.Substring(0, 1);
                }
                else if (length == 2)
                {
                    firstPair = "00";
                    secondPair = hexValue.Substring(0, 1) + "0";
                }
                else if (length == 3)
                {
                    firstPair = hexValue.Substring(0, 1);
                    firstPair = "0" + firstPair;
                    secondPair = hexValue.Substring(1, 2);
                }
                else
                {
                    firstPair = hexValue.Substring(0, 2);
                    secondPair = hexValue.Substring(2, 2);
                }
                string hexString = "FE 0F 00 00 00 10 02" + " " + firstPair + " " + secondPair;
                byte[] data = HexStringToByteArray(hexString);
                ushort crc = CalculateCRC16(data);
                string CRC = crc.ToString("X4");
                string CRC2 = CRC.Substring(0, 2);
                string CRC1 = CRC.Substring(2, 2);
                try
                {
                    byte[] img2tz = new byte[] { 0xFE, 0x0F, 0x00, 0x00, 0x00, 0x10, 0x02, Convert.ToByte(firstPair, 16), Convert.ToByte(secondPair, 16), Convert.ToByte(CRC1, 16), Convert.ToByte(CRC2, 16) };//FE 0F 00 00 00 10 02 FF FF A6 64
                    serialPort.Write(img2tz, 0, img2tz.Length);
                    return true.ToString();
                }
                catch
                {
                    return "xuất hiện lỗi";
                }
            }
            catch (Exception ex)
            {
                return false.ToString();
            }
            finally
            {
                // Đóng kết nối cổng COM sau khi sử dụng
                serialPort.Close();
                serialPort.Dispose();
            }
          
        }
        static ushort CalculateCRC16(byte[] data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) == 0x0001)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }
        // Chuyển đổi chuỗi hex sang mảng byte
        static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", ""); // Loại bỏ khoảng trắng
            int length = hex.Length / 2;
            byte[] bytes = new byte[length];

            for (int i = 0; i < length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }
    }
}
