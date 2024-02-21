using MerryTest.testitem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Airoha.AdjustANC
{
   
    public class Adjust
    {

        static string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public void ReadCardinal(string TypeName, bool isFB)
        {
            if (isFB)
            {
                if (Cardinal.L_FB_CurveGianCardinal != null && Cardinal.R_FB_CurveGianCardinal != null)
                    return;
                string L = $@"{dllPath}\{TypeName}\L_FB_Curve_data.txt";
                if (!File.Exists(L))
                    throw new Exception($@"未在 Airoha_ANC\{TypeName}找到校准基数文件 {Path.GetFileName(L)} False".Show());
                string R = $@"{dllPath}\{TypeName}\R_FB_Curve_data.txt";
                if (!File.Exists(R))
                    throw new Exception($@"未在 Airoha_ANC\{TypeName}找到校准基数文件 {Path.GetFileName(R)} False".Show());
                Cardinal.L_FB_CurveGianCardinal = adjustSplit(File.ReadAllLines(L));
                Cardinal.R_FB_CurveGianCardinal = adjustSplit(File.ReadAllLines(R));
            }
            else
            {
                if (Cardinal.L_FF_CurveGianCardinal != null && Cardinal.R_FF_CurveGianCardinal != null)
                    return;
                string L = $@"{dllPath}\{TypeName}\L_FF_Curve_data.txt";
                if (!File.Exists(L))
                    throw new Exception($@"未在 Airoha_ANC\{TypeName}找到校准基数文件 {Path.GetFileName(L)} False".Show());
                string R = $@"{dllPath}\{TypeName}\R_FF_Curve_data.txt";
                if (!File.Exists(R))
                    throw new Exception($@"未在 Airoha_ANC\{TypeName}找到校准基数文件 {Path.GetFileName(R)} False".Show());
                Cardinal.L_FF_CurveGianCardinal = adjustSplit(File.ReadAllLines(L));
                Cardinal.R_FF_CurveGianCardinal = adjustSplit(File.ReadAllLines(R));
            }

        }
        Dictionary<double, Dictionary<double, double[]>> adjustSplit(string[] Data)
        {
            Dictionary<double, Dictionary<double, double[]>> DicData = new Dictionary<double, Dictionary<double, double[]>>();
            try
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    if (Data[i].Trim().Length > 1)
                    {
                        try
                        {
                            string[] Str = Data[i].Split(',');
                            double[] Ydata = new double[Str.Length - 2];
                            double ID = Convert.ToDouble(Str[0].Split('&')[0]).Round(2);
                            double IID = Convert.ToDouble(Str[0].Split('&')[1]).Round(2);



                            if (!DicData.ContainsKey(ID))
                            {
                                DicData.Add(ID, new Dictionary<double, double[]>());
                            }

                            for (int j = 1; j < Str.Length - 1; j++)
                            {

                                Ydata[j - 1] = Convert.ToDouble(Str[j]).Round(2);

                            }
                            DicData[ID][IID] = Ydata;
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                $"=> 0009 读取校准基数异常\r\n{ex}".Show();
            }

            return DicData;

        }

















        //################################  平衡优先  //################################
        #region 平衡优先

        public string ANCAdjust_ForCurves(string FilePath, bool ShowFroms, double MaxDiff, string isFB,

            ref double LeftGain, ref double RightGain
           )
        {
            try
            {




                CurvesInfo.info info = new CurvesInfo.info();
                //读取数据
                ReadFB_SCData(FilePath, isFB, ref info);
                //补充缺失的limit
                SupplementLimit(ref info);
                Dictionary<double, double[]> L_Cardinal = null;
                Dictionary<double, double[]> R_Cardinal = null;
                if (isFB == "FB")
                {
                    L_Cardinal = Cardinal.L_FB_CurveGianCardinal[Math.Round((LeftGain), 1)];
                    R_Cardinal = Cardinal.R_FB_CurveGianCardinal[Math.Round((RightGain), 1)];
                }
                else
                {
                    L_Cardinal = Cardinal.L_FF_CurveGianCardinal[Math.Round((LeftGain), 1)];
                    R_Cardinal = Cardinal.R_FF_CurveGianCardinal[Math.Round((RightGain), 1)];
                }


                //不需要校准 符合上下限 感差小于2
                string CheckCurvesResult = CheckCurves(info, MaxDiff);
                if (CheckCurvesResult.Contains("Not Adjust True"))
                    return "Not Adjust True";


                _CurvesData NextDiffCurves = null;
                //计算左右耳所有曲线
                CalculateAllCurve(info,
                    L_Cardinal,
                    R_Cardinal,
                    out Dictionary<double, _CurvesData> L_NotScreenAllCurves,
                    out Dictionary<double, _CurvesData> R_NotScreenAllCurves);


                //嵌入limit 筛选可以Pass的曲线
                ScreenPassCurves(info,
                    L_NotScreenAllCurves,
                    R_NotScreenAllCurves,
                    out Dictionary<double, _CurvesData> L_PassCurves,
                    out Dictionary<double, _CurvesData> R_PassCurves);
                string logStr = "";
                StringBuilder adjustLog = new StringBuilder();
                foreach (var item in L_PassCurves)
                    logStr += $"{item.Key}，";
                adjustLog.AppendLine($"嵌入Limit L {isFB} Pass数量:{L_PassCurves.Count} // :{logStr}\r\n");
                logStr = "";
                foreach (var item in R_PassCurves)
                    logStr += $"{item.Key}，";
                adjustLog.AppendLine($"嵌入Limit R {isFB} Pass数量:{R_PassCurves.Count} // :{logStr}\r\n");

                bool LisPass = L_PassCurves.Count > 0;
                bool RisPass = R_PassCurves.Count > 0;
                bool DiffPass = false;


                //当左右耳都符合规格的曲线时就符合规格的曲线筛选感差
                if (LisPass && RisPass)
                {
                    adjustLog.AppendLine($"左右耳都有Pass的曲线\r\n");
                    //传入limit 筛选感度感差
                    bool ScreenDiffFlag = ScreenDiffPassCurves(info, L_PassCurves, R_PassCurves, ref NextDiffCurves, out string Gain);
                    if (ScreenDiffFlag)
                    {
                        DiffPass = true;
                        adjustLog.AppendLine($"嵌入limit筛选感差曲线 OK {ScreenDiffFlag}：{Gain}\r\n");
                    }
                    //当所有平衡感差超limit时
                    else
                    {
                        //就筛选最平衡的曲线
                        CalculateAllDiffCurves(info,
                            L_PassCurves,
                            R_PassCurves,
                            ref NextDiffCurves);

                        adjustLog.AppendLine($"嵌入limit筛选感差曲线 NG {false}，筛选Pass中最平衡 筛选：{NextDiffCurves.CurveName}\r\n");

                    }
                }
                //当只有左耳或者右耳符合规格时
                else
                {
                    //当只有左耳符合规格
                    if (LisPass)
                    {
                        //传入 L符合规格的 ，传入R 全部曲线 筛选最合适的曲线
                        CalculateAllDiffCurves(info,
                           L_PassCurves,
                           R_NotScreenAllCurves,
                           ref NextDiffCurves);
                        adjustLog.AppendLine($"左耳OK 右耳 NG 筛选左耳OK右耳NG组合平衡：{NextDiffCurves.CurveName}\r\n");

                    }
                    //当只有右耳符合规格
                    if (RisPass)
                    {
                        //传入 R符合规格的 ，传入L 全部曲线 筛选最合适的曲线

                        CalculateAllDiffCurves(info,
                       L_NotScreenAllCurves,
                       R_PassCurves,
                       ref NextDiffCurves);
                        adjustLog.AppendLine($"左耳NG 右耳OK 筛选右耳OK左耳NG组合平衡：{NextDiffCurves.CurveName}\r\n");
                    }
                    if (!LisPass && !RisPass)
                    {
                        //计算感差曲线
                        CalculateAllDiffCurves(info,
                            L_NotScreenAllCurves,
                            R_NotScreenAllCurves,
                            ref NextDiffCurves);
                        adjustLog.AppendLine($"左右耳 NG 筛选最平衡：{NextDiffCurves.CurveName} \r\n");

                    }
                }
                string[] Next = NextDiffCurves.CurveName.Split('&');
                LeftGain = double.Parse(Next[0]).Round(1);
                RightGain = double.Parse(Next[1]).Round(1);
                if (ShowFroms)
                {
                    Task.Run(() =>
                    {
                        _CurvesData L_Curves = L_NotScreenAllCurves[double.Parse(Next[0]).Round(1)];
                        _CurvesData R_Curves = R_NotScreenAllCurves[double.Parse(Next[1]).Round(1)];
                        L_Curves.CurveName = $"L_{L_Curves.CurveName}";
                        R_Curves.CurveName = $"R_{R_Curves.CurveName}";
                        L_Curves._Color = Color.Yellow;
                        R_Curves._Color = Color.Blue;
                        NextDiffCurves._Color = LisPass ? Color.LimeGreen : Color.Red;


                        CurvesForms forms = new CurvesForms(3, "平衡优先");
                        forms.lb_L.BackColor = LisPass ? Color.LightGreen : Color.Red;
                        forms.lb_R.BackColor = RisPass ? Color.LightGreen : Color.Red;
                        forms.lb_Diff.BackColor = DiffPass ? Color.LightGreen : Color.Red;

                        forms.AddCurves_(info.L_Curves_Uplimit);
                        forms.AddCurves_(info.L_Curves_Lowlimit);
                        forms.AddDiffCurves(info.Balance_Curves_Uplimit);
                        forms.AddDiffCurves(info.Balance_Curves_Lowlimit);
                        forms.AddCurves_(L_Curves);
                        forms.AddCurves_(R_Curves);
                        forms.AddDiffCurves(NextDiffCurves);
                        forms.AddLog(adjustLog.ToString());
                        forms.ShowDialog();

                    });

                }
                return "True";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"=> 0010 数据异常，算法报错\r\n{ex}”", "算法异常");
                return $"{ex.Message} False";
            }


        }

        #endregion

        //################################  目标优先  //################################

        #region 目标优先

        public string SelectVolumeANCAdjust_ForCurves(string FilePath, bool ShowFroms, double MaxDiff, string isFB,

            string TargetS,

           ref double LeftGain, ref double RightGain
           )
        {
            try
            {
                CurvesInfo.info info = new CurvesInfo.info();
                //读取数据
                ReadFB_SCData(FilePath, isFB, ref info);
                //补充缺失的limit
                SupplementLimit(ref info);
                //不需要校准 符合上下限 感差小于2
                string CheckCurvesResult = CheckCurves(info, MaxDiff);
                Dictionary<double, double[]> L_Cardinal = null;
                Dictionary<double, double[]> R_Cardinal = null;
                double LgainDouble = Math.Round((LeftGain), 1);
                double RgainDouble = Math.Round((RightGain), 1);

                try
                {
                    if (isFB == "FB")
                    {
                        L_Cardinal = Cardinal.L_FB_CurveGianCardinal[LgainDouble];
                        R_Cardinal = Cardinal.R_FB_CurveGianCardinal[RgainDouble];
                    }
                    else
                    {
                        L_Cardinal = Cardinal.L_FF_CurveGianCardinal[LgainDouble];
                        R_Cardinal = Cardinal.R_FF_CurveGianCardinal[RgainDouble];
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"=> 0013 选择校准基数报错{LgainDouble}|{RgainDouble}\r\n{ex}”", "算法异常");

                    throw;
                }

                if (CheckCurvesResult.Contains("Not Adjust True"))
                    return "Not Adjust True";
                _CurvesData NextDiffCurves = null;
                //计算左右耳所有曲线
                CalculateAllCurve(info,
                    L_Cardinal,
                    R_Cardinal,
                    out Dictionary<double, _CurvesData> L_NotScreenAllCurves,
                    out Dictionary<double, _CurvesData> R_NotScreenAllCurves);

                //嵌入limit 筛选可以Pass的曲线
                ScreenPassCurves第二版(info,
                    L_NotScreenAllCurves,
                    R_NotScreenAllCurves,
                    out Dictionary<double, _CurvesData> L_PassCurves,
                    out Dictionary<double, _CurvesData> R_PassCurves);
                string logStr = "";
                StringBuilder adjustLog = new StringBuilder();
                foreach (var item in L_PassCurves)
                    logStr += $"{item.Key}，";
                adjustLog.AppendLine($"嵌入Limit L {isFB} Pass数量:{L_PassCurves.Count} // :{logStr}\r\n");
                logStr = "";
                foreach (var item in R_PassCurves)
                    logStr += $"{item.Key}，";
                adjustLog.AppendLine($"嵌入Limit R {isFB} Pass数量:{R_PassCurves.Count} // :{logStr}\r\n");

                bool LisPass = L_PassCurves.Count > 0;
                bool RisPass = R_PassCurves.Count > 0;
                bool DiffPass = false;
                List<string> Balance_PassGain = new List<string>();
                string NexGain = "Note Get Gain";
                Dictionary<string, _CurvesData> Balance_Curves = new Dictionary<string, _CurvesData>();
                adjustLog.AppendLine($"开始筛选     目标值：{TargetS}        \r\n");

                //当左右耳都符合规格的曲线时就符合规格的曲线筛选感差
                try
                {
                    if (LisPass && RisPass)
                    {
                        adjustLog.AppendLine($"左右耳都有Pass的曲线\r\n");
                        //传入limit 筛选感度感差
                        bool ScreenDiffFlag = ScreenBalancePass(info, L_PassCurves, R_PassCurves, out Balance_Curves, out Balance_PassGain);
                        if (ScreenDiffFlag)
                        {
                            ScreenTargetGain(TargetS, L_PassCurves, R_PassCurves, Balance_PassGain, out NexGain);
                            NextDiffCurves = Balance_Curves[NexGain];
                            DiffPass = true;

                        }
                        //当所有平衡感差超limit时
                        else
                        {
                            CalculateAllBalanceCurves(info, L_PassCurves, R_PassCurves, out Balance_Curves, out Balance_PassGain);
                            ScreenTargetGain(TargetS, L_PassCurves, R_PassCurves, Balance_PassGain, out NexGain);
                            NextDiffCurves = Balance_Curves[NexGain];
                            adjustLog.AppendLine($"嵌入limit筛选感差曲线 NG {false}，筛选Pass中靠近目标值 筛选：{NextDiffCurves.CurveName}\r\n");
                        }
                    }

                    //当只有左耳或者右耳符合规格时
                    else
                    {
                        //当只有左耳符合规格
                        if (LisPass)
                        {
                            //传入 L符合规格的 ，传入R 全部曲线 筛选最合适的曲线

                            CalculateAllBalanceCurves(info, L_PassCurves, R_NotScreenAllCurves, out Balance_Curves, out Balance_PassGain);
                            ScreenTargetGain(TargetS, L_PassCurves, R_NotScreenAllCurves, Balance_PassGain, out NexGain);
                            NextDiffCurves = Balance_Curves[NexGain];
                            adjustLog.AppendLine($"左耳OK 右耳 NG 筛选左耳OK右耳NG组合目标值：{NextDiffCurves.CurveName}\r\n");
                        }
                        //当只有右耳符合规格
                        if (RisPass)
                        {
                            //传入 R符合规格的 ，传入L 全部曲线 筛选最合适的曲线

                            CalculateAllBalanceCurves(info, L_NotScreenAllCurves, R_PassCurves, out Balance_Curves, out Balance_PassGain);
                            ScreenTargetGain(TargetS, L_NotScreenAllCurves, R_PassCurves, Balance_PassGain, out NexGain);
                            NextDiffCurves = Balance_Curves[NexGain];
                            adjustLog.AppendLine($"左耳NG 右耳OK 筛选右耳OK左耳NG组合目标值：{NextDiffCurves.CurveName}\r\n");
                        }
                        if (!LisPass && !RisPass)
                        {
                            //计算感差曲线
                            CalculateAllBalanceCurves(info, L_NotScreenAllCurves, R_NotScreenAllCurves, out Balance_Curves, out Balance_PassGain);
                            ScreenTargetGain(TargetS, L_NotScreenAllCurves, R_NotScreenAllCurves, Balance_PassGain, out NexGain);
                            NextDiffCurves = Balance_Curves[NexGain];
                            adjustLog.AppendLine($"左右耳 NG 筛选最目标值：{NextDiffCurves.CurveName} \r\n");

                        }
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"=> 0014 选择平衡的时候报错了 NexGain：{NexGain}|Balance_Curves:{Balance_Curves.Count}|Balance_PassGain:{Balance_PassGain.Count}\r\n{ex}”", "算法异常");

                }


                string[] Next = NextDiffCurves.CurveName.Split('&');
                LeftGain = double.Parse(Next[0]).Round(1);
                RightGain = double.Parse(Next[1]).Round(1);
                if (ShowFroms)
                {
                    Task.Run(() =>
                    {
                        _CurvesData L_Curves = L_NotScreenAllCurves[double.Parse(Next[0]).Round(1)];
                        _CurvesData R_Curves = R_NotScreenAllCurves[double.Parse(Next[1]).Round(1)];
                        L_Curves.CurveName = $"L_{L_Curves.CurveName}";
                        R_Curves.CurveName = $"R_{R_Curves.CurveName}";
                        L_Curves._Color = Color.Yellow;
                        R_Curves._Color = Color.Blue;
                        NextDiffCurves._Color = LisPass ? Color.LimeGreen : Color.Red;


                        CurvesForms forms = new CurvesForms(3, "目标优先");
                        forms.lb_L.BackColor = LisPass ? Color.LightGreen : Color.Red;
                        forms.lb_R.BackColor = RisPass ? Color.LightGreen : Color.Red;
                        forms.lb_Diff.BackColor = DiffPass ? Color.LightGreen : Color.Red;

                        forms.AddCurves_(info.L_Curves_Uplimit);
                        forms.AddCurves_(info.L_Curves_Lowlimit);
                        forms.AddDiffCurves(info.Balance_Curves_Uplimit);
                        forms.AddDiffCurves(info.Balance_Curves_Lowlimit);
                        forms.AddCurves_(L_Curves);
                        forms.AddCurves_(R_Curves);
                        forms.AddDiffCurves(NextDiffCurves);
                        forms.AddLog(adjustLog.ToString());
                        forms.ShowDialog();

                    });

                }
                return "True";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"=> 0001 数据异常，算法报错\r\n{ex}”", "算法异常");
                return $"{ex.Message} False";
            }


        }



        #endregion


        void ReadFB_SCData(string FilePath, string isFB, ref CurvesInfo.info info)
        {
            string txtPath = $@"{FilePath}\ANC Test Curves.txt";
            string[] curves; string L_Name = null; string R_Name = null; string Balance_Name = null;

            if (isFB == "FB")
            {
                txtPath = $@"{FilePath}\FB Test Curves.txt";
                L_Name = "FB FR_L(Delta)";
                R_Name = "FB FR_R(Delta)";
                Balance_Name = $"FB FR_Balance";

            }
            else if (isFB == "FF")
            {
                txtPath = $@"{FilePath}\ANC Test Curves.txt";

                L_Name = "ANC FR_L(Delta)";
                R_Name = "ANC FR_R(Delta)";
                Balance_Name = $"ANC FR_Balance";

            }

            for (int i = 0; i < 30; i++)
            {
                if (File.Exists(txtPath))
                    break;
                Thread.Sleep(100);

            }
            if (!File.Exists(txtPath))
                throw new Exception($"找不到SC的测试数据 {Path.GetFileName(txtPath)} False".Show());
            Thread.Sleep(100);
            curves = File.ReadAllLines(txtPath);
            for (int i = 0; i < curves.Length; i += 3)
            {
                string curveName = curves[i].Split(',')[0];
                string[] StrXdata = curves[i + 1].Split(',');
                List<double> _Xdata = new List<double>();

                for (int x = 0; x < StrXdata.Length; x++)
                {
                    if (StrXdata[x] == "") continue;
                    _Xdata.Add(double.Parse(StrXdata[x]));
                }

                string[] StrYdata = curves[i + 2].Split(',');
                List<double> _Ydata = new List<double>();
                for (int y = 0; y < StrYdata.Length; y++)
                {
                    if (StrYdata[y] == "") continue;
                    _Ydata.Add(double.Parse(StrYdata[y]));

                }
                _CurvesData _curves = curveName.Contains("Limit")
                    ? new _CurvesData(curveName, _Xdata.ToArray(), _Ydata.ToArray(), Color.Red)
                    : new _CurvesData(curveName, _Xdata.ToArray(), _Ydata.ToArray());



                if (curveName.Contains(L_Name))
                {
                    if (curveName.Contains("Upper Limit"))
                        info.L_Curves_Uplimit = _curves;
                    else if (curveName.Contains("Lower Limit"))
                        info.L_Curves_Lowlimit = _curves;
                    else
                        info.L_Curves = _curves;
                }
                else if (curveName.Contains(R_Name))
                {
                    if (curveName.Contains("Upper Limit"))
                        info.R_Curves_Uplimit = _curves;

                    else if (curveName.Contains("Lower Limit"))
                        info.R_Curves_Lowlimit = _curves;
                    else
                        info.R_Curves = _curves;
                }
                else if (curveName.Contains(Balance_Name))
                {
                    if (curveName.Contains("Upper Limit"))
                        info.Balance_Curves_Uplimit = _curves;
                    else if (curveName.Contains("Lower Limit"))
                        info.Balance_Curves_Lowlimit = _curves;
                    else
                        info.Balance_Curves = _curves;
                }

                else if (curveName.Contains("Difference"))
                {
                    if (curveName.Contains("Upper Limit"))
                        info.Balance_Curves_Uplimit = _curves;
                    else if (curveName.Contains("Lower Limit"))
                        info.Balance_Curves_Lowlimit = _curves;
                    else
                        info.Balance_Curves = _curves;
                }
                else if (curveName.Contains("Left"))
                {
                    if (curveName.Contains("Upper Limit"))
                        info.R_Curves_Uplimit = _curves;

                    else if (curveName.Contains("Lower Limit"))
                        info.R_Curves_Lowlimit = _curves;
                    else
                        info.R_Curves = _curves;
                }
                else if (curveName.Contains("Right"))
                {
                    if (curveName.Contains("Upper Limit"))
                        info.R_Curves_Uplimit = _curves;

                    else if (curveName.Contains("Lower Limit"))
                        info.R_Curves_Lowlimit = _curves;
                    else
                        info.R_Curves = _curves;
                }

                else
                {
                    MessageBox.Show($"测试数据曲线读取错误“{curveName}”", "算法异常");
                }
            }
        }

        string CheckCurves(CurvesInfo.info info, double MaxDiff)
        {
            bool ifCorves(_CurvesData Curves, _CurvesData UpLimit, _CurvesData LowLimit)
            {
                int indexStart = ScreenApproachFigure(Curves.Xdata, UpLimit.Xdata[0]);
                int indexEnd = ScreenApproachFigure(Curves.Xdata, UpLimit.Xdata[UpLimit.Xdata.Length - 1]);
                for (int i = 0; i < UpLimit.Ydata.Length; i++)
                {
                    if (UpLimit.Ydata[i] <= Curves.Ydata[i + indexStart] || LowLimit.Ydata[i] >= Curves.Ydata[i + indexStart])
                    {
                        return false;
                    }
                }

                return true;
            }
            bool ifDiffCorves(_CurvesData Curves, _CurvesData UpLimit, double MaxLimit)
            {
                int indexStart = Array.IndexOf(Curves.Xdata, UpLimit.Xdata[0]);
                int indexEnd = Array.IndexOf(Curves.Xdata, UpLimit.Xdata[UpLimit.Xdata.Count() - 1]);


                for (int i = indexStart; i < indexEnd; i++)
                {
                    if (Curves.Ydata[i].abs() > MaxLimit)
                    {
                        return false;
                    }
                }

                return true;
            }

            if (!ifCorves(info.L_Curves, info.L_Curves_Uplimit, info.L_Curves_Lowlimit)
                || !ifCorves(info.L_Curves, info.L_Curves_Uplimit, info.L_Curves_Lowlimit))
                return "False";
            if (!ifDiffCorves(info.Balance_Curves, info.L_Curves_Uplimit, MaxDiff))
                return "False";
            return "Not Adjust True";
        }
        /// <summary>
        /// 补充缺失的limit
        /// </summary>
        /// <param name="info"></param>
        void SupplementLimit(ref CurvesInfo.info info)
        {
            double[] SuppXdata = null;
            double[] SuppYdata = null;
            //    L    上限
            supplemen(info.L_Curves.Xdata, info.L_Curves_Uplimit.Xdata, info.L_Curves_Uplimit.Ydata, out SuppXdata, out SuppYdata);
            info.L_Curves_Uplimit.Xdata = SuppXdata;
            info.L_Curves_Uplimit.Ydata = SuppYdata;

            //    L    下限
            supplemen(info.L_Curves.Xdata, info.L_Curves_Lowlimit.Xdata, info.L_Curves_Lowlimit.Ydata, out SuppXdata, out SuppYdata);
            info.L_Curves_Lowlimit.Xdata = SuppXdata;
            info.L_Curves_Lowlimit.Ydata = SuppYdata;

            //    R    上限
            supplemen(info.R_Curves.Xdata, info.R_Curves_Uplimit.Xdata, info.R_Curves_Uplimit.Ydata, out SuppXdata, out SuppYdata);
            info.R_Curves_Uplimit.Xdata = SuppXdata;
            info.R_Curves_Uplimit.Ydata = SuppYdata;

            //    R    下限
            supplemen(info.R_Curves.Xdata, info.R_Curves_Lowlimit.Xdata, info.R_Curves_Lowlimit.Ydata, out SuppXdata, out SuppYdata);
            info.R_Curves_Lowlimit.Xdata = SuppXdata;
            info.R_Curves_Lowlimit.Ydata = SuppYdata;

            //    Balance    上限
            supplemen(info.Balance_Curves.Xdata, info.Balance_Curves_Uplimit.Xdata, info.Balance_Curves_Uplimit.Ydata, out SuppXdata, out SuppYdata);
            info.Balance_Curves_Uplimit.Xdata = SuppXdata;
            info.Balance_Curves_Uplimit.Ydata = SuppYdata;

            //    Balance    下限
            supplemen(info.Balance_Curves.Xdata, info.Balance_Curves_Lowlimit.Xdata, info.Balance_Curves_Lowlimit.Ydata, out SuppXdata, out SuppYdata);
            info.Balance_Curves_Lowlimit.Xdata = SuppXdata;
            info.Balance_Curves_Lowlimit.Ydata = SuppYdata;


        }
        void supplemen(double[] DataXdata, double[] LimitXdata, double[] LimitYdata, out double[] SuppXdata, out double[] SuppYdata)
        {
            double[] L_CurvesXdata = DataXdata; ;
            List<double> suppXLimit = new List<double>();
            List<double> suppYLimit = new List<double>();
            SuppXdata = null;
            SuppYdata = null;
            try
            {
                for (int i = 0; i < LimitXdata.Length - 1; i++)
                {
                    int index_1 = ScreenApproachFigure(L_CurvesXdata, LimitXdata[i]);
                    int index_2 = ScreenApproachFigure(L_CurvesXdata, LimitXdata[i + 1]);
                    for (int x = index_1; x < index_2; x++)
                    {
                        if (!suppXLimit.Contains(L_CurvesXdata[x]))
                            suppXLimit.Add(L_CurvesXdata[x]);
                    }
                    double count = (index_2 - index_1);
                    double Diff = (LimitYdata[i] - LimitYdata[i + 1]).Round(3);
                    double AvgDiff = (Diff / count).Round(3);

                    for (int x = 0; x < count; x++)
                    {
                        suppYLimit.Add((LimitYdata[i] - AvgDiff * x).Round(2));
                    }
                    if (LimitXdata.Length - 1 == i + 1)
                    {
                        suppXLimit.Add(L_CurvesXdata[index_2]);
                        suppYLimit.Add(LimitYdata[i + 1]);

                    }
                }
                SuppXdata = suppXLimit.ToArray();
                SuppYdata = suppYLimit.ToArray();
            }
            catch (Exception ex)
            {

                MessageBox.Show("=> 0012 上下限补点报错了\r\n" + ex.Message);
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
                        NexYdata[i] = (info.L_Curves.Ydata[i] - CardinalCurves[i]);
                        NexXdata[i] = info.L_Curves.Xdata[i];
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
                        NexYdata[i] = (info.R_Curves.Ydata[i] - CardinalCurves[i]);
                        NexXdata[i] = info.R_Curves.Xdata[i];
                    }
                    R_NexGainCurves.Add(AdjustValue.Key,
                        new _CurvesData($"R_Gain:{AdjustValue.Key}", NexXdata, NexYdata));
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"=> 0002 计算左右耳所有曲线报错了\r\n“{ex.StackTrace}”", "算法异常");
            }





        }

        /// <summary>
        /// 不嵌入limit 筛选平衡曲线
        /// </summary>
        /// <param name="info"></param>
        /// <param name="L_AllCurves"></param>
        /// <param name="R_AllCurves"></param>
        /// <param name="curves"></param>
        void CalculateAllDiffCurves(CurvesInfo.info info,
            Dictionary<double, _CurvesData> L_AllCurves,
            Dictionary<double, _CurvesData> R_AllCurves,
              ref _CurvesData curves)
        {
            try
            {
                int StartCurvesIndex = ScreenApproachFigure(info.Balance_Curves.Xdata, info.Balance_Curves_Uplimit.Xdata[0]);
                int EndCurvesIndex = ScreenApproachFigure(info.Balance_Curves.Xdata, info.Balance_Curves_Uplimit.Xdata[1]);

                Dictionary<string, _CurvesData> DifferenceCurves = new Dictionary<string, _CurvesData>();
                Dictionary<string, double[]> asbDifference = new Dictionary<string, double[]>();
                Dictionary<string, double> diffMaxValue = new Dictionary<string, double>();

                foreach (var LpassData in L_AllCurves)
                {
                    double[] L_Ydata = LpassData.Value.Ydata;
                    double[] L_Xdata = LpassData.Value.Xdata;
                    foreach (var RpassData in R_AllCurves)
                    {
                        double[] R_Ydata = RpassData.Value.Ydata;
                        double[] R_Xdata = RpassData.Value.Xdata;
                        string DiffName = $"{LpassData.Key}&{RpassData.Key}";
                        double[] DiffYdata = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                        double[] DiffXdata = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                        double[] DiffAsb = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                        for (int i = 0; i < DiffYdata.Length; i++)
                        {

                            double L = L_Ydata[StartCurvesIndex + i];
                            double R = R_Ydata[StartCurvesIndex + i];
                            double Xdata = LpassData.Value.Xdata[StartCurvesIndex + i];
                            DiffYdata[i] = (L - R).Round(2);
                            DiffXdata[i] = Xdata;
                            DiffAsb[i] = DiffYdata[i].abs();
                        }

                        DifferenceCurves.Add(DiffName, new _CurvesData(DiffName, DiffXdata, DiffYdata));
                        asbDifference[DiffName] = DiffAsb;
                        diffMaxValue[DiffName] = DiffAsb.Max();


                    }
                }

                string Gain = "NG";
                double MinValue = double.MaxValue;
                foreach (var item in diffMaxValue)
                {
                    if (item.Value < MinValue)
                    {
                        Gain = item.Key;
                        MinValue = item.Value;
                    }
                }
                if (DifferenceCurves.ContainsKey(Gain))
                {
                    curves = DifferenceCurves[Gain];
                }
                else
                {
                    MessageBox.Show($"计算所有感差曲线异常\r\n“{Gain}”", "算法异常");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"=> 0003 Error 计算所有感差曲线异常\r\n“{ex.StackTrace}”", "算法异常");
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
                _CurvesData L_UpLimitData = info.L_Curves_Uplimit;
                _CurvesData L_LowLimitData = info.L_Curves_Lowlimit;
                int L_CurvesIndex = Array.LastIndexOf(info.L_Curves.Xdata, L_UpLimitData.Xdata[0]);
                if (L_CurvesIndex < 0)
                    throw (new Exception("嵌入limit筛选数据错误，limit和数据频率不匹配"));
                foreach (KeyValuePair<double, _CurvesData> AllCurves in L_NotScreenAllCurves)
                {

                    double[] NexYdata = AllCurves.Value.Ydata;
                    double[] NexXdata = AllCurves.Value.Xdata;
                    bool Pass = true;
                    for (int i = 0; i < L_UpLimitData.Xdata.Length; i++)
                    {
                        double nextValue = NexYdata[i + L_CurvesIndex];
                        double nextXdata = NexXdata[i + L_CurvesIndex];
                        double UpLimit = L_UpLimitData.Ydata[i] - 0.2;
                        double LowLimit = L_LowLimitData.Ydata[i] + 0.2;
                        if (nextValue > UpLimit || nextValue < LowLimit)
                            Pass = false;
                    }
                    if (Pass)
                        L_PassCurves.Add(AllCurves.Key, new _CurvesData($"L_Gain:{AllCurves.Key}", NexXdata, NexYdata));
                }
                _CurvesData R_UpLimitData = info.R_Curves_Uplimit;
                _CurvesData R_LowLimitData = info.R_Curves_Lowlimit;
                int R_CurvesIndex = Array.LastIndexOf(info.R_Curves.Xdata, R_UpLimitData.Xdata[0]);
                if (R_CurvesIndex < 0)
                    throw (new Exception("嵌入limit筛选数据错误，limit和数据频率不匹配"));
                foreach (KeyValuePair<double, _CurvesData> AllCurves in R_NotScreenAllCurves)
                {

                    double[] NexYdata = AllCurves.Value.Ydata;
                    double[] NexXdata = AllCurves.Value.Xdata;
                    bool Pass = true;
                    for (int i = 0; i < R_UpLimitData.Xdata.Length; i++)
                    {
                        double nextValue = NexYdata[i + R_CurvesIndex];
                        double nextXdata = NexXdata[i + R_CurvesIndex];
                        double UpLimit = R_UpLimitData.Ydata[i] - 0.2;
                        double LowLimit = R_LowLimitData.Ydata[i] + 0.2;
                        if (nextValue > UpLimit || nextValue < LowLimit)
                            Pass = false;
                    }
                    if (Pass)
                        R_PassCurves.Add(AllCurves.Key, new _CurvesData($"R_Gain:{AllCurves.Key}", NexXdata, NexYdata));
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"=> 0004 嵌入limit筛选左右耳曲线异常\r\n“{ex.StackTrace}”", "算法异常");

            }

        }





        /// <summary>
        /// 嵌入limit 筛选最平衡曲线
        /// </summary>
        /// <param name="info"></param>
        /// <param name="L_PassCurves"></param>
        /// <param name="R_PassCurves"></param>
        /// <param name="curves"></param>
        /// <param name="Gain"></param>
        /// <returns></returns>
        bool ScreenDiffPassCurves(CurvesInfo.info info,
            Dictionary<double, _CurvesData> L_PassCurves,
            Dictionary<double, _CurvesData> R_PassCurves,
          ref _CurvesData curves, out string Gain)
        {

            _CurvesData Uplimit = info.Balance_Curves_Uplimit;
            _CurvesData LowLimit = info.Balance_Curves_Lowlimit;
            bool result = false;
            Gain = "NG";
            try
            {


                int StartCurvesIndex = ScreenApproachFigure(info.Balance_Curves.Xdata, Uplimit.Xdata[0]);
                int EndCurvesIndex = ScreenApproachFigure(info.Balance_Curves.Xdata, Uplimit.Xdata[1]);

                Dictionary<string, _CurvesData> DiffAllCurves = new Dictionary<string, _CurvesData>();
                Dictionary<string, double[]> asbDifference = new Dictionary<string, double[]>();
                Dictionary<string, double> diffMaxValue = new Dictionary<string, double>();

                foreach (var LpassData in L_PassCurves)
                {
                    double[] L_Ydata = LpassData.Value.Ydata;
                    double[] L_Xdata = LpassData.Value.Xdata;
                    foreach (var RpassData in R_PassCurves)
                    {
                        bool pass = true;
                        double[] R_Ydata = RpassData.Value.Ydata;
                        double[] R_Xdata = RpassData.Value.Xdata;
                        string DiffName = $"{LpassData.Key}&{RpassData.Key}";
                        double[] DiffYdata = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                        double[] DiffXdata = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                        double[] DiffAsb = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                        for (int i = 0; i < DiffYdata.Length; i++)
                        {

                            double L = L_Ydata[StartCurvesIndex + i];
                            double R = R_Ydata[StartCurvesIndex + i];

                            DiffYdata[i] = (L - R).Round(2);
                            DiffAsb[i] = DiffYdata[i].abs();
                            DiffXdata[i] = LpassData.Value.Xdata[StartCurvesIndex + i];

                            if (DiffAsb[i] > Uplimit.Ydata[0].abs())
                                pass = false;

                        }
                        if (pass)
                        {
                            result = true;
                            DiffAllCurves.Add(DiffName, new _CurvesData(DiffName, DiffXdata, DiffYdata));
                            asbDifference[DiffName] = DiffAsb;
                            diffMaxValue[DiffName] = DiffAsb.Max();
                        }

                    }
                }
                double MinValue = double.MaxValue;
                foreach (var item in diffMaxValue)
                {
                    if (item.Value < MinValue)
                    {
                        Gain = item.Key;
                        MinValue = item.Value;
                    }
                }
                if (DiffAllCurves.ContainsKey(Gain))
                {
                    curves = DiffAllCurves[Gain];
                }
                else
                {

                }

            }
            catch (Exception ex)
            {

                MessageBox.Show($"=> 0005 Error 嵌入limit筛选感差曲线报错\r\n“{ex.StackTrace}”", "算法异常");

            }
            return result;


        }




        /// <summary>
        /// 嵌入limit 筛选 Pass曲线
        /// </summary>
        /// <param name="info"></param>
        /// <param name="L_NotScreenAllCurves"></param>
        /// <param name="R_NotScreenAllCurves"></param>
        /// <param name="L_PassCurves"></param>
        /// <param name="R_PassCurves"></param>
        void ScreenPassCurves第二版(CurvesInfo.info info,
            Dictionary<double, _CurvesData> L_NotScreenAllCurves,
            Dictionary<double, _CurvesData> R_NotScreenAllCurves,
           out Dictionary<double, _CurvesData> L_PassCurves,
           out Dictionary<double, _CurvesData> R_PassCurves)
        {
            L_PassCurves = new Dictionary<double, _CurvesData>();
            R_PassCurves = new Dictionary<double, _CurvesData>();
            try
            {
                _CurvesData L_UpLimitData = info.L_Curves_Uplimit;
                _CurvesData L_LowLimitData = info.L_Curves_Lowlimit;

                foreach (KeyValuePair<double, _CurvesData> AllCurves in L_NotScreenAllCurves)
                {
                    bool upLlimitPass = true;
                    for (int i = 0; i < L_UpLimitData.Xdata.Length; i++)
                    {
                        int limitIndex = ScreenApproachFigure(AllCurves.Value.Xdata, L_UpLimitData.Xdata[i]);
                        double nextValue = AllCurves.Value.Ydata[limitIndex];
                        if (L_UpLimitData.Ydata[i] - 0.4 < nextValue)
                        {
                            upLlimitPass = false;
                        }
                    }
                    bool LowLlimitPass = true;

                    for (int i = 0; i < L_LowLimitData.Xdata.Length; i++)
                    {
                        int limitIndex = ScreenApproachFigure(AllCurves.Value.Xdata, L_LowLimitData.Xdata[i]);
                        double nextValue = AllCurves.Value.Ydata[limitIndex];

                        if (L_LowLimitData.Ydata[i] + 0.4 > nextValue)
                        {
                            LowLlimitPass = false;
                        }
                    }

                    if (upLlimitPass && LowLlimitPass)
                        L_PassCurves.Add(AllCurves.Key, new _CurvesData($"L_Gain:{AllCurves.Key}", AllCurves.Value.Xdata, AllCurves.Value.Ydata));
                }


                _CurvesData R_UpLimitData = info.R_Curves_Uplimit;
                _CurvesData R_LowLimitData = info.R_Curves_Lowlimit;

                foreach (KeyValuePair<double, _CurvesData> AllCurves in R_NotScreenAllCurves)
                {

                    bool upLlimitPass = true;
                    for (int i = 0; i < R_UpLimitData.Xdata.Length; i++)
                    {
                        int limitIndex = ScreenApproachFigure(AllCurves.Value.Xdata, R_UpLimitData.Xdata[i]);
                        double nextValue = AllCurves.Value.Ydata[limitIndex];
                        if (R_UpLimitData.Ydata[i] - 0.1 < nextValue)
                        {
                            upLlimitPass = false;
                        }
                    }
                    bool LowLlimitPass = true;

                    for (int i = 0; i < R_LowLimitData.Xdata.Length; i++)
                    {
                        int limitIndex = ScreenApproachFigure(AllCurves.Value.Xdata, R_LowLimitData.Xdata[i]);
                        double nextValue = AllCurves.Value.Ydata[limitIndex];

                        if (R_LowLimitData.Ydata[i] + 0.1 > nextValue)
                        {
                            LowLlimitPass = false;
                        }
                    }

                    if (upLlimitPass && LowLlimitPass)
                        R_PassCurves.Add(AllCurves.Key, new _CurvesData($"L_Gain:{AllCurves.Key}", AllCurves.Value.Xdata, AllCurves.Value.Ydata));


                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"=> 0004 嵌入limit筛选左右耳曲线异常\r\n“{ex.StackTrace}”", "算法异常");

            }

        }








        /// <summary>
        /// 嵌入limit 筛选感差Pass的感差Gain
        /// </summary>
        /// <param name="info"></param>
        /// <param name="L_PassCurves"></param>
        /// <param name="R_PassCurves"></param>
        /// <param name="Balance_PassCurves"></param>
        /// <param name="Balance_PassGain"></param>
        /// 
        /// <returns></returns>
        bool ScreenBalancePass(CurvesInfo.info info,
            Dictionary<double, _CurvesData> L_PassCurves,
            Dictionary<double, _CurvesData> R_PassCurves,
           out Dictionary<string, _CurvesData> Balance_PassCurves,
           out List<string> Balance_PassGain)
        {
            _CurvesData Uplimit = info.Balance_Curves_Uplimit;
            _CurvesData LowLimit = info.Balance_Curves_Lowlimit;
            Balance_PassGain = new List<string>();
            bool result = false;
            Balance_PassCurves = new Dictionary<string, _CurvesData>();
            try
            {



                Dictionary<string, _CurvesData> DiffAllCurves = new Dictionary<string, _CurvesData>();
                Dictionary<string, double> diffMaxValue = new Dictionary<string, double>();

                foreach (var LpassData in L_PassCurves)
                {
                    double[] L_Ydata = LpassData.Value.Ydata;
                    double[] L_Xdata = LpassData.Value.Xdata;
                    foreach (var RpassData in R_PassCurves)
                    {
                        string DiffName = $"{LpassData.Key}&{RpassData.Key}";
                        double[] R_Ydata = RpassData.Value.Ydata;
                        double[] R_Xdata = RpassData.Value.Xdata;



                        int UpStartCurvesIndex = ScreenApproachFigure(L_Xdata, Uplimit.Xdata[0]);
                        int UpwEndCurvesIndex = ScreenApproachFigure(L_Xdata, Uplimit.Xdata[Uplimit.Xdata.Length - 1]);
                        double[] DiffYdata = new double[UpwEndCurvesIndex + 1 - UpStartCurvesIndex];
                        double[] DiffXdata = new double[UpwEndCurvesIndex + 1 - UpStartCurvesIndex];
                        double[] DiffAsb = new double[UpwEndCurvesIndex + 1 - UpStartCurvesIndex];
                        bool pass = true;

                        for (int i = UpStartCurvesIndex; i <= UpwEndCurvesIndex; i++)
                        {
                            double L = L_Ydata[i];
                            double R = R_Ydata[i];
                            int DiffIndex = i - UpStartCurvesIndex;
                            DiffYdata[DiffIndex] = (L - R).Round(2);
                            DiffAsb[DiffIndex] = DiffYdata[DiffIndex].abs();
                            DiffXdata[DiffIndex] = L_Xdata[i];
                            if (!(DiffYdata[DiffIndex] < Uplimit.Ydata[DiffIndex] - 0.6 && DiffYdata[DiffIndex] > LowLimit.Ydata[DiffIndex] + 0.6))
                                pass = false;
                        }
                        if (pass)
                        {
                            result = true;
                            DiffAllCurves.Add(DiffName, new _CurvesData(DiffName, DiffXdata, DiffYdata));
                            Balance_PassGain.Add(DiffName);
                            diffMaxValue[DiffName] = DiffAsb.Max();
                        }

                    }
                }

                Balance_PassCurves = DiffAllCurves;


            }
            catch (Exception ex)
            {

                MessageBox.Show($"=> 0006 Error 嵌入limit筛选感差曲线报错\r\n“{ex.StackTrace}”", "算法异常");

            }
            return result;


        }

        /// <summary>
        ///   计算感差曲线
        /// </summary>
        /// <param name="info"></param>
        /// <param name="L_PassCurves"></param>
        /// <param name="R_PassCurves"></param>
        /// <param name="Balance_Curves"></param>
        /// <param name="Balance_PassGain"></param>
        /// 
        /// <returns></returns>
        void CalculateAllBalanceCurves(CurvesInfo.info info,
            Dictionary<double, _CurvesData> L_PassCurves,
            Dictionary<double, _CurvesData> R_PassCurves,
           out Dictionary<string, _CurvesData> Balance_Curves,
           out List<string> Balance_Gain)
        {
            _CurvesData Uplimit = info.Balance_Curves_Uplimit;
            _CurvesData LowLimit = info.Balance_Curves_Lowlimit;
            Balance_Gain = new List<string>();
            bool result = false;
            Balance_Curves = new Dictionary<string, _CurvesData>();
            try
            {

                int StartCurvesIndex = ScreenApproachFigure(info.Balance_Curves.Xdata, Uplimit.Xdata[0]);
                int EndCurvesIndex = ScreenApproachFigure(info.Balance_Curves.Xdata, Uplimit.Xdata[1]);

                Dictionary<string, _CurvesData> DiffAllCurves = new Dictionary<string, _CurvesData>();
                Dictionary<string, double[]> asbDifference = new Dictionary<string, double[]>();
                Dictionary<string, double> diffMaxValue = new Dictionary<string, double>();

                foreach (var LpassData in L_PassCurves)
                {
                    double[] L_Ydata = LpassData.Value.Ydata;
                    double[] L_Xdata = LpassData.Value.Xdata;
                    foreach (var RpassData in R_PassCurves)
                    {
                        bool pass = true;
                        double[] R_Ydata = RpassData.Value.Ydata;
                        double[] R_Xdata = RpassData.Value.Xdata;
                        string DiffName = $"{LpassData.Key}&{RpassData.Key}";
                        double[] DiffYdata = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                        double[] DiffXdata = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                        double[] DiffAsb = new double[EndCurvesIndex + 1 - StartCurvesIndex];
                        for (int i = 0; i < DiffYdata.Length; i++)
                        {
                            double L = L_Ydata[StartCurvesIndex + i];
                            double R = R_Ydata[StartCurvesIndex + i];
                            DiffYdata[i] = (L - R).Round(2);
                            DiffAsb[i] = DiffYdata[i].abs();
                            DiffXdata[i] = LpassData.Value.Xdata[StartCurvesIndex + i];
                            if (DiffAsb[i] > Uplimit.Ydata[0].abs())
                                pass = false;

                        }

                        result = true;
                        DiffAllCurves.Add(DiffName, new _CurvesData(DiffName, DiffXdata, DiffYdata));
                        Balance_Gain.Add(DiffName);
                        asbDifference[DiffName] = DiffAsb;
                        diffMaxValue[DiffName] = DiffAsb.Max();


                    }
                }

                Balance_Curves = DiffAllCurves;


            }
            catch (Exception ex)
            {

                MessageBox.Show($"=> 0007 Error 计算感差曲线报错\r\n“{ex.StackTrace}”", "算法异常");

            }



        }

        void _ScreenTargetGain(double[] TargetFreq,
            double TargetVolume,
            Dictionary<double, _CurvesData> L_PassCurves,
            Dictionary<double, _CurvesData> R_PassCurves,
            List<string> Balance_PassGain, out string Gain)
        {
            Gain = "NG";
            try
            {
                Dictionary<double, double> Lgain = new Dictionary<double, double>();
                Dictionary<double, double> Rgain = new Dictionary<double, double>();

                foreach (var item in Balance_PassGain)
                {
                    string[] strs = item.Split('&');

                    double L = Convert.ToDouble(strs[0]).Round(1);
                    double R = Convert.ToDouble(strs[1]).Round(1);
                    if (R == -0.7)
                    {

                    }
                    if (!Lgain.ContainsKey(L))
                        Lgain.Add(L, 0);
                    if (!Rgain.ContainsKey(R))
                        Rgain.Add(R, 0);
                }
                List<double> Lvalue = new List<double>();
                for (int i = 0; i < Lgain.Count; i++)
                {
                    List<int> StartCurvesIndexs = new List<int>();
                    foreach (var item in TargetFreq)
                    {
                        StartCurvesIndexs.Add(ScreenApproachFigure(L_PassCurves.First().Value.Xdata, item));

                    }
                    double key = Lgain.ElementAt(i).Key;

                    double sum = 0;
                    foreach (var item in StartCurvesIndexs)
                    {
                        sum += L_PassCurves[key].Ydata[item].Round(1);
                    }
                    double avg = sum / StartCurvesIndexs.Count;
                    Lgain[key] = avg.Round(1);
                    Lvalue.Add(Lgain[key]);
                }
                List<double> Rvalue = new List<double>();

                for (int i = 0; i < Rgain.Count; i++)
                {
                    List<int> StartCurvesIndexs = new List<int>();
                    foreach (var item in TargetFreq)
                    {
                        StartCurvesIndexs.Add(ScreenApproachFigure(R_PassCurves.First().Value.Xdata, item));

                    }
                    double key = Rgain.ElementAt(i).Key;

                    double sum = 0;
                    foreach (var item in StartCurvesIndexs)
                    {
                        sum += R_PassCurves[key].Ydata[item].Round(1);
                    }
                    double avg = sum / StartCurvesIndexs.Count;
                    Rgain[key] = avg.Round(1);
                    Rvalue.Add(Rgain[key]);
                }

                int SelectLGain = ScreenApproachFigure(Lvalue.ToArray(), TargetVolume);
                int SelectRGain = ScreenApproachFigure(Rvalue.ToArray(), TargetVolume);
                Gain = $"{Lgain.ElementAt(SelectLGain).Key}&{Rgain.ElementAt(SelectRGain).Key}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"=> 0008 筛选靠近目标值报错了 目标值：{TargetVolume} 频率：{TargetFreq} Gain：{Gain}\r\n“{ex.StackTrace}”", "算法异常");


            }



        }
        void ScreenTargetGain(string TargetS,
            Dictionary<double, _CurvesData> L_Curves,
            Dictionary<double, _CurvesData> R_Curves,
            List<string> Balance_PassGain, out string Gain)
        {
            Gain = "NG";
            try
            {
                Dictionary<string, double> Ta = new Dictionary<string, double>();
                string[] x1 = TargetS.Replace("}", "").Split('{');
                foreach (var item in x1)
                {
                    if (!item.Contains(':'))
                        continue;
                    string[] x0 = item.Split(':');
                    Ta.Add(x0[0], double.Parse(x0[1]));
                }
                //先讲左边的目标值全部找到
                for (int cc = 0; cc < Ta.Count; cc++)
                {
                    string[] TargetFreq = Ta.ElementAt(cc).Key.Split(':');
                    double TargetValue = Ta.ElementAt(cc).Value;


                    Dictionary<string, double> Lgain = new Dictionary<string, double>();
                    List<double> LgainList = new List<double>();
                    List<int> LStartCurvesIndexs = new List<int>();

                    foreach (var Freqitem in TargetFreq)
                        LStartCurvesIndexs.Add(ScreenApproachFigure(L_Curves.First().Value.Xdata, double.Parse(Freqitem).Round(1)));


                    foreach (var Gainitem in Balance_PassGain)
                    {
                        string[] strs = Gainitem.Split('&');

                        double L = Convert.ToDouble(strs[0]).Round(1);


                        double sum = 0;
                        foreach (var Indexitem in LStartCurvesIndexs)
                        {
                            sum += L_Curves[L].Ydata[Indexitem].Round(1);
                        }
                        double avg = sum / LStartCurvesIndexs.Count;
                        //存起来
                        Lgain[Gainitem] = avg.Round(1);
                        //因为字典不容易筛选所以用list 数组
                        LgainList.Add(Lgain[Gainitem]);
                    }

                    //找到离目标值最近的索引
                    int[] LgainIndex = ScreenApproachFigure(LgainList.ToArray(), TargetValue, 0.2);
                    List<double> LValues = new List<double>();

                    foreach (var item in LgainIndex)
                        LValues.Add(Lgain.ElementAt(item).Value);

                    //将左边可能相同的目标不同的gain存起来

                    List<string> LPassGain = new List<string>();
                    foreach (var Value in LValues)
                    {
                        foreach (var item in Lgain)
                            if (item.Value == Value)
                                if (!LPassGain.Contains(item.Key))
                                    LPassGain.Add(item.Key);
                    }

                    //开始筛选右边
                    Dictionary<string, double> Rgain = new Dictionary<string, double>();
                    List<double> RgainList = new List<double>();

                    List<int> RStartCurvesIndexs = new List<int>();
                    foreach (var Freqitem in TargetFreq)
                        RStartCurvesIndexs.Add(ScreenApproachFigure(R_Curves.First().Value.Xdata, double.Parse(Freqitem).Round(1)));
                    foreach (var RGainitem in LPassGain)
                    {
                        string[] strs = RGainitem.Split('&');

                        double R = Convert.ToDouble(strs[1]).Round(1);



                        double sum = 0;
                        foreach (var Indexitem in RStartCurvesIndexs)
                            sum += R_Curves[R].Ydata[Indexitem].Round(1);
                        double avg = sum / RStartCurvesIndexs.Count;
                        //存起来
                        Rgain[RGainitem] = avg.Round(1);
                        //因为字典不容易筛选所以用list 数组
                        RgainList.Add(Rgain[RGainitem]);
                    }

                    //找到离目标值最近的索引
                    int[] RgainIndex = ScreenApproachFigure(RgainList.ToArray(), TargetValue, 0.2);


                    Dictionary<double, _CurvesData> SaveL_Curves = new Dictionary<double, _CurvesData>();
                    Dictionary<double, _CurvesData> SaveR_Curves = new Dictionary<double, _CurvesData>();
                    List<string> SaveBalance_PassGain = new List<string>();


                    foreach (var __index in RgainIndex)
                    {
                        var _V = Rgain.ElementAt(__index);
                        double Lvalue = double.Parse(_V.Key.Split('&')[0]);
                        double Rvalue = double.Parse(_V.Key.Split('&')[1]);




                        SaveBalance_PassGain.Add(_V.Key);
                        SaveL_Curves[Lvalue] = L_Curves[Lvalue];
                        SaveR_Curves[Rvalue] = R_Curves[Rvalue];
                    }
                    int GainIndex = ScreenApproachFigure(RgainList.ToArray(), TargetValue);
                    Gain = Rgain.ElementAt(GainIndex).Key;
                    L_Curves.Clear();
                    R_Curves.Clear();
                    Balance_PassGain.Clear();
                    L_Curves = SaveL_Curves;
                    R_Curves = SaveR_Curves;

                    Balance_PassGain = SaveBalance_PassGain;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"=> 0008 筛选靠近目标值报错了 目标值：{TargetS}  Gain：{Gain}\r\n“{ex.StackTrace}”", "算法异常");
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
