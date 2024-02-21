using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;

namespace MerryDllFramework
{
    public class LBBXYZ
    {
        private string ComPort { get; set; }
        private int BaudRate { get; set; }

        private SerialPort serialPort;

        public LBBXYZ(string comport, int baudrate)
        {
            ComPort = comport;
            BaudRate = baudrate;
        }
        /// <summary>
        /// Connect device
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            bool isOk = true;
            try
            {
                serialPort = new SerialPort();
                serialPort.PortName = ComPort;
                serialPort.BaudRate = BaudRate;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;
                serialPort.Parity = Parity.None;

                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                isOk = false;
                MessageBox.Show($"Init: {ex.ToString()}");
            }

            return isOk;
        }
        /// <summary>
        /// Disconect device
        /// </summary>
        public void Disconnect()
        {
            if(serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    serialPort.Dispose();
                }
            }
        }
        /// <summary>
        /// Set color target mode,0 - auto, 3 - fixed, 5 - RGB
        /// </summary>
        /// <param name="mode">0 - auto, 3 - fixed, 5 - RGB</param>
        /// <returns></returns>
        public bool SetTargetMode(string mode)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    string cmd = $":001w_target_type01-16={mode}\\n";
                    serialPort.Write(cmd);
                    serialPort.Write(new Byte[] { 0X0A }, 0, 1);
                    Thread.Sleep(50);
                    var recData = new byte[serialPort.BytesToRead];
                    serialPort.Read(recData, 0, recData.Length);
                    string value = Encoding.ASCII.GetString(recData);
                    if (value.Contains(cmd))
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    MessageBox.Show("Serial port is not open");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"HanOpticSensDLL: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Get lux of 1 - 16 channel = > [0-15]
        /// </summary>
        /// <param name="aveCount"></param>
        /// <returns></returns>
        public double[] GetLux(int aveCount)
        {
            double[] results = new double[16];
            try
            {
                for (int i = 0; i < aveCount; i++)
                {
                    string cmd = ":001r_lux01-16\\n";
                    if (serialPort.IsOpen)
                    {
                        serialPort.Write(cmd);
                        serialPort.Write(new Byte[] { 0X0A }, 0, 1);
                        Thread.Sleep(150);
                        var recData = new byte[serialPort.BytesToRead];
                        serialPort.Read(recData, 0, recData.Length);
                        string Value = Encoding.ASCII.GetString(recData);//将byte数组转换为ASCll OK = 4F 4B
                        string[] result = Value.Split('=');
                        if (result.Length > 1)
                        {
                            var luxStrings = result[1].Split(',');
                            for (int j = 0; j < 16; j++)
                            {
                                results[j] += Convert.ToDouble(luxStrings[j]);
                            }
                        }
                    }
                }
                for (int i = 0; i < 16; i++)
                {
                    results[i] = results[i] / aveCount;
                }
                return results;
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetLux: " + ex.Message);
            }
            return new double[16];
        }
        /// <summary>
        /// Get cie 1931 xy, [0], [1] => channel 1 x - y
        /// </summary>
        /// <param name="aveCount"></param>
        /// <returns></returns>
        public double[] GetCIE1931xy(int aveCount)
        {
            double[] results = new double[32];
            try
            {
                for (int i = 0; i < aveCount; i++)
                {
                    string cmd = ":001r_xy01-16\\n";
                    if (serialPort.IsOpen)
                    {
                        serialPort.Write(cmd);
                        serialPort.Write(new Byte[] { 0X0A }, 0, 1);
                        Thread.Sleep(150);
                        var recData = new byte[serialPort.BytesToRead];
                        serialPort.Read(recData, 0, recData.Length);
                        string Value = Encoding.ASCII.GetString(recData);//将byte数组转换为ASCll OK = 4F 4B
                        string[] result = Value.Split('=');
                        if (result.Length > 1)
                        {
                            var luxStrings = result[1].Split(',');
                            for (int j = 0; j < 32; j++)
                            {
                                results[j] += Convert.ToDouble(luxStrings[j]);
                            }
                        }
                    }
                }
                for (int i = 0; i < 32; i++)
                {
                    results[i] = results[i] / aveCount;
                }
                return results;
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetLux: " + ex.Message);
            }
            return new double[32];
        }

        public double[] GetRGB(int aveCount)
        {
            double[] results = new double[32];
            try
            {
                for (int i = 0; i < aveCount; i++)
                {
                    string cmd = ":001r_rgbi01-02\\n";
                    if (serialPort.IsOpen)
                    {
                        serialPort.Write(cmd);
                        serialPort.Write(new Byte[] { 0X0A }, 0, 1);
                        Thread.Sleep(150);
                        var recData = new byte[serialPort.BytesToRead];
                        serialPort.Read(recData, 0, recData.Length);
                        string Value = Encoding.ASCII.GetString(recData);//将byte数组转换为ASCll OK = 4F 4B
                        string[] result = Value.Split('=');
                        if (result.Length > 1)
                        {
                            var luxStrings = result[1].Split(',');
                            for (int j = 0; j < 32; j++)
                            {
                                results[j] += Convert.ToDouble(luxStrings[j]);
                            }
                        }
                    }
                }
                for (int i = 0; i < 32; i++)
                {
                    results[i] = results[i] / aveCount;
                }
                return results;
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetLux: " + ex.Message);
            }
            return new double[32];
        }
    }
}
