// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Interop.SetupAPI
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Runtime.InteropServices;

namespace BlueNinjaSoftware.HIDLib.Interop
{
  [StandardModule]
  internal sealed class SetupAPI
  {
    public const int DBT_DEVICEARRIVAL = 32768;
    public const int DBT_DEVICEREMOVECOMPLETE = 32772;
    public const int WM_DEVICECHANGE = 537;
    public const short DIGCF_PRESENT = 2;
    public const short DIGCF_DEVICEINTERFACE = 16;

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern int SetupDiCreateDeviceInfoList(ref Guid ClassGuid, int hwndParent);

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern int SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetupDiEnumDeviceInterfaces(
      IntPtr DeviceInfoSet,
      ref SetupAPI.SP_DEVINFO_DATA DeviceInfoData,
      ref Guid InterfaceClassGuid,
      uint MemberIndex,
      ref SetupAPI.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetupDiEnumDeviceInterfaces(
      IntPtr DeviceInfoSet,
      IntPtr DeviceInfoData,
      ref Guid InterfaceClassGuid,
      uint MemberIndex,
      ref SetupAPI.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr SetupDiGetClassDevs(
      ref Guid ClassGuid,
      [MarshalAs(UnmanagedType.LPTStr)] string Enumerator,
      int hwndParent,
      SetupAPI.DiGetClassFlagsEnum Flags);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetupDiGetDeviceInterfaceDetail(
      IntPtr DeviceInfoSet,
      ref SetupAPI.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
      ref SetupAPI.SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData,
      int DeviceInterfaceDetailDataSize,
      out int RequiredSize,
      ref SetupAPI.SP_DEVINFO_DATA DeviceInfoData);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetupDiGetDeviceInterfaceDetail(
      IntPtr DeviceInfoSet,
      ref SetupAPI.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
      IntPtr DeviceInterfaceDetailData,
      int DeviceInterfaceDetailDataSize,
      out int RequiredSize,
      IntPtr DeviceInfoData);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetupDiGetDeviceInterfaceDetail(
      IntPtr DeviceInfoSet,
      ref SetupAPI.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
      ref SetupAPI.SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData,
      int DeviceInterfaceDetailDataSize,
      out int RequiredSize,
      IntPtr DeviceInfoData);

    [Flags]
    public enum SPIntFlagsEnum
    {
      ACTIVE = 1,
      DEFAULT = 2,
      REMOVED = 4,
    }

    [Flags]
    public enum DiGetClassFlagsEnum
    {
      DEFAULT = 1,
      PRESENT = 2,
      ALLCLASSES = 4,
      PROFILE = 8,
      DEVICEINTERFACE = 16, // 0x00000010
    }

    public struct SP_DEVICE_INTERFACE_DATA
    {
      public int cbSize;
      public Guid InterfaceClassGuid;
      public SetupAPI.SPIntFlagsEnum Flags;
      public IntPtr Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct SP_DEVICE_INTERFACE_DETAIL_DATA
    {
      public uint cbSize;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public string DevicePath;
    }

    public struct SP_DEVINFO_DATA
    {
      public int cbSize;
      public Guid ClassGuid;
      public int DevInst;
      public IntPtr Reserved;
    }
  }
}
