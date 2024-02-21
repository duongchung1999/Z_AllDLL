// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Capabilities.UsageRange
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Interop;
using Microsoft.VisualBasic;

namespace BlueNinjaSoftware.HIDLib.Capabilities
{
  public class UsageRange
  {
    private ACapability _Parent;
    private HID.RangeStruct _Range;

    internal UsageRange(ref ACapability Parent, HID.RangeStruct Range)
    {
      this._Parent = Parent;
      this._Range = Range;
    }

    public ushort UsageMin => this._Range.UsageMin;

    public ushort UsageMax => this._Range.UsageMax;

    public ushort StringMin => this._Range.StringMin;

    public ushort StringMax => this._Range.StringMax;

    public ushort DesignatorMin => this._Range.DesignatorMin;

    public ushort DesignatorMax => this._Range.DesignatorMax;

    public ushort DataIndexMin => this._Range.DataIndexMin;

    public ushort DataIndexMax => this._Range.DataIndexMax;

    public string GetString(ushort StringID)
    {
      string str1 = Strings.StrDup((int) sbyte.MaxValue, char.MinValue);
      string str2;
      return HID.HidD_GetIndexedString((int) this._Parent._Parent._Parent._Parent._Parent.SafeHandle.DangerousGetHandle(), (uint) StringID, str1, checked ((uint) (str1.Length * 2))) ? Utility.ParseString(str1) : str2;
    }
  }
}
