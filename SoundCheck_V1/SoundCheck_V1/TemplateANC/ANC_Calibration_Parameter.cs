using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundCheck_V1.TemplateANC
{
    public class ANC_Calibration_Parameter
    {
        /// <summary>
        /// 校准内容
        /// </summary>
        public string gb_CalibrationDetails;
        /// <summary>
        /// 校准方式
        /// </summary>
        public string gb_CalibrationMethod;
        /// <summary>
        /// 校准特色
        /// </summary>
        public string gb_CalibrationFeature;
        /// <summary>
        /// 初始化增益
        /// </summary>
        public string gb_InitGainSeting;
        /// <summary>
        /// 结果类型
        /// </summary>
        public string gb_ResultType;
        /// <summary>
        /// 最后执行 最后是否烧录
        /// </summary>
        public string gb_FinalExecution;
        /// <summary>
        /// FB 最深点频率类型
        /// </summary>
        public string gb_FBMinType;
        /// <summary>
        /// FB 平均值频点类型
        /// </summary>
        public string gb_FBAvgType;
        /// <summary>
        /// FF 最深点频率类型
        /// </summary>
        public string gb_FFMinType;
        /// <summary>
        /// FF 平均值频点类型
        /// </summary>
        public string gb_FFAvgType;
        /// <summary>
        /// 混合 最深点频率类型
        /// </summary>
        public string gb_HMinType;
        /// <summary>
        /// 混合 平均值频率类型
        /// </summary>
        public string gb_HAvgType;
        /// <summary>
        /// 第一步测试最大的尝试次数
        /// </summary>
        public double nb_TestMinCount;
        /// <summary>
        /// 最大的尝试次数
        /// </summary>
        public double nb_TestMaxCount;
        /// <summary>
        /// FB 增益上限
        /// </summary>
        public double nb_FBGainMax;
        /// <summary>
        /// FB 增益下限
        /// </summary>
        public double nb_FBGainMin;
        /// <summary>
        /// FF 增益上限
        /// </summary>
        public double nb_FFGainMax;
        /// <summary>
        /// FF 增益下限
        /// </summary>
        public double nb_FFGainMin;

        /// <summary>
        /// FB 左初始增益
        /// </summary>
        public double nb_FBLInitGain;
        /// <summary>
        /// FB 右初始增益
        /// </summary>
        public double nb_FBRInitGain;
        /// <summary>
        /// FF 左初始增益
        /// </summary>
        public double nb_FFLInitGain;
        /// <summary>
        /// FF 右初始增益
        /// </summary>
        public double nb_FFRInitGain;
        /// <summary>
        /// 增益步进
        /// </summary>
        public double nb_GainSpan;
        /// <summary>
        /// 精扫步进
        /// </summary>
        public double nb_ScanGainSpan;

        /// <summary>
        /// FB 起始频率
        /// </summary>
        public double nb_FBMinSratrFreq;
        /// <summary>
        /// FB 结束频率
        /// </summary>
        public double nb_FBMinENDFreq;
        /// <summary>
        /// FB 单值检查频率
        /// </summary>
        public double nb_FBMinSingleFreq;
 
        /// <summary>
        /// FB 模式时效果的目标值
        /// </summary>
        public double nb_FBMinTarget;

        /// <summary>
        /// FB 最深点目标值容错率
        /// </summary>
        public double nb_FBMinFault_Tolerant;

        /// <summary>
        /// FB 起始频率
        /// </summary>
        public double nb_FBAvgSratrFreq;
        /// <summary>
        /// FB 结束频率
        /// </summary>
        public double nb_FBAvgENDFreq;

        /// <summary>
        /// FB 平均值的限度
        /// </summary>
        public double nb_FBAvgTarget;

        /// <summary>
        /// FB 平均值容错率
        /// </summary>
        public double nb_FBAvgFault_Tolerant;
        /// <summary>
        /// FF 最深点开始频率
        /// </summary>
        public double nb_FFMinSratrFreq;
        /// <summary>
        /// FF 最深点结束频率
        /// </summary>
        public double nb_FFMinENDFreq;
        /// <summary>
        /// FF 最深点目标频率
        /// </summary>
        public double nb_FFMinSingleFreq;
        /// <summary>
        /// FF 最深点目标值
        /// </summary>
        public double nb_FFMinTarget;
        /// <summary>
        /// FF 最深点目标容错率
        /// </summary>
        public double nb_FFMinFault_Tolerant;

        /// <summary>
        /// FF 平均点开始频率
        /// </summary>
        public double nb_FFAvgSratrFreq;
        /// <summary>
        /// FF 平均点结束频率
        /// </summary>
        public double nb_FFAvgENDFreq;
        /// <summary>
        /// FF 平均点目标值
        /// </summary>
        public double nb_FFAvgTarget;
        /// <summary>
        /// FF 平均点目标值容错率
        /// </summary>
        public double nb_FFAvgFault_Tolerant;

        /// <summary>
        /// 混合 最深点开始频率
        /// </summary>
        public double nb_HMinSratrFreq;
        /// <summary>
        /// 混合 最深点结束频率
        /// </summary>
        public double nb_HMinENDFreq;
        /// <summary>
        /// 混合 最深点频率
        /// </summary>
        public double nb_HMinSingleFreq;
        /// <summary>
        /// 混合 最深点目标值
        /// </summary>
        public double nb_HMinTarget;

        /// <summary>
        /// 混合 平均点开始频率
        /// </summary>
        public double nb_HAvgSratrFreq;
        /// <summary>
        /// 混合 平均点结束频率
        /// </summary>
        public double nb_HAvgENDFreq;
        /// <summary>
        /// 混合 平均点目标值
        /// </summary>
        public double nb_HAvgTarget;
        /// <summary>
        /// 混合 平均点上限
        /// </summary>
        public double nb_HAvgUplimit;
        /// <summary>
        /// 混合 平均点下限
        /// </summary>
        public double nb_HAvgLowlimit;
        /// <summary>
        /// 显示校准结果
        /// </summary>
        public bool cb_ShowCailbration;
        /// <summary>
        /// 校准调试阻断器
        /// </summary>
        public double nb_cb_DebugCalibration;
        /// <summary>
        /// 启用烧录失败提示
        /// </summary>
        public bool cb_ShowCMDMSG;

        /// <summary>
        /// 标准品是否烧录
        /// </summary>
        public bool cb_TE_BZP_WriteGain;
        /// <summary>
        /// 标准品校准
        /// </summary>
        public bool cb_TE_BZP_Calibration;

        /// <summary>
        /// 启用精扫模式
        /// </summary>
        public bool cb_EnterAccuracyscan;
        /// <summary>
        /// 启用平衡优先
        /// </summary>
        public bool cb_EnterBalance;
        /// <summary>
        /// FB 启用平均上下限
        /// </summary>
        public bool cb_FBAvgEnableLimit;
        /// <summary>
        /// 混合 启用平均上下限
        /// </summary>
        public bool cb_HAvgEnableLimit;
        /// <summary>
        /// 立讯特色 FBL 校准目标值
        /// </summary>
        public string str_FBLTarget;
        /// <summary>
        /// 立讯特色 FBR 校准目标值
        /// </summary>
        public string str_FBRTarget;
        /// <summary>
        /// 立讯特色 FFL 校准目标值
        /// </summary>
        public string str_FFLTarget;
        /// <summary>
        /// 立讯特色 FFR 校准目标值
        /// </summary>
        public string str_FFRTarget;
        /// <summary>
        /// FB 平均频率点
        /// </summary>
        public double[] dg_FBAvgFreqs = new double[10];
        /// <summary>
        /// FF 平均频率点
        /// </summary>
        public double[] dg_FFAvgFreqs = new double[10];
        /// <summary>
        /// 混合平均频率点
        /// </summary>
        public double[] dg_HAvgFreqs = new double[10];

        /// <summary>
        /// 指令集
        /// </summary>
        public string cbb_CMDList = "";

    }

}
