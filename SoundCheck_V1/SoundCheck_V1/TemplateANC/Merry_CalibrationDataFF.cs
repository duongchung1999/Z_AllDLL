using ICSharpCode.SharpZipLib.Tar;
using MerryDllFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MerryDllFramework.CurvesInfo;

namespace SoundCheck_V1.TemplateANC
{
    public class Merry_CalibrationDataFF : Method
    {




        public Dictionary<string, CurvesInfo.info> All_CurvesData = new Dictionary<string, CurvesInfo.info>();


        public string SelectAll_CurvesData(
            ANC_Calibration_Parameter _Parameter,
            bool SCResult, ref double L_Gain, ref double R_Gain)
        {
            CurvesInfo.info LastInfo = new CurvesInfo.info()
            {
                L_Gain = L_Gain,
                R_Gain = R_Gain,
                SCResult = SCResult,
            };
            CurvesInfo.info NextInfo = new CurvesInfo.info();
            //读取数据
            ReadFB_SCData(false, ref LastInfo, ref NextInfo);
            //补充缺失的limit
            SupplementLimit(ref LastInfo, ref NextInfo);
            All_CurvesData[$"{LastInfo.L_Gain}&{LastInfo.R_Gain}"] = LastInfo;
            Dictionary<double, _CurvesData> L_NotScreenAllCurves = new Dictionary<double, _CurvesData>();
            Dictionary<double, _CurvesData> R_NotScreenAllCurves = new Dictionary<double, _CurvesData>();
            StringBuilder adjustLog = new StringBuilder();
            foreach (var item in All_CurvesData)
            {
                adjustLog.AppendLine($"历史的Gain值:{item.Value.L_Gain}&&{item.Value.R_Gain},SC当时结果：{item.Value.SCResult}");
                L_NotScreenAllCurves[item.Value.L_Gain] = item.Value.L_Curves;
                R_NotScreenAllCurves[item.Value.R_Gain] = item.Value.R_Curves;
            }
            ScreenPassCurves(All_CurvesData.First().Value, L_NotScreenAllCurves, R_NotScreenAllCurves, out Dictionary<double, _CurvesData> L_PassCurves, out Dictionary<double, _CurvesData> R_PassCurves, 0);
            NextInfo.L_Result = L_PassCurves.Count > 0;
            NextInfo.R_Result = R_PassCurves.Count > 0;
            NextInfo.Balance_Result = false;
            CopyDictionary(NextInfo.L_Result ? L_PassCurves : L_NotScreenAllCurves,
                           NextInfo.R_Result ? R_PassCurves : R_NotScreenAllCurves,
                           out Dictionary<double, _CurvesData> L_ScreenAllCurves,
                           out Dictionary<double, _CurvesData> R_ScreenAllCurves);
            if (NextInfo.L_Result && NextInfo.R_Result)
            {
                NextInfo.Balance_Result = ScreenBalancePassCurves(ref NextInfo, L_PassCurves, R_PassCurves, 0);
                //平衡优先级 只考虑平衡，不考虑深度等其他参数
                if (_Parameter.cb_EnterBalance)
                {
                    if (NextInfo.Balance_Result)
                    {
                        adjustLog.AppendLine($"平衡优先级 左耳 {NextInfo.L_Result} 右耳 {NextInfo.R_Result} 筛选平衡OK组合=>SCSP001：{NextInfo.Balance_Curves.CurveName}");
                    }
                    else
                    {
                        CalculateAllBalanceCurves(ref NextInfo, L_PassCurves, R_PassCurves);
                        adjustLog.AppendLine($"平衡优先级 左耳 {NextInfo.L_Result} 右耳 {NextInfo.R_Result} 筛选平衡NG组合=>SCSP002：{NextInfo.Balance_Curves.CurveName}");

                    }
                }
                //常规流程
                else
                {
                    if (NextInfo.Balance_Result)
                    {
                        //筛选降噪最深点
                        ScreenFFMinSingle(_Parameter, L_PassCurves, R_PassCurves,
                            out L_ScreenAllCurves,
                            out R_ScreenAllCurves, 20);
                        adjustLog.AppendLine($"常规 最深点筛选 =>SCSP003 L：{L_ScreenAllCurves.Count}");
                        adjustLog.AppendLine($"常规 最深点筛选 =>SCSP003 R：{R_ScreenAllCurves.Count}");


                        //筛选降噪平均点
                        ScreenFFAvgSingle(_Parameter, L_ScreenAllCurves, R_ScreenAllCurves,
                            out L_ScreenAllCurves,
                            out R_ScreenAllCurves, 12);
                        adjustLog.AppendLine($"常规 平均点筛选 =>SCSP004 L：{L_ScreenAllCurves.Count}");
                        adjustLog.AppendLine($"常规 平均点筛选 =>SCSP004 R：{R_ScreenAllCurves.Count}");


                        //筛选降噪最平衡点
                        ScreenBalancePassCurves(ref NextInfo, L_ScreenAllCurves, R_ScreenAllCurves, 0);
                        adjustLog.AppendLine($"常规 平衡 =>SCSP005：：{NextInfo.Balance_Curves.CurveName}");

                    }
                    else
                    {

                        //筛选降噪最深点
                        ScreenFFMinSingle(_Parameter, L_ScreenAllCurves, R_ScreenAllCurves,
                            out L_ScreenAllCurves,
                            out R_ScreenAllCurves, 20);
                        adjustLog.AppendLine($"常规 最深点筛选 =>SCSP006 L：{L_ScreenAllCurves.Count}");
                        adjustLog.AppendLine($"常规 最深点筛选 =>SCSP006 R：{R_ScreenAllCurves.Count}");


                        //筛选降噪平均点
                        ScreenFFAvgSingle(_Parameter, L_ScreenAllCurves, R_ScreenAllCurves,
                            out L_ScreenAllCurves,
                            out R_ScreenAllCurves, 12);
                        adjustLog.AppendLine($"常规 平均点筛选 =>SCSP007 L：{L_ScreenAllCurves.Count}");
                        adjustLog.AppendLine($"常规 平均点筛选 =>SCSP007 R：{R_ScreenAllCurves.Count}");


                        //筛选降噪最平衡点
                        CalculateAllBalanceCurves(ref NextInfo, L_ScreenAllCurves, R_ScreenAllCurves);
                        adjustLog.AppendLine($"常规 平衡 =>SCSP008：：{NextInfo.Balance_Curves.CurveName}");
                    }
                }
            }
            else
            {
                if (_Parameter.cb_EnterBalance)
                {
                    CalculateAllBalanceCurves(ref NextInfo, L_ScreenAllCurves, R_ScreenAllCurves);
                    adjustLog.AppendLine($"平衡优先级 左耳 {NextInfo.L_Result} 右耳 {NextInfo.R_Result} 筛选平衡NG组合=>SCSP002：{NextInfo.Balance_Curves.CurveName}");
                }
                else
                {
                    //筛选降噪最深点
                    ScreenFFMinSingle(_Parameter, L_ScreenAllCurves, R_ScreenAllCurves,
                        out L_ScreenAllCurves,
                        out R_ScreenAllCurves, 20);
                    adjustLog.AppendLine($"常规 最深点筛选 =>SCSP006 L：{L_ScreenAllCurves.Count}");
                    adjustLog.AppendLine($"常规 最深点筛选 =>SCSP006 R：{R_ScreenAllCurves.Count}");


                    //筛选降噪平均点
                    ScreenFFAvgSingle(_Parameter, L_ScreenAllCurves, R_ScreenAllCurves,
                        out L_ScreenAllCurves,
                        out R_ScreenAllCurves, 12);
                    adjustLog.AppendLine($"常规 平均点筛选 =>SCSP007 L：{L_ScreenAllCurves.Count}");
                    adjustLog.AppendLine($"常规 平均点筛选 =>SCSP007 R：{R_ScreenAllCurves.Count}");

                    //筛选降噪最平衡点
                    CalculateAllBalanceCurves(ref NextInfo, L_ScreenAllCurves, R_ScreenAllCurves);
                    adjustLog.AppendLine($"常规 平衡 =>SCSP008：：{NextInfo.Balance_Curves.CurveName}");

                }


            }
            if (_Parameter.cb_ShowCailbration)
                ShowCailbration(LastInfo, NextInfo, adjustLog, "美律特色增益汇总挑选", 4, "FF");
            L_Gain = NextInfo.L_Gain;
            R_Gain = NextInfo.R_Gain;
            if (LastInfo.L_Gain == NextInfo.L_Gain && LastInfo.R_Gain == NextInfo.R_Gain)
            {
                return "Not Calibration True";
            }
            else
            {

                return "True";
            }
        }


