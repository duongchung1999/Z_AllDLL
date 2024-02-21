// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Capabilities.SingleUsage
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Interop;
using Microsoft.VisualBasic;

namespace BlueNinjaSoftware.HIDLib.Capabilities
{
  public class SingleUsage
  {
    private ACapability _Parent;
    private HID.NotRangeStruct _NotRange;

    internal SingleUsage(ref ACapability Parent, HID.NotRangeStruct NotRange)
    {
      this._Parent = Parent;
      this._NotRange = NotRange;
    }

    public ushort Usage => this._NotRange.Usage;

    public ushort StringID => this._NotRange.StringID;

    public ushort DesignatorID => this._NotRange.DesignatorID;

    public ushort DataIndex => this._NotRange.DataIndex;

    public string GetString()
    {
      string str1 = Strings.StrDup((int) sbyte.MaxValue, char.MinValue);
      string str2;
      return HID.HidD_GetIndexedString((int) this._Parent._Parent._Parent._Parent._Parent.SafeHandle.DangerousGetHandle(), (uint) this._NotRange.StringID, str1, checked ((uint) (str1.Length * 2))) ? Utility.ParseString(str1) : str2;
    }
  }
}
