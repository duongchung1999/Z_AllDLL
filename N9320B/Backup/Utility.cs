// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Utility
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Runtime.CompilerServices;

namespace BlueNinjaSoftware.HIDLib
{
  [StandardModule]
  public sealed class Utility
  {
    internal static string ParseString(string Source)
    {
      Source = Source.TrimEnd(char.MinValue);
      if (Operators.CompareString(Source, "Љ", false) == 0)
        Source = (string) null;
      return Source;
    }

    internal static string PadHex(string HexVal) => "0x" + Strings.StrDup(HexVal.Length % 2, "0") + HexVal;

    internal static string PadHex(string HexVal, int PadSize) => Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject((object) "0x", NewLateBinding.LateGet((object) null, typeof (Strings), "StrDup", new object[2]
    {
      RuntimeHelpers.GetObjectValue(Interaction.IIf(PadSize > HexVal.Length, (object) checked (PadSize - HexVal.Length), (object) (HexVal.Length % 2))),
      (object) "0"
    }, (string[]) null, (Type[]) null, (bool[]) null)), (object) HexVal));
  }
}
