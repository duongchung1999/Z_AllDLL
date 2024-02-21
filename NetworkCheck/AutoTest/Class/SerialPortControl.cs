using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest
{
    internal class SerialPortControl
    {
        public SerialPortControl(Dictionary<string, object> Config)
            => this.Config = Config;
        Dictionary<string, object> Config;

        SerialPort port = new SerialPort
        {
            ReadTimeout = 1000,
            WriteTimeout = 1000,

        };
        public void Test()
        {
            port.PortName = "COM200";
            port.Open();

            port.DataReceived += Port_DataReceived;
            while (true)
            {
                Thread.Sleep(1000);
            }


        }
        object lock_obj = new object();
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (lock_obj)
            {
                SerialPort port = (SerialPort)sender;
                byte[] bytes = new byte[port.BytesToRead];
                port.Read(bytes, 0, bytes.Length);
                string Line = Encoding.ASCII.GetString(bytes);
                string[] cmd = Line.CMDSplit();
                switch (cmd[0])
                {
                    default:
                        port.Write("CMD:False");
                        break;
                }

            }
        }
    }
}
