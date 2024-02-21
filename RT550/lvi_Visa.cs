using CommonUtil;
using Ivi.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using VISAInstrument.Port;

namespace lvi_Visa
{
    public class VISA
    {
        private PortOperatorBase _portOperatorBase = null;

        public bool Connent()
        {
            string IP = "192.168.1.61";

            string IpRegex = @"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$";

            if (!Regex.IsMatch(IP, IpRegex))
            {
                MessageBox.Show("IP地址不正确！");
                return false;
            }

            if (!PortUltility.OpenIPAddress(IP, out string fullAddress))
            {
                MessageBox.Show("未找到设备!");
                return false;
            }

            try
            {
                _portOperatorBase = new LANPortOperator(fullAddress);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化设备失败:{ex.Message}");
                return false;
            }
            try
            {
                _portOperatorBase.Timeout = 2000;
                _portOperatorBase.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"连接设备失败:{ex.Message}");
                return false;
            }
            MessageBox.Show("连接成功！");
            return true;
        }
        public void Close()
        {
            if (_portOperatorBase != null)
            {
                try
                {
                    _portOperatorBase.Close();

                }
                catch { }
                _portOperatorBase = null;
            }
        }
        public string WriteCommand(string cmd)
        {

            if (!string.IsNullOrEmpty(cmd))
            {
                try
                {
                    _portOperatorBase.WriteLine(cmd);
                    return cmd;
                }
                catch { }

            }

            return "False";
        }
        public string ReadString()
        {
            try
            {
                var result = _portOperatorBase.Read();
                return result;
            }
            catch (IOTimeoutException)
            {

            }
            catch (Exception ex)
            {

            }
            return "Error False";
        }
        public string ReadByte()
        {
            try
            {
                byte[] result = _portOperatorBase.ReadBytes(2048);
                string hexstr = ConvertHelper.ByteArrayToHexString(result);

                return hexstr;
            }
            catch (IOTimeoutException)
            {
                MessageBox.Show($"[Read][ERROR:Timeout]");
            }
            catch (Exception ex)
            {
            }
            return "Error False";
        }
    }
}
