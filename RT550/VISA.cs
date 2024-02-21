using CommonUtil;
using Ivi.Visa;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VISAInstrument.Port;

namespace lvi_Visa
{
    public class VISA
    {
        public string Port;

        public static Dictionary<string, object> Config = new Dictionary<string, object>();
        public VISA()
        {
            if (!Directory.Exists($"D:\\MerryTestLog\\MerryTest_RT550Log")) Directory.CreateDirectory($"D:\\MerryTest_RT550Log");
        }
        List<string> listLog = new List<string>();

        #region CW测试
        public string NXPCrystalTrimCWTest(string Channel, int Count, string InitiaTriml, string IP)
        {
            try
            {
                Config["FreqTrim"] = "null";
                #region 仪器初始化部分
                if (!Connect(IP)) return "Connent RT550 False";
                foreach (var item in CWSetConfig(Channel))
                {
                    WriteCommand(item);

                }
                foreach (var item in CW())
                {
                    WriteCommand(item);
                    ReadString();
                    Thread.Sleep(30);
                }
                #endregion
                //写入默认校准值标志位
                bool oneTest = true;
                //读取的频偏
                double offset = 0;
                //校准值
                string Trim = "";
                for (int i = 0; i < Count; i++)
                {
                    WriteCommand("CWRESULT FREQOFF");
                    string ReadStrVisa = ReadString();
                    if (!double.TryParse(ReadStrVisa, out offset))
                    {
                        Trim = ReadStrVisa;
                        continue;
                    };
                    Trim = CallMethod("NXPRF", "Run", new object[] { new object[] { $"dllname=NXPRF&method=NXPCrystalTrim&NXPTrim={InitiaTriml}&OffSet={offset}&OneTest={oneTest}" } });
                    Console.WriteLine(offset);
                    Console.WriteLine(Trim);

                    //不包含Fasle表示校准成功，产品会写入最后一次合适的校准值，需要重新读取频偏
                    if (!Trim.Contains("False"))
                    {
                        //读取频偏
                        WriteCommand("CWRESULT FREQOFF");
                        ReadStrVisa = ReadString();
                        if (!double.TryParse(ReadStrVisa, out offset))
                            return $"最后一次校准成功，但是频偏读取失败 {ReadStrVisa} False";
                        Config["FreqTrim"] = Trim;
                        return (offset / 1000).ToString();
                    }
                    //包含Error表示校准产品已经报错，无法进行操作时可以直接回滚
                    else if (Trim.Contains("Error"))
                        return Trim;
                    Thread.Sleep(100);
                    oneTest = false;
                }
                return $"FreqTrim：{Trim} Freq：{(offset / 1000)} TimeOut False";
            }
            finally
            {
                listLog.Add("");
                File.AppendAllLines($"D:\\MerryTestLog\\MerryTest_RT550Log\\{DateTime.Now.ToString("yy_MM_dd")}.txt", listLog.ToArray());
                listLog.Clear();

            }
        }

