// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.HIDReportReceivedEventArgs
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Reports;
using System;

namespace BlueNinjaSoftware.HIDLib
{
  public class HIDReportReceivedEventArgs : EventArgs
  {
    private AHIDReport _Report;

    public HIDReportReceivedEventArgs(AHIDReport Report) => this._Report = Report;

    public AHIDReport Report => this._Report;
  }
}
