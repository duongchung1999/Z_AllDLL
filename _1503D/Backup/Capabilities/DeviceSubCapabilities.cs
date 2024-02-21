// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Capabilities.DeviceSubCapabilities
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Interop;

namespace BlueNinjaSoftware.HIDLib.Capabilities
{
  public class DeviceSubCapabilities
  {
    internal DeviceCapabilities _Parent;
    private ushort _RepLength;
    private ushort _NumDataIndices;
    private ButtonCapabilities _ButtonCaps;
    private ValueCapabilities _ValueCaps;

    internal DeviceSubCapabilities(
      ref DeviceCapabilities Parent,
      ushort RepLength,
      ushort NumDataIndices,
      HID.HIDP_BUTTON_CAPS[] ButtonCaps,
      HID.HIDP_VALUE_CAPS[] ValueCaps)
    {
      this._Parent = Parent;
      this._RepLength = RepLength;
      this._NumDataIndices = NumDataIndices;
      DeviceSubCapabilities Parent1 = this;
      this._ButtonCaps = new ButtonCapabilities(ref Parent1, ButtonCaps);
      DeviceSubCapabilities Parent2 = this;
      this._ValueCaps = new ValueCapabilities(ref Parent2, ValueCaps);
    }

    public ushort ReportByteLength => this._RepLength;

    public ushort NumberDataIndices => this._NumDataIndices;

    public ValueCapabilities ValueCapabilities => this._ValueCaps;

    public ButtonCapabilities ButtonCapabilities => this._ButtonCaps;
  }
}
