using MerryDllFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SoundCheck_V1.TemplateANC.Expand;

namespace SoundCheck_V1.TemplateANC
{

    public class ANC_CalibrationMethod
    {
        ANC_Calibration_Parameter _Parameter = new ANC_Calibration_Parameter();
        LuxShare_CalibrationData lux_cbd = new LuxShare_CalibrationData();
        Merry_CalibrationDataFB cdb = new Merry_CalibrationDataFB();
        Merry_CalibrationDataFF cdf = new Merry_CalibrationDataFF();
        Func<string> SwitchFBOnly
        {
            get
            {
                return (Func<string>)Config[$"{_Parameter.cbb_CMDList}.SwitchFBOnly"];
            }
        }
        Func<string> SwitchFFOnly
        {
            get
            {
                return (Func<string>)Config[$"{_Parameter.cbb_CMDList}.SwitchFFOnly"];
            }
        }
        Func<string> SwitchHybrid
        {
            get
            {
                return (Func<string>)Config[$"{_Parameter.cbb_CMDList}.SwitchHybrid"];
            }
        }
        Func<string> GetGain
        {
            get
            {
                return (Func<string>)Config[$"{_Parameter.cbb_CMDList}.GetGain"];
            }
        }
        Func<string, string> SetGain
        {
            get
            {
                return (Func<string, string>)Config[$"{_Parameter.cbb_CMDList}.SetGain"];
            }
        }
        Func<string, string> SaveGain
        {
            get
            {
                return (Func<string, string>)Config[$"{_Parameter.cbb_CMDList}.SaveGain"];
            }
        }



        public string ANC_ParameterPath = "";
        public String SequenceDic;
        public Dictionary<string, object> Config;
        public WindowControl fw = new WindowControl();

        public void ReadANCConfig()
        {
            string ConfigPath = ANC_ParameterPath;// $@".\ANC_Parameter.ini";
            INIClass._path = ConfigPath;
            Dictionary<string, string> Section_rb = INIClass.GetSection("RadioButton");
            Dictionary<string, string> Section_nb = INIClass.GetSection("NumericUpDown");
            Dictionary<string, string> Section_cb = INIClass.GetSection("CheckBox");
            Dictionary<string, string> Section_dg = INIClass.GetSection("DataGridView");
            Dictionary<string, string> Section_String = INIClass.GetSection("String");
            Dictionary<string, string> Section_ccb = INIClass.GetSection("ComboBox");


            foreach (var item in _Parameter.GetType().GetFields())
            {
                if (Section_rb.ContainsKey(item.Name))
                {
                    item.SetValue(_Parameter, Section_rb[item.Name].ToString());
                    continue;
                }
                if (Section_cb.ContainsKey(item.Name))
                {
                    bool.TryParse(Section_cb[item.Name], out var value);
                    item.SetValue(_Parameter, value);
                    continue;
                }
                if (Section_nb.ContainsKey(item.Name))
                {
                    double.TryParse(Section_nb[item.Name], out var value);
                    item.SetValue(_Parameter, value.Round());
                    continue;

                }

                if (Section_dg.ContainsKey(item.Name))
                {
                    List<double> values = new List<double>();
                    foreach (var items in Section_dg[item.Name].Split(','))
                        if (double.TryParse(items, out var value))
                            values.Add(value.Round());
                    item.SetValue(_Parameter, values.ToArray());
                    continue;
                }
                if (Section_String.ContainsKey(item.Name))
                {
                    item.SetValue(_Parameter, Section_String[item.Name]);
                    continue;
                }
                if (Section_ccb.ContainsKey(item.Name))
                {
                    item.SetValue(_Parameter, Section_ccb[item.Name]);
                    continue;
                }

            }
            _Parameter.nb_TestMaxCount += 1;
        }

