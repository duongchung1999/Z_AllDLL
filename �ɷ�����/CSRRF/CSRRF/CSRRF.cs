using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TestEngineAPI;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {

        public string Run(object[] Command)
        {
            List<string> cmd = new List<string>();

            foreach (var item in Command)
            {
                if (item.GetType() != typeof(string)) continue;
                cmd = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < cmd.Count; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            switch (cmd[1].ToString())
            {
                case "openTestEngineSpiTrans": return openTestEngineSpiTrans(ref CsrHandle);
                case "closeTestEngine": return closeTestEngine(ref CsrHandle);
                case "psReadBdAddr": return psReadBdAddr();//读取BD号
                case "CsrCrystalTrim": return CsrCrystalTrim();//RF校准
                case "radiotestTxstart": return radiotestTxstart(ushort.Parse(cmd[1]));//释放RF信号
                case "psReadXtalOffset": return psReadXtalOffset();//读取校准值
                case "ColdReset": return ColdReset();//重置产品
                case "WriteTimeBD": return WriteTimeBD();//写入时间进制的BD码
                case "IntoDut": return IntoDut();//进入DUT测试模式
            }
            return "Command Error False";

        }
        uint CsrHandle;
        static object obj_Lock = new object();
        /// <summary>
        /// 打开装置
        /// </summary>
        /// <param name="CsrHandle"></param>
        /// <returns></returns>
        static string openTestEngineSpiTrans(ref uint CsrHandle)
        {
            lock (obj_Lock)
            {
                //查询SPI
                //ushort maxLen = 256;
                //ushort count = 0;
                //StringBuilder portsStr = new StringBuilder(maxLen);
                //StringBuilder transStr = new StringBuilder(maxLen);
                //int i = TestEngine.teGetAvailableSpiPorts(out maxLen, portsStr, transStr, out ushort cout);
                if (CsrHandle != 0)
                {
                    TestEngine.closeTestEngine(CsrHandle);
                    CsrHandle = 0;
                    Thread.Sleep(1500);
                }
                CsrHandle = TestEngine.openTestEngineSpiTrans("SPITRANS=USB SPIPORT=0", 0);
                if (CsrHandle > 0) return $"{CsrHandle}";
                return $"{CsrHandle} False";
            }

        }
        /// <summary>
        /// 关闭装置
        /// </summary>
        /// <param name="CsrHandle"></param>
        /// <returns></returns>
        static string closeTestEngine(ref uint CsrHandle)
        {
            int result = TestEngine.closeTestEngine(CsrHandle);
            if (result != 1) return $"{result} closeTestEngine False";
            CsrHandle = 0;
            return "True";
        }
        /// <summary>
        /// 只读BD号
        /// </summary>
        /// <returns></returns>
        string psReadBdAddr()
        {
            var result = TestEngine.psReadBdAddr(CsrHandle, out uint lap, out byte uap, out ushort nap);
            string BdAddress = string.Format("{0:X4}{1:X2}{2:X6}", nap, uap, lap);
            return result == 1 ? BdAddress : $"{result} psReadBdAddr False";
        }
        /// <summary>
        /// 校准
        /// </summary>
        /// <returns></returns>
        string CsrCrystalTrim()
        {
            try
            {
                int sleep = 150;
                double freq = 2441;
                int result;
                string reStr;
                string ReadRreqoffCW = "";
                bool flag = true;
                //释放信号
                Thread.Sleep(sleep);
                reStr = radiotestTxstart((ushort)freq);
                WriteLog("1、释放信号成功");
                if (reStr != "True") return reStr;
                double OffsetHz = 0;
                for (int i = 0; i < 3; i++)
                {

                    ReadRreqoffCW = CallMethod($"dllname=RT550&method=ReadFreqoffCW&Channel={39}&init={flag}");
                    WriteLog($"2、第{i}次读取频偏{ReadRreqoffCW}");
                    if (double.TryParse(ReadRreqoffCW, out OffsetHz))
                    {
                        flag = false;
                        break;
                    }
                    if (ReadRreqoffCW.Contains("False")) return ReadRreqoffCW;
                    Thread.Sleep(500);
                }
                //计算校准值
                double actualFreqMhz = (OffsetHz / 1000 / 1000) + freq;
                Thread.Sleep(sleep);
                result = TestEngine.radiotestCalcXtalOffset(freq, actualFreqMhz, out short trim);
                WriteLog($"3、radiotestCalcXtalOffset：{result}");

                if (result != 1) return $"{result} radiotestCalcXtalOffset False";
                //读取校准值
                Thread.Sleep(sleep);
                reStr = psReadXtalOffset();
                WriteLog($"4、读校准值psReadXtalOffset：{result}");

                if (reStr.Contains("False")) return reStr;
                //将需要校准的和原本校准的值相加写入
                trim = (short)(trim + Convert.ToInt16(reStr));
                //写入校准值
                WriteLog($"5、写校准值psWriteXtalOffset：{trim}");

                Thread.Sleep(sleep);
                result = TestEngine.psWriteXtalOffset(CsrHandle, trim);
                WriteLog($"6、写好了：{result}");

                if (result != 1) return $"{result} psWriteXtalOffset False";
                //重置设备
                Thread.Sleep(sleep);
                result = TestEngine.bccmdSetColdReset(CsrHandle, 2000);
                WriteLog($"7、重置：{result}");

                if (result != 1) return $"{result} bccmdSetWarmReset False";
                //关闭指针
                Thread.Sleep(sleep);
                reStr = closeTestEngine(ref CsrHandle);
                WriteLog($"8、关闭：{reStr}");

                if (reStr != "True") return reStr;
                Thread.Sleep(1000);
                //打开设备
                reStr = openTestEngineSpiTrans(ref CsrHandle);
                WriteLog($"9、打开：{reStr}");
                if (reStr.Contains("False")) return reStr;
                Thread.Sleep(sleep);
                reStr = radiotestTxstart((ushort)freq);
                WriteLog($"10、释放信号：{reStr}");

                if (reStr != "True") return reStr;
                for (int i = 0; i < 3; i++)
                {
                    WriteLog($"11、第{i}次读取频偏{ReadRreqoffCW}");


                    ReadRreqoffCW = CallMethod($"dllname=RT550&method=ReadFreqoffCW&Channel={39}&init={flag}");
                    if (double.TryParse(ReadRreqoffCW, out OffsetHz))
                    {
                        return $"{OffsetHz / 1000}";

                    }
                    Thread.Sleep(500);
                }
                return $"{ReadRreqoffCW} False";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CSR类 CsrCrystalTrim 方法报错了\r\n{ex}");
                return $"{ex.Message} False";
            }

        }
        void WriteLog(string log)
        {
            //File.WriteAllText($".\\LOG\\looog.txt", $"{DateTime.Now}::::{log}");
            //Thread.Sleep(100);
        }
        /// <summary>
        /// 释放信号Freq
        /// </summary>
        /// <param name="Khz"></param>
        /// <returns></returns>
        string radiotestTxstart(ushort Khz)
        {
            int result = TestEngine.radiotestTxstart(CsrHandle, Khz, 50, 255, 0);
            Thread.Sleep(1500);
            if (result != 1) return $"{result} radiotestTxstart False";
            return "True";
        }
        /// <summary>
        /// 读取校准值
        /// </summary>
        /// <returns></returns>
        string psReadXtalOffset()
        {
            //读取校准值
            int result = TestEngine.psReadXtalOffset(CsrHandle, out short offset);
            if (result != 1) return $"{result} psReadXtalOffset False";
            return $"{offset}";
        }
        /// <summary>
        /// 重置该设备
        /// </summary>
        /// <returns></returns>
        string ColdReset()
        {
            int result = TestEngine.bccmdSetColdReset(CsrHandle, 1500);
            return result == 1 ? "True" : $"{result} bccmdSetWarmReset False";
        }
        /// <summary>
        /// 写入时间BD号
        /// </summary>
        /// <returns></returns>
        string WriteTimeBD()
        {
            string BDAddress = DateTime.Now.ToString("yyMMddHHmmss").PadLeft(12, '0');
            if (BDAddress.Length != 12) return $"随机BD长度异常： {BDAddress} False";
            if (Config["SN"].ToString().Contains("TE_BZP")) return "标准品";
            uint nap = UInt32.Parse(BDAddress.Substring(0, 4), System.Globalization.NumberStyles.HexNumber);
            uint uap = UInt32.Parse(BDAddress.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            uint lap = UInt32.Parse(BDAddress.Substring(6, 6), System.Globalization.NumberStyles.HexNumber);
            int result = TestEngine.psWriteBdAddr(CsrHandle, lap, uap, nap);
            if (result == 1) return BDAddress;
            return $"result： {result} False";
        }

        string IntoDut()
        {
            int result;
            result = TestEngine.bccmdEnableDeviceConnect(CsrHandle);
            if (result != 1) return $"{result} bccmdEnableDeviceConnect False";
            result = TestEngine.bccmdEnableDeviceUnderTestMode(CsrHandle);
            if (result != 1) return $"{result} bccmdEnableDeviceUnderTestMode False";
            return "True";
        }




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
                var mi = dllType[keys].GetMethod("Interface");
                mi.Invoke(MagicClassObject[keys], new object[] { Config });//方法有参数时，需要把null替换为参数的集合
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"调用{keys}.Dll失败 \r\n{ex}");
                dllType[keys] = null;
                MagicClassObject[keys] = null;
                return false;
            }

        }
        static string CallMethod(string cmd)
        {
            string keys = cmd.Split('&')[0].Split('=')[1];
            if (!LoadDll(keys)) return "Load Dll False";
            try
            {
                var mi = dllType[keys].GetMethod("Run");
                return mi.Invoke(MagicClassObject[keys], new object[] { new object[] { cmd } }).ToString();//方法有参数时，需要把null替换为参数的集合
            }
            catch (Exception ex)
            {
                MessageBox.Show($"调用{keys}方法失败{"Run"}\r\n{ex}");
                return "Invoke False";
            }
        }
        #region 接口方法
        public static Dictionary<string, object> Config = new Dictionary<string, object> { };
        public object Interface(Dictionary<string, object> keys) => Config = keys;
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：CSRRF";
            string dllfunction = "Dll功能说明 ：CSRRF芯片RF测试和RF校准";
            string dllHistoryVersion = "历史Dll版本：0.0.0.0";
            string dllVersion = "当前Dll版本：0.0.1.1";
            string dllChangeInfo = "Dll改动信息：";
            string[] info = { dllname, dllfunction, dllHistoryVersion, dllVersion, dllChangeInfo };

            return info;
        }


        #endregion
    }
}