        public string Qcc514XCrystalTrimCWTest(string Channel, int Count, string IP)
        {
            try
            {
                Config["FreqTrim"] = "null";
                #region 仪器初始化部分
                if (!Connect(IP)) return "Connent RT550 False";
                foreach (var item in CWSetConfig(Channel))
                {
                    WriteCommand(item);
                    if (item.Contains("OPMD CWMEAS")) Thread.Sleep(600);
                }

                foreach (var item in CW())
                {
                    WriteCommand(item);
                    ReadString();
                    Thread.Sleep(30);
                }
                Thread.Sleep(600);
                #endregion
                //读取的频偏
                double offset = -9 * 1000 * 1000;
                //校准值
                string Trim = "";
                try
                {
                    //写入默认校准值
                    Trim = CallMethod("QCCRF", "Run", new object[] { new object[] { $"dllname=QCCRF&method=QCC514xCrystalTrim&Channel={Channel}&OffSet={offset}&Count={0}&OneTest={true}" } });
                    if (Trim.Contains("Debug"))
                        return Trim;
                    int TrimCount = 0;
                    //第3次开始计算校准，算入判断过程
                    for (int i = 1; i < Count; i++)
                    {
                        Thread.Sleep(100);
                        WriteCommand("CWRESULT FREQOFF");
                        Thread.Sleep(10);
                        string ReadStrVisa = ReadString();
                        if (!double.TryParse(ReadStrVisa, out offset))
                        {
                            Trim = ReadStrVisa;
                            continue;
                        };
                        Console.WriteLine(offset);
                        Trim = CallMethod("QCCRF", "Run", new object[] { new object[] { $"dllname=QCCRF&method=QCC514xCrystalTrim&Channel={Channel}&OffSet={offset}&Count={TrimCount}&OneTest={false}" } });
                        if (!Trim.Contains("cap")) TrimCount++;
                        //不包含Fasle表示校准成功，产品会写入最后一次合适的校准值，需要重新读取频偏
                        if (!Trim.Contains("False"))
                        {
                            //读取频偏
                            WriteCommand("CWRESULT FREQOFF");
                            ReadStrVisa = ReadString();
                            if (!double.TryParse(ReadStrVisa, out offset))
                                return $"最后一次校准成功，但是频偏读取失败 {ReadStrVisa} False";
                            Config["FreqTrim"] = Trim;
                            return (offset / 1000).ToString();
                        }
                        //包含Debug表示对产品操作异常，无法进行操作时可以直接回滚。
                        else if (Trim.Contains("Debug"))
                            return Trim;
                    }
                }
                finally
                {
                    CallMethod("QCCRF", "Run", new object[] { new object[] { $"dllname=QCCRF&method=CloseTestEngineDebug" } });
                }
                return $"FreqTrim：{Trim} Freq：{(offset / 1000)} TimeOut False";
            }
            finally
            {
                listLog.Add("");
                File.AppendAllLines($"D:\\MerryTestLog\\MerryTest_RT550Log\\{DateTime.Now.ToString("yy_MM_dd")}.txt", listLog.ToArray());
                listLog.Clear();

            }

        }

        public string ReadFreqoffCW(string Channel, string IP, bool OneTest)
        {
            try
            {
                if (OneTest)
                {
                    if (!Connect(IP)) return "Connent RT550 False";
                    foreach (var item in CWSetConfig(Channel))
                    {
                        WriteCommand(item);
                        if (item.Contains("OPMD CWMEAS")) Thread.Sleep(600);
                    }
                    foreach (var item in CW())
                    {
                        WriteCommand(item);
                        ReadString();
                        Thread.Sleep(30);
                    }
                }
                WriteCommand("CWRESULT FREQOFF");
                Thread.Sleep(10);
                string ReadStrVisa = ReadString();
                return ReadStrVisa;
            }
            finally
            {
                listLog.Add("");
                File.AppendAllLines($"D:\\MerryTestLog\\MerryTest_RT550Log\\{DateTime.Now.ToString("yy_MM_dd")}.txt", listLog.ToArray());
                listLog.Clear();
            }

        }
        /// <summary>
        /// CW测试设定指令
        /// </summary>
        /// <param name="Channel"></param>
        /// <returns></returns>
        string[] CWSetConfig(string Channel)
        {
            string[] CW =
            {

$"cwmeas chan,{Channel},0.003",
"OPMD SCRIPT",
"OPMD CWMEAS",
$"cwmeas chan,{Channel},0.003",
"CWRESULT POWER",
"CWRESULT FREQOFF",
"CWRESULT MOD"
            };
            return CW;
        }
        /// <summary>
        /// CW测试指令
        /// </summary>
        /// <param name="Channel"></param>
        /// <returns></returns>
        string[] CW()
        {
            string[] CW =
            {
                $"CWRESULT POWER",
"CWRESULT FREQOFF",
"OPMD CWMEAS",
$"CWRESULT MOD"
            };
            return CW;
        }
        #endregion

        #region BLE TX 测试
        public double _BLE_AvgPower = 0;
        public double _BLE_DeltaF2Avg = 0;

