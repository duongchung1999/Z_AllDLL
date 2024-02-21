using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MT8852B_V1.Ulitity.Communication;

namespace MT8852B_V1
{
    internal class ControlMT8852B
    {
        public string GPIB = "";
        string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Dictionary<string, string> TestResultTable = new Dictionary<string, string>();
        Dictionary<string, string> BLETestResultTable = new Dictionary<string, string>();


        //#############################################    DUT    !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        #region dut测试
        public string DUT_Test(int ConnectSleepTime, int TimeOut)
        {


            TestResultTable.Clear();
            if (!Connect(GPIB))
                return "Conect MT8852B False";
            try
            {
                string PairResult = DUTConnect();
                if (PairResult.Contains("False")) return PairResult;
                Thread.Sleep(ConnectSleepTime);
                string[] TestCommand = File.ReadAllLines($"{dllPath}/DutTestCommand.txt");
                string[] ReadCommand = File.ReadAllLines($"{dllPath}/DutReadResultCommand.txt");
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
                bool TestFlag = isIns(TimeOut, "45", "13");

                if (!TestFlag)
                {
                    WriteCommand("*ins?");
                    TestFalseStr = $"{ReadString()} {TestFalseStr}";
                }
                foreach (var item in ReadCommand)
                {

                    WriteCommand(item);
                    TestResultTable[item] = ReadString();
                }
                return TestFlag ? PairResult : TestFalseStr;
            }
            finally
            {
                DUTDisconnect();
                DiscardInBuffer();
            }
        }
        string DUTConnect()
        {
            bool ConnectFlag = false;
            string BD = null;
            string NoteConnect = "Not Connect False";

            WriteCommand($"OPMD SCRIPT;");
            WriteCommand($"OPMD SCRIPT;SCPTSEL 5");
            WriteCommand($"SCPTNM 5,RF Test");
            WriteCommand($"INQRSP?");
            WriteCommand($"INQUIRY");
            for (int i = 0; i < 2; i++)
            {
                WriteCommand($"CONNECT");

                if (isIns(15, "45", "13"))
                {
                    ConnectFlag = true;
                    WriteCommand("SYSCFG? EUTADDR");
                    BD = ReadString();
                    break;
                }
                WriteCommand("*ins?");
                NoteConnect = $"{ReadString()} Pair Time Out False";

            }
            return ConnectFlag ? BD : NoteConnect;
        }


        public void DUTDisconnect()
        {

            WriteCommand("ABORT");
            WriteCommand("OPMD SCRIPT");
            WriteCommand("DISCONNECT");
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


        //#############################################    CW     !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        #region CW测试
        public string ReadFreqoffCW(string Channel, bool OneTest)
        {
            if (OneTest)
            {
                if (!Connect(GPIB))
                    return "Conect MT8852B False";
                foreach (var item in CWSetConfig(Channel))
                {
                    WriteCommand(item);
                    if (item.Contains("OPMD CWMEAS")) Thread.Sleep(1000);
                    ReadString();
                }
                for (int i = 0; i < 5; i++)
                {
                    WriteCommand("CWRESULT FREQOFF");
                    Thread.Sleep(180);
                    ReadString();
                }
                DiscardInBuffer();
            }
            string ReadStrVisa = "";
            for (int i = 0; i < 3; i++)
            {
                WriteCommand("CWRESULT FREQOFF");
                Thread.Sleep(180);
                ReadStrVisa = ReadString();
                if (ReadStrVisa != "") break;
            }
            return ReadStrVisa;

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
"CWRESULT MOD",
$"CWRESULT POWER",
"CWRESULT FREQOFF",
"OPMD CWMEAS",
$"CWRESULT MOD"
            };
            return CW;
        }
        #endregion

