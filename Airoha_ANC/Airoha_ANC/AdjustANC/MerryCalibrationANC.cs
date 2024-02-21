using Airoha.AdjustANC;
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

namespace Airoha.AdjustANC
{
    public class MerryCalibrationANC
    {
        static string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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

        public Dictionary<string, CurvesInfo.info> infos = new Dictionary<string, CurvesInfo.info>();

        public void CheckInfos(bool ShowFroms,
        Dictionary<string, double[]> LeftTargetStr,
        Dictionary<string, double[]> RightTargetStr,
        ref double LeftGain, ref double RightGain
        )
        {
         
            Dictionary<double, _CurvesData> L_NotScreenAllCurves = new Dictionary<double, _CurvesData>();
            Dictionary<double, _CurvesData> R_NotScreenAllCurves = new Dictionary<double, _CurvesData>();
            CurvesInfo.info info = infos.First().Value;
            _CurvesData NextDiffCurves = null;
            StringBuilder adjustLog = new StringBuilder();
            bool DiffPass = false;
            foreach (var item in infos)
            {
                adjustLog.AppendLine($"{item.Key}");

                string[] Key = item.Key.Split(new char[] { '&' });
                L_NotScreenAllCurves[double.Parse(Key[0]).Round(1)] = item.Value.L_Curves;
                R_NotScreenAllCurves[double.Parse(Key[1]).Round(1)] = item.Value.R_Curves;
            }
            ScreenPassCurves(infos.First().Value, L_NotScreenAllCurves, R_NotScreenAllCurves, out Dictionary<double, _CurvesData> L_PassCurves, out Dictionary<double, _CurvesData> R_PassCurves);
            bool LisPass = L_PassCurves.Count > 0;
            bool RisPass = R_PassCurves.Count > 0;

            //当左右耳都符合规格的曲线时就符合规格的曲线筛选感差
            if (LisPass && RisPass)
            {
                ScreenTargetGain_2(LeftTargetStr, RightTargetStr, L_PassCurves, R_PassCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);
                DiffPass = ScreenDiffPassCurves(info, LCurves, RCurves, ref NextDiffCurves, out string Gain);
                if (DiffPass)
                {
                    adjustLog.AppendLine($"左耳OK 右耳 OK 筛选平衡OK组合=>A101：{NextDiffCurves.CurveName}\r\n");

                }
                else
                {
                    CalculateAllDiffCurves(info, L_PassCurves, R_PassCurves, ref NextDiffCurves);
                    adjustLog.AppendLine($"左耳OK 右耳 OK 筛选平衡NG组合=>A102：{NextDiffCurves.CurveName}\r\n");
                }

            }
            //当只有左耳或者右耳符合规格时
            else
            {

                //当只有左耳符合规格
                if (LisPass)
                {
                    ScreenTargetGain_2(LeftTargetStr, RightTargetStr, L_PassCurves, R_NotScreenAllCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);

                    CalculateAllDiffCurves(info, LCurves, RCurves, ref NextDiffCurves);
                    adjustLog.AppendLine($"左耳OK 右耳 NG 筛选平衡=>A103：{NextDiffCurves.CurveName}\r\n");

                }
                //当只有右耳符合规格
                if (RisPass)
                {
                    ScreenTargetGain_2(LeftTargetStr, RightTargetStr, L_NotScreenAllCurves, R_PassCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);
                    CalculateAllDiffCurves(info, LCurves, RCurves, ref NextDiffCurves);
                    adjustLog.AppendLine($"左耳NG 右耳 NG 筛选平衡=>A104：{NextDiffCurves.CurveName}\r\n");
                }
                if (!LisPass && !RisPass)
                {
                    ScreenTargetGain_2(LeftTargetStr, RightTargetStr, L_NotScreenAllCurves, R_NotScreenAllCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);
                    CalculateAllDiffCurves(info, LCurves, RCurves, ref NextDiffCurves);
                    adjustLog.AppendLine($"左耳NG 右耳 NG 筛选平衡=>A105：{NextDiffCurves.CurveName}\r\n");

                }
            }
            string[] Next = NextDiffCurves.CurveName.Split('&');
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

                    CurvesForms forms = new CurvesForms(6, "固定测试几次后最后的平衡");
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
        }



