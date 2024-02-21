using MerryDllFramework;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public class LuxShare_CalibrationData : Method
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
            ReadFB_SCData(isFB, ref LastInfo, ref NextInfo);
            //补充缺失的limit
            SupplementLimit(ref LastInfo, ref NextInfo);
            All_CurvesData[$"{LastInfo.L_Gain}&{LastInfo.R_Gain}"] = LastInfo;
            Dictionary<double, _CurvesData> L_NotScreenAllCurves = new Dictionary<double, _CurvesData>();
            Dictionary<double, _CurvesData> R_NotScreenAllCurves = new Dictionary<double, _CurvesData>();
            StringBuilder adjustLog = new StringBuilder();

            adjustLog = new StringBuilder();
            Dictionary<string, double[]> LeftTargetStr = null;
            Dictionary<string, double[]> RightTargetStr = null;
            if (isFB)
            {
                adjustLog.Append($"FF 左耳目标：{_Parameter.str_FBLTarget}  右耳目标：{_Parameter.str_FBRTarget}\r\n");
                LeftTargetStr = _Parameter.str_FBLTarget.TargetToDic();
                RightTargetStr = _Parameter.str_FBRTarget.TargetToDic();

            }
            else
            {
                adjustLog.Append($"FB 左耳目标：{_Parameter.str_FFLTarget}  右耳目标：{_Parameter.str_FFRTarget}\r\n");
                LeftTargetStr = _Parameter.str_FFLTarget.TargetToDic();
                RightTargetStr = _Parameter.str_FFRTarget.TargetToDic();

            }
            foreach (var item in All_CurvesData)
            {
                adjustLog.AppendLine($"历史的Gain值:{item.Value.L_Gain}&&{item.Value.R_Gain},SC当时结果：{item.Value.SCResult}");
                L_NotScreenAllCurves[item.Value.L_Gain] = item.Value.L_Curves;
                R_NotScreenAllCurves[item.Value.R_Gain] = item.Value.R_Curves;
            }
            ScreenPassCurves(All_CurvesData.First().Value, L_NotScreenAllCurves, R_NotScreenAllCurves, out Dictionary<double, _CurvesData> L_PassCurves, out Dictionary<double, _CurvesData> R_PassCurves);
            NextInfo.L_Result = L_PassCurves.Count > 0;
            NextInfo.R_Result = R_PassCurves.Count > 0;
            NextInfo.Balance_Result = false;
            CopyDictionary(NextInfo.L_Result ? L_PassCurves : L_NotScreenAllCurves,
                           NextInfo.R_Result ? R_PassCurves : R_NotScreenAllCurves,
                           out Dictionary<double, _CurvesData> L_ScreenAllCurves,
                           out Dictionary<double, _CurvesData> R_ScreenAllCurves);


            //当左右耳都符合规格的曲线时就符合规格的曲线筛选感差
            if (NextInfo.L_Result && NextInfo.R_Result)
            {
                ScreenTargetGain(LeftTargetStr, RightTargetStr, L_PassCurves, R_PassCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);
                NextInfo.Balance_Result = ScreenBalancePassCurves(ref NextInfo, LCurves, RCurves, 0);
                if (NextInfo.Balance_Result)
                {
                    adjustLog.AppendLine($"平衡筛选 左耳 {NextInfo.L_Result} 右耳 {NextInfo.R_Result} 筛选平衡OK组合=>SLS145：{NextInfo.Balance_Curves.CurveName}\r\n");

                }
                else
                {
                    CalculateAllBalanceCurves(ref NextInfo, L_PassCurves, R_PassCurves);
                    adjustLog.AppendLine($"平衡筛选 左耳 {NextInfo.L_Result} 右耳 {NextInfo.R_Result} 筛选平衡NG组合=>SLS151：{NextInfo.Balance_Curves.CurveName}\r\n");
                }

            }
            //当只有左耳或者右耳符合规格时
            else
            {
                ScreenTargetGain(LeftTargetStr, RightTargetStr, L_ScreenAllCurves, R_ScreenAllCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);
                CalculateAllBalanceCurves(ref NextInfo, LCurves, RCurves);

                adjustLog.AppendLine($"平衡筛选 左耳 {NextInfo.L_Result} 右耳 {NextInfo.R_Result} =>A105：{NextInfo.Balance_Curves.CurveName}\r\n");

            }

            if (_Parameter.cb_ShowCailbration)
                ShowCailbration(LastInfo, NextInfo, adjustLog, "美律特色增益汇总挑选", 4, isFB ? "FB" : "FF");
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
        static bool ___isFB;
        public static bool isFB
        {
            get
            {
                return ___isFB;
            }
            set
            {
                ___isFB = value;
            }
        }
        /// <summary>
        ///  立讯特色校准
        /// </summary>
        /// <param name="_Parameter"></param>
        /// <param name="isFB"></param>
        /// <param name="L_Gain"></param>
        /// <param name="R_Gain"></param>
        /// <returns></returns>
        public string ANCAdjust_ForCurves(
            ANC_Calibration_Parameter _Parameter,
            bool SCResult, ref double L_Gain, ref double R_Gain
           )
        {

            CurvesInfo.info LastInfo = new CurvesInfo.info()
            {
                L_Gain = L_Gain,
                R_Gain = R_Gain,
                SCResult = SCResult
            };
            CurvesInfo.info NextInfo = new CurvesInfo.info();

            //读取数据
            ReadFB_SCData(isFB, ref LastInfo, ref NextInfo);

            //补充缺失的limit
            SupplementLimit(ref LastInfo, ref NextInfo);
            All_CurvesData[$"{LastInfo.L_Gain}&{LastInfo.R_Gain}"] = LastInfo;
            Dictionary<double, double[]> L_Cardinal = null;
            Dictionary<double, double[]> R_Cardinal = null;

            //获取所有的Gain
            if (isFB)
            {
                if (!L_FB_CurveGianCardinal.ContainsKey(L_Gain) || !R_FB_CurveGianCardinal.ContainsKey(R_Gain))
                {
                    L_Gain = L_FB_CurveGianCardinal.ElementAt((L_FB_CurveGianCardinal.Count() / 2)).Key;
                    R_Gain = R_FB_CurveGianCardinal.ElementAt((R_FB_CurveGianCardinal.Count() / 2)).Key;
                    return "Gain Value missing".AddFalse();
                }
                GetGainRegion(_Parameter.nb_FBGainMax, _Parameter.nb_FBGainMin,
                    L_FB_CurveGianCardinal[L_Gain],
                    R_FB_CurveGianCardinal[R_Gain],
                    out L_Cardinal,
                    out R_Cardinal);
            }
            else
            {
                if (!L_FF_CurveGianCardinal.ContainsKey(L_Gain) || !R_FF_CurveGianCardinal.ContainsKey(R_Gain))
                {
                    L_Gain = L_FF_CurveGianCardinal.ElementAt((L_FF_CurveGianCardinal.Count() / 2)).Key;
                    R_Gain = R_FF_CurveGianCardinal.ElementAt((R_FF_CurveGianCardinal.Count() / 2)).Key;
                    return "Gain Value missing".AddFalse();
                }
                GetGainRegion(_Parameter.nb_FFGainMax, _Parameter.nb_FFGainMin,
                    L_FF_CurveGianCardinal[L_Gain],
                    R_FF_CurveGianCardinal[R_Gain],
                    out L_Cardinal,
                    out R_Cardinal);
            }


            //计算左右耳所有曲线
            CalculateAllCurve(LastInfo,
                L_Cardinal,
                R_Cardinal,
                out Dictionary<double, _CurvesData> L_NotScreenAllCurves,
                out Dictionary<double, _CurvesData> R_NotScreenAllCurves);
            //条件筛选
            ScreenMethod(ref LastInfo, ref NextInfo, _Parameter, L_NotScreenAllCurves, R_NotScreenAllCurves, out StringBuilder log);
            bool ShowFlg = true;
            if (L_Gain == NextInfo.L_Gain)
            {
                L_Gain = (L_Gain >= 0
                    ? L_Gain - 0.2
                    : L_Gain + 0.2).Round();
                ShowFlg = false;
            }
            else
                L_Gain = NextInfo.L_Gain.Round();
            if (R_Gain == NextInfo.R_Gain)
            {
                R_Gain = (R_Gain >= 0
                    ? R_Gain - 0.2
                    : R_Gain + 0.2).Round();
                ShowFlg = false;
            }
            else
                R_Gain = NextInfo.R_Gain.Round();

            //显示Gain
            if (_Parameter.cb_ShowCailbration && ShowFlg)
                ShowCailbration(LastInfo, NextInfo, log, "立讯特色搜索增益中", 3, isFB ? "FB" : "FF");
            return "True";


        }


        void ScreenMethod(
            ref CurvesInfo.info LastInfo,
            ref CurvesInfo.info NextInfo,
            ANC_Calibration_Parameter _Parameter,
            Dictionary<double, _CurvesData> L_NotScreenAllCurves,
            Dictionary<double, _CurvesData> R_NotScreenAllCurves,
            out StringBuilder adjustLog
            )
        {

            //嵌入limit 筛选可以Pass的曲线
            ScreenPassCurves(LastInfo,
                L_NotScreenAllCurves,
                R_NotScreenAllCurves,
                out Dictionary<double, _CurvesData> L_PassCurves,
                out Dictionary<double, _CurvesData> R_PassCurves);

            adjustLog = new StringBuilder();
            Dictionary<string, double[]> LeftTargetStr = null;
            Dictionary<string, double[]> RightTargetStr = null;
            if (isFB)
            {
                adjustLog.Append($"FF 左耳目标：{_Parameter.str_FBLTarget}  右耳目标：{_Parameter.str_FBRTarget}\r\n");
                LeftTargetStr = _Parameter.str_FBLTarget.TargetToDic();
                RightTargetStr = _Parameter.str_FBRTarget.TargetToDic();

            }
            else
            {
                adjustLog.Append($"FB 左耳目标：{_Parameter.str_FFLTarget}  右耳目标：{_Parameter.str_FFRTarget}\r\n");
                LeftTargetStr = _Parameter.str_FFLTarget.TargetToDic();
                RightTargetStr = _Parameter.str_FFRTarget.TargetToDic();

            }
            adjustLog = new StringBuilder();
            adjustLog.AppendLine($"嵌入Limit L Pass数量:{L_PassCurves.Count} // :");
            foreach (var item in L_PassCurves)
                adjustLog.Append($"{item.Key}，");
            adjustLog.AppendLine($" ");
            adjustLog.AppendLine($"嵌入Limit R Pass数量:{R_PassCurves.Count} // :");
            foreach (var item in R_PassCurves)
                adjustLog.Append($"{item.Key}，");

            NextInfo.L_Result = L_PassCurves.Count > 0;
            NextInfo.R_Result = R_PassCurves.Count > 0;
            NextInfo.Balance_Result = false;

            CopyDictionary(
                NextInfo.L_Result ? L_PassCurves : L_NotScreenAllCurves,
                      NextInfo.R_Result ? R_PassCurves : R_NotScreenAllCurves,
                      out Dictionary<double, _CurvesData> L_ScreenAllCurves,
                      out Dictionary<double, _CurvesData> R_ScreenAllCurves);


            //当左右耳都符合规格的曲线时就符合规格的曲线筛选感差
            if (NextInfo.L_Result && NextInfo.R_Result)
            {
                ScreenTargetGain(LeftTargetStr, RightTargetStr, L_PassCurves, R_PassCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);
                NextInfo.Balance_Result = ScreenBalancePassCurves(ref NextInfo, LCurves, RCurves, 0.4);
                if (NextInfo.Balance_Result)
                {
                    adjustLog.AppendLine($"平衡筛选 左耳 {NextInfo.L_Result} 右耳 {NextInfo.R_Result} 筛选平衡OK组合=>SLS145：{NextInfo.Balance_Curves.CurveName}\r\n");

                }
                else
                {
                    CalculateAllBalanceCurves(ref NextInfo, L_PassCurves, R_PassCurves);
                    adjustLog.AppendLine($"平衡筛选 左耳 {NextInfo.L_Result} 右耳 {NextInfo.R_Result} 筛选平衡NG组合=>SLS151：{NextInfo.Balance_Curves.CurveName}\r\n");
                }

            }
            //当只有左耳或者右耳符合规格时
            else
            {
                ScreenTargetGain(LeftTargetStr, RightTargetStr, L_ScreenAllCurves, R_ScreenAllCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);
                CalculateAllBalanceCurves(ref NextInfo, LCurves, RCurves);
                adjustLog.AppendLine($"平衡筛选 左耳 {NextInfo.L_Result} 右耳 {NextInfo.R_Result} =>A105：{NextInfo.Balance_Curves.CurveName}\r\n");
            }
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

        void ScreenTargetGain(
        Dictionary<string, double[]> LeftTargetStr,
        Dictionary<string, double[]> RightTargetStr,
        Dictionary<double, _CurvesData> L_Curves,
        Dictionary<double, _CurvesData> R_Curves,
            out Dictionary<double, _CurvesData> L_SelectCurves,
            out Dictionary<double, _CurvesData> R_SelectCurves)
        {

            L_SelectCurves = new Dictionary<double, _CurvesData>();
            R_SelectCurves = new Dictionary<double, _CurvesData>();

            try
            {
                selectAvg(LeftTargetStr, ref L_Curves, ref L_SelectCurves);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"=>SLA 304 左边 筛选靠近目标值报错了   \r\n{ex}", "Sound Check 算法提示");
            }
            try
            {
                selectAvg(RightTargetStr, ref R_Curves, ref R_SelectCurves);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"=>SLA 313 右边 筛选靠近目标值报错了   \r\n{ex}", "Sound Check 算法提示");
            }
        }


        void selectAvg(Dictionary<string, double[]> Target, ref Dictionary<double, _CurvesData> _Curves, ref Dictionary<double, _CurvesData> SelectCurves)
        {
            foreach (var item in _Curves)
                SelectCurves[item.Key] = item.Value;
            foreach (var item in Target)
            {
                Dictionary<double, double> LavgList = new Dictionary<double, double>();
                List<double> AvgsList = new List<double>();
                foreach (var Curves in SelectCurves)
                {
                    List<double> freqs = new List<double>();
                    foreach (var key in item.Key.Split('|'))
                        freqs.Add(double.Parse(key));
                    double avg = AvgTarget(Curves.Value, freqs);
                    LavgList[Curves.Key] = avg;
                    AvgsList.Add(avg);
                }
                int[] indexs = ScreenApproachFigure(AvgsList.ToArray(), item.Value[0], item.Value[1]);
                List<double> volues = new List<double>();
                foreach (var key in indexs)
                {
                    var v = SelectCurves.ElementAt(key);
                    SelectCurves[v.Key] = v.Value;
                    volues.Add(v.Key);
                }
                SelectCurves.Clear();
                Console.WriteLine($"Target : {item}");

                foreach (var x in volues)
                {
                    SelectCurves[x] = _Curves[x];
                    Console.WriteLine($@"{x.ToString().PadLeft(5, ' ')}   :  {LavgList[x].ToString().PadLeft(5, ' ')}");
                }
                Console.WriteLine("####################################################################################");
            }
        }




        /// <summary>
        /// 计算左右耳曲线
        /// </summary>
        /// <param name="info"></param>
        /// <param name="L_Cardinal"></param>
        /// <param name="R_Cardinal"></param>
        /// <param name="L_NexGainCurves"></param>
        /// <param name="R_NexGainCurves"></param>
        void CalculateAllCurve(CurvesInfo.info info,
        Dictionary<double, double[]> L_Cardinal,
        Dictionary<double, double[]> R_Cardinal,
        out Dictionary<double, _CurvesData> L_NexGainCurves,
        out Dictionary<double, _CurvesData> R_NexGainCurves)
        {
            L_NexGainCurves = new Dictionary<double, _CurvesData>();
            R_NexGainCurves = new Dictionary<double, _CurvesData>();
            try
            {


                foreach (KeyValuePair<double, double[]> AdjustValue in L_Cardinal)
                {
                    double[] CardinalCurves = AdjustValue.Value;
                    double[] NexYdata = new double[CardinalCurves.Length];
                    double[] NexXdata = new double[CardinalCurves.Length];
                    for (int i = 0; i < CardinalCurves.Length; i++)
                    {
                        NexYdata[i] = (info.L_Curves.Ydata[i] - CardinalCurves[i]).Round(2);
                        NexXdata[i] = info.L_Curves.Xdata[i].Round(2);
                    }
                    if (AdjustValue.Key == 0)
                    {

                    }
                    L_NexGainCurves.Add(AdjustValue.Key,
                        new _CurvesData($"L_Gain:{AdjustValue.Key}", NexXdata, NexYdata));
                }

                foreach (KeyValuePair<double, double[]> AdjustValue in R_Cardinal)
                {
                    double[] CardinalCurves = AdjustValue.Value;
                    double[] NexYdata = new double[CardinalCurves.Length];
                    double[] NexXdata = new double[CardinalCurves.Length];
                    for (int i = 0; i < CardinalCurves.Length; i++)
                    {
                        NexYdata[i] = (info.R_Curves.Ydata[i] - CardinalCurves[i]).Round(2); ;
                        NexXdata[i] = info.R_Curves.Xdata[i].Round(2);
                    }
                    R_NexGainCurves.Add(AdjustValue.Key,
                        new _CurvesData($"R_Gain:{AdjustValue.Key}", NexXdata, NexYdata));
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"=>SLC 414 计算左右耳所有曲线报错了\r\n{ex}", "Sound Check 算法提示");
            }





        }



        /// <summary>
        /// 嵌入limit 筛选 Pass曲线
        /// </summary>
        /// <param name="info"></param>
        /// <param name="L_NotScreenAllCurves"></param>
        /// <param name="R_NotScreenAllCurves"></param>
        /// <param name="L_PassCurves"></param>
        /// <param name="R_PassCurves"></param>
        void ScreenPassCurves(CurvesInfo.info info,
            Dictionary<double, _CurvesData> L_NotScreenAllCurves,
            Dictionary<double, _CurvesData> R_NotScreenAllCurves,
           out Dictionary<double, _CurvesData> L_PassCurves,
           out Dictionary<double, _CurvesData> R_PassCurves)
        {
            L_PassCurves = new Dictionary<double, _CurvesData>();
            R_PassCurves = new Dictionary<double, _CurvesData>();


            try
            {
                double limitDiff = 0.4;

                _CurvesData L_UpLimitData = info.L_Curves_Uplimit;
                _CurvesData L_LowLimitData = info.L_Curves_Lowlimit;
                int L_UpindexStart = ScreenApproachFigure(info.L_Curves.Xdata, L_UpLimitData.Xdata[0]);
                int L_LowindexStart = ScreenApproachFigure(info.L_Curves.Xdata, L_LowLimitData.Xdata[0]);
                foreach (KeyValuePair<double, _CurvesData> AllCurves in L_NotScreenAllCurves)
                {

                    double[] NexYdata = AllCurves.Value.Ydata;
                    double[] NexXdata = AllCurves.Value.Xdata;
                    bool Pass = true;
                    for (int i = 0; i < L_UpLimitData.Xdata.Length; i++)
                    {
                        double nextValue = NexYdata[i + L_UpindexStart];
                        double nextXdata = NexXdata[i + L_UpindexStart];
                        double UpLimit = L_UpLimitData.Ydata[i] - limitDiff;
                        if (nextValue > UpLimit)
                            Pass = false;
                    }
                    for (int i = 0; i < L_LowLimitData.Xdata.Length; i++)
                    {
                        double nextValue = NexYdata[i + L_LowindexStart];
                        double nextXdata = NexXdata[i + L_LowindexStart];

                        double LowLimit = L_LowLimitData.Ydata[i] + limitDiff;
                        if (nextValue < LowLimit)
                            Pass = false;
                    }
                    if (Pass)
                        L_PassCurves.Add(AllCurves.Key, new _CurvesData($"L_Gain:{AllCurves.Key}", NexXdata, NexYdata));
                }

                _CurvesData R_UpLimitData = info.R_Curves_Uplimit;
                _CurvesData R_LowLimitData = info.R_Curves_Lowlimit;
                int R_UpindexStart = ScreenApproachFigure(info.L_Curves.Xdata, R_UpLimitData.Xdata[0]);
                int R_LowindexStart = ScreenApproachFigure(info.L_Curves.Xdata, R_LowLimitData.Xdata[0]);
                foreach (KeyValuePair<double, _CurvesData> AllCurves in R_NotScreenAllCurves)
                {

                    double[] NexYdata = AllCurves.Value.Ydata;
                    double[] NexXdata = AllCurves.Value.Xdata;
                    bool Pass = true;
                    for (int i = 0; i < R_UpLimitData.Xdata.Length; i++)
                    {
                        double nextValue = NexYdata[i + R_UpindexStart];
                        double nextXdata = NexXdata[i + R_UpindexStart];
                        double UpLimit = R_UpLimitData.Ydata[i] - limitDiff;
                        if (nextValue > UpLimit)
                            Pass = false;
                    }
                    for (int i = 0; i < R_LowLimitData.Xdata.Length; i++)
                    {
                        double nextValue = NexYdata[i + R_UpindexStart];
                        double nextXdata = NexXdata[i + R_UpindexStart];
                        double LowLimit = R_LowLimitData.Ydata[i] + limitDiff;
                        if (nextValue < LowLimit)
                            Pass = false;
                    }
                    if (Pass)
                        R_PassCurves.Add(AllCurves.Key, new _CurvesData($"R_Gain:{AllCurves.Key}", NexXdata, NexYdata));
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"=>SLS 506 嵌入limit筛选左右耳曲线异常\r\n“{ex}”", "Sound Check 算法提示");

            }

        }




        /// <summary>
        /// 选择目标的接近值
        /// </summary>
        /// <param name="ListFigure"></param>
        /// <param name="ApproachFigure"></param>
        /// <returns></returns>
        int ScreenApproachFigure(double[] ListFigure, double ApproachFigure)
        {
            List<double> list1 = new List<double>();
            for (int i = 0; i < ListFigure.Length; i++)
            {
                list1.Add((ListFigure[i] - ApproachFigure).Round(2).abs());
            }
            int Index = Array.IndexOf(list1.ToArray(), list1.Min());
            return Index;

        }
        int[] ScreenApproachFigure(double[] ListFigure, double ApproachFigure, double range)
        {
            List<double> list1 = new List<double>();
            List<int> indexs = new List<int>();
            for (int i = 0; i < ListFigure.Length; i++)
            {
                list1.Add((ListFigure[i] - ApproachFigure).Round(2).abs());
            }
            double absRange = list1.Min() + range;
            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] <= absRange)
                    indexs.Add(i);
            }

            return indexs.ToArray();

        }

    }
}

