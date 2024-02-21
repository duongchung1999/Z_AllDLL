// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Interop.User32
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using Microsoft.VisualBasic.CompilerServices;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BlueNinjaSoftware.HIDLib.Interop
{
  [StandardModule]
  internal sealed class User32
  {
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr RegisterDeviceNotification(
      IntPtr hRecipient,
      IntPtr NotificationFilter,
      User32.DeviceNotifyEnum Flags);

    [DllImport("user32.dll")]
    public static extern bool UnregisterDeviceNotification(IntPtr Handle);

    [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CreateFile(
      string fileName,
      FileAccess fileAccess,
      FileShare fileShare,
      IntPtr securityAttributes,
      FileMode creationDisposition,
      User32.EFileAttributes flags,
      IntPtr template);

    public enum DeviceNotifyEnum
    {
      WINDOW_HANDLE = 0,
      SERVICE_HANDLE = 1,
      ALL_INTERFACE_CLASSES = 4,
    }

    public enum DeviceTypeEnum
    {
      OEM = 0,
      VOLUME = 2,
      PORT = 3,
      DEVICEINTERFACE = 5,
      HANDLE = 6,
    }

    [Flags]
    public enum EFileAttributes : uint
    {
      Readonly = 1,
      Hidden = 2,
      System = 4,
      Directory = 16, // 0x00000010
      Archive = 32, // 0x00000020
      Device = 64, // 0x00000040
      Normal = 128, // 0x00000080
      Temporary = 256, // 0x00000100
      SparseFile = 512, // 0x00000200
      ReparsePoint = 1024, // 0x00000400
      Compressed = 2048, // 0x00000800
      Offline = 4096, // 0x00001000
      NotContentIndexed = 8192, // 0x00002000
      Encrypted = 16384, // 0x00004000
      Write_Through = 2147483648, // 0x80000000
      Overlapped = 1073741824, // 0x40000000
      NoBuffering = 536870912, // 0x20000000
      RandomAccess = 268435456, // 0x10000000
      SequentialScan = 134217728, // 0x08000000
      DeleteOnClose = 67108864, // 0x04000000
      BackupSemantics = 33554432, // 0x02000000
      PosixSemantics = 16777216, // 0x01000000
      OpenReparsePoint = 2097152, // 0x00200000
      OpenNoRecall = 1048576, // 0x00100000
      FirstPipeInstance = 524288, // 0x00080000
    }

    public struct DEV_BROADCAST_HDR
    {
      public int dbch_size;
      public User32.DeviceTypeEnum dbch_devicetype;
      public int dbch_reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DEV_BROADCAST_DEVICEINTERFACE
    {
      public int dbcc_size;
      public User32.DeviceTypeEnum dbcc_devicetype;
      public int dbcc_reserved;
      public Guid dbcc_classguid;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public string dbcc_name;
    }

    public struct DEV_BROADCAST_HANDLE
    {
      public int dbch_size;
      public User32.DeviceTypeEnum dbch_devicetype;
      public int dbch_reserved;
      public int dbch_handle;
      public int dbch_hdevnotify;
    }
  }
}
