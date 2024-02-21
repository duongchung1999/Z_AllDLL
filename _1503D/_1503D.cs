using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Agilent.CommandExpert.ScpiNet.AgSCPI99_1_0;


namespace MerryDllFramework
{
    /// <summary dllName="_1503D">
    /// _1503D
    /// </summary>
    public class MerryDll : IMerryAllDll
    {
        #region 接口方法
        Dictionary<string, object> Config =  new Dictionary<string, object>();
        public string Interface(Dictionary<string, object> keys) => (Config = keys).ToString();

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：_1503D";
            string dllfunction = "Dll功能说明 ：操作";
            string dllHistoryVersion = "历史Dll版本：V23.7.17.1";
            string dllVersion = "当前Dll版本：V23.9.12.0";
            string dllChangeInfo = "V23.9.12.0 ： 2023/9/12 增加读取电压的指令";


            string[] info = { dllname, dllfunction,
                dllHistoryVersion
                , dllVersion,
                dllChangeInfo
            };
            return info;
        }

        public string Run(object[] Command)
        {
            List<string> cmd = new List<string>();
            string AddName = "";
            double add;
            string MARK_Y;
            string MARK_X;
            double Result;
            double nu;
            foreach (var item in Command)
            {

                if (item.GetType().ToString().Contains("TestitemEntity")) AddName = item.GetType().GetProperty("测试项目").GetValue(item, null).ToString();
                if (item.GetType().ToString() != "System.String") continue;
                cmd = new List<string>(item.ToString().Split('&'));
                for (int i = 1; i < cmd.Count; i++) cmd[i] = cmd[i].Split('=')[1];

            }
            //MessageBox.Show($"{Config["_1503D"]}");
            switch (cmd[1])
            {

                case "setup":
                    if (setup(cmd[2], cmd[3],cmd[4]))
                        return true.ToString();
                    else return false.ToString();
                case "CurrentRead":
                    CurrentRead(cmd[2], out MARK_X, cmd[3]);
                    return Math.Round((double.Parse(MARK_X)), 4).ToString();
                case "VoltRead":
                    VoltRead(cmd[2], out MARK_X);
                    return Math.Round((double.Parse(MARK_X)), 4).ToString();
                default: return "指令错误 False";
            }
        }
        #endregion
        #region 测试项目
        string number = "";
        AgSCPI99 COMM;

