// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Interop.HID
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Runtime.InteropServices;

namespace BlueNinjaSoftware.HIDLib.Interop
{
  [StandardModule]
  internal sealed class HID
  {
    [DllImport("hid.dll", SetLastError = true)]
    public static extern void HidD_GetHidGuid(out Guid HidGuid);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetNumInputBuffers(int HidDeviceObject, ref uint NumberBuffers);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_SetNumInputBuffers(int HidDeviceObject, uint NumberBuffers);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_FlushQueue(int HidDeviceObject);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetPreparsedData(int HidDeviceObject, ref IntPtr PreparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetAttributes(
      int HidDeviceObject,
      ref HID.HIDD_ATTRIBUTES Attributes);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern HID.NTSTATUS HidP_GetCaps(
      IntPtr PreparsedData,
      ref HID.HIDP_CAPS Capabilities);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern HID.NTSTATUS HidP_GetValueCaps(
      HID.HIDP_REPORT_TYPE ReportType,
      [MarshalAs(UnmanagedType.LPArray), Out] HID.HIDP_VALUE_CAPS[] ValueCaps,
      ref uint ValueCapsLength,
      IntPtr PreparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern HID.NTSTATUS HidP_GetButtonCaps(
      HID.HIDP_REPORT_TYPE ReportType,
      [MarshalAs(UnmanagedType.LPArray), Out] HID.HIDP_BUTTON_CAPS[] ButtonCaps,
      ref uint ButtonCapsLength,
      IntPtr PreparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetInputReport(
      int HidDeviceObject,
      [In, Out] byte[] lpReportBuffer,
      uint ReportBufferLength);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_SetOutputReport(
      int HidDeviceObject,
      [In] byte[] lpReportBuffer,
      uint ReportBufferLength);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetFeature(
      int HidDeviceObject,
      [In, Out] byte[] lpReportBuffer,
      uint ReportBufferLength);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_SetFeature(
      int HidDeviceObject,
      [In] byte[] lpReportBuffer,
      uint ReportBufferLength);

    [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern HID.NTSTATUS HidP_InitializeReportForID(
      HID.HIDP_REPORT_TYPE ReportType,
      byte ReportID,
      IntPtr PreparsedData,
      [In, Out] byte[] Report,
      uint ReportLength);

    [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool HidD_GetManufacturerString(
      int HidDeviceObject,
      [MarshalAs(UnmanagedType.LPTStr)] string Buffer,
      uint BufferLength);

    [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool HidD_GetProductString(
      int HidDeviceObject,
      [MarshalAs(UnmanagedType.LPTStr)] string Buffer,
      uint BufferLength);

    [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool HidD_GetSerialNumberString(
      int HidDeviceObject,
      [MarshalAs(UnmanagedType.LPTStr)] string Buffer,
      uint BufferLength);

    [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool HidD_GetPhysicalDescriptor(
      int HidDeviceObject,
      [MarshalAs(UnmanagedType.LPTStr)] string Buffer,
      uint BufferLength);

    [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool HidD_GetIndexedString(
      int HidDeviceObject,
      uint StringIndex,
      [MarshalAs(UnmanagedType.LPTStr)] string Buffer,
      uint BufferLength);

    public enum HIDP_REPORT_TYPE : short
    {
      Input,
      Output,
      Feature,
    }

    public enum NTSTATUS : uint
    {
      BUFFER_TOO_SMALL = 0,
      INVALID_PREPARSED_DATA = 1,
      INVALID_REPORT_LENGTH = 2,
      INVALID_REPORT_TYPE = 3,
      DATA_INDEX_NOT_FOUND = 4,
      REPORT_DOES_NOT_EXIST = 5,
      NT_WARNING = 268435456, // 0x10000000
      NT_ERROR = 402653184, // 0x18000000
      NT_INFORMATION = 1073741824, // 0x40000000
    }

    internal struct HIDD_ATTRIBUTES
    {
      public uint Size;
      public ushort VendorID;
      public ushort ProductID;
      public ushort VersionNumber;
    }

    internal struct HIDP_CAPS
    {
      public ushort Usage;
      public ushort UsagePage;
      public ushort InputReportByteLength;
      public ushort OutputReportByteLength;
      public ushort FeatureReportByteLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
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

    [StructLayout(LayoutKind.Explicit)]
    internal struct HIDP_VALUE_CAPS
    {
      [FieldOffset(0)]
      public ushort UsagePage;
      [FieldOffset(2)]
      public sbyte ReportID;
      [FieldOffset(3)]
      public bool IsAlias;
      [FieldOffset(7)]
      public ushort BitField;
      [FieldOffset(9)]
      public ushort LinkCollection;
      [FieldOffset(11)]
      public short LinkUsage;
      [FieldOffset(13)]
      public short LinkUsagePage;
      [FieldOffset(15)]
      public bool IsRange;
      [FieldOffset(19)]
      public bool IsStringRange;
      [FieldOffset(23)]
      public bool IsDesignatorRange;
      [FieldOffset(27)]
      public bool IsAbsolute;
      [FieldOffset(31)]
      public bool HasNull;
      [FieldOffset(35)]
      public byte Reserved;
      [FieldOffset(36)]
      public ushort BitSize;
      [FieldOffset(38)]
      public ushort ReportCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
      [FieldOffset(40)]
      public ushort[] Reserved2;
      [FieldOffset(50)]
      public int UnitsExp;
      [FieldOffset(54)]
      public int Units;
      [FieldOffset(58)]
      public int LogicalMin;
      [FieldOffset(62)]
      public int LogicalMax;
      [FieldOffset(66)]
      public int PhysicalMin;
      [FieldOffset(70)]
      public int PhysicalMax;
      [FieldOffset(74)]
      public HID.RangeStruct Range;
      [FieldOffset(74)]
      public HID.NotRangeStruct NotRange;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct HIDP_BUTTON_CAPS
    {
      [FieldOffset(0)]
      public ushort UsagePage;
      [FieldOffset(2)]
      public sbyte ReportID;
      [FieldOffset(3)]
      public bool IsAlias;
      [FieldOffset(7)]
      public ushort BitField;
      [FieldOffset(9)]
      public ushort LinkCollection;
      [FieldOffset(11)]
      public short LinkUsage;
      [FieldOffset(13)]
      public short LinkUsagePage;
      [FieldOffset(15)]
      public bool IsRange;
      [FieldOffset(19)]
      public bool IsStringRange;
      [FieldOffset(23)]
      public bool IsDesignatorRange;
      [FieldOffset(27)]
      public bool IsAbsolute;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
      [FieldOffset(32)]
      public uint[] Reserved;
      [FieldOffset(72)]
      public HID.RangeStruct Range;
      [FieldOffset(72)]
      public HID.NotRangeStruct NotRange;
    }

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
  }
}