        public string BLETxTest(string Channel, string IP)
        {
            if (!Connect(IP)) return "Connent RT550 False";

            foreach (var item in BLETX1(Channel))
            {
                WriteCommand(item);
            }
            if (!isIns(100, "45", "46")) return "Test BLE1 False";
            string _BLE_Result = "";
            foreach (var item in BLETX2())
            {

                WriteCommand(item);
                _BLE_Result = ReadString();
                if (item == "ORESULT TEST,0,LEOP2M")
                    _BLE_AvgPower = double.Parse(_BLE_Result.Split(',')[3]);


            }
            if (!isIns(100, "45", "46")) return "Test BLE2 False";
            foreach (var item in BLETX3())
            {
                WriteCommand(item);
                _BLE_Result = ReadString();
                if (item == "XRESULT LEMI2M,HOPOFFL,0")
                    _BLE_DeltaF2Avg = double.Parse(_BLE_Result.Split(',')[6]);

            }
            if (!isIns(100, "45", "46")) return "Test BLE3 False";
            foreach (var item in BLETX4())
            {
                WriteCommand(item);
                ReadString();
            }
            return "True";
        }

        /// <summary>
        /// 测试部分
        /// </summary>
        /// <returns></returns>
        string[] BLETX1(string Channel)
        {

            return new string[]{
            "OPMD SCRIPT",
"OPMD SCRIPT",
"FIXEDOFF 3,0",
"PATHOFF 3,FIXED",
"scptsel 3",
"LESCPTCFG 3,LEPKTTYPE,BLE,FALSE",
"LESCPTCFG 3,LEPKTTYPE,2LE,TRUE",
"LESCPTCFG 3,LEPKTTYPE,BLECTE,FALSE",
"LESCPTCFG 3,LEPKTTYPE,2LECTE,FALSE",
"LESCPTCFG 3,LEPKTTYPE,LR8,FALSE",
"LESCPTCFG 3,LEPKTTYPE,LR2,FALSE",
"SETBLECAPTYP 2LE",
$"CFGBLECAP {Channel},RF",
"MEASBLECAPX2 LEOP2M,NA,71764129,37,AOA,20"

            };
        }
        string[] BLETX2()
        {

            return new string[] {
"ORESULT TEST,0,LEOP",
"XRESULT LEOP,HOPOFFL,0",
"XRESULT LEOP,HOPOFFM,0",
"XRESULT LEOP,HOPOFFH,0",
"ORESULT TEST,0,LEOP2M",
"XRESULT LEOP2M,HOPOFFL,0",
"XRESULT LEOP2M,HOPOFFM,0",
"XRESULT LEOP2M,HOPOFFH,0",
"ORESULT TEST,0,LEOPBLECTE",
"XRESULT LEOPBLECTE,HOPOFFL,0",
"XRESULT LEOPBLECTE,HOPOFFM,0",
"XRESULT LEOPBLECTE,HOPOFFH,0",
"ORESULT TEST,0,LEOP2LECTE",
"XRESULT LEOP2LECTE,HOPOFFL,0",
"XRESULT LEOP2LECTE,HOPOFFM,0",
"XRESULT LEOP2LECTE,HOPOFFH,0",
"ORESULT TEST,0,LEOPLR8",
"XRESULT LEOPLR8,HOPOFFL,0",
"XRESULT LEOPLR8,HOPOFFM,0",
"XRESULT LEOPLR8,HOPOFFH,0",
"ABORTCAP",
"MEASBLECAPX2 LEMI2M,MOD10101010,71764129,37,AOA,20" };

        }
        string[] BLETX3()
        {

            return new string[] {
                "ORESULT TEST,0,LEMI",
"XRESULT LEMI,HOPOFFL,0",
"XRESULT LEMI,HOPOFFM,0",
"XRESULT LEMI,HOPOFFH,0",
"ORESULT TEST,0,LEMI2M",
"XRESULT LEMI2M,HOPOFFL,0",
"XRESULT LEMI2M,HOPOFFM,0",
"XRESULT LEMI2M,HOPOFFH,0",
"ORESULT TEST,0,LEMILR8",
"XRESULT LEMILR8,HOPOFFL,0",
"XRESULT LEMILR8,HOPOFFM,0",
"XRESULT LEMILR8,HOPOFFH,0",
"ABORTCAP",
"MEASBLECAPX2 LEICD2M,NA,71764129,37,AOA,20"
            };
        }
        string[] BLETX4()
        {

            return new string[] { "ORESULT TEST,1,LEICD",
                "XRESULT LEICD,HOPOFFL,1",
                "XRESULT LEICD,HOPOFFM,1",
                "XRESULT LEICD,HOPOFFH,1",
                "ORESULT TEST,1,LEICD2M",
                "XRESULT LEICD2M,HOPOFFL,1",
                "XRESULT LEICD2M,HOPOFFM,1",
                "XRESULT LEICD2M,HOPOFFH,1",
                "ORESULT TEST,1,LEICDBLECTE",
                "XRESULT LEICDBLECTE,HOPOFFL,1",
                "XRESULT LEICDBLECTE,HOPOFFM,1",
                "XRESULT LEICDBLECTE,HOPOFFH,1",
                "ORESULT TEST,1,LEICD2LECTE",
                "XRESULT LEICD2LECTE,HOPOFFL,1",
                "XRESULT LEICD2LECTE,HOPOFFM,1",
                "XRESULT LEICD2LECTE,HOPOFFH,1",
                "ORESULT TEST,1,LEICDLR8",
                "XRESULT LEICDLR8,HOPOFFL,1",
                "XRESULT LEICDLR8,HOPOFFM,1",
                "XRESULT LEICDLR8,HOPOFFH,1",
                "ABORTCAP" }; ;
        }


