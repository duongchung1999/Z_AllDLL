// Decompiled with JetBrains decompiler
// Type: BlueNinjaSoftware.HIDLib.Capabilities.DeviceCapabilities
// Assembly: HIDLib, Version=0.1.0.743, Culture=neutral, PublicKeyToken=null
// MVID: 1A59FD53-A8D7-4168-BCB4-F5EF4D8CF47C
// Assembly location: C:\Users\CH210003\Desktop\HIDLib.dll

using BlueNinjaSoftware.HIDLib.Interop;
using Microsoft.VisualBasic;
using System;

namespace BlueNinjaSoftware.HIDLib.Capabilities
{
  public class DeviceCapabilities
  {
    internal HIDDevice _Parent;
    private HID.HIDP_CAPS _Caps;
    private DeviceSubCapabilities _InputCaps;
    private DeviceSubCapabilities _OutputCaps;
    private DeviceSubCapabilities _FeatureCaps;

    internal DeviceCapabilities(ref HIDDevice Parent, IntPtr PHIDP_PREPARSED_DATA)
    {
      HID.NTSTATUS caps = HID.HidP_GetCaps(PHIDP_PREPARSED_DATA, ref this._Caps);
      if (caps >= HID.NTSTATUS.NT_WARNING)
        throw new ApplicationException("Could not get capabilities: " + caps.ToString());
      this._Parent = Parent;
      HID.HIDP_VALUE_CAPS[] ValueCaps1;
      if (this._Caps.NumberInputValueCaps > (ushort) 0)
      {
        ValueCaps1 = new HID.HIDP_VALUE_CAPS[checked ((int) this._Caps.NumberInputValueCaps - 1 + 1)];
        HID.HIDP_VALUE_CAPS[] ValueCaps2 = ValueCaps1;
        uint numberInputValueCaps = (uint) this._Caps.NumberInputValueCaps;
        ref uint local = ref numberInputValueCaps;
        IntPtr PreparsedData = PHIDP_PREPARSED_DATA;
        int valueCaps = (int) HID.HidP_GetValueCaps(HID.HIDP_REPORT_TYPE.Input, ValueCaps2, ref local, PreparsedData);
        this._Caps.NumberInputValueCaps = checked ((ushort) numberInputValueCaps);
        HID.NTSTATUS ntstatus = (HID.NTSTATUS) valueCaps;
        if (ntstatus >= HID.NTSTATUS.NT_WARNING)
        {
          int num = (int) Interaction.MsgBox((object) ("Can't get input value caps: " + ntstatus.ToString()));
        }
      }
      HID.HIDP_BUTTON_CAPS[] ButtonCaps1;
      if (this._Caps.NumberInputButtonCaps > (ushort) 0)
      {
        ButtonCaps1 = new HID.HIDP_BUTTON_CAPS[checked ((int) this._Caps.NumberInputButtonCaps - 1 + 1)];
        HID.HIDP_BUTTON_CAPS[] ButtonCaps2 = ButtonCaps1;
        uint numberInputButtonCaps = (uint) this._Caps.NumberInputButtonCaps;
        ref uint local = ref numberInputButtonCaps;
        IntPtr PreparsedData = PHIDP_PREPARSED_DATA;
        int buttonCaps = (int) HID.HidP_GetButtonCaps(HID.HIDP_REPORT_TYPE.Input, ButtonCaps2, ref local, PreparsedData);
        this._Caps.NumberInputButtonCaps = checked ((ushort) numberInputButtonCaps);
        HID.NTSTATUS ntstatus = (HID.NTSTATUS) buttonCaps;
        if (ntstatus >= HID.NTSTATUS.NT_WARNING)
        {
          int num = (int) Interaction.MsgBox((object) ("Can't get input button caps: " + ntstatus.ToString()));
        }
      }
      DeviceCapabilities Parent1 = this;
      this._InputCaps = new DeviceSubCapabilities(ref Parent1, this._Caps.InputReportByteLength, this._Caps.NumberInputDataIndices, ButtonCaps1, ValueCaps1);
      HID.HIDP_VALUE_CAPS[] ValueCaps3;
      if (this._Caps.NumberOutputValueCaps > (ushort) 0)
      {
        ValueCaps3 = new HID.HIDP_VALUE_CAPS[checked ((int) this._Caps.NumberOutputValueCaps - 1 + 1)];
        HID.HIDP_VALUE_CAPS[] ValueCaps4 = ValueCaps3;
        uint numberOutputValueCaps = (uint) this._Caps.NumberOutputValueCaps;
        ref uint local = ref numberOutputValueCaps;
        IntPtr PreparsedData = PHIDP_PREPARSED_DATA;
        int valueCaps = (int) HID.HidP_GetValueCaps(HID.HIDP_REPORT_TYPE.Output, ValueCaps4, ref local, PreparsedData);
        this._Caps.NumberOutputValueCaps = checked ((ushort) numberOutputValueCaps);
        HID.NTSTATUS ntstatus = (HID.NTSTATUS) valueCaps;
        if (ntstatus >= HID.NTSTATUS.NT_WARNING)
        {
          int num = (int) Interaction.MsgBox((object) ("Can't get output value caps: " + ntstatus.ToString()));
        }
      }
      HID.HIDP_BUTTON_CAPS[] ButtonCaps3;
      if (this._Caps.NumberOutputButtonCaps > (ushort) 0)
      {
        ButtonCaps3 = new HID.HIDP_BUTTON_CAPS[checked ((int) this._Caps.NumberOutputButtonCaps - 1 + 1)];
        HID.HIDP_BUTTON_CAPS[] ButtonCaps4 = ButtonCaps3;
        uint outputButtonCaps = (uint) this._Caps.NumberOutputButtonCaps;
        ref uint local = ref outputButtonCaps;
        IntPtr PreparsedData = PHIDP_PREPARSED_DATA;
        int buttonCaps = (int) HID.HidP_GetButtonCaps(HID.HIDP_REPORT_TYPE.Output, ButtonCaps4, ref local, PreparsedData);
        this._Caps.NumberOutputButtonCaps = checked ((ushort) outputButtonCaps);
        HID.NTSTATUS ntstatus = (HID.NTSTATUS) buttonCaps;
        if (ntstatus >= HID.NTSTATUS.NT_WARNING)
        {
          int num = (int) Interaction.MsgBox((object) ("Can't get output button caps: " + ntstatus.ToString()));
        }
      }
      DeviceCapabilities Parent2 = this;
      this._OutputCaps = new DeviceSubCapabilities(ref Parent2, this._Caps.OutputReportByteLength, this._Caps.NumberOutputDataIndices, ButtonCaps3, ValueCaps3);
      HID.HIDP_VALUE_CAPS[] ValueCaps5;
      if (this._Caps.NumberFeatureValueCaps > (ushort) 0)
      {
        ValueCaps5 = new HID.HIDP_VALUE_CAPS[checked ((int) this._Caps.NumberFeatureValueCaps - 1 + 1)];
        HID.HIDP_VALUE_CAPS[] ValueCaps6 = ValueCaps5;
        uint featureValueCaps = (uint) this._Caps.NumberFeatureValueCaps;
        ref uint local = ref featureValueCaps;
        IntPtr PreparsedData = PHIDP_PREPARSED_DATA;
        int valueCaps = (int) HID.HidP_GetValueCaps(HID.HIDP_REPORT_TYPE.Feature, ValueCaps6, ref local, PreparsedData);
        this._Caps.NumberFeatureValueCaps = checked ((ushort) featureValueCaps);
        HID.NTSTATUS ntstatus = (HID.NTSTATUS) valueCaps;
        if (ntstatus >= HID.NTSTATUS.NT_WARNING)
        {
          int num = (int) Interaction.MsgBox((object) ("Can't get feature value caps: " + ntstatus.ToString()));
        }
      }
      HID.HIDP_BUTTON_CAPS[] ButtonCaps5;
      if (this._Caps.NumberFeatureButtonCaps > (ushort) 0)
      {
        ButtonCaps5 = new HID.HIDP_BUTTON_CAPS[checked ((int) this._Caps.NumberFeatureButtonCaps - 1 + 1)];
        HID.HIDP_BUTTON_CAPS[] ButtonCaps6 = ButtonCaps5;
        uint featureButtonCaps = (uint) this._Caps.NumberFeatureButtonCaps;
        ref uint local = ref featureButtonCaps;
        IntPtr PreparsedData = PHIDP_PREPARSED_DATA;
        int buttonCaps = (int) HID.HidP_GetButtonCaps(HID.HIDP_REPORT_TYPE.Feature, ButtonCaps6, ref local, PreparsedData);
        this._Caps.NumberFeatureButtonCaps = checked ((ushort) featureButtonCaps);
        HID.NTSTATUS ntstatus = (HID.NTSTATUS) buttonCaps;
        if (ntstatus >= HID.NTSTATUS.NT_WARNING)
        {
          int num = (int) Interaction.MsgBox((object) ("Can't get feature button caps: " + ntstatus.ToString()));
        }
      }
      DeviceCapabilities Parent3 = this;
      this._FeatureCaps = new DeviceSubCapabilities(ref Parent3, this._Caps.FeatureReportByteLength, this._Caps.NumberFeatureDataIndices, ButtonCaps5, ValueCaps5);
    }

    public ushort Usage => this._Caps.Usage;

    public ushort UsagePage => this._Caps.UsagePage;

    public DeviceSubCapabilities InputCapabilities => this._InputCaps;

    public DeviceSubCapabilities OutputCapabilities => this._OutputCaps;

    public DeviceSubCapabilities FeatureCapabilities => this._FeatureCaps;

    public ushort NumberLinkCollectionNodes => this._Caps.NumberLinkCollectionNodes;
  }
}
