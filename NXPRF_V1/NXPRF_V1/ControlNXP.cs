using NXPRF_V1.API;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NXPRF_V1
{
    public class ControlNXP
    {

        public Invoke invoke = null;
        public Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        public string LowLimit;
        public string UpLimit;
        SerialPort RFSerialPort = new SerialPort();
        public string PortName = "";

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



        #region 串口通用方法区域
        /// <summary>
        /// 设置串口参数并打开串口
        /// </summary>
        /// <returns></returns>
        string ConnectPort()
        {
            try
            {
                if (RFSerialPort.IsOpen)
                    return "True";
                RFSerialPort.PortName = PortName;
                RFSerialPort.BaudRate = 9600;
                RFSerialPort.Parity = Parity.None;
                RFSerialPort.DataBits = 8;
                RFSerialPort.StopBits = StopBits.One;
                RFSerialPort.RtsEnable = true;
                RFSerialPort.DtrEnable = true;
                RFSerialPort.WriteTimeout = 1000;
                RFSerialPort.ReadTimeout = 1000;
                RFSerialPort.Open();
                return "True";
            }
            catch (Exception ex)
            {
                return $"{PortName} {ex.Message} False";
            }
        }

        /// <summary>
        /// 关闭COM口
        /// </summary>
        /// <returns></returns>
        public bool CloseCOM()
        {
            if (RFSerialPort.IsOpen)
                RFSerialPort.Close();
            RFSerialPort.Dispose();

            return true;
        }

        #region 下指令通用方法
        /// <summary>
        /// 下指令并循环读取返回值方法
        /// </summary>
        /// <param name="command">指令</param>
        /// <returns></returns>
        public string Write(byte[] command)
        {
            try
            {
                // 使用串口下指令
                RFSerialPort.Write(command, 0, command.Length);
                Thread.Sleep(100);
                string rs = "";
                // 循环读取返回值
                for (int j = 0; j < 30; j++)
                {
                    int bytesToRead = RFSerialPort.BytesToRead;
                    byte[] buffer = new byte[bytesToRead];
                    RFSerialPort.Read(buffer, 0, buffer.Length);
                    foreach (var i in buffer)
                    {
                        rs += i.ToString("X2") + " ";
                    }
                    if (rs.Length > 0) break;
                    Thread.Sleep(200);
                }
                return rs.Trim();
            }
            catch (Exception ex)
            {
                return $"{ex.Message} False";
            }
        }


        string WriteCommand(byte[] command, string ReturnValue)
        {
            string Result = "";
            try
            {
                Result = ConnectPort();
                if (Result.Contains("False"))
                    return Result;
                Result = Write(command);
                if (Result.Contains(ReturnValue))
                    return "True";
                return $"{Result} False";
            }
            catch (Exception ex)
            {

                return $"{Result} {ex.Message} False";

            }
        }
        #endregion
        #endregion

        #region Crystal Trim(RF校准区域)
        /// <summary>
        /// NXP RF校准前置动作
        /// </summary>
        /// <returns></returns>
        public string NXPRFCalibrationPrepare()
        {
            try
            {
                string rs = NXPRFTestPrepare();
                if (!rs.Equals("True")) return rs;
                // 开启CW TX CH19通道
                rs = WriteCommand(SendCommand.EnableCWTxCH19, ReturnValue.CWTxCH19ReturnValue);
                if (rs.Contains("False"))
                    return $"EnableCWTxCH19 {rs}";
                Thread.Sleep(200);
                return "True";
            }
            catch (Exception ex)
            {
                return $"{ex.Message} False";
            }
        }
        public string Trim = "null";

        public string 调试RF()
        {
            int Trim = 1;
            PortName = "COM13";
            string NXPResult = ConnectPort();
            if (NXPResult.Contains("False"))
                return $"Connevt Port {NXPResult} False";
            //进入RF测试模式信号发射模式
            NXPResult = NXPRFCalibrationPrepare();
            if (NXPResult.Contains("False"))
                return $"CalibrationPrepare {NXPResult} False";
            while (true)
            {
                string READsTR = Console.ReadLine();

                if (READsTR == "ex")
                {
                    CloseDTM();
                    CloseCOM();
                    return "";
                }
                byte[] command = new byte[] { 0x01, 0x92, 0xFC, 0x02, Convert.ToByte(READsTR), Convert.ToByte(READsTR) };

                //第一次测试直接写个默认值进去
                NXPResult = WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
                if (NXPResult.Contains("False"))
                    return $"{NXPResult} Write InitTrim False";

            }



            return "";
        }

        public string NXPCrystalTrim_MT8852B(string InitTrim, int TrimCount)
        {
            string ReadRreqoffCW;
            string NXPResult = "";
            bool flag = true;
            bool result = false;
            double OffsetHz = 0;
            double OldOffsetHz = 0;
            int countTest = 0;
            int Trim = Convert.ToInt32(InitTrim);
            byte[] command = new byte[] { 0x01, 0x92, 0xFC, 0x02, Convert.ToByte(Trim), Convert.ToByte(Trim) };
            try
            {
                //打开串口
                NXPResult = ConnectPort();
                if (NXPResult.Contains("False"))
                    return $"Connevt Port {NXPResult} False";

                //进入RF测试模式信号发射模式
                NXPResult = NXPRFCalibrationPrepare();
                if (NXPResult.Contains("False"))
                    return $"CalibrationPrepare {NXPResult} False";
                //第一次测试直接写个默认值进去
                NXPResult = WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
                if (NXPResult.Contains("False"))
                    return $"{NXPResult} Write InitTrim False";


                for (int i = 0; i < TrimCount; i++)
                {
                    //读取频偏
                    ReadRreqoffCW = invoke.CallMethod($"dllname=MT8852B_V1&method=CWReadFreqOffset&Channel={38}&init={flag}", OnceConfig);
                    for (int c = 0; c < 3; c++)
                    {
                        //频偏转换成数值类型
                        if (double.TryParse(ReadRreqoffCW, out OffsetHz))
                            break;
                        if (ReadRreqoffCW.Contains("False")) return ReadRreqoffCW;
                    }
                    countTest++;
                    //当频偏大于0的时候
                    if (OffsetHz < 0)
                    {
                        if (!flag && result)
                        {
                            Trim = Math.Abs(OffsetHz) > Math.Abs(OldOffsetHz) ? --Trim : Trim;
                            command[4] = Convert.ToByte(Trim);
                            command[5] = Convert.ToByte(Trim);
                            if (countTest >= 3)
                            {
                                WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
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
                            command[4] = Convert.ToByte(Trim);
                            command[5] = Convert.ToByte(Trim);
                            if (countTest >= 3)
                            {
                                WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
                                break;
                            }
                        }
                        result = true;
                        if (!flag) Trim++;
                    }
                    OldOffsetHz = OffsetHz;
                    command[4] = Convert.ToByte(Trim);
                    command[5] = Convert.ToByte(Trim);
                    WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
                    flag = false;
                    Thread.Sleep(50);
                }
                this.Trim = $"{Trim}";
                ReadRreqoffCW = invoke.CallMethod($"dllname=MT8852B_V1&method=CWReadFreqOffset&Channel={38}&init={flag}", OnceConfig);
                if (!double.TryParse(ReadRreqoffCW, out OffsetHz))
                    return ReadRreqoffCW;
                return (OffsetHz / 1000).ToString();
            }
            catch (Exception ex)
            {
                return $"Error {ex.Message} False";
            }
        }

        #region MyRegion

        //public string NXPCrystalTrim_RT550(string InitTrim, int TrimCount)
        //{
        //    string ReadRreqoffCW;
        //    string NXPResult = "";
        //    bool flag = true;
        //    bool result = false;
        //    double OffsetHz = 0;
        //    double OldOffsetHz = 0;
        //    int countTest = 0;
        //    int Trim = Convert.ToInt32(InitTrim);
        //    byte[] command = new byte[] { 0x01, 0x92, 0xFC, 0x02, Convert.ToByte(Trim), Convert.ToByte(Trim) };
        //    try
        //    {
        //        //打开串口
        //        NXPResult = ConnectPort();
        //        if (NXPResult.Contains("False"))
        //            return $"Connevt Port {NXPResult} False";

        //        //进入RF测试模式信号发射模式
        //        NXPResult = NXPRFCalibrationPrepare();
        //        if (NXPResult.Contains("False"))
        //            return $"CalibrationPrepare {NXPResult} False";
        //        //第一次测试直接写个默认值进去
        //        NXPResult = WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
        //        if (NXPResult.Contains("False"))
        //            return $"{NXPResult} Write InitTrim False";
        //        try
        //        {
        //            ReadRreqoffCW = invoke.CallMethod($"dllname=RT550_V1&method=LockRT550", OnceConfig);
        //            for (int i = 0; i < TrimCount; i++)
        //            {
        //                //读取频偏
        //                ReadRreqoffCW = invoke.CallMethod($"dllname=RT550_V1&method=CWReadFreqOffset&Channel={38}&init={flag}", OnceConfig);
        //                for (int c = 0; c < 3; c++)
        //                {
        //                    //频偏转换成数值类型
        //                    if (double.TryParse(ReadRreqoffCW, out OffsetHz))
        //                        break;
        //                    if (ReadRreqoffCW.Contains("False")) return ReadRreqoffCW;
        //                }
        //                Console.WriteLine($"\r\n{OffsetHz}\r\n");
        //                countTest++;
        //                //当频偏大于0的时候
        //                if (OffsetHz < 0)
        //                {
        //                    if (!flag && result)
        //                    {
        //                        Trim = Math.Abs(OffsetHz) > Math.Abs(OldOffsetHz) ? --Trim : Trim;
        //                        command[4] = Convert.ToByte(Trim);
        //                        command[5] = Convert.ToByte(Trim);
        //                        if (countTest >= 3)
        //                        {
        //                            WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
        //                            break;
        //                        }
        //                    }
        //                    result = false;
        //                    if (!flag) Trim--;
        //                }
        //                //当频偏小于0的时候
        //                else
        //                {
        //                    if (!flag && !result)
        //                    {
        //                        Trim = Math.Abs(OffsetHz) > Math.Abs(OldOffsetHz) ? ++Trim : Trim;
        //                        command[4] = Convert.ToByte(Trim);
        //                        command[5] = Convert.ToByte(Trim);
        //                        if (countTest >= 3)
        //                        {
        //                            WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
        //                            break;
        //                        }
        //                    }
        //                    result = true;
        //                    if (!flag) Trim++;
        //                }
        //                OldOffsetHz = OffsetHz;
        //                command[4] = Convert.ToByte(Trim);
        //                command[5] = Convert.ToByte(Trim);
        //                WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
        //                flag = false;
        //                Thread.Sleep(50);
        //            }
        //            this.Trim = $"{Trim}";
        //            ReadRreqoffCW = invoke.CallMethod($"dllname=RT550_V1&method=CWReadFreqOffset&Channel={38}&init={flag}", OnceConfig);
        //            if (!double.TryParse(ReadRreqoffCW, out OffsetHz))
        //                return ReadRreqoffCW;
        //            return (OffsetHz / 1000).ToString();
        //        }
        //        finally
        //        {
        //            invoke.CallMethod($"dllname=RT550_V1&method=StopTesting", OnceConfig);
        //            invoke.CallMethod($"dllname=RT550_V1&method=UnlockRT550", OnceConfig);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"Error {ex.Message} False";
        //    }
        //}

        #endregion


        public string NXPCrystalTrim_RT550(string InitTrim, int TrimCount)
        {
            string ReadRreqoffCW;
            string NXPResult;
            double OffsetHz = -51e9;
            int Trim = Convert.ToInt32(InitTrim);
            bool UpLimitFlag = double.TryParse(this.UpLimit, out double UpLimit); UpLimit *= 1000;
            bool LowLimitFlag = double.TryParse(this.LowLimit, out double LowLimit); LowLimit *= 1000;
            bool LimitFlag = LowLimitFlag && UpLimitFlag;
            if (!LimitFlag)
                return "要设定上下限 False";
            bool XtalFalg = true;
            try
            {
                //打开串口
                NXPResult = ConnectPort();
                if (NXPResult.Contains("False"))
                    return $"Connevt Port {NXPResult} False";

                //进入RF测试模式信号发射模式
                NXPResult = NXPRFCalibrationPrepare();
                if (NXPResult.Contains("False"))
                    return $"CalibrationPrepare {NXPResult} False";
                //第一次测试直接写个默认值进去
                byte[] command = new byte[] { 0x01, 0x92, 0xFC, 0x02, Convert.ToByte(Trim), Convert.ToByte(Trim) };
                NXPResult = WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
                if (NXPResult.Contains("False"))
                    return $"{NXPResult} Write InitTrim False";
                try
                {
                    ReadRreqoffCW = invoke.CallMethod($"dllname=RT550_V1&method=LockRT550", OnceConfig);


                    for (int i = 0; i < TrimCount; i++)
                    {
                        bool initFlag = i == 0;

                        this.Trim = Trim.ToString();
                        if (!RT550ReadFreq(initFlag, out string FreqStr))
                        {
                            return FreqStr;
                        }
                        OffsetHz = double.Parse(FreqStr);
                        //读取到符合limit的频偏直接出去，不用一直校准
                        if (LimitFlag && UpLimit > OffsetHz && OffsetHz > LowLimit)
                            return (OffsetHz / 1000).ToString();

                        XtalCrystalTrim(XtalFalg, UpLimit, LowLimit, ref OffsetHz, ref Trim, out string Info);
                        if (Info.Contains("False"))
                            return Info;
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
            catch (Exception ex)
            {
                return $"Error {ex.Message} False";
            }
        }

        int TrimDiff(double Freq)
        {
            int Diff = 0;
            if (Freq > 0)
            {
                if (Freq > 178)
                {
                    Diff = 14;
                }
                else if (Freq > 138)
                {
                    Diff = 13;
                }
                else if (Freq > 126)
                {
                    Diff = 12;
                }
                else if (Freq > 104)
                {
                    Diff = 11;
                }
                else if (Freq > 84)
                {
                    Diff = 10;
                }
                else if (Freq > 66)
                {
                    Diff = 9;
                }
                else if (Freq > 50)
                {
                    Diff = 8;
                }
                else if (Freq > 22)
                {
                    Diff = 7;
                }
                else if (Freq > 9.7)
                {
                    Diff = 6;
                }
                else if (Freq > 4.0)
                {
                    Diff = 5;
                }
                else
                {
                    Diff = 5;
                }
            }
            else
            {
                if (Freq < -17.7)
                {
                    Diff = 5;
                }
                else if (Freq < -29)
                {
                    Diff = 6;
                }
            }
            return Diff;
        }

        bool RT550ReadFreq(bool InitFlag, out string Freq)
        {

            Freq = invoke.CallMethod($"dllname=RT550_V1&method=CWReadFreqOffset&Channel={38}&init={InitFlag}", OnceConfig);
            for (int c = 0; c < 3; c++)
            {
                if (double.TryParse(Freq, out _))
                    return true;
                if (Freq.Contains("False")) return false;

            }
            return false;
        }
        void XtalCrystalTrim(bool OnceTrim, double Uplimit, double Lowlimit, ref double OffsetHz, ref int Trim, out string Info)
        {
            if (OnceTrim)
            {
                double Diff = TrimDiff(OffsetHz / 1000);
                double offse = OffsetHz / 1000;
                double up = Uplimit / 1000;
                double lo = Lowlimit / 1000;

                for (int i = 0; i < 15; i++)
                {
                    if (offse < 0)
                    {
                        offse += 4.1;
                        Trim--;
                    }
                    else
                    {
                        offse -= Diff;
                        Diff *= 0.90;
                        Trim++;
                    }
                   Console.WriteLine($"Diff:{Diff}" );
                    Console.WriteLine($"offse:{offse}");
                    Console.WriteLine($"Trim:{Trim}");


                    if (up > offse && offse > lo)
                        break;
                }
                byte[] cmd = new byte[] { 0x01, 0x92, 0xFC, 0x02, Convert.ToByte(Trim), Convert.ToByte(Trim) };
                Info = WriteCommand(cmd, ReturnValue.ConfigXtalCapsReturnValue);
                return;

            }
            //当频偏小于0的时候
            if (OffsetHz < 0)
            {
                Trim--;
            }
            //当频偏大于0的时候
            else
            {
                Trim++;
            }
            byte[] command = new byte[] { 0x01, 0x92, 0xFC, 0x02, Convert.ToByte(Trim), Convert.ToByte(Trim) };
            Info = WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
            Thread.Sleep(50);
            return;

        }


        #endregion

        /// <summary>
        /// 开始2440收包率测试
        /// </summary>
        /// <returns></returns>
        string StartPacketRateTest(byte[] SendCommandValue, string ReturnCommandValue)
        {
            try
            {
                string Result;
                Result = ConnectPort();
                if (Result.Contains("False"))
                    return Result;

                Result = WriteCommand(SendCommand.SetPHYMode2M, ReturnValue.SetPHYModeReturnValue);
                if (Result.Contains("False"))
                    return $"SetPHYMode2M {Result}";


                Thread.Sleep(100);
                Result = WriteCommand(SendCommandValue, ReturnCommandValue);
                if (Result.Contains("False"))
                    return $"FalseOpenRx {Result}";
                Thread.Sleep(200);
                return "True";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}False";
            }
        }




        //#####################################  RF信号测试  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        #region NXP RF测试 
        /// <summary>
        /// NXP RF测试前置动作
        /// </summary>
        /// <returns></returns>
        public string NXPRFTestPrepare()
        {
            try
            {
                string Result;
                Result = ConnectPort();
                if (Result.Contains("False"))
                    return Result;

                // 产品进入桥接模式
                Result = WriteCommand(SendCommand.SetBridgeMode, ReturnValue.SetBridgeModeReturnValue);
                if (Result.Contains("False"))
                    return $"SetBridgeMode {Result}";

                Thread.Sleep(100);
                // 产品DTM复位
                Result = WriteCommand(SendCommand.DtmReset, ReturnValue.DtmResetReturnValue);
                if (Result.Contains("False"))
                    return $"DtmReset {Result}";

                Thread.Sleep(100);
                // 设置物理模式2M
                Result = WriteCommand(SendCommand.SetPHYMode2M, ReturnValue.SetPHYModeReturnValue);
                if (Result.Contains("False"))
                    return $"SetPHYMode {Result}";

                Thread.Sleep(100);
                // 设置Output Power Negative 4dBm
                Result = WriteCommand(SendCommand.SetOutputPowerNegative4dBm, ReturnValue.SetOutputPowerReturnValue);
                if (Result.Contains("False"))
                    return $"SetOutputPower {Result}";
                Thread.Sleep(100);
                return "True";
            }
            catch (Exception ex)
            {
                return $"{ex.Message} False";
            }
        }

        public string OpenTx2402DTMCH0()
            => WriteCommand(SendCommand.OpenTx2402DTMCH0, ReturnValue.OpenTxDTMCH0ReturnValue); // 开启TX2402 Power测试通道

        public string OpenTx2440DTMCH19()
            => WriteCommand(SendCommand.OpenTx2440DTMCH19, ReturnValue.OpenTxDTMCH0ReturnValue).ToString(); // 开启TX2440 Power测试通道
        public string OpenTx2480DTMCH39()
            => WriteCommand(SendCommand.OpenTx2480TxDTMCH39, ReturnValue.OpenTxDTMCH0ReturnValue).ToString(); // 开启TX2480 Power测试通道




        public string OpenRx2402DTMCH0()
            => StartPacketRateTest(SendCommand.OpenRx2402CH00, ReturnValue.OpenRXCH0ReturnValue).ToString(); // 开启RX2402 Power测试通道

        public string OpenRx2440DTMCH19()
            => StartPacketRateTest(SendCommand.OpenRx2440CH19, ReturnValue.OpenRXCH0ReturnValue).ToString(); // 开启RX2440 Power测试通道

        public string OpenRx2480DTMCH39()
            => StartPacketRateTest(SendCommand.OpenRx2480CH39, ReturnValue.OpenRXCH0ReturnValue).ToString(); // 开启RX2480 Power测试通道



        /// <summary>
        /// 结束收包率测试并返回结果
        /// </summary>
        /// <returns></returns>
        public string EndPacketRateTest()
        {
            try
            {
                Thread.Sleep(500);
                string Result;
                Result = ConnectPort();

                if (Result.Contains("False"))
                    return Result;

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
        public string CloseDTM()
        {
            try
            {
                string result = WriteCommand(SendCommand.CloseDTMCH, ReturnValue.CloseDTMCHReturnValue);
                Thread.Sleep(100);
                return result;
            }
            catch (Exception ex)
            {
                return $"{ex.Message} False";
            }
        }

        public string Test()
        {
            for (int i = 0; i < 100; i++)
            {
                //byte[] command = new byte[] { 0x01, 0x92, 0xFC, 0x02, Convert.ToByte(i), Convert.ToByte(i) };
                //byte[] cmd
                //Console.WriteLine(i);
                //WriteCommand(command, ReturnValue.ConfigXtalCapsReturnValue);
                Thread.Sleep(200);
                Console.WriteLine(Write(SendCommand.GetDongleXtalCapsIn));
                Console.WriteLine(Write(SendCommand.GetDongleXtalCapsOut));

            }
            return "";

        }



        #endregion





    }
}
