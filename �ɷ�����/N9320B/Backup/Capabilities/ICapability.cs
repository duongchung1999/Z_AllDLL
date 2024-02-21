// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Capabilities.ICapability
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

namespace BlueNinjaSoftware.HIDLib.Capabilities
{
  public interface ICapability
  {
    ushort UsagePage { get; }

    sbyte ReportID { get; }

    bool IsAlias { get; }

    ushort BitField { get; }

    ushort LinkCollection { get; }

    short LinkUsage { get; }

    short LinkUsagePage { get; }

    bool IsRange { get; }

    bool IsStringRange { get; }

    bool IsDesignatorRange { get; }

    bool IsAbsolute { get; }

    UsageRange UsageRange { get; }

    SingleUsage SingleUsage { get; }
  }
}
