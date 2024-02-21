using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoundCheck_V1.TemplateANC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MerryDllFramework
{
    public class CurvesInfo
    {
        public struct info
        {
            public bool L_Result;
            public double L_Gain;
            public _CurvesData L_Curves;
            public _CurvesData L_Curves_Uplimit;
            public _CurvesData L_Curves_Lowlimit;

            public bool R_Result;
            public double R_Gain;
            public _CurvesData R_Curves;
            public _CurvesData R_Curves_Uplimit;
            public _CurvesData R_Curves_Lowlimit;

            public bool Balance_Result;
            public _CurvesData Balance_Curves;
            public _CurvesData Balance_Curves_Uplimit;
            public _CurvesData Balance_Curves_Lowlimit;

            public bool SCResult;
        }
        public static void ReadFB_SCDataA(string FilePath, string Lowlimit, string Name, string Uplimit, ref CurvesInfo.info info)
        {
            info.L_Curves = null;
            info.L_Curves_Lowlimit = null;
            info.L_Curves_Uplimit = null;

            string txtPath = $@"{FilePath}";
            string[] curves;
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

                if (curveName.Contains(Uplimit))
                    info.L_Curves_Uplimit = _curves;
                else if (curveName.Contains(Lowlimit))
                    info.L_Curves_Lowlimit = _curves;
                else if (curveName.Contains(Name))
                    info.L_Curves = _curves;

            }
            if (info.L_Curves == null)
            {
                MessageBox.Show("=> S0015 读取曲线失败", "Sound Check 算法提示");

            }
            if (info.L_Curves_Uplimit == null)
            {
                MessageBox.Show("=> S0014 读取上限失败", "Sound Check 算法提示");

            }
            if (info.L_Curves_Lowlimit == null)
            {
                MessageBox.Show("=> S0013 读取下限失败", "Soundcheck 提示");

            }
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

        }
        public static void ReadFB_SCDataB(string FilePath,
            string LowlimitL, string NameL, string UplimitL,
            string LowlimitR, string NameR, string UplimitR,
            ref CurvesInfo.info info)
        {
            info.L_Curves = null;
            info.L_Curves_Lowlimit = null;
            info.L_Curves_Uplimit = null;
            info.R_Curves = null;
            info.R_Curves_Lowlimit = null;
            info.R_Curves_Uplimit = null;
            string txtPath = $@"{FilePath}";
            string[] curves;
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

                if (curveName.Contains(UplimitL))
                    info.L_Curves_Uplimit = _curves;
                else if (curveName.Contains(LowlimitL))
                    info.L_Curves_Lowlimit = _curves;
                else if (curveName.Contains(NameL))
                    info.L_Curves = _curves;
                else if (curveName.Contains(UplimitR))
                    info.R_Curves_Uplimit = _curves;
                else if (curveName.Contains(LowlimitR))
                    info.R_Curves_Lowlimit = _curves;
                else if (curveName.Contains(NameR))
                    info.R_Curves = _curves;


            }
            if (info.L_Curves == null)
                MessageBox.Show("=> S0017 读取曲线 A 失败", "Sound Check_V1 提示");
            if (info.L_Curves_Uplimit == null)
                MessageBox.Show("=> S0018 读取上限 A 失败", "Sound Check_V1 提示");
            if (info.L_Curves_Lowlimit == null)
                MessageBox.Show("=> S0019 读取下限 A 失败", "Sound Check_V1 提示");


            if (info.R_Curves == null)
                MessageBox.Show("=> S0017 读取曲线 B 失败", "Sound Check_V1 提示");
            if (info.R_Curves_Uplimit == null)
                MessageBox.Show("=> S0018 读取上限 B 失败", "Sound Check_V1 提示");
            if (info.R_Curves_Lowlimit == null)
                MessageBox.Show("=> S0019 读取下限 B 失败", "Sound Check_V1 提示");


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


            //    L    上限
            supplemen(info.R_Curves.Xdata, info.R_Curves_Uplimit.Xdata, info.R_Curves_Uplimit.Ydata, out SuppXdata, out SuppYdata);
            info.R_Curves_Uplimit.Xdata = SuppXdata;
            info.R_Curves_Uplimit.Ydata = SuppYdata;
            //    L    下限
            supplemen(info.R_Curves.Xdata, info.R_Curves_Lowlimit.Xdata, info.R_Curves_Lowlimit.Ydata, out SuppXdata, out SuppYdata);
            info.R_Curves_Lowlimit.Xdata = SuppXdata;
            info.R_Curves_Lowlimit.Ydata = SuppYdata;




        }


        public static void CalibrationMethodB(ref CurvesInfo.info info, double Frequency, bool Ceiling, out double L_Figure, out double R_Figure)
        {
            int Index1 = MerryDllFramework.CurvesInfo.ScreenApproachFigure(info.L_Curves.Xdata, Frequency);
            int Index2 = MerryDllFramework.CurvesInfo.ScreenApproachFigure(info.L_Curves_Uplimit.Xdata, Frequency);
            int Index3 = MerryDllFramework.CurvesInfo.ScreenApproachFigure(info.L_Curves_Lowlimit.Xdata, Frequency);
            int Index4 = MerryDllFramework.CurvesInfo.ScreenApproachFigure(info.R_Curves.Xdata, Frequency);
            int Index5 = MerryDllFramework.CurvesInfo.ScreenApproachFigure(info.R_Curves_Uplimit.Xdata, Frequency);
            int Index6 = MerryDllFramework.CurvesInfo.ScreenApproachFigure(info.R_Curves_Lowlimit.Xdata, Frequency);
            double L_Value = info.L_Curves_Lowlimit.Ydata[Index1];
            double L_Uplimit = info.L_Curves_Lowlimit.Ydata[Index2];
            double L_Lowlimit = info.L_Curves_Lowlimit.Ydata[Index3];
            double R_Value = info.R_Curves_Lowlimit.Ydata[Index4];
            double R_Uplimit = info.R_Curves_Lowlimit.Ydata[Index5];
            double R_Lowlimit = info.R_Curves_Lowlimit.Ydata[Index6];
            Calibration(L_Lowlimit, L_Value, L_Uplimit, Ceiling, out L_Figure);
            Calibration(R_Lowlimit, R_Value, R_Uplimit, Ceiling, out R_Figure);


        }
        static void Calibration(double Low, double Value, double Upp, bool Ceiling, out double Figure)
        {
            bool add = true;

            double result = 0;
            if (Value > Upp)
            {
                if (Ceiling)
                {
                    result = Math.Ceiling(Math.Abs(Upp - Value));

                }
                else
                {
                    result = Math.Abs(Upp - Value);

                }
                add = false;
            }
            else if (Value < Low)
            {
                if (Ceiling)
                    result = Math.Ceiling(Math.Abs(Low - Value));
                else
                    result = Math.Abs(Low - Value);

                add = true;

            }
            Figure = add ? result : -(result);
        }


        public static void supplemen(double[] DataXdata, double[] LimitXdata, double[] LimitYdata, out double[] SuppXdata, out double[] SuppYdata)
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

                MessageBox.Show("=> S0012 上下限补点报错了\r\n" + ex.Message);
            }
        }

        public static int ScreenApproachFigure(double[] ListFigure, double ApproachFigure)
        {
            List<double> list1 = new List<double>();
            for (int i = 0; i < ListFigure.Length; i++)
            {
                list1.Add((ListFigure[i] - ApproachFigure).Round(2).abs());
            }
            int Index = Array.IndexOf(list1.ToArray(), list1.Min());
            return Index;

        }





    }
    public class _CurvesData
    {
        public _CurvesData(string CurveName, double[] Xdata, double[] Ydata, Color _coloc)
        {
            this.CurveName = CurveName;
            this.Xdata = Xdata;
            this.Ydata = Ydata;
            this._Color = _coloc;
        }
        public _CurvesData(string CurveName, double[] Xdata, double[] Ydata)
        {
            this.CurveName = CurveName;
            this.Xdata = Xdata;
            this.Ydata = Ydata;

        }
        public string CurveName;
        public double[] Xdata;
        public double[] Ydata;
        public Color color;
        public Color _Color
        {
            get
            {
                if (color.Name == "0")
                {
                    Random RD = new Random();
                    int R = RD.Next(255);
                    int G = RD.Next(255);
                    int B = RD.Next(255);
                    color = Color.FromArgb(R, G, B);
                }
                return color;
            }
            set
            {
                color = value;
            }
        }

    }



}