        //#############################################    BLE     !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        #region BLE测试
        string BLETest(string[] SendCMD, bool InitFlag)
        {
            BLETestResultTable.Clear();
            if (!Connect(GPIB))
                return "Conect MT8852B False";
            string Value = "Note Test False";
            string[] ReadCMD = new string[] {
                "SCPTSEL?",
                "OPTSTATUS?",
                "LEOPCFG? 3, LEPKTTYPE,LR8",
                "LEOPCFG? 3, LEPKTTYPE,2LE",
                "ORESULT TEST,0,LEOP2M" };

            if (InitFlag)
                init();
            foreach (var item in SendCMD)
            {

                if (item.Contains("*OPC?"))
                {
                    isOPC();
                }
                else
                {
                    WriteCommand(item);
                    if (item.Contains("?"))
                        ReadString();
                }
            }
            bool TestFlag = isIns(10, "45", "46");
            if (!TestFlag)
            {
                WriteCommand("*ins?");
                string inss = ReadString();
                Value = $"{inss} False";
            }
            foreach (var item in ReadCMD)
            {
                WriteCommand(item);
                BLETestResultTable[item] = ReadString();
            }
            return TestFlag ? "True" : Value;
        }

        public string BLE_TX_LowPowerTest(bool InitFlag, double AddNumber)
        {
            string[] SendCMD = File.ReadAllLines($@"{dllPath}\BLE_SendStrLowCmd.txt");
            string TestFlag = BLETest(SendCMD, InitFlag);
            return TestFlag == "True" ? BLEGetTestResult("ORESULT TEST,0,LEOP2M", 2, AddNumber) : TestFlag;
        }

        public string BLE_TX_MiddlePowerTest(bool InitFlag, double AddNumber)
        {
            string[] SendCMD = File.ReadAllLines($@"{dllPath}\BLE_SendStrMiddleCmd.txt");
            string TestFlag = BLETest(SendCMD, InitFlag);
            return TestFlag == "True" ? BLEGetTestResult("ORESULT TEST,0,LEOP2M", 2, AddNumber) : TestFlag;

        }

        public string BLE_TX_HighPowerTest(bool InitFlag, double AddNumber)
        {
            string[] SendCMD = File.ReadAllLines($@"{dllPath}\BLE_SendStrHighCmd.txt");

            string TestFlag = BLETest(SendCMD, InitFlag);
            return TestFlag == "True" ? BLEGetTestResult("ORESULT TEST,0,LEOP2M", 2, AddNumber) : TestFlag;
        }

        public string BLE_TX_CarrierOffset(double AddNumber)
        {
            WriteCommand("SERVULPX LEICD2M, 1, NA,True,71764129,37");
            isOPC();
            isIns(10, "45", "46");
            string[] ReadCMD = new string[] {
                "SCPTSEL?",
                "OPTSTATUS?",
                "LEICDCFG? 3, LEPKTTYPE,LR8",
                "LEICDCFG? 3, LEPKTTYPE,2LE",
                "ORESULT TEST,1,LEICD2M" };
            foreach (var item in ReadCMD)
            {
                WriteCommand(item);
                BLETestResultTable[item] = ReadString();
            }

            return BLEGetTestResult_Khz("ORESULT TEST,1,LEICD2M", 2, AddNumber);
        }

        public string BLE_TX_Modulation(double AddNumber)
        {

            WriteCommand("SERVULPX LEMI2M, 1, MOD10101010,True,71764129,37");
            isOPC();
            isIns(10, "45", "46");
            string[] ReadCMD = new string[] {
                "SCPTSEL?",
                "OPTSTATUS?",
                "LEMICFG? 3, LEPKTTYPE,LR8",
                "LEMICFG? 3, LEPKTTYPE,2LE",
                "ORESULT TEST,0,LEMI2M" };
            foreach (var item in ReadCMD)
            {
                WriteCommand(item);
                BLETestResultTable[item] = ReadString();
            }
            return BLEGetTestResult_Khz("ORESULT TEST,0,LEMI2M", 5, AddNumber);

        }

        public string BLE_RX_Sensitivity()
        {
            string[] SendCMD = File.ReadAllLines($@"{dllPath}\BLE_Sensitivity.txt");
            foreach (var item in SendCMD)
            {
                if (item.Contains("*OPC?"))
                {
                    isOPC();
                }
                else
                {
                    WriteCommand(item);
                    if (item.Contains("?"))
                        ReadString();
                }
            }
            bool SendFlag = isOPC(20);
            return SendFlag ? "True" : "Sensitivity False";

        }