        //#################################################### 这里 ####################################################
        /// <summary>
        /// FF校准对外接口
        /// </summary>
        /// <param name="_Parameter"></param>
        /// <param name="SequenceDic"></param>
        /// <param name="NGCurvesIf"></param>
        /// <param name="L_Gain"></param>
        /// <param name="R_Gain"></param>
        /// <returns></returns>
        public string ANCAdjust_ForCurvesFF(
         ANC_Calibration_Parameter _Parameter,
          bool SCResult, ref double L_Gain, ref double R_Gain)
        {
            try
            {
                CurvesInfo.info LastInfo = new CurvesInfo.info()
                {
                    L_Gain = L_Gain,
                    R_Gain = R_Gain,
                    SCResult = SCResult,

                };
                CurvesInfo.info NextInfo = new CurvesInfo.info();
                //读取数据
                ReadFB_SCData(false, ref LastInfo, ref NextInfo);

                //补充缺失的limit
                SupplementLimit(ref LastInfo, ref NextInfo);

                All_CurvesData[$"{LastInfo.L_Gain}&{LastInfo.R_Gain}"] = LastInfo;
                if (!L_FF_CurveGianCardinal.ContainsKey(L_Gain) || !R_FF_CurveGianCardinal.ContainsKey(R_Gain))
                {
                    L_Gain = L_FF_CurveGianCardinal.ElementAt((L_FF_CurveGianCardinal.Count() / 2)).Key;
                    R_Gain = R_FF_CurveGianCardinal.ElementAt((R_FF_CurveGianCardinal.Count() / 2)).Key;
                    return "Gain Value missing".AddFalse();
                }
                //获取所有的Gain
                GetGainRegion(_Parameter.nb_FFGainMax, _Parameter.nb_FFGainMin,
                    L_FF_CurveGianCardinal[L_Gain], R_FF_CurveGianCardinal[R_Gain],
                    out Dictionary<double, double[]> L_Cardinal, out Dictionary<double, double[]> R_Cardinal);

                //计算左右耳所有曲线
                CalculateAllCurve(LastInfo,
                    L_Cardinal,
                    R_Cardinal,
                    out Dictionary<double, _CurvesData> L_NotScreenAllCurves,
                    out Dictionary<double, _CurvesData> R_NotScreenAllCurves);


                //条件筛选
                ScreenMethod(ref LastInfo, ref NextInfo, _Parameter, L_NotScreenAllCurves, R_NotScreenAllCurves, out StringBuilder log);
                //显示Gain

                bool ShowFlg = true;
                //if (L_Gain == NextInfo.L_Gain)
                //{
                //    L_Gain = (L_Gain >= 0
                //        ? L_Gain - 0.2
                //        : L_Gain + 0.2).Round();
                //    ShowFlg = false;
                //}
                //else
                L_Gain = NextInfo.L_Gain.Round();
                //if (R_Gain == NextInfo.R_Gain)
                //{
                //    R_Gain = (R_Gain >= 0
                //        ? R_Gain - 0.2
                //        : R_Gain + 0.2).Round();
                //    ShowFlg = false;
                //}
                //else
                R_Gain = NextInfo.R_Gain.Round();
                //显示Gain
                if (_Parameter.cb_ShowCailbration && ShowFlg)
                    ShowCailbration(LastInfo, NextInfo, log, "美律特色搜索增益中", 3, "FF");

                return "True";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"=>SMA 217 数据异常，算法报错“ANCAdjust_ForCurvesFF”\r\n“{ex}”", "Sound Check 算法提示");
                return $"{ex.Message} False";
            }
        }


