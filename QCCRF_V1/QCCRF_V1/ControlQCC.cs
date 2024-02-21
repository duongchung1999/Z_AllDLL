using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestEngineAPI;

namespace QCCRF_V1.API
{
    internal class ControlQCC
    {
        static object obj_lock = new object();

        public Invoke invoke = null;
        uint QccHandle = 0;
        public Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        public string UpLimit;
        public string LowLimit;


        static string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static string OpenTestEngineDebug(int DebugPort, string Str, ref uint QccHandle)
        {
            lock (obj_lock)
            {
                if (!File.Exists($"{dllPath}/{Str.Split(':')[0]}"))
                    return $"{Str} Not Found False";
                for (int i = 0; i < 3; i++)
                {
                    QccHandle = TestEngine.openTestEngineDebug(DebugPort, 0, 4);
                    if (QccHandle > 0)
                        break;
                    Thread.Sleep(1500);
                }
            }

            if (QccHandle <= 0) return $"Debug {DebugPort}:{QccHandle} False";
            var ini = $"{dllPath}/{Str}";
            //初始化缓冲区
            Thread.Sleep(1000);
            var flag = TestEngine.teConfigCacheInit(QccHandle, ini);
            if (flag != 1) return $"Debug teConfigCacheInit {DebugPort}:{QccHandle}:{flag} False";
            //读取设备数据到缓冲区
            Thread.Sleep(100);
            flag = TestEngine.teConfigCacheRead(QccHandle, null, 0);
            if (flag != 1) return $"Debug teConfigCacheRead {DebugPort}:{QccHandle}:{flag} False";
            return $"{DebugPort}:{QccHandle}:{Str}";

        }

        public string OpenTestEngineDebug()
            => OpenTestEngineDebug(
                OnceConfig.ContainsKey("DebugPort") ? Convert.ToInt32(OnceConfig["DebugPort"]) : 1,
                "hyd.sdb:QCC512X_CONFIG", ref QccHandle);

        public string OpenTestEngineDebug(string sdbFile)
            => OpenTestEngineDebug(
                    OnceConfig.ContainsKey("DebugPort") ? Convert.ToInt32(OnceConfig["DebugPort"]) : 1,
                    sdbFile, ref QccHandle);




        public string _QCC514xCrystalTrim(int Count, double TrimRatio)
        {
            int teRet = TestEngine.teAppDisable(QccHandle, 0);
            if (teRet != 1) return $"Debug teAppDisable {teRet} False";
            teRet = TestEngine.teRadTxCwStart(QccHandle, 39, 8);
            if (teRet != 1) return $"Debug teRadTxCwStart {teRet} False";
            return CrystalTrim(Count, TrimRatio);
        }
        public string _QCC512xCrystalTrim(int Count, double TrimRatio)
        {
            int teRet = TestEngine.teAppDisable(QccHandle, 0);
            if (teRet != 1) return $"Debug teAppDisable {teRet} False";
            teRet = TestEngine.radiotestTxstart(QccHandle, 2441, 50, 255, 0);
            if (teRet != 1) return $"Debug teRadTxCwStart {teRet} False";
            return CrystalTrim(Count, TrimRatio);
        }

