using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RT550_V1.Communication;

namespace RT550_V1.utility
{
    class ControlRT550
    {
        #region 指令说明
        string[] TestCommand(String TXPWR = "-70", string PATHEDIT = "-10")
        {
            string[] Command =
           {

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
$"PATHEDIT 1, CHAN, 0, {PATHEDIT}",
$"PATHEDIT 1, CHAN, 39, {PATHEDIT}",
$"PATHEDIT 1, CHAN, 78, {PATHEDIT}",
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
$"SSCFG 3,TXPWR,{TXPWR}",
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
        string[] DUTReadCommand()
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
        /// CW测试设定指令
        /// </summary>
        /// <param name="Channel"></param>
        /// <returns></returns>
        string[] CWSetConfig(string Channel)
        {
            string[] CW =
            {


$"cwmeas chan,{Channel},0.003",
"CWRESULT POWER",
"CWRESULT FREQOFF",
"OPMD CWMEAS",
"CWRESULT MOD"
            };
            return CW;
        }

        #endregion

        public string ComPort;
        public string IPv4;
        string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


        //#############################################    DUT    !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        #region dut测试
        Dictionary<string, string> TestResultTable = new Dictionary<string, string>();
        public string DUT_Test(int ConnectSleepTime, int TimeOut, string TXP, string ExtraLoss, bool ManualFlag, string Address)
        {
            TestResultTable.Clear();
            if (!Connect(IPv4))
                return "RT550 Connect False";
            try
            {
                string PairResult = DUTConnect(TXP, ExtraLoss, ManualFlag, Address);
                if (PairResult.Contains("False")) return PairResult;
                Thread.Sleep(ConnectSleepTime);
                string[] TestCommand = File.ReadAllLines($"{dllPath}/DutTestCommand.txt");
                string[] ReadCommand = File.ReadAllLines($"{dllPath}/DutReadResultCommand.txt");
                bool TestFlag = false;
                string TestFalseStr = "Test False";
                foreach (var item in TestCommand)
                {
                    if (item.Contains("*OPC?"))
                    {
                        isOPC();
                    }
                    else
                    {
                        WriteCommand(item);
                    }
                }
                TestFlag = isIns(TimeOut, "45", "46");
                TestFalseStr = $"{ReadString()} {TestFalseStr}";
                foreach (var item in ReadCommand)
                {
                    WriteCommand(item);
                    TestResultTable[item] = ReadString();
                }
                return TestFlag ? PairResult : TestFalseStr;
            }
            finally
            {
                WriteCommand($"disconnect");
            }


        }

        public string DUT_Optional_Test(int TimeOut, string TXP, string ExtraLoss, string PathEditLow, string PathEditMiddle, string PathEditHigh, string Packaging, string PathEdit_P)
        {
            TestResultTable.Clear();
            if (!Connect(IPv4))
                return "RT550 Connect False";
            try
            {
                string SendCMD =
                    $@"
SYSCFG? BTADDR
txp?
EXTRALOSS?
port?
PORT {ComPort}
TXP {TXP}
EXTRALOSS {ExtraLoss}
SYSCFG? CONFIG,RANGE
SYSCFG? INQSET,TIMEOUT
SYSCFG? PAGSET,PAGETO
SYSCFG? EUTRS232
SYSCFG? EUTSRCE
SYSCFG? EUTADDR
SYSCFG EUTRS232,19200
SYSCFG PAGSET,PAGETO,20
SYSCFG INQSET,TIMEOUT,20
SCPTSEL 3
scptsel?
OPMD SCRIPT
SYSCFG EUTSRCE,INQUIRY
OPMD SCRIPT
SYSCFG SCPTSET,LPSTFAIL,ON
TESTMODE
*INE 15
*ESE 56
*SRE 35
SCPTTSTGP 3, STDTSTS, ON
SCPTTSTGP 3, EDRTSTS, OFF
SCPTTSTGP 3, BLETSTS, OFF
PATHTBLCLR 1
PATHTBLCLR 2
PATHTBLCLR 3
PATHTBLCLR 4
PATHTBLCLR 5
PATHOFF 3,TABLE
PATHTBL 3,1
PATHEDIT 1, CHAN, 0, {PathEditLow}
PATHEDIT 1, CHAN, 39, {PathEditMiddle}
PATHEDIT 1, CHAN, 78, {PathEditHigh}
SYSCFG AUTH,STATE,OFF
OPMD SCRIPT
SCPTSEL 3
TXPWR 3,{TXP}
*OPC?
SCRIPTMODE 3,STANDARD
SCPTCFG 3,ALLTSTS,OFF
SCPTCFG 3,SS,ON
SSCFG 3,LTXFREQ,FREQ,2480MHz
SSCFG 3,LRXFREQ,FREQ,2402MHz
SSCFG 3,LFREQSEL,ON
SSCFG 3,MTXFREQ,FREQ,2402MHz
SSCFG 3,MRXFREQ,FREQ,2441MHz
SSCFG 3,MFREQSEL,ON
SSCFG 3,HTXFREQ,FREQ,2402MHz
SSCFG 3,HRXFREQ,FREQ,2480MHz
SSCFG 3,HFREQSEL,ON
SSCFG 3,NUMPKTS,{Packaging}
SSCFG 3,PKTCOUNT, TX
SSCFG 3,TXPWR,{PathEdit_P}
*OPC?
SSCFG 3,HOPPING,HOPOFF
SSCFG 3,DIRTYTX,ON
SSCFG 3,DRIFTS,ON
SCPTCFG 3,OP,ON
OPCFG 3,LTXFREQ,FREQ,2402MHz
OPCFG 3,LRXFREQ,FREQ,2480MHz
OPCFG 3,LFREQSEL,ON
OPCFG 3,MTXFREQ,FREQ,2441MHz
OPCFG 3,MRXFREQ,FREQ,2402MHz
OPCFG 3,MFREQSEL,ON
OPCFG 3,HTXFREQ,FREQ,2480MHz
OPCFG 3,HRXFREQ,FREQ,2402MHz
OPCFG 3,HFREQSEL,ON
OPCFG 3,HOPMODE, Defined
OPCFG 3,HOPPING,HOPOFF
OPCFG 3,NUMPKTS,1
OPCFG 3,PKTTYPE,LONG
OPCFG 3,TSTCTRL,LOOPBACK
SCPTCFG 3,MI,ON
MICFG 3,LTXFREQ,FREQ,2402MHz
MICFG 3,LRXFREQ,FREQ,2480MHz
MICFG 3,LFREQSEL,ON
MICFG 3,MTXFREQ,FREQ,2441MHz
MICFG 3,MRXFREQ,FREQ,2402MHz
MICFG 3,MFREQSEL,ON
MICFG 3,HTXFREQ,FREQ,2480MHz
MICFG 3,HRXFREQ,FREQ,2402MHz
MICFG 3,HFREQSEL,ON
MICFG 3,HOPMODE,Defined
MICFG 3,HOPPING,HOPON
MICFG 3,NUMPKTS,1
MICFG 3,TSTCTRL,LOOPBACK
MICFG 3,PKTTYPE,LONG
MICFG 3,TOGGLE,ONCE
*OPC?
SCPTCFG 3,IC,ON
ICCFG 3,LTXFREQ,FREQ,2402MHz
ICCFG 3,LRXFREQ,FREQ,2480MHz
ICCFG 3,LFREQSEL,ON
ICCFG 3,MTXFREQ,FREQ,2441MHz
ICCFG 3,MRXFREQ,FREQ,2402MHz
ICCFG 3,MFREQSEL,ON
ICCFG 3,HTXFREQ,FREQ,2480MHz
ICCFG 3,HRXFREQ,FREQ,2402MHz
ICCFG 3,HFREQSEL,ON
ICCFG 3,HOPMODE,Defined
ICCFG 3,HOPPING,HOPOFF
ICCFG 3,NUMPKTS,3
ICCFG 3,TSTCTRL,LOOPBACK
SCPTCFG 3,CD,ON
CDCFG 3,LTXFREQ,FREQ,2402MHz
CDCFG 3,LRXFREQ,FREQ,2480MHz
CDCFG 3,LFREQSEL,ON
CDCFG 3,MTXFREQ,FREQ,2441MHz
CDCFG 3,MRXFREQ,FREQ,2402MHz
CDCFG 3,MFREQSEL,ON
CDCFG 3,HTXFREQ,FREQ,2480MHz
CDCFG 3,HRXFREQ,FREQ,2402MHz
CDCFG 3,HFREQSEL,ON
CDCFG 3,HOPMODE,Defined
CDCFG 3,HOPPING,HOPON
CDCFG 3,NUMPKTS,1
CDCFG 3,TSTCTRL,LOOPBACK
CDCFG 3,PKTSIZE,OneSlot,True
CDCFG 3,PKTSIZE,ThreeSlot,True
CDCFG 3,PKTSIZE,FiveSlot,True
*OPC?
RUN

";
                string ReadCMD =
                    @"
ERRLST
XResult OP,HopOffH
XResult OP,HopOffL
XResult OP,HopOffM
XResult MI,HopOffL
XResult MI,HopOffM
XResult MI,HopOffH
XResult IC,HopOffL
XResult IC,HopOffM
XResult IC,HopOffH
XResult CD,HopOnL
XResult CD,HopOnM
XResult CD,HopOnH
XResult SS,HopOffL
XResult SS,HopOffM
XResult SS,HopOffH
ABORT
*OPC?
OPMD SCRIPT
disconnect
";
                string[] TestCommand = SendCMD.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                string[] ReadCommand = ReadCMD.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                bool TestFlag = false;
                string TestFalseStr = "Test False";

                foreach (var item in TestCommand)
                {
                    if (item.Trim().Length <= 0) continue;
                    if (item.Contains("*OPC?"))
                    {
                        isOPC();
                    }
                    else
                    {
                        WriteCommand(item);
                    }
                }
                TestFlag = isIns(TimeOut, "45", "46");
                TestFalseStr = $"{ReadString()} {TestFalseStr}";
                WriteCommand("SYSCFG? EUTADDR");
                string BD = ReadString();
                foreach (var item in ReadCommand)
                {
                    if (item.Trim().Length <= 0) continue;
                    WriteCommand(item);
                    TestResultTable[item] = ReadString();
                }
                return TestFlag ? BD : TestFalseStr;
            }
            finally
            {
                WriteCommand($"disconnect");
            }


        }

        string DUTConnect(string TXP, string ExtraLoss, bool ManualFlag, string Address)
        {
            bool connectFlag = false;
            string BD = null;
            string NoteConnect = "Not Connect False";
            string[] cmdS = {
"*IDN?",
"SYSCFG? BTADDR",
"txp?",
"EXTRALOSS?",
"port?",
$"PORT {ComPort}",
$"TXP {TXP}",
$"EXTRALOSS {ExtraLoss}",
"SYSCFG? CONFIG,RANGE",
"SYSCFG? INQSET,TIMEOUT",
"SYSCFG? PAGSET,PAGETO",
"SYSCFG? EUTRS232",
"SYSCFG? EUTSRCE",
"SYSCFG? EUTADDR",
"SYSCFG EUTRS232,19200",
"SYSCFG PAGSET,PAGETO,20",
"SYSCFG INQSET,TIMEOUT,20",
"SCPTSEL 3",
"scptsel?",
$"OPMD SCRIPT"
            };
            foreach (var item in cmdS)
            {
                WriteCommand(item);
            }
            if (ManualFlag)
            {
                if (Address.Trim().Length <= 0)
                    return $"MANUAL Address {Address} Error False";
                WriteCommand("SYSCFG EUTSRCE,MANUAL");
                WriteCommand($"SYSCFG EUTADDR,{Address}");

            }
            else
            {
                WriteCommand("SYSCFG EUTSRCE,INQUIRY");
            }

            Thread.Sleep(500);
            isOPC();
            for (int i = 0; i < 2; i++)
            {
                WriteCommand("CONNECT");
                if (isIns(12))
                {
                    connectFlag = true;
                    WriteCommand("SYSCFG? EUTADDR");
                    BD = ReadString();
                    break;
                }
                string ManualStr = ManualFlag ? "MANUAL" : "INQUIRY";
                NoteConnect = $"{ReadString()} {ManualStr} Pair Time Out False";
            }
            return connectFlag ? BD : NoteConnect;

        }
        bool isIns(int TimeOutS = 15, string PassStr = "45", string PassStr2 = "45")
        {
            TimeOutS *= 4;
            for (int i = 0; i < TimeOutS; i++)
            {

                WriteCommand("*ins?");
                string inss = ReadString();
                if (inss == PassStr || inss == PassStr2)
                    return true;
                Thread.Sleep(250);
            }
            return false;

        }
        public string DUTGetTestResult(string ReadCMD, int Index, double AddNumber)
        {

            string DUTResult = TestResultTable[ReadCMD];
            string[] ResultSplit = DUTResult.Split(',');
            if (ResultSplit[2] != "TRUE") return $"{DUTResult} False";
            string IndexStr = ResultSplit[Index];
            double value = double.Parse(IndexStr);
            return Math.Round(value + AddNumber, 3).ToString();
        }
        public string DUTGetTestResult_Khz(string ReadCMD, int Index, double AddNumber)
        {

            string DUTResult = TestResultTable[ReadCMD];
            string[] ResultSplit = DUTResult.Split(',');
            if (ResultSplit[2] != "TRUE") return $"{DUTResult} False";
            string IndexStr = ResultSplit[Index];
            double value = double.Parse(ResultSplit[Index]);
            return Math.Round(value / 1000 + AddNumber, 3).ToString();
        }

        public string DUTSwitchUnitsGetTestResult(string units, string ReadCMD, int Index, double AddNumber)
        {

            if (units == "/1000_Khz")
            {
                return DUTGetTestResult_Khz(ReadCMD, Index, AddNumber);
            }
            else
            {
                return DUTGetTestResult(ReadCMD, Index, AddNumber);

            }

        }


        #endregion

        //#############################################    CW     !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        #region CW
        bool init = false;
        public string CWReadFreqOffset(string Channel, bool OneTest, out string Power)
        {
            Power = "Connent RT550 False";
            string ReadStrVisa = "Connent RT550 False";
            if (!Connect(IPv4))
                return "Connent RT550 False";
            if (OneTest || !init)
            {
                WriteCommand($"PORT {this.ComPort}");
                foreach (var item in CWSetConfig(Channel))
                {
                    WriteCommand(item);
                    if (item.Contains("OPMD CWMEAS")) Thread.Sleep(200);
                }
                init = true;
                Thread.Sleep(100);
            }
            WriteCommand("CWRESULT POWER");
            Thread.Sleep(100);
            Power = ReadString();
            double poaerD = double.Parse(Power);
            if (poaerD <= -60) return $"Freq No Signal Power {poaerD} False";


            WriteCommand("CWRESULT FREQOFF");
            ReadStrVisa = ReadString();
            if (double.TryParse(ReadStrVisa, out double Freqoff))
            {
                return Freqoff.ToString();
            }
            return ReadStrVisa;
        }
        public string CWReadFreqOffset(string Channel, bool OneTest)
            => CWReadFreqOffset(Channel, OneTest, out _);
        public string CWReadPower(string FreqMhzStr, bool OneTest)
        {
            double FreqMhz = double.Parse(FreqMhzStr);
            double TrunCate = Math.Truncate(FreqMhz);
            string Channel = ((int)TrunCate - 2402).ToString();
            CWReadFreqOffset(Channel, OneTest, out string Power);
            return Power;
        }


        #endregion

        //#############################################    BLE     !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        #region BLE

        public string BLE_Optional_Rx_Test(int Channel, string Standard, string PowerLevel, string Payload, string SyncWord, string Spacing, string PayloadLength, string Packets)
        {
            int frequency = 2402 + (Channel * 2);
            if (!Connect(IPv4)) return "Connent RT550 False";
            WriteCommand($"PORT {this.ComPort}");
            Thread.Sleep(1000);
            string CommandTxt = $"LEPKTGENX {SyncWord},{Payload},{Spacing},{frequency},{Packets},{PowerLevel},OFF,OFF,{PayloadLength},{Standard},OFF,AOA,20,START";
            WriteCommand(CommandTxt);
            Thread.Sleep((int)(double.Parse(Packets) * 0.7));
            string Result = ReadString();
            isOPC();
            return Result.Contains("not supported query").ToString();
        }
        public string BLERxTest(string Channel, string Packets)
        {
            int frequency = 2402;
            frequency += (int.Parse(Channel) * 2);
            if (!Connect(IPv4)) return "Connent RT550 False";
            WriteCommand($"PORT {this.ComPort}");
            Thread.Sleep(10);
            string CommandTxt = File.ReadAllLines($"{dllPath}/BLE_RX_TestCommand.txt")[0];
            CommandTxt = CommandTxt.Replace("{frequency}", $"{frequency}").Replace("{Packets}", $"{Packets}");
            //WriteCommand($"LEPKTGENX 71764129,PRBS9,625,{frequency},{Packets},-60,OFF,OFF,37,2LE,OFF,AOA,20,START");
            WriteCommand(CommandTxt);

            Thread.Sleep((int)(double.Parse(Packets) * 0.7));
            string Result = ReadString();
            return Result.Contains("not supported query").ToString();
        }

        Dictionary<string, string> BLETestResultTable = new Dictionary<string, string>();

        public string BLE_Tx_PowerTest(string Channel, double AddNumber)
        {
            BLETestResultTable.Clear();
            if (!Connect(IPv4))
                return "Connent RT550 False";
            WriteCommand($"PORT {this.ComPort}");
            Thread.Sleep(200);
            string[] BLE_TX_TestCommand = File.ReadAllLines($"{dllPath}/BLE_TX_TestCommand.txt");
            string[] ReadCMD =
                {
//"ORESULT TEST,0,LEOP",
//"XRESULT LEOP,HOPOFFL,0",
//"XRESULT LEOP,HOPOFFM,0",
//"XRESULT LEOP,HOPOFFH,0",
"ORESULT TEST,0,LEOP2M",
"XRESULT LEOP2M,HOPOFFL,0",
"XRESULT LEOP2M,HOPOFFM,0",
"XRESULT LEOP2M,HOPOFFH,0",
//"ORESULT TEST,0,LEOPBLECTE",
//"XRESULT LEOPBLECTE,HOPOFFL,0",
//"XRESULT LEOPBLECTE,HOPOFFM,0",
//"XRESULT LEOPBLECTE,HOPOFFH,0",
//"ORESULT TEST,0,LEOP2LECTE",
//"XRESULT LEOP2LECTE,HOPOFFL,0",
//"XRESULT LEOP2LECTE,HOPOFFM,0",
//"XRESULT LEOP2LECTE,HOPOFFH,0",
//"ORESULT TEST,0,LEOPLR8",
//"XRESULT LEOPLR8,HOPOFFL,0",
//"XRESULT LEOPLR8,HOPOFFM,0",
//"XRESULT LEOPLR8,HOPOFFH,0",
"ABORTCAP"};
            foreach (var item in BLE_TX_TestCommand)
            {
                if (item.Contains("*OPC?"))
                {
                    isOPC();
                }
                else if (item.Contains("CFGBLECAP"))
                {
                    WriteCommand(item.Replace("{Channel}", Channel));
                }
                else
                {
                    WriteCommand(item);
                }
            }
            isOPC();
            isIns(10, "45", "46");
            foreach (var item in ReadCMD)
            {
                WriteCommand(item);
                BLETestResultTable[item] = ReadString();
            }
            return BLEGetTestResult("ORESULT TEST,0,LEOP2M", 3, AddNumber, 1);

        }

        public string BLE_CarrierOffset(double AddNumber)
        {
            WriteCommand("MEASBLECAPX2 LEICD2M,NA,71764129,37,AOA,20");
            isOPC();
            isIns(10, "45", "46");
            string[] ReadCMD =
                {
//"ORESULT TEST,1,LEICD",
//"XRESULT LEICD,HOPOFFL,1",
//"XRESULT LEICD,HOPOFFM,1",
//"XRESULT LEICD,HOPOFFH,1",
"ORESULT TEST,1,LEICD2M",
"XRESULT LEICD2M,HOPOFFL,1",
"XRESULT LEICD2M,HOPOFFM,1",
"XRESULT LEICD2M,HOPOFFH,1",
//"ORESULT TEST,1,LEICDBLECTE",
//"XRESULT LEICDBLECTE,HOPOFFL,1",
//"XRESULT LEICDBLECTE,HOPOFFM,1",
//"XRESULT LEICDBLECTE,HOPOFFH,1",
//"ORESULT TEST,1,LEICD2LECTE",
//"XRESULT LEICD2LECTE,HOPOFFL,1",
//"XRESULT LEICD2LECTE,HOPOFFM,1",
//"XRESULT LEICD2LECTE,HOPOFFH,1",
//"ORESULT TEST,1,LEICDLR8",
//"XRESULT LEICDLR8,HOPOFFL,1",
//"XRESULT LEICDLR8,HOPOFFM,1",
//"XRESULT LEICDLR8,HOPOFFH,1",
"ABORTCAP"};
            foreach (var item in ReadCMD)
            {
                WriteCommand(item);
                BLETestResultTable[item] = ReadString();
            }
            return BLEGetTestResult_Khz("ORESULT TEST,1,LEICD2M", 2, AddNumber, 1);
        }

        public string BLE_Modulation(double AddNumber)
        {
            WriteCommand("MEASBLECAPX2 LEMI2M,MOD10101010,71764129,37,AOA,20");
            isOPC();
            isIns(10, "45", "46");
            string[] ReadCMD =
                {
//"XRESULT LEMI,HOPOFFL,0",
//"XRESULT LEMI,HOPOFFM,0",
//"XRESULT LEMI,HOPOFFH,0",
"ORESULT TEST,0,LEMI2M",
"XRESULT LEMI2M,HOPOFFL,0",
"XRESULT LEMI2M,HOPOFFM,0",
"XRESULT LEMI2M,HOPOFFH,0",
//"ORESULT TEST,0,LEMILR8",
//"XRESULT LEMILR8,HOPOFFL,0",
//"XRESULT LEMILR8,HOPOFFM,0",
//"XRESULT LEMILR8,HOPOFFH,0",
"ABORTCAP"};

            foreach (var item in ReadCMD)
            {
                WriteCommand(item);
                BLETestResultTable[item] = ReadString();
            }

            return BLEGetTestResult_Khz("XRESULT LEMI2M,HOPOFFL,0", 6, AddNumber, 2);
        }

        public string BLE_Tx_PowerTest_2M(string Channel, string FIXEDOFF, string SyncWord, string PayloadLength, int ResultIndex, double AddNumber)
        {
            BLETestResultTable.Clear();
            if (!Connect(IPv4))
                return "Connent RT550 False";
            WriteCommand($"PORT {this.ComPort}");
            Thread.Sleep(100);
            string[] BLE_TX_TestCommand = new string[] {
                 "OPMD SCRIPT",
                "OPMD SCRIPT",
                $"FIXEDOFF 3,{FIXEDOFF}",
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
                "*OPC?",
                $"MEASBLECAPX2 LEOP2M,NA,{SyncWord},{PayloadLength},AOA,20",
            };
            string[] ReadCMD =
                {
"ORESULT TEST,0,LEOP2M",
"XRESULT LEOP2M,HOPOFFL,0",
"XRESULT LEOP2M,HOPOFFM,0",
"XRESULT LEOP2M,HOPOFFH,0",
"ABORTCAP"};
            SendCommands(BLE_TX_TestCommand, ReadCMD);
            return BLEGetTestResult("ORESULT TEST,0,LEOP2M", ResultIndex, AddNumber, 1);
        }

        public string BLE_CarrierOffset_2M(string SyncWord, string PayloadLength, int ResultIndex, double AddNumber)
        {
            string[] ReadCMD =
                  {
//"ORESULT TEST,1,LEICD",
//"XRESULT LEICD,HOPOFFL,1",
//"XRESULT LEICD,HOPOFFM,1",
//"XRESULT LEICD,HOPOFFH,1",
"ORESULT TEST,1,LEICD2M",
"XRESULT LEICD2M,HOPOFFL,1",
"XRESULT LEICD2M,HOPOFFM,1",
"XRESULT LEICD2M,HOPOFFH,1",
//"ORESULT TEST,1,LEICDBLECTE",
//"XRESULT LEICDBLECTE,HOPOFFL,1",
//"XRESULT LEICDBLECTE,HOPOFFM,1",
//"XRESULT LEICDBLECTE,HOPOFFH,1",
//"ORESULT TEST,1,LEICD2LECTE",
//"XRESULT LEICD2LECTE,HOPOFFL,1",
//"XRESULT LEICD2LECTE,HOPOFFM,1",
//"XRESULT LEICD2LECTE,HOPOFFH,1",
//"ORESULT TEST,1,LEICDLR8",
//"XRESULT LEICDLR8,HOPOFFL,1",
//"XRESULT LEICDLR8,HOPOFFM,1",
//"XRESULT LEICDLR8,HOPOFFH,1",
"ABORTCAP"};
            SendCommands(new string[] { $"MEASBLECAPX2 LEICD2M,NA,{SyncWord},{PayloadLength},AOA,20" }, ReadCMD);
            return BLEGetTestResult_Khz("ORESULT TEST,1,LEICD2M", ResultIndex, AddNumber, 1);
        }
        public string BLE_Modulation_2M(string SyncWord, string Payload, string PayloadLength, int ResultIndex, string units, double AddNumber)
        {
            string[] ReadCMD =
                 {
//"XRESULT LEMI,HOPOFFL,0",
//"XRESULT LEMI,HOPOFFM,0",
//"XRESULT LEMI,HOPOFFH,0",
"ORESULT TEST,0,LEMI2M",
"XRESULT LEMI2M,HOPOFFL,0",
"XRESULT LEMI2M,HOPOFFM,0",
"XRESULT LEMI2M,HOPOFFH,0",
//"ORESULT TEST,0,LEMILR8",
//"XRESULT LEMILR8,HOPOFFL,0",
//"XRESULT LEMILR8,HOPOFFM,0",
//"XRESULT LEMILR8,HOPOFFH,0",
"ABORTCAP"};
            SendCommands(new string[] { $"MEASBLECAPX2 LEMI2M,{Payload},{SyncWord},{PayloadLength},AOA,20" }, ReadCMD);
            return BLESwitchUnitsGetTestResult(units, "ORESULT TEST,0,LEMI2M", ResultIndex, AddNumber, 1);
        }



        public string BLE_Tx_PowerTest_1M(string Channel, string FIXEDOFF, string SyncWord, string PayloadLength, int ResultIndex, double AddNumber)
        {
            BLETestResultTable.Clear();
            if (!Connect(IPv4))
                return "Connent RT550 False";
            WriteCommand($"PORT {this.ComPort}");
            Thread.Sleep(100);
            string[] BLE_TX_TestCommand = {
                "OPMD SCRIPT",
                "OPMD SCRIPT",
                $"FIXEDOFF 3,{FIXEDOFF}",
                "PATHOFF 3,FIXED",
                "scptsel 3",
                "LESCPTCFG 3,LEPKTTYPE,BLE,TRUE",
                "LESCPTCFG 3,LEPKTTYPE,2LE,FALSE",
                "LESCPTCFG 3,LEPKTTYPE,BLECTE,FALSE",
                "LESCPTCFG 3,LEPKTTYPE,2LECTE,FALSE",
                "LESCPTCFG 3,LEPKTTYPE,LR8,FALSE",
                "LESCPTCFG 3,LEPKTTYPE,LR2,FALSE",
                "SETBLECAPTYP BLE",
                $"CFGBLECAP {Channel},RF",
                "*OPC?",
                $"MEASBLECAPX2 LEOP,NA,{SyncWord},{PayloadLength},AOA,20",


            };
            string[] ReadCMD =
                {
"ORESULT TEST,0,LEOP",
"XRESULT LEOP,HOPOFFL,0",
"XRESULT LEOP,HOPOFFM,0",
"XRESULT LEOP,HOPOFFH,0",
"ABORTCAP"};
            SendCommands(BLE_TX_TestCommand, ReadCMD);
            return BLEGetTestResult("ORESULT TEST,0,LEOP", ResultIndex, AddNumber, 1);

        }
        public string BLE_CarrierOffset_1M(string SyncWord, string PayloadLength, int ResultIndex, double AddNumber)
        {
            string[] ReadCMD =
                {
"ORESULT TEST,1,LEICD",
"XRESULT LEICD,HOPOFFL,1",
"XRESULT LEICD,HOPOFFM,1",
"XRESULT LEICD,HOPOFFH,1",
"ABORTCAP"};
            SendCommands(new string[] { $"MEASBLECAPX2 LEICD,NA,{SyncWord},{PayloadLength},AOA,20" }, ReadCMD);
            return BLEGetTestResult_Khz("ORESULT TEST,1,LEICD", ResultIndex, AddNumber, 1);
        }
        public string BLE_Modulation_1M(string SyncWord, string Payload, string PayloadLength, int ResultIndex, string units, double AddNumber)
        {
            string[] ReadCMD =
                {
                "ORESULT TEST,0,LEMI",
"XRESULT LEMI,HOPOFFL,0",
"XRESULT LEMI,HOPOFFM,0",
"XRESULT LEMI,HOPOFFH,0",
"ABORTCAP"};
            SendCommands(new string[] { $"MEASBLECAPX2 LEMI,{Payload},{SyncWord},{PayloadLength},AOA,20" }, ReadCMD);
            return BLESwitchUnitsGetTestResult(units, "ORESULT TEST,0,LEMI", ResultIndex, AddNumber, 1);
        }


        void SendCommands(string[] Command1, string[] Command2)
        {
            foreach (var item in Command1)
            {
                if (item.Contains("*OPC?"))
                {
                    isOPC();
                }
                else
                {
                    WriteCommand(item);
                }
            }
            isOPC();
            isIns(10, "45", "46");
            foreach (var item in Command2)
            {
                WriteCommand(item);
                BLETestResultTable[item] = ReadString();
            }
        }
        public string BLEGetTestResult(string ReadCMD, int Index, double AddNumber, int ResultFlagIndex)
        {

            string DUTResult = BLETestResultTable[ReadCMD];
            string[] ResultSplit = DUTResult.Split(',');
            if (ResultSplit[ResultFlagIndex] != "TRUE") return $"{DUTResult} False";
            string IndexStr = ResultSplit[Index];
            double value = double.Parse(IndexStr);
            return Math.Round(value + AddNumber, 3).ToString();
        }

        public string BLEGetTestResult_Khz(string ReadCMD, int Index, double AddNumber, int ResultFlagIndex)
        {

            string DUTResult = BLETestResultTable[ReadCMD];
            string[] ResultSplit = DUTResult.Split(',');
            if (ResultSplit[ResultFlagIndex] != "TRUE") return $"{DUTResult} False";
            string IndexStr = ResultSplit[Index];
            double value = double.Parse(IndexStr);
            return Math.Round(value / 1000 + AddNumber, 3).ToString();
        }

        public string BLESwitchUnitsGetTestResult(string units, string ReadCMD, int Index, double AddNumber, int ResultFlagIndex)
        {

            if (units == "/1000_Khz")
            {
                return BLEGetTestResult_Khz(ReadCMD, Index, AddNumber, ResultFlagIndex);
            }
            else
            {
                return BLEGetTestResult(ReadCMD, Index, AddNumber, ResultFlagIndex);

            }

        }


        #region BLE旧代码，已经被遗弃
        public string BLETxTest(string Channel)
        {
            if (!Connect(IPv4)) return "Connent RT550 False";

            WriteCommand($"PORT {this.ComPort}");
            Thread.Sleep(10);

            foreach (var item in BLETX1(Channel))
            {
                WriteCommand(item);
            }

            if (!isIns(6, "45", "46")) return "Test BLE1 False";


            foreach (var item in BLETX2())
            {
                WriteCommand(item);
                BLETestResultTable[item] = ReadString();
            }



            if (!isIns(6, "45", "46")) return "Test BLE2 False";
            foreach (var item in BLETX3())
            {
                WriteCommand(item);
                BLETestResultTable[item] = ReadString();
            }


            if (!isIns(6, "45", "46")) return "Test BLE3 False";
            foreach (var item in BLETX4())
            {
                WriteCommand(item);
                BLETestResultTable[item] = ReadString();
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

            return new string[] {
"ORESULT TEST,1,LEICD",
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
"ABORTCAP"
            };
        }
        #endregion

        #endregion


        void isOPC()
        {
            for (int i = 0; i < 15; i++)
            {
                WriteCommand("*OPC?");
                Thread.Sleep(100);
                string OPC = ReadString();
                if (OPC == "1") return;
                Thread.Sleep(500);
            }
        }

        public string StopTesting()
        {
            if (_portOperatorBase != null) WriteCommand("OPMD SCRIPT");
            init = false;
            return "True";
        }
        public string Set_RF_Port(string Port)
        {
            this.ComPort = Port;
            if (!Connect(IPv4))
                return "RT550 Connect False";
            string Result = WriteCommand($"PORT {this.ComPort}");
            if (Result.Contains("False"))
            {
                return Result;
            }
            Thread.Sleep(500);
            return "True";
        }


        public string SaveLog()
        {
            lock (listLog)
            {
                try
                {
                    string D = "D:/MerryTestLog/MerryTest_RT550Log";
                    string root = "./LOG";
                    string pathName = $"RT550SCPI_CMD_{DateTime.Now:yyMMdd}.txt";
                    if (!Directory.Exists(D))
                        Directory.CreateDirectory(D);
                    if (!Directory.Exists(root))
                        Directory.CreateDirectory(root);
                    File.AppendAllLines($"{D}/{pathName}", listLog.ToArray());
                    File.AppendAllLines($"{root}/{pathName}", listLog.ToArray());
                    listLog.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("RT550指令日记生成失败");
                    Task.Run(() => MessageBox.Show(ex.ToString()));
                }
            }


            return "True";
        }




    }
    static class _expand
    {
        public static int DUTSwictModulationPattern(this string index)
        {
            switch (index)
            {
                case "F1_Max": return 3;
                case "F1_Average": return 4;
                case "F2_Max": return 5;
                case "F2_Average": return 6;
                case "F2_Avg/Flavg": return 7;
                case "F2_Max_Failed": return 8;
                case "F2_Max_Count_(Total)": return 9;
                case "PassRate": return 13;
                default:
                    return 0;
            }
        }

        public static int BLESwictModulationPattern(this string index)
        {
            switch (index)
            {
                case "F1_Max": return 2;
                case "F1_Average": return 3;
                case "F2_Max": return 4;
                case "F2_Average": return 5;
                case "F2_Avg/Flavg": return 6;
                case "F2_Max_Failed": return 7;
                case "F2_Max_Count_(Total)": return 8;
                case "PassRate": return 12;
                default:
                    return 0;
            }
        }
        public static int SwictPowerResult(this string index)
        {
            switch (index)
            {
                case "PeakToAvg": return 5;
                case "AvgPower": return 3;
                default:
                    return 0;
            }
        }
        public static int SwictCarrierFrequencyResult(this string index)
        {
            switch (index)
            {
                case "Max+veOffset": return 3;
                case "Max-veOffset": return 4;
                case "AverageOffset": return 2;
                case "MaxDrift": return 6;
                case "AvgDrift": return 7;
                case "InitialDriftRate": return 11;
                case "DriftRate": return 5;
                default:
                    return 0;
            }
        }


    }

    class _LockRT550
    {
        private static bool Flag;
        static object obj_Lock = new object();
        static string title = "";
        public static bool Lock(string testID)
        {
            lock (obj_Lock)
            {
                if (Flag)
                {
                    RT550_V1.Froms.ProgressBars progress = new RT550_V1.Froms.ProgressBars();
                    progress.Text = $"{testID}:线程提示";
                    progress.lable = $"{title}:线程正在使用RT550";
                    progress.ShowDialog();
                    while (Flag)
                        Thread.Sleep(100);
                }
                Flag = true;
                title = testID;
                return true;


            }
        }
        public static bool UnLock()
        {
            Thread.Sleep(250);
            RT550_V1.Froms.ProgressBars.showDialogFlag = Flag = false;
            return true;
        }


    }
}


