using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        Dictionary<string, object> dic;
        static string value = "";
        public object Interface(Dictionary<string, object> keys) => dic = keys;


        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：_8082";
            string dllfunction = "Dll功能说明 ：弹出窗体，串口调试";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllHistoryVersion2 = "                     ：22.11.19.0";
            string dllVersion = "当前Dll版本：22.11.19.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo2 = "23.01.29.0：2023/1/29：改动串口错误返回值";
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
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }

            switch (cmd[1].ToLower())
            {
                case "forms": return MessageBox(cmd[2]).ToString();
                case "sleep": Thread.Sleep(int.TryParse(cmd[2], out int i) ? i : 1000); return true.ToString();
                case "send_read": return Send_Read(cmd[2], cmd[3]);
                case "read_voltage": return ReadVoltage(cmd[2], cmd[3]);
                case "check_signal": return CheckSignal(cmd[2], cmd[3], cmd[4]).ToString();
                case "read_8082": return ReadAllVoltage(cmd[2]);
                case "chanel_voltage": return ChanelVoltage(cmd[2], cmd[3]);
                case "powerfrequency":
                    string[] str = new string[7];
                    str[2] = cmd[2];
                    str[3] = cmd[3];
                    str[4] = cmd[4];
                    if (cmd.Length < 6)
                    {
                        str[5] = cmd[5];
                    }
                    else
                    {
                        str[5] = "500";
                    }

                    return Power(cmd[2], cmd[3], cmd[4], cmd[5]);
                default: return "指令错误 False";

            }
        }
        #endregion
        SerialPort Comport = new SerialPort();
        string Send_Read(string ComPort, string Command)
        {

            try
            {
                Comport.PortName = ComPort;
                Comport.Open();
                if (Command.Contains('?'))
                {
                    Comport.WriteLine(Command);
                    Thread.Sleep(500);
                    return Comport.ReadLine();
                }
                else
                {
                    Comport.WriteLine(Command);
                    return true.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"{ex.Message} Error";
            }
            finally
            {
                if (Comport.IsOpen) Comport.Close();
                Comport.Dispose();
            }
        }
        public string Portname = "";
        public string Chanel1 = "";
        public string Chanel2 = "";
        public string Chanel3 = "";
        public string Chanel4 = "";
        public string Chanel5 = "";
        public string Chanel6 = "";
        public string Chanel7 = "";
        public string Chanel8 = "";

        bool CheckSignal(string Comport, string chanel, string time)
        {
            int Time = Convert.ToInt32(time);
            int DelayNumbers = Time / 500;
            int i = 0;
            bool flag = false;
            while (i < DelayNumbers)
            {
                i++;
                ReadAllVoltage(Comport);
                var value = ChanelVoltage(Comport, chanel);
                if(Convert.ToInt32(value)>=5)
                {
                    flag = true;
                    break;
                }
            }
            if (flag == false)
            {
                MessageBox("没有PLC返回的信号/ Không có tín hiệu phản hồi từ PLC");
            }
            return flag;
        }
        string ReadVoltage(string ComPort, string Chanel)
        {

            try
            {
                Comport.PortName = ComPort;
                Portname = ComPort;
                if (Comport.IsOpen)
                {
                    Comport.Close();
                }
                Comport.Open();
                Thread.Sleep(500);
                string cmd = "";
                switch (Chanel)
                {
                    case "01":
                        cmd = "#010\r\n";
                        break;
                    case "02":
                        cmd = "#011\r\n";
                        break;
                    case "03":
                        cmd = "#012\r\n";
                        break;
                    case "04":
                        cmd = "#013\r\n";
                        break;
                    case "05":
                        cmd = "#014\r\n";
                        break;
                    case "06":
                        cmd = "#015\r\n";
                        break;
                    case "07":
                        cmd = "#016\r\n";
                        break;
                    case "08":
                        cmd = "#017\r\n";
                        break;
                    default: return false.ToString();
                }
                byte[] bCmd = System.Text.ASCIIEncoding.Default.GetBytes(cmd);
                Comport.Write(bCmd, 0, bCmd.Length);
                Thread.Sleep(500);
                var result = Comport.ReadLine();
                value = result;
                if (!string.IsNullOrEmpty(result))
                {
                    //Console.WriteLine($"Receive data：{result}");

                    var values = result.Trim().Remove(0, 2).Split('+');
                    //for (int i = 1; i <= values.Length; i++)
                    //{
                    //    Console.WriteLine($"chanel{i}, Value:{values[0]}");
                    //}
                    return values[0];
                }
                else
                {
                    return false.ToString();
                }

            }
            catch (Exception ex)
            {
                return $"{ex.Message} Error";
            }
            finally
            {
                if (Comport.IsOpen) Comport.Close();
                Comport.Dispose();
            }
        }
        string ReadAllVoltage(string ComPort)
        {

            try
            {
                Comport.PortName = ComPort;
                Portname = ComPort;
                if (Comport.IsOpen)
                {
                    Comport.Close();
                }
                Comport.Open();
                Thread.Sleep(500);
                string cmd = "";
                string[] cmdAll = {"#010\r\n", "#011\r\n", "#012\r\n", "#013\r\n", "#014\r\n", "#015\r\n", "#016\r\n", "#017\r\n"};
                Chanel1 = SelectCmd(cmdAll[0]);
                Chanel2 = SelectCmd(cmdAll[1]);
                Chanel3 = SelectCmd(cmdAll[2]);
                Chanel4 = SelectCmd(cmdAll[3]);
                Chanel5 = SelectCmd(cmdAll[4]);
                Chanel6 = SelectCmd(cmdAll[5]);
                Chanel7 = SelectCmd(cmdAll[6]);
                Chanel8 = SelectCmd(cmdAll[7]);
                return true.ToString();

            }
            catch 
            {
                return false.ToString();
            }
            finally
            {
                if (Comport.IsOpen) Comport.Close();
                Comport.Dispose();
            }
        }
        string ChanelVoltage(string ComPort, string Chanel)
        {
            if (ComPort == Portname)
            {
                switch (Chanel)
                {
                    case "01":
                        return Chanel1;
                    case "02":
                        return Chanel2;
                    case "03":
                        return Chanel3;
                    case "04":
                        return Chanel4;
                    case "05":
                        return Chanel5;
                    case "06":
                        return Chanel6;
                    case "07":
                        return Chanel7;
                    case "08":
                        return Chanel8;
                    default:
                        return false.ToString();

                }
            }
            else return "False : 错误 COMPORT";
        }
        public string SelectCmd(string cmd)
        {
            //string cmd = "";
            //string[] cmdAll = { "#010\r\n", "#011\r\n", "#012\r\n", "#013\r\n", "#014\r\n", "#015\r\n", "#016\r\n", "#017\r\n" };
            byte[] bCmd = System.Text.ASCIIEncoding.Default.GetBytes(cmd);
            Comport.Write(bCmd, 0, bCmd.Length);
            var result = Comport.ReadLine();
            value = result;
            if (!string.IsNullOrEmpty(result))
            {
                var values = result.Trim().Remove(0, 2).Split('+');
                double f = Convert.ToDouble(values[0]);
                return f.ToString();
            }
            else return false.ToString();

        }
        string Power(string ComPort, string Command, string Signal, string Sleep)
        {

            using (SerialPort Comport = new SerialPort(ComPort))
            {
                try
                {
                    bool signal = bool.TryParse(Signal, out bool result) && result;
                    Comport.Open();
                    switch (Command)
                    {
                        //  Com口 4脚设置是否高电频
                        case "DtrEnable": Comport.DtrEnable = signal; break;
                        //  Com口 7脚设置是否高电频
                        case "RtsEnable": Comport.RtsEnable = signal; break;
                        default: return "指令错误,请查看Command.xlsx False";
                    }
                    return true.ToString();
                }
                catch (Exception ex)
                {
                    return $"{ex.Message} Error";
                }
                finally
                {
                    Thread.Sleep(int.Parse(Sleep));
                    if (Comport.IsOpen) Comport.Close();
                }
            }
        }

    }
}
