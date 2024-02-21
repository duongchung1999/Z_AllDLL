using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShizukuLib;
using System.IO.Ports;
using System.Threading;
using static ShizukuLib.ShizukuProtocolMeterData;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        Dictionary<string, object> OnceConfig;
        Dictionary<string, object> Config;
        public object Interface(Dictionary<string, object> Config) => this.Config = Config;
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Shizuku";
            string dllfunction = "Dll功能说明 ：USB测电量";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllHistoryVersion2 = "                     ：21.8.14.0";
            string dllVersion = "当前Dll版本：22.8.4";
            string dllChangeInfo = "Dll改动信息：";
            string[] info = { dllname, dllfunction, dllHistoryVersion, dllHistoryVersion2,
                dllVersion, dllChangeInfo
            };


            return info;
        }
        public string Run(object[] Command)
        {
            List<string> cmd = new List<string>();
            foreach (object item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>))
                    OnceConfig = (Dictionary<string, object>)item;
                if (type == typeof(string))
                {
                    cmd.AddRange(item.ToString().Split('&'));
                    for (int i = 0; i < cmd.Count; i++) cmd[i] = cmd[i].Split('=')[1].Trim();
                }
            }
            switch (cmd[1])
            {
                case "CurrentToA": return CurrentToA();
                case "CurrentTomA": return CurrentTomA();
                case "CurrentTouA": return CurrentTouA();

                default: return "Connend Error False";

            }
        }

        string Current(string nulber, int Modulation)
        {
            try
            {
                /*First scan for devices*/
                var devices = ShizukuDeviceScanHelper.GetAllValidDevices();

                if (devices.Count == 0)
                    return "not found Device False";

                /*Pick a device and construct a ShizukuProtocol object, which 
                             handles low layer communications with the multimeter.
                             In this case we just pick the first one for demo purpose.*/
                protocolStack = new ShizukuProtocol(devices[0], () => { });
                /*It's recommended to send a reset command before any operation*/
                protocolStack.Reset();
                InitMeterDataReport();
                Thread.Sleep(2500);
                return (USBMeterData_RealTime.Current * Modulation).ToString("N5");


            }
            catch (Exception ex)
            {

                return $"{ex.Message} False";
            }
            finally
            {
                protocolStack?.CloseConnection();

            }


        }
        /// <summary>
        /// 读取电流 以安为单位
        /// </summary>
        /// <returns></returns>
        public string CurrentToA()
        => Current("", 1);

        /// <summary>
        /// 读取电流 以毫安为单位
        /// </summary>
        /// <returns></returns>
        public string CurrentTomA()
           => Current("", 1000);
        /// <summary>
        /// 读取电流 以微安为单位
        /// </summary>
        /// <returns></returns>
        public string CurrentTouA()
            => Current("", 1000 * 1000);

        ShizukuProtocol protocolStack;
        SerialPort targetPort;
        ShizukuProtocol.CallerDescriptor.Callback_t mdr_cb;
        USBMeterData_t USBMeterData_RealTime = new USBMeterData_t();
        ShizukuProtocolMeterData pMeterData;

        void MeterDataReportCallback(byte[] packet)
        {
            /*When a meter report packet is received, this method is called, it converts the packet into USBMeterData_RealTime*/

            byte[] payload = packet.Skip(9).Take(32).ToArray();

            ByteStructConverter byteStructConverter = new ByteStructConverter();

            USBMeterData_RealTime = (USBMeterData_t)byteStructConverter.BytesToStruct(payload, USBMeterData_RealTime.GetType());
        }

        async Task InitMeterDataReport()
        {
            /*Then you can read the meter data*/

            pMeterData = new ShizukuProtocolMeterData(protocolStack);

            /*Specify a callback when a meter data report packet is received*/

            mdr_cb = new ShizukuProtocol.CallerDescriptor.Callback_t(MeterDataReportCallback);

            /*We request the meter to report meter data every 1000 miliseconds*/

            await pMeterData.MeterDataReport_Setup(1000, mdr_cb, false);

        }

    }
}
