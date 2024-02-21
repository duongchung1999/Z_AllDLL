// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.HIDDataReceivedEventArgs
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using System;

namespace BlueNinjaSoftware.HIDLib
{
  public class HIDDataReceivedEventArgs : EventArgs
  {
    private byte[] _Data;

    public HIDDataReceivedEventArgs(byte[] Data) => this._Data = Data;

    public byte[] Data => this._Data;
  }
}