        /// <summary>
        /// 通用校准方法入口
        /// </summary>
        /// <returns></returns>
        public string Calibration()
        {
            string DirePath = $"{SequenceDic}\\lastlog";
            if (Directory.Exists(DirePath))
                Directory.Delete(DirePath, true);
            string FormsName = "Correction or not_";
            //string FormsName_FB = "Correction or not_FB";
            string TestData_FB = $@"{SequenceDic}\FB Test Curves.txt";
            string PassText_FB = $@"{SequenceDic}\FB Test Result.txt";
            //string FormsName_FF = "Correction or not_ANC";
            string TestData_FF = $@"{SequenceDic}\ANC Test Curves.txt";
            string PassText_FF = $@"{SequenceDic}\ANC Test Result.txt";
            string Result = "Note Test False";
            double LFBGain = -100;
            double RFBGain = -100;
            double LFFGain = -100; ;
            double RFFGain = -100;
            string initFail = "";
            WriteGain(SequenceDic, LFBGain, RFBGain, LFFGain, RFFGain, -100, -100);
            switch (_Parameter.gb_CalibrationDetails)
            {

                case Switch_CalibrationDetails.rb_cbtFB://校准FB
                    Result = SingleCalibration(Switch_ANC_Mode.SwitchFBOnly, FormsName, TestData_FB, PassText_FB, out LFBGain, out RFBGain, out LFFGain, out RFFGain);
                    break;
                case Switch_CalibrationDetails.rb_cbtFF://校准FF
                    Result = SingleCalibration(Switch_ANC_Mode.SwitchFFOnly, FormsName, TestData_FF, PassText_FF, out LFBGain, out RFBGain, out LFFGain, out RFFGain);
                    break;

                case Switch_CalibrationDetails.rb_cbtHybrid://校准FB + 混合
                                                            //初始化ANC状态
                    initFail = InitGain(Switch_ANC_Mode.SwitchFBOnly, out LFBGain, out RFBGain, out LFFGain, out RFFGain);
                    if (initFail.ContainsFalse())
                        return initFail;
                    Result = HybridCalibration(true, Switch_ANC_Mode.SwitchFBOnly, FormsName, TestData_FB, PassText_FB, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);
                    if (Result.Contains("False")) break;

                    initFail = SwitchANCModeMethod(Switch_ANC_Mode.SwitchHybrid, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);
                    if (initFail.ContainsFalse())
                        return initFail;
                    Result = HybridCalibration(true, Switch_ANC_Mode.SwitchFFOnly, FormsName, TestData_FF, PassText_FF, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);
                    break;

                case Switch_CalibrationDetails.rb_cbtOffFB_ONFF: //固定FB + 校准混合
                    initFail = InitGain(Switch_ANC_Mode.SwitchFBOnly, out LFBGain, out RFBGain, out LFFGain, out RFFGain);
                    if (initFail.ContainsFalse())
                        return initFail;
                    Result = HybridCalibration(false, Switch_ANC_Mode.SwitchFBOnly, FormsName, TestData_FB, PassText_FB, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);
                    if (Result.Contains("False")) break;
                    initFail = SwitchANCModeMethod(Switch_ANC_Mode.SwitchHybrid, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);
                    if (initFail.ContainsFalse())
                        return initFail;

                    Result = HybridCalibration(true, Switch_ANC_Mode.SwitchFFOnly, FormsName, TestData_FF, PassText_FF, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);


                    break;
                case Switch_CalibrationDetails.rb_cbtOnFB_OffFF://校准FB + 固定FF
                    initFail = InitGain(Switch_ANC_Mode.SwitchFBOnly, out LFBGain, out RFBGain, out LFFGain, out RFFGain);
                    if (initFail.ContainsFalse())
                        return initFail;
                    Result = HybridCalibration(true, Switch_ANC_Mode.SwitchFBOnly, FormsName, TestData_FB, PassText_FB, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);
                    if (Result.Contains("False")) break;
                    initFail = SwitchANCModeMethod(Switch_ANC_Mode.SwitchHybrid, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);
                    if (initFail.ContainsFalse())
                        return initFail;
                    Result = HybridCalibration(false, Switch_ANC_Mode.SwitchFFOnly, FormsName, TestData_FF, PassText_FF, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);
                    break;
                case Switch_CalibrationDetails.rb_cbtOffFB_OffFF://固定FB + 固定FF
                    initFail = InitGain(Switch_ANC_Mode.SwitchFBOnly, out LFBGain, out RFBGain, out LFFGain, out RFFGain);
                    if (initFail.ContainsFalse())
                        return initFail;
                    Result = HybridCalibration(false, Switch_ANC_Mode.SwitchFBOnly, FormsName, TestData_FB, PassText_FB, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);
                    if (Result.Contains("False")) break;
                    initFail = SwitchANCModeMethod(Switch_ANC_Mode.SwitchHybrid, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);
                    if (initFail.ContainsFalse())
                        return initFail;
                    Result = HybridCalibration(false, Switch_ANC_Mode.SwitchFFOnly, FormsName, TestData_FF, PassText_FF, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);
                    break;
                default:
                    break;
            }

            //  当 标准品                                      并且设定标准品写入Gain值                    或者不是标准品      才会写入Gain
            if ((Config["SN"].ToString().Contains("TE_BZP") && _Parameter.cb_TE_BZP_WriteGain) || !Config["SN"].ToString().Contains("TE_BZP"))
            {
                try
                {
                    CMDSaveGain(ref Result, ref LFBGain, ref RFBGain, ref LFFGain, ref RFFGain);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Save\r\n" + ex.ToString());

                    throw;
                }
            }
            else
            {

            }

            return Result;
        }
        void CMDSaveGain(ref string Result, ref double LFBGain, ref double RFBGain, ref double LFFGain, ref double RFFGain)
        {
            string FuncResult = "";
            switch (_Parameter.gb_FinalExecution)
            {
                case Switch_isSaveGain.rb_NotWriteGain:
                    break;
                case Switch_isSaveGain.rb_PassSaveGain:
                    if (!Result.ContainsFalse())
                        FuncResult = SaveGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",LFBGain},
                                            {"FB_Gain_R",RFBGain},
                                            {"FF_Gain_L",LFFGain},
                                            {"FF_Gain_R",RFFGain},
                                            {"MSG","Calibration_SaveGain" },
                                            {"cmdCompleted","" },

                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SaveGain");
                    break;
                case Switch_isSaveGain.rb_SaveGain:
                default:
                    FuncResult = SaveGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",LFBGain},
                                            {"FB_Gain_R",RFBGain},
                                            {"FF_Gain_L",LFFGain},
                                            {"FF_Gain_R",RFFGain},//"MSG",isfb?"Calibration_FB":"Calibration_FF"
                                            {"MSG","Calibration_SaveGain" },
                                            {"cmdCompleted","" },

                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SaveGain");
                    break;
            }
            WriteGain(SequenceDic, LFBGain, RFBGain, LFFGain, RFFGain, -100, -100);

        }

