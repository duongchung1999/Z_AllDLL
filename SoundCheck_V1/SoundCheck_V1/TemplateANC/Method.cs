using MerryDllFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundCheck_V1.TemplateANC
{
    public class Method
    {
        public static string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string SequenceDic = "";

        public static Dictionary<double, Dictionary<double, double[]>> L_FB_CurveGianCardinal = null;
        public static Dictionary<double, Dictionary<double, double[]>> R_FB_CurveGianCardinal = null;
        public static Dictionary<double, Dictionary<double, double[]>> L_FF_CurveGianCardinal = null;
        public static Dictionary<double, Dictionary<double, double[]>> R_FF_CurveGianCardinal = null;
        /// <summary>
        /// 读取Gain值的基数
        /// </summary>
        /// <param name="TypeName"></param>
        /// <param name="isFB"></param>
        public static void ReadCardinal(
            string dicPath, bool isFB)
        {
            try
            {

                if (isFB)
                {
                    if (L_FB_CurveGianCardinal != null && R_FB_CurveGianCardinal != null)
                        return;
                    string L = $@"{dicPath}\L_FB_Curve_data.ini";
                    if (!File.Exists(L))
                        throw new Exception($"未在找到校准基数文件\r\n{L} False".Show());
                    string R = $@"{dicPath}\R_FB_Curve_data.ini";
                    if (!File.Exists(R))
                        throw new Exception($"未在找到校准基数文件\r\n{R} False".Show());
                    L_FB_CurveGianCardinal = adjustSplit(File.ReadAllLines(L));
                    R_FB_CurveGianCardinal = adjustSplit(File.ReadAllLines(R));
                }
                else
                {
                    if (L_FF_CurveGianCardinal != null && R_FF_CurveGianCardinal != null)
                        return;
                    string L = $@"{dicPath}\L_FF_Curve_data.ini";
                    if (!File.Exists(L))
                        throw new Exception($"未在找到校准基数文件\r\n{L} False".Show());
                    string R = $@"{dicPath}\R_FF_Curve_data.ini";
                    if (!File.Exists(R))
                        throw new Exception($"未在找到校准基数文件\r\n{R} False".Show());
                    L_FF_CurveGianCardinal = adjustSplit(File.ReadAllLines(L));
                    R_FF_CurveGianCardinal = adjustSplit(File.ReadAllLines(R));
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"=>SMR066 校准文件异常了\r\n{ex}", "Sound Check 算法提示");
                throw ex;
            }
        }
        /// <summary>
        /// 切割Gain值
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        static Dictionary<double, Dictionary<double, double[]>> adjustSplit(string[] Data)
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
                MessageBox.Show($"=>SMA117 读取校准基数异常\r\n{ex}", "Sound Check 算法提示");
            }

            return DicData;

        }

        /// <summary>
        /// 读取ANC 测试的曲线数据
        /// </summary>
        /// <param name="FilePath">曲线路径</param>
        /// <param name="isFB">是否是FB</param>
        /// <param name="LastInfo">上一条曲线的容器</param>
        /// <param name="NexInfo">下一条曲线的容器</param>
        /// <exception cref="Exception"></exception>
        public static void ReadFB_SCData(
            bool isFB,
            ref CurvesInfo.info LastInfo,
            ref CurvesInfo.info NexInfo)
        {
            string txtPath = $@"{SequenceDic}\ANC Test Curves.txt";
            string[] curves; string L_Name = null; string R_Name = null; string Balance_Name = null;

            if (isFB)
            {
                txtPath = $@"{SequenceDic}\FB Test Curves.txt";
                L_Name = "FB FR_L(Delta)";
                R_Name = "FB FR_R(Delta)";
                Balance_Name = $"FB FR_Balance";

            }
            else if (!isFB)
            {
                txtPath = $@"{SequenceDic}\ANC Test Curves.txt";

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
                        NexInfo.L_Curves_Uplimit = LastInfo.L_Curves_Uplimit = _curves;
                    else if (curveName.Contains("Lower Limit"))
                        NexInfo.L_Curves_Lowlimit = LastInfo.L_Curves_Lowlimit = _curves;
                    else
                    {
                        LastInfo.L_Curves = _curves;
                        NexInfo.L_Curves = new _CurvesData(_curves.CurveName, _curves.Xdata, null);
                    }
                }
                else if (curveName.Contains(R_Name))
                {
                    if (curveName.Contains("Upper Limit"))
                        NexInfo.R_Curves_Uplimit = LastInfo.R_Curves_Uplimit = _curves;

                    else if (curveName.Contains("Lower Limit"))
                        NexInfo.R_Curves_Lowlimit = LastInfo.R_Curves_Lowlimit = _curves;
                    else
                    {
                        LastInfo.R_Curves = _curves;
                        NexInfo.R_Curves = new _CurvesData(_curves.CurveName, _curves.Xdata, null);

                    }
                }
                else if (curveName.Contains(Balance_Name))
                {
                    if (curveName.Contains("Upper Limit"))
                        NexInfo.Balance_Curves_Uplimit = LastInfo.Balance_Curves_Uplimit = _curves;
                    else if (curveName.Contains("Lower Limit"))
                        NexInfo.Balance_Curves_Lowlimit = LastInfo.Balance_Curves_Lowlimit = _curves;
                    else
                    {
                        LastInfo.Balance_Curves = _curves;
                        NexInfo.Balance_Curves = new _CurvesData(_curves.CurveName, _curves.Xdata, null);
                    }
                }


                else if (curveName.Contains("Left"))
                {

                    if (curveName.Contains("Upper Limit"))
                        NexInfo.L_Curves_Uplimit = LastInfo.L_Curves_Uplimit = _curves;

                    else if (curveName.Contains("Lower Limit"))
                        NexInfo.L_Curves_Lowlimit = LastInfo.L_Curves_Lowlimit = _curves;
                    else
                    {
                        LastInfo.L_Curves = _curves;
                        NexInfo.L_Curves = new _CurvesData(_curves.CurveName, _curves.Xdata, null);

                    }
                }
                else if (curveName.Contains("Right"))
                {

                    if (curveName.Contains("Upper Limit"))
                        NexInfo.R_Curves_Uplimit = LastInfo.R_Curves_Uplimit = _curves;
                    else if (curveName.Contains("Lower Limit"))
                        NexInfo.R_Curves_Lowlimit = LastInfo.R_Curves_Lowlimit = _curves;
                    else
                    {
                        LastInfo.R_Curves = _curves;
                        NexInfo.R_Curves = new _CurvesData(_curves.CurveName, _curves.Xdata, null);


                    }
                }
                else if (curveName.Contains("Difference"))
                {
                    if (curveName.Contains("Upper Limit"))
                        NexInfo.Balance_Curves_Uplimit = LastInfo.Balance_Curves_Uplimit = _curves;
                    else if (curveName.Contains("Lower Limit"))
                        NexInfo.Balance_Curves_Lowlimit = LastInfo.Balance_Curves_Lowlimit = _curves;
                    else
                    {
                        LastInfo.Balance_Curves = _curves;
                        NexInfo.Balance_Curves = new _CurvesData(_curves.CurveName, _curves.Xdata, null);
                    }
                }

                else
                {
                    MessageBox.Show($"=>SMR280 测试数据曲线读取错误  {curveName}", "Sound Check 算法提示");
                }
            }
        }

        /// <summary>
        /// 挑选允许修改的Gain值
        /// </summary>
        /// <param name="Up"></param>
        /// <param name="Down"></param>
        /// <param name="L_data"></param>
        /// <param name="R_data"></param>
        /// <param name="L_Cardinal"></param>
        /// <param name="R_Cardinal"></param>
        public void GetGainRegion(
            double Up, double Down,
            Dictionary<double, double[]> L_data,
            Dictionary<double, double[]> R_data,
            out Dictionary<double, double[]> L_Cardinal,
            out Dictionary<double, double[]> R_Cardinal)
        {
            L_Cardinal = new Dictionary<double, double[]>();
            R_Cardinal = new Dictionary<double, double[]>();
            foreach (var item in L_data)
                if (item.Key <= Up && item.Key >= Down)
                    L_Cardinal.Add(item.Key, item.Value);
            foreach (var item in R_data)
                if (item.Key <= Up && item.Key >= Down)
                    R_Cardinal.Add(item.Key, item.Value);

        }

        /// <summary>
        /// 补充缺失的limit
        /// </summary>
        /// <param name="info"></param>
        public static void SupplementLimit(
            ref CurvesInfo.info LastInfo, ref CurvesInfo.info NextInfo)
        {
            try
            {
                double[] SuppXdata = null;
                double[] SuppYdata = null;
                //    L    上限
                Supplemen(LastInfo.L_Curves.Xdata, LastInfo.L_Curves_Uplimit.Xdata, LastInfo.L_Curves_Uplimit.Ydata, out SuppXdata, out SuppYdata);
                NextInfo.L_Curves_Uplimit.Xdata = LastInfo.L_Curves_Uplimit.Xdata = SuppXdata;
                NextInfo.L_Curves_Uplimit.Ydata = LastInfo.L_Curves_Uplimit.Ydata = SuppYdata;

                //    L    下限
                Supplemen(LastInfo.L_Curves.Xdata, LastInfo.L_Curves_Lowlimit.Xdata, LastInfo.L_Curves_Lowlimit.Ydata, out SuppXdata, out SuppYdata);
                NextInfo.L_Curves_Lowlimit.Xdata = LastInfo.L_Curves_Lowlimit.Xdata = SuppXdata;
                NextInfo.L_Curves_Lowlimit.Ydata = LastInfo.L_Curves_Lowlimit.Ydata = SuppYdata;

                //    R    上限
                Supplemen(LastInfo.R_Curves.Xdata, LastInfo.R_Curves_Uplimit.Xdata, LastInfo.R_Curves_Uplimit.Ydata, out SuppXdata, out SuppYdata);
                NextInfo.R_Curves_Uplimit.Xdata = LastInfo.R_Curves_Uplimit.Xdata = SuppXdata;
                NextInfo.R_Curves_Uplimit.Ydata = LastInfo.R_Curves_Uplimit.Ydata = SuppYdata;

                //    R    下限
                Supplemen(LastInfo.R_Curves.Xdata, LastInfo.R_Curves_Lowlimit.Xdata, LastInfo.R_Curves_Lowlimit.Ydata, out SuppXdata, out SuppYdata);
                NextInfo.R_Curves_Lowlimit.Xdata = LastInfo.R_Curves_Lowlimit.Xdata = SuppXdata;
                NextInfo.R_Curves_Lowlimit.Ydata = LastInfo.R_Curves_Lowlimit.Ydata = SuppYdata;

                //    Balance    上限
                Supplemen(LastInfo.Balance_Curves.Xdata, LastInfo.Balance_Curves_Uplimit.Xdata, LastInfo.Balance_Curves_Uplimit.Ydata, out SuppXdata, out SuppYdata);
                NextInfo.Balance_Curves_Uplimit.Xdata = LastInfo.Balance_Curves_Uplimit.Xdata = SuppXdata;
                NextInfo.Balance_Curves_Uplimit.Ydata = LastInfo.Balance_Curves_Uplimit.Ydata = SuppYdata;

                //    Balance    下限
                Supplemen(LastInfo.Balance_Curves.Xdata, LastInfo.Balance_Curves_Lowlimit.Xdata, LastInfo.Balance_Curves_Lowlimit.Ydata, out SuppXdata, out SuppYdata);
                NextInfo.Balance_Curves_Lowlimit.Xdata = LastInfo.Balance_Curves_Lowlimit.Xdata = SuppXdata;
                NextInfo.Balance_Curves_Lowlimit.Ydata = LastInfo.Balance_Curves_Lowlimit.Ydata = SuppYdata;
            }
            catch (Exception ex)
            {

                MessageBox.Show($"=>SMS356 自动补充缺失的limit报错了 \r\n{ex}", "Sound Check 算法提示");
                throw ex;


            }



        }
        /// <summary>
        /// 自动补充缺失的limit
        /// </summary>
        /// <param name="DataXdata"></param>
        /// <param name="LimitXdata"></param>
        /// <param name="LimitYdata"></param>
        /// <param name="SuppXdata"></param>
        /// <param name="SuppYdata"></param>
        static void Supplemen(
            double[] DataXdata, double[] LimitXdata,
            double[] LimitYdata, out double[] SuppXdata,
            out double[] SuppYdata)
        {
            double[] L_CurvesXdata = DataXdata; ;
            List<double> suppXLimit = new List<double>();
            List<double> suppYLimit = new List<double>();
            SuppXdata = null;
            SuppYdata = null;
            try
            {
                if (LimitXdata.Length <= 1)
                {
                    suppXLimit.Add(LimitXdata[0]);
                    suppYLimit.Add(LimitYdata[0]);

                }
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

                MessageBox.Show($"=>SMS421 自动补充缺失的limit报错了 \r\n{ex}", "Sound Check 算法提示");
            }
        }

        /// <summary>
        /// 复制一份曲线字典
        /// </summary>
        /// <param name="L_Curves"></param>
        /// <param name="R_Curves"></param>
        /// <param name="L_ScreenAllCurves"></param>
        /// <param name="R_ScreenAllCurves"></param>
        public void CopyDictionary(
            Dictionary<double, _CurvesData> L_Curves,
            Dictionary<double, _CurvesData> R_Curves,
            out Dictionary<double, _CurvesData> L_ScreenAllCurves,
            out Dictionary<double, _CurvesData> R_ScreenAllCurves)
        {
            L_ScreenAllCurves = new Dictionary<double, _CurvesData>();
            R_ScreenAllCurves = new Dictionary<double, _CurvesData>();
            foreach (var item in L_Curves)
                L_ScreenAllCurves.Add(item.Key, item.Value);

            foreach (var item in R_Curves)
                R_ScreenAllCurves.Add(item.Key, item.Value);


        }

        /// <summary>
        /// 计算左右耳曲线
        /// </summary>
        /// <param name="info"></param>
        /// <param name="L_Cardinal"></param>
        /// <param name="R_Cardinal"></param>
        /// <param name="L_NexGainCurves"></param>
        /// <param name="R_NexGainCurves"></param>
        public static void CalculateAllCurve(
            CurvesInfo.info info,
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
                MessageBox.Show($"=>SMC506 计算左右耳所有曲线报错了\r\n{ex}", "Sound Check 算法提示");

            }





        }

        /// <summary>
        /// 不嵌入limit 筛选平衡曲线
        /// </summary>
        /// <param name="info"></param>
        /// <param name="L_AllCurves"></param>
        /// <param name="R_AllCurves"></param>
        /// <param name="curves"></param>
        public static void CalculateAllBalanceCurves(
            ref CurvesInfo.info NextInfo,
            Dictionary<double, _CurvesData> L_AllCurves,
            Dictionary<double, _CurvesData> R_AllCurves)
        {
            _CurvesData Uplimit = NextInfo.Balance_Curves_Uplimit;
            _CurvesData LowLimit = NextInfo.Balance_Curves_Lowlimit;
            try
            {


                int UpStartCurvesIndex = ScreenApproachFigure(NextInfo.Balance_Curves.Xdata, Uplimit.Xdata[0]);
                int LowStartCurvesIndex = ScreenApproachFigure(NextInfo.Balance_Curves.Xdata, LowLimit.Xdata[0]);

                int StrrtIndex = UpStartCurvesIndex < LowStartCurvesIndex
                    ? UpStartCurvesIndex
                    : LowStartCurvesIndex;
                int capacity = Uplimit.Xdata.Length > LowLimit.Xdata.Length
                    ? Uplimit.Xdata.Length
                    : LowLimit.Xdata.Length;



                Dictionary<string, _CurvesData> BalanceAllCurves = new Dictionary<string, _CurvesData>();
                Dictionary<string, double[]> asbBalanceerence = new Dictionary<string, double[]>();
                Dictionary<string, double> BalanceMaxValue = new Dictionary<string, double>();
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
                        BalanceAllCurves.Add(DiffName, new _CurvesData(DiffName, DiffXdata, DiffYdata));
                        asbBalanceerence[DiffName] = DiffAsb;
                        BalanceMaxValue[DiffName] = DiffAsb.Max();

                    }
                }
                double MinValue = double.MaxValue;
                string Gain = "NG";
                foreach (var item in BalanceMaxValue)
                {
                    if (item.Value < MinValue)
                    {
                        Gain = item.Key;
                        MinValue = item.Value;
                    }
                }
                string[] gains = Gain.Split('&');
                NextInfo.Balance_Result = false;
                NextInfo.Balance_Curves.CurveName = Gain;
                NextInfo.Balance_Curves = BalanceAllCurves[Gain];
                NextInfo.Balance_Curves._Color = false ? Color.LightGreen : Color.Red;
                NextInfo.L_Curves._Color = Color.Yellow;
                NextInfo.L_Gain = double.Parse(gains[0]);
                NextInfo.L_Curves.CurveName = $"L_{gains[0]}";
                NextInfo.L_Curves = L_AllCurves[NextInfo.L_Gain];
                NextInfo.R_Curves._Color = Color.Blue;
                NextInfo.R_Gain = double.Parse(gains[1]);
                NextInfo.R_Curves.CurveName = $"R_{gains[1]}";
                NextInfo.R_Curves = R_AllCurves[NextInfo.R_Gain];

            }
            catch (Exception ex)
            {

                MessageBox.Show($"=>SMC607 不嵌入limit筛选平衡曲线报错\r\n{ex}", "Sound Check 算法提示");

            }


        }


        /// <summary>
        /// 模拟时用的筛选 嵌入limit 筛选 Pass曲线
        /// </summary>
        /// <param name="info"></param>
        /// <param name="L_NotScreenAllCurves"></param>
        /// <param name="R_NotScreenAllCurves"></param>
        /// <param name="L_PassCurves"></param>
        /// <param name="R_PassCurves"></param>
        public static void ScreenPassCurves(
            CurvesInfo.info info,
            Dictionary<double, _CurvesData> L_NotScreenAllCurves,
            Dictionary<double, _CurvesData> R_NotScreenAllCurves,
            out Dictionary<double, _CurvesData> L_PassCurves,
            out Dictionary<double, _CurvesData> R_PassCurves, double LimitDiff)
        {
            L_PassCurves = new Dictionary<double, _CurvesData>();
            R_PassCurves = new Dictionary<double, _CurvesData>();


            try
            {

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
                        double UpLimit = L_UpLimitData.Ydata[i] - LimitDiff;
                        if (nextValue > UpLimit)
                            Pass = false;
                    }
                    for (int i = 0; i < L_LowLimitData.Xdata.Length; i++)
                    {
                        double nextValue = NexYdata[i + L_LowindexStart];
                        double nextXdata = NexXdata[i + L_LowindexStart];

                        double LowLimit = L_LowLimitData.Ydata[i] + LimitDiff;
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
                        double UpLimit = R_UpLimitData.Ydata[i] - LimitDiff;
                        if (nextValue > UpLimit)
                            Pass = false;
                    }
                    for (int i = 0; i < R_LowLimitData.Xdata.Length; i++)
                    {
                        double nextValue = NexYdata[i + R_UpindexStart];
                        double nextXdata = NexXdata[i + R_UpindexStart];
                        double LowLimit = R_LowLimitData.Ydata[i] + LimitDiff;
                        if (nextValue < LowLimit)
                            Pass = false;
                    }
                    if (Pass)
                        R_PassCurves.Add(AllCurves.Key, new _CurvesData($"R_Gain:{AllCurves.Key}", NexXdata, NexYdata));
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"=>SMS703 左右嵌入limit筛选感差曲线报错\r\n{ex}", "Sound Check 算法提示");
            }

        }



        /// <summary>
        /// 嵌入limit 筛选最平衡曲线
        /// </summary>
        /// <param name="NextInfo">下一条曲线</param>
        /// <param name="L_PassCurves">左边能pass的曲线</param>
        /// <param name="R_PassCurves">右边能pass的曲线</param>
        /// <returns></returns>
        public static bool ScreenBalancePassCurves(
            ref CurvesInfo.info NextInfo,
            Dictionary<double, _CurvesData> L_PassCurves,
            Dictionary<double, _CurvesData> R_PassCurves, double LimitDiff)
        {
            _CurvesData Uplimit = NextInfo.Balance_Curves_Uplimit;
            _CurvesData LowLimit = NextInfo.Balance_Curves_Lowlimit;
            bool result = false;
            try
            {
                string Gain = "NG";
                double[] _Xdata = L_PassCurves.First().Value.Xdata;
                int UpStartCurvesIndex = ScreenApproachFigure(_Xdata, Uplimit.Xdata[0]);
                int LowStartCurvesIndex = ScreenApproachFigure(_Xdata, LowLimit.Xdata[0]);

                int StrrtIndex = UpStartCurvesIndex < LowStartCurvesIndex
                    ? UpStartCurvesIndex
                    : LowStartCurvesIndex;
                int capacity = Uplimit.Xdata.Length > LowLimit.Xdata.Length
                    ? Uplimit.Xdata.Length
                    : LowLimit.Xdata.Length;

                Dictionary<string, _CurvesData> BalanceAllCurves = new Dictionary<string, _CurvesData>();
                Dictionary<string, double[]> asbBalanceerence = new Dictionary<string, double[]>();
                Dictionary<string, double> BalanceMaxValue = new Dictionary<string, double>();
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
                            if (DiffYdata[UpIndex + i] > Uplimit.Ydata[i] - LimitDiff)
                            {
                                pass = false;
                            }
                        }


                        int LowIndex = ScreenApproachFigure(DiffXdata, LowLimit.Xdata[0]);
                        for (int i = 0; i < LowLimit.Ydata.Length; i++)
                        {
                            if (DiffYdata[LowIndex + i] < LowLimit.Ydata[i] + LimitDiff)
                            {
                                pass = false;
                            }
                        }
                        if (pass)
                        {
                            result = true;
                            BalanceAllCurves.Add(DiffName, new _CurvesData(DiffName, DiffXdata, DiffYdata));
                            asbBalanceerence[DiffName] = DiffAsb;
                            BalanceMaxValue[DiffName] = DiffAsb.Max();
                        }

                    }
                }
                double MinValue = double.MaxValue;
                foreach (var item in BalanceMaxValue)
                {
                    if (item.Value < MinValue)
                    {
                        Gain = item.Key;
                        MinValue = item.Value;
                    }
                }
                if (BalanceAllCurves.ContainsKey(Gain))
                {
                    string[] gains = Gain.Split('&');
                    NextInfo.Balance_Curves = BalanceAllCurves[Gain];
                    NextInfo.Balance_Result = result;
                    NextInfo.Balance_Curves.CurveName = Gain;
                    NextInfo.Balance_Curves._Color = result ? Color.Purple : Color.Red;

                    NextInfo.L_Gain = double.Parse(gains[0]);
                    NextInfo.L_Curves = L_PassCurves[NextInfo.L_Gain];
                    NextInfo.L_Curves._Color = Color.Yellow;

                    NextInfo.R_Gain = double.Parse(gains[1]);
                    NextInfo.R_Curves = R_PassCurves[NextInfo.R_Gain];
                    NextInfo.R_Curves._Color = Color.Blue;
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"=>SMS830 嵌入limit筛选感差曲线报错\r\n{ex}", "Sound Check 算法提示");
            }
            return result;


        }
        /// <summary>
        /// 选择目标的接近值
        /// </summary>
        /// <param name="ListFigure"></param>
        /// <param name="ApproachFigure"></param>
        /// <returns></returns>
        public static int ScreenApproachFigure(
            double[] ListFigure,
            double ApproachFigure)
        {
            List<double> list1 = new List<double>();
            for (int i = 0; i < ListFigure.Length; i++)
            {
                list1.Add((ListFigure[i] - ApproachFigure).Round(2).abs());
            }
            int Index = Array.IndexOf(list1.ToArray(), list1.Min());
            return Index;

        }
        public static int[] ScreenApproachFigure(
            double[] ListFigure,
            double ApproachFigure,
            double Amount, double TargetFault_Tolerant)
        {
            Dictionary<int, double> list1 = new Dictionary<int, double>();
            List<int> indexs = new List<int>();
            for (int i = 0; i < ListFigure.Length; i++)
            {
                list1.Add(i, (ListFigure[i] - ApproachFigure).Round().abs());
            }
            Dictionary<int, double> ody = list1.OrderBy(o => o.Value).ToDictionary(o => o.Key, p => p.Value);

            for (int i = 0; i < ody.Count; i++)
            {

                if (ody.ElementAt(i).Value <= TargetFault_Tolerant)
                    indexs.Add(ody.ElementAt(i).Key);
            }
            if (indexs.Count < Amount * 0.6)
            {
                for (int i = 0; i < ody.Count; i++)
                {
                    if (indexs.Contains(ody.ElementAt(i).Key))
                        continue;

                    indexs.Add(ody.ElementAt(i).Key);
                    if (indexs.Count >= Amount)
                        break;
                }
            }

            //排序、倒序代码
            //Dictionary<string, double> dic1Asc = dic1.OrderBy(o => o.Value).ToDictionary(o => o.Key, p => p.Value);
            //Dictionary<string, double> dic1desc = dic1.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, p => p.Value); 
            return indexs.OrderBy(o => o).ToArray();

        }



        /// <summary>
        /// 显示校准的预测曲线
        /// </summary>
        /// <param name="LastInfo">上一个曲线数据</param>
        /// <param name="NextInfo">下一个曲线数据</param>
        /// <param name="adjustLog">信息</param>
        public void ShowCailbration(
            CurvesInfo.info LastInfo,
            CurvesInfo.info NextInfo,
            StringBuilder adjustLog,
            string msg, int ShowTime, string CalibrationMode)
        {
            Task.Run(() =>
            {
                CurvesForms forms = new CurvesForms(ShowTime, msg);
                forms.lb_tatle.Text = CalibrationMode;
                string str = $"Last: {LastInfo.L_Gain}&&{LastInfo.R_Gain} Next:{NextInfo.L_Gain}&&{NextInfo.R_Gain}";
                forms.lb_Gain.Text = str;
                forms.lb_L.BackColor = NextInfo.L_Result ? Color.LightGreen : Color.Red;
                forms.lb_R.BackColor = NextInfo.R_Result ? Color.LightGreen : Color.Red;
                forms.lb_Balance.BackColor = NextInfo.Balance_Result ? Color.LightGreen : Color.Red;

                forms.AddCurves_(NextInfo.L_Curves_Uplimit);
                forms.AddCurves_(NextInfo.L_Curves_Lowlimit);
                forms.AddBalanceCurves(NextInfo.Balance_Curves_Uplimit);
                forms.AddBalanceCurves(NextInfo.Balance_Curves_Lowlimit);
                forms.AddCurves_(NextInfo.L_Curves);
                forms.AddCurves_(NextInfo.R_Curves);
                forms.AddBalanceCurves(NextInfo.Balance_Curves);
                forms.AddLog(adjustLog.ToString());
                forms.ShowDialog();

            });



        }

    }
}
