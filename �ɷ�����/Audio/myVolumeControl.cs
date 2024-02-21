using NAudio.CoreAudioApi;
using System;
using System.Management;
using System.Threading;

namespace System.Audio.Volume
{
    internal class myVolumeControl
    {
        public static void mySetVolume(Int32 value)
        {
            SistemVolumChanger.SetVolume(value);
        }
        public static Int32 myGetVolume()
        {
            return SistemVolumChanger.GetVolume();
        }
        public static bool SETMICVOL(bool flag)
        {
            SistemVolumChanger.SETMICVOL(flag);
            return true;
        }
        public static bool SETVOL(bool flag)
        {
            SistemVolumChanger.SETVOL(flag);
            return true;
        }
        public static bool SPK(int value)
        {
            SETVOL(false);
            SistemVolumChanger.SPKVOL(value);
            return true;
        }
    }
    static class SistemVolumChanger
    {
        public static void SPKVOL(int value)
        {
            try
            {
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                MMDevice device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                device.AudioEndpointVolume.MasterVolumeLevelScalar = (float)value / 100.0f;
            }
            catch (Exception)
            {
            }
        }
        public static string SPKVOL(string name, int value)
        {
            try
            {
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                MMDeviceCollection device = DevEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                foreach (MMDevice dev in device)
                {
                    if (dev.FriendlyName.Contains(name))
                    {
                        dev.AudioEndpointVolume.MasterVolumeLevelScalar = (float)value / 100.0f;
                        return true.ToString();
                    }

                }
                return "Device is Null False";
            }
            catch (Exception ex)
            {
                return ex.Message + " False";
            }
        }

        public static string MICVOL(string name, int value)
        {
            try
            {
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                MMDeviceCollection device = DevEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                foreach (MMDevice dev in device)
                {
                    if (dev.FriendlyName.Contains(name))
                    {
                        dev.AudioEndpointVolume.MasterVolumeLevelScalar = (float)value / 100.0f;
                        return true.ToString();
                    }

                }
                return "Device is Null False";
            }
            catch (Exception ex)
            {
                return ex.Message + " False";
            }
        }
        public static string SearchPlayDevice(string name, int timeOut)
        {
            try
            {
                for (int i = 0; i < timeOut; i++)
                {
                    MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                    MMDeviceCollection device = DevEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                    foreach (MMDevice dev in device)
                        if (dev.FriendlyName.Contains(name))
                            return true.ToString();
                    Thread.Sleep(1000);
                }

                return "Device is Null False";
            }
            catch (Exception ex)
            {
                return ex.Message + " False";
            }
        }

        public static string SearchRecordDevice(string name, int timeOut)
        {
            try
            {
                for (int i = 0; i < timeOut; i++)
                {
                    MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                    MMDeviceCollection device = DevEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                    foreach (MMDevice dev in device)
                        if (dev.FriendlyName.Contains(name))
                            return true.ToString();

                    Thread.Sleep(1000);
                }

                return "Device is Null False";
            }
            catch (Exception ex)
            {
                return ex.Message + " False";
            }
        }

        public static void SetVolume(int value)
        {
            if (value < 0)
                value = 0;

            if (value > 100)
                value = 100;

            String osFriendlyName = GetOSFriendlyName();

            if (osFriendlyName.Contains(MSWindowsFriendlyNames.WindowsXP))
            {
                SetVolumeForWIndowsXP(value);
            }
            else if (osFriendlyName.Contains(MSWindowsFriendlyNames.WindowsVista) || osFriendlyName.Contains(MSWindowsFriendlyNames.Windows7) || osFriendlyName.Contains(MSWindowsFriendlyNames.Windows8) || osFriendlyName.Contains(MSWindowsFriendlyNames.Windows10))
            {
                SetVolumeForWIndowsVista78(value);
            }
            else
            {
                SetVolumeForWIndowsVista78(value);
            }
        }

        public static int GetVolume()
        {
            int result = 0;
            String osFriendlyName = GetOSFriendlyName();

            if (osFriendlyName.Contains(MSWindowsFriendlyNames.WindowsXP))
            {
                Int32 v = PC_VolumeControl.VolumeControl.GetVolume();
                UInt32 vleft = (UInt32)v & 0xffff;
                UInt32 vright = ((UInt32)v & 0xffff0000) >> 16;
                result = ((Int32)vleft | (Int32)vright) * 100 / 0xffff;

            }
            else if (osFriendlyName.Contains(MSWindowsFriendlyNames.WindowsVista) || osFriendlyName.Contains(MSWindowsFriendlyNames.Windows7) || osFriendlyName.Contains(MSWindowsFriendlyNames.Windows8) || osFriendlyName.Contains(MSWindowsFriendlyNames.Windows10))
            {
                try
                {
                    MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                    MMDevice device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    result = (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                }
                catch (Exception)
                {
                }

            }
            else
            {
                try
                {

                    MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                    MMDevice device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    result = (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                }
                catch (Exception)
                {
                }
            }
            return result;
        }

        private static void SetVolumeForWIndowsVista78(int value)
        {
            try
            {
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                MMDevice device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                device.AudioEndpointVolume.MasterVolumeLevelScalar = (float)value / 100.0f;
            }
            catch (Exception)
            {
            }
        }
        public static void SETMICVOL(bool flag)
        {
            try
            {
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                MMDevice device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
                device.AudioEndpointVolume.Mute = flag;
            }
            catch (Exception)
            {
            }
        }
        public static void SETVOL(bool flag)
        {
            try
            {
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                MMDevice device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                device.AudioEndpointVolume.Mute = flag;
            }
            catch (Exception)
            {
            }
        }
        private static void SetVolumeForWIndowsXP(int value)
        {
            try
            {
                double newVolume = ushort.MaxValue * value / 100.0;

                UInt32 v = ((UInt32)newVolume) & 0xffff;
                UInt16 vAll = (UInt16)(v | (v << 16));

                PC_VolumeControl.VolumeControl.SetVolume((Int32)vAll);
            }
            catch (Exception)
            {
            }
        }
        public static string GetOSFriendlyName()
        {
            string result = string.Empty;
            ManagementClass mc = new ManagementClass("Win32_OperatingSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                result = mo["Caption"].ToString();
            }
            return result;
        }

    }

    internal static class MSWindowsFriendlyNames
    {
        public static string WindowsXP { get { return "Windows XP"; } }
        public static string WindowsVista { get { return "Windows Vista"; } }
        public static string Windows7 { get { return "Windows 7"; } }
        public static string Windows8 { get { return "Windows 8"; } }
        public static string Windows10 { get { return "Windows 10"; } }
    }
}