        string CrystalTrim(int Count, double TrimRatio)
        {
            string QccWriteResult;
            string ReadRreqoffCW;
            double OffsetHz = -51e9;
            int Trim = 0;
            bool UpLimitFlag = double.TryParse(this.UpLimit, out double UpLimit); UpLimit *= 1000;
            bool LowLimitFlag = double.TryParse(this.LowLimit, out double LowLimit); LowLimit *= 1000;
            bool LimitFlag = LowLimitFlag && UpLimitFlag;
            if (!LimitFlag)
                return "要设定上下限 False";
            bool XtalFalg = true;
            try
            {
                ReadRreqoffCW = invoke.CallMethod($"dllname=RT550_V1&method=LockRT550", OnceConfig);
                //将校准值写成0比较好校准
                for (int i = 0; i < Count; i++)
                {
                    bool initFlag = i == 0;
                    if (!RT550ReadFreq(initFlag, out string FreqStr))
                        return FreqStr;
                    OffsetHz = double.Parse(FreqStr);
                    //读取到符合limit的频偏直接出去，不用一直校准
                    if (LimitFlag && UpLimit > OffsetHz && OffsetHz > LowLimit)
                        return (OffsetHz / 1000).ToString();
                    //第一次校准将校准值写成0会好校准很多
                    if (initFlag)
                    {
                        QccWriteResult = WriteConfigCache(Trim.ToString(), "system3:XtalFreqTrim");
                        if (!RT550ReadFreq(false, out FreqStr))
                            return FreqStr;
                        OffsetHz = double.Parse(FreqStr);
                    }
                    //粗调
                    CapCrystalTrim(OffsetHz, out bool TrimFlag, out string DebugInfo);
                    if (DebugInfo.Contains("False"))
                        return DebugInfo;
                    if (!TrimFlag)
                        continue;
                    //细调节
                    XtalCrystalTrim(XtalFalg, TrimRatio, ref OffsetHz, ref Trim, out DebugInfo);
                    if (DebugInfo.Contains("False"))
                        return DebugInfo;
                    XtalFalg = false;
                }
                return (OffsetHz / 1000).ToString();
            }
            finally
            {
                invoke.CallMethod($"dllname=RT550_V1&method=StopTesting", OnceConfig);
                invoke.CallMethod($"dllname=RT550_V1&method=UnlockRT550", OnceConfig);
            }

        }
        void CapCrystalTrim(double OffsetHz, out bool TrimFlag, out string DebugInfo)
        {
            DebugInfo = "";
            TrimFlag = Math.Abs(OffsetHz) < 9000;
            //粗调
            if (TrimFlag)
                return;
            DebugInfo = WriteConfigCap((OffsetHz) > 0);

        }
        void XtalCrystalTrim(bool OnceTrim, double Ratio, ref double OffsetHz, ref int Trim, out string DebugInfo)
        {
            if (OnceTrim)
            {
                DebugInfo = teConfigCacheReadItem("system3:XtalFreqTrim");
                if (DebugInfo.Contains("False"))
                    return;
                int OldTrim = Convert.ToInt32(DebugInfo);
                int i = Convert.ToInt16(OffsetHz / Ratio);
                Trim = OldTrim + i;
                DebugInfo = WriteConfigCache(Trim.ToString(), "system3:XtalFreqTrim");
                return;
            }
            //细调
            //当频偏大于0的时候
            if (OffsetHz < 0)
            {
                Trim--;
            }
            //当频偏小于0的时候
            else
            {
                Trim++;
            }
            DebugInfo = WriteConfigCache(Trim.ToString(), "system3:XtalFreqTrim");
            if (DebugInfo.Contains("False")) return;
            return;
        }

        string WriteConfigCap(bool add)
        {
            int teRet;
            StringBuilder valueString = new StringBuilder(512);
            uint maxLen = 128;
            teRet = TestEngine.teConfigCacheReadItem(QccHandle, "system3:XtalLoadCapacitance", valueString, out maxLen);
            if (teRet != 1)
                return $"Debug teConfigCacheReadItem {teRet} False";
            int cap = Convert.ToInt32(valueString.ToString(), 16);
            string WriteCapTrim = Convert.ToString((add ? cap + 1 : cap - 1), 16);
            Console.WriteLine($"粗调 system3:XtalLoadCapacitance:{WriteCapTrim}");

            teRet = TestEngine.teConfigCacheWriteItem(QccHandle, "system3:XtalLoadCapacitance", $"0x{WriteCapTrim}");
            if (teRet != 1)
                return $"Debug teConfigCacheWriteItem {teRet} Value Xtal:{$"0x{WriteCapTrim}"} False";
            Thread.Sleep(100);
            teRet = TestEngine.teConfigCacheWrite(QccHandle, null, 0);
            if (teRet != 1)
                return $"Debug teConfigCacheWrite {teRet} False";
            return $"0x{WriteCapTrim}";

        }
        string WriteConfigCache(string Trim, string key)
        {
            int teRet;
            teRet = TestEngine.teConfigCacheWriteItem(QccHandle, key, Trim);
            if (teRet != 1)
                return $"Debug teConfigCacheWriteItem {teRet} Value Trim:{$"0x{Trim}"} False";
            Thread.Sleep(100);
            teRet = TestEngine.teConfigCacheWrite(QccHandle, null, 0);
            if (teRet != 1)
                return $"Debug teConfigCacheWrite {teRet} False";
            return "True";
        }