        /// <summary>
        /// 扫描的方法
        /// </summary>
        /// <param name="LastInfo">上一个曲线数据</param>
        /// <param name="NextInfo">下一个曲线数据</param>
        /// <param name="_Parameter">参数类</param>
        /// <param name="L_NotScreenAllCurves">左耳预测的曲线集</param>
        /// <param name="R_NotScreenAllCurves">右耳预测的曲线集</param>
        /// <param name="adjustLog"></param>
        public void ScreenMethod(
            ref CurvesInfo.info LastInfo,
            ref CurvesInfo.info NextInfo,
            ANC_Calibration_Parameter _Parameter,
            Dictionary<double, _CurvesData> L_NotScreenAllCurves,
            Dictionary<double, _CurvesData> R_NotScreenAllCurves,
            out StringBuilder adjustLog
            )
        {


            //嵌入limit 筛选可以Pass的曲线
            ScreenPassCurves(LastInfo, L_NotScreenAllCurves, R_NotScreenAllCurves, out Dictionary<double, _CurvesData> L_PassCurves, out Dictionary<double, _CurvesData> R_PassCurves, 0.2);
            adjustLog = new StringBuilder();
            adjustLog.AppendLine($"嵌入Limit L FF Pass数量:{L_PassCurves.Count} // :");
            foreach (var item in L_PassCurves)
                adjustLog.Append($"{item.Key}，");
            adjustLog.AppendLine($" ");
            adjustLog.AppendLine($"嵌入Limit R FF Pass数量:{R_PassCurves.Count} // ");
            foreach (var item in R_PassCurves)
                adjustLog.Append($"{item.Key}，");

            NextInfo.L_Result = L_PassCurves.Count > 0;
            NextInfo.R_Result = R_PassCurves.Count > 0;
            NextInfo.Balance_Result = false;
            CopyDictionary(NextInfo.L_Result ? L_PassCurves : L_NotScreenAllCurves,
                           NextInfo.R_Result ? R_PassCurves : R_NotScreenAllCurves,
                           out Dictionary<double, _CurvesData> L_ScreenAllCurves,
                           out Dictionary<double, _CurvesData> R_ScreenAllCurves);

            if (NextInfo.L_Result && NextInfo.R_Result)
            {
                NextInfo.Balance_Result = ScreenBalancePassCurves(ref NextInfo, L_PassCurves, R_PassCurves, 0.4);
                //平衡优先级 只考虑平衡，不考虑深度等其他参数
                if (_Parameter.cb_EnterBalance)
                {
                    if (NextInfo.Balance_Result)
                    {
                        adjustLog.AppendLine($"平衡优先级 左耳 {NextInfo.L_Result} 右耳 {NextInfo.R_Result} 筛选平衡OK组合=>SCSP001：{NextInfo.Balance_Curves.CurveName}\r\n");
                    }
                    else
                    {
                        CalculateAllBalanceCurves(ref NextInfo, L_PassCurves, R_PassCurves);
                        adjustLog.AppendLine($"平衡优先级 左耳 {NextInfo.L_Result} 右耳 {NextInfo.R_Result} 筛选平衡NG组合=>SCSP002：{NextInfo.Balance_Curves.CurveName}\r\n");

                    }
                }
                //常规流程
                else
                {
                    if (NextInfo.Balance_Result)
                    {
                        //筛选降噪最深点
                        ScreenFFMinSingle(_Parameter, L_PassCurves, R_PassCurves,
                            out L_ScreenAllCurves,
                            out R_ScreenAllCurves, 20);
                        adjustLog.AppendLine($"常规 最深点筛选 =>SCSP003 L：{L_ScreenAllCurves.Count}\r\n");
                        adjustLog.AppendLine($"常规 最深点筛选 =>SCSP003 R：{R_ScreenAllCurves.Count}\r\n");


                        //筛选降噪平均点
                        ScreenFFAvgSingle(_Parameter, L_ScreenAllCurves, R_ScreenAllCurves,
                            out L_ScreenAllCurves,
                            out R_ScreenAllCurves, 12);
                        adjustLog.AppendLine($"常规 平均点筛选 =>SCSP004 L：{L_ScreenAllCurves.Count}\r\n");
                        adjustLog.AppendLine($"常规 平均点筛选 =>SCSP004 R：{R_ScreenAllCurves.Count}\r\n");


                        //筛选降噪最平衡点
                        ScreenBalancePassCurves(ref NextInfo, L_ScreenAllCurves, R_ScreenAllCurves, 0.4);
                        adjustLog.AppendLine($"常规 平衡 =>SCSP005：：{NextInfo.Balance_Curves.CurveName}\r\n");

                    }
                    else
                    {

                        //筛选降噪最深点
                        ScreenFFMinSingle(_Parameter, L_ScreenAllCurves, R_ScreenAllCurves,
                            out L_ScreenAllCurves,
                            out R_ScreenAllCurves, 20);
                        adjustLog.AppendLine($"常规 最深点筛选 =>SCSP006 L：{L_ScreenAllCurves.Count}\r\n");
                        adjustLog.AppendLine($"常规 最深点筛选 =>SCSP006 R：{R_ScreenAllCurves.Count}\r\n");


                        //筛选降噪平均点
                        ScreenFFAvgSingle(_Parameter, L_ScreenAllCurves, R_ScreenAllCurves,
                            out L_ScreenAllCurves,
                            out R_ScreenAllCurves, 12);
                        adjustLog.AppendLine($"常规 平均点筛选 =>SCSP007 L：{L_ScreenAllCurves.Count}\r\n");
                        adjustLog.AppendLine($"常规 平均点筛选 =>SCSP007 R：{R_ScreenAllCurves.Count}\r\n");


                        //筛选降噪最平衡点
                        CalculateAllBalanceCurves(ref NextInfo, L_ScreenAllCurves, R_ScreenAllCurves);
                        adjustLog.AppendLine($"常规 平衡 =>SCSP008：：{NextInfo.Balance_Curves.CurveName}\r\n");
                    }
                }
            }
            else
            {
                if (_Parameter.cb_EnterBalance)
                {
                    CalculateAllBalanceCurves(ref NextInfo, L_ScreenAllCurves, R_ScreenAllCurves);
                    adjustLog.AppendLine($"平衡优先级 左耳{NextInfo.L_Result} 右耳 {NextInfo.R_Result} 筛选平衡NG组合=>SCSP002：{NextInfo.Balance_Curves.CurveName}\r\n");
                }
                else
                {
                    //筛选降噪最深点
                    ScreenFFMinSingle(_Parameter, L_ScreenAllCurves, R_ScreenAllCurves,
                        out L_ScreenAllCurves,
                        out R_ScreenAllCurves, 20);
                    adjustLog.AppendLine($"常规 最深点筛选 =>SCSP006 L：{L_ScreenAllCurves.Count}\r\n");
                    adjustLog.AppendLine($"常规 最深点筛选 =>SCSP006 R：{R_ScreenAllCurves.Count}\r\n");


                    //筛选降噪平均点
                    ScreenFFAvgSingle(_Parameter, L_ScreenAllCurves, R_ScreenAllCurves,
                        out L_ScreenAllCurves,
                        out R_ScreenAllCurves, 12);
                    adjustLog.AppendLine($"常规 平均点筛选 =>SCSP007 L：{L_ScreenAllCurves.Count}\r\n");
                    adjustLog.AppendLine($"常规 平均点筛选 =>SCSP007 R：{R_ScreenAllCurves.Count}\r\n");

                    //筛选降噪最平衡点
                    CalculateAllBalanceCurves(ref NextInfo, L_ScreenAllCurves, R_ScreenAllCurves);
                    adjustLog.AppendLine($"常规 平衡 =>SCSP008：：{NextInfo.Balance_Curves.CurveName}\r\n");

                }


            }

        }


