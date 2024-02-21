using Ivi.Visa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using VISAInstrument.Port;

namespace lvi_Visa
{
    public class VISA
    {
        public PortOperatorBase _portOperatorBase = null;
        public static Dictionary<string, object> Config = new Dictionary<string, object>();
        public VISA()
        {
            if (!Directory.Exists($"D:\\MerryTestLog\\MB8852T_Log")) Directory.CreateDirectory($"D:\\MerryTestLog\\MB8852T_Log");
        }
        List<string> listLog = new List<string>();

        #region CW测试
        public string ReadFreqoffCW(string Channel, bool OneTest)
        {
            try
            {
                if (OneTest)
                {

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
            finally
            {
                WriteViasLogi();
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
"CWRESULT MOD",
$"CWRESULT POWER",
"CWRESULT FREQOFF",
"OPMD CWMEAS",
$"CWRESULT MOD"
            };
            return CW;
        }
        /// <summary>
        /// CW测试指令
        /// </summary>
        /// <param name="Channel"></param>
        /// <returns></returns>

        #endregion

        #region BLE TX 测试
        public double _BLE_AvgPower = 0;
        public double _BLE_DeltaF2Avg = 0;

        public string BLETxTest(string Channel)
        {


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
        public string BLERxTest(string Channel, string Packets)
        {
            int frequency = 2402;
            frequency += (int.Parse(Channel) * 2);

            WriteCommand($"LEPKTGENX 71764129,PRBS9,625,{frequency},{Packets},-60,OFF,OFF,37,2LE,OFF,AOA,20,START");
            Thread.Sleep((int)(double.Parse(Packets) * 0.7));
            string str = ReadString();
            return str.Contains("not supported query").ToString();
        }



        #endregion

        #region DUT待测物测试
        public string DUTConnect()
        {
            try
            {

                string Result = "";
                Thread.Sleep(500);
                for (int i = 0; i < 2; i++)
                {
                    foreach (var item in ConnectDut())
                    {
                        WriteCommand(item);
                        ReadString();
                    }
                    if (!isIns(200))
                    {
                        Result = "Pair Timeout False";
                        continue;
                    }

                    WriteCommand("SYSCFG? EUTADDR");
                    string Address = ReadString();
                    foreach (var item in ConnectTestStart())
                    {
                        WriteCommand(item);
                        ReadString();
                    }
                    if (!isIns(250, "45", "46"))
                    {
                        Result = "Test False Timeout False";
                        continue;
                    }
                    //WriteCommand("SYSCFG? EUTNAME");
                    ////Read Name
                    //string Name = ReadString();
                    //send Get Address

                    foreach (var item in ConnectTestEnd())
                    {
                        WriteCommand(item);
                        ReadString();
                    }
                    return $"{Address}";
                }
                return Result;

            }
            finally
            {
                DUTDisconnect();
                DiscardInBuffer();
                WriteViasLogi();

            }

        }

        public void DUTDisconnect()
        {
            WriteCommand("OPMD SCRIPT");
            WriteCommand("DISCONNECT");
        }

        #region 指令部分

        string[] ConnectDut()
        {
            return new string[]
            {

"OPMD SCRIPT;",
"*OPC?",
"OPMD SCRIPT;SCPTSEL 5",
"*OPC?",
"SCPTNM 5,RF Test",
"INQUIRY",
"INQRSP?",
 "CONNECT",
"*INS?"
            };
        }

        string[] ConnectTestStart()
        {
            string[] cmd =
            {

"SCPTCFG 5,ALLTSTS,OFF",
"SCPTCFG 5,OP,ON",
"SCPTCFG 5,PC,OFF",
"SCPTCFG 5,IC,ON",
"SCPTCFG 5,CD,OFF",
"SCPTCFG 5,SS,ON",
"SCPTCFG 5,MS,OFF",
"SCPTCFG 5,MI,ON",
"SCPTCFG 5,MP,OFF",
"SCPTCFG 5,ERP,OFF",
"SCPTCFG 5,EBS,OFF",
"OPCFG 5,HOPPING,HOPOFF",
"OPCFG 5,HOPMODE,DEFINED",
"OPCFG 5,LFREQSEL,ON",
"OPCFG 5,MFREQSEL,ON",
"OPCFG 5,HFREQSEL,ON",
"OPCFG 5,LTXFREQ,FREQ,2402000000.000",
"OPCFG 5,MTXFREQ,FREQ,2441000000.000",
"OPCFG 5,HTXFREQ,FREQ,2480000000.000",
"OPCFG 5,LRXFREQ,FREQ,2480000000.000",
"OPCFG 5,MRXFREQ,FREQ,2402000000.000",
"OPCFG 5,HRXFREQ,FREQ,2402000000.000",
"OPCFG 5,NUMPKTS,10",
"OPCFG 5,PKTTYPE,LONG",
"OPCFG 5,TSTCTRL,LOOPBACK",
"OPCFG 5,AVGMXLIM,20.00",
"OPCFG 5,AVGMNLIM,-6.00",
"OPCFG 5,PEAKLIM,23.00",
"ICCFG 5,LFREQSEL,ON",
"ICCFG 5,MFREQSEL,ON",
"ICCFG 5,HFREQSEL,ON",
"ICCFG 5,LTXFREQ,FREQ,2402000000.000",
"ICCFG 5,MTXFREQ,FREQ,2441000000.000",
"ICCFG 5,HTXFREQ,FREQ,2480000000.000",
"ICCFG 5,LRXFREQ,FREQ,2480000000.000",
"ICCFG 5,MRXFREQ,FREQ,2402000000.000",
"ICCFG 5,HRXFREQ,FREQ,2402000000.000",
"ICCFG 5,NUMPKTS,10",
"ICCFG 5,HOPPING,HOPOFF",
"ICCFG 5,HOPMODE,DEFINED",
"ICCFG 5,MXPOSLIM,75000.00",
"ICCFG 5,MXNEGLIM,75000.00",
"ICCFG 5,TSTCTRL,LOOPBACK",
"SSCFG 5,LFREQSEL,ON",
"SSCFG 5,MFREQSEL,ON",
"SSCFG 5,HFREQSEL,ON",
"SSCFG 5,LTXFREQ,FREQ,2480000000.000",
"SSCFG 5,LRXFREQ,FREQ,2402000000.000",
"SSCFG 5,MTXFREQ,FREQ,2402000000.000",
"SSCFG 5,MRXFREQ,FREQ,2441000000.000",
"SSCFG 5,HTXFREQ,FREQ,2402000000.000",
"SSCFG 5,HRXFREQ,FREQ,2480000000.000",
"SSCFG 5,NUMPKTS,1000",
"SSCFG 5,TXPWR,-70.00",
"SSCFG 5,BERLIM,0.10",
"SSCFG 5,FERLIM,100.00",
"SSCFG 5,HOPPING,HOPOFF",
"SSCFG 5,DIRTYTX,ON",
"SSCFG 5,PKTCOUNT,TX",
"SSCFG 5,DIRTYTAB,OFFSET,0,75.000000KHZ,14.000000KHZ,-2.000000KHZ,1.000000KHZ,39.000000KHZ,0.000000KHZ,-42.000000KHZ,74.000000KHZ,-19.000000KHZ,-75.000000KHZ",
"SSCFG 5,DIRTYTAB,SYMT,0,-20.000000,-20.000000,20.000000,20.000000,20.000000,-20.000000,-20.000000,-20.000000,-20.000000,20.000000",
"SSCFG 5,DIRTYTAB,MODINDEX,0,0.280000,0.300000,0.290000,0.320000,0.330000,0.340000,0.290000,0.310000,0.280000,0.350000",
"MICFG 5,LFREQSEL,ON",
"MICFG 5,MFREQSEL,ON",
"MICFG 5,HFREQSEL,ON",
"MICFG 5,LTXFREQ,FREQ,2402000000.000",
"MICFG 5,LRXFREQ,FREQ,2480000000.000",
"MICFG 5,MTXFREQ,FREQ,2441000000.000",
"MICFG 5,MRXFREQ,FREQ,2402000000.000",
"MICFG 5,HTXFREQ,FREQ,2480000000.000",
"MICFG 5,HRXFREQ,FREQ,2402000000.000",
"MICFG 5,NUMPKTS,10",
"MICFG 5,F1AVGMIN,140000.00",
"MICFG 5,F1AVGMAX,175000.00",
"MICFG 5,F2MAXLIM,115000.00",
"MICFG 5,F1F2MAX,0.80",
"MICFG 5,PKTTYPE,LONG",
"MICFG 5,TSTCTRL,LOOPBACK",
"MICFG 5,TOGGLE,ONCE",
"PATHOFF 5,TABLE",
"*OPC?",
"PATHTBL 5,5",
"PATHEDIT 5,FREQ,2402MHz,-1.20",
"PATHEDIT 5,FREQ,2441MHz,-1.50",
"PATHEDIT 5,FREQ,2480MHz,-2.20",
"SYSCFG? EUTSRCE",
"RUN"
            };
            return cmd;
        }
        string[] ConnectTestEnd()
        {
            string[] read = {
 "ORESULT TEST,0,OP",
"ORESULT TEST,0,IC",
"ORESULT TEST,3,SS",
"ORESULT TEST,0,MI",
"DISCONNECT",
"XRESULT OP,HOPONL",
"XRESULT OP,HOPONM",
"XRESULT OP,HOPONH",
"XRESULT OP,HOPOFFL",
"XRESULT OP,HOPOFFM",
"XRESULT OP,HOPOFFH",
"XRESULT OP,HOPONALL",
"XRESULT OP,HOPONANY",
"XRESULT IC,HOPONL",
"XRESULT IC,HOPONM",
"XRESULT IC,HOPONH",
"XRESULT IC,HOPOFFL",
"XRESULT IC,HOPOFFM",
"XRESULT IC,HOPOFFH",
"XRESULT IC,HOPONALL",
"XRESULT IC,HOPONANY",
"XRESULT SS,HOPOFFL",
"XRESULT SS,HOPOFFM",
"XRESULT SS,HOPOFFH",
"XRESULT SS,HOPONANY",
"XRESULT MI,HOPOFFL",
"XRESULT MI,HOPOFFM",
"XRESULT MI,HOPOFFH",
};
            return read;
        }


        /// <summary>
        /// 判断会在7秒后返回False
        /// </summary>
        /// <returns></returns>
        public bool isIns(int PiarCount = 150, string PassStr = "45", string PassStr2 = "45", string FailStr = "xxx")
        {
            try
            {
                for (int i = 0; i < PiarCount; i++)
                {

                    WriteCommand("*ins?");
                    string inss = ReadString();
                    inss = inss.Replace("\n", "");
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
            finally
            {
                DiscardInBuffer();

            }


        }
        /// <summary>
        /// 建立连接和初始化设备
        /// </summary>
        /// <returns></returns>
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

        void WriteViasLogi()
        {
            listLog.Add("");
            File.AppendAllLines($"D:\\MerryTestLog\\MB8852T_Log\\{DateTime.Now.ToString("yy_MM_dd")}.txt", listLog.ToArray());
            listLog.Clear();
        }
        #endregion

        #region 仪器操作部分
        public bool Connect(string IP)
        {
            if (_portOperatorBase != null)
            {
                return true;
            }
            List<string> listGPIB = new List<string>(PortUltility.FindAddresses(PortType.GPIB));

            if (!PortUltility.OpenIPAddress(IP, out string fullAddress) || !listGPIB.Contains(fullAddress))
            {
                MessageBox.Show($"连接地址：{IP}:未找到设备!");
                return false;
            }
            try
            {
                _portOperatorBase = new GPIBPortOperator(fullAddress);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化设备失败:{ex.Message}");
                _portOperatorBase = null;
                return false;
            }
            try
            {
                _portOperatorBase.Timeout = 50;
                _portOperatorBase.Open();
            }
            catch (Exception ex)
            {
                _portOperatorBase = null;
                MessageBox.Show($"连接设备失败:{ex.Message}");
                return false;

            }
            Thread.Sleep(200);
            WriteCommand("OPMD SCRIPT");
            Thread.Sleep(200);
            WriteCommand("DISCONNECT");
            DiscardInBuffer();
            Thread.Sleep(5000);

            return true;
        }
        public string WriteCommand(string cmd)
        {
            Thread.Sleep(20);
            if (!string.IsNullOrEmpty(cmd))
            {
                try
                {
                    if (_portOperatorBase == null) return "Command False";
                    _portOperatorBase.WriteLine(cmd);
                    string Write = $"{DateTime.Now:yy年MM月dd日 HH时mm分ss秒}  [Write]   {cmd}";
                    listLog.Add(Write);
                    Console.WriteLine(Write);
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
            Thread.Sleep(100);
            string result = "Error False";
            try
            {
                if (_portOperatorBase == null) return "Visa not connected False ";
                result = _portOperatorBase.Read();
                result = result.Replace("\n", "");
                string Read = $"{DateTime.Now:yy年MM月dd日 HH时mm分ss秒}  [Read]   {result}";
                listLog.Add(Read);
                return result;
            }
            catch (IOTimeoutException)
            {

            }
            catch (Exception)
            {

            }
            finally
            {
                Console.WriteLine($"{DateTime.Now:yy年MM月dd日 HH时mm分ss秒}  [Read]   {result}");

            }

            return result;
        }
        public void DiscardInBuffer()
        {
            if (_portOperatorBase != null && _portOperatorBase.IsPortOpen)
                for (int i = 0; i < 10; i++)
                    ReadString();
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
                MessageBox.Show($"MT8852B调用{keys}.Dll失败 \r\n{ex}");
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
                MessageBox.Show($"MT8852B调用{keys}方法失败{methods}\r\n{ex}");
                return "Invoke False";
            }
        }

    }
}