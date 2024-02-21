// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.HIDManagement
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Interop;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace BlueNinjaSoftware.HIDLib
{
  public class HIDManagement : IDisposable
  {
    public static List<HIDDevice> GetDevices() => (List<HIDDevice>) HIDManagement.GetDevices((ushort) 0, (ushort) 0, true);

    public static IList<HIDDevice> GetDevices(bool PresentOnly) => HIDManagement.GetDevices((ushort) 0, (ushort) 0, PresentOnly);

    public static IList<HIDDevice> GetDevices(ushort VendorID) => HIDManagement.GetDevices(VendorID, (ushort) 0, true);

    public static IList<HIDDevice> GetDevices(ushort VendorID, bool PresentOnly) => HIDManagement.GetDevices(VendorID, (ushort) 0, PresentOnly);

    public static IList<HIDDevice> GetDevices(ushort VendorID, ushort ProductID) => HIDManagement.GetDevices(VendorID, ProductID, true);

    public static IList<HIDDevice> GetDevices(
      ushort VendorID,
      ushort ProductID,
      bool PresentOnly)
    {
      List<HIDDevice> hidDeviceList1 = new List<HIDDevice>();
      IntPtr classDevs;
      try
      {
        Guid HidGuid = new Guid();
        uint MemberIndex = 0;
        HID.HidD_GetHidGuid(out HidGuid);
        classDevs = SetupAPI.SetupDiGetClassDevs(ref HidGuid, (string) null, (int) IntPtr.Zero, (SetupAPI.DiGetClassFlagsEnum) Conversions.ToInteger(Operators.OrObject((object) SetupAPI.DiGetClassFlagsEnum.DEVICEINTERFACE, Interaction.IIf(PresentOnly, (object) SetupAPI.DiGetClassFlagsEnum.PRESENT, (object) 0))));
        SetupAPI.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData;
        DeviceInterfaceData.cbSize = Marshal.SizeOf((object) DeviceInterfaceData);
        for (; SetupAPI.SetupDiEnumDeviceInterfaces(classDevs, IntPtr.Zero, ref HidGuid, MemberIndex, ref DeviceInterfaceData); MemberIndex = checked ((uint) ((long) MemberIndex + 1L)))
        {
          uint num1 = 0;
          IntPtr DeviceInfoSet1 = classDevs;
          ref SetupAPI.SP_DEVICE_INTERFACE_DATA local1 = ref DeviceInterfaceData;
          IntPtr zero1 = IntPtr.Zero;
          int num2 = checked ((int) num1);
          ref int local2 = ref num2;
          IntPtr zero2 = IntPtr.Zero;
          SetupAPI.SetupDiGetDeviceInterfaceDetail(DeviceInfoSet1, ref local1, zero1, 0, out local2, zero2);
          SetupAPI.SP_DEVICE_INTERFACE_DETAIL_DATA interfaceDetailData = new SetupAPI.SP_DEVICE_INTERFACE_DETAIL_DATA();
          interfaceDetailData.cbSize = checked ((uint) (4 + Marshal.SystemDefaultCharSize));
          IntPtr DeviceInfoSet2 = classDevs;
          ref SetupAPI.SP_DEVICE_INTERFACE_DATA local3 = ref DeviceInterfaceData;
          ref SetupAPI.SP_DEVICE_INTERFACE_DETAIL_DATA local4 = ref interfaceDetailData;
          int DeviceInterfaceDetailDataSize = Marshal.SizeOf((object) interfaceDetailData);
          num2 = 0;
          ref int local5 = ref num2;
          IntPtr zero3 = IntPtr.Zero;
          if (!SetupAPI.SetupDiGetDeviceInterfaceDetail(DeviceInfoSet2, ref local3, ref local4, DeviceInterfaceDetailDataSize, out local5, zero3))
            throw new ApplicationException(string.Format("SetupDiGetDeviceInterfaceDetail failed on index {0}: {1}", (object) MemberIndex, (object) new Win32Exception(Marshal.GetLastWin32Error()).ToString()));
          IntPtr securityAttributes;
          SafeFileHandle handle = new SafeFileHandle(User32.CreateFile(interfaceDetailData.DevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, securityAttributes, FileMode.Open, User32.EFileAttributes.Overlapped, IntPtr.Zero), true);
          if (!handle.IsInvalid)
          {
            HID.HIDD_ATTRIBUTES Attributes;
            Attributes.Size = checked ((uint) Marshal.SizeOf((object) Attributes));
            if (HID.HidD_GetAttributes((int) handle.DangerousGetHandle(), ref Attributes))
            {
              if ((int) Attributes.VendorID == (int) VendorID | VendorID == (ushort) 0 && (int) Attributes.ProductID == (int) ProductID | ProductID == (ushort) 0)
              {
                List<HIDDevice> hidDeviceList2 = hidDeviceList1;
                FileStream Stream = new FileStream(handle, FileAccess.ReadWrite, 8, true);
                HIDDevice hidDevice = new HIDDevice(ref Stream, interfaceDetailData.DevicePath, Attributes);
                hidDeviceList2.Add(hidDevice);
              }
              else
                handle.Dispose();
            }
            else
              handle.Dispose();
          }
        }
        return (IList<HIDDevice>) hidDeviceList1.AsReadOnly();
      }
      catch (ApplicationException ex)
      {
        ProjectData.SetProjectError((Exception) ex);
        int num = (int) Interaction.MsgBox((object) ex.ToString());
        throw;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        Exception exception = ex;
        int num = (int) Interaction.MsgBox((object) exception.ToString());
        throw new ApplicationException(string.Format("Could not get device {0}:{1}: {2}", (object) Utility.PadHex(Conversion.Hex(VendorID), 4), (object) Utility.PadHex(Conversion.Hex(ProductID), 4), (object) exception.ToString()));
      }
      finally
      {
        SetupAPI.SetupDiDestroyDeviceInfoList(classDevs);
      }
    }

    ~HIDManagement()
    {
      this.FreeUnmanagedResources();
      // ISSUE: explicit finalizer call
      base.Finalize();
    }

    public void Dispose()
    {
      this.FreeUnmanagedResources();
      GC.SuppressFinalize((object) this);
    }

    private void FreeUnmanagedResources()
    {
    }
  }
}