        #endregion


        #region BLE RX 测试
        public string BLERxTest(string Channel, string Packets, string IP)
        {
            int frequency = 2402;
            frequency += (int.Parse(Channel) * 2);
            if (!Connect(IP)) return "Connent RT550 False";
            WriteCommand($"LEPKTGENX 71764129,PRBS9,625,{frequency},{Packets},-60,OFF,OFF,37,2LE,OFF,AOA,20,START");
            Thread.Sleep((int)(double.Parse(Packets) * 0.7));
            string str = ReadString();
            return str.Contains("not supported query").ToString();
        }



        #endregion


        #region DUT待测物测试
        public string DUTConnect(string IP)
        {
            try
            {
                if (!Connect(IP)) return "Connent RT550 False";
                string Result = "";
                bool flagconnect = true;
                Thread.Sleep(500);
                for (int i = 0; i < 2; i++)
                {
                    foreach (var item in ConnectCommand())
                    {
                        WriteCommand(item);
                        ReadString();
                    }
                    if (!isIns(200))
                    {
                        Result = "Pair Timeout False";
                        flagconnect = false;
                        continue;
                    }
                    //send Get Name
                    WriteCommand("SYSCFG? EUTNAME");
                    //Read Name
                    string Name = ReadString();
                    //send Get Address
                    WriteCommand("SYSCFG? EUTADDR");
                    string Address = ReadString();
                    Thread.Sleep(1000);
                    
                    return $"Name:{Name} address:{Address}";
                }
                if(flagconnect == false)
                {
                    return Result;
                }
                return true.ToString();

            }
            finally
            {
                listLog.Add("");
                File.AppendAllLines($"D:\\MerryTestLog\\MerryTest_RT550Log\\{DateTime.Now.ToString("yy_MM_dd")}.txt", listLog.ToArray());
                listLog.Clear();

            }

        }

        public string StartTest(String TXP)
        {
            try
            {
                foreach (var item in TestCommand(TXP))
                {
                    WriteCommand(item);
                }
                bool flag = isIns(300, "45", "46");
                foreach (var item in ReadCommand())
                {
                    WriteCommand(item);
                    ReadString();
                }
                return flag ? "True" : "Test False";

            }
            finally
            {
                listLog.Add("");
                File.AppendAllLines($"D:\\MerryTestLog\\MerryTest_RT550Log\\{DateTime.Now.ToString("yy_MM_dd")}.txt", listLog.ToArray());
                listLog.Clear();
            }

        }
        #region 指令部分

