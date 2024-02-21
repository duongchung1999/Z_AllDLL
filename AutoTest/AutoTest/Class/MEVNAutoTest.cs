using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTest.SwitchConvert;


namespace AutoTest.Class
{
    internal class MEVNAutoTest
    {
        public MEVNAutoTest(Dictionary<string, object> Config)
              => this.Config = Config;
        Dictionary<string, object> Config;

        public void MEVNTest()
        {

            Thread.Sleep(2000);
            //MessageBox.Show("来到自动测试");

            Console.WriteLine($"AutoTest:Into Auto Test Mode {Config["AutoTest"]}");
            string SwitchPortName = "COM200";

            try
            {

                SerialPort InsulateBox = new SerialPort();
                InsulateBox.PortName = "COM1";
                InsulateBox.ReadTimeout = 1000;
                InsulateBox.WriteTimeout = 1000;

                InsulateBox.Open();
                //SendStr(InsulateBox, "open");
                //MessageBox.Show("现在继电器1闭合");
                ComWrite(close1);
                ComWrite(close2);
                ComWrite(close3);
                ComWrite(opencmd1);
                SendStr(InsulateBox, "open");
                // Command(SwitchPortName, "1");
                //Switch16(SwitchPortName, "1");
                //置启动测试为false
                Config["StartTest"] = false;

                while (true)
                {
                    if (InsulateBox.BytesToRead <= 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    if (!ReadStr(InsulateBox).ToLower().Contains("close"))
                        continue;
                    //Command(SwitchPortName, "1");
                    //Switch16(SwitchPortName, "1");
                    ComWrite(close1);
                    ComWrite(close2);
                    ComWrite(close3);
                    Config["SN"] = "";
                    //启动测试
                    Console.WriteLine("AutoTest:Start Test");
                    Config["StartTest"] = true;
                    Thread.Sleep(2000);
                    //如果为True就是正在测试
                    while ((bool)Config["StartTest"])
                        Thread.Sleep(50);
                    Console.WriteLine($"AutoTest:End Of Test Result:{Config["TestResultFlag"]}");
                    if ((bool)Config["TestResultFlag"])
                    {
                        //MessageBox.Show("现在继电器1,2闭合");
                        // Command(SwitchPortName, "1.2");
                        // Switch16(SwitchPortName, "1.2");
                        ComWrite(opencmd2);
                        ComWrite(opencmd1);
                    }
                    else
                    {
                        //MessageBox.Show("现在继电器1,3闭合");
                        //Command(SwitchPortName, "1.3");
                        //Switch16(SwitchPortName, "1.3");
                        ComWrite(opencmd1);
                        ComWrite(opencmd3);
                    }
                    SendStr(InsulateBox, "open");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("自动测试停止，报错了");
                MessageBox.Show("拍照记录下来\r\n" + ex.ToString());
            }

        }
        string ReadStr(SerialPort port)
        {
            string str = "";
            if (port.BytesToRead > 0)
            {
                byte[] bytes = new byte[port.BytesToRead];
                port.Read(bytes, 0, bytes.Length);
                str = Encoding.UTF8.GetString(bytes);
                Console.WriteLine($"{DateTime.Now.ToString("MM/dd-HH:mm:ss")}|{port.PortName}|Read|{str}");
            }
            return str;
        }
        void SendStr(SerialPort port, string CMD)
        {
            Console.WriteLine($"{DateTime.Now.ToString("MM/dd-HH:mm:ss")}|{port.PortName}|WriteLine|{CMD}");
            port.WriteLine(CMD);
        }
        static byte[] opencmd1 = new byte[]
            {
                0xFE,
                0x05,
                0x00,
                0x00,
                0xFF,
                0x00,
                0x98,
                0x35
            };
        static byte[] close1 = new byte[]
           {
                0xFE,
                0x05,
                0x00,
                0x00,
                0x00,
                0x00,
                0xD9,
                0xC5
           };
        static byte[] opencmd2 = new byte[]
           {
                0xFE,
                0x05,
                0x00,
                0x01,
                0xFF,
                0x00,
                0xC9,
                0xF5
           };
        static byte[] close2 = new byte[]
           {
                0xFE,
                0x05,
                0x00,
                0x01,
                0x00,
                0x00,
                0x88,
                0x05
           };
        static byte[] opencmd3 = new byte[]
         {
                0xFE,
                0x05,
                0x00,
                0x02,
                0xFF,
                0x00,
                0x39,
                0xF5
         };
        static byte[] close3 = new byte[]
           {
                0xFE,
                0x05,
                0x00,
                0x02,
                0x00,
                0x00,
                0x78,
                0x05
           };
        static void ComWrite(byte[] cmd)
        {
            SerialPort sp = new SerialPort("COM200");
            sp.BaudRate = 9600;
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.DataBits = 8;
            try
            {
                if (sp.IsOpen) sp.Close();
                sp.Open();
                Thread.Sleep(10);
                sp.Write(cmd, 0, cmd.Length);

            }
            finally
            {
                sp.Close();
            }
        }
    }
}
