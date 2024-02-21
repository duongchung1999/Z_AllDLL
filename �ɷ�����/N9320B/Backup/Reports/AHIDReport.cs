// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Reports.AHIDReport
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using Microsoft.VisualBasic;
using System;

namespace BlueNinjaSoftware.HIDLib.Reports
{
  public abstract class AHIDReport : IEquatable<AHIDReport>
  {
    private ushort _ReportLength;
    private byte[] _Buffer;

    protected AHIDReport(ushort ReportLength)
    {
      this._ReportLength = ReportLength;
      this._Buffer = new byte[checked ((int) ReportLength - 1 + 1)];
    }

    protected AHIDReport(ushort ReportLength, byte ReportID)
    {
      this._ReportLength = ReportLength;
      this._Buffer = new byte[checked ((int) ReportLength - 1 + 1)];
      this._Buffer[0] = ReportID;
    }

    protected AHIDReport(ushort ReportLength, byte ReportID, byte[] ReportData)
    {
      this._ReportLength = ReportLength;
      this._Buffer = new byte[checked ((int) ReportLength - 1 + 1)];
      this._Buffer[0] = ReportID;
      if (ReportData.Length != checked ((int) this._ReportLength - 1))
        throw new ArgumentException("Report data size too large.");
      Array.Copy((Array) ReportData, 0, (Array) this._Buffer, 1, ReportData.Length);
    }

    protected AHIDReport(byte[] ReportBuffer)
    {
      this._ReportLength = checked ((ushort) ReportBuffer.Length);
      this._Buffer = (byte[]) ReportBuffer.Clone();
    }

    public virtual byte ReportID
    {
      get => this._Buffer[0];
      set => this._Buffer[0] = value;
    }

    public ushort ReportLength => this._ReportLength;

    public virtual byte[] ReportData
    {
      get
      {
        byte[] numArray = new byte[checked ((int) this.ReportLength - 2 + 1)];
        Array.Copy((Array) this._Buffer, 1, (Array) numArray, 0, checked ((int) this.ReportLength - 2));
        return numArray;
      }
      set
      {
        if (value.Length > checked ((int) this._ReportLength - 1))
          throw new InvalidOperationException("Report data size too large.");
        Array.Clear((Array) this._Buffer, 0, this._Buffer.Length);
        Array.Copy((Array) value, 0, (Array) this._Buffer, 1, value.Length);
      }
    }

    public virtual byte get_ReportData(ushort Index) => (int) Index <= checked ((int) this._ReportLength - 2) ? this._Buffer[checked ((int) Index + 1)] : throw new InvalidOperationException("Index falls outside of valid range.");

    public virtual void set_ReportData(ushort Index, byte Value)
    {
      if ((int) Index > checked ((int) this._ReportLength - 2))
        throw new InvalidOperationException("Index falls outside of valid range.");
      this._Buffer[checked ((int) Index + 1)] = Value;
    }

    public byte[] GetBuffer() => this._Buffer;

    public override string ToString() => string.Format("Report ID: {0}, Data: {1}", (object) Utility.PadHex(Conversion.Hex(this.ReportID), 2), (object) BitConverter.ToString(this.ReportData));

    public bool Equals(AHIDReport other)
    {
      if (other == null || this._Buffer.Length != other._Buffer.Length)
        return false;
      int num = checked (this._Buffer.Length - 1);
      int index = 0;
      while (index <= num)
      {
        if ((int) this._Buffer[index] != (int) other._Buffer[index])
          return false;
        checked { ++index; }
      }
      return true;
    }

    public override bool Equals(object obj) => obj is AHIDReport && this.Equals((AHIDReport) obj);
  }
}
