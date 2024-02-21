using QCCRF_V1.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    /// <summary dllName="QCCRF_V1">
    /// QCCRF_V1  高通芯片
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        #region 字段、构造函数
        public MerryDll()
        {

        }
        public MerryDll(Dictionary<string, object> Config)
        {
            Qcc.invoke = new Invoke(Config);
            this.Config = Config;
        }
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：QCCRF_V1";
            string dllfunction = "Dll功能说明 ：高通芯片RF测试和RF校准";
            string dllHistoryVersion = "历史Dll版本：0.0.0.0";
            string dllVersion = "当前Dll版本：22.9.29.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "22.9.29.0：支持连扳和单板测试";
            string[] info = { dllname, dllfunction, dllHistoryVersion, dllVersion, dllChangeInfo, dllChangeInfo1 };
            return info;
        }
        public object Interface(Dictionary<string, object> Config)
        {
            Qcc.invoke = new Invoke(Config);
            return this.Config = Config;
        }


        Dictionary<string, object> Config = new Dictionary<string, object>();
        ControlQCC Qcc = new ControlQCC();





        #endregion


        public string Run(object[] Command)
        {
            SplitCMD(Command, out string[] CMD);
            switch (CMD[1])
            {
                case "OpenTestEngineDebug": return OpenTestEngineDebug();
                case "OpenTestEngineDebugFor_sdb": return OpenTestEngineDebugFor_sdb(CMD[2]);
                case "CloseTestEngineDebug": return CloseTestEngineDebug();

                case "_QCC512xCrystalTrim": return _QCC512xCrystalTrim(int.Parse(CMD[2]), double.Parse(CMD[3]));
                case "_QCC514xCrystalTrim": return _QCC514xCrystalTrim(int.Parse(CMD[2]), double.Parse(CMD[3]));
                case "_QCC512xCrystalTrim_MT8852B": return _QCC512xCrystalTrim_MT8852B(int.Parse(CMD[2]));
                case "_QCC514xCrystalTrim_MT8852B": return _QCC514xCrystalTrim_MT8852B(int.Parse(CMD[2]));
                case "teConfigCacheReadItem": return teConfigCacheReadItem(CMD[2]);
                case "IntoDut": return IntoDut();
                case "teChipReset": return teChipReset();
                case "bccmdSetColdReset": return bccmdSetColdReset();
                case "bccmdSetWarmReset": return bccmdSetWarmReset();
                case "XtalLoadCapacitance": return XtalLoadCapacitance();
                case "XtalFreqTrim": return XtalFreqTrim();

                case "hciReset": return hciReset();

                default:
                    return "Command Error False";
            }

        }
        void SplitCMD(object[] Command, out string[] CMD)
        {
            List<string> listCMD = new List<string>();
            foreach (var item in Command)
            {

                Type type = item.GetType();
                if (type.ToString().Contains("TestitemEntity"))
                {
                    PropertyInfo Lowproperty = type.GetProperty("数值下限");
                    Qcc.LowLimit = Lowproperty.GetValue(item, null).ToString();
                    PropertyInfo Upproperty = type.GetProperty("数值上限");
                    Qcc.UpLimit = Upproperty.GetValue(item, null).ToString();
                }
                if (type == typeof(Dictionary<string, object>))
                    Qcc.OnceConfig = (Dictionary<string, object>)item;
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            CMD = listCMD.ToArray();
        }

        /// <summary isPublicTestItem="true">
        /// 进入Debug模式
        /// </summary>
        /// <returns>Port|Handle|sdb</returns>
        public string OpenTestEngineDebug()
            => Qcc.OpenTestEngineDebug();

        /// <summary isPublicTestItem="true">
        /// 进入Debug模式指定sdb文件
        /// </summary>
        /// <param name="sdb_File">sdb文件及芯片参数</param>
        /// <returns>Port|Handle|sdb</returns>
        public string OpenTestEngineDebugFor_sdb(string sdb_File)
            => Qcc.OpenTestEngineDebug(sdb_File);

        /// <summary isPublicTestItem="true">
        /// 芯片512X系列校准 用RT550去校准
        /// </summary>
        /// <param name="Count">校准的次数 最好是15</param>
        /// <param name="TrimRatio">芯片细调的比例 不知可以写 600</param>
        /// <returns>频偏 Khz</returns>
        public string _QCC512xCrystalTrim(int Count, double TrimRatio)
            => Qcc._QCC512xCrystalTrim(Count, TrimRatio);

        /// <summary isPublicTestItem="true">
        /// 芯片514X系列校准 用RT550去校准
        /// </summary>
        /// <param name="Count">校准的次数 最好是15</param>
        /// <param name="TrimRatio">芯片细调的比例 不知可以写 600</param>
        /// <returns>频偏 Khz</returns>
        public string _QCC514xCrystalTrim(int Count, double TrimRatio)
            => Qcc._QCC514xCrystalTrim(Count, TrimRatio);

        /// <summary isPublicTestItem="true">
        /// 芯片512X系列校准 用MT8852B去校准
        /// </summary>
        /// <param name="Count">校准的次数 最好是15</param>
        /// <returns>频偏 Khz</returns>
        public string _QCC512xCrystalTrim_MT8852B(int Count)
            => Qcc._QCC512xCrystalTrim_MT8852B(Count);

        /// <summary isPublicTestItem="true">
        /// 芯片514X系列校准 用MT8852B去校准
        /// </summary>
        /// <param name="Count">校准的次数 最好是15</param>
        /// <returns>频偏 Khz</returns>
        public string _QCC514xCrystalTrim_MT8852B(int Count)
            => Qcc._QCC514xCrystalTrim_MT8852B(Count);

        /// <summary isPublicTestItem="true">
        /// 关闭Debug模式
        /// </summary>
        /// <returns>True</returns>
        public string CloseTestEngineDebug()
            => Qcc.CloseTestEngineDebug();

        /// <summary isPublicTestItem="true"> 
        /// 读取芯片Pskey信息
        /// </summary>
        /// <param name="Pskey">Pskey位置</param>
        /// <returns>Key内容的字符串</returns>
        public string teConfigCacheReadItem(string Pskey)
            => Qcc.teConfigCacheReadItem(Pskey);


        /// <summary isPublicTestItem="true">
        /// 进入DUT测试模式
        /// </summary>
        /// <returns>True or Failure Info</returns>
        public string IntoDut()
            => Qcc.IntoDut();


        /// <summary isPublicTestItem="true">
        /// teChipReset 支持一部分芯片
        /// </summary>
        /// <returns>True or Failure Info</returns>
        public string teChipReset()
            => Qcc.teChipReset();


        /// <summary isPublicTestItem="true">
        /// bccmdSetColdReset 支持另一部分芯片
        /// </summary>
        /// <returns>True</returns>
        public string bccmdSetColdReset()
            => Qcc.bccmdSetColdReset();


        /// <summary isPublicTestItem="true">
        /// bccmdSetWarmReset 支持额外部分芯片
        /// </summary>
        /// <returns>True or Failure Info</returns>
        public string bccmdSetWarmReset()
            => Qcc.bccmdSetWarmReset();


        /// <summary isPublicTestItem="true">
        /// hciReset 部分芯片
        /// </summary>
        /// <returns>True or Failure Info</returns>
        public string hciReset()
            => Qcc.hciReset();

        /// <summary isPublicTestItem="true">
        /// 读取粗调校准值 CAP
        /// </summary>
        /// <returns></returns>

        public string XtalLoadCapacitance()
        {
            return teConfigCacheReadItem("system3:XtalLoadCapacitance");
        }

        /// <summary isPublicTestItem="true">
        /// 读取细调校准值 Xtal
        /// </summary>
        /// <returns></returns>
        public string XtalFreqTrim()
        {
            return teConfigCacheReadItem("system3:XtalFreqTrim");
        }

    }
}