        /// <summary>
        /// 单模式校准
        /// </summary>
        /// <param name="SwitchANCMode"></param>
        /// <param name="FormsName"></param>
        /// <param name="PassText"></param>
        /// <param name="TestData"></param>
        /// <returns></returns>
        string SingleCalibration(string SwitchANCMode, string FormsName, string TestData, string PassText, out double LFBGain, out double RFBGain, out double LFFGain, out double RFFGain)
        {
            //按键返回值
            string KeyDown = "";
            //调用字典事件返回值
            string FuncResult = "";
            //测试结束标识
            string TestEND = $@"{SequenceDic}\Final Test Result.txt";
            //测试次数记录
            int TestCount = 1;
            //每次测试的结果记录
            Dictionary<int, bool> EveryResult = new Dictionary<int, bool>();
            //初始化ANC状态
            string initFlag = InitGain(SwitchANCMode, out LFBGain, out RFBGain, out LFFGain, out RFFGain);
            if (initFlag.ContainsFalse())
                return initFlag;
            do

            {
                string PassStr = $"{TestCount} Pass";
                try
                {
                    //等待SC测试数据
                    string scResult = fw.ExistsFile($"{TestData}|{TestEND}", 20000);
                    if (Config["SN"].ToString().Contains("TE_BZP") && !_Parameter.cb_TE_BZP_Calibration)
                    {
                        KeyDown = ExistsFileOrForms(FormsName, "F2");
                        return "标准品";
                    }
                    if (scResult.ContainsFalse())
                        return $"Not {scResult} {TestCount}Error => SAS202".AddFalse();
                    //判断SC是不是测试完毕了
                    scResult = fw.ExistsFile($"{TestEND}", 100);
                    if (scResult.Contains("True"))
                        return $"Found Final Test Result.txt {TestCount}Error=> SAS203".AddFalse();
                    //判断SC测试结果是不是Pass
                    bool PassTxtResult = fw.ExistsFile($"{PassText}", 200).Equals("True");
                    //把测试结果记录下来
                    EveryResult[TestCount] = PassTxtResult;
                    //判断测试结果pass 测试次数大于等于第一步测试次数 就返回OK
                    if (PassTxtResult && TestCount >= _Parameter.nb_TestMinCount)
                    {
                        KeyDown = ExistsFileOrForms(FormsName, "F2");
                        return KeyDown.ContainsFalse()
                        ? $"{TestCount} Pass SendKey F2 Error => SAS204 {KeyDown}"
                        : PassStr;
                    }
                    //目的从第一步测试内的所有数据挑选曲线 条件 大于3次   所有测试记录中包含True            即将满足第一步测试最大次数尝试 
                    else if (_Parameter.nb_TestMinCount >= 2 && EveryResult.Values.Contains(true) && _Parameter.nb_TestMinCount - 1 == TestCount)
                    {

                        string ContinueCalibration = "";

                        switch (SwitchANCMode)
                        {
                            case Switch_ANC_Mode.SwitchFBOnly:
                                if (_Parameter.gb_CalibrationFeature.isLux_CalibrationFeature())
                                {
                                    LuxShare_CalibrationData.isFB = true; ;
                                    ContinueCalibration = lux_cbd.SelectAll_CurvesData(_Parameter, PassTxtResult, ref LFBGain, ref RFBGain);

                                }
                                else
                                    ContinueCalibration = cdb.SelectAll_CurvesData(_Parameter, PassTxtResult, ref LFBGain, ref RFBGain);
                                LFFGain = -100;
                                RFFGain = -100;
                                FuncResult = SetGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",LFBGain},
                                            {"FB_Gain_R",RFBGain },
                                            {"cmdCompleted","" },
                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");
                                break;
                            case Switch_ANC_Mode.SwitchFFOnly:
                                if (_Parameter.gb_CalibrationFeature.isLux_CalibrationFeature())
                                {
                                    LuxShare_CalibrationData.isFB = false; ;

                                    ContinueCalibration = lux_cbd.SelectAll_CurvesData(_Parameter, PassTxtResult, ref LFFGain, ref RFFGain);

                                }
                                else
                                    ContinueCalibration = cdf.SelectAll_CurvesData(_Parameter, PassTxtResult, ref LFFGain, ref RFFGain);
                                LFBGain = -100;
                                RFBGain = -100;

                                FuncResult = SetGain(new Dictionary<string, object>()
                                        {
                                            {"FF_Gain_L",LFFGain},
                                            {"FF_Gain_R",RFFGain },
                                            {"cmdCompleted","" },
                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");
                                break;
                            default:
                                MessageBox.Show("单模式校准参数设定异常，识别校准ANC错误", "Sound Check 算法提示");
                                break;
                        }
                        if (ContinueCalibration.Contains("Not Calibration True"))
                        {
                            KeyDown = ExistsFileOrForms(FormsName, "F2");
                            return KeyDown.ContainsFalse()
                            ? $"{TestCount} Pass SendKey F2 Error => SAS325 {KeyDown}"
                            : PassStr;
                        }


                    }
                    else if (TestCount < _Parameter.nb_TestMaxCount)
                    {


                        switch (SwitchANCMode)
                        {
                            case Switch_ANC_Mode.SwitchFBOnly:
                                if (_Parameter.gb_CalibrationFeature.isLux_CalibrationFeature())
                                {
                                    LuxShare_CalibrationData.isFB = true;
                                    lux_cbd.ANCAdjust_ForCurves(_Parameter, PassTxtResult, ref LFBGain, ref RFBGain);

                                }
                                else
                                    cdb.ANCAdjust_ForCurvesFB(_Parameter, PassTxtResult, ref LFBGain, ref RFBGain);
                                LFFGain = -100;
                                RFFGain = -100;
                                FuncResult = SetGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",LFBGain},
                                            {"FB_Gain_R",RFBGain },
                                            {"cmdCompleted","" },
                                            {"MSG","Calibration_FB" },

                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");

                                break;
                            case Switch_ANC_Mode.SwitchFFOnly:
                                if (_Parameter.gb_CalibrationFeature.isLux_CalibrationFeature())
                                {
                                    LuxShare_CalibrationData.isFB = false;
                                    lux_cbd.ANCAdjust_ForCurves(_Parameter, PassTxtResult, ref LFFGain, ref RFFGain);

                                }
                                else
                                    cdf.ANCAdjust_ForCurvesFF(_Parameter, PassTxtResult, ref LFFGain, ref RFFGain);
                                LFBGain = -100;
                                RFBGain = -100;
                                FuncResult = SetGain(new Dictionary<string, object>()
                                        {
                                            {"FF_Gain_L",LFFGain},
                                            {"FF_Gain_R",RFFGain },
                                            {"cmdCompleted","" },
                                            {"MSG","Calibration_FF" },

                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");

                                break;
                            default:
                                MessageBox.Show("单模式校准参数设定异常，识别校准ANC错误", "Sound Check 算法提示");
                                break;
                        }
                    }
                    if (_Parameter.nb_cb_DebugCalibration >= 2)
                    {
                        MessageBox.Show($"校准中断成功，当前的Gain值\r\nLFBGain:{LFBGain}\r\nRFBGain:{RFBGain}\r\nLFFGain:{LFFGain}\r\nRFFGain:{RFFGain}");
                        CopyAllTxt($"{SwitchANCMode}_{TestCount}_{LFBGain}_{RFBGain}_{LFFGain}_{RFFGain}");

                    }
                    else if (_Parameter.nb_cb_DebugCalibration >= 1)
                        CopyAllTxt($"{SwitchANCMode}_{TestCount}_{LFBGain}_{RFBGain}_{LFFGain}_{RFFGain}");
                    WriteGain(SequenceDic, LFBGain, RFBGain, LFFGain, RFFGain, -100, -100);
                    DeleteAllTxt(SequenceDic);
                    KeyDown = ExistsFileOrForms(FormsName, "F4");
                    if (KeyDown.ContainsFalse())
                    {
                        KeyDown = ExistsFileOrForms(FormsName, "F4");
                        if (KeyDown.ContainsFalse())
                            return $"{TestCount} SendKey F4 Error => SAS206 {KeyDown}";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erroe=> SAS 375\r\n{ex}", "Sound Check 算法提示");
                    return $"Erroe {ex.Message}".AddFalse(); ;
                }
                TestCount++;
            } while (TestCount <= _Parameter.nb_TestMaxCount);

            string FuncMsg = FuncResult.ContainsFalse() ? FuncResult : "";
            string cb = SwitchANCMode.Contains("FB") ? "FB Mode" : "FF Mode";
            return $"Calibration {cb} NG {TestCount - 1} {FuncMsg}".AddFalse();
        }
        /// <summary>
        /// 混合模式校准
        /// </summary>
        /// <param name="SwitchANCMode"></param>
        /// <param name="FormsName"></param>
        /// <param name="PassText"></param>
        /// <param name="TestData"></param>
        /// <returns></returns>
        string HybridCalibration(bool isCalibration, string SwitchANCMode, string FormsName, string TestData, string PassText, ref double LFBGain, ref double RFBGain, ref double LFFGain, ref double RFFGain)
        {
            //按键返回值
            string KeyDown = "";
            //调用字典事件返回值
            string FuncResult = "";
            //测试结束标识
            string TestEND = $@"{SequenceDic}\Final Test Result.txt";
            //测试次数记录
            int TestCount = 1;
            //每次测试的结果记录
            Dictionary<int, bool> EveryResult = new Dictionary<int, bool>();
            do

            {
                string PassStr = $"{TestCount} Pass";
                try
                {
                    //等待SC测试数据
                    string scResult = fw.ExistsFile($"{TestData}|{TestEND}", 20000);
                    if (Config["SN"].ToString().Contains("TE_BZP") && !_Parameter.cb_TE_BZP_Calibration)
                    {
                        KeyDown = ExistsFileOrForms(FormsName, "F2");
                        return "标准品";
                    }
                    else if (!isCalibration)
                    {
                        KeyDown = ExistsFileOrForms(FormsName, "F2");
                        return "ANC Setting Not Calibration";
                    }
                    if (scResult.ContainsFalse())
                        return $"Not {scResult} {TestCount}Error => SAS202".AddFalse();
                    //判断SC是不是测试完毕了
                    scResult = fw.ExistsFile($"{TestEND}", 100);
                    if (scResult.Contains("True"))
                        return $"Found Final Test Result.txt {TestCount}Error=> SAS203".AddFalse();
                    //判断SC测试结果是不是Pass
                    bool PassTxtResult = fw.ExistsFile($"{PassText}", 200).Equals("True");
                    //把测试结果记录下来
                    EveryResult[TestCount] = PassTxtResult;
                    //判断测试结果pass 测试次数大于等于第一步测试次数 就返回OK
                    if (PassTxtResult && TestCount >= _Parameter.nb_TestMinCount)
                    {
                        KeyDown = ExistsFileOrForms(FormsName, "F2");
                        return KeyDown.ContainsFalse()
                        ? $"{TestCount} Pass SendKey F2 Error => SAS204 {KeyDown}"
                        : PassStr;
                    }
                    //目的从第一步测试内的所有数据挑选曲线 条件 大于3次   所有测试记录中包含True            即将满足第一步测试最大次数尝试 
                    else if (_Parameter.nb_TestMinCount >= 2 && EveryResult.Values.Contains(true) && _Parameter.nb_TestMinCount - 1 == TestCount)
                    {
                        string ContinueCalibration = "";

                        switch (SwitchANCMode)
                        {
                            case Switch_ANC_Mode.SwitchFBOnly:
                                if (_Parameter.gb_CalibrationFeature.isLux_CalibrationFeature())
                                {
                                    LuxShare_CalibrationData.isFB = true;
                                    ContinueCalibration = lux_cbd.SelectAll_CurvesData(_Parameter, PassTxtResult, ref LFBGain, ref RFBGain);

                                }
                                else
                                    ContinueCalibration = cdb.SelectAll_CurvesData(_Parameter, PassTxtResult, ref LFBGain, ref RFBGain);
                                FuncResult = SetGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",LFBGain},
                                            {"FB_Gain_R",RFBGain },
                                            {"FF_Gain_L",LFFGain },
                                            {"FF_Gain_R",RFFGain },
                                            {"MSG","Calibration_FB" },
                                            {"cmdCompleted","" },

                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");
                                break;
                            case Switch_ANC_Mode.SwitchFFOnly:
                                if (_Parameter.gb_CalibrationFeature.isLux_CalibrationFeature())
                                {
                                    LuxShare_CalibrationData.isFB = false;

                                    ContinueCalibration = lux_cbd.SelectAll_CurvesData(_Parameter, PassTxtResult, ref LFFGain, ref RFFGain);
                                }
                                else
                                    ContinueCalibration = cdf.SelectAll_CurvesData(_Parameter, PassTxtResult, ref LFFGain, ref RFFGain);
                                FuncResult = SetGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",LFBGain },
                                            {"FB_Gain_R",RFBGain },
                                            {"FF_Gain_L",LFFGain},
                                            {"FF_Gain_R",RFFGain },
                                            {"MSG","Calibration_FF" },
                                            {"cmdCompleted","" },
                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");
                                break;
                            default:
                                MessageBox.Show("多模式校准参数设定异常，识别校准ANC错误", "Sound Check 算法提示");
                                break;
                        }

                        if (ContinueCalibration.Contains("Not Calibration True"))
                        {
                            KeyDown = ExistsFileOrForms(FormsName, "F2");
                            return KeyDown.ContainsFalse()
                            ? $"{TestCount} Pass SendKey F2 Error => SAS325 {KeyDown}"
                            : PassStr;
                        }
                    }
                    else if (TestCount < _Parameter.nb_TestMaxCount)
                    {


                        switch (SwitchANCMode)
                        {
                            case Switch_ANC_Mode.SwitchFBOnly:
                                if (_Parameter.gb_CalibrationFeature.isLux_CalibrationFeature())
                                {
                                    LuxShare_CalibrationData.isFB = true;
                                    lux_cbd.ANCAdjust_ForCurves(_Parameter, PassTxtResult, ref LFBGain, ref RFBGain);

                                }
                                else
                                    cdb.ANCAdjust_ForCurvesFB(_Parameter, PassTxtResult, ref LFBGain, ref RFBGain);
                                FuncResult = SetGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",LFBGain},
                                            {"FB_Gain_R",RFBGain },
                                            {"FF_Gain_L",LFFGain},
                                            {"FF_Gain_R",RFFGain },
                                            {"MSG","Calibration_FB" },
                                            {"cmdCompleted","" },

                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");

                                break;
                            case Switch_ANC_Mode.SwitchFFOnly:
                                if (_Parameter.gb_CalibrationFeature.isLux_CalibrationFeature())
                                {
                                    LuxShare_CalibrationData.isFB = false;
                                    lux_cbd.ANCAdjust_ForCurves(_Parameter, PassTxtResult, ref LFFGain, ref RFFGain);

                                }
                                else
                                    cdf.ANCAdjust_ForCurvesFF(_Parameter, PassTxtResult, ref LFFGain, ref RFFGain);
                                FuncResult = SetGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",LFBGain},
                                            {"FB_Gain_R",RFBGain },
                                            {"FF_Gain_L",LFFGain},
                                            {"FF_Gain_R",RFFGain },
                                            {"MSG","Calibration_FF" },
                                            {"cmdCompleted","" },
                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");

                                break;
                            default:
                                MessageBox.Show("多模式校准参数设定异常，识别校准ANC错误", "Sound Check 算法提示");
                                break;
                        }
                    }
                    if (_Parameter.nb_cb_DebugCalibration >= 2)
                    {
                        MessageBox.Show($"校准中断成功，当前的Gain值\r\nLFBGain:{LFBGain}\r\nRFBGain:{RFBGain}\r\nLFFGain:{LFFGain}\r\nRFFGain:{RFFGain}");
                        CopyAllTxt($"{SwitchANCMode}_{TestCount}_{LFBGain}_{RFBGain}_{LFFGain}_{RFFGain}");
                    }
                    else if (_Parameter.nb_cb_DebugCalibration >= 1)
                        CopyAllTxt($"{SwitchANCMode}_{TestCount}_{LFBGain}_{RFBGain}_{LFFGain}_{RFFGain}");


                    WriteGain(SequenceDic, LFBGain, RFBGain, LFFGain, RFFGain, -100, -100);

                    DeleteAllTxt(SequenceDic);

                    KeyDown = ExistsFileOrForms(FormsName, "F4");
                    if (KeyDown.ContainsFalse())
                    {
                        KeyDown = ExistsFileOrForms(FormsName, "F4");
                        if (KeyDown.ContainsFalse())
                            return $"{TestCount} SendKey F4 Error => SAS206 {KeyDown}";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erroe=> SAS 375\r\n{ex}", "Sound Check 算法提示");
                    return $"Erroe {ex.Message}".AddFalse(); ;
                }
                TestCount++;
            } while (TestCount <= _Parameter.nb_TestMaxCount);

            string FuncMsg = FuncResult.ContainsFalse() ? FuncResult : "";
            string cb = SwitchANCMode.Contains("FB") ? "FB Mode" : "FF Mode";
            return $"Calibration {cb} NG {TestCount - 1} {FuncMsg}".AddFalse();
        }
        /// <summary>
        /// 初始化Gain
        /// </summary>
        /// <param name="SwitchANCMode"></param>
        /// <param name="LFBGain"></param>
        /// <param name="RFBGain"></param>
        /// <param name="LFFGain"></param>
        /// <param name="RFFGain"></param>
        /// <returns></returns>
        string InitGain(string SwitchANCMode, out double LFBGain, out double RFBGain, out double LFFGain, out double RFFGain)
        {
            string formsResult;
            string FuncResult;
            LFBGain = 0;
            RFBGain = 0;
            LFFGain = 0;
            RFFGain = 0;
            cdb.All_CurvesData.Clear();
            cdf.All_CurvesData.Clear();
            lux_cbd.All_CurvesData.Clear();
            bool isfb = false;
            try
            {
                formsResult = SearchFromsOrFile("Command_", false, "null", 20000, out _, out _);
                if (formsResult.ContainsFalse()) return $"前面项目失败了， 等待Command_窗体超时".AddFalse();

                switch (SwitchANCMode)
                {
                    case Switch_ANC_Mode.SwitchFBOnly:
                        isfb = true;
                        SoundCheck_V1.TemplateANC.Method.ReadCardinal($@"{Config["adminPath"]}\TestItem\{Config["Name"]}\ANC_Cardinal", true);
                        FuncResult = SwitchFBOnly().ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SwitchFBOnly");

                        break;
                    case Switch_ANC_Mode.SwitchFFOnly:
                        SoundCheck_V1.TemplateANC.Method.ReadCardinal($@"{Config["adminPath"]}\TestItem\{Config["Name"]}\ANC_Cardinal", false);
                        FuncResult = SwitchFFOnly().ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SwitchFFOnly");
                        isfb = false;
                        break;
                    case Switch_ANC_Mode.SwitchHybrid:
                    default:
                        isfb = false;
                        SoundCheck_V1.TemplateANC.Method.ReadCardinal($@"{Config["adminPath"]}\TestItem\{Config["Name"]}\ANC_Cardinal", false);
                        FuncResult = SwitchHybrid().ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SwitchHybrid");

                        break;
                }
                Thread.Sleep(100);
                //不是标准品就走正常流程
                if (!Config["SN"].ToString().Contains("TE_BZP"))
                {
                    if (_Parameter.gb_InitGainSeting.IsInitGain())
                    {
                        //初始化Gain值
                        LFBGain = _Parameter.nb_FBLInitGain;
                        RFBGain = _Parameter.nb_FBRInitGain;
                        LFFGain = _Parameter.nb_FFLInitGain;
                        RFFGain = _Parameter.nb_FFRInitGain;
                        FuncResult = SetGain(new Dictionary<string, object>()
                     {
                        {"FB_Gain_L",LFBGain+0.2},
                        {"FB_Gain_R",RFBGain+0.2},
                        {"FF_Gain_L",LFFGain+0.2},
                        {"FF_Gain_R",RFFGain+0.2},
                        {"MSG",isfb?"Calibration_FB":"Calibration_FF"},
                        {"returnData","" },
                        {"cmdCompleted","" },

                     }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");
                        Thread.Sleep(100);

                        FuncResult = SetGain(new Dictionary<string, object>()
                    {
                        {"FB_Gain_L",LFBGain},
                        {"FB_Gain_R",RFBGain},
                        {"FF_Gain_L",LFFGain},
                        {"FF_Gain_R",RFFGain},
                        {"MSG",isfb?"Calibration_FB":"Calibration_FF"},
                        {"returnData","" },
                        {"cmdCompleted","" },

                    }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain"); ;

                    }
                    else
                    {
                        //读取Gain值

                        FuncResult = GetGain().ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.GetGain");
                        JToken Resul = FuncResult.StringToJson();
                        if (!Resul.Value<bool>("cmdCompleted"))
                            return $"ANC.GetGain".AddFalse();
                        LFBGain = Resul.Value<double>("FB_Gain_L").Round();
                        RFBGain = Resul.Value<double>("FB_Gain_R").Round();
                        LFFGain = Resul.Value<double>("FF_Gain_L").Round();
                        RFFGain = Resul.Value<double>("FF_Gain_R").Round();


                    }
                }
                //是标准品走标准品流程
                else
                {
                    //                                                      标准品要校准就初始化
                    if (_Parameter.gb_InitGainSeting.IsInitGain() && _Parameter.cb_TE_BZP_Calibration)
                    {
                        //初始化Gain值
                        LFBGain = _Parameter.nb_FBLInitGain;
                        RFBGain = _Parameter.nb_FBRInitGain;
                        LFFGain = _Parameter.nb_FFLInitGain;
                        RFFGain = _Parameter.nb_FFRInitGain;
                        FuncResult = SetGain(new Dictionary<string, object>()
                     {
                        {"FB_Gain_L",LFBGain+0.2},
                        {"FB_Gain_R",RFBGain+0.2},
                        {"FF_Gain_L",LFFGain+0.2},
                        {"FF_Gain_R",RFFGain+0.2},
                        {"MSG","Calibration_Init"},
                        {"returnData","" },
                        {"cmdCompleted","" },

                     }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");
                        Thread.Sleep(100);

                        FuncResult = SetGain(new Dictionary<string, object>()
                    {
                        {"FB_Gain_L",LFBGain},
                        {"FB_Gain_R",RFBGain},
                        {"FF_Gain_L",LFFGain},
                        {"FF_Gain_R",RFFGain},
                        {"MSG","Calibration_Init"},
                        {"returnData","" },
                        {"cmdCompleted","" },

                    }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain"); ;

                    }
                    else
                    {
                        //读取Gain值

                        FuncResult = GetGain().ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.GetGain");
                        JToken Resul = FuncResult.StringToJson();
                        if (!Resul.Value<bool>("cmdCompleted"))
                            return $"ANC.GetGain".AddFalse();
                        LFBGain = Resul.Value<double>("FB_Gain_L").Round();
                        RFBGain = Resul.Value<double>("FB_Gain_R").Round();
                        LFFGain = Resul.Value<double>("FF_Gain_L").Round();
                        RFFGain = Resul.Value<double>("FF_Gain_R").Round();


                    }
                }

                WriteGain(SequenceDic, LFBGain, RFBGain, LFFGain, RFFGain, -100, -100);
                return "True";
            }
            catch (Exception ex)
            {

                MessageBox.Show($"初始化Gain和切换ANC模式失败 Error=> TA201 False\r\n{ex}");
                return $"Init ANC Mode".AddFalse();
            }



        }
        public string SwitchANCModeMethod(string SwitchANCMode, ref double LFBGain, ref double RFBGain, ref double LFFGain, ref double RFFGain)
        {
            string formsResult;
            string FuncResult;

            formsResult = SearchFromsOrFile("Command_", false, "null", 20000, out _, out _);
            if (formsResult.ContainsFalse()) return $@"Await {SwitchANCMode}".AddFalse();
            bool isfb = true;
            switch (SwitchANCMode)
            {
                case Switch_ANC_Mode.SwitchFBOnly:
                    isfb = true;
                    SoundCheck_V1.TemplateANC.Method.ReadCardinal($@"{Config["adminPath"]}\TestItem\{Config["Name"]}\ANC_Cardinal", true);
                    FuncResult = SwitchFBOnly().ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SwitchFBOnly");

                    break;
                case Switch_ANC_Mode.SwitchFFOnly:
                    isfb = false;

                    SoundCheck_V1.TemplateANC.Method.ReadCardinal($@"{Config["adminPath"]}\TestItem\{Config["Name"]}\ANC_Cardinal", false);
                    FuncResult = SwitchFFOnly().ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SwitchFFOnly");

                    break;
                case Switch_ANC_Mode.SwitchHybrid:
                default:
                    isfb = false;
                    SoundCheck_V1.TemplateANC.Method.ReadCardinal($@"{Config["adminPath"]}\TestItem\{Config["Name"]}\ANC_Cardinal", false);
                    FuncResult = SwitchHybrid().ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SwitchHybrid");
                    break;
            }
            //重启开启ANC的模式后，需要将Gain切换一个没设定过的值，值才会生效

            FuncResult = SetGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",LFBGain + 0.2},
                                            {"FB_Gain_R",RFBGain + 0.2},
                                            {"FF_Gain_L",LFFGain + 0.2},
                                            {"FF_Gain_R",RFFGain + 0.2},
                                            {"MSG",isfb?"Calibration_FB":"Calibration_FF" },
                                            {"cmdCompleted","" },

                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");
            Thread.Sleep(100);
            FuncResult = SetGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",LFBGain},
                                            {"FB_Gain_R",RFBGain },
                                            {"FF_Gain_L",LFFGain},
                                            {"FF_Gain_R",RFFGain },
                                            {"MSG",isfb?"Calibration_FB":"Calibration_FF" },
                                            {"cmdCompleted","" },

                                        }.ObjectToJson()).ShowCMDMSG(_Parameter.cb_ShowCMDMSG, "ANC.SetGain");

            return "True";
        }
        /// <summary>
        /// 二次调用点击窗体的方法
        /// </summary>
        /// <param name="FormsName"></param>
        /// <param name="isbutton"></param>
        /// <param name="Key"></param>
        /// <param name="TimeOut"></param>
        /// <param name="IsFile"></param>
        /// <param name="IsForms"></param>
        /// <returns></returns>
        public string SearchFromsOrFile(string FormsName, bool isbutton, string Key, int TimeOut, out bool IsFile, out bool IsForms)
        {

            string TestEND = $@"{SequenceDic}\Final Test Result.txt";
            string FileResult = "";
            string FormsResult = "";
            IsFile = false;
            IsForms = false;
            Thread FileThread = new Thread(() =>
            {
                FileResult = fw.ExistsFile(TestEND, TimeOut);
            });
            Thread FormsThread = new Thread(() =>
            {
                if (isbutton)
                {
                    FormsResult = fw.SendKeyToWindow(FormsName, Key, TimeOut);

                }
                else
                {
                    FormsResult = fw.SearchWindow(FormsName, TimeOut);

                }
            });
            FileThread.Start();
            FormsThread.Start();
            double Count = (TimeOut + 3000) / 50;
            for (int i = 0; i < Count; i++)
            {
                if (FileResult != "" || FormsResult != "")
                {
                    break;
                }
                Thread.Sleep(50);
            }

            if (FileThread.IsAlive)
                FileThread.Abort();
            if (FormsThread.IsAlive)
                FormsThread.Abort();
            IsFile = !FileResult.Contains("True");
            IsForms = FormsResult.Contains("True");
            if (FormsResult != "")
                return FormsResult;
            if (FileResult.Contains("True"))
                return $"Found File {Path.GetFileName(TestEND)} False";
            return $"Time Out Null False";
        }
        /// <summary>
        /// 点击窗体的方法
        /// </summary>
        /// <param name="FormsName"></param>
        /// <param name="isbutton"></param>
        /// <param name="Key"></param>
        /// <param name="TimeOut"></param>
        /// <param name="IsFile"></param>
        /// <param name="IsForms"></param>
        /// <returns></returns>
        public string ExistsFileOrForms(string FormsName, bool isbutton, string Key, int TimeOut, out bool IsFile, out bool IsForms)
        {

            string TestEND = $@"{SequenceDic}\TEST-END.txt";
            string FileResult = "";
            string FormsResult = "";
            IsFile = false;
            IsForms = false;
            Thread FileThread = new Thread(() =>
            {
                FileResult = fw.ExistsFile(TestEND, TimeOut);
            });
            Thread FormsThread = new Thread(() =>
            {
                if (isbutton)
                {
                    FormsResult = fw.SendKeyToWindow(FormsName, Key, TimeOut);

                }
                else
                {
                    FormsResult = fw.SearchWindow(FormsName, TimeOut);

                }
            });
            FileThread.Start();
            FormsThread.Start();
            double Count = (TimeOut + 3000) / 50;
            for (int i = 0; i < Count; i++)
            {
                if (FileResult != "" || FormsResult != "")
                {
                    break;
                }
                Thread.Sleep(50);
            }
            if (FileThread.IsAlive)
                FileThread.Abort();
            if (FormsThread.IsAlive)
                FormsThread.Abort();
            IsFile = !FileResult.Contains("True");
            IsForms = FormsResult.Contains("True");
            if (FormsResult != "")
                return FormsResult;
            if (FileResult.Contains("True"))
                return $"Found File {Path.GetFileName(TestEND)} False";
            return $"Time Out Null False";
        }
        public string ExistsFileOrForms(string FormsName, string Key, int TimeOut = 3500)
        {
            string str = "Note Search Forms False";
            Thread.Sleep(50);
            // Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 2; i++)
            {
                str = ExistsFileOrForms(FormsName, true, Key, TimeOut, out _, out _);
                if (!str.Contains("False"))
                    break;
                Thread.Sleep(1000);
                str = $"Send {Key} Error {str}";
            }
            return str;
        }
        /// <summary isPublicTestItem="false">
        /// 清除所有txt文件
        /// </summary>
        /// <param name="FilePath">输入指定路径清除 或 使用Sqc路径 输入“default”</param>
        public string DeleteAllTxt(string FilePath)
        {
            switch (FilePath)
            {
                case "default":
                    foreach (var item in Directory.GetFiles(SequenceDic, "*.txt"))
                    {
                        File.Delete(item);
                    }
                    break;

                default:
                    foreach (var item in Directory.GetFiles(FilePath, "*.txt"))
                    {
                        File.Delete(item);
                    }
                    break;
            }
            return "True";
        }


        public string CopyAllTxt(string Gain)
        {

            string DirePath = $"{SequenceDic}\\lastlog";
            Directory.CreateDirectory(DirePath);

            foreach (var item in Directory.GetFiles(SequenceDic, "*.txt"))
            {
                string Name = Path.GetFileNameWithoutExtension(item);
                File.Copy(item, $"{DirePath}\\{Gain}_{Name}.txt", true);
            }


            return "True";
        }

    }


}