        public void ScreenFFMinSingle(ANC_Calibration_Parameter _Parameter,
            Dictionary<double, _CurvesData> L_Cardinal,
            Dictionary<double, _CurvesData> R_Cardinal,
            out Dictionary<double, _CurvesData> L_ScreenAllCurves,
            out Dictionary<double, _CurvesData> R_ScreenAllCurves,
            int CurvesAmount
            )
        {
            L_ScreenAllCurves = new Dictionary<double, _CurvesData>();
            R_ScreenAllCurves = new Dictionary<double, _CurvesData>();
            try
            {
                //范围最深点
                if (_Parameter.gb_FFMinType.isRegion())
                {
                    int L_StartFreqIndex = ScreenApproachFigure(L_Cardinal.First().Value.Xdata, _Parameter.nb_FFMinSratrFreq);
                    int L_ENDFreqIndex = ScreenApproachFigure(L_Cardinal.First().Value.Xdata, _Parameter.nb_FFMinENDFreq);
                    Dictionary<double, double> L_RegionMicFreq = new Dictionary<double, double>();
                    foreach (var item in L_Cardinal)
                    {
                        List<double> Ydata = new List<double>();
                        for (int i = L_StartFreqIndex; i <= L_ENDFreqIndex; i++)
                            Ydata.Add(item.Value.Ydata[i]);
                        L_RegionMicFreq.Add(item.Key, Ydata.Min());
                    }

                    int[] L_Min = ScreenApproachFigure(L_RegionMicFreq.Values.ToArray(), _Parameter.nb_FFMinTarget, CurvesAmount, _Parameter.nb_FFMinFault_Tolerant);
                    foreach (var item in L_Min)
                        L_ScreenAllCurves.Add(L_Cardinal.ElementAt(item).Key, L_Cardinal.ElementAt(item).Value);


                    int R_StartFreqIndex = ScreenApproachFigure(R_Cardinal.First().Value.Xdata, _Parameter.nb_FFMinSratrFreq);
                    int R_ENDFreqIndex = ScreenApproachFigure(R_Cardinal.First().Value.Xdata, _Parameter.nb_FFMinENDFreq);
                    Dictionary<double, double> R_RegionMicFreq = new Dictionary<double, double>();
                    foreach (var item in R_Cardinal)
                    {
                        List<double> Ydata = new List<double>();
                        for (int i = R_StartFreqIndex; i <= R_ENDFreqIndex; i++)
                            Ydata.Add(item.Value.Ydata[i]);
                        R_RegionMicFreq.Add(item.Key, Ydata.Min());
                    }

                    int[] R_Min = ScreenApproachFigure(R_RegionMicFreq.Values.ToArray(), _Parameter.nb_FFMinTarget, CurvesAmount, _Parameter.nb_FFMinFault_Tolerant);
                    foreach (var item in R_Min)
                        R_ScreenAllCurves.Add(R_Cardinal.ElementAt(item).Key, R_Cardinal.ElementAt(item).Value);
                }
                else if (!_Parameter.gb_FFMinType.isRegion())
                {


                    int L_SingleFreqIndex = ScreenApproachFigure(L_Cardinal.First().Value.Xdata, _Parameter.nb_FFMinSingleFreq);
                    Dictionary<double, double> L_RegionMicFreq = new Dictionary<double, double>();
                    foreach (var item in L_Cardinal)
                        L_RegionMicFreq.Add(item.Key, item.Value.Ydata[L_SingleFreqIndex]);

                    int[] L_Min = ScreenApproachFigure(L_RegionMicFreq.Values.ToArray(), _Parameter.nb_FFMinTarget, CurvesAmount, _Parameter.nb_FFMinFault_Tolerant);
                    foreach (var item in L_Min)
                        L_ScreenAllCurves.Add(L_Cardinal.ElementAt(item).Key, L_Cardinal.ElementAt(item).Value);


                    int R_SingleFreqIndex = ScreenApproachFigure(R_Cardinal.First().Value.Xdata, _Parameter.nb_FFMinSingleFreq);
                    Dictionary<double, double> R_RegionMicFreq = new Dictionary<double, double>();
                    foreach (var item in R_Cardinal)
                        R_RegionMicFreq.Add(item.Key, item.Value.Ydata[R_SingleFreqIndex]);

                    int[] R_Min = ScreenApproachFigure(R_RegionMicFreq.Values.ToArray(), _Parameter.nb_FFMinTarget, CurvesAmount, _Parameter.nb_FFMinFault_Tolerant);
                    foreach (var item in R_Min)
                        R_ScreenAllCurves.Add(R_Cardinal.ElementAt(item).Key, R_Cardinal.ElementAt(item).Value);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"=>SMS 439 寻找目标最深点异常“ScreenFFMinSingle”\r\n“{ex}”", "Sound Check 算法提示");

            }






        }


