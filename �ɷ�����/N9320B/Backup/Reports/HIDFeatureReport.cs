// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Reports.HIDFeatureReport
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

namespace BlueNinjaSoftware.HIDLib.Reports
{
  public class HIDFeatureReport : AHIDReport
  {
    protected internal HIDFeatureReport(ushort ReportLength)
      : base(ReportLength)
    {
    }

    protected internal HIDFeatureReport(ushort ReportLength, byte ReportID)
      : base(ReportLength, ReportID)
    {
    }

    protected internal HIDFeatureReport(ushort ReportLength, byte ReportID, byte[] ReportData)
      : base(ReportLength, ReportID, ReportData)
    {
    }

    protected internal HIDFeatureReport(byte[] ReportBuffer)
      : base(ReportBuffer)
    {
    }
  }
}
