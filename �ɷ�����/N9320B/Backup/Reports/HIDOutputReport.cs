// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Reports.HIDOutputReport
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

namespace BlueNinjaSoftware.HIDLib.Reports
{
  public class HIDOutputReport : AHIDReport
  {
    protected internal HIDOutputReport(ushort ReportLength)
      : base(ReportLength)
    {
    }

    protected internal HIDOutputReport(ushort ReportLength, byte ReportID)
      : base(ReportLength, ReportID)
    {
    }

    protected internal HIDOutputReport(ushort ReportLength, byte ReportID, byte[] ReportData)
      : base(ReportLength, ReportID, ReportData)
    {
    }

    protected internal HIDOutputReport(byte[] ReportBuffer)
      : base(ReportBuffer)
    {
    }
  }
}