        public void ScreenFFAvgSingle(ANC_Calibration_Parameter _Parameter,
        Dictionary<double, _CurvesData> L_Cardinal,
        Dictionary<double, _CurvesData> R_Cardinal,
        out Dictionary<double, _CurvesData> L_ScreenAllCurves,
        out Dictionary<double, _CurvesData> R_ScreenAllCurves,
        int CurvesAmount)
        {

            L_ScreenAllCurves = new Dictionary<double, _CurvesData>();
            R_ScreenAllCurves = new Dictionary<double, _CurvesData>();
            try
            {
                //范围最深点
                if (_Parameter.gb_FFAvgType.isRegion())
                {
                    int L_StartFreqIndex = ScreenApproachFigure(L_Cardinal.First().Value.Xdata, _Parameter.nb_FFAvgSratrFreq);
                    int L_ENDFreqIndex = ScreenApproachFigure(L_Cardinal.First().Value.Xdata, _Parameter.nb_FFAvgENDFreq);
                    Dictionary<double, double> L_RegionAvgFreq = new Dictionary<double, double>();
                    foreach (var item in L_Cardinal)
                    {
                        List<double> Ydata = new List<double>();
                        for (int i = L_StartFreqIndex; i <= L_ENDFreqIndex; i++)
                            Ydata.Add(item.Value.Ydata[i]);
                        L_RegionAvgFreq.Add(item.Key, Ydata.Average());
                    }

                    int[] L_Avg = ScreenApproachFigure(L_RegionAvgFreq.Values.ToArray(), _Parameter.nb_FFAvgTarget, CurvesAmount, _Parameter.nb_FFAvgFault_Tolerant);
                    foreach (var item in L_Avg)
                        L_ScreenAllCurves.Add(L_Cardinal.ElementAt(item).Key, L_Cardinal.ElementAt(item).Value);

                    int R_StartFreqIndex = ScreenApproachFigure(R_Cardinal.First().Value.Xdata, _Parameter.nb_FFAvgSratrFreq);
                    int R_ENDFreqIndex = ScreenApproachFigure(R_Cardinal.First().Value.Xdata, _Parameter.nb_FFAvgENDFreq);
                    Dictionary<double, double> R_RegionAvgFreq = new Dictionary<double, double>();
                    foreach (var item in R_Cardinal)
                    {
                        List<double> Ydata = new List<double>();
                        for (int i = R_StartFreqIndex; i <= R_ENDFreqIndex; i++)
                            Ydata.Add(item.Value.Ydata[i]);
                        R_RegionAvgFreq.Add(item.Key, Ydata.Average());
                    }

                    int[] R_Avg = ScreenApproachFigure(R_RegionAvgFreq.Values.ToArray(), _Parameter.nb_FFAvgTarget, CurvesAmount, _Parameter.nb_FFAvgFault_Tolerant);
                    foreach (var item in R_Avg)
                        R_ScreenAllCurves.Add(R_Cardinal.ElementAt(item).Key, R_Cardinal.ElementAt(item).Value);
                }
                else if (!_Parameter.gb_FFAvgType.isRegion())
                {
                    //如果参数少了就跳出
                    if (_Parameter.dg_FFAvgFreqs.Count() <= 0)
                    {
                        MessageBox.Show("FF 校准参数没有设定单点的频率，条件筛选失败");
                        return;
                    }
                    //计算左边的单点平均值
                    Dictionary<double, double> L_SingleAvgFreq = new Dictionary<double, double>();
                    foreach (var item in L_Cardinal)
                    {
                        List<double> SingleFreqs = new List<double>();
                        foreach (var Singles in _Parameter.dg_FFAvgFreqs)
                        {
                            int index = ScreenApproachFigure(item.Value.Xdata, Singles);
                            SingleFreqs.Add(item.Value.Ydata[index]);
                        }
                        L_SingleAvgFreq.Add(item.Key, SingleFreqs.Average());
                    }
                    //计算右边的单点平均值
                    int[] L_Avg = ScreenApproachFigure(L_SingleAvgFreq.Values.ToArray(), _Parameter.nb_FFAvgTarget, CurvesAmount, _Parameter.nb_FFAvgFault_Tolerant);
                    foreach (var item in L_Avg)
                        L_ScreenAllCurves.Add(L_Cardinal.ElementAt(item).Key, L_Cardinal.ElementAt(item).Value);

                    Dictionary<double, double> R_SingleAvgFreq = new Dictionary<double, double>();
                    foreach (var item in R_Cardinal)
                    {
                        List<double> SingleFreqs = new List<double>();
                        foreach (var Singles in _Parameter.dg_FFAvgFreqs)
                        {
                            int index = ScreenApproachFigure(item.Value.Xdata, Singles);
                            SingleFreqs.Add(item.Value.Ydata[index]);
                        }
                        R_SingleAvgFreq.Add(item.Key, SingleFreqs.Average());
                    }
                    int[] R_Avg = ScreenApproachFigure(R_SingleAvgFreq.Values.ToArray(), _Parameter.nb_FFAvgTarget, CurvesAmount, _Parameter.nb_FFAvgFault_Tolerant);
                    foreach (var item in R_Avg)
                        R_ScreenAllCurves.Add(R_Cardinal.ElementAt(item).Key, R_Cardinal.ElementAt(item).Value);

                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"=>SMS 541 寻找目标平均值异常“ScreenFFAvgSingle”\r\n“{ex}”", "Sound Check 算法提示");

            }






        }