        /// <summary>
        /// 初始化设备
        /// </summary>
        /// <param name="PairBitAddress"></param>
        /// <returns></returns>
        string[] PartOne()
        {
            string[] CommandOne =
            {
                "AUTH?",
"*IDN?",
"SYSCFG? BTADDR",
"txp?",
"EXTRALOSS?",
"port?",
"SYSCFG? CONFIG,RANGE",
"SYSCFG? INQSET,TIMEOUT",
"SYSCFG? PAGSET,PAGETO",
"SYSCFG? EUTRS232",
"SYSCFG? EUTSRCE",
"SYSCFG? EUTADDR",
"SYSCFG EUTRS232,19200",
"SYSCFG EUTSRCE,MANUAL",
"SCPTNM? 1",
"SCPTCFG? 1,2",
"scptsel?",
"SCPTNM? 2",
"SCPTCFG? 2,2",
"scptsel?",
"SCPTNM? 3",
"SCPTCFG? 3,2",
"scptsel?",
"SCPTNM? 4",
"SCPTCFG? 4,2",
"scptsel?",
"SCPTNM? 5",
"SCPTCFG? 5,2",
"scptsel?",
"SCPTNM? 6",
"SCPTCFG? 6,2",
"scptsel?",
"SCPTNM? 7",
"SCPTCFG? 7,2",
"scptsel?",
"SCPTNM? 8",
"SCPTCFG? 8,2",
"scptsel?",
"SCPTNM? 9",
"SCPTCFG? 9,2",
"scptsel?",
"SCPTNM? 10",
"SCPTCFG? 10,2",
"scptsel?"
            };
            return CommandOne;
        }
        /// <summary>
        /// 读取的部分指令
        /// </summary>
        /// <returns></returns>
        string[] Read()
        {
            return new string[] {
"ERRLST",
"XResult OP,HopOffH",
"XResult OP,HopOffL",
"XResult OP,HopOffM",
"XResult OP,HopOffH",
"XResult IC,HopOffH",
"XResult IC,HopOffL",
"XResult IC,HopOffM",
"XResult IC,HopOffH",
"XResult SS,HopOffL",
"XResult SS,HopOffM",
"XResult SS,HopOffH",
"ABORT",
"*OPC?",
"DISCONNECT"
            };
        }

