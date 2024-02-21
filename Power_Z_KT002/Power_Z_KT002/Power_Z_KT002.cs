using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShizukuLib;
using System.Management;
using static ShizukuLib.ShizukuProtocolMeterData;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法
        bool a = false;
        string b1;
        bool s=false;
        bool k = false;
        string aa;
        bool MessageBox(string msg)
        {
            Common.Box boxs = new Common.Box(msg);
            boxs.ShowDialog();
            var result = boxs.DialogResult;//先关闭会获取不到值
            return result == DialogResult.Yes;
        }
        Dictionary<string, object> dic;
        public object Interface(Dictionary<string, object> keys) => dic = keys;
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：POWER_Z";
            string dllfunction = "Dll功能说明 ：弹出窗体，串口调试";
            string dllHistoryVersion = "历史Dll版本：2023.6.12.0";
            string dllVersion = "当前Dll版本：23.8.19.6";
            string dllChangeInfo = "23.8.19.1  2023/8/19 分开各方法";
            string dllChangeInfo2 = "";
            string[] info = { dllname, dllfunction, dllHistoryVersion,
                dllVersion, dllChangeInfo,dllChangeInfo2
            };
            return info;
        }
        public string Run(object[] Command)
        {
           
            counter = 0;
            string[] cmd = new string[0];
            foreach (var item in Command)
            {
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            switch (cmd[1])
            {
                case "GetCurrent": return GetCurrent(cmd[2]);
                case "GetVoltage": return GetVoltage();
                case "GetPower": return GetPower();
                default: return "False Command Error!";
            }
        }
        #endregion   
        string relay(string Command,string q)
        {
            a = false;
            k = false;
            if (Command == "ample" || Command == "AMPLE")
            {
                try
                {
                    Comport_();
                    if (k == true)
                    {
                        protocolStack.CloseConnection();
                        return "lỗi cổng COM/COM端口错误";
                    }
                    if (k != true)
                    {
                        Run_1();
                    }
                    while (true)
                    {
                        if (a == true)
                        {
                            
                            break;
                        }
                    }
                    a = false;
                    counter = 0;
                    if (q == "uA")
                    {
                        float z = float.Parse(b1);
                        z = z * 1000000;
                        return z + "";
                    }
                    if (q == "mA")
                    {
                        float z = float.Parse(b1);
                        z = z * 1000;
                        return z+"";
                    }
                    if (q == "A")
                    {
                        float z = float.Parse(b1);
                        return z + "";
                    }

                }
                catch (Exception ex)
                {
                    return false.ToString();
                }
            }
            if (Command == "VOLT" || Command == "volt")
            {
                try
                {
                    Comport_();
                    if (k == true)
                    {
                        protocolStack.CloseConnection();
                        return "lỗi cổng COM/COM端口错误";
                    }
                    if (k != true)
                    {
                        Run_2();
                    }
                   
                    while (true)
                    {
                        if (a == true)
                        {
                            break;
                        }
                    }
                    a = false;
                    counter = 0;
                    return b1;
                }
                catch (Exception ex)
                {
                    return false.ToString();
                }
            }
            if (Command == "watt" || Command == "WATT")
            {
                try
                {
                    Comport_();
                    if (k == true)
                    {
                        protocolStack.CloseConnection();
                        return "lỗi cổng COM/COM端口错误";
                    }
                    if (k != true)
                    {
                        Run_3();
                    }
                    
                    while (true)
                    {
                        if (a == true)
                        {
                            break;
                        }
                    }
                    a = false;
                    counter = 0;
                    return b1;
                }
                catch (Exception ex)
                {
                    return false.ToString();
                }
            }
            else
            {
                return false.ToString() + "lỗi ở cài đặt/设置错误";
            }
        }
        /// <summary isPublicTestItem="true">
        /// 读取电流 GetCurrent
        /// </summary>
        /// <param name="type" options="5,0.5,0.0005" >档位</param>
        /// <returns>浮点数或报错信息</returns>
        string GetCurrent(string type)
        {
            a = false;
            k = false;
            try
            {
                Comport_();
                if (k == true)
                {
                    protocolStack.CloseConnection();
                    return "lỗi cổng COM/COM端口错误";
                }
                if (k != true)
                {
                    Run_1();
                }
                while (true)
                {
                    if (a == true)
                    {

                        break;
                    }
                }
                a = false;
                counter = 0;
                if (type == "0.5")
                {
                    float z = float.Parse(b1);
                    z = z * 1000;
                    return z + "";
                }
                else if (type == "0.0005")
                {
                    float z = float.Parse(b1);
                    return z + "";
                }
                else
                {
                    float z = float.Parse(b1);
                    z = z * 1000000;
                    return z + "";
                }

            }
            catch (Exception ex)
            {
                return false.ToString();
            }
        }
        /// <summary isPublicTestItem="true">
        /// 读取电压 GetVoltage
        /// </summary>
        /// <returns>浮点数或报错信息</returns>
        string GetVoltage()
        {
            a = false;
            k = false;
            try
            {
                Comport_();
                if (k == true)
                {
                    protocolStack.CloseConnection();
                    return "lỗi cổng COM/COM端口错误";
                }
                if (k != true)
                {
                    Run_2();
                }

                while (true)
                {
                    if (a == true)
                    {
                        break;
                    }
                }
                a = false;
                counter = 0;
                return b1;
            }
            catch (Exception ex)
            {
                return false.ToString();
            }
        }
        /// <summary isPublicTestItem="true">
        /// 读取功率 GetPower
        /// </summary>
        /// <returns>浮点数或报错信息</returns>
        string GetPower()
        {
            a = false;
            k = false;

            try
            {
                Comport_();
                if (k == true)
                {
                    protocolStack.CloseConnection();
                    return "lỗi cổng COM/COM端口错误";
                }
                if (k != true)
                {
                    Run_3();
                }

                while (true)
                {
                    if (a == true)
                    {
                        break;
                    }
                }
                a = false;
                counter = 0;
                return b1;
            }
            catch (Exception ex)
            {
                return false.ToString();
            }
        }
        private int counter = 0;
        private async void Comport_()
        {
            await InitProtocolStack();
        }
        ShizukuProtocol protocolStack;
        SerialPort targetPort;
        private async Task InitProtocolStack()
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length>0)
            {
                var devices = ShizukuDeviceScanHelper.GetAllValidDevices();
                if (devices.Count == 0)
                {
                    k = true;

                }
                protocolStack = new ShizukuProtocol(devices[0], () => { });
                await protocolStack.Reset();
            }
            else
            {
                k = true;
              
            } 
        }

        private async void Run_1()
        {
            await InitMeterDataReport1();
           
        }
        ShizukuProtocol.CallerDescriptor.Callback_t mdr_cb;
        USBMeterData_t USBMeterData_RealTime = new USBMeterData_t();
        ShizukuProtocolMeterData pMeterData;
        private void MeterDataReportCallback1(byte[] packet)
        {
           
            /*When a meter report packet is received, this method is called, it converts the packet into USBMeterData_RealTime*/
            byte[] payload = packet.Skip(9).Take(32).ToArray();
            ByteStructConverter byteStructConverter = new ByteStructConverter();
            USBMeterData_RealTime = (USBMeterData_t)byteStructConverter.BytesToStruct(payload, USBMeterData_RealTime.GetType());
            b1 = USBMeterData_RealTime.Current + "";
            counter++;

            if (counter >= 1)
            {
                // Tắt sự kiện MeterDataReportCallback bằng cách gỡ bỏ callback
                pMeterData.MeterDataReport_Setup(0, null, false);
                protocolStack.CloseConnection();
                 // Chuyển sang sự kiện run()
                 a = true;
            }
        }
        private async Task InitMeterDataReport1() 
        {
            /*Then you can read the meter data*/
            pMeterData = new ShizukuProtocolMeterData(protocolStack);
            /*Specify a callback when a meter data report packet is received*/
            mdr_cb = new ShizukuProtocol.CallerDescriptor.Callback_t(MeterDataReportCallback1);
            /*We request the meter to report meter data every 1000 miliseconds*/
            var task = pMeterData.MeterDataReport_Setup(100, mdr_cb, false);
        }
        private async void Run_2()
        {
            await InitMeterDataReport2();
        }
        private void MeterDataReportCallback2(byte[] packet)
        {
            counter++;
            /*When a meter report packet is received, this method is called, it converts the packet into USBMeterData_RealTime*/
            byte[] payload = packet.Skip(9).Take(32).ToArray();
            ByteStructConverter byteStructConverter = new ByteStructConverter();
            USBMeterData_RealTime = (USBMeterData_t)byteStructConverter.BytesToStruct(payload, USBMeterData_RealTime.GetType());
            b1 = USBMeterData_RealTime.Voltage + "";
          
            if (counter >= 1)
            {
                // Tắt sự kiện MeterDataReportCallback bằng cách gỡ bỏ callback
                pMeterData.MeterDataReport_Setup(0, null, false);
                protocolStack.CloseConnection();
                // Chuyển sang sự kiện run()
                a = true;
            }
        }
        private async Task InitMeterDataReport2()
        {
            /*Then you can read the meter data*/
            pMeterData = new ShizukuProtocolMeterData(protocolStack);
            /*Specify a callback when a meter data report packet is received*/
            mdr_cb = new ShizukuProtocol.CallerDescriptor.Callback_t(MeterDataReportCallback2);
            /*We request the meter to report meter data every 1000 miliseconds*/
            var task = pMeterData.MeterDataReport_Setup(100, mdr_cb, false);          
        }
        private async void Run_3()
        {
            await InitMeterDataReport3();
        }
        private void MeterDataReportCallback3(byte[] packet)
        {
            counter++;
            /*When a meter report packet is received, this method is called, it converts the packet into USBMeterData_RealTime*/
            byte[] payload = packet.Skip(9).Take(32).ToArray();
            ByteStructConverter byteStructConverter = new ByteStructConverter();
            USBMeterData_RealTime = (USBMeterData_t)byteStructConverter.BytesToStruct(payload, USBMeterData_RealTime.GetType());
            b1 = USBMeterData_RealTime.Power + "";
            if (counter >= 1)
            {
                // Tắt sự kiện MeterDataReportCallback bằng cách gỡ bỏ callback
                pMeterData.MeterDataReport_Setup(0, null, false);
                protocolStack.CloseConnection();
                a = true;
                // Chuyển sang sự kiện run()
            }
        }
        private async Task InitMeterDataReport3()
        {
            /*Then you can read the meter data*/
            pMeterData = new ShizukuProtocolMeterData(protocolStack);
            /*Specify a callback when a meter data report packet is received*/
            mdr_cb = new ShizukuProtocol.CallerDescriptor.Callback_t(MeterDataReportCallback3);
            /*We request the meter to report meter data every 1000 miliseconds*/
            var task = pMeterData.MeterDataReport_Setup(100, mdr_cb, false);   
        }
    }
}
