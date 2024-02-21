using Microsoft.VisualBasic;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Airoha.AdjustANC.CurvesInfo;
using static MerryTest.testitem.UsbInfo;

namespace MerryTest.testitem
{
    /// <summary>
    /// 获取句柄类
    /// </summary>
    public class GetHandle
    {
        #region 参数及引用区
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CloseHandle(IntPtr hFile);
        [DllImport("hid.dll")]//获得GUID
        private static extern void HidD_GetHidGuid(ref Guid hidGuid);
        [DllImport("setupapi.dll", SetLastError = true)]//过滤设备，获取需要的设备
        private static extern IntPtr SetupDiGetClassDevs(ref Guid gClass, [MarshalAs(UnmanagedType.LPStr)] string strEnumerator, IntPtr hParent, Digcf nFlags);
        private enum Digcf  //3
        {
            DigcfDefault = 0x1,//返回与系统默认设备相关的设备
            DigcfPresent = 0x2,//返回当前存在的设备
            DigcfAllclasses = 0x4,//返回所有安装的设备
            DigcfProfile = 0x8,//只返回当前硬件配置文件的设备
            DigcfDeviceinterface = 0x10//返回所有支持的设备
        }
        internal struct SpDeviceInterfaceData
        {
            internal int Size;
            internal Guid InterfaceClassGuid;
            internal int Flags;
            internal int Reserved;
        }
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, uint devInfo, ref Guid interfaceClassGuid, uint memberIndex, ref SpDeviceInterfaceData deviceInterfaceData);
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SpDeviceInterfaceData deviceInterfaceData, IntPtr deviceInterfaceDetailData,
                                                                 uint deviceInterfaceDetailDataSize, ref uint requiredSize, IntPtr deviceInfoData);
        [StructLayout(LayoutKind.Sequential, Pack = 2)]//2
        internal struct SpDeviceInterfaceDetailData
        {
            internal int Size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string DevicePath;
        }
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_WRITE = 0x2;
        private const uint FILE_SHARE_READ = 0x1;
        private const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        private const uint OPEN_EXISTING = 3;
        public IntPtr Handle = IntPtr.Zero;
        public string Path = "";
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPStr)] string strName, uint nAccess, uint nShareMode, IntPtr lpSecurity, uint nCreationFlags, uint nAttributes, IntPtr lpTemplate);
        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr lpDeviceInfoSet, ref SpDeviceInterfaceData oInterfaceData, ref SpDeviceInterfaceDetailData oDetailData, uint nDeviceInterfaceDetailDataSize, ref uint nRequiredSize, IntPtr lpDeviceInfoData);
        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern IntPtr SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);
        #endregion
        #region 获取装置路径
        public bool GetHidDevicePath(string headsetPID, string headsetVID, string donglePID, string dongleVID)
        {
            var hidGuid = Guid.Empty;
            var TXregPid = new Regex(donglePID.ToLower());
            var TXregVid = new Regex(dongleVID.ToLower());
            var RXregPid = new Regex(headsetPID.ToLower());
            var RXregVid = new Regex(headsetVID.ToLower());
            Regex[] ExternAgs = { new Regex("col01"), new Regex("col02"), new Regex("col03"), new Regex("col04"), new Regex("col05"), new Regex("col06") };
            var result = true;
            var resultflag = false;
            var hidHandle = IntPtr.Zero;
            uint deviceSerialNumber = 0;
            HidD_GetHidGuid(ref hidGuid);

            var hDevInfo = SetupDiGetClassDevs(ref hidGuid, null, IntPtr.Zero, Digcf.DigcfPresent | Digcf.DigcfDeviceinterface);//过滤设备，获取需要的设备
            try
            {
                var deviceInterfaceData = new SpDeviceInterfaceData();
                deviceInterfaceData.Size = Marshal.SizeOf(deviceInterfaceData);
                while (result)
                {
                    result = SetupDiEnumDeviceInterfaces(hDevInfo, 0, ref hidGuid, deviceSerialNumber, ref deviceInterfaceData);//获取设备，true获取到                   
                    if (result)
                    {
                        uint nRequiredSize = 0;
                        SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, IntPtr.Zero, 0, ref nRequiredSize, IntPtr.Zero);

                        var detailData = new SpDeviceInterfaceDetailData();
                        detailData.Size = 5;	// hardcoded to 5! Sorry, but this works and trying more future proof versions by setting the size to the struct sizeof failed miserably. If you manage to sort it, mail me! Thx
                        result = SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, ref detailData, nRequiredSize, ref nRequiredSize, IntPtr.Zero);//获取接口的详细信息，必须调用两次，一次返回长度，二次获取数据

                        if (result)
                        {
                            if (detailData.DevicePath != null)
                            {
                                Match MatchTXPid = TXregPid.Match(detailData.DevicePath);
                                Match MatchRxPid = RXregPid.Match(detailData.DevicePath);
                                Match MatchTXVID = TXregVid.Match(detailData.DevicePath);
                                Match MatchRXVID = RXregVid.Match(detailData.DevicePath);
                                Match[] mathExternAgs = new Match[6];
                                for (int i = 0; i < 6; i++)
                                {
                                    mathExternAgs[i] = ExternAgs[i].Match(detailData.DevicePath);
                                }
                                if (MatchTXPid.Success && MatchTXVID.Success)
                                {
                                    for (int i = 0; i < 6; i++)
                                    {
                                        if (mathExternAgs[i].Success)
                                        {
                                            donglepath[i] = detailData.DevicePath;
                                            resultflag = true;
                                        }
                                    }
                                }
                                else if (MatchRxPid.Success && MatchRXVID.Success)
                                {
                                    for (int i = 0; i < 6; i++)
                                    {
                                        if (mathExternAgs[i].Success)
                                        {
                                            headsetpath[i] = detailData.DevicePath;
                                            resultflag = true;
                                        }
                                    }
                                }
                            }
                            deviceSerialNumber++;
                        }
                    }
                }
            }
            catch (Exception)
            {

                resultflag = false;
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(hDevInfo);
            }
            return resultflag;
        }

        UsbInfo UI = new UsbInfo();
        public bool GetHidDevicePathIlst(List<string> existPath, out Dictionary<string, Info> deviceList)
        {
            deviceList = new Dictionary<string, Info>();
            var hidGuid = Guid.Empty;

            var result = true;
            var resultflag = false;
            var hidHandle = IntPtr.Zero;
            uint deviceSerialNumber = 0;
            HidD_GetHidGuid(ref hidGuid);

            var hDevInfo = SetupDiGetClassDevs(ref hidGuid, null, IntPtr.Zero, Digcf.DigcfPresent | Digcf.DigcfDeviceinterface);//过滤设备，获取需要的设备
            try
            {
                var deviceInterfaceData = new SpDeviceInterfaceData();
                deviceInterfaceData.Size = Marshal.SizeOf(deviceInterfaceData);
                while (result)
                {
                    result = SetupDiEnumDeviceInterfaces(hDevInfo, 0, ref hidGuid, deviceSerialNumber, ref deviceInterfaceData);//获取设备，true获取到                   
                    if (result)
                    {
                        uint nRequiredSize = 0;
                        SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, IntPtr.Zero, 0, ref nRequiredSize, IntPtr.Zero);

                        var detailData = new SpDeviceInterfaceDetailData();
                        detailData.Size = 5;	// hardcoded to 5! Sorry, but this works and trying more future proof versions by setting the size to the struct sizeof failed miserably. If you manage to sort it, mail me! Thx
                        result = SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, ref detailData, nRequiredSize, ref nRequiredSize, IntPtr.Zero);//获取接口的详细信息，必须调用两次，一次返回长度，二次获取数据

                        if (result)
                        {
                            if (detailData.DevicePath != null)
                            {
                                bool existDev = false;
                                existPath.ForEach(item =>
                                {
                                    if (item.Contains(detailData.DevicePath))
                                    {
                                        existDev = true;
                                        return;
                                    }
                                });
                                if (!existDev)
                                {
                                    UI.GetDeviceInfo(detailData.DevicePath, out Info _info);
                                    deviceList.Add(detailData.DevicePath, _info);
                                }
                                else
                                {
                                    deviceList.Add(detailData.DevicePath, new Info()
                                    {
                                        Path = detailData.DevicePath
                                    });
                                }
                            }
                            deviceSerialNumber++;
                        }
                    }
                }
            }
            catch (Exception)
            {

                resultflag = false;
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(hDevInfo);
            }
            return resultflag;
        }



        #endregion
        #region 将路径转换成句柄
        public static IntPtr GetHidDeviceHandle(string HidDevicePath)
        {
            IntPtr _HIDWriteHandle = IntPtr.Zero;
            if (!String.IsNullOrEmpty(HidDevicePath))
            {

                _HIDWriteHandle = CreateFile(HidDevicePath, GENERIC_WRITE | GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            }
            return _HIDWriteHandle;
        }
        #endregion
        #region 获取装置路径1
        public bool GetHidDevicePath(string PID, string VID, string col)
        {
            Guid hidGuid = Guid.Empty;
            Regex TXregPid = new Regex(PID.ToLower());
            Regex TXregVid = new Regex(VID.ToLower());
            Regex ExternAgs = new Regex(col.ToLower());
            bool result = true;
            IntPtr hidHandle = IntPtr.Zero;
            UInt32 deviceSerialNumber = 0;
            HidD_GetHidGuid(ref hidGuid);

            IntPtr hDevInfo = SetupDiGetClassDevs(ref hidGuid, null, IntPtr.Zero, Digcf.DigcfPresent | Digcf.DigcfDeviceinterface);//过滤设备，获取需要的设备
            try
            {
                SpDeviceInterfaceData deviceInterfaceData = new SpDeviceInterfaceData();
                deviceInterfaceData.Size = Marshal.SizeOf(deviceInterfaceData);
                while (result)
                {

                    result = SetupDiEnumDeviceInterfaces(hDevInfo, 0, ref hidGuid, deviceSerialNumber, ref deviceInterfaceData);//获取设备，true获取到                   
                    if (result)
                    {
                        uint nRequiredSize = 0;
                        SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, IntPtr.Zero, 0, ref nRequiredSize, IntPtr.Zero);

                        SpDeviceInterfaceDetailData detailData = new SpDeviceInterfaceDetailData();
                        detailData.Size = 5;	// hardcoded to 5! Sorry, but this works and trying more future proof versions by setting the size to the struct sizeof failed miserably. If you manage to sort it, mail me! Thx
                        result = SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, ref detailData, nRequiredSize, ref nRequiredSize, IntPtr.Zero);//获取接口的详细信息，必须调用两次，一次返回长度，二次获取数据

                        if (result)
                        {
                            if (detailData.DevicePath != null)
                            {
                                Match MatchPid = TXregPid.Match(detailData.DevicePath);
                                Match MatchVID = TXregVid.Match(detailData.DevicePath);
                                Match mathExternAgs = ExternAgs.Match(detailData.DevicePath);

                                if (MatchPid.Success && MatchVID.Success && mathExternAgs.Success)
                                {
                                    Path = detailData.DevicePath;
                                    return true;
                                }
                            }
                            deviceSerialNumber++;
                        }
                    }
                }
            }
            catch (Exception)
            {

                return false;
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(hDevInfo);
            }
            return false;
        }
        public bool GetHidDevicePath(string PID, string VID, int I, int O)
        {
            Guid hidGuid = Guid.Empty;
            Regex TXregPid = new Regex(PID.ToLower());
            Regex TXregVid = new Regex(VID.ToLower());
            bool result = true;
            IntPtr hidHandle = IntPtr.Zero;
            UInt32 deviceSerialNumber = 0;
            HidD_GetHidGuid(ref hidGuid);

            IntPtr hDevInfo = SetupDiGetClassDevs(ref hidGuid, null, IntPtr.Zero, Digcf.DigcfPresent | Digcf.DigcfDeviceinterface);//过滤设备，获取需要的设备
            try
            {
                SpDeviceInterfaceData deviceInterfaceData = new SpDeviceInterfaceData();
                deviceInterfaceData.Size = Marshal.SizeOf(deviceInterfaceData);
                while (result)
                {

                    result = SetupDiEnumDeviceInterfaces(hDevInfo, 0, ref hidGuid, deviceSerialNumber, ref deviceInterfaceData);//获取设备，true获取到                   
                    if (result)
                    {
                        uint nRequiredSize = 0;
                        SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, IntPtr.Zero, 0, ref nRequiredSize, IntPtr.Zero);

                        SpDeviceInterfaceDetailData detailData = new SpDeviceInterfaceDetailData();
                        detailData.Size = 5;	// hardcoded to 5! Sorry, but this works and trying more future proof versions by setting the size to the struct sizeof failed miserably. If you manage to sort it, mail me! Thx
                        result = SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, ref detailData, nRequiredSize, ref nRequiredSize, IntPtr.Zero);//获取接口的详细信息，必须调用两次，一次返回长度，二次获取数据

                        if (result)
                        {
                            if (detailData.DevicePath != null)
                            {
                                Match MatchPid = TXregPid.Match(detailData.DevicePath);
                                Match MatchVID = TXregVid.Match(detailData.DevicePath);
                                if (MatchPid.Success && MatchVID.Success)
                                {
                                    //将装置路径转换成加密设备指针
                                    using (SafeFileHandle handle = new SafeFileHandle(User32.CreateFile(detailData.DevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, User32.EFileAttributes.Overlapped, IntPtr.Zero), true))
                                    {
                                        if (handle.IsInvalid)
                                            continue;
                                        //加密的指针转换成int类型
                                        int HidDeviceObject = (int)handle.DangerousGetHandle();
                                        //加密指针无效就出去
                                        HID.HIDD_ATTRIBUTES hidd_attributes = new HID.HIDD_ATTRIBUTES();
                                        //计算空间大小
                                        hidd_attributes.Size = (uint)Marshal.SizeOf(hidd_attributes);
                                        IntPtr _PreparsedData = IntPtr.Zero;
                                        //        根据流获取准备好的数据                根据加密指针创建一个写读的流
                                        if (HID.HidD_GetPreparsedData((int)new FileStream(handle, FileAccess.ReadWrite, 8, true).SafeFileHandle.DangerousGetHandle(), ref _PreparsedData))
                                        {
                                            //获取写入和读取的长度信息
                                            HID.HIDP_CAPS _Caps = new HID.HIDP_CAPS();
                                            //名称
                                            HID.NTSTATUS ntstatus = HID.HidP_GetCaps(_PreparsedData, ref _Caps);
                                            if (I == _Caps.InputReportByteLength && O == _Caps.OutputReportByteLength)
                                            {
                                                Path = detailData.DevicePath;
                                                return true;
                                            }

                                        }

                                    }
                                }
                            }
                            deviceSerialNumber++;
                        }
                    }
                }
            }
            catch (Exception)
            {

                return false;
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(hDevInfo);
            }
            return false;
        }

        #endregion

        #region 对外方法
        /// <summary>
        /// Headset句柄对象数组
        /// </summary>
        public IntPtr[] headsethandle = new IntPtr[6];
        /// <summary>
        /// Dongle句柄对象数组
        /// </summary>
        public IntPtr[] donglehandle = new IntPtr[6];
        /// <summary>
        /// Headset句柄地址
        /// </summary>
        public string[] headsetpath = new string[6];
        /// <summary>
        /// Dongle句柄地址
        /// </summary>
        public string[] donglepath = new string[6];
        /// <summary>
        /// 抓取句柄 donglePIDVID不能为NULL
        /// </summary>
        /// <param name="headsetPID">headsetPID</param>
        /// <param name="headsetVID">headsetVID</param>
        /// <param name="donglePID">donglePID</param>
        /// <param name="dongleVID">dongleVID</param>
        /// <returns>抓取是否成功</returns>
        public bool gethandle(string headsetPID, string headsetVID, string donglePID, string dongleVID)
        {
            bool flag = true;
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    headsetpath[i] = "";
                    donglepath[i] = "";
                }
                flag = GetHidDevicePath(headsetPID, headsetVID, donglePID, dongleVID);
                for (int i = 0; i < 6; i++)
                {
                    headsethandle[i] = GetHidDeviceHandle(headsetpath[i]);
                    donglehandle[i] = GetHidDeviceHandle(donglepath[i]);
                }

            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }
        public bool gethandle(string PID, string VID, string col)
        {
            bool flag;
            try
            {
                Path = "";
                Handle = IntPtr.Zero;
                flag = GetHidDevicePath(PID, VID, col);
                if (Path == "") return false;
                Handle = GetHidDeviceHandle(Path);
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }
        public bool CloseHandle(ref IntPtr Handle)
        {
            if (Handle != IntPtr.Zero) CloseHandle(Handle);
            Handle = IntPtr.Zero;
            return true;
        }
        #endregion


    }
}
