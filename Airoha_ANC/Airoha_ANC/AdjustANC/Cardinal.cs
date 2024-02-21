using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Airoha.AdjustANC
{
    public class Cardinal
    {

        public static Dictionary<double, Dictionary<double, double[]>> L_FB_CurveGianCardinal = null;

        public static Dictionary<double, Dictionary<double, double[]>> L_FF_CurveGianCardinal = null;

        public static Dictionary<double, Dictionary<double, double[]>> R_FB_CurveGianCardinal = null;

        public static Dictionary<double, Dictionary<double, double[]>> R_FF_CurveGianCardinal = null;

    }
    static class Myconvert
    {

        public static double abs(this double str)
        {
            return Math.Abs(str);
        }
        public static double Round(this double str, int i)
        {
            return Math.Round(str, i);
        }
        public static bool ContainsFalse(this string str)
        {
            return str.Contains("False");
        }

        public static string Show(this string str)
        {
            MessageBox.Show(str, "Airoha_ANC 提示");
            return str;
        }
    }

}
