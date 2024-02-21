using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestEngineAPI;
using QCCRF;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：QCCRF";
            string dllfunction = "Dll功能说明 ：高通芯片RF测试和RF校准";
            string dllHistoryVersion = "历史Dll版本：0.0.0.0";
            string dllVersion = "当前Dll版本：0.0.1.1";
            string dllChangeInfo = "Dll改动信息：";
            string[] info = { dllname, dllfunction, dllHistoryVersion, dllVersion, dllChangeInfo };
            return info;
        }
        Invoke invoke = null;
        Dictionary<string, object> Config = new Dictionary<string, object>();
        public object Interface(Dictionary<string, object> keys)
        {
            invoke = new Invoke(keys);
            return Config = keys;
        }
        public string _dllpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        /// <summary>
        /// 记录校准值
        /// </summary>
        static int Trim;
        /// <summary>
        /// 记录上一次的频偏状态(大于0为true,小于0为false)
        /// </summary>
        static bool result;
        /// <summary>
        /// 记录上一次的频偏值
        /// </summary>
        static double OldFrequencyOffsetValue = 0;
        public string Run(object[] Command)
        {

            List<string> cmd = new List<string>();

            foreach (var item in Command)
            {
                if (item.GetType() != typeof(string)) continue;
                cmd = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < cmd.Count; i++) cmd[i] = cmd[i].Split('=')[1];
            }

            switch (cmd[1])
            {
                case "QCC514xCrystalTrim": return QCC514xCrystalTrim(cmd[2], double.Parse(cmd[3]), int.Parse(cmd[4]), bool.Parse(cmd[5]));
                case "_QCC514xCrystalTrim": return _QCC514xCrystalTrim(int.Parse(cmd[2]));
                case "_QCC512xCrystalTrim": return _QCC512xCrystalTrim(int.Parse(cmd[2]));

                case "_QCC514xCrystalTrim_MT8852B": return _QCC514xCrystalTrim_MT8852B(int.Parse(cmd[2]));
                case "_QCC512xCrystalTrim_MT8852B": return _QCC512xCrystalTrim_MT8852B(int.Parse(cmd[2]));

                case "QCC3024CrystalTrim": return QCC3024CrystalTrim(int.Parse(cmd[2]));
                case "OpenTestEngineDebug514X": return OpenTestEngineDebug514X();
                case "OpenTestEngineDebug": return OpenTestEngineDebug(cmd[2]);
                case "CloseTestEngineDebug": return CloseTestEngineDebug();
                case "teConfigCacheReadItem": return teConfigCacheReadItem(cmd[2]);
                case "bccmdSetColdReset": return bccmdSetColdReset();
                case "teChipReset": return teChipReset();
                case "IntoDut": return IntoDut();

                default:
                    return "Command Error False";
            }

        }

        string bccmdSetColdReset()
        {
            var result = TestEngine.bccmdSetColdReset(QccHandle, 2000);
            if (result != 1) return $"{result} bccmdSetColdReset False";
            Thread.Sleep(3000);
            return "True";
        }
        string teChipReset()
        {
            var result = TestEngine.teChipReset(QccHandle, 0);
            if (result != 1) return $"{result} teChipReset False";
            Thread.Sleep(2000);
            return "True";
        }
        string IntoDut()
        {
            var result = TestEngine.hciSlave(QccHandle);
            if (result != 1) return $"HciSlave {result} False";
            result = TestEngine.hciEnableDeviceUnderTestMode(QccHandle);
            return result == 1 ? "True" : $"hciEnableDeviceUnderTestMode {result} False";
        }
        string teConfigCacheReadItem(string key)
        {
            StringBuilder valueString = new StringBuilder(512);
            uint maxLen = 128;
            int i = TestEngine.teConfigCacheReadItem(QccHandle, key, valueString, out maxLen);
            if (i != 1) return $"{i} teConfigCacheReadItem {valueString} False";
            return valueString.ToString();
        }

        uint QccHandle = 0;
        public string 调试用()
        {
            int teRet;
            string QccWriteResult = OpenTestEngineDebug("hyd.sdb:QCC512X_CONFIG");
            teRet = TestEngine.teAppDisable(QccHandle, 0);
            var result = TestEngine.teChipReset(QccHandle, 0);
            if (QccWriteResult != "True") return QccWriteResult;
            teRet = TestEngine.teAppDisable(QccHandle, 0);
            if (teRet != 1)
                return $"{teRet} teAppDisable False";
            teRet = TestEngine.radiotestTxstart(QccHandle, 2441, 50, 255, 0);
            if (teRet != 1)
                return $"{teRet} radiotestTxstart False";
            while (true)
            {
                StringBuilder valueString = new StringBuilder(512);
                uint maxLen = 128;
                int i = TestEngine.teConfigCacheReadItem(QccHandle, "system3:XtalFreqTrim", valueString, out maxLen);
                Console.WriteLine("校准后：" + valueString);
                Console.WriteLine("请输入校正值：" + valueString);
                String STR = Console.ReadLine();
                if (STR.ToUpper() == "EX") break;

                TestEngine.teConfigCacheWriteItem(QccHandle, "system3:XtalFreqTrim", STR);
                Thread.Sleep(100);
                TestEngine.teConfigCacheWrite(QccHandle, null, 0);

            }


            TestEngine.closeTestEngine(QccHandle);
            return "";

        }
        #region 514X
        string Qcc514xIniRF(string Channel)
        {
            int teRet = 1;
            string DebugStr = OpenTestEngineDebug514X();
            if (DebugStr.Contains("False")) return $"{DebugStr}";
            teRet = TestEngine.teAppDisable(QccHandle, 0);
            if (teRet != 1) return $"Debug teAppDisable {teRet} False";
            teRet = TestEngine.teRadTxCwStart(QccHandle, byte.Parse(Channel), 8);
            if (teRet != 1) return $"Debug teRadTxCwStart {teRet} False";
            return "True";
        }
        string QCC514xCrystalTrim(string channel, double FrequencyOffsetValue, int count, bool flag)
        {
            string QccWriteResult = "";

            if (flag)
            {
                string QccIniResult = Qcc514xIniRF(channel);
                if (QccIniResult.Contains("False"))
                    return QccIniResult;
            }
            //粗调
            if (!flag)
            {
                string result = "";
                if (FrequencyOffsetValue > 10000)
                {
                    result = WriteConfigCap(true);
                    if (result.Contains("False"))
                        return $"{result}";
                    return $"{result} cap False";
                }
                else if (FrequencyOffsetValue < -10000)
                {
                    result = WriteConfigCap(false);
                    if (result.Contains("False"))
                        return $"{result}";
                    return $"{result} cap False";
                }

            }
            try
            {
                if (flag) Trim = 0;
                if (FrequencyOffsetValue < 0)
                {
                    if (!flag && result)
                    {
                        int trim;
                        if (Math.Abs(FrequencyOffsetValue) > Math.Abs(OldFrequencyOffsetValue))
                        {
                            trim = --Trim;
                        }
                        else
                        {
                            trim = Trim;
                        }
                        if (count >= 3)
                        {
                            QccWriteResult = WriteConfigCache(trim.ToString(), "system15:XtalFreqTrim");
                            if (QccWriteResult.Contains("False")) return QccWriteResult;
                            Trim = 0;
                            return Convert.ToString(trim);
                        }
                    }
                    result = false;
                    if (!flag) Trim--;
                }
                else
                {
                    if (!flag && !result)
                    {
                        int trim;
                        if (Math.Abs(FrequencyOffsetValue) > Math.Abs(OldFrequencyOffsetValue))
                        {
                            trim = ++Trim;
                        }
                        else
                        {
                            trim = Trim;
                        }
                        if (Trim > 15 || Trim < -15) return $"{Trim}";
                        if (count >= 3)
                        {
                            QccWriteResult = WriteConfigCache(trim.ToString(), "system15:XtalFreqTrim");
                            if (QccWriteResult.Contains("False")) return QccWriteResult;
                            Trim = 0;
                            return Convert.ToString(trim);
                        }

                    }
                    result = true;

                    if (!flag) Trim++;
                }
                if (!flag) OldFrequencyOffsetValue = FrequencyOffsetValue;
                QccWriteResult = WriteConfigCache(Trim.ToString(), "system15:XtalFreqTrim");
                if (QccWriteResult.Contains("False")) return QccWriteResult;
                return $"{Trim} False";
            }
            catch (Exception ex)
            {
                return $"Error {ex.Message} False";
            }

        }

        /// <summary>
        /// 写粗调部分
        /// </summary>
        /// <param name="add"></param>
        /// <returns></returns>

        #endregion
        string QCC3024CrystalTrim(int count)
        {
            string QccWriteResult;
            bool flag = true;
            bool result = false;
            string ReadRreqoffCW;
            double OffsetHz = 0;
            double OldOffsetHz = 0;
            int Trim = 0;
            QccWriteResult = WriteConfigCache(Trim.ToString(), "system3:XtalFreqTrim");
            int teRet = TestEngine.teAppDisable(QccHandle, 0);
            if (teRet != 1)
                return $"{teRet} teAppDisable False";
            teRet = TestEngine.radiotestTxstart(QccHandle, 2441, 50, 255, 0);
            if (teRet != 1)
                return $"{teRet} radiotestTxstart False";
            int countTest = 0;
            for (int i = 0; i < count; i++)
            {

                ReadRreqoffCW = invoke.CallMethod($"dllname=RT550&method=ReadFreqoffCW&Channel={39}&init={flag}");
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
                        Trim = Math.Abs(OffsetHz) > Math.Abs(OldOffsetHz) ? ++Trim : Trim;
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
            ReadRreqoffCW = invoke.CallMethod($"dllname=RT550&method=ReadFreqoffCW&Channel={39}&init={flag}");
            if (!double.TryParse(ReadRreqoffCW, out OffsetHz))
                return ReadRreqoffCW;
            return (OffsetHz / 1000).ToString();

        }
        string _QCC514xCrystalTrim(int count)
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
            int teRet = TestEngine.teAppDisable(QccHandle, 0);
            if (teRet != 1) return $"Debug teAppDisable {teRet} False";
            teRet = TestEngine.teRadTxCwStart(QccHandle, 39, 8);
            if (teRet != 1) return $"Debug teRadTxCwStart {teRet} False";
            for (int i = 0; i < count; i++)
            {

                ReadRreqoffCW = invoke.CallMethod($"dllname=RT550&method=ReadFreqoffCW&Channel={39}&init={flag}");
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

            ReadRreqoffCW = invoke.CallMethod($"dllname=RT550&method=ReadFreqoffCW&Channel={39}&init={flag}");
            if (!double.TryParse(ReadRreqoffCW, out OffsetHz))
                return ReadRreqoffCW;
            return (OffsetHz / 1000).ToString();

        }


        string _QCC512xCrystalTrim(int count)
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
            int teRet = TestEngine.teAppDisable(QccHandle, 0);
            if (teRet != 1) return $"Debug teAppDisable {teRet} False";
            teRet = TestEngine.radiotestTxstart(QccHandle, 2441, 50, 255, 0);
            if (teRet != 1) return $"Debug teRadTxCwStart {teRet} False";
            for (int i = 0; i < count; i++)
            {
                ReadRreqoffCW = invoke.CallMethod($"dllname=RT550&method=ReadFreqoffCW&Channel={39}&init={flag}");
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

            ReadRreqoffCW = invoke.CallMethod($"dllname=RT550&method=ReadFreqoffCW&Channel={39}&init={flag}");
            if (!double.TryParse(ReadRreqoffCW, out OffsetHz))
                return ReadRreqoffCW;
            return (OffsetHz / 1000).ToString();

        }
        string _QCC514xCrystalTrim_MT8852B(int count)
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
            int teRet = TestEngine.teAppDisable(QccHandle, 0);
            if (teRet != 1) return $"Debug teAppDisable {teRet} False";
            teRet = TestEngine.teRadTxCwStart(QccHandle, 39, 8);
            if (teRet != 1) return $"Debug teRadTxCwStart {teRet} False";
            for (int i = 0; i < count; i++)
            {

                ReadRreqoffCW = invoke.CallMethod($"dllname=MT8852B&method=ReadFreqoffCW&Channel={39}&init={i == 0}");
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

            ReadRreqoffCW = invoke.CallMethod($"dllname=MT8852B&method=ReadFreqoffCW&Channel={39}&init={flag}");
            if (!double.TryParse(ReadRreqoffCW, out OffsetHz))
                return ReadRreqoffCW;
            return (OffsetHz / 1000).ToString();

        }
        string _QCC512xCrystalTrim_MT8852B(int count)
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
            int teRet = TestEngine.teAppDisable(QccHandle, 0);
            if (teRet != 1) return $"Debug teAppDisable {teRet} False";
            teRet = TestEngine.radiotestTxstart(QccHandle, 2441, 50, 255, 0);
            if (teRet != 1) return $"Debug teRadTxCwStart {teRet} False";
            for (int i = 0; i < count; i++)
            {
                ReadRreqoffCW = invoke.CallMethod($"dllname=MT8852B&method=ReadFreqoffCW&Channel={39}&init={flag}");
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

            ReadRreqoffCW = invoke.CallMethod($"dllname=MT8852B&method=ReadFreqoffCW&Channel={39}&init={flag}");
            if (!double.TryParse(ReadRreqoffCW, out OffsetHz))
                return ReadRreqoffCW;
            return (OffsetHz / 1000).ToString();

        }

        string OpenTestEngineDebug514X()
        {
            for (int i = 0; i < 3; i++)
            {
                QccHandle = TestEngine.openTestEngineDebug(1, 0, 4);
                if (QccHandle == 1)
                {
                    break;
                }
                else if (QccHandle > 1)
                {
                    Thread.Sleep(1000);
                    TestEngine.closeTestEngine(QccHandle);
                    Thread.Sleep(2000);
                }
                Thread.Sleep(1000);
            }
            if (QccHandle != 1) return $"Debug {QccHandle} False";
            var ini = $@"{_dllpath}\hydracore_config.sdb:QCC514X_CONFIG";
            //初始化缓冲区
            Thread.Sleep(500);
            var flag = TestEngine.teConfigCacheInit(QccHandle, ini);
            if (flag != 1) return $"Debug teConfigCacheInit {flag} False";
            //读取设备数据到缓冲区
            Thread.Sleep(500);
            flag = TestEngine.teConfigCacheRead(QccHandle, null, 0);
            if (flag != 1) return $"Debug teConfigCacheRead {flag} False";
            return "True";
        }
        string OpenTestEngineDebug(string Str)
        {
            for (int i = 0; i < 3; i++)
            {
                QccHandle = TestEngine.openTestEngineDebug(1, 0, 4);
                if (QccHandle == 1)
                {
                    break;
                }
                else if (QccHandle > 1)
                {
                    Thread.Sleep(1000);
                    TestEngine.closeTestEngine(QccHandle);
                    Thread.Sleep(2000);
                }
                Thread.Sleep(1000);
            }
            if (QccHandle != 1) return $"Debug {QccHandle} False";
            var ini = $@"{_dllpath}\{Str}";
            //初始化缓冲区
            Thread.Sleep(500);
            var flag = TestEngine.teConfigCacheInit(QccHandle, ini);
            if (flag != 1) return $"Debug teConfigCacheInit {flag} False";
            //读取设备数据到缓冲区
            Thread.Sleep(500);
            flag = TestEngine.teConfigCacheRead(QccHandle, null, 0);
            if (flag != 1) return $"Debug teConfigCacheRead {flag} False";
            return "True";
        }

        string CloseTestEngineDebug()
        {
            int rst;
            Console.WriteLine($"closeTestEngine:{QccHandle}");
            if (QccHandle > 0) rst = TestEngine.closeTestEngine(QccHandle);
            Thread.Sleep(1000);
            QccHandle = 0;
            return "True";
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
            Config["FreqTrim"] = $"0x{WriteCapTrim}";

            teRet = TestEngine.teConfigCacheWriteItem(QccHandle, "system3:XtalLoadCapacitance", $"0x{WriteCapTrim}");
            if (teRet != 1)
                return $"Debug teConfigCacheWriteItem {teRet} False";
            Thread.Sleep(100);
            teRet = TestEngine.teConfigCacheWrite(QccHandle, null, 0);
            if (teRet != 1)
                return $"Debug teConfigCacheWrite {teRet} False";
            return $"0x{WriteCapTrim}";

        }
        string WriteConfigCache(string Trim, string key)
        {
            Config["FreqTrim"] = Trim;
            int teRet;
            teRet = TestEngine.teConfigCacheWriteItem(QccHandle, key, Trim);
            if (teRet != 1)
                return $"Debug teConfigCacheWriteItem {teRet} False";
            Thread.Sleep(100);
            teRet = TestEngine.teConfigCacheWrite(QccHandle, null, 0);
            if (teRet != 1)
                return $"Debug teConfigCacheWrite {teRet} False";
            return "True";
        }



    }
}
