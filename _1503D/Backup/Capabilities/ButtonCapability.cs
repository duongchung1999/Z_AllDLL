// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Capabilities.ButtonCapability
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Interop;

namespace BlueNinjaSoftware.HIDLib.Capabilities
{
  public class ButtonCapability : ACapability
  {
    private HID.HIDP_BUTTON_CAPS _ButtonCaps;

    internal ButtonCapability(ref ButtonCapabilities Parent, HID.HIDP_BUTTON_CAPS ButtonCaps)
    {
      ACapabilities Parent1 = (ACapabilities) Parent;
      // ISSUE: explicit constructor call
      base.\u002Ector(ref Parent1, ButtonCaps.Range, ButtonCaps.NotRange);
      Parent = (ButtonCapabilities) Parent1;
      this._ButtonCaps = ButtonCaps;
    }

    public ushort UsagePage => this._ButtonCaps.UsagePage;

    public sbyte ReportID => this._ButtonCaps.ReportID;

    public bool IsAlias => this._ButtonCaps.IsAlias;

    public ushort BitField => this._ButtonCaps.BitField;

    public ushort LinkCollection => this._ButtonCaps.LinkCollection;

    public short LinkUsage => this._ButtonCaps.LinkUsage;

    public short LinkUsagePage => this._ButtonCaps.LinkUsagePage;

    public bool IsRange => this._ButtonCaps.IsRange;

    public bool IsStringRange => this._ButtonCaps.IsStringRange;

    public bool IsDesignatorRange => this._ButtonCaps.IsDesignatorRange;

    public bool IsAbsolute => this._ButtonCaps.IsAbsolute;
  }
}
