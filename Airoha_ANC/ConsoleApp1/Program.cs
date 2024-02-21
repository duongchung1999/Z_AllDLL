using Airoha.AdjustANC;
using MerryDllFramework;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {


            try
            {
                SerialPort port = new SerialPort();
                port.PortName = "COM1";
                port.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }



            MerryDllFramework.MerryDll dll = new MerryDllFramework.MerryDll();
            dll.Interface(new Dictionary<string, object>()
            {
                { "SqcPath","C:\\Users\\ch200001\\source\\repos\\MerryTest\\Z_AllDll\\Airoha_ANC\\ConsoleApp1\\bin\\Debug\\"},
                { "adminPath","C:\\Users\\ch200001\\source\\repos\\MerryTest\\Z_AllDll\\Airoha_ANC\\ConsoleApp1\\bin\\Debug\\"},
                { "SN","1234"},
                { "Name","HDT565"},

            });
            //Console.WriteLine(dll.Run(new object[] { "dllname=Airoha_ANC&method=GetDevicePath&PID=A520&VID=413C&TimeOut=15" }));
            //Console.WriteLine(dll.Run(new object[] { "dllname=Airoha_ANC&method=HID_InitGain&FB=1&FF=1&Device=Master" }));
            //Console.WriteLine(dll.HID_SetGain("Master"));
            Airoha.AdjustANC.AdjustClass3 a = new Airoha.AdjustANC.AdjustClass3();
            //MerryDllFramework.MerryDll.LFB = -1;
            //MerryDllFramework.MerryDll.RFB = -0;
            //MerryDllFramework.MerryDll.LFF = -2.3;
            //MerryDllFramework.MerryDll.RFF = 6;


            double[] gain = new double[] { 0, 0 };
            //string GainA = Interaction.InputBox(" ", "提示", $"{gain[0]}", -1, -1);
            //string GainB = Interaction.InputBox(" ", "提示", $"{gain[1]}", -1, -1);

            //if (GainA != "")
            //{
            //    gain[0] = double.Parse(GainA);
            //}
            //if (GainB != "")
            //{
            //    gain[1] = double.Parse(GainB);
            //}

            string str = a.ANCAdjust_ForCurves(
               @"C:\Users\ch200001\source\repos\MerryTest\Z_AllDll\Airoha_ANC\ConsoleApp1\bin\Debug",
               true,
               "HDT565",
               "{140:-11} ".TargetStrDic(),
               "{140:-11}".TargetStrDic(),
                "FF", out bool ss,
               ref gain[0], ref gain[1]);


        }


    }
}