        string[] ConnectCommand()
        {
            string[] str = {
            "AUTH?",
"*IDN?",
"SYSCFG? BTADDR",
"txp?",
"EXTRALOSS?",
"port?",
$"PORT { this.Port}",
"SYSCFG? CONFIG,RANGE",
"SYSCFG? INQSET,TIMEOUT",
"SYSCFG? PAGSET,PAGETO",
"SYSCFG? EUTRS232",
"SYSCFG? EUTSRCE",
"SYSCFG? EUTADDR",
"SYSCFG EUTRS232,19200",
"SCPTNM? 1",
"SCPTCFG? 1,2",
"scptsel?",
"SCPTNM? 2",
"SCPTCFG? 2,2",
"scptsel?",
"SCPTNM? 3",
"SCPTCFG? 3,2",
"scptsel?",
"SCPTNM? 4",
"SCPTCFG? 4,2",
"scptsel?",
"SCPTNM? 5",
"SCPTCFG? 5,2",
"scptsel?",
"SCPTNM? 6",
"SCPTCFG? 6,2",
"scptsel?",
"SCPTNM? 7",
"SCPTCFG? 7,2",
"scptsel?",
"SCPTNM? 8",
"SCPTCFG? 8,2",
"scptsel?",
"SCPTNM? 9",
"SCPTCFG? 9,2",
"scptsel?",
"SCPTNM? 10",
"SCPTCFG? 10,2",
"scptsel?",
"OPMD SCRIPT",
"CONNECT"
            };
            return str;
        }
        string[] TestCommand(String TXP)
        {
            string[] Command =
           {

"INQRSP?",
"SYSCFG? EUTADDR",
"*ins?",
"*ins?",
"*ins?",
"*ins?",
"SYSCFG? EUTNAME",
"SYSCFG? EUTFEAT",
"BOOTSTATUS?",
"BOOTSTATUS?",
"CONT",
"CONT",
"OPTSTATUS?",
"OPTSTATUS?",
"*IDN?",
"*IDN?",
"*OPC?",
"*OPC?",
"*CLS",
"*CLS",
"PORT?",
"SYSCFG SCPTSET,LPSTFAIL,ON",
"TESTMODE",
"*INE 15",
"*ESE 56",
"*SRE 35",
"SCPTTSTGP 3, STDTSTS, ON",
"SCPTTSTGP 3, EDRTSTS, OFF",
"SCPTTSTGP 3, BLETSTS, OFF",
"PATHTBLCLR 1",
"PATHTBLCLR 2",
"PATHTBLCLR 3",
"PATHTBLCLR 4",
"PATHTBLCLR 5",
"PATHOFF 3,TABLE",
"PATHTBL 3,1",
"PATHEDIT 1, CHAN, 0, -10.0",
"PATHEDIT 1, CHAN, 39, -10.0",
"PATHEDIT 1, CHAN, 78, -10.0",
"SYSCFG AUTH,STATE,OFF",
"OPMD SCRIPT",
"SYSCFG PAGSET,PAGETO,19",
"SYSCFG INQSET,TIMEOUT,19",
"SYSCFG EUTSRCE, INQUIRY",
"SCPTSEL 3",
"TXPWR 3,-40",
"*OPC?",
"SCRIPTMODE 3,STANDARD",
"SCPTCFG 3,ALLTSTS,OFF",
"SCPTCFG 3,SS,ON",
"SSCFG 3,LTXFREQ,FREQ,2480MHz",
"SSCFG 3,LRXFREQ,FREQ,2402MHz",
"SSCFG 3,LFREQSEL,ON",
"SSCFG 3,MTXFREQ,FREQ,2402MHz",
"SSCFG 3,MRXFREQ,FREQ,2441MHz",
"SSCFG 3,MFREQSEL,ON",
"SSCFG 3,HTXFREQ,FREQ,2402MHz",
"SSCFG 3,HRXFREQ,FREQ,2480MHz",
"SSCFG 3,HFREQSEL,ON",
"SSCFG 3,NUMPKTS,500",
"SSCFG 3,PKTCOUNT, TX",
$"SSCFG 3,TXPWR,{TXP}",
"*OPC?",
"SSCFG 3,HOPPING,HOPOFF",
"SSCFG 3,DIRTYTX,ON",
"SSCFG 3,DRIFTS,ON",
"SCPTCFG 3,OP,ON",
"OPCFG 3,LTXFREQ,FREQ,2402MHz",
"OPCFG 3,LRXFREQ,FREQ,2480MHz",
"OPCFG 3,LFREQSEL,ON",
"OPCFG 3,MTXFREQ,FREQ,2441MHz",
"OPCFG 3,MRXFREQ,FREQ,2402MHz",
"OPCFG 3,MFREQSEL,ON",
"OPCFG 3,HTXFREQ,FREQ,2480MHz",
"OPCFG 3,HRXFREQ,FREQ,2402MHz",
"OPCFG 3,HFREQSEL,ON",
"OPCFG 3,HOPMODE, Defined",
"OPCFG 3,HOPPING,HOPOFF",
"OPCFG 3,NUMPKTS,1",
"OPCFG 3,PKTTYPE,LONG",
"OPCFG 3,TSTCTRL,LOOPBACK",
"SCPTCFG 3,MI,ON",
"MICFG 3,LTXFREQ,FREQ,2402MHz",
"MICFG 3,LRXFREQ,FREQ,2480MHz",
"MICFG 3,LFREQSEL,ON",
"MICFG 3,MTXFREQ,FREQ,2441MHz",
"MICFG 3,MRXFREQ,FREQ,2402MHz",
"MICFG 3,MFREQSEL,ON",
"MICFG 3,HTXFREQ,FREQ,2480MHz",
"MICFG 3,HRXFREQ,FREQ,2402MHz",
"MICFG 3,HFREQSEL,ON",
"MICFG 3,HOPMODE,Defined",
"MICFG 3,HOPPING,HOPON",
"MICFG 3,NUMPKTS,1",
"MICFG 3,TSTCTRL,LOOPBACK",
"MICFG 3,PKTTYPE,LONG",
"MICFG 3,TOGGLE,ONCE",
"*OPC?",
"SCPTCFG 3,IC,ON",
"ICCFG 3,LTXFREQ,FREQ,2402MHz",
"ICCFG 3,LRXFREQ,FREQ,2480MHz",
"ICCFG 3,LFREQSEL,ON",
"ICCFG 3,MTXFREQ,FREQ,2441MHz",
"ICCFG 3,MRXFREQ,FREQ,2402MHz",
"ICCFG 3,MFREQSEL,ON",
"ICCFG 3,HTXFREQ,FREQ,2480MHz",
"ICCFG 3,HRXFREQ,FREQ,2402MHz",
"ICCFG 3,HFREQSEL,ON",
"ICCFG 3,HOPMODE,Defined",
"ICCFG 3,HOPPING,HOPOFF",
"ICCFG 3,NUMPKTS,3",
"ICCFG 3,TSTCTRL,LOOPBACK",
"SCPTCFG 3,CD,ON",
"CDCFG 3,LTXFREQ,FREQ,2402MHz",
"CDCFG 3,LRXFREQ,FREQ,2480MHz",
"CDCFG 3,LFREQSEL,ON",
"CDCFG 3,MTXFREQ,FREQ,2441MHz",
"CDCFG 3,MRXFREQ,FREQ,2402MHz",
"CDCFG 3,MFREQSEL,ON",
"CDCFG 3,HTXFREQ,FREQ,2480MHz",
"CDCFG 3,HRXFREQ,FREQ,2402MHz",
"CDCFG 3,HFREQSEL,ON",
"CDCFG 3,HOPMODE,Defined",
"CDCFG 3,HOPPING,HOPON",
"CDCFG 3,NUMPKTS,1",
"CDCFG 3,TSTCTRL,LOOPBACK",
"CDCFG 3,PKTSIZE,OneSlot,True",
"CDCFG 3,PKTSIZE,ThreeSlot,True",
"CDCFG 3,PKTSIZE,FiveSlot,True",
"*OPC?",
"RUN"
            };
            return Command;
        }
        string[] ReadCommand()
        {
            string[] Command =
            {


"ERRLST",
#region 测试功率
"XResult OP,HopOffH",
"XResult OP,HopOffL",
"XResult OP,HopOffM",
                #endregion

#region 调制特性

"XResult MI,HopOffL",
"XResult MI,HopOffM",
"XResult MI,HopOffH",
	#endregion



#region 频偏
"XResult IC,HopOffL",
"XResult IC,HopOffM",
"XResult IC,HopOffH",
#endregion


"XResult CD,HopOnL",
"XResult CD,HopOnM",
"XResult CD,HopOnH",

#region 灵敏度
"XResult SS,HopOffL",
"XResult SS,HopOffM",
"XResult SS,HopOffH",
	#endregion

"ABORT",
"*OPC?",
"OPMD SCRIPT",
"disconnect"
            };
            return Command;
        }