        public string _QCC514xCrystalTrim_MT8852B(int Count)
        {
            int teRet = TestEngine.teAppDisable(QccHandle, 0);
            if (teRet != 1) return $"Debug teAppDisable {teRet} False";
            teRet = TestEngine.teRadTxCwStart(QccHandle, 39, 8);
            if (teRet != 1) return $"Debug teRadTxCwStart {teRet} False";
            return CrystalTrim_MT8852B(Count);
        }
        public string _QCC512xCrystalTrim_MT8852B(int Count)
        {
            int teRet = TestEngine.teAppDisable(QccHandle, 0);
            if (teRet != 1) return $"Debug teAppDisable {teRet} False";
            teRet = TestEngine.radiotestTxstart(QccHandle, 2441, 50, 255, 0);
            if (teRet != 1) return $"Debug teRadTxCwStart {teRet} False";
            return CrystalTrim_MT8852B(Count);
        }
        string CrystalTrim_MT8852B(int Count)
        {
            string QccWriteResult;
            bool flag = true;
            bool result = false;
            string ReadRreqoffCW;
            double OffsetHz = 0;
            double OldOffsetHz = 0;
            int Trim = 0;
            int countTest = 0;
            QccWriteResult = WriteConfigCache(Trim.ToString(), "system3:XtalFreqTrim");
            for (int i = 0; i < Count; i++)
            {
                ReadRreqoffCW = invoke.CallMethod($"dllname=MT8852B_V1&method=CWReadFreqOffset&Channel={39}&init={flag}", OnceConfig);
                for (int c = 0; c < 3; c++)
                {
                    if (double.TryParse(ReadRreqoffCW, out OffsetHz))
                        break;
                    if (ReadRreqoffCW.Contains("False")) return ReadRreqoffCW;

                }
                //粗调
                if (Math.Abs(OffsetHz) > 9000)
                {
                    QccWriteResult = WriteConfigCap((OffsetHz) > 0);
                    if (QccWriteResult.Contains("False"))
                        return $"{QccWriteResult}";
                    continue;
                }
                countTest++;

                //细调
                //当频偏大于0的时候
                if (OffsetHz < 0)
                {
                    if (!flag && result)
                    {
                        Trim = Math.Abs(OffsetHz) > Math.Abs(OldOffsetHz) ? --Trim : Trim;
                        if (countTest >= 3)
                        {
                            QccWriteResult = WriteConfigCache(Trim.ToString(), "system3:XtalFreqTrim");
                            if (QccWriteResult.Contains("False")) return QccWriteResult;
                            break;
                        }

                    }
                    result = false;
                    if (!flag) Trim--;
                }
                //当频偏小于0的时候
                else
                {
                    if (!flag && !result)
                    {
                        Trim = Math.Abs(OffsetHz) > Math.Abs(OldOffsetHz) ? ++Trim : Trim;
                        if (countTest >= 3)
                        {
                            QccWriteResult = WriteConfigCache(Trim.ToString(), "system3:XtalFreqTrim");
                            if (QccWriteResult.Contains("False")) return QccWriteResult;
                            break;
                        }
                    }
                    result = true;
                    if (!flag) Trim++;
                }
                OldOffsetHz = OffsetHz;
                QccWriteResult = WriteConfigCache(Trim.ToString(), "system3:XtalFreqTrim");
                if (QccWriteResult.Contains("False")) return QccWriteResult;
                flag = false;
            }
            ReadRreqoffCW = invoke.CallMethod($"dllname=MT8852B_V1&method=CWReadFreqOffset&Channel={39}&init={flag}", OnceConfig);
            if (!double.TryParse(ReadRreqoffCW, out OffsetHz))
                return ReadRreqoffCW;
            return (OffsetHz / 1000).ToString();


        }

        bool RT550ReadFreq(bool InitFlag, out string Freq)
        {

            Freq = invoke.CallMethod($"dllname=RT550_V1&method=CWReadFreqOffset&Channel={39}&init={InitFlag}", OnceConfig);
            for (int c = 0; c < 3; c++)
            {
                if (double.TryParse(Freq, out _))
                    return true;
                if (Freq.Contains("False")) return false;

            }
            return false;
        }




        static string CloseTestEngineDebug(ref uint QccHandle)
        {
            int rst;
            if (QccHandle > 0)
            {
                rst = TestEngine.closeTestEngine(QccHandle);
                Thread.Sleep(1000);
            }
            QccHandle = 0;
            return "True";
        }
        public string CloseTestEngineDebug()
            => CloseTestEngineDebug(ref QccHandle);

        public string IntoDut()
        {
            int result = TestEngine.teAppDisable(QccHandle, 0);
            if (result != 1) return $"teAppDisable {result} False";
            result = TestEngine.hciSlave(QccHandle);
            if (result != 1) return $"HciSlave {result} False";
            result = TestEngine.hciEnableDeviceUnderTestMode(QccHandle);
            return result == 1 ? "True" : $"hciEnableDeviceUnderTestMode {result} False";
        }
        public string teConfigCacheReadItem(string key)
        {
            StringBuilder valueString = new StringBuilder(512);
            uint maxLen = 128;
            int i = TestEngine.teConfigCacheReadItem(QccHandle, key, valueString, out maxLen);
            if (i != 1) return $"{i} teConfigCacheReadItem {valueString} False";
            return valueString.ToString();
        }

        public string bccmdSetColdReset()
        {
            var result = TestEngine.bccmdSetColdReset(QccHandle, 2000);
            if (result != 1) return $"{result} bccmdSetColdReset False";
            return "True";
        }
        public string bccmdSetWarmReset()
        {
            var result = TestEngine.bccmdSetWarmReset(QccHandle, 2000);
            if (result != 1) return $"{result} bccmdSetWarmReset False";
            return "True";
        }
        public string teChipReset()
        {
            var result = TestEngine.teChipReset(QccHandle, 0);
            if (result != 1) return $"{result} teChipReset False";
            return "True";
        }
        public string hciReset()
        {
            var result = TestEngine.hciReset(QccHandle);
            if (result != 1) return $"{result} hciReset False";
            return "True";
        }






    }
}
