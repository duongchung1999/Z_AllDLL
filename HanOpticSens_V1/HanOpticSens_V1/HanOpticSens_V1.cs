using HanOpticSens_V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    /// <summary dllName="HanOpticSens_V1">
    /// 涵测光电
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法
        Dictionary<string, object> Config;
        Dictionary<string, object> OnceConfig = new Dictionary<string, object>();
        ControlHanOptic hanOptic = new ControlHanOptic();
        string AddNumber;

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：HanOpticSens_V1";
            string dllfunction = "Dll功能说明 ：光学测试仪器控制";
            string dllHistoryVersion = "历史Dll版本：    ";
            string dllVersion = "当前Dll版本：23.6.29.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "22.12.24.0：重构方法 可以上传项目到后台";
            string dllChangeInfo2 = "23.6.29.0：重构方法增加常规百分比补偿";

            string[] info = { dllname, dllfunction,
                dllHistoryVersion,
                dllVersion,
                dllChangeInfo,dllChangeInfo1
            };

            return info;
        }
        public object Interface(Dictionary<string, object> keys)
            => Config = keys;
        #endregion

        public string Run(object[] Command)
        {
            {

                SplitCMD(Command, out string[] cmd);
                switch (cmd[1])
                {
                    #region 常规读取
                    case "r_doWave": return r_doWave(cmd[2], int.Parse(cmd[3]));
                    case "r_lux": return r_lux(cmd[2], int.Parse(cmd[3]));
                    case "cd_mm": return cd_mm(cmd[2], int.Parse(cmd[3]));
                    case "ColorCoordinateX": return ColorCoordinateX(cmd[2], int.Parse(cmd[3]));
                    case "ColorCoordinateY": return ColorCoordinateY(cmd[2], int.Parse(cmd[3]));
                    #endregion
                    case "WhiteWaveLength":
                    case "SwictTargetWhite": return SwictTargetWhite(cmd[2]);
                    case "WhiteCCT":
                    case "SwictTargetWhiteCCT": return SwictTargetWhiteCCT(cmd[2]);
                    case "MixtureWaveLength":
                    case "SwictTargetSingleOYC": return SwictTargetSingleOYC(cmd[2]);
                    case "RGB_WaveLength":
                    case "SwictTargetSingleRGB": return SwictTargetSingleRGB(cmd[2]);
                    case "RedWaveLength":
                    case "SwictTargetSingleR": return SwictTargetSingleR(cmd[2]);
                    case "lux":
                    case "r_chromaLux": return r_chromaLux(cmd[2], int.Parse(cmd[3]));
                    case "X":
                    case "r_chromaX": return r_chromaX(cmd[2], int.Parse(cmd[3]));
                    case "Y":
                    case "r_chromaY": return r_chromaY(cmd[2], int.Parse(cmd[3]));
                    case "dowave":
                    case "r_chromaDowave": return r_chromaDowave(cmd[2], int.Parse(cmd[3]));
                    case "duty":
                    case "r_chromaDuty": return r_chromaDuty(cmd[2], int.Parse(cmd[3]));
                    case "cct":
                    case "r_chromaCCT": return r_chromaCCT(cmd[2], int.Parse(cmd[3]));
                    case "r_chroma_cd_mm": return r_chroma_cd_mm(cmd[2], int.Parse(cmd[3]), double.Parse(cmd[4]));

                }
                return "Command Error False";

            }



        }
        void SplitCMD(object[] Command, out string[] CMD)
        {
            List<string> listCMD = new List<string>();
            string TestName = "";

            foreach (var item in Command)
            {
                Type type = item.GetType();
                if (type == typeof(Dictionary<string, object>))
                    OnceConfig = (Dictionary<string, object>)item;
                if (type.ToString().Contains("TestitemEntity"))
                {
                    PropertyInfo property = type.GetProperty("测试项目");
                    TestName = property.GetValue(item, null).ToString();
                }
                if (type != typeof(string)) continue;
                listCMD = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < listCMD.Count; i++) listCMD[i] = listCMD[i].Split('=')[1];
            }
            if (OnceConfig.ContainsKey("SN"))
            {
                string TestID = OnceConfig["TestID"].ToString();
                AddNumber = WindowsAPI.GetValue($"{TestID}#HanOpticSens", TestName, ".\\AllDLL\\MenuStrip\\MoreAddDeploy.ini");
            }
            else
            {
                AddNumber = WindowsAPI.GetValue("HanOpticSens", $"{TestName}");

            }
            CMD = listCMD.ToArray();
        }
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 常规读取 #############################################################

        #region 常规读取
        /// <summary isPublicTestItem="true">
        /// 常规 读取主波长  doWave
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <param name="Channel">仪器的通道</param>
        /// <returns>double</returns>
        public string r_doWave(string Port, int Channel)
            => hanOptic.SendHanOpti(Port, ":001r_dowave01-16\\n", Channel, AddNumber);
        /// <summary isPublicTestItem="true">
        /// 常规 读可见光照度  r_lux
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <param name="Channel">仪器的通道</param>
        /// <returns>double</returns>
        public string r_lux(string Port, int Channel)
            => hanOptic.SendHanOpti(Port, ":001r_lux01-16\\n", Channel, AddNumber);
        /// <summary isPublicTestItem="true">
        /// 常规 读亮度 等于r_lux 指令 该指令适合非光纤类产品 cd_mm
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <param name="Channel">仪器的通道</param>
        /// <returns>double</returns>
        public string cd_mm(string Port, int Channel)
            => hanOptic.SendHanOpti(Port, ":001r_cd_mm01-16\\n", Channel, AddNumber);
        /// <summary isPublicTestItem="true">
        /// 常规 读取色坐标 X r_xy
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <param name="Channel">仪器的通道</param>
        /// <returns>double</returns>
        public string ColorCoordinateX(string Port, int Channel)
        {
            string cn = Channel.ToString().PadLeft(2, '0');

            return hanOptic.SendHanOpti(Port, $":001r_xy{cn}-{cn}\\n", 1, AddNumber);
        }

        /// <summary isPublicTestItem="true">
        /// 常规 读取色坐标 Y r_xy
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <param name="Channel">仪器的通道</param>
        /// <returns>double</returns>
        public string ColorCoordinateY(string Port, int Channel)
        {
            string cn = Channel.ToString().PadLeft(2, '0');
            return hanOptic.SendHanOpti(Port, $":001r_xy{cn}-{cn}\\n", 2, AddNumber);
        }
        #endregion

        #region 高级

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 切换模式 #############################################################

        #region 切换模式

        /// <summary isPublicTestItem="true">
        /// 高级 白平衡模式 比较适合白光 切换测试模式 
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <returns>True or False</returns>
        public string SwictTargetWhite(string Port)
            => hanOptic.ResponseHanOpti(Port, ":001w_target_type01-16=0\\n").Contains(":001w_target_type").ToString();

        /// <summary isPublicTestItem="true">
        /// 高级 白色照明LED 色温CCT 区间(2000K-20000k)之间最准确 切换测试模式 
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <returns>True or False</returns>
        public string SwictTargetWhiteCCT(string Port)
            => hanOptic.ResponseHanOpti(Port, ":001w_target_type01-16=3\\n").Contains(":001w_target_type").ToString();

        /// <summary isPublicTestItem="true">
        /// 高级 单芯灯珠发 混色 橙(600nm 左右) 黄(570nm 左右) 青(500nm 左右) 切换测试模式 
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <returns>True or False</returns>
        public string SwictTargetSingleOYC(string Port)
            => hanOptic.ResponseHanOpti(Port, ":001w_target_type01-16=4\\n").Contains(":001w_target_type").ToString();

        /// <summary isPublicTestItem="true">
        /// 高级 RGB 单色灯 红(630nm 左右) 绿(525nm 左右) 蓝(470nm 左右) 切换测试模式 
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <returns>True or False</returns>
        public string SwictTargetSingleRGB(string Port)
            => hanOptic.ResponseHanOpti(Port, ":001w_target_type01-16=5\\n").Contains(":001w_target_type").ToString();

        /// <summary isPublicTestItem="true">
        /// 高级 Red 单红色灯 主波长在(600-690nm) 切换测试模式
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <returns>True or False</returns>
        public string SwictTargetSingleR(string Port)
            => hanOptic.ResponseHanOpti(Port, ":001w_target_type01-16=6\\n").Contains(":001w_target_type").ToString();


        #endregion

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 读取数值 #############################################################
        #region MyRegion

        /// <summary isPublicTestItem="true">
        /// 高级 读可见光照度 Lux 
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <param name="Channel">仪器的通道</param>
        /// <returns>double</returns>
        public string r_chromaLux(string Port, int Channel)
        {
            string cn = Channel.ToString().PadLeft(2, '0');
            return hanOptic.SendHanOpti(Port, $":001r_chroma{cn}-{cn}\\n", 1, AddNumber);
        }

        /// <summary isPublicTestItem="true">
        /// 高级 读色坐标 X 
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <param name="Channel">仪器的通道</param>
        /// <returns>double</returns>
        public string r_chromaX(string Port, int Channel)
        {
            string cn = Channel.ToString().PadLeft(2, '0');
            return hanOptic.SendHanOpti(Port, $":001r_chroma{cn}-{cn}\\n", 2, AddNumber);
        }

        /// <summary isPublicTestItem="true">
        /// 高级 读色坐标 Y
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <param name="Channel">仪器的通道</param>
        /// <returns>double</returns>
        public string r_chromaY(string Port, int Channel)
        {
            string cn = Channel.ToString().PadLeft(2, '0');
            return hanOptic.SendHanOpti(Port, $":001r_chroma{cn}-{cn}\\n", 3, AddNumber);
        }

        /// <summary isPublicTestItem="true">
        /// 高级 读主波长 dowave
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <param name="Channel">仪器的通道</param>
        /// <returns>double</returns>
        public string r_chromaDowave(string Port, int Channel)
        {
            string cn = Channel.ToString().PadLeft(2, '0');
            return hanOptic.SendHanOpti(Port, $":001r_chroma{cn}-{cn}\\n", 4, AddNumber);
        }
        /// <summary isPublicTestItem="true">
        /// 高级 读主波长率
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <param name="Channel">仪器的通道</param>
        /// <returns>double</returns>
        public string r_chromaDuty(string Port, int Channel)
        {
            string cn = Channel.ToString().PadLeft(2, '0');
            return hanOptic.SendHanOpti(Port, $":001r_chroma{cn}-{cn}\\n", 6, AddNumber);
        }
        /// <summary isPublicTestItem="true">
        /// 高级 色温 CCT
        /// </summary>
        /// <param name="Port">串口号</param>
        /// <param name="Channel">仪器的通道</param>
        /// <returns>double</returns>
        public string r_chromaCCT(string Port, int Channel)
        {
            string cn = Channel.ToString().PadLeft(2, '0');
            return hanOptic.SendHanOpti(Port, $":001r_chroma{cn}-{cn}\\n", 6, AddNumber);
        }
        /// <summary>
        /// 高级 亮度值 cd_mm (坎德拉/平方米)
        /// </summary>
        /// <param name="Port">串口</param>
        /// <param name="Channel">通道</param>
        /// <param name="Coefficient">*的比率</param>
        /// <returns></returns>
        public string r_chroma_cd_mm(string Port, int Channel, double Coefficient)
        {
            string cn = Channel.ToString().PadLeft(2, '0');
            return hanOptic.SendHanOpti(Port, $":001r_chroma{cn}-{cn}\\n", 1, AddNumber, Coefficient);
        }
        #endregion
        #endregion



    }
}