        public string BLE_Optional_RX_Sensitivity(int Channel, string Standard, string PowerLevel, string Payload, string SyncWord, string Spacing, string PayloadLength, string Packets)
        {
            int frequency = 2402 + (Channel * 2);

            string[] SendCMD = new string[] {
                "SCPTSEL 9",
                "PATHOFF 9, OFF",
                "*RST",
                "*OPC?",
                "SYSCFG EUTRS232,115200",
                "SYSCFG USBADAPTOR,PORT,A",
                "SYSCFG EUTHANDSHAKE,RTS/CTS",
                "OPMD LESIGGEN",
                "*OPC?",
                $"LEPKTGEN {SyncWord},{Payload},{Spacing},{frequency},{Packets},{PowerLevel},OFF,OFF,{PayloadLength},{Standard},START",
                "*OPC?",
            };

            foreach (var item in SendCMD)
            {
                if (item.Contains("*OPC?"))
                {
                    isOPC();
                }
                else
                {
                    WriteCommand(item);
                    if (item.Contains("?"))
                        ReadString();
                }
            }
            bool SendFlag = isOPC(20);
            return SendFlag ? "True" : "Sensitivity False";

        }


        string BLEGetTestResult(string ReadCMD, int Index, double AddNumber)
        {
            string DUTResult = BLETestResultTable[ReadCMD];
            string[] ResultSplit = DUTResult.Split(',');
            if (ResultSplit[1] != "TRUE") return $"{DUTResult} False";
            string IndexStr = ResultSplit[Index];
            double value = double.Parse(IndexStr);
            return Math.Round(value + AddNumber, 3).ToString();
        }
        string BLEGetTestResult_Khz(string ReadCMD, int Index, double AddNumber)
        {
            string DUTResult = BLETestResultTable[ReadCMD];
            string[] ResultSplit = DUTResult.Split(',');
            if (ResultSplit[1] != "TRUE") return $"{DUTResult} False";
            string IndexStr = ResultSplit[Index];
            double value = double.Parse(ResultSplit[Index]);
            return Math.Round(value / 1000 + AddNumber, 3).ToString();
        }


        #endregion


        bool isIns(int TimeOutS = 15, string PassStr = "45", string PassStr2 = "45")
        {
            try
            {
                TimeOutS *= 2;
                for (int i = 0; i < TimeOutS; i++)
                {

                    WriteCommand("*ins?");
                    string inss = ReadString();
                    if (inss == PassStr || inss == PassStr2)
                        return true;
                    Thread.Sleep(500);
                }
                return false;
            }
            finally
            {
                DiscardInBuffer();

            }


        }
        bool isOPC(int Count = 15)
        {
            DiscardInBuffer(10);

            for (int i = 0; i < Count; i++)
            {
                WriteCommand("*OPC?");
                Thread.Sleep(50);
                string OPC = ReadString();
                if (OPC == "1")
                    return true;
                Thread.Sleep(200);
            }
            return false;

        }
        void init()
        {
            WriteCommand("*RST");
            Thread.Sleep(500);
            WriteCommand("OPMD SCRIPT;");
            isOPC();
            WriteCommand("SCPTSEL 3");
            isOPC();
            WriteCommand("SYSCFG EUTRS232,115200");
            WriteCommand("SYSCFG USBADAPTOR,PORT,A");
            WriteCommand("SYSCFG EUTHANDSHAKE,RTS/CTS");
        }



        public string SaveLog()
        {
            lock (listLog)
            {
                try
                {
                    string D = "D:/MerryTestLog/MerryTest_MT8852BLog";
                    string root = "./LOG";
                    string pathName = $"MT8852BSCPI_CMD_{DateTime.Now:yyMMdd}.txt";
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
                    MessageBox.Show("MT8852B指令日记生成失败");
                    Task.Run(() => MessageBox.Show(ex.ToString()));
                }
            }


            return "True";
        }

    }
    static class _expand
    {
        public static int SwictModulationPattern(this string index)
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
                case "Failed": return 10;
                case "Tested": return 11;
                default:
                    return 0;
            }
        }
    }
}