        #region 左边以目标，右边以平衡

        public string ANCAdjust_ForCurves(
            string FilePath, bool ShowFroms, string TypeName,
            Dictionary<string, double[]> LeftTargetStr,
            Dictionary<string, double[]> RightTargetStr,
            string isFB, out bool NGCurvesIf,
            ref double LeftGain, ref double RightGain
           )
        {
            NGCurvesIf = false;
            string lastGain = $"{LeftGain}&{RightGain}";
            Double LastGainL = LeftGain;
            Double LastGainR = RightGain;
            try
            {
                CurvesInfo.info info = new CurvesInfo.info();
                //读取数据
                ReadFB_SCData(FilePath, isFB, ref info);

                //补充缺失的limit
                SupplementLimit(ref info);
                infos[lastGain] = info;
                //拿到校准值
                Out_Cardinal(out Dictionary<double, double[]> L_Cardinal, out Dictionary<double, double[]> R_Cardinal, isFB, LeftGain, RightGain, TypeName);
                //检测曲线当前状态
                string CheckCurvesResult = _CheckCurves(info, LeftTargetStr, RightTargetStr);
                NGCurvesIf = (CheckCurvesResult == "Rising Limit False");

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
                adjustLog.AppendLine($"左耳目标：{LeftTargetStr.dicToTarget()}  右耳目标：{RightTargetStr.dicToTarget()}\r\n");

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
                    ScreenTargetGain_2(LeftTargetStr, RightTargetStr, L_PassCurves, R_PassCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);
                    DiffPass = ScreenDiffPassCurves(info, LCurves, RCurves, ref NextDiffCurves, out string Gain);
                    if (DiffPass)
                    {
                        adjustLog.AppendLine($"左耳OK 右耳 OK 筛选平衡OK组合=>A101：{NextDiffCurves.CurveName}\r\n");

                    }
                    else
                    {
                        CalculateAllDiffCurves(info, L_PassCurves, R_PassCurves, ref NextDiffCurves);
                        adjustLog.AppendLine($"左耳OK 右耳 OK 筛选平衡NG组合=>A102：{NextDiffCurves.CurveName}\r\n");
                    }

                }
                //当只有左耳或者右耳符合规格时
                else
                {

                    //当只有左耳符合规格
                    if (LisPass)
                    {
                        ScreenTargetGain_2(LeftTargetStr, RightTargetStr, L_PassCurves, R_NotScreenAllCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);

                        CalculateAllDiffCurves(info, LCurves, RCurves, ref NextDiffCurves);
                        adjustLog.AppendLine($"左耳OK 右耳 NG 筛选平衡=>A103：{NextDiffCurves.CurveName}\r\n");

                    }
                    //当只有右耳符合规格
                    if (RisPass)
                    {
                        ScreenTargetGain_2(LeftTargetStr, RightTargetStr, L_NotScreenAllCurves, R_PassCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);
                        CalculateAllDiffCurves(info, LCurves, RCurves, ref NextDiffCurves);
                        adjustLog.AppendLine($"左耳NG 右耳 NG 筛选平衡=>A104：{NextDiffCurves.CurveName}\r\n");
                    }
                    if (!LisPass && !RisPass)
                    {
                        ScreenTargetGain_2(LeftTargetStr, RightTargetStr, L_NotScreenAllCurves, R_NotScreenAllCurves, out Dictionary<double, _CurvesData> LCurves, out Dictionary<double, _CurvesData> RCurves);
                        CalculateAllDiffCurves(info, LCurves, RCurves, ref NextDiffCurves);
                        adjustLog.AppendLine($"左耳NG 右耳 NG 筛选平衡=>A105：{NextDiffCurves.CurveName}\r\n");

                    }
                }

                string[] Next = NextDiffCurves.CurveName.Split('&');

           
                LeftGain = double.Parse(Next[0]).Round(1);
                RightGain = double.Parse(Next[1]).Round(1);

                if (LastGainL == LeftGain && LastGainR == RightGain)
                {
                    NextDiffCurves = new _CurvesData("", new double[0], new double[0]);
                    LeftGain = LeftGain > 0 ? LeftGain - 0.1 : LeftGain + 0.1;
                    RightGain = RightGain > 0 ? RightGain - 0.1 : RightGain + 0.1;
                    Next[0] = $"{LeftGain}";
                    Next[1] = $"{RightGain}";

                }
                else
                {
                    LeftGain = double.Parse(Next[0]).Round(1);
                    RightGain = double.Parse(Next[1]).Round(1);
                }
                if (ShowFroms && lastGain != NextDiffCurves.CurveName)
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


                        CurvesForms forms = new CurvesForms(4, "目标平衡优先");
                        string str = $"Last: {lastGain} Next:{NextDiffCurves.CurveName}".Replace("&", "&&");
                        forms.lb_Gain.Text = str;
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







        void Out_Cardinal(out Dictionary<double, double[]> L_Cardinal, out Dictionary<double, double[]> R_Cardinal, string isFB, double LeftGain, double RightGain, string TypeName)
        {
            try
            {
  ReadCardinal(TypeName, isFB == "FB");

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"=> A0011 校准技术缺失产生异常\r\n{ex}”", "算法异常");
                throw ex;
            }
          
        }
        void ReadCardinal(string TypeName, bool isFB)
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