        bool ifCorves(_CurvesData Curves, _CurvesData UpLimit, _CurvesData LowLimit)
        {
            int UpindexStart = ScreenApproachFigure(Curves.Xdata, UpLimit.Xdata[0]);
            for (int i = 0; i < UpLimit.Ydata.Length; i++)
            {
                double UpLimitValue = UpLimit.Ydata[i];
                double UpLimitX = UpLimit.Xdata[i];



                double Value = Curves.Ydata[i + UpindexStart];
                double ValueX = Curves.Xdata[i + UpindexStart];

                if (UpLimitValue <= Value)
                {
                    return false;
                }
            }
            int LowindexStart = ScreenApproachFigure(Curves.Xdata, LowLimit.Xdata[0]);
            for (int i = 0; i < LowLimit.Ydata.Length; i++)
            {


                double LowLimitValue = LowLimit.Ydata[i];
                double LowLimitX = LowLimit.Xdata[i];

                double Value = Curves.Ydata[i + LowindexStart];
                double ValueX = Curves.Xdata[i + LowindexStart];


                if (LowLimitValue >= Value)
                {
                    return false;
                }
            }
            return true;
        }
        double AvgTarget(_CurvesData Curves, List<double> Freqs)
        {
            List<double> _val = new List<double>();
            foreach (var freq in Freqs)
            {
                int index = ScreenApproachFigure(Curves.Xdata, freq);
                _val.Add(Curves.Ydata[index]);
            }
            return _val.Average();

        }






    }
}

