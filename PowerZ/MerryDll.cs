using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandHandle;
using System.Threading;


namespace MerryDllFramework
{
    /// <summary dllName="_PowerZ">
    /// _PowerZ
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        public string Run(object[] Command)
        {
            string[] cmd = new string[20];

            foreach (var item in Command)
            {
                if (item.GetType().ToString() != "System.String") continue;
                cmd = item.ToString().Split('&');
                for (int i = 1; i < cmd.Length; i++) cmd[i] = cmd[i].Split('=')[1];
            }

            switch (cmd[1])
            {
                case "GetVoltage": return GetVoltage(cmd[2]);
                case "GetCurrent": return GetCurrent(cmd[2]);
                case "GetPower": return GetPower();
                default: return "False Command Error!";

            }
        }

        Dictionary<string, object> dic;

        public string Interface(Dictionary<string, object> keys)
        {
            dic = keys;
            return "";
        }

        static Command command = new Command();
        static GetHandle getHandle = new GetHandle();

        public double volPowerZ = 0;
        public double curPowerZ = 0;
        public double tempPowerZ = 0;

        private string ReadValue(string type, string range)
        {
            double valueReturn = 0;
            getHandle.gethandle("5750", "0483", "5750", "0483");
            Thread.Sleep(500);

            var value = "55 05 22 05 0B 01 8D 1C";
            var indexs = "1 6 7 8 9 10 11 12 13 34 35";
            var returnvalue = "AA";

            int voltageInt = 0;
            int currentInt = 0;
            int tempInt = 0;

            command.WriteReturn(value, 64, returnvalue, indexs, getHandle.donglepath[6], getHandle.donglehandle[6]);
            string[] item = command.ReturnValue.Split(' ');
            if (item[0] == "25" || item[0] == "26")
            {
                switch (type)
                {
                    case "voltage":
                        string volEdit = string.Concat(item[4], item[3], item[2], item[1]);
                        voltageInt = Convert.ToInt32(volEdit, 16);
                        switch (range)
                        {
                            case "V":
                                valueReturn = Convert.ToDouble(voltageInt) / 1000000;
                                break;
                            case "mV":
                                valueReturn = Convert.ToDouble(voltageInt) / 1000;
                                break;
                            case "uV":
                                valueReturn = Convert.ToDouble(voltageInt);
                                break;

                        }
                        break;
                    case "current":
                        string curEdit = string.Concat(item[8], item[7], item[6], item[5]);
                        currentInt = Convert.ToInt32(curEdit, 16);
                        switch (range)
                        {
                            case "A":
                                valueReturn = Convert.ToDouble(currentInt) / 1000000;
                                break;
                            case "mA":
                                valueReturn = Convert.ToDouble(currentInt) / 1000;
                                break;
                            case "uA":
                                valueReturn = Convert.ToDouble(currentInt);
                                break;

                        }
                        break;
                }
                
                
            }

            if(valueReturn != 0)
            {
                valueReturn = Math.Round(valueReturn, 4);
            }

            return valueReturn.ToString();
            


        }
        /// <summary isPublicTestItem="true">
        /// 读取电压 GetVoltage
        /// </summary>
        /// <param name="type" options="5,0.5,0.0005" >档位</param>
        /// <returns>浮点数或报错信息</returns>
        private string GetVoltage(string range)
        {
            double valueReturn = 0;
            getHandle.gethandle("5750", "0483", "5750", "0483");
            Thread.Sleep(500);

            var value = "55 05 22 05 0B 01 8D 1C";
            var indexs = "1 6 7 8 9 10 11 12 13 34 35";
            var returnvalue = "AA";

            int voltageInt = 0;

            command.WriteReturn(value, 64, returnvalue, indexs, getHandle.donglepath[6], getHandle.donglehandle[6]);
            string[] item = command.ReturnValue.Split(' ');
            if (item[0] == "25" || item[0] == "26")
            {
                string volEdit = string.Concat(item[4], item[3], item[2], item[1]);
                voltageInt = Convert.ToInt32(volEdit, 16);
                switch (range)
                {
                    case "0.5":
                        valueReturn = Convert.ToDouble(voltageInt) / 1000;
                        break;
                    case "0.0005":
                        valueReturn = Convert.ToDouble(voltageInt);
                        break;
                    default:
                        valueReturn = Convert.ToDouble(voltageInt) / 1000000;
                        break;

                }

            }

            if (valueReturn != 0)
            {
                valueReturn = Math.Round(valueReturn, 4);
            }
            return valueReturn.ToString();
        }
        /// <summary isPublicTestItem="true">
        /// 读取电流 GetCurrent
        /// </summary>
        /// <param name="type" options="5,0.5,0.0005" >档位</param>
        /// <returns>浮点数或报错信息</returns>
        private string GetCurrent(string range)
        {
            double valueReturn = 0;
            getHandle.gethandle("5750", "0483", "5750", "0483");
            Thread.Sleep(500);

            var value = "55 05 22 05 0B 01 8D 1C";
            var indexs = "1 6 7 8 9 10 11 12 13 34 35";
            var returnvalue = "AA";
            int currentInt = 0;

            command.WriteReturn(value, 64, returnvalue, indexs, getHandle.donglepath[6], getHandle.donglehandle[6]);
            string[] item = command.ReturnValue.Split(' ');
            if (item[0] == "25" || item[0] == "26")
            {
                string curEdit = string.Concat(item[8], item[7], item[6], item[5]);
                currentInt = Convert.ToInt32(curEdit, 16);
                switch (range)
                {
                    case "0.5":
                        valueReturn = Convert.ToDouble(currentInt) / 1000;
                        break;
                    case "0.0005":
                        valueReturn = Convert.ToDouble(currentInt);
                        break;

                    default:
                        valueReturn = Convert.ToDouble(currentInt) / 1000000;
                        break;

                }
            }
            if (valueReturn != 0)
            {
                valueReturn = Math.Round(valueReturn, 4);
            }
            return valueReturn.ToString();
        }
        /// <summary isPublicTestItem="true">
        /// 读取功率 GetPower
        /// </summary>
        /// <returns>浮点数或报错信息</returns>
        private string GetPower()
        {
            try
            {
                var volt = Convert.ToDouble(GetVoltage("5"));
                var curr = Convert.ToDouble(GetCurrent("5"));
                var power = Math.Round(volt * curr, 2);
                return power.ToString("0.00");
            }
            catch
            {
                return false.ToString();
            }
           
        }


        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：PowerZ";
            string dllfunction = "Dll功能说明 ：电流表控制";
            string dllHistoryVersion = "历史Dll版本：21.8.26.0";
            string dllVersion = "当前Dll版本：23.9.01.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = "23.9.01.0：2023/9/01：区分具体指令";
            string[] info = { dllname, dllfunction, dllHistoryVersion,
                dllVersion, dllChangeInfo,dllChangeInfo1
            };

            return info;
        }



    }
}