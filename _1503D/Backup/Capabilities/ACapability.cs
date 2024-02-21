// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Capabilities.ACapability
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Interop;

namespace BlueNinjaSoftware.HIDLib.Capabilities
{
  public abstract class ACapability
  {
    internal ACapabilities _Parent;
    private UsageRange _UsageRange;
    private SingleUsage _SingleUsage;

    internal ACapability(
      ref ACapabilities Parent,
      HID.RangeStruct CapsRange,
      HID.NotRangeStruct CapsNotRange)
    {
      this._Parent = Parent;
      ACapability Parent1 = this;
      this._UsageRange = new UsageRange(ref Parent1, CapsRange);
      ACapability Parent2 = this;
      this._SingleUsage = new SingleUsage(ref Parent2, CapsNotRange);
    }

    public UsageRange UsageRange => this._UsageRange;

    public SingleUsage SingleUsage => this._SingleUsage;
  }
}