        /// <summary>
        /// 判断会在7秒后返回False
        /// </summary>
        /// <returns></returns>
        public bool isIns(int PiarCount = 150, string PassStr = "45", string PassStr2 = "46", string FailStr = "xxx")
        {
            for (int i = 0; i < PiarCount; i++)
            {

                WriteCommand("*ins?");
                string inss = ReadString();
                if (inss == PassStr || inss == PassStr2)
                {
                    return true;
                }
                else if (inss == FailStr)
                {
                    return false;
                }
                Thread.Sleep(48);
            }
            return false;

        }
        PortOperatorBase _portOperatorBase = null;
        /// <summary>
        /// 建立连接和初始化设备
        /// </summary>
        /// <returns></returns>

        public string ReadByte()
        {
            try
            {
                byte[] result = _portOperatorBase.ReadBytes(2048);
                string hexstr = ConvertHelper.ByteArrayToHexString(result);

                return hexstr;
            }
            catch (IOTimeoutException)
            {
                MessageBox.Show($"[Read][ERROR:Timeout]");
            }
            catch (Exception)
            {
            }
            return "Error False";
        }
        public void Close()
        {
            if (_portOperatorBase != null)
            {
                try
                {
                    _portOperatorBase.Close();

                }
                catch { }
                _portOperatorBase = null;
            }
        }
        #endregion

