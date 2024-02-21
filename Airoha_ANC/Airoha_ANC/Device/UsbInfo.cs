using Microsoft.VisualBasic;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static MerryTest.testitem.UsbInfo;

namespace MerryTest.testitem
{
    public class UsbInfo
    {
        public struct Info
        {
            public int I;
            public int F;
            public int O;
            public string Path;
            public IntPtr Handle;
            public string VendorID;
            public string ProductID;
            public string ProductName;
            public string ManufacturerName;
            public string SerialNumber;
            public string Descriptor;
        }

        public string GetDeviceInfo(string Path, out Info info)
        {

            info = new Info();
            //将装置路径转换成加密设备指针
            SafeFileHandle handle = new SafeFileHandle(User32.CreateFile(Path, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, User32.EFileAttributes.Overlapped, IntPtr.Zero), true);

            try
            {
                //加密的指针转换成int类型
                int HidDeviceObject = (int)handle.DangerousGetHandle();
                //加密指针无效就出去
                if (handle.IsInvalid)
                    return "Handle invalid False";
                info.Path = Path;

                //创建一个存储PID VID的对象结构体
                HID.HIDD_ATTRIBUTES hidd_attributes = new HID.HIDD_ATTRIBUTES();
                //计算空间大小
                hidd_attributes.Size = (uint)Marshal.SizeOf(hidd_attributes);
                string _Manufacturer = Strings.StrDup(0x7f, '\0');
                //获取产品的供应商名称
                if (HID.HidD_GetManufacturerString(HidDeviceObject, _Manufacturer, (uint)(_Manufacturer.Length * 2)))
                    info.ManufacturerName = _Manufacturer.TrimEnd(new char[] { '\0' });
                //产品名称
                string _Product = Strings.StrDup(0x7f, '\0');
                if (HID.HidD_GetProductString(HidDeviceObject, _Product, (uint)(_Product.Length * 2)))
                    info.ProductName = _Product.TrimEnd(new char[] { '\0' });
                //获取GUID
                string _Serial = Strings.StrDup(0x7f, '\0');
                if (HID.HidD_GetSerialNumberString(HidDeviceObject, _Serial, (uint)(_Serial.Length * 2)))
                    info.SerialNumber = _Serial.TrimEnd(new char[] { '\0' });
                //获取产品描述
                string _Descriptor = Strings.StrDup(0x7f, '\0');
                if (HID.HidD_GetPhysicalDescriptor(HidDeviceObject, _Descriptor, (uint)(_Descriptor.Length * 2)))
                    info.Descriptor = _Descriptor.TrimEnd(new char[] { '\0' });
                //根据加密设备的指针获取属性 PID VID
                if (HID.HidD_GetAttributes(HidDeviceObject, ref hidd_attributes))
                {
                    info.ProductID = hidd_attributes.ProductID.ToString("X2").PadLeft(4, '0');
                    info.VendorID = hidd_attributes.VendorID.ToString("X2").PadLeft(4, '0');
                    IntPtr _PreparsedData = IntPtr.Zero;
                    //        根据流获取准备好的数据                根据加密指针创建一个写读的流
                    if (HID.HidD_GetPreparsedData((int)new FileStream(handle, FileAccess.ReadWrite, 8, true).SafeFileHandle.DangerousGetHandle(), ref _PreparsedData))
                    {
                        //获取写入和读取的长度信息
                        HID.HIDP_CAPS _Caps = new HID.HIDP_CAPS();
                        //名称
                        HID.NTSTATUS ntstatus = HID.HidP_GetCaps(_PreparsedData, ref _Caps);
                        info.I = _Caps.InputReportByteLength;
                        info.F = _Caps.FeatureReportByteLength;
                        info.O = _Caps.OutputReportByteLength;
                        return "True";
                    }
                }
                //供应商名称

                return "True";
            }
            finally
            {
                handle.Dispose();
            }
           
        }
        public bool GetHandle(ref Info info)
        {
            CloseHandle(ref info.Handle);
            if (!String.IsNullOrEmpty(info.Path))
            {
                info.Handle = HID.CreateFile(info.Path, HID.GENERIC_WRITE | HID.GENERIC_READ, HID.FILE_SHARE_READ | HID.FILE_SHARE_WRITE, IntPtr.Zero, HID.OPEN_EXISTING, 0, IntPtr.Zero);
                return info.Handle != IntPtr.Zero;
            }
            return false;
        }

