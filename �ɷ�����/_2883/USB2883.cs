using System;
using System.Runtime.InteropServices;
namespace USB2833
{
    /// <summary>
    /// USB2883
    /// </summary>
    internal partial class USB2883
    {
        // 本卡最多支持12路模拟量单端输入通道
        public const Int32 USB2883_MAX_AI_CHANNELS = 12;

        //***********************************************************
        // 用于AD采集的参数结构
        public struct USB2883_PARA_AD
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = USB2883_MAX_AI_CHANNELS)]
            public Int32[] bChannelArray;           // 采样通道选择阵列，分别控制6个通道，=TRUE表示该通道采样，否则不采样
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = USB2883_MAX_AI_CHANNELS)]
            public Int32[] InputRange;              // 模拟量输入量程选择(前六个通道量程必须一致，后六个通道量程必须一致)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = USB2883_MAX_AI_CHANNELS)]
            public Int32[] Gains;                   // 增益控制
            public Int32 Frequency;                 // 采集频率,单位为Hz, [1000, 250000]
            public Int32 TriggerMode;               // 触发模式选择
            public Int32 TriggerSource;             // 触发源选择
            public Int32 TriggerDir;                // 触发方向选择(正向/负向触发)
            public Int32 TrigLevelVolt;             // 触发电平(量程按模拟输入量程)
            public Int32 TrigWindow;                // 触发灵敏度设置uS,[50,1638]
            public Int32 ClockSource;               // 时钟源选择(内/外时钟源)
            public Int32 bClockOutput;              // 允许时钟输出到CLKOUT,=TRUE:允许时钟输出, =FALSE:禁止时钟输出
        }

        //***********************************************************
        // AD参数USB2883_PARA_AD中的Gains[x]使用的硬件增益选项
        public const Int32 USB2883_GAINS_1MULT = 0x00; // 1倍增益
        public const Int32 USB2883_GAINS_2MULT = 0x01; // 2倍增益
        public const Int32 USB2883_GAINS_4MULT = 0x02; // 4倍增益
        public const Int32 USB2883_GAINS_8MULT = 0x03; // 8倍增益

        //***********************************************************
        // AD硬件参数USB2883_PARA_AD中的InputRange量程所使用的选项
        public const Int32 USB2883_INPUT_N10000_P10000mV = 0x00;    // ±10000mV
        public const Int32 USB2883_INPUT_N5000_P5000mV = 0x01;      // ±5000mV

        //***********************************************************
        // AD硬件参数USB2883_PARA_AD中的TriggerMode成员变量所使用触发模式选项
        public const Int32 USB2883_TRIGMODE_EDGE = 0x00;    // 边沿触发
        public const Int32 USB2883_TRIGMODE_PULSE = 0x01;   // 电平触发

        //***********************************************************
        // AD硬件参数USB2883_PARA_AD中的TriggerSource触发源信号所使用的选项
        public const Int32 USB2883_TRIGMODE_SOFT = 0x00;    // 软件触发
        public const Int32 USB2883_TRIGSRC_DTR = 0x01;      // 选择DTR作为触发源
        public const Int32 USB2883_TRIGSRC_ATR = 0x02;      // 选择ATR作为触发源
        public const Int32 USB2883_TRIGSRC_TRIGGER = 0x03;  // Trigger信号触发（用于多卡同步）

        //***********************************************************
        // AD硬件参数USB2883_PARA_AD中的TriggerDir触发方向所使用的选项
        public const Int32 USB2883_TRIGDIR_NEGATIVE = 0x00;     // 负向触发(低电平/下降沿触发)
        public const Int32 USB2883_TRIGDIR_POSITIVE = 0x01;     // 正向触发(高电平/上升沿触发)
        public const Int32 USB2883_TRIGDIR_POSIT_NEGAT = 0x02;  // 正负向触发(高/低电平或上升/下降沿触发)

        //***********************************************************
        // AD硬件参数USB2883_PARA_AD中的ClockSource时钟源所使用的选项
        public const Int32 USB2883_CLOCKSRC_IN = 0x00;      // 内部时钟定时触发
        public const Int32 USB2883_CLOCKSRC_OUT = 0x01;     // 外部时钟定时触发(使用CN1上的CLKIN信号输入)

        //*************************************************************************************
        // 用于AD采样的实际硬件参数
        public struct USB2883_STATUS_AD
        {
            public Int32 bADEanble;     // AD是否已经使能，=TRUE:表示已使能，=FALSE:表示未使能
            public Int32 bTrigger;      // AD是否被触发，=TRUE:表示已被触发，=FALSE:表示未被触发
            public Int32 bHalf;         // 采集数据是否已达半满，=TRUE:表示已半满，=FALSE:表示未半满
        }

        //######################## 常规通用函数 #################################
        [DllImport("USB2883_32.DLL")]
        public static extern IntPtr USB2883_CreateDevice(Int32 DeviceLgcID);    // 创建设备对象(该函数使用系统内逻辑设备ID）

        [DllImport("USB2883_32.DLL")]
        public static extern IntPtr USB2883_CreateDeviceEx(Int32 DevicePhysID); // 使用物理ID创建设备对象

        [DllImport("USB2883_32.DLL")]
        public static extern IntPtr USB2883_GetDeviceCurrentID(IntPtr hDevice, ref Int32 DevicePhysID); // 取得当前设备的物理ID号

        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_SetDevicePhysID(                     // 设置当前设备的物理ID号,物理ID[0~255],需重新上电
                                                           IntPtr hDevice,      // 设备对象句柄,它由CreateDevice()函数创建
                                                           Int32 DevicePhysID); // 设置当前设备的物

        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_ResetDevice(IntPtr hDevice);         // 复位整个USB设备

        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_ReleaseDevice(IntPtr hDevice);       // 释放设备对象(关键函数)

        //####################### AD数据读取函数 #################################
        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_ADCalibration(IntPtr hDevice);           // AD自动校准函数

        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_InitDeviceAD(                              // 初始化设备,当返回TRUE后,设备即刻开始传输.
                                                        IntPtr hDevice,               // 设备对象句柄,它由CreateDevice()函数创建
                                                        ref USB2883_PARA_AD pADPara); // 硬件参数, 它仅在此函数中决定硬件状态

        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_StartDeviceAD(IntPtr hDevice);           // 在初始化之后，启动设备

        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_StopDeviceAD(IntPtr hDevice);           // 在启动设备之后，暂停设备


        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_ReadDeviceAD(                            // 从采集任务中读取采样数据(电压数据序列)(Read analog data from the task)
                                                        IntPtr hDevice,             // 设备对象句柄,它由CreateDevice()函数创建
                                                        UInt16[] ADBuffer,          // 模拟数据数组(电压数组),用于返回采样的电压数据，取值区间由各通道采样时的采样范围决定(单位:V)
                                                        Int32 nReadSizeWords,       // 读取AD数据的长度(字)
                                                        ref Int32 nRetSizeWords,    // 实际返回数据的长度(字),
                                                        Int32 bEnoughRtn,           // TRUE：不够读取点数返回0；FALSE:不够读取点数返回实际点数
                                                        ref Int32 nSurplusWords);   // 返回FIFO剩余点数

        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_ReleaseDeviceAD(IntPtr hDevice);           // 停止AD采集，释放AD对象所占资源

        //################# AD的硬件参数操作函数 ########################
        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_SaveParaAD(                                // 校验AO工作参数(Verify Parameter),建议在初始化生成任务前调用此函数校验各参数合法性
                                                      IntPtr hDevice,                 // 设备对象句柄,它由CreateDevice()函数创建
                                                      ref USB2883_PARA_AD pAOParam);  // AO工作参数

        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_LoadParaAD(                                // 从USB2883.ini中加载AO参数
                                                      IntPtr hDevice,                 // 设备对象句柄,它由CreateDevice()函数创建
                                                      ref USB2883_PARA_AD pAOParam);  // AO工作参数

        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_ResetParaAD(                              // 保存AO参数至USB2883.ini
                                                       IntPtr hDevice,               // 设备对象句柄,它由CreateDevice()函数创建
                                                       ref USB2883_PARA_AD pAOParam);// AO工作参数

        //####################### 数字I/O输入输出函数 #################################
        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_GetDeviceDI(                             // 取得开关量状态
                                                       IntPtr hDevice,              // 设备对象句柄,它由CreateDevice()函数创建
                                                       Byte[] bDISts);              // 开关输入状态(注意: 必须定义为16个字节元素的数组)
        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_SetDeviceDO(                            // 取得开关量状态
                                                       IntPtr hDevice,             // 设备对象句柄,它由CreateDevice()函数创建
                                                       Byte[] bDOSts);             // 开关输出状态(注意: 必须定义为16个字节元素的数组)

        //############################################################################
        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_GetDevVersion(                            // 获取设备固件及程序版本
                                                         IntPtr hDevice,             // 设备对象句柄,它由CreateDevice函数创建
                                                         ref UInt32 pulFmwVersion,   // 固件版本
                                                         ref UInt32 pulDriverVersion);// 驱动版本

        //############################ 线程操作函数 ################################
        [DllImport("USB2883_32.DLL")]
        public static extern IntPtr USB2883_CreateSystemEvent();    // 创建内核系统事件对象

        [DllImport("USB2883_32.DLL")]
        public static extern Int32 USB2883_ReleaseSystemEvent(IntPtr hEvent); // 释放内核事件对象
    }
}