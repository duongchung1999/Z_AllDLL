// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.HIDDevice
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Capabilities;
using BlueNinjaSoftware.HIDLib.Interop;
using BlueNinjaSoftware.HIDLib.Reports;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace BlueNinjaSoftware.HIDLib
{
  public class HIDDevice : IEquatable<HIDDevice>, IDisposable
  {
    private FileStream _Stream;
    private string _DevicePath;
    private HID.HIDD_ATTRIBUTES _Attributes;
    private IntPtr _PreparsedData;
    private DeviceCapabilities _Capabilities;
    private string _Manufacturer;
    private string _Product;
    private string _Serial;
    private string _Descriptor;

    public event HIDDataReceivedEventHandler DataReceived;

    public event HIDReportReceivedEventHandler ReportReceived;

    public event EventHandler Disconnected;

    public event EventHandler Reconnected;

    public HIDDevice(string DevicePath)
    {
      this._Manufacturer = "";
      this._Product = "";
      this._Serial = "";
      this._Descriptor = "";
      try
      {
        SafeFileHandle handle = new SafeFileHandle(User32.CreateFile(DevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, User32.EFileAttributes.Overlapped, IntPtr.Zero), true);
        this._Stream = !handle.IsInvalid ? new FileStream(handle, FileAccess.ReadWrite, 8, true) : throw new ApplicationException("Cannot open specified device: " + this.GetLastWin32Error().Message);
        this._Attributes.Size = checked ((uint) Marshal.SizeOf((object) this._Attributes));
        if (!HID.HidD_GetAttributes((int) this._Stream.SafeFileHandle.DangerousGetHandle(), ref this._Attributes))
          throw new ApplicationException(string.Format("Could not get attributes for device '{0}': {1}", (object) DevicePath, (object) this.GetLastWin32Error().Message));
        this._DevicePath = DevicePath;
        this.DeviceSetup();
      }
      catch (ApplicationException ex)
      {
        ProjectData.SetProjectError((Exception) ex);
        throw;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        Exception exception = ex;
        throw new ApplicationException(string.Format("Could not access device '{0}': {1}", (object) DevicePath, (object) exception.Message));
      }
    }

    internal HIDDevice(ref FileStream Stream, string DevicePAth, HID.HIDD_ATTRIBUTES Attributes)
    {
      this._Manufacturer = "";
      this._Product = "";
      this._Serial = "";
      this._Descriptor = "";
      this._Stream = Stream;
      this._Attributes = Attributes;
      this._DevicePath = DevicePAth;
      this.DeviceSetup();
    }

    public ushort VendorID => this._Attributes.VendorID;

    public ushort ProductID => this._Attributes.ProductID;

    public string Manufacturer => this._Manufacturer;

    public string ProductName => this._Product;

    public string SerialNumber => this._Serial;

    public string Descriptor => this._Descriptor;

    public ushort VersionNumber => this._Attributes.VersionNumber;

    public string DevicePath => this._DevicePath;

    public bool Attached => this._Stream != null;

    public SafeFileHandle SafeHandle => this._Stream.SafeFileHandle;

    public DeviceCapabilities Capabilities => this._Capabilities;

    public HIDOutputReport CreateOutputReport() => new HIDOutputReport(this._Capabilities.OutputCapabilities.ReportByteLength);

    public HIDOutputReport CreateOutputReport(byte ReportID) => new HIDOutputReport(this._Capabilities.OutputCapabilities.ReportByteLength, ReportID);

    public HIDOutputReport CreateOutputReport(byte ReportID, byte[] ReportData) => new HIDOutputReport(this._Capabilities.OutputCapabilities.ReportByteLength, ReportID, ReportData);

    public HIDOutputReport CreateOutputReport(byte[] ReportBuffer)
    {
      if (ReportBuffer.Length == (int) this._Capabilities.OutputCapabilities.ReportByteLength)
        return new HIDOutputReport(ReportBuffer);
      throw new ArgumentException(string.Format("Report length incorrect - was {0}, but must be {1}.", (object) ReportBuffer.Length, (object) this._Capabilities.OutputCapabilities.ReportByteLength));
    }

    public HIDOutputReport InitializeOutputReport(byte ReportID)
    {
      HIDOutputReport hidOutputReport = new HIDOutputReport(this._Capabilities.OutputCapabilities.ReportByteLength, ReportID);
      HID.NTSTATUS ntstatus = HID.HidP_InitializeReportForID(HID.HIDP_REPORT_TYPE.Output, ReportID, this._PreparsedData, hidOutputReport.GetBuffer(), (uint) hidOutputReport.ReportLength);
      if (ntstatus < HID.NTSTATUS.NT_WARNING)
        return hidOutputReport;
      throw new ApplicationException("Could not initialize output report: " + ntstatus.ToString());
    }

    public HIDFeatureReport CreateFeatureReport() => new HIDFeatureReport(this._Capabilities.FeatureCapabilities.ReportByteLength);

    public HIDFeatureReport CreateFeatureReport(byte ReportID) => new HIDFeatureReport(this._Capabilities.FeatureCapabilities.ReportByteLength, ReportID);

    public HIDFeatureReport CreateFeatureReport(byte ReportID, byte[] ReportData) => new HIDFeatureReport(this._Capabilities.FeatureCapabilities.ReportByteLength, ReportID, ReportData);

    public HIDFeatureReport CreateFeatureReport(byte[] ReportBuffer)
    {
      if (ReportBuffer.Length == (int) this._Capabilities.FeatureCapabilities.ReportByteLength)
        return new HIDFeatureReport(ReportBuffer);
      throw new ArgumentException(string.Format("Report length incorrect - was {0}, but must be {1}.", (object) ReportBuffer.Length, (object) this._Capabilities.FeatureCapabilities.ReportByteLength));
    }

    public HIDFeatureReport InitializeFeatureReport(byte ReportID)
    {
      HIDFeatureReport hidFeatureReport = new HIDFeatureReport(this._Capabilities.FeatureCapabilities.ReportByteLength, ReportID);
      HID.NTSTATUS ntstatus = HID.HidP_InitializeReportForID(HID.HIDP_REPORT_TYPE.Feature, ReportID, this._PreparsedData, hidFeatureReport.GetBuffer(), (uint) hidFeatureReport.ReportLength);
      if (ntstatus < HID.NTSTATUS.NT_WARNING)
        return hidFeatureReport;
      throw new ApplicationException("Could not initialize feature report: " + ntstatus.ToString());
    }

    public HIDInputReport ReadInputReport()
    {
      HIDInputReport InputReport = new HIDInputReport(this._Capabilities.InputCapabilities.ReportByteLength);
      HIDInputReport hidInputReport;
      return this.DoReadInputReport(ref InputReport) ? InputReport : hidInputReport;
    }

    public HIDInputReport ReadInputReport(byte ReportID)
    {
      HIDInputReport InputReport = new HIDInputReport(this._Capabilities.InputCapabilities.ReportByteLength, ReportID);
      HIDInputReport hidInputReport;
      return this.DoReadInputReport(ref InputReport) ? InputReport : hidInputReport;
    }

    public HIDFeatureReport ReadFeatureReport()
    {
      HIDFeatureReport FeatureReport = new HIDFeatureReport(this._Capabilities.FeatureCapabilities.ReportByteLength);
      HIDFeatureReport hidFeatureReport;
      return this.DoReadFeatureReport(ref FeatureReport) ? FeatureReport : hidFeatureReport;
    }

    public HIDFeatureReport ReadFeatureReport(byte ReportID)
    {
      HIDFeatureReport FeatureReport = new HIDFeatureReport(this._Capabilities.FeatureCapabilities.ReportByteLength, ReportID);
      HIDFeatureReport hidFeatureReport;
      return this.DoReadFeatureReport(ref FeatureReport) ? FeatureReport : hidFeatureReport;
    }

    public bool WriteReport(AHIDReport Report)
    {
      switch (Report)
      {
        case HIDOutputReport _:
          if ((int) Report.ReportLength == (int) this._Capabilities.OutputCapabilities.ReportByteLength)
            return HID.HidD_SetOutputReport((int) this._Stream.SafeFileHandle.DangerousGetHandle(), Report.GetBuffer(), (uint) Report.ReportLength);
          throw new ArgumentException(string.Format("Report length incorrect - was {0}, but must be {1}.", (object) Report.ReportLength, (object) this._Capabilities.OutputCapabilities.ReportByteLength));
        case HIDFeatureReport _:
          if ((int) Report.ReportLength == (int) this._Capabilities.FeatureCapabilities.ReportByteLength)
            return HID.HidD_SetFeature((int) this._Stream.SafeFileHandle.DangerousGetHandle(), Report.GetBuffer(), (uint) Report.ReportLength);
          throw new ArgumentException(string.Format("Report length incorrect - was {0}, but must be {1}.", (object) Report.ReportLength, (object) this._Capabilities.FeatureCapabilities.ReportByteLength));
        default:
          throw new ArgumentException("Invalid report type - must be HIDOutputReport or HIDFeatureReport.");
      }
    }

    public bool BulkRead(ref byte[] Data) => throw new NotImplementedException();

    public bool BulkRead(ref string Data) => throw new NotImplementedException();

    public bool BulkWrite(byte[] Data) => throw new NotImplementedException();

    public bool BulkWrite(string Data) => throw new NotImplementedException();

    public Win32Exception GetLastWin32Error() => new Win32Exception(Marshal.GetLastWin32Error());

    public override string ToString() => string.Format("VID: {0}, PID: {1}, {2} {3} rev. {4}, SN: {5}", (object) Utility.PadHex(Conversion.Hex(this._Attributes.VendorID), 4), (object) Utility.PadHex(Conversion.Hex(this._Attributes.ProductID), 4), (object) this._Manufacturer, (object) this._Product, (object) Utility.PadHex(Conversion.Hex(this._Attributes.VersionNumber), 4), (object) this._Serial);

    private void DeviceSetup()
    {
      this.GetCaps();
      this.GetParsedStringData();
    }

    private void GetCaps()
    {
      if (!HID.HidD_GetPreparsedData((int) this._Stream.SafeFileHandle.DangerousGetHandle(), ref this._PreparsedData))
        throw new ApplicationException(string.Format("Could not get preparsed data for device '{0}': {1}", (object) this._Stream.Name, (object) new Win32Exception(Marshal.GetLastWin32Error()).Message));
      try
      {
        HIDDevice Parent = this;
        this._Capabilities = new DeviceCapabilities(ref Parent, this._PreparsedData);
      }
      catch (ApplicationException ex)
      {
        ProjectData.SetProjectError((Exception) ex);
        throw new ApplicationException(string.Format("Could not get capabilities data for device '{0}': {1}", (object) this._Stream.Name, (object) ex.ToString()));
      }
    }

    private void GetParsedStringData()
    {
      string str1 = Strings.StrDup((int) sbyte.MaxValue, char.MinValue);
      if (HID.HidD_GetManufacturerString((int) this._Stream.SafeFileHandle.DangerousGetHandle(), str1, checked ((uint) (str1.Length * 2))))
        this._Manufacturer = Utility.ParseString(str1);
      string str2 = Strings.StrDup((int) sbyte.MaxValue, char.MinValue);
      if (HID.HidD_GetProductString((int) this._Stream.SafeFileHandle.DangerousGetHandle(), str2, checked ((uint) (str2.Length * 2))))
        this._Product = Utility.ParseString(str2);
      string str3 = Strings.StrDup((int) sbyte.MaxValue, char.MinValue);
      if (HID.HidD_GetSerialNumberString((int) this._Stream.SafeFileHandle.DangerousGetHandle(), str3, checked ((uint) (str3.Length * 2))))
        this._Serial = Utility.ParseString(str3);
      string str4 = Strings.StrDup((int) sbyte.MaxValue, char.MinValue);
      if (!HID.HidD_GetPhysicalDescriptor((int) this._Stream.SafeFileHandle.DangerousGetHandle(), str4, checked ((uint) (str4.Length * 2))))
        return;
      this._Descriptor = Utility.ParseString(str4);
    }

    private bool DoReadInputReport(ref HIDInputReport InputReport) => HID.HidD_GetInputReport((int) this._Stream.SafeFileHandle.DangerousGetHandle(), InputReport.GetBuffer(), (uint) InputReport.ReportLength);

    private bool DoReadFeatureReport(ref HIDFeatureReport FeatureReport) => HID.HidD_GetFeature((int) this._Stream.SafeFileHandle.DangerousGetHandle(), FeatureReport.GetBuffer(), (uint) FeatureReport.ReportLength);

    private void BeginAsyncRead()
    {
      if (!(this._Stream.CanRead & this._Stream.IsAsync))
        return;
      byte[] array = new byte[checked ((int) this._Capabilities.InputCapabilities.ReportByteLength - 1 + 1)];
      this._Stream.BeginRead(array, 0, array.Length, new AsyncCallback(this.OnReportReceived), (object) array);
    }

    private void OnReportReceived(IAsyncResult ar)
    {
      HIDReportReceivedEventArgs args = new HIDReportReceivedEventArgs((AHIDReport) new HIDInputReport((byte[]) ar));
      this._Stream.EndRead(ar);
      this.BeginAsyncRead();
      HIDReportReceivedEventHandler reportReceivedEvent = this.ReportReceivedEvent;
      if (reportReceivedEvent == null)
        return;
      reportReceivedEvent((object) this, args);
    }

    public bool Equals(HIDDevice other) => Operators.CompareString(other.DevicePath, this._Stream.Name, false) == 0;

    public override bool Equals(object obj) => obj is HIDDevice && this.Equals((HIDDevice) obj);

    ~HIDDevice()
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
      try
      {
        this._Capabilities._Parent = (HIDDevice) null;
        this._Capabilities = (DeviceCapabilities) null;
        if (this._PreparsedData != IntPtr.Zero)
          HID.HidD_FreePreparsedData(ref this._PreparsedData);
        this._Stream.Close();
        this._Stream.Dispose();
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        int num = (int) Interaction.MsgBox((object) ("HIDDevice.Free error: " + ex.ToString()));
        ProjectData.ClearProjectError();
      }
    }
  }
}
