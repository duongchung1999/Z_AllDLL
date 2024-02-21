// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Capabilities.ButtonCapabilities
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Interop;
using System.Collections.Generic;

namespace BlueNinjaSoftware.HIDLib.Capabilities
{
  public class ButtonCapabilities : ACapabilities
  {
    internal ButtonCapabilities(ref DeviceSubCapabilities Parent, HID.HIDP_BUTTON_CAPS[] ButtonCaps)
      : base(ref Parent)
    {
      if (ButtonCaps == null)
        return;
      int num = checked (ButtonCaps.Length - 1);
      int index = 0;
      while (index <= num)
      {
        List<ACapability> capsList = this._CapsList;
        ButtonCapabilities Parent1 = this;
        ButtonCapability buttonCapability = new ButtonCapability(ref Parent1, ButtonCaps[index]);
        capsList.Add((ACapability) buttonCapability);
        checked { ++index; }
      }
    }

    public ButtonCapability this[ushort Index] => (ButtonCapability) base[Index];
  }
}
