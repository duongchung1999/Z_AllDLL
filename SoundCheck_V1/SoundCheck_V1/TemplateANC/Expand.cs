using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SoundCheck_V1.TemplateANC
{
    public static class Expand
    {
        public static bool IsInitGain(this string str)
        {
            switch (str)
            {
                case "rb_InitGain":
                    return true;
                case "rb_NotInitGain":
                default:
                    return false;

            }
        }
        public static bool isRegion(this string str)
        {
            return str.Contains("Region");
        }
        public static bool isLux_CalibrationFeature(this string str)
        {
            if (str.Contains("rb_LuxshareFeatrue"))
            {
                return true;
            }
            else if (str.Contains("rb_MerryFeature"))
            {
                return false;
            }
            return false;
        }
        public static Dictionary<string, double[]> TargetToDic(this string str)
        {

            Dictionary<string, double[]> TargetS = new Dictionary<string, double[]>();
            foreach (var item in str.Replace("}", "").Replace(" ", "").Split('{'))
            {
                if (item.Trim() == "") continue;
                List<double> list = new List<double>();
                string[] strs = item.Split(':');
                if (strs.Length >= 3)
                {
                    list.Add(double.Parse(strs[1]));
                    list.Add(double.Parse(strs[2]));
                }
                else
                {
                    list.Add(double.Parse(strs[1]));
                    list.Add(1);
                }

                TargetS[strs[0]] = list.ToArray();
            }
            return TargetS;
        }

        public static string Show(this string str)
        {
            MessageBox.Show(str, "Sound Check 算法提示");
            return str;
        }

        public static double abs(this double str)
            => Math.Abs(str);
        public static double Round(this double str, int i = 1)
            => Math.Round(str, i);
        public static bool ContainsFalse(this string str)
            => str.Contains("False")|| str.Contains("false");
        public static string AddFalse(this string str)
            => $"{str} {false}";
        public static string ObjectToJson(this Dictionary<string, object> dic)
            => JsonConvert.SerializeObject(dic);
        public static JToken StringToJson(this string str)
            => JsonConvert.DeserializeObject<JToken>(str);
        public static string ShowCMDMSG(this string MSG, bool flag, string Caption)
        {
        
            if (flag && (MSG.Contains("false") || MSG.Contains("False"))) MessageBox.Show(MSG, "Func 执行提示：" + Caption);
            return MSG;
        }
        public static void WriteGain(string DicPath, double FBL, double FBR, double FFL, double FFR, double AMBL, double AMBR)
        {
            Task.Run(() =>
            {
                string FilePath = $"{DicPath}\\Gain.csv";
                string Gain = $"100,200,300,400,500,600,\r\n{FBL},{FBR},{FFL},{FFR},{AMBL},{AMBR}";
                for (int i = 0; i < 6; i++)
                {
                    try
                    {
                        File.WriteAllText(FilePath, Gain);
                        return;
                    }
                    catch { Thread.Sleep(500); }
                }
            });

        }

        /// <summary>
        /// 切换ANC 模式
        /// </summary>
        public struct Switch_ANC_Mode
        {
            /// <summary>
            /// 打开FB模式
            /// </summary>
            public const string SwitchFBOnly = "SwitchFBOnly";
            /// <summary>
            /// 打开FF模式
            /// </summary>
            public const string SwitchFFOnly = "SwitchFFOnly";
            /// <summary>
            /// 打开混合模式
            /// </summary>
            public const string SwitchHybrid = "SwitchHybrid";
        }
        /// <summary>
        /// 校准内容
        /// </summary>
        public struct Switch_CalibrationDetails
        {
            /// <summary>
            /// 校准FB
            /// </summary>
            public const string rb_cbtFB = "rb_cbtFB";
            /// <summary>
            /// 校准FF
            /// </summary>
            public const string rb_cbtFF = "rb_cbtFF";
            /// <summary>
            /// FB+混合校准
            /// </summary>
            public const string rb_cbtHybrid = "rb_cbtHybrid";
            /// <summary>
            /// 固定FB+校准FF
            /// </summary>
            public const string rb_cbtOffFB_ONFF = "rb_cbtOffFB_ONFF";
            /// <summary>
            /// 校准FB+固定FF
            /// </summary>
            public const string rb_cbtOnFB_OffFF = "rb_cbtOnFB_OffFF";
            /// <summary>
            /// 固定FB+固定FF
            /// </summary>
            public const string rb_cbtOffFB_OffFF = "rb_cbtOffFB_OffFF";
        }
        /// <summary>
        /// 最后执行是否烧录
        /// </summary>
        public struct Switch_isSaveGain
        {
            /// <summary>
            /// 烧录Gain
            /// </summary>
            public const string rb_SaveGain = "rb_SaveGain";
            /// <summary>
            /// 不烧录Gain
            /// </summary>
            public const string rb_NotWriteGain = "rb_NotWriteGain";
            /// <summary>
            /// Pass才烧录Gain
            /// </summary>
            public const string rb_PassSaveGain = "rb_PassSaveGain";

        }

    }

}