        string _CheckCurves(
            CurvesInfo.info info,
            Dictionary<string, double[]> LeftTargetStr,
            Dictionary<string, double[]> RightTargetStr)
        {

            bool L = ifCorves(info.L_Curves, info.L_Curves_Uplimit, info.L_Curves_Lowlimit);
            bool R = ifCorves(info.R_Curves, info.R_Curves_Uplimit, info.R_Curves_Lowlimit);
            bool Balance = ifCorves(info.Balance_Curves, info.Balance_Curves_Uplimit, info.Balance_Curves_Lowlimit);
            if (!L || !R || !Balance)
                return "Rising Limit False";

            foreach (var item in LeftTargetStr)
            {
                List<double> freqs = new List<double>();
                foreach (var key in item.Key.Split('|'))
                    freqs.Add(double.Parse(key));

                double Avg = AvgTarget(info.L_Curves, freqs);
                double diff = (Avg.abs() - item.Value[0].abs()).abs().Round(2);
                if (diff >= item.Value[1])
                    return "Left Exceed Region False";
            }
            foreach (var item in RightTargetStr)
            {
                List<double> freqs = new List<double>();
                foreach (var key in item.Key.Split('|'))
                    freqs.Add(double.Parse(key));
                double Avg = AvgTarget(info.R_Curves, freqs);
                double diff = (Avg.abs() - item.Value[0].abs()).abs().Round(2);
                if (diff >= item.Value[1])
                    return "Right Exceed Region False";
            }
            return "Adjust True";

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

        void ScreenTargetGain_2(
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
                Console.WriteLine("左边调试");
                selectAvg(LeftTargetStr, ref L_Curves, ref L_SelectCurves);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"=> 0008 筛选靠近目标值报错了 目标值：{LeftTargetStr.dicToTarget()} \r\n“{ex.StackTrace}”", "算法异常");
            }
            try
            {
                Console.WriteLine("右边调试");

                selectAvg(RightTargetStr, ref R_Curves, ref R_SelectCurves);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"=> 0009 筛选靠近目标值报错了 目标值：{RightTargetStr.dicToTarget()} \r\n“{ex.StackTrace}”", "算法异常");
            }
        }
        #endregion

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
                        info.L_Curves_Uplimit = _curves;

                    else if (curveName.Contains("Lower Limit"))
                        info.L_Curves_Lowlimit = _curves;
                    else
                        info.L_Curves = _curves;
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
            _CurvesData Uplimit = info.Balance_Curves_Uplimit;
            _CurvesData LowLimit = info.Balance_Curves_Lowlimit;
            try
            {


                int UpStartCurvesIndex = ScreenApproachFigure(info.Balance_Curves.Xdata, Uplimit.Xdata[0]);
                int LowStartCurvesIndex = ScreenApproachFigure(info.Balance_Curves.Xdata, LowLimit.Xdata[0]);

                int StrrtIndex = UpStartCurvesIndex < LowStartCurvesIndex
                    ? UpStartCurvesIndex
                    : LowStartCurvesIndex;
                int capacity = Uplimit.Xdata.Length > LowLimit.Xdata.Length
                    ? Uplimit.Xdata.Length
                    : LowLimit.Xdata.Length;



                Dictionary<string, _CurvesData> DiffAllCurves = new Dictionary<string, _CurvesData>();
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

                        double[] DiffYdata = new double[capacity];
                        double[] DiffXdata = new double[capacity];
                        double[] DiffAsb = new double[capacity];
                        for (int i = 0; i < DiffYdata.Length; i++)
                        {

                            double L = L_Ydata[StrrtIndex + i];
                            double R = R_Ydata[StrrtIndex + i];
                            DiffXdata[i] = LpassData.Value.Xdata[StrrtIndex + i];
                            DiffYdata[i] = (L - R).Round(2);
                            DiffAsb[i] = DiffYdata[i].abs();


                        }
                        DiffAllCurves.Add(DiffName, new _CurvesData(DiffName, DiffXdata, DiffYdata));
                        asbDifference[DiffName] = DiffAsb;
                        diffMaxValue[DiffName] = DiffAsb.Max();

                    }
                }
                double MinValue = double.MaxValue;
                foreach (var item in diffMaxValue)
                {
                    if (item.Value < MinValue)
                    {
                        curves = DiffAllCurves[item.Key];
                        MinValue = item.Value;
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show($"=> 0005 Error 嵌入limit筛选感差曲线报错\r\n“{ex.StackTrace}”", "算法异常");

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
                double limitDiff = 0.2;

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


                int UpStartCurvesIndex = ScreenApproachFigure(info.Balance_Curves.Xdata, Uplimit.Xdata[0]);
                int LowStartCurvesIndex = ScreenApproachFigure(info.Balance_Curves.Xdata, LowLimit.Xdata[0]);

                int StrrtIndex = UpStartCurvesIndex < LowStartCurvesIndex
                    ? UpStartCurvesIndex
                    : LowStartCurvesIndex;
                int capacity = Uplimit.Xdata.Length > LowLimit.Xdata.Length
                    ? Uplimit.Xdata.Length
                    : LowLimit.Xdata.Length;



                Dictionary<string, _CurvesData> DiffAllCurves = new Dictionary<string, _CurvesData>();
                Dictionary<string, double[]> asbDifference = new Dictionary<string, double[]>();
                Dictionary<string, double> diffMaxValue = new Dictionary<string, double>();
                double limitDiff = 0.4;
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

                        double[] DiffYdata = new double[capacity];
                        double[] DiffXdata = new double[capacity];
                        double[] DiffAsb = new double[capacity];
                        for (int i = 0; i < DiffYdata.Length; i++)
                        {

                            double L = L_Ydata[StrrtIndex + i];
                            double R = R_Ydata[StrrtIndex + i];
                            DiffXdata[i] = LpassData.Value.Xdata[StrrtIndex + i];
                            DiffYdata[i] = (L - R).Round(2);
                            DiffAsb[i] = DiffYdata[i].abs();


                        }

                        int UpIndex = ScreenApproachFigure(DiffXdata, Uplimit.Xdata[0]);
                        for (int i = 0; i < Uplimit.Ydata.Length; i++)
                        {
                            if (DiffYdata[UpIndex + i] > Uplimit.Ydata[i] - limitDiff)
                            {
                                pass = false;
                            }
                        }


                        int LowIndex = ScreenApproachFigure(DiffXdata, LowLimit.Xdata[0]);
                        for (int i = 0; i < LowLimit.Ydata.Length; i++)
                        {
                            if (DiffYdata[LowIndex + i] < LowLimit.Ydata[i] + limitDiff)
                            {
                                pass = false;
                            }
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

