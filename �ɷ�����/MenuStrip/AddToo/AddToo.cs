using AddToo;
using Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AddToo.INIOperationClass;

namespace MerryDllFramework
{
    public class MerryDll : IMerryAllDll
    {

        #region 主程序调用接口
        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：AddToo";
            string dllfunction = "Dll功能说明 ：补偿值";
            string dllHistoryVersion = "历史Dll版本：0.0.1.0";
            string dllVersion = "当前Dll版本：0.0.1.0";
            string dllChangeInfo = "Dll改动信息：";
            string[] info = { dllname, dllfunction, dllHistoryVersion, dllVersion, dllChangeInfo };

            return info;
        }
        Dictionary<string, object> Keys;
        public string Interface(Dictionary<string, object> keys) => (this.Keys = keys).ToString();
        public async void Run()
        {
            if (!File.Exists(_path))
            {
                using (File.Create(_path))
                {

                } ;
                Thread.Sleep(1000);
            }

            string SectionN9320b = "N9320B";
            string SectionRT550 = "RT550";
            string HanOpticSens = "HanOpticSens";
            string MT8852B = "MT8852B";

            Dictionary<string, string> addvalue = new Dictionary<string, string>();
            //用于储存已经存在在值
            List<string> key = new List<string>();
            key.AddRange(INIGetAllItemKeys(_path, SectionN9320b));
            key.AddRange(INIGetAllItemKeys(_path, MT8852B));
            key.AddRange(INIGetAllItemKeys(_path, SectionRT550));
            key.AddRange(INIGetAllItemKeys(_path, HanOpticSens));

            List<string> writekey = new List<string>();
            List<string[]> testitem = (List<string[]>)Keys["TestItem"];

            //根据测试项目读取已经存在的值跟键
            foreach (var item in testitem)
            {
                if (item[6].Contains(SectionN9320b))
                {
                    writekey.Add($"{item[1]}&{SectionN9320b}");
                    if (!key.Contains(item[1])) continue;

                    addvalue.Add($"{item[1]}&{SectionN9320b}", INIGetStringValue(_path, SectionN9320b, item[1], ""));
                }

                if (item[6].Contains(MT8852B))
                {
                    writekey.Add($"{item[1]}&{MT8852B}");
                    if (!key.Contains(item[1])) continue;
                    addvalue.Add($"{item[1]}&{MT8852B}", INIGetStringValue(_path, MT8852B, item[1], ""));
                }





                if (item[6].Contains(SectionRT550))
                {
                    if (item[6].ToLower().Contains("connect"))
                    {
                        string _Port = $"{item[1]}_Port&{SectionRT550}";

                        writekey.Add(_Port);
                        if (!key.Contains($"{item[1]}_Port") || !key.Contains($"{item[1]}_TXP") || !key.Contains($"{item[1]}_TXPWR"))
                        {
                            INIWriteValue(_path, SectionRT550, $"{item[1]}_Port", "COM1");

                        }
                        addvalue.Add(_Port, INIGetStringValue(_path, SectionRT550, $"{item[1]}_Port", ""));
                        continue;
                    }
                    if (item[6].ToLower().Contains("starttest"))
                    {
                        addvalue.Add($"{item[1]}&{SectionRT550}", INIGetStringValue(_path, SectionRT550, $"{item[1]}", "-70"));
                        writekey.Add($"{item[1]}&{SectionRT550}");
                        continue;
                    }
                    writekey.Add($"{item[1]}&{SectionRT550}");
                    if (!key.Contains(item[1])) continue;
                    addvalue.Add($"{item[1]}&{SectionRT550}", INIGetStringValue(_path, SectionRT550, item[1], ""));
                }

                if (item[6].Contains(HanOpticSens))
                {
                    writekey.Add($"{item[1]}&{HanOpticSens}");
                    if (!key.Contains(item[1])) continue;
                    addvalue.Add($"{item[1]}&{HanOpticSens}", INIGetStringValue(_path, HanOpticSens, item[1], ""));
                }

            }
            string[] a = INIGetAllSectionNames(_path);
            //清楚多余字段，值，键
            foreach (var item in INIGetAllSectionNames(_path))
            {
                INIEmptySection(_path, item);
                if (item != SectionN9320b && item != SectionRT550 && item != HanOpticSens && item != MT8852B) INIDeleteSection(_path, item);
            }
            //写入字段，值，键
            foreach (var item in writekey)
            {
                if (addvalue.ContainsKey(item))
                {
                    if (item.Contains(SectionRT550)) INIWriteValue(_path, SectionRT550, item.Split('&')[0], addvalue[item]);
                    if (item.Contains(SectionN9320b)) INIWriteValue(_path, SectionN9320b, item.Split('&')[0], addvalue[item]);
                    if (item.Contains(MT8852B)) INIWriteValue(_path, MT8852B, item.Split('&')[0], addvalue[item]);

                    if (item.Contains(HanOpticSens)) INIWriteValue(_path, HanOpticSens, item.Split('&')[0], addvalue[item]);
                    continue;
                }
                if (item.Contains(SectionRT550)) INIWriteValue(_path, SectionRT550, item.Split('&')[0], "0");
                if (item.Contains(SectionN9320b)) INIWriteValue(_path, SectionN9320b, item.Split('&')[0], "0");
                if (item.Contains(MT8852B)) INIWriteValue(_path, MT8852B, item.Split('&')[0], "0");

                if (item.Contains(HanOpticSens)) INIWriteValue(_path, HanOpticSens, item.Split('&')[0], "0");
            }

            Form1.GetForm1(_path, "").Show();
        }
        #endregion

        string _path = ".\\AllDLL\\MenuStrip\\AddDeploy.ini";
        [DllImport("kernel32.dll")]
        static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder value, int size, string INIpath);
        [DllImport("kernel32.dll")]
        static extern int WritePrivateProfileString(string section, string key, string val, string path);
        public string GetValue(string section, string key)
        {
            StringBuilder var = new StringBuilder(512);
            GetPrivateProfileString(section, key, "null", var, 512, _path);
            return var.ToString().Trim();
        }
        public long SetValue(string section, string Key, string value) => WritePrivateProfileString(section, Key, value, _path);
    }
}
