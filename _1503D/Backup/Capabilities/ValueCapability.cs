// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Capabilities.ValueCapability
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Interop;

namespace BlueNinjaSoftware.HIDLib.Capabilities
{
  public class ValueCapability : ACapability
  {
    private HID.HIDP_VALUE_CAPS _ValueCaps;
    public bool HasNull;
    public ushort BitSize;
    public ushort ReportCount;
    public int UnitsExp;
    public int Units;
    public int LogicalMin;
    public int LogicalMax;
    public int PhysicalMin;
    public int PhysicalMax;

    internal ValueCapability(ref ValueCapabilities Parent, HID.HIDP_VALUE_CAPS ValueCaps)
    {
      ACapabilities Parent1 = (ACapabilities) Parent;
      // ISSUE: explicit constructor call
      base.\u002Ector(ref Parent1, ValueCaps.Range, ValueCaps.NotRange);
      Parent = (ValueCapabilities) Parent1;
      this._ValueCaps = ValueCaps;
    }

    public ushort UsagePage => this._ValueCaps.UsagePage;

    public sbyte ReportID => this._ValueCaps.ReportID;

    public bool IsAlias => this._ValueCaps.IsAlias;

    public ushort BitField => this._ValueCaps.BitField;

    public ushort LinkCollection => this._ValueCaps.LinkCollection;

    public short LinkUsage => this._ValueCaps.LinkUsage;

    public short LinkUsagePage => this._ValueCaps.LinkUsagePage;

    public bool IsRange => this._ValueCaps.IsRange;

    public bool IsStringRange => this._ValueCaps.IsStringRange;

    public bool IsDesignatorRange => this._ValueCaps.IsDesignatorRange;

    public bool IsAbsolute => this._ValueCaps.IsAbsolute;
  }
}