        public bool CloseHandle(ref IntPtr Handle)
        {
            if (Handle != IntPtr.Zero) HID.CloseHandle(Handle);
            Handle = IntPtr.Zero;
            return true;
        }
    }
    internal sealed class User32
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(string fileName, FileAccess fileAccess, FileShare fileShare, IntPtr securityAttributes, FileMode creationDisposition, EFileAttributes flags, IntPtr template);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, DeviceNotifyEnum Flags);
        [DllImport("user32.dll")]
        public static extern bool UnregisterDeviceNotification(IntPtr Handle);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public User32.DeviceTypeEnum dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public string dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_HANDLE
        {
            public int dbch_size;
            public User32.DeviceTypeEnum dbch_devicetype;
            public int dbch_reserved;
            public int dbch_handle;
            public int dbch_hdevnotify;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public User32.DeviceTypeEnum dbch_devicetype;
            public int dbch_reserved;
        }

        public enum DeviceNotifyEnum
        {
            ALL_INTERFACE_CLASSES = 4,
            SERVICE_HANDLE = 1,
            WINDOW_HANDLE = 0
        }

        public enum DeviceTypeEnum
        {
            DEVICEINTERFACE = 5,
            HANDLE = 6,
            OEM = 0,
            PORT = 3,
            VOLUME = 2
        }

        [Flags]
        public enum EFileAttributes : uint
        {
            Archive = 0x20,
            BackupSemantics = 0x2000000,
            Compressed = 0x800,
            DeleteOnClose = 0x4000000,
            Device = 0x40,
            Directory = 0x10,
            Encrypted = 0x4000,
            FirstPipeInstance = 0x80000,
            Hidden = 2,
            NoBuffering = 0x20000000,
            Normal = 0x80,
            NotContentIndexed = 0x2000,
            Offline = 0x1000,
            OpenNoRecall = 0x100000,
            OpenReparsePoint = 0x200000,
            Overlapped = 0x40000000,
            PosixSemantics = 0x1000000,
            RandomAccess = 0x10000000,
            Readonly = 1,
            ReparsePoint = 0x400,
            SequentialScan = 0x8000000,
            SparseFile = 0x200,
            System = 4,
            Temporary = 0x100,
            Write_Through = 0x80000000
        }
    }
    internal sealed class HID
    {
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_FlushQueue(int HidDeviceObject);
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetAttributes(int HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetFeature(int HidDeviceObject, [In, Out] byte[] lpReportBuffer, uint ReportBufferLength);
        [DllImport("hid.dll", SetLastError = true)]
        public static extern void HidD_GetHidGuid(out Guid HidGuid);
        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool HidD_GetIndexedString(int HidDeviceObject, uint StringIndex, [MarshalAs(UnmanagedType.LPTStr)] string Buffer, uint BufferLength);
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetInputReport(int HidDeviceObject, [In, Out] byte[] lpReportBuffer, uint ReportBufferLength);
        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool HidD_GetManufacturerString(int HidDeviceObject, [MarshalAs(UnmanagedType.LPTStr)] string Buffer, uint BufferLength);
        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool HidD_GetManufacturerString(IntPtr HidDeviceObject, [MarshalAs(UnmanagedType.LPTStr)] string Buffer, uint BufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetNumInputBuffers(int HidDeviceObject, ref uint NumberBuffers);
        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool HidD_GetPhysicalDescriptor(int HidDeviceObject, [MarshalAs(UnmanagedType.LPTStr)] string Buffer, uint BufferLength);
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint FILE_SHARE_WRITE = 0x2;
        public const uint FILE_SHARE_READ = 0x1;
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        public const uint OPEN_EXISTING = 3;
        public IntPtr Handle = IntPtr.Zero;
        public string Path = "";
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPStr)] string strName, uint nAccess, uint nShareMode, IntPtr lpSecurity, uint nCreationFlags, uint nAttributes, IntPtr lpTemplate);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int CloseHandle(IntPtr hFile);
        /// <summary>
        /// 根据流获取准备好的数据
        /// </summary>
        /// <param name="HidDeviceObject">加密设备指针转换的流</param>
        /// <param name="PreparsedData"></param>
        /// <returns></returns>
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetPreparsedData(int HidDeviceObject, ref IntPtr PreparsedData);
        [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool HidD_GetProductString(int HidDeviceObject, [MarshalAs(UnmanagedType.LPTStr)] string Buffer, uint BufferLength);
        [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool HidD_GetSerialNumberString(int HidDeviceObject, [MarshalAs(UnmanagedType.LPTStr)] string Buffer, uint BufferLength);
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_SetFeature(int HidDeviceObject, [In] byte[] lpReportBuffer, uint ReportBufferLength);
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_SetNumInputBuffers(int HidDeviceObject, uint NumberBuffers);
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_SetOutputReport(int HidDeviceObject, [In] byte[] lpReportBuffer, uint ReportBufferLength);
        [DllImport("hid.dll", SetLastError = true)]
        public static extern NTSTATUS HidP_GetButtonCaps(HIDP_REPORT_TYPE ReportType, [Out, MarshalAs(UnmanagedType.LPArray)] HIDP_BUTTON_CAPS[] ButtonCaps, ref uint ButtonCapsLength, IntPtr PreparsedData);
        [DllImport("hid.dll", SetLastError = true)]
        public static extern NTSTATUS HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);
        [DllImport("hid.dll", SetLastError = true)]
        public static extern NTSTATUS HidP_GetValueCaps(HIDP_REPORT_TYPE ReportType, [Out, MarshalAs(UnmanagedType.LPArray)] HIDP_VALUE_CAPS[] ValueCaps, ref uint ValueCapsLength, IntPtr PreparsedData);
        [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern NTSTATUS HidP_InitializeReportForID(HIDP_REPORT_TYPE ReportType, byte ReportID, IntPtr PreparsedData, [In, Out] byte[] Report, uint ReportLength);

        [StructLayout(LayoutKind.Sequential)]
        internal struct HIDD_ATTRIBUTES
        {
            public uint Size;
            public ushort VendorID;
            public ushort ProductID;
            public ushort VersionNumber;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct HIDP_BUTTON_CAPS
        {
            [FieldOffset(7)]
            public ushort BitField;
            [FieldOffset(0x1b)]
            public bool IsAbsolute;
            [FieldOffset(3)]
            public bool IsAlias;
            [FieldOffset(0x17)]
            public bool IsDesignatorRange;
            [FieldOffset(15)]
            public bool IsRange;
            [FieldOffset(0x13)]
            public bool IsStringRange;
            [FieldOffset(9)]
            public ushort LinkCollection;
            [FieldOffset(11)]
            public short LinkUsage;
            [FieldOffset(13)]
            public short LinkUsagePage;
            [FieldOffset(0x48)]
            public HID.NotRangeStruct NotRange;
            [FieldOffset(0x48)]
            public HID.RangeStruct Range;
            [FieldOffset(2)]
            public sbyte ReportID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10), FieldOffset(0x20)]
            public uint[] Reserved;
            [FieldOffset(0)]
            public ushort UsagePage;
        }
        /// <summary>
        /// USB装置的详细通讯长度信息
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct HIDP_CAPS
        {
            public ushort Usage;
            public ushort UsagePage;
            /// <summary>
            /// 写入的长度
            /// </summary>
            public ushort InputReportByteLength;
            /// <summary>
            /// 读取的长度
            /// </summary>
            public ushort OutputReportByteLength;
            /// <summary>
            /// 特征 长度
            /// </summary>
            public ushort FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x11)]
            public ushort[] Reserved;
            public ushort NumberLinkCollectionNodes;
            public ushort NumberInputButtonCaps;
            public ushort NumberInputValueCaps;
            public ushort NumberInputDataIndices;
            public ushort NumberOutputButtonCaps;
            public ushort NumberOutputValueCaps;
            public ushort NumberOutputDataIndices;
            public ushort NumberFeatureButtonCaps;
            public ushort NumberFeatureValueCaps;
            public ushort NumberFeatureDataIndices;
        }

        public enum HIDP_REPORT_TYPE : short
        {
            Feature = 2,
            Input = 0,
            Output = 1
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct HIDP_VALUE_CAPS
        {
            [FieldOffset(7)]
            public ushort BitField;
            [FieldOffset(0x24)]
            public ushort BitSize;
            [FieldOffset(0x1f)]
            public bool HasNull;
            [FieldOffset(0x1b)]
            public bool IsAbsolute;
            [FieldOffset(3)]
            public bool IsAlias;
            [FieldOffset(0x17)]
            public bool IsDesignatorRange;
            [FieldOffset(15)]
            public bool IsRange;
            [FieldOffset(0x13)]
            public bool IsStringRange;
            [FieldOffset(9)]
            public ushort LinkCollection;
            [FieldOffset(11)]
            public short LinkUsage;
            [FieldOffset(13)]
            public short LinkUsagePage;
            [FieldOffset(0x3e)]
            public int LogicalMax;
            [FieldOffset(0x3a)]
            public int LogicalMin;
            [FieldOffset(0x4a)]
            public HID.NotRangeStruct NotRange;
            [FieldOffset(70)]
            public int PhysicalMax;
            [FieldOffset(0x42)]
            public int PhysicalMin;
            [FieldOffset(0x4a)]
            public HID.RangeStruct Range;
            [FieldOffset(0x26)]
            public ushort ReportCount;
            [FieldOffset(2)]
            public sbyte ReportID;
            [FieldOffset(0x23)]
            public byte Reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5), FieldOffset(40)]
            public ushort[] Reserved2;
            [FieldOffset(0x36)]
            public int Units;
            [FieldOffset(50)]
            public int UnitsExp;
            [FieldOffset(0)]
            public ushort UsagePage;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NotRangeStruct
        {
            public ushort Usage;
            public ushort Reserved1;
            public ushort StringID;
            public ushort Reserved2;
            public ushort DesignatorID;
            public ushort Reserved3;
            public ushort DataIndex;
            public ushort Reserved4;
        }

        public enum NTSTATUS : uint
        {
            BUFFER_TOO_SMALL = 0,
            DATA_INDEX_NOT_FOUND = 4,
            INVALID_PREPARSED_DATA = 1,
            INVALID_REPORT_LENGTH = 2,
            INVALID_REPORT_TYPE = 3,
            NT_ERROR = 0x18000000,
            NT_INFORMATION = 0x40000000,
            NT_WARNING = 0x10000000,
            REPORT_DOES_NOT_EXIST = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RangeStruct
        {
            public ushort UsageMin;
            public ushort UsageMax;
            public ushort StringMin;
            public ushort StringMax;
            public ushort DesignatorMin;
            public ushort DesignatorMax;
            public ushort DataIndexMin;
            public ushort DataIndexMax;
        }
    }


}
