using System;
using System.Runtime.InteropServices;
using USB2833;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace MerryDllFramework
{
    public class StartUSB2883
    {
        [DllImport("msvcrt.dll")]
        private static extern Int32 _getch();
        [DllImport("msvcrt.dll")]
        private static extern Int32 _kbhit();
        private static USB2883.USB2883_PARA_AD ADPara; // 硬件参数
        public double[] avg;
        /// <summary>
        /// 启动数据卡
        /// </summary>
        /// <param name="DeviceID">数据卡ID</param>
        /// <param name="itmes">测试次数</param>
        /// <param name="frequency">测试频率</param>
        /// <returns></returns>
        public bool Start(string DeviceID, string itmes)
        {
            int itme = Convert.ToInt32(itmes);
            double[] avg = new double[12];
            IntPtr hDevice;
            Int32 InputRange;
            Int32 DeviceLgcID;
            ADPara.bChannelArray = new Int32[12];
            ADPara.InputRange = new Int32[12];
            ADPara.Gains = new Int32[12];
            Int32 nReadSizeWords;   // 每次读取AD数据的长度(字)
            Int32 nRetSizeWords = 0;
            Int32 nSurplusWords = 0;
            Int32 ulChannelCount = 0;

            int nADChannel = 0;
            UInt16[] ADBuffer = new UInt16[1024 * 16]; // 分配缓冲区(存储原始数据)
            UInt16 ADData;
            Single fVolt = 0.0f;
            DeviceLgcID = Convert.ToInt32(DeviceID);//(1，2,3)
            hDevice = USB2883.USB2883_CreateDeviceEx(DeviceLgcID); // 创建设备对象
            if (hDevice == (IntPtr)(-1))
            {
                return false; // 如果创建设备对象失败，则返回
            }
            //量程
            InputRange = 0;
            // 预置硬件参数
            ADPara.Frequency = Convert.ToInt32(250000);				// 采样频率(Hz)
            for (UInt32 nChannel = 0; nChannel < 12; nChannel++)
            {
                ADPara.bChannelArray[nChannel] = 1;
                ADPara.InputRange[nChannel] = InputRange;			// 模拟量输入量程范围
                ADPara.Gains[nChannel] = USB2883.USB2883_GAINS_1MULT;
            }
            ADPara.TriggerMode = USB2883.USB2883_TRIGMODE_EDGE; // 边沿触发
            ADPara.TriggerSource = USB2883.USB2883_TRIGMODE_SOFT;	// 选择ATR作为触发源
            ADPara.TriggerDir = USB2883.USB2883_TRIGDIR_NEGATIVE; // 下降沿触发
            ADPara.TrigLevelVolt = 0;
            ADPara.TrigWindow = 40;
            ADPara.ClockSource = USB2883.USB2883_CLOCKSRC_IN;	// 使用内部时钟
            ADPara.bClockOutput = 1;

            if (!(USB2883.USB2883_InitDeviceAD(hDevice, ref ADPara) > 0)) // 初始化硬件
            {
                return false; // 如果创建设备对象失败，则返回
            }
            for (Int32 nChannel = 0; nChannel < 12; nChannel++)
            {
                if (ADPara.bChannelArray[nChannel] == 1)
                {
                    ulChannelCount++;
                }
            }
            nReadSizeWords = 16384 - 16384 % ulChannelCount;

            // 开始采集任务
            USB2883.USB2883_StartDeviceAD(hDevice);
            // Boolean bFirstWait = true; // 为每次等待只显示一次提示
            // 循环读取AI数据
            int i = 0;
            while (_kbhit() == 0)
            {
                if (USB2883.USB2883_ReadDeviceAD(hDevice, ADBuffer, nReadSizeWords, ref nRetSizeWords, 1, ref nSurplusWords) == 0)
                {
                    _getch();
                }
                if (nRetSizeWords <= 0)
                {
                    continue;
                }
                for (int Index = 0; Index < 64;)
                {
                    for (nADChannel = 0; nADChannel < 12; nADChannel++)
                    {
                        if (i >= itme) goto Exit;
                        if (_kbhit() != 0) goto Exit;
                        ADData = (UInt16)((ADBuffer[Index] ^ 0x800) & 0xFFF);
                        // 将原码转换为电压值
                        switch (InputRange)
                        {
                            case USB2883.USB2883_INPUT_N10000_P10000mV: // -10000mV - +10000mV
                                fVolt = (float)((20000.0 / 4096) * ADData - 10000.0);
                                break;
                            case USB2883.USB2883_INPUT_N5000_P5000mV:	// -5000mV - +5000mV
                                fVolt = (float)((10000.0 / 4096) * ADData - 5000.0);
                                break;
                            default:
                                break;
                        }
                        avg[nADChannel] += fVolt;
                        Index++;
                    }
                    i++;
                    Thread.Sleep(8);
                }
            }
            Exit:

            for (int j = 0; j < avg.Length; j++)
            {
                avg[j] = avg[j] / itme;
            }
            this.avg = avg;
            var dllpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // 停止AD
            USB2883.USB2883_StopDeviceAD(hDevice);
            // 释放AD
            USB2883.USB2883_ReleaseDeviceAD(hDevice);
            // 释放设备对象
            USB2883.USB2883_ReleaseDevice(hDevice);
            try
            {
                using (FileStream fs = new FileStream($@"{dllpath}\USB2883_id{DeviceID}.txt", FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        for (int c = 0; c < avg.Length; c++)
                        {
                            sw.WriteLine("id: " + DeviceID + "\t通道：" + c + "\t电压值：" + Math.Round(avg[c], 2) + "mV");
                        }
                    }
                }

            }
            catch { }
            return true;
        }


    }
}
