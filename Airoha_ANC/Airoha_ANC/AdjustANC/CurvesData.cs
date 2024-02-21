using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Airoha.AdjustANC
{
    public class CurvesInfo
    {
        public struct info
        {
            public _CurvesData L_Curves;
            public _CurvesData L_Curves_Uplimit;
            public _CurvesData L_Curves_Lowlimit;

            public _CurvesData R_Curves;
            public _CurvesData R_Curves_Uplimit;
            public _CurvesData R_Curves_Lowlimit;

            public _CurvesData Balance_Curves;
            public _CurvesData Balance_Curves_Uplimit;
            public _CurvesData Balance_Curves_Lowlimit;
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
