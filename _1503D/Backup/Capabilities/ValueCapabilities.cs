// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Capabilities.ValueCapabilities
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Interop;
using System.Collections.Generic;

namespace BlueNinjaSoftware.HIDLib.Capabilities
{
  public class ValueCapabilities : ACapabilities
  {
    internal ValueCapabilities(ref DeviceSubCapabilities Parent, HID.HIDP_VALUE_CAPS[] ValueCaps)
      : base(ref Parent)
    {
      if (ValueCaps == null)
        return;
      int num = checked (ValueCaps.Length - 1);
      int index = 0;
      while (index <= num)
      {
        List<ACapability> capsList = this._CapsList;
        ValueCapabilities Parent1 = this;
        ValueCapability valueCapability = new ValueCapability(ref Parent1, ValueCaps[index]);
        capsList.Add((ACapability) valueCapability);
        checked { ++index; }
      }
    }

    public ValueCapability this[ushort Index] => (ValueCapability) base[Index];
  }
}