        #endregion


        #region 仪器操作部分
        public bool Connect(string IP)
        {
            if (_portOperatorBase != null) return true;
            string IpRegex = @"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$";
            if (!Regex.IsMatch(IP, IpRegex))
            {
                MessageBox.Show("IP地址不正确！");
                return false;
            }

            if (!PortUltility.OpenIPAddress(IP, out string fullAddress))
            {
                MessageBox.Show("未找到设备!");
                return false;
            }
            try
            {
                _portOperatorBase = new LANPortOperator(fullAddress);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化设备失败:{ex.Message}");
                _portOperatorBase = null;
                return false;
            }
            try
            {
                _portOperatorBase.Timeout = 2000;
                _portOperatorBase.Open();
            }
            catch (Exception ex)
            {
                _portOperatorBase = null;
                MessageBox.Show($"连接设备失败:{ex.Message}");
                return false;
            }
            Thread.Sleep(3000);
            WriteCommand($"PORT {this.Port}");
            return true;
        }
        public string WriteCommand(string cmd)
        {
            Thread.Sleep(2);
            if (!string.IsNullOrEmpty(cmd))
            {
                try
                {
                    if (_portOperatorBase == null) return "Command False";
                    _portOperatorBase.WriteLine(cmd);
                    string Write = $"{DateTime.Now:yy年MM月dd日 HH时mm分ss秒}  [Write]   {cmd}";
                    listLog.Add(Write);
#if DEBUG
                    Console.WriteLine(Write);
#endif
                    return cmd;
                }
                catch
                {
                    _portOperatorBase = null;
                }

            }

            return "False";
        }
        public string ReadString()
        {
            Thread.Sleep(2);
            try
            {
                if (_portOperatorBase == null) return "Visa not connected False ";
                var result = _portOperatorBase.Read();
                string Read = $"{DateTime.Now:yy年MM月dd日 HH时mm分ss秒}  [Read]   {result}";
                listLog.Add(Read);
#if DEBUG
                Console.WriteLine(Read);
#endif
                return result;
            }
            catch (IOTimeoutException)
            {

            }
            catch (Exception)
            {

            }
            return "Error False";
        }
        #endregion
        static Dictionary<string, Type> dllType = new Dictionary<string, Type>();
        static Dictionary<string, object> MagicClassObject = new Dictionary<string, object>();
        static bool LoadDll(string keys)
        {
            try
            {
                if (dllType.ContainsKey(keys)) return true;
                string _namespace = "MerryDllFramework";
                string _class = "MerryDll";
                //根据路径读取Dll
                var ass = Assembly.LoadFrom($".\\AllDLL\\{keys}\\{keys}.dll");
                //根据抓的dll执行该命名空间及类
                dllType[keys] = ass.GetType($"{_namespace}.{_class}");
                //抓取该类构造函数并且抓取改方法
                MagicClassObject[keys] = dllType[keys].GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RT550调用{keys}.Dll失败 \r\n{ex}");
                dllType[keys] = null;
                MagicClassObject[keys] = null;
                return false;
            }

        }
        static string CallMethod(string keys, string methods, object[] parameter)
        {
            if (!LoadDll(keys)) return "Load Dll False";
            try
            {
                var mi = dllType[keys].GetMethod(methods);
                return mi.Invoke(MagicClassObject[keys], parameter).ToString();//方法有参数时，需要把null替换为参数的集合
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RT550调用{keys}方法失败{methods}\r\n{ex}");
                return "Invoke False";
            }
        }

    }
}