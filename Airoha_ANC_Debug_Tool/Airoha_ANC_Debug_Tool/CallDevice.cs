using MerryTest.testitem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MerryTest.testitem.UsbInfo;

namespace Airoha_ANC_Debug_Tool
{
    internal class CallDevice
    {

        GetHandle GetHD = new GetHandle();
        UsbInfo GetInfo = new UsbInfo();
        Command HidCMD = new Command();
        public UsbInfo.Info info = new UsbInfo.Info();
        public string _allValue = "False";
        void OpenDevice()
        {
            GetInfo.GetHandle(ref info);
            //Airoha 芯片使用Get Report 方法会有缓冲区数据的问题， 开始前先读几次 后面下指令会比较正常
            if (info.Handle != IntPtr.Zero)
            {
                for (int i = 0; i < 2; i++)
                {
                    HidCMD.GetReportReturn("07", info.I, info.Handle, "0");
                }
            }


        }
        void CloseDevice()
            => GetInfo.CloseHandle(ref info.Handle);
        public string SendReport(string cmd, string ContainsReport)
        {
            return SendReport(cmd, ContainsReport, out _);
        }
        public string SendReport(string cmd, string LastIndexOfStr, string ValueIndex)
        {
            string Value = "";
            string Report = SendReport(cmd, LastIndexOfStr, out string AllValue, true);
            if (Report.Contains("False"))
                return Report;
            string[] valus = AllValue.Split(' ');
            foreach (var item in ValueIndex.Split(' '))
            {
                if (item.Trim().Length <= 0)
                    continue;
                Value += $"{valus[int.Parse(item)]} ";
            }
            Value = Value.Trim();
            return Value;


        }
        public string SendReport(string cmd, string CheckReportStr, out string AllValue, bool LastIndexOf = false)
        {
            OpenDevice();
            try
            {


                int Count = 5;
                AllValue = "";
                _allValue = "False";
                for (int i = 0; i < Count; i++)
                {
                    if (HidCMD.SetReportSend(cmd, info.I, info.Handle))
                    {
                        HidCMD.GetReportReturn("07", info.I, info.Handle, "0");
                        _allValue= AllValue = HidCMD.ALLReturnValue;
                        if (LastIndexOf)
                        {
                            string CheckStr = HidCMD.ALLReturnValue.Substring(0, CheckReportStr.Length);
                            if (CheckStr.Trim() == CheckReportStr.Trim())
                                return "True";
                        }
                        else
                        {
                            if (HidCMD.ALLReturnValue.Contains(CheckReportStr))
                                return "True";
                        }

                    };
                    if (i < Count - 1)
                    {
                        Thread.Sleep(100);
                    }
                }
                return $"Send CMD {info.Handle} False";
            }
            finally
            {
                CloseDevice();
            }
        }

    }
}
