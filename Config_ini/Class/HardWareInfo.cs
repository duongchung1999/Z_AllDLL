using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Net;
using System.Net.Sockets;

public class HardwareInfo
{

 
    public string GetMACAddress()
    {
        ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
        ManagementObjectCollection moc = mc.GetInstances();
        string MACAddress = String.Empty;
        foreach (ManagementObject mo in moc)
        {
            if (MACAddress == String.Empty)
            {
                if ((bool)mo["IPEnabled"] == true) MACAddress = mo["MacAddress"].ToString();
            }
            mo.Dispose();
        }

        MACAddress = MACAddress.Replace(":", "");
        return MACAddress;
    }
    ///
    /// Retrieving Motherboard Manufacturer.
    /// 
    /// 
    ///
    /// Retrieving Motherboard Product Id.
    /// 
    /// 
    ///
    /// Retrieving CD-DVD Drive Path.
    /// 
    /// 
    ///
    /// Retrieving BIOS Maker.
    /// 
    /// 
    public string GetBIOSmaker()
    {

        ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");

        foreach (ManagementObject wmi in searcher.Get())
        {
            try
            {
                return wmi.GetPropertyValue("Manufacturer").ToString();

            }

            catch { }

        }

        return "BIOS Maker: Unknown";

    }
    ///
    /// Retrieving BIOS Serial No.
    /// 
    /// 
    ///
    /// Retrieving BIOS Caption.
    /// 
    /// 

    ///
    /// Retrieving Physical Ram Memory.
    /// 
    /// 
    public string GetPhysicalMemory()
    {
        ManagementScope oMs = new ManagementScope();
        ObjectQuery oQuery = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
        ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
        ManagementObjectCollection oCollection = oSearcher.Get();

        long MemSize = 0;
        long mCap = 0;

        // In case more than one Memory sticks are installed
        foreach (ManagementObject obj in oCollection)
        {
            mCap = Convert.ToInt64(obj["Capacity"]);
            MemSize += mCap;
        }
        MemSize = (MemSize / 1024) / 1024 / 1024;
        return MemSize.ToString() + "GB";
    }
    ///
    /// Retrieving No of Ram Slot on Motherboard.
    /// 
    /// 
    ///
    /// method for retrieving the CPU Manufacturer
    /// using the WMI class
    /// 
    /// CPU Manufacturer
    ///
    /// method to retrieve the CPU's current
    /// clock speed using the WMI class
    /// 
    /// Clock speed
    ///
    /// method to retrieve the network adapters
    /// default IP gateway using WMI
    /// 
    /// adapters default IP gateway
    //public string GetDefaultIPGateway()
    //{
    //    List<string> ipv4 = new List<string>();
    //    foreach (var ipa in Dns.GetHostAddresses(Environment.MachineName)) if (ipa.AddressFamily == AddressFamily.InterNetwork) ipv4.Add(ipa.ToString());
    //    return ipv4[0];
    //}
    ///
    /// Retrieve CPU Speed.
    /// 
    /// 
    public double? GetCpuSpeedInGHz()
    {
        double? GHz = null;
        using (ManagementClass mc = new ManagementClass("Win32_Processor"))
        {
            foreach (ManagementObject mo in mc.GetInstances())
            {
                GHz = 0.001 * (UInt32)mo.Properties["CurrentClockSpeed"].Value;
                break;
            }
        }
        return GHz;
    }
    ///
    /// Retrieving Current Language
    /// 
    /// 
    ///
    /// Retrieving Current Language.
    /// 
    /// 
    public string GetOSInformation()
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
        foreach (ManagementObject wmi in searcher.Get())
        {
            try
            {
                return ((string)wmi["Caption"]).Trim() + ", " + (string)wmi["Version"] + ", " + (string)wmi["OSArchitecture"];
            }
            catch { }
        }
        return "BIOS Maker: Unknown";
    }
    ///
    /// Retrieving Processor Information.
    /// 
    /// 
    public String GetProcessorInformation()
    {
        ManagementClass mc = new ManagementClass("win32_processor");
        ManagementObjectCollection moc = mc.GetInstances();
        String info = String.Empty;
        foreach (ManagementObject mo in moc)
        {
            string name = (string)mo["Name"];
            name = name.Replace("(TM)", "™").Replace("(tm)", "™").Replace("(R)", "®").Replace("(r)", "®").Replace("(C)", "©").Replace("(c)", "©").Replace("    ", " ").Replace("  ", " ");

            info = name + ", " + (string)mo["Caption"] + ", " + (string)mo["SocketDesignation"];
            //mo.Properties["Name"].Value.ToString();
            //break;
        }
        return info;
    }
    ///
    /// Retrieving Computer Name.
    /// 
    /// 
    public String GetComputerName()
    {
        return Environment.MachineName;
    }

}