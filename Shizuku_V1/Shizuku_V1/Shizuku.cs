using ShizukuLib;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ShizukuLib.ShizukuProtocolMeterData;

namespace Shizuku_V1
{
    internal class Shizuku : IDisposable
    {
        public Shizuku(bool onceFlag)
        {
            this.onceFlag = onceFlag;
            USBMeterData_RealTime = new USBMeterData_t();
        }
        bool onceFlag = true;
        bool isconnect = false;
        public string Current(string number, double Modulation)
        {
            try
            {
                if (onceFlag)
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
                    onceFlag = false;

                }
                for (int i = 0; i < 50; i++)
                {
                    Thread.Sleep(100);
                    if (isconnect)
                        break;
                }
                if (!isconnect)

                    return "Connect Device False";
                isconnect = false;

                return Math.Round(USBMeterData_RealTime.Current * Modulation, 2).ToString();
            }
            catch (Exception ex)
            {

                return $"{ex.Message} False";
            }

        }
        public string Voltage(string number, double Modulation, int Round)
        {
            try
            {
                if (onceFlag)
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
                    onceFlag = false;

                }
                for (int i = 0; i < 50; i++)
                {
                    Thread.Sleep(100);
                    if (isconnect)
                        break;
                }
                if (!isconnect)

                    return "Connect Device False";
                isconnect = false;

                double VoltageValue = Math.Round(USBMeterData_RealTime.Voltage * Modulation, Round);
                return VoltageValue.ToString();
            }
            catch (Exception ex)
            {

                return $"{ex.Message} False";
            }

        }


        public string ReadValue(string Mode, string number, double Modulation, int Round)
        {
            try
            {
                isconnect = false;
                if (onceFlag)
                {
                    /*First scan for devices*/
                    var devices = ShizukuDeviceScanHelper.GetAllValidDevices();

                    if (devices.Count == 0)
                        return "not found Device False";
                    int index = -1;
                    for (int i = 0; i < devices.Count; i++)
                    {
                        if (devices[i].SerialNo.Contains(number))
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index == -1)
                        return "not found Device False";
                    /*Pick a device and construct a ShizukuProtocol object, which 
                                 handles low layer communications with the multimeter.
                                 In this case we just pick the first one for demo purpose.*/
                    protocolStack = new ShizukuProtocol(devices[index], () => { });
                    /*It's recommended to send a reset command before any operation*/
                    protocolStack.Reset();
                    InitMeterDataReport();
                    onceFlag = false;

                }
                for (int i = 0; i < 50; i++)
                {
                    Thread.Sleep(100);
                    if (isconnect)
                        break;
                }
                if (!isconnect)
                    return "Connect Device False";
                double Value = double.NaN;
                switch (Mode)
                {
                    case "Current":
                        Value = USBMeterData_RealTime.Current;
                        break;
                    case "Voltage":
                        Value = USBMeterData_RealTime.Voltage;
                        break;
                    default:
                        break;
                }

                return Math.Round(Value * Modulation, Round).ToString();
            }
            catch (Exception ex)
            {

                return $"{ex.Message} False";
            }

        }













        ShizukuProtocol protocolStack;
        SerialPort targetPort;
        ShizukuProtocol.CallerDescriptor.Callback_t mdr_cb;
        USBMeterData_t USBMeterData_RealTime = null;
        ShizukuProtocolMeterData pMeterData;

        void MeterDataReportCallback(byte[] packet)
        {

            /*When a meter report packet is received, this method is called, it converts the packet into USBMeterData_RealTime*/

            byte[] payload = packet.Skip(9).Take(32).ToArray();

            ByteStructConverter byteStructConverter = new ByteStructConverter();

            USBMeterData_RealTime = (USBMeterData_t)byteStructConverter.BytesToStruct(payload, USBMeterData_RealTime.GetType());
            isconnect = true;
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

        public void Dispose()
        {
            protocolStack?.CloseConnection();
            protocolStack = null;
            targetPort?.Dispose();
            targetPort = null;
            mdr_cb = null;
            USBMeterData_RealTime = null;
            pMeterData = null;

        }
        ~Shizuku()
        {

        }
    }
}
