using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

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
            try
            {
                switch (cmd[1].ToString())
                {
                    case "NXPRFTestPrepare": return NXPRFTestPrepare().ToString(); // NXP RF测试前置
                    case "NXPRFCalibrationPrepare": return NXPRFCalibrationPrepare().ToString(); // NXP RF 校准前置动作
                    case "NXPCrystalTrim": return NXPCrystalTrim(cmd[2], double.Parse(cmd[3]), Convert.ToBoolean(cmd[4])); // NXP RF 校准方法

                    case "OpenTx2402DTMCH0": return WriteCommand(SendCommand.OpenTx2402DTMCH0, ReturnValue.OpenTxDTMCH0ReturnValue).ToString(); // 开启TX2402 Power测试通道
                    case "OpenTx2440DTMCH19": return WriteCommand(SendCommand.OpenTx2440DTMCH19, ReturnValue.OpenTxDTMCH0ReturnValue).ToString(); // 开启TX2440 Power测试通道
                    case "OpenTx2480DTMCH39": return WriteCommand(SendCommand.OpenTx2480TxDTMCH39, ReturnValue.OpenTxDTMCH0ReturnValue).ToString(); // 开启TX2480 Power测试通道

                    case "OpenRx2402DTMCH0": return WriteCommand(SendCommand.OpenRx2402CH00, ReturnValue.OpenRXCH0ReturnValue).ToString(); // 开启TX2402 Power测试通道
                    case "OpenRx2440DTMCH19": return WriteCommand(SendCommand.OpenRx2440CH19, ReturnValue.OpenRXCH0ReturnValue).ToString(); // 开启TX2440 Power测试通道
                    case "OpenRx2480DTMCH39": return WriteCommand(SendCommand.OpenRx2480CH39, ReturnValue.OpenRXCH0ReturnValue).ToString(); // 开启TX2480 Power测试通道

                    case "CloseDTM": return WriteCommand(SendCommand.CloseDTMCH, ReturnValue.CloseDTMCHReturnValue).ToString(); // 关闭Power 测试通道
                    case "PacketRateTest": return PacketRateTest(cmd[2].ToString()); // 收包率测试
                    case "StartPacketRateTest": return StartPacketRateTest(); // 开始收包率测试
                    case "EndPacketRateTest": return EndPacketRateTest(); // 结束收包率测试
                }
                return "Command Error False";
            }
            catch (Exception ex)
            {
                return $"Error {ex.Message} False";
            }
            finally
            {
                CloseCOM();
            }

        }

        #region 指令区域

        // 发送指令区域
        struct SendCommand
        {
            /// <summary>
            /// 设置产品进入应用模式
            /// </summary>
            public static byte[] SetApplicationMode = { 0x01, 0xA4, 0xFC, 0x01, 0x00 };

            /// <summary>
            /// 设置产品进入桥接模式
            /// </summary>
            public static byte[] SetBridgeMode = { 0x01, 0xA4, 0xFC, 0x01, 0x01 };

            /// <summary>
            /// 设置产品进入CW测试模式
            /// </summary>
            public static byte[] SetCWTestMode = { 0x01, 0xA4, 0xFC, 0x01, 0x02 };

            /// <summary>
            /// DTM 复位
            /// </summary>
            public static byte[] DtmReset = { 0x01, 0x03, 0x0c, 0x00 };

            /// <summary>
            /// 设置物理模式2M
            /// </summary>
            public static byte[] SetPHYMode2M = { 0x01, 0x88, 0xFC, 0x01, 0x01 };

            /// <summary>
            /// 设置物理模式1M
            /// </summary>
            public static byte[] SetPHYMode1M = { 0x01, 0x88, 0xFC, 0x01, 0x00 };

            /// <summary>
            /// 设置Output Power Negative 10dBm
            /// </summary>
            public static byte[] SetOutputPowerNegative10dBm = { 0x01, 0x8A, 0xFC, 0x01, 0x00 };

            /// <summary>
            /// 设置Output Power Negative 4dBm
            /// </summary>
            public static byte[] SetOutputPowerNegative4dBm = { 0x01, 0x8A, 0xFC, 0x01, 0x07 };

            /// <summary>
            /// 启用产品CW Tx CH19通道
            /// </summary>
            public static byte[] EnableCWTxCH19 = { 0x01, 0x89, 0xFC, 0x01, 0x93 };

            /// <summary>
            /// 关闭产品CW Tx CH19通道
            /// </summary>
            public static byte[] DisableCWTxCH19 = { 0x01, 0x89, 0xFC, 0x01, 0x13 };

            /// <summary>
            /// 开启TX 2402测试通道
            /// </summary>
            public static byte[] OpenTx2402DTMCH0 = { 0x01, 0x1E, 0x20, 0x03, 0x00, 0x25, 0x02 };

            /// <summary>
            /// 开启TX 2440测试通道
            /// </summary>
            public static byte[] OpenTx2440DTMCH19 = { 0x01, 0x1E, 0x20, 0x03, 0x13, 0x25, 0x02 };

            /// <summary>
            /// 开启TX 2480测试通道
            /// </summary>
            public static byte[] OpenTx2480TxDTMCH39 = { 0x01, 0x1E, 0x20, 0x03, 0x27, 0x25, 0x02 };

            /// <summary>
            /// 关闭测试通道
            /// </summary>
            public static byte[] CloseDTMCH = { 0x01, 0x1F, 0x20, 0x00 };

            /// <summary>
            /// 开启RX2402测试通道
            /// </summary>
            public static byte[] OpenRx2402CH00 = { 0x01, 0x1D, 0x20, 0x01, 0x00 };

            /// <summary>
            /// 开启RX2440测试通道
            /// </summary>
            public static byte[] OpenRx2440CH19 = { 0x01, 0x1D, 0x20, 0x01, 0x13 };

            /// <summary>
            /// 开启RX2480测试通道
            /// </summary>
            public static byte[] OpenRx2480CH39 = { 0x01, 0x1D, 0x20, 0x01, 0x27 };

            /// <summary>
            /// 重置产品
            /// </summary>
            public static byte[] Reset = { 0x01, 0x98, 0xFC, 0x00 };

            public static byte[] GetDongleXtalCapsIn = { 0x01, 0x9F, 0xFC, 0x01, 0x00 };
            public static byte[] GetDongleXtalCapsOut = { 0x01, 0x9F, 0xFC, 0x01, 0x01 };
            public static byte[] SetDongleXtalCapsIn = { 0x01, 0xA0, 0xFC, 0x03, 0x00, 0x1F, 0x00 };
            public static byte[] SetDongleXtalCapsOut = { 0x01, 0xA0, 0xFC, 0x03, 0x01, 0x1F, 0x00 };
            public static byte[] GetHeadsetXtalCapsIn = { 0x01, 0xA1, 0xFC, 0x01, 0x00 };
            public static byte[] GetHeadsetXtalCapsOut = { 0x01, 0xA1, 0xFC, 0x01, 0x01 };
            public static byte[] SetHeadsetXtalCapsIn = { 0x01, 0xA2, 0xFC, 0x03, 0x00, 0x1F, 0x00 };
            public static byte[] SetHeadsetXtalCapsOut = { 0x01, 0xA2, 0xFC, 0x03, 0x01, 0x1F, 0x00 };
        }

        // 指令返回值区域
        struct ReturnValue
        {
            /// <summary>
            /// 设置连接模式返回值
            /// </summary>
            public static string SetBridgeModeReturnValue = "04 0E 04 01 A4 FC 00";

            /// <summary>
            /// DTM重置返回值
            /// </summary>
            public static string DtmResetReturnValue = "04 0E 04 01 03 0C 00";

            /// <summary>
            /// 设置物理模式返回值
            /// </summary>
            public static string SetPHYModeReturnValue = "04 0E 04 01 88 FC 00";

            /// <summary>
            /// 设置Set Output Power返回值
            /// </summary>
            public static string SetOutputPowerReturnValue = "04 0E 04 01 8A FC 00";

            /// <summary>
            /// CW TX CH19开关返回值
            /// </summary>
            public static string CWTxCH19ReturnValue = "04 0E 04 01 89 FC 00";

            /// <summary>
            /// 修改校准值返回值
            /// </summary>
            public static string ConfigXtalCapsReturnValue = "04 0E 04 01 92 FC 00";

            /// <summary>
            /// 开启TX测试通道返回值
            /// </summary>
            public static string OpenTxDTMCH0ReturnValue = "04 0E 04 01 1E 20 00";

            /// <summary>
            /// 关闭测试通道返回值
            /// </summary>
            public static string CloseDTMCHReturnValue = "04 0E 06 01 1F 20 00";

            /// <summary>
            /// 开启RX测试通道返回值
            /// </summary>
            public static string OpenRXCH0ReturnValue = "04 0E 04 01 1D 20 00";

            /// <summary>
            /// 重置指令返回值
            /// </summary>
            public static string ResetReturnValue = "04 0E 04 01 98 FC 00";

            public static string GetDongleXtalCapsInReturnValue = "04 0E 07 01 9F FC 00 00 29 00";
            public static string GetDongleXtalCapsOutReturnValue = "04 0E 07 01 9F FC 00 01 29 00";
            public static string SetDongleXtalCapsInReturnValue = "04 0E 05 01 A0 FC 00 00";
            public static string SetDongleXtalCapsOutReturnValue = "04 0E 05 01 A0 FC 00 01";
            public static string GetHeadsetXtalCapsInReturnValue = "04 0E 07 01 A1 FC 00 00 24 00";
            public static string GetHeadsetXtalCapsOutReturnValue = "04 0E 07 01 A1 FC 00 01 24 00";
            public static string SetHeadsetXtalCapsInReturnValue = "04 0E 05 01 A2 FC 00 00";
            public static string SetHeadsetXtalCapsOutReturnValue = "04 0E 05 01 A2 FC 00 01";


        }
        #endregion

        #region 方法区域
        private static SerialPort RFSerialPort = new SerialPort();

        #region 串口通用方法区域
        /// <summary>
        /// 设置串口参数并打开串口
        /// </summary>
        /// <returns></returns>
        public static bool ConnectPort()
        {
            bool result = false;
            try
            {
                RFSerialPort.PortName = keyValues["NXP_RF_Comport"].ToString();
                RFSerialPort.BaudRate = 9600;
                RFSerialPort.Parity = Parity.None;
                RFSerialPort.DataBits = 8;
                RFSerialPort.StopBits = StopBits.One;
                RFSerialPort.RtsEnable = true;
                RFSerialPort.DtrEnable = true;
                RFSerialPort.WriteTimeout = 2000;
                RFSerialPort.ReadTimeout = 2000;
                RFSerialPort.Open();
                result = true;
            }
            catch
            {
            }
            return result;
        }

        /// <summary>
        /// 关闭COM口
        /// </summary>
        /// <returns></returns>
        bool CloseCOM()
        {
            if (RFSerialPort.IsOpen)
            {
                RFSerialPort.Close();
                RFSerialPort.Dispose();
            }
            return true;
        }

        #region 下指令通用方法
        public static string Write(byte[] command)
        {
            try
            {
                RFSerialPort.Write(command, 0, command.Length);
                Thread.Sleep(100);
                int bytesToRead = RFSerialPort.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                RFSerialPort.Read(buffer, 0, buffer.Length);
                string rs = "";
                foreach (var i in buffer)
                {
                    rs += i.ToString("X2") + " ";
                }
                return rs;
            }
            catch
            {
                return "False";
            }
        }


        bool WriteCommand(byte[] command, string ReturnValue)
        {
            try
            {
                if (!RFSerialPort.IsOpen)
                {
                    if (!ConnectPort()) return false;
                }
                for (int i = 0; i < 5; i++)
                {
                    if (Write(command).Trim().Contains(ReturnValue)) return true;
                    Thread.Sleep(200);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        #endregion
        #endregion

        #region NXP RF测试前置动作
        /// <summary>
        /// NXP RF测试前置动作
        /// </summary>
        /// <returns></returns>
        string NXPRFTestPrepare()
        {
            try
            {
                if (!RFSerialPort.IsOpen)
                {
                    if (!ConnectPort()) return "FalseCOM";
                }
                // 产品进入桥接模式
                if (!WriteCommand(SendCommand.SetBridgeMode, ReturnValue.SetBridgeModeReturnValue)) return "FalseSetBridgeMode";
                Thread.Sleep(200);
                // 产品DTM复位
                if (!WriteCommand(SendCommand.DtmReset, ReturnValue.DtmResetReturnValue)) return "FalseDtmReset";
                Thread.Sleep(200);
                // 设置物理模式2M
                if (!WriteCommand(SendCommand.SetPHYMode2M, ReturnValue.SetPHYModeReturnValue)) return "FalseSetPHYMode";
                Thread.Sleep(200);
                // 设置Output Power Negative 4dBm
                if (!WriteCommand(SendCommand.SetOutputPowerNegative4dBm, ReturnValue.SetOutputPowerReturnValue)) return "FalseSetOutputPower";
                Thread.Sleep(200);
                return "True";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}False";
            }
        }
        #endregion

        #region Crystal Trim(RF校准区域)
        /// <summary>
        /// NXP RF校准前置动作
        /// </summary>
        /// <returns></returns>
        string NXPRFCalibrationPrepare()
        {
            try
            {
                string rs = NXPRFTestPrepare();
                if (!rs.Equals("True")) return rs;
                // 开启CW TX CH19通道
                if (!WriteCommand(SendCommand.EnableCWTxCH19, ReturnValue.CWTxCH19ReturnValue)) return "FalseEnableCWTxCH19";
                Thread.Sleep(200);
                return "True";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}False";
            }
        }
        /// <summary>
        /// 记录校准值
        /// </summary>
        static int Trim = 0;

        /// <summary>
        /// 频偏校准方法
        /// </summary>
        /// <param name="FrequencyOffsetValue">传入当前频偏值</param>
        /// <returns></returns>
        string NXPCrystalTrim(double FrequencyOffsetValue)
        {

            try
            {
                if (!RFSerialPort.IsOpen)
                {
                    if (!ConnectPort()) return "FalseCOM";
                }
                // 当频偏值大于0时，第一次 a = FrequencyOffsetValue/5,第二次则为a=a+FrequencyOffsetValue/5
                // 当频偏值小于0时则第一次为a = |FrequencyOffsetValue|/5,第二次则为a=a-|FrequencyOffsetValue|/5
                if (FrequencyOffsetValue >= -2 && FrequencyOffsetValue <= 2)
                {
                    int trim = Trim;
                    Trim = 0;
                    if (!WriteCommand(SendCommand.DisableCWTxCH19, ReturnValue.CWTxCH19ReturnValue)) return "FalseDisableCWTxCH19";
                    return Convert.ToString(trim, 16);
                }
                int value = Convert.ToInt32(Math.Abs(FrequencyOffsetValue) / 5);
                if (value == 0) value = 1;
                if (FrequencyOffsetValue < 0)
                {
                    if (value == 0) value = 1;
                    if (Trim == 0)
                    {
                        Trim = value;
                        return "False";
                    }
                    Trim = Trim - value;
                }
                else
                {
                    if (Trim == 0)
                    {
                        Trim = value;
                        return "False";
                    }
                    Trim = Trim + value;
                }
                byte[] command = { 0x01, 0x92, 0xFC, 0x02, Convert.ToByte(Trim.ToString(), 16), Convert.ToByte(Trim.ToString(), 16) };
                WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
                return "False";
            }
            catch
            {
                return "False";
            }
        }

        /// <summary>
        /// 记录上一次的频偏值
        /// </summary>
        static double OldFrequencyOffsetValue = 0;

        /// <summary>
        /// 记录上一次的频偏状态(大于0为true,小于0为false)
        /// </summary>
        static bool result;

        /// <summary>
        /// 频偏校准方法
        /// </summary>
        /// <param name="FrequencyOffsetValue">传入当前频偏值</param>
        /// <param name="flag">传入当前flag(第一次传true,第二次传false)</param>
        /// <returns></returns>
        string NXPCrystalTrim(string NXPTrim, double FrequencyOffsetValue, bool flag)
        {
            try
            {
                if (flag)  Trim = Convert.ToInt32(NXPTrim);
                byte[] command = new byte[] { };
                if (!RFSerialPort.IsOpen)
                {
                    if (!ConnectPort()) return "FalseCOM";
                }
                if (FrequencyOffsetValue < 0)
                {
                    if (!flag && result && OldFrequencyOffsetValue != 0)
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
                        command = new byte[] { 0x01, 0x92, 0xFC, 0x02, Convert.ToByte(Trim), Convert.ToByte(Trim) };
                        WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
                        Thread.Sleep(300);
                        Trim = 0;
                        OldFrequencyOffsetValue = 0;
                        //if (!WriteCommand(SendCommand.DisableCWTxCH19, ReturnValue.CWTxCH19ReturnValue)) return "FalseDisableCWTxCH19";
                        return Convert.ToString(trim);
                    }
                    result = false;
                    if (!flag) Trim--;
                }
                else
                {
                    if (!flag && !result && OldFrequencyOffsetValue != 0)
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
                        command = new byte[] { 0x01, 0x92, 0xFC, 0x02, Convert.ToByte(Trim), Convert.ToByte(Trim) };
                        WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
                        Thread.Sleep(300);
                        Trim = 0;
                        OldFrequencyOffsetValue = 0;
                        //if (!WriteCommand(SendCommand.DisableCWTxCH19, ReturnValue.CWTxCH19ReturnValue)) return "FalseDisableCWTxCH19";
                        return Convert.ToString(trim);
                    }
                    result = true;

                    if (!flag) Trim++;
                }
                if (!flag) OldFrequencyOffsetValue = FrequencyOffsetValue;
                command = new byte[] { 0x01, 0x92, 0xFC, 0x02, Convert.ToByte(Trim ), Convert.ToByte(Trim ) };
                WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
                Thread.Sleep(300);
                return $"{Trim} False";
            }
            catch (Exception ex)
            {
                return $"Error {ex.Message} False";
            }
        }
        #endregion

        #region 收包率测试
        string PacketRateTest(string time)
        {
            try
            {
                if (!RFSerialPort.IsOpen)
                {
                    if (!ConnectPort()) return "FalseCOM";
                }
                if (!WriteCommand(SendCommand.SetPHYMode2M, ReturnValue.SetPHYModeReturnValue)) return "FalseSetPHYMode2M";

                if (!WriteCommand(SendCommand.OpenRx2440CH19, ReturnValue.OpenRXCH0ReturnValue)) return "FalseOpenRx2440CH19";
                Thread.Sleep(Convert.ToInt32(time));
                for (int j = 0; j < 5; j++)
                {
                    string rs = Write(SendCommand.CloseDTMCH).Trim();
                    if (rs.Contains(ReturnValue.CloseDTMCHReturnValue))
                    {
                        string[] values = rs.Split(' ');
                        int result = Convert.ToInt32(values[values.Length - 1].PadLeft(2, '0') + values[values.Length - 2].PadLeft(2, '0'), 16);
                        return Math.Round(Convert.ToDouble(result.ToString()) / 10, 2).ToString();
                    }
                    Thread.Sleep(200);
                }
                return "False";
            }
            catch
            {
                return "False";
            }


        }

        /// <summary>
        /// 开始收包率测试
        /// </summary>
        /// <returns></returns>
        string StartPacketRateTest()
        {
            try
            {
                if (!RFSerialPort.IsOpen)
                {
                    if (!ConnectPort()) return "FalseCOM";
                }
                if (!WriteCommand(SendCommand.SetPHYMode2M, ReturnValue.SetPHYModeReturnValue)) return "FalseSetPHYMode2M";

                if (!WriteCommand(SendCommand.OpenRx2440CH19, ReturnValue.OpenRXCH0ReturnValue)) return "FalseOpenRx2440CH19";
                return "True";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}False";
            }
        }

        /// <summary>
        /// 结束并返回结果
        /// </summary>
        /// <returns></returns>
        string EndPacketRateTest()
        {
            try
            {
                if (!RFSerialPort.IsOpen)
                {
                    if (!ConnectPort()) return "FalseCOM";
                }
                string rs = "";
                for (int j = 0; j < 5; j++)
                {
                    rs = Write(SendCommand.CloseDTMCH).Trim();
                    if (rs.Contains(ReturnValue.CloseDTMCHReturnValue))
                    {
                        string[] values = rs.Split(' ');
                        int result = Convert.ToInt32(values[values.Length - 1].PadLeft(2, '0') + values[values.Length - 2].PadLeft(2, '0'), 16);
                        return Math.Round(Convert.ToDouble(result.ToString()) / 10, 2).ToString();

                    }
                    Thread.Sleep(200);
                }
                return $"{rs}False";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}False";
            }
        }
        #endregion

        #endregion

        #region 接口方法
        public static Dictionary<string, object> keyValues = new Dictionary<string, object> { };
        public object Interface(Dictionary<string, object> keys) => keyValues = keys;


        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：NXPRF";
            string dllfunction = "Dll功能说明 ：NXP芯片RF测试和RF校准";
            string dllHistoryVersion = "历史Dll版本：0.0.0.0";
            string dllVersion = "当前Dll版本：0.0.1.1";
            string dllChangeInfo = "Dll改动信息：";
            string[] info = { dllname, dllfunction, dllHistoryVersion, dllVersion, dllChangeInfo };

            return info;
        }
        #endregion

    }

}
