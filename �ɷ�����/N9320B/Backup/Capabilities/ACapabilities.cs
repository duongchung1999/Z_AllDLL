// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Capabilities.ACapabilities
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using System.Collections;
using System.Collections.Generic;

namespace BlueNinjaSoftware.HIDLib.Capabilities
{
  public abstract class ACapabilities : IEnumerable
  {
    internal DeviceSubCapabilities _Parent;
    internal List<ACapability> _CapsList;

    protected ACapabilities(ref DeviceSubCapabilities Parent)
    {
      this._CapsList = new List<ACapability>();
      this._Parent = Parent;
    }

    public ushort Count => checked ((ushort) this._CapsList.Count);

    protected virtual ACapability this[ushort Index] => this._CapsList[(int) Index];

    public IEnumerator GetEnumerator() => (IEnumerator) this._CapsList.GetEnumerator();
  }
}