        /// <summary isPublicTestItem="true">
        /// 设定输出的电压电流 1503D Setup
        /// </summary>
        /// <param name="channel" options="1,2" >通道</param>
        /// <param name="Voltage" options="3.8,5" >Volt</param>
        /// <param name="current1" options="1,0.5" >Current</param>
        /// <returns>浮点数或报错信息</returns>
        private bool setup(string channel, string Voltage,string current1)
        {
            bool flag = false;
            string SetupCurrent = "MAX";
           /* if (typeCurrent == "MIN" || typeCurrent == "MID" || typeCurrent == "MAX") SetupCurrent = typeCurrent;*/

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    //程序第一次启动将会自动搜索装置
                    //if (number == "")
                    //{
                    //    foreach (var item in Hardware.AllUsbDevices)
                    //    {
                    //        try
                    //        {
                    //            if (!item.Name.Contains("IVI")) continue;
                    //            if (!item.PNPDeviceID.Contains("GES857631")) continue;
                    //            number = item.PNPDeviceID.Substring(22, 9);
                    //            break;
                    //        }
                    //        catch (Exception)
                    //        {

                    //            throw;
                    //        }

                    //    }
                    //    if (number == "")
                    //    {
                    //        MessageBox.Show("number = null");
                    //        return false;
                    //    }
                    //}
                    // Input parameter
                    string queryString3 = "*IDN?";
                    string ReturnValue = null;
                    // In order to use the following driver class, you need to reference this assembly : [C:\ProgramData\Keysight\Command Expert\ScpiNetDrivers\AgSCPI99_1_0.dll]
                    if (COMM == null)
                    {
                        COMM = new AgSCPI99($"{Config["_1503D"]}");
                        COMM.SCPI.CLS.Command();
                        COMM.SCPI.RST.Command();
                        COMM.SCPI.WAI.Command();
                    }
                    COMM.SCPI.WAI.Command();
                    COMM.Transport.Command.Invoke($":SOUR{channel}:VOLT {Voltage}"); //设定电压的指令
                    COMM.Transport.Command.Invoke($":SENS{channel}:CURR:RANG {SetupCurrent}");//设定电流的指令
                    COMM.Transport.Command.Invoke($":SOUR{channel}:CURR {current1}"); //设定电压的指令
                    COMM.Transport.Command.Invoke($":OUTPut{channel}:STATe ON");
                    flag = true;
                }
                catch
                {
                    COMM = null;
                    number = "";
                    flag = false;
                }
                if (flag) break;
                Thread.Sleep(500);
            }
            return flag;
        }

        /// <summary isPublicTestItem="true">
        /// 读取电流 CurrentRead
        /// </summary>
        /// <param name="chanel" options="1,2" >通道</param>
        ///  <param name="type" options="uA,mA,A" >通道</param>
        /// <returns>浮点数或报错信息</returns>
        private bool CurrentRead(string chanel, out string MARK_X,string type)
        {
            int Count = 2;
            MARK_X = double.MinValue.ToString();
            bool flag = false;
            List<double> doubles = new List<double>();
            
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    // In order to use the following driver class, you need to reference this assembly : [C:\ProgramData\Keysight\Command Expert\ScpiNetDrivers\AgSCPI99_1_0.dll]

                    if (COMM == null)
                    {
                        COMM = new AgSCPI99($"{Config["_1503D"]}");//用于与仪器建立连接， Config["_1503D"]是在Config.ini文件里设定/ Lệnh kết nối tới thiết bị, giá trị Config["_1503D"] được thiết lập trong file Config.ini
                        COMM.SCPI.CLS.Command();
                        COMM.SCPI.RST.Command();
                        COMM.SCPI.WAI.Command();
                    }
                   
                    for (int j = 0; j < Count; j++)
                    {
                        COMM.SCPI.WAI.Command();
                        COMM.Transport.Command.Invoke(":OUTPut:STATe ON");
                        COMM.SCPI.WAI.Command();
                        COMM.Transport.Query.Invoke($":MEASure{chanel}:CURRent?", out MARK_X);
                        Thread.Sleep(50);
                    }
                    flag = true;
                    if (type == "uA")
                    {
                        float MARK_X1 = float.Parse(MARK_X);
                        float MARK_X2 = MARK_X1 * 1000000;
                        MARK_X = MARK_X2 + "";
                    }
                    else if(type == "mA")
                    {
                        float MARK_X1 = float.Parse(MARK_X);
                        float MARK_X2 = MARK_X1 * 1000;
                        MARK_X = MARK_X2 + "";
                    }
                    else if (type != "A"& type != "mA"& type != "uA")
                    {
                        return false;
                    }
                    

                }
                catch (Exception ex)
                {
                    COMM = null;
                    number = "";
                    flag = false;
                }
                if (flag) break;
                Thread.Sleep(500);
            }
            return flag;
        }
        /// <summary isPublicTestItem="true">
        /// 读取电压 VoltRead
        /// </summary>
        /// <param name="chanel" options="1,2" >通道</param>
        /// <returns>浮点数或报错信息</returns>
        private bool VoltRead(string chanel, out string MARK_X)
        {
            int Count = 2;
            MARK_X = double.MinValue.ToString();
            bool flag = false;
            List<double> doubles = new List<double>();

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    // In order to use the following driver class, you need to reference this assembly : [C:\ProgramData\Keysight\Command Expert\ScpiNetDrivers\AgSCPI99_1_0.dll]

                    if (COMM == null)
                    {
                        COMM = new AgSCPI99($"{Config["_1503D"]}");//用于与仪器建立连接， Config["_1503D"]是在Config.ini文件里设定/ Lệnh kết nối tới thiết bị, giá trị Config["_1503D"] được thiết lập trong file Config.ini
                        COMM.SCPI.CLS.Command();
                        COMM.SCPI.RST.Command();
                        COMM.SCPI.WAI.Command();
                    }

                    for (int j = 0; j < Count; j++)
                    {
                        COMM.SCPI.WAI.Command();
                        COMM.Transport.Command.Invoke(":OUTPut:STATe ON");
                        COMM.SCPI.WAI.Command();
                        COMM.Transport.Query.Invoke($":MEASure{chanel}:VOLTage?", out MARK_X);
                        Thread.Sleep(50);
                    }
                    flag = true;

                }
                catch (Exception ex)
                {
                    COMM = null;
                    number = "";
                    flag = false;
                }
                if (flag) break;
                Thread.Sleep(500);
            }
            return flag;
        }

        #endregion
    }


    internal struct PnPEntityInfo
    {
        public String PNPDeviceID;      // 设备ID
        public String Name;             // 设备名称
        public String Description;      // 设备描述
        public String Service;          // 服务ư
        public String Status;           // 设备状态
        public UInt16 VendorID;         // 供应商标识
        public UInt16 ProductID;        // 产品编号 
        public Guid ClassGuid;          // 设备安装类GUID
    }

   
    internal partial class Hardware
    {
        #region UsbDevice
        /// <summary>
        /// 获取所有的USB设备实体（过滤没有VID和PID的设备）
        /// </summary>
        public static PnPEntityInfo[] AllUsbDevices
        {
            get
            {
                return WhoUsbDevice(UInt16.MinValue, UInt16.MinValue, Guid.Empty);
            }
        }

        /// <summary>
        /// 查询USB设备实体（设备要求有VID和PID）
        /// </summary>
        /// <param name="VendorID">供应商标识，MinValue忽视</param>
        /// <param name="ProductID">产品编号，MinValue忽视</param>
        /// <param name="ClassGuid">设备安装类Guid，Empty忽视</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoUsbDevice(UInt16 VendorID, UInt16 ProductID, Guid ClassGuid)
        {
            List<PnPEntityInfo> UsbDevices = new List<PnPEntityInfo>();

            // 获取USB控制器及其相关联的设备实体
            ManagementObjectCollection USBControllerDeviceCollection = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice").Get();
            if (USBControllerDeviceCollection != null)
            {
                foreach (ManagementObject USBControllerDevice in USBControllerDeviceCollection)
                {   // 获取设备实体的DeviceID
                    String Dependent = (USBControllerDevice["Dependent"] as String).Split(new Char[] { '=' })[1];

                    // 过滤掉没有VID和PID的USB设备
                    Match match = Regex.Match(Dependent, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        UInt16 theVendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识
                        if (VendorID != UInt16.MinValue && VendorID != theVendorID) continue;

                        UInt16 theProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                        if (ProductID != UInt16.MinValue && ProductID != theProductID) continue;

                        ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID=" + Dependent).Get();
                        if (PnPEntityCollection != null)
                        {
                            foreach (ManagementObject Entity in PnPEntityCollection)
                            {
                                try
                                {
                                    Guid theClassGuid = new Guid(Entity["ClassGuid"] as String);    // 设备安装类GUID
                                    try
                                    {
                                        if (ClassGuid != Guid.Empty && ClassGuid != theClassGuid) continue;
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }


                                    PnPEntityInfo Element;
                                    Element.PNPDeviceID = Entity["PNPDeviceID"] as String;  // 设备ID
                                    Element.Name = Entity["Name"] as String;                // 设备名称
                                    Element.Description = Entity["Description"] as String;  // 设备描述
                                    Element.Service = Entity["Service"] as String;          // 服务
                                    Element.Status = Entity["Status"] as String;            // 设备状态
                                    Element.VendorID = theVendorID;     // 供应商标识
                                    Element.ProductID = theProductID;   // 产品编号
                                    Element.ClassGuid = theClassGuid;   // 设备安装类GUID

                                    UsbDevices.Add(Element);
                                }
                                catch
                                {

                                    continue;
                                }

                            }
                        }
                    }
                }
            }

            if (UsbDevices.Count == 0) return null; else return UsbDevices.ToArray();
        }

        /// <summary>
        /// 查询USB设备实体（设备要求有VID和PID）
        /// </summary>
        /// <param name="VendorID">供应商标识，MinValue忽视</param>
        /// <param name="ProductID">产品编号，MinValue忽视</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoUsbDevice(UInt16 VendorID, UInt16 ProductID)
        {
            return WhoUsbDevice(VendorID, ProductID, Guid.Empty);
        }

        /// <summary>
        /// 查询USB设备实体（设备要求有VID和PID）
        /// </summary>
        /// <param name="ClassGuid">设备安装类Guid，Empty忽视</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoUsbDevice(Guid ClassGuid)
        {
            return WhoUsbDevice(UInt16.MinValue, UInt16.MinValue, ClassGuid);
        }

        /// <summary>
        /// 查询USB设备实体（设备要求有VID和PID）
        /// </summary>
        /// <param name="PNPDeviceID">设备ID，可以是不完整信息</param>
        /// <returns>设备列表</returns>        
        public static PnPEntityInfo[] WhoUsbDevice(String PNPDeviceID)
        {
            List<PnPEntityInfo> UsbDevices = new List<PnPEntityInfo>();

            // 获取USB控制器及其相关联的设备实体
            ManagementObjectCollection USBControllerDeviceCollection = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice").Get();
            if (USBControllerDeviceCollection != null)
            {
                foreach (ManagementObject USBControllerDevice in USBControllerDeviceCollection)
                {   // 获取设备实体的DeviceID
                    String Dependent = (USBControllerDevice["Dependent"] as String).Split(new Char[] { '=' })[1];
                    if (!String.IsNullOrEmpty(PNPDeviceID))
                    {   // 注意：忽视大小写
                        if (Dependent.IndexOf(PNPDeviceID, 1, PNPDeviceID.Length - 2, StringComparison.OrdinalIgnoreCase) == -1) continue;
                    }

                    // 过滤掉没有VID和PID的USB设备
                    Match match = Regex.Match(Dependent, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID=" + Dependent).Get();
                        if (PnPEntityCollection != null)
                        {
                            foreach (ManagementObject Entity in PnPEntityCollection)
                            {
                                PnPEntityInfo Element;
                                Element.PNPDeviceID = Entity["PNPDeviceID"] as String;  // 设备ID
                                Element.Name = Entity["Name"] as String;                // 设备名称
                                Element.Description = Entity["Description"] as String;  // 设备描述
                                Element.Service = Entity["Service"] as String;          // 服务
                                Element.Status = Entity["Status"] as String;            // 设备状态
                                Element.VendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识   
                                Element.ProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号                         // 产品编号
                                Element.ClassGuid = new Guid(Entity["ClassGuid"] as String);            // 设备安装类GUID

                                UsbDevices.Add(Element);
                            }
                        }
                    }
                }
            }

            if (UsbDevices.Count == 0) return null; else return UsbDevices.ToArray();
        }

        /// <summary>
        /// 根据服务定位USB设备
        /// </summary>
        /// <param name="ServiceCollection">要查询的服务集合</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoUsbDevice(String[] ServiceCollection)
        {
            if (ServiceCollection == null || ServiceCollection.Length == 0)
                return WhoUsbDevice(UInt16.MinValue, UInt16.MinValue, Guid.Empty);

            List<PnPEntityInfo> UsbDevices = new List<PnPEntityInfo>();

            // 获取USB控制器及其相关联的设备实体
            ManagementObjectCollection USBControllerDeviceCollection = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice").Get();
            if (USBControllerDeviceCollection != null)
            {
                foreach (ManagementObject USBControllerDevice in USBControllerDeviceCollection)
                {   // 获取设备实体的DeviceID
                    String Dependent = (USBControllerDevice["Dependent"] as String).Split(new Char[] { '=' })[1];

                    // 过滤掉没有VID和PID的USB设备
                    Match match = Regex.Match(Dependent, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID=" + Dependent).Get();
                        if (PnPEntityCollection != null)
                        {
                            foreach (ManagementObject Entity in PnPEntityCollection)
                            {
                                String theService = Entity["Service"] as String;          // 服务
                                if (String.IsNullOrEmpty(theService)) continue;

                                foreach (String Service in ServiceCollection)
                                {   // 注意：忽视大小写
                                    if (String.Compare(theService, Service, true) != 0) continue;

                                    PnPEntityInfo Element;
                                    Element.PNPDeviceID = Entity["PNPDeviceID"] as String;  // 设备ID
                                    Element.Name = Entity["Name"] as String;                // 设备名称
                                    Element.Description = Entity["Description"] as String;  // 设备描述
                                    Element.Service = theService;                           // 服务
                                    Element.Status = Entity["Status"] as String;            // 设备状态
                                    Element.VendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识   
                                    Element.ProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                                    Element.ClassGuid = new Guid(Entity["ClassGuid"] as String);            // 设备安装类GUID

                                    UsbDevices.Add(Element);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (UsbDevices.Count == 0) return null; else return UsbDevices.ToArray();
        }
        #endregion

        #region PnPEntity
        /// <summary>
        /// 所有即插即用设备实体（过滤没有VID和PID的设备）
        /// </summary>
        public static PnPEntityInfo[] AllPnPEntities
        {
            get
            {
                return WhoPnPEntity(UInt16.MinValue, UInt16.MinValue, Guid.Empty);
            }
        }

        /// <summary>
        /// 根据VID和PID及设备安装类GUID定位即插即用设备实体
        /// </summary>
        /// <param name="VendorID">供应商标识，MinValue忽视</param>
        /// <param name="ProductID">产品编号，MinValue忽视</param>
        /// <param name="ClassGuid">设备安装类Guid，Empty忽视</param>
        /// <returns>设备列表</returns>
        /// <remarks>
        /// HID：{745a17a0-74d3-11d0-b6fe-00a0c90f57da}
        /// Imaging Device：{6bdd1fc6-810f-11d0-bec7-08002be2092f}
        /// Keyboard：{4d36e96b-e325-11ce-bfc1-08002be10318} 
        /// Mouse：{4d36e96f-e325-11ce-bfc1-08002be10318}
        /// Network Adapter：{4d36e972-e325-11ce-bfc1-08002be10318}
        /// USB：{36fc9e60-c465-11cf-8056-444553540000}
        /// </remarks>
        public static PnPEntityInfo[] WhoPnPEntity(UInt16 VendorID, UInt16 ProductID, Guid ClassGuid)
        {
            List<PnPEntityInfo> PnPEntities = new List<PnPEntityInfo>();

            // 枚举即插即用设备实体
            String VIDPID;
            if (VendorID == UInt16.MinValue)
            {
                if (ProductID == UInt16.MinValue)
                    VIDPID = "'%VID[_]____&PID[_]____%'";
                else
                    VIDPID = "'%VID[_]____&PID[_]" + ProductID.ToString("X4") + "%'";
            }
            else
            {
                if (ProductID == UInt16.MinValue)
                    VIDPID = "'%VID[_]" + VendorID.ToString("X4") + "&PID[_]____%'";
                else
                    VIDPID = "'%VID[_]" + VendorID.ToString("X4") + "&PID[_]" + ProductID.ToString("X4") + "%'";
            }

            String QueryString;
            if (ClassGuid == Guid.Empty)
                QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE" + VIDPID;
            else
                QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE" + VIDPID + " AND ClassGuid='" + ClassGuid.ToString("B") + "'";

            ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher(QueryString).Get();
            if (PnPEntityCollection != null)
            {
                foreach (ManagementObject Entity in PnPEntityCollection)
                {
                    String PNPDeviceID = Entity["PNPDeviceID"] as String;
                    Match match = Regex.Match(PNPDeviceID, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        PnPEntityInfo Element;

                        Element.PNPDeviceID = PNPDeviceID;                      // 设备ID
                        Element.Name = Entity["Name"] as String;                // 设备名称
                        Element.Description = Entity["Description"] as String;  // 设备描述
                        Element.Service = Entity["Service"] as String;          // 服务
                        Element.Status = Entity["Status"] as String;            // 设备状态
                        Element.VendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识
                        Element.ProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                        Element.ClassGuid = new Guid(Entity["ClassGuid"] as String);            // 设备安装类GUID

                        PnPEntities.Add(Element);
                    }
                }
            }

            if (PnPEntities.Count == 0) return null; else return PnPEntities.ToArray();
        }

        /// <summary>
        /// 根据VID和PID定位即插即用设备实体
        /// </summary>
        /// <param name="VendorID">供应商标识，MinValue忽视</param>
        /// <param name="ProductID">产品编号，MinValue忽视</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoPnPEntity(UInt16 VendorID, UInt16 ProductID)
        {
            return WhoPnPEntity(VendorID, ProductID, Guid.Empty);
        }

        /// <summary>
        /// 根据设备安装类GUID定位即插即用设备实体
        /// </summary>
        /// <param name="ClassGuid">设备安装类Guid，Empty忽视</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoPnPEntity(Guid ClassGuid)
        {
            return WhoPnPEntity(UInt16.MinValue, UInt16.MinValue, ClassGuid);
        }

        /// <summary>
        /// 根据设备ID定位设备
        /// </summary>
        /// <param name="PNPDeviceID">设备ID，可以是不完整信息</param>
        /// <returns>设备列表</returns>
        /// <remarks>
        /// 注意：对于下划线，需要写成“[_]”，否则视为任意字符
        /// </remarks>
        public static PnPEntityInfo[] WhoPnPEntity(String PNPDeviceID)
        {
            List<PnPEntityInfo> PnPEntities = new List<PnPEntityInfo>();

            // 枚举即插即用设备实体
            String QueryString;
            if (String.IsNullOrEmpty(PNPDeviceID))
            {
                QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE '%VID[_]____&PID[_]____%'";
            }
            else
            {   // LIKE子句中有反斜杠字符将会引发WQL查询异常
                QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE '%" + PNPDeviceID.Replace('\\', '_') + "%'";
            }

            ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher(QueryString).Get();
            if (PnPEntityCollection != null)
            {
                foreach (ManagementObject Entity in PnPEntityCollection)
                {
                    String thePNPDeviceID = Entity["PNPDeviceID"] as String;
                    Match match = Regex.Match(thePNPDeviceID, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        PnPEntityInfo Element;

                        Element.PNPDeviceID = thePNPDeviceID;                   // 设备ID
                        Element.Name = Entity["Name"] as String;                // 设备名称
                        Element.Description = Entity["Description"] as String;  // 设备描述
                        Element.Service = Entity["Service"] as String;          // 服务
                        Element.Status = Entity["Status"] as String;            // 设备状态
                        Element.VendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识
                        Element.ProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                        Element.ClassGuid = new Guid(Entity["ClassGuid"] as String);            // 设备安装类GUID

                        PnPEntities.Add(Element);
                    }
                }
            }

            if (PnPEntities.Count == 0) return null; else return PnPEntities.ToArray();
        }

        /// <summary>
        /// 根据服务定位设备
        /// </summary>
        /// <param name="ServiceCollection">要查询的服务集合，null忽视</param>
        /// <returns>设备列表</returns>
        /// <remarks>
        /// 跟服务相关的类：
        ///     Win32_SystemDriverPNPEntity
        ///     Win32_SystemDriver
        /// </remarks>
        public static PnPEntityInfo[] WhoPnPEntity(String[] ServiceCollection)
        {
            if (ServiceCollection == null || ServiceCollection.Length == 0)
                return WhoPnPEntity(UInt16.MinValue, UInt16.MinValue, Guid.Empty);

            List<PnPEntityInfo> PnPEntities = new List<PnPEntityInfo>();

            // 枚举即插即用设备实体
            String QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE '%VID[_]____&PID[_]____%'";
            ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher(QueryString).Get();
            if (PnPEntityCollection != null)
            {
                foreach (ManagementObject Entity in PnPEntityCollection)
                {
                    String PNPDeviceID = Entity["PNPDeviceID"] as String;
                    Match match = Regex.Match(PNPDeviceID, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        String theService = Entity["Service"] as String;            // 服务
                        if (String.IsNullOrEmpty(theService)) continue;

                        foreach (String Service in ServiceCollection)
                        {   // 注意：忽视大小写
                            if (String.Compare(theService, Service, true) != 0) continue;

                            PnPEntityInfo Element;

                            Element.PNPDeviceID = PNPDeviceID;                      // 设备ID
                            Element.Name = Entity["Name"] as String;                // 设备名称
                            Element.Description = Entity["Description"] as String;  // 设备描述
                            Element.Service = theService;                           // 服务
                            Element.Status = Entity["Status"] as String;            // 设备状态
                            Element.VendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识
                            Element.ProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                            Element.ClassGuid = new Guid(Entity["ClassGuid"] as String);            // 设备安装类GUID

                            PnPEntities.Add(Element);
                            break;
                        }
                    }
                }
            }

            if (PnPEntities.Count == 0) return null; else return PnPEntities.ToArray();
        }
        #endregion
    }

}




