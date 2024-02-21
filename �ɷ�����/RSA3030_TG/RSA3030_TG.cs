using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {
        //接口信息
        #region 接口方法


        Dictionary<string, object> dic;
        public string Interface(Dictionary<string, object> keys)
        {
            dic = keys;
            return "";
        }
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：RSA3030_TG";
            string dllfunction = "Dll功能说明 ：RSA3030_TG频谱仪读取频偏，峰值...";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllVersion = "当前Dll版本：0.0.1.0";
            string dllChangeInfo = "Dll改动信息：";
            string[] info = { dllname, dllfunction, dllHistoryVersion, dllVersion, dllChangeInfo };
            return info;
        }
        public string Run(object[] Command)
        {
            string[] cmd = new string[20];
            string ReadValue = "False";
            foreach (var item in Command)
            {
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }
            switch (cmd[1])
            {
                case "Peak": ReadValue = RSA3030_peak(cmd[2]); break;
                case "Skewing": ReadValue = RSA3030_Skewing(cmd[2]); break;
                case "ReadOrSen": ReadValue = WriteCommand(cmd[2]); break;
                default: ReadValue = "指令错误"; break;
            }

            return ReadValue;
        }
        #endregion




        string Resource = "";
        CVisaOpt m_VisaOpt = new CVisaOpt();
        private string RSA3030_peak(string command)
        {

            Thread.Sleep(100);
            string strback = "False"; try
            {
                //查找资源示例        
                if (Resource == "") ResetDevice();
                //写中心频率
                m_VisaOpt.Write($":SENSe:FREQuency:CENTer {command}e6");
                //打开连续追踪峰值
                m_VisaOpt.Write(":CALCulate:MARKer1:CPSearch:STATe ON");
                m_VisaOpt.Write(":INITiate:CONTinuous 0");
                Thread.Sleep(400);
                //单词扫描
                m_VisaOpt.Write(":INITiate:CONTinuous 0");
                Thread.Sleep(600);
                //读取命令
                strback = m_VisaOpt.Read(":CALCulate:MARKer1:Y?");
                //释放资源
                //m_VisaOpt.Release();
            }
            catch (Exception EX)
            {
                Resource = "";
                throw EX;
            }
            return strback;
        }
        private string RSA3030_Skewing(string command)
        {
            Thread.Sleep(100);
            string strback = "False"; try
            {
                //查找资源示例并打开资源  
                if (Resource == "") ResetDevice();
                //写入中心频点
                m_VisaOpt.Write($":SENSe:FREQuency:CENTer {command}e6");

                m_VisaOpt.Write(":INITiate:CONTinuous 0");
                Thread.Sleep(400);
                //单次扫描
                m_VisaOpt.Write(":INITiate:CONTinuous 0");
                Thread.Sleep(600);
                //读取命令
                strback = (double.Parse(m_VisaOpt.Read(":CALCulate:MARKer1:X?")) / 1000 - double.Parse(command + "e3")).ToString();
                //释放资源
                // m_VisaOpt.Release();
            }
            catch { Resource = ""; }
            return strback;
        }
        private string WriteCommand(string command)
        {
            Thread.Sleep(200);
            if (Resource == "")
            {
                ResetDevice();
            }
            if (command.Contains("?"))
            {
                return m_VisaOpt.Read(command);
            }
            else
            {
                m_VisaOpt.Write(command);
                return "True";
            }
        }
        private void ResetDevice()
        {

            foreach (var item in m_VisaOpt.FindResource("?*INSTR")) if (item.Contains("USB")) Resource = item;
            //初始化设备
            m_VisaOpt.OpenResource(Resource); m_VisaOpt.Write("*IDN?"); m_VisaOpt.Write("*CLS"); m_VisaOpt.Write("*WAI"); Thread.Sleep(500);
            m_VisaOpt.Write("*RST");
            m_VisaOpt.Write("*WAI");
            Thread.Sleep(3300);
            //写SPAN
            m_VisaOpt.Write(":SENSe:FREQuency:SPAN 2.5e6");
            //写RBW
            m_VisaOpt.Write(":SENSe:BANDwidth:RESolution 3e3");
            //单次扫描
            //设置峰值搜索模式为最大参数
            m_VisaOpt.Write(":CALCulate:MARKer:PEAK:SEARch:MODE MAXimum");
            //设置峰值追踪
            m_VisaOpt.Write(":CALCulate:MARKer1:CPSearch:STATe 1");
            m_VisaOpt.Write("*WAI");
            Thread.Sleep(1000);
        }


    }
}
