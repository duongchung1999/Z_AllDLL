using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    internal class _8342 : IDisposable
    {

        public SerialPort port = new SerialPort()
        {
            BaudRate = 9600,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
            ReadTimeout = 500
        };

        public string PortName = "";

        public string WriteLineText(string Command)
        {
            try
            {
                if (!port.IsOpen)
                {
                    port.PortName = PortName;
                    port.Open();
                    port.WriteLine("*IDN?");
                }
                port.DiscardOutBuffer();
                if (Command.Contains("\r\n"))
                {
                    port.Write(Command);

                }
                else
                {
                    port.WriteLine(Command);
                }
                return "True";
            }
            catch (Exception ex)
            {
                if (port.IsOpen)
                    port.Close();
                port.Dispose();
                port = new SerialPort()
                {
                    BaudRate = 9600,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    ReadTimeout = 500
                };
                return $"{port} {ex.Message} False";
            }
        }

        public string ReadLine()
        {
            string Value = "Read False";
            try
            {
                if (port.BytesToRead <= 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        WriteLineText(":VAL1?");
                        Thread.Sleep(250);
                        if (port.BytesToRead > 0)
                            break;
                    }
                }

                while (port.BytesToRead > 0)
                {

                    Value = port.ReadLine();
                    Thread.Sleep(50);

                }
                return Value;
            }
            catch (Exception ex)
            {

                if (port.IsOpen)
                    port.Close();
                port.Dispose();
                port = new SerialPort()
                {
                    BaudRate = 9600,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    ReadTimeout = 500
                };
                return $"{port} {ex.Message} False";
            }

        }

        public void Dispose()
        {
            port?.Dispose();
        }
        ~_8342()
        {
            Dispose();
        }
    }
}
