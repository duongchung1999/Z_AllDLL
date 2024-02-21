using Config_ini.Class;
using Config_ini.Forms;
using FluentFTP;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ICSharpCode.SharpZipLib.Zip.FastZip;

namespace MerryDllFramework
{
    /// <summary>
    /// 存放Config数据
    /// </summary>
    public class MerryDll : IMerryAllDll
    {


        #region 接口方法
        static Dictionary<string, object> Config;
        public string Run(object[] Command) => "";
        public object Interface(Dictionary<string, object> keys)
        {
            Config = keys;
            property = new Property(Config);
            testData = new TestData(Config);
            return Config;
        }
        public void OnceConfigInterface(Dictionary<string, object> OnceConfig)
            => testData.OnceConfigInterface(OnceConfig);

        public string[] GetDllInfo()
        {
            string dllname = "DLL 名称       ：Config_ini";
            string dllfunction = "Dll功能说明 ：集合数据库，储存，保存，上传数据功能，及获取Config";
            string dllHistoryVersion = "历史Dll版本：22.8.8.0";
            string dllVersion = "当前Dll版本：23.12.05.0";
            string dllChangeInfo = "Dll改动信息：";
            string dllChangeInfo1 = " 1.17.5：2021 /12/22：增加工程模式和产测模式";
            string dllChangeInfo2 = " 22.7.9： 修复本地数据错位问题";
            string dllChangeInfo3 = " 22.8.8.0： 增加BZP字段，增加窗体程序自动更新";
            string dllChangeInfo4 = " 23.12.5.0： 处理打开程序出现弹框提示的问题";
            string[] info = { dllname, dllfunction,
                dllHistoryVersion, dllVersion, dllChangeInfo,
                dllChangeInfo1,dllChangeInfo2,dllChangeInfo3,dllChangeInfo4
            };

            return info;
        }

        Property property;
        TestData testData;
        #endregion
        /// <summary>
        /// 获取或更新Config
        /// </summary>
        public void MesConfig()
            => property.MesConfig();
        /// <summary>
        /// 读取本地Config
        /// </summary>
        public void GetConfig()
        {

            #region 写读Config
            property.LoadConfig();


            #region 运行注册表待定
            string[] regPath = Directory.GetFiles($"{Config["adminPath"]}\\Config", @"*.reg");
            Directory.CreateDirectory($"{Config["adminPath"]}\\TestData");
            if (regPath.Length > 0)
                Process.Start("regedit", string.Format(" /s {0}", regPath[0]));
            #endregion
            testData.ClearSoundCheckLogging((bool)Config["SoundcheckDataUploadFTP"]);


            #endregion

        }

        /// <summary>
        /// 根据料号获取Mysql参数
        /// </summary>
        /// <param name="OrderNumber"></param>
        /// <returns></returns>
        public string GetMysqlPartNumberInfo(string OrderNumber) => property.GetMysqlPartNumberInfo(OrderNumber);
       
        /// <summary>
        /// 保存数据部分
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public void SaveTestData(string url) =>
                  testData.SaveTestData(url);
        /// <summary>
        /// 获取已经存在的机型
        /// </summary>
        /// <returns></returns>
        public List<string> APIGetTypeNames()
        {
            List<string> types = new List<string>();
            string APIinfo = "";
            if (APIinfo.Contains("False"))
            {
                MessageBox.Show($"获取数据库机型失败 \r\n{APIinfo}", "TE系统提示");
                return null;
            }
            foreach (JToken obj in JArray.Parse(APIinfo))
                types.Add(obj["model_name"].ToString().Split('.')[0]);
            return types;
        }
        /// <summary>
        /// 获取所有的站别
        /// </summary>
        /// <returns></returns>
        public List<string> APIGetStations()
        {
            List<string> types = new List<string>();
            string APIinfo = "";// HttpPost("http://10.175.5.59:20005/api/GetALLModelStation", "");
            if (APIinfo.Contains("False"))
            {
                MessageBox.Show($"获取数据库测试站失败 \r\n{APIinfo}", "TE系统提示");
                return null;
            }
            foreach (JToken obj in JArray.Parse(APIinfo))
            {
                types.Add(obj["station_name"].ToString());
            }
            return types;
        }


        /// <summary>
        /// 读取测试计划
        /// </summary>
        /// <returns></returns>
        public void GetTestItem()
        {
            //检查机型版本部分
            string TypeName = Config["Name"].ToString();
            string Station = Config["Station"].ToString();
            bool flag = false;
            if (!(bool)Config["EngineerMode"])
            {
                Config["LoadMessagebox"] = $"正在加载测试计划";
                if (!Directory.Exists($@"{Config["adminPath"]}\TestItem\{TypeName}"))
                    Directory.CreateDirectory($@"{Config["adminPath"]}\TestItem\{TypeName}");
                for (int i = 0; i < 3; i++)
                    if (VersionControl.DownloadTestTxt(TypeName, Station, (string)Config["adminPath"]))
                    {
                        flag = true;
                        break;
                    }
                if (!flag)
                {
                    MessageBox.Show($"{TypeName}机型 \"{Station}\" 站未有上传记录，程序即将关闭", "TE系统提示");
                    Process.GetCurrentProcess().Kill();
                }

                testData.ReadTestItem(false);
            }
            else
            {
                Config["LoadMessagebox"] = $"等待登录";
                if (!File.Exists(@".\Config\TestControl.ini"))
                {
                    userLogin box = new userLogin
                    {
                        MdiParent = ((Form)Config["admin"]).MdiParent,
                        StartPosition = FormStartPosition.CenterScreen
                    };
                    box.ShowDialog();
                    if (box.DialogResult != DialogResult.OK)
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }
                testData.ReadTestItem((bool)Config["NetworkFlag"]);
            }
            return;

        }


        #region 更新主模板程序

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        /// <summary>
        /// 检查模板信息
        /// </summary>
        /// <returns></returns>
        public string CheckMerryTest()
        {
            if (!(bool)Config["EngineerMode"])
            {

                string TemplateName = "MerryTestFramework.app";
                string DownloadPath = $@"{Config["adminPath"]}\AllDLL\Download";
                if (Directory.Exists(DownloadPath))
                    Directory.Delete(DownloadPath, true);

                Dictionary<string, DateTime> TestData = new Dictionary<string, DateTime>();
                DateTime date = new FileInfo($"{Config["adminPath"]}\\{TemplateName}.dll").LastWriteTime;

                TestData.Add(TemplateName, date);
                Config["LoadMessagebox"] = "正在检查平台版本信息";

                Dictionary<string, bool> versionInfo = VersionControl.CheckAllDLLVersion(TestData);
                Dictionary<string, bool> DownLoad = new Dictionary<string, bool>();
                //下载辅助更新组件
                if (versionInfo.ContainsValue(false))
                {
                    VersionControl.DownloadAllDLL("Download", DownloadPath);
                    VersionControl.Compress($@"{Config["adminPath"]}\AllDLL", $"{DownloadPath}\\Download.zip", null);
                }
                Config["LoadMessagebox"] = "正在下载平台程序";
                foreach (var item in versionInfo)
                    if (!item.Value)
                        DownLoad[item.Key] = VersionControl.DownloadAllDLL(item.Key, DownloadPath);

                if (DownLoad.ContainsValue(false))
                    Process.GetCurrentProcess().Kill();

                //下需要更新就使用外部程序更新
                if (versionInfo.ContainsValue(false))
                {
                    List<string> updateInfos = new List<string>();
                    foreach (var item in versionInfo)
                        if (!item.Value)
                        {
                            updateInfos.Add($"{$"True={DownloadPath}\\{item.Key}.zip"}");
                            updateInfos.Add($"{System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName}");
                            updateInfos.Add($"{Config["adminPath"]}");
                        }
                    string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    File.WriteAllLines($@"{DownloadPath}\Paths.txt", updateInfos.ToArray(), Encoding.UTF8);
                    Config["LoadMessagebox"] = "部分模块需要关闭程序更新";
                    Process.Start($@"{DownloadPath}\Compress.exe");
                    for (int i = 2; i > 0; i--)
                    {
                        Config["LoadMessagebox"] = $"部分模块需要关闭程序更新倒计时{i}";
                        Thread.Sleep(1000);
                    }
                    Process.GetCurrentProcess().Kill();
                }
            }
            return "";
        }


        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


        #endregion

        #region 更新所有Dll的方法






        /// <summary>
        /// 检查所有设备Dll的版本信息
        /// </summary>
        /// <returns></returns>
        public string CheckAllDllVersion()
        {
            if (!(bool)Config["EngineerMode"])
            {

                CheckAllDllTime(out Dictionary<string, DateTime> TestData);
                DownloadAllDll(TestData, out Dictionary<string, bool> DownLoad);
                UpdataAlldll(DownLoad);

            }
            return "";
        }
        private void CheckAllDllTime(out Dictionary<string, DateTime> TestData)
        {
            List<string[]> TestItem = (List<string[]>)Config["TestItem"];
            TestData = new Dictionary<string, DateTime>();
            TestData.Add("Config_ini", new FileInfo($"{Config["adminPath"]}\\AllDLL\\Config_ini\\Config_ini.dll").LastWriteTime);
            foreach (var item in (List<string[]>)Config["LoadEvent"])
            {
                string alldllName = item[0];
                DateTime date = new FileInfo($"{Config["adminPath"]}\\AllDLL\\{alldllName}\\{alldllName}.dll").LastWriteTime;
                if (!TestData.ContainsKey(alldllName)) TestData.Add(alldllName, date);
            }
            foreach (var item in (List<string[]>)Config["StartTestEvent"])
            {
                string alldllName = item[0];
                DateTime date = new FileInfo($"{Config["adminPath"]}\\AllDLL\\{alldllName}\\{alldllName}.dll").LastWriteTime;
                if (!TestData.ContainsKey(alldllName)) TestData.Add(alldllName, date);
            }
            foreach (var item in (List<string[]>)Config["TestEndEvent"])
            {
                string alldllName = item[0];
                DateTime date = new FileInfo($"{Config["adminPath"]}\\AllDLL\\{alldllName}\\{alldllName}.dll").LastWriteTime;
                if (!TestData.ContainsKey(alldllName)) TestData.Add(alldllName, date);
            }
            foreach (var item in (List<string[]>)Config["ClosedEvent"])
            {
                string alldllName = item[0];
                DateTime date = new FileInfo($"{Config["adminPath"]}\\AllDLL\\{alldllName}\\{alldllName}.dll").LastWriteTime;
                if (!TestData.ContainsKey(alldllName)) TestData.Add(alldllName, date);
            }

            foreach (string[] item in TestItem)
            {
                int methodID = int.Parse(item[5]);
                if (methodID >= 4 && 6 >= methodID)
                {
                    string alldllName = item[6].Split('&')[0].Split('=')[1];
                    DateTime date = new FileInfo($"{Config["adminPath"]}\\AllDLL\\{alldllName}\\{alldllName}.dll").LastWriteTime;
                    if (!TestData.ContainsKey(alldllName)) TestData.Add(alldllName, date);
                }
            }
        }

        private void DownloadAllDll(Dictionary<string, DateTime> TestData, out Dictionary<string, bool> DownLoad)
        {
            string DownloadPath = $@"{Config["adminPath"]}\AllDLL\Download";
            Config["LoadMessagebox"] = "正在检查设备模块版本信息";
            Dictionary<string, bool> versionInfo = VersionControl.CheckAllDLLVersion(TestData);
            DownLoad = new Dictionary<string, bool>();

            //下载辅助更新组件
            if (versionInfo.ContainsValue(false))
            {
                VersionControl.DownloadAllDLL("Download", DownloadPath);
                VersionControl.Compress($@"{Config["adminPath"]}\AllDLL", $"{DownloadPath}\\Download.zip", null);
            }

            Config["LoadMessagebox"] = "正在下载设备模块版本信息";
            foreach (var item in versionInfo)
                if (!item.Value)
                {
                    Config["LoadMessagebox"] = $"{string.Format("{0:0.00%}", Math.Round((double)DownLoad.Count / (double)versionInfo.Count, 2))}\r\n正在下载设备模块版本信息:{item.Key}";

                    DownLoad[item.Key] = VersionControl.DownloadAllDLL(item.Key, DownloadPath);

                }

            if (DownLoad.ContainsValue(false))
                Process.GetCurrentProcess().Kill();

        }

        private void UpdataAlldll(Dictionary<string, bool> DownLoad)
        {
            Config["LoadMessagebox"] = "下载完成开始更新";
            string DownloadPath = $@"{Config["adminPath"]}\AllDLL\Download";
            Dictionary<string, bool> CompressResult = new Dictionary<string, bool>();
            Dictionary<string, bool> UpLoadResult = new Dictionary<string, bool>();
            //将所有下载好的zid压缩包解压在download文件夹，这样方便覆盖差异文件
            foreach (var item in DownLoad)
                CompressResult[item.Key] = VersionControl.Compress($@"{Config["adminPath"]}\AllDLL\Download", $"{Config["adminPath"]}\\AllDll\\Download\\{item.Key}.zip", null);
            //判断一下如果解压失败说明压缩包有问题
            if (CompressResult.ContainsValue(false))
                foreach (var item in CompressResult)
                    if (!item.Value) MessageBox.Show($"{item.Key}模块解压失败程序异常", "Config_ini更新模块信息");
            //解压好后开始更新，将所有差异文件用复制粘贴的方式替换掉
            foreach (var item in CompressResult)
                UpLoadResult[item.Key] = VersionControl.UpdateEachFile($@"{Config["adminPath"]}\AllDLL\{item.Key}", $@"{Config["adminPath"]}\AllDLL\Download\{item.Key}");
            //文件覆盖失败就使用exe取执行
            if (UpLoadResult.ContainsValue(false))
            {

                List<string> updateInfos = new List<string>
                {
                    $"{Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)}",
                    $"{System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName}",
                    $"{Config["adminPath"]}\\AllDLL"
                };

                foreach (var item in UpLoadResult)
                    if (!item.Value)
                    {
                        updateInfos.Add($"False");
                        updateInfos.Add($"{$"{DownloadPath}\\{item.Key}.zip"}");
                        updateInfos.Add($"{Config["adminPath"]}\\AllDLL");
                    }
                if (UpLoadResult.ContainsValue(false))
                {
                    string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    File.WriteAllLines($@"{DownloadPath}\Paths.txt", updateInfos.ToArray(), Encoding.UTF8);
                    Config["LoadMessagebox"] = "部分模块需要关闭程序更新";
                    Process.Start($@"{DownloadPath}\Compress.exe");
                    for (int i = 2; i > 0; i--)
                    {
                        Config["LoadMessagebox"] = $"部分模块需要关闭程序更新倒计时{i}";
                        Thread.Sleep(1000);
                    }
                    Process.GetCurrentProcess().Kill();

                }
            }




        }






        #endregion
        #region 更新机型Dll的方法









        /// <summary>
        /// 检查机型的版本信息
        /// </summary>
        /// <returns></returns>
        public void CheckTypeNameVersion()
        {
            string DownloadPath = $@"{Config["adminPath"]}\AllDLL\Download";
            if (Directory.Exists(DownloadPath))
                Directory.Delete(DownloadPath, true);

            if (!(bool)Config["EngineerMode"])
            {
                //检查机型版本部分
                string TypeName = Config["Name"].ToString();
                string TestItemPath = $"{Config["adminPath"]}\\TestItem\\{TypeName}";
                string ErrorMessage = "";

                Config["LoadMessagebox"] = $"正在检查机型版本信息";
                if (!VersionControl.CheckTypeNameDllVersion(TypeName, new FileInfo($"{Config["adminPath"]}\\TestItem\\{TypeName}\\{TypeName}.dll").LastWriteTime, out ErrorMessage))
                {
                    if (ErrorMessage.Contains("Close"))
                    {
                        Config["LoadMessagebox"] = $"{ErrorMessage}";
                        MessageBox.Show(ErrorMessage, "TE系统提示");
                        Process.GetCurrentProcess().Kill();
                    }
                    Config["LoadMessagebox"] = $"正在下载机型信息";
                    if (!VersionControl.DownloadTestDll(TypeName, (string)Config["adminPath"], out ErrorMessage))
                    {
                        Config["LoadMessagebox"] = ErrorMessage;
                        MessageBox.Show(ErrorMessage, "TE系统提示");
                        Process.GetCurrentProcess().Kill();
                    }
                    Config["LoadMessagebox"] = $"正在更新";
                    if (!VersionControl.UpdateEachFile($@"{Config["adminPath"]}\TestItem\{TypeName}", $@"{Config["adminPath"]}\AllDLL\Download\{TypeName}"))
                    {
                        Config["LoadMessagebox"] = ErrorMessage;
                        MessageBox.Show(ErrorMessage, "TE系统提示");
                        Process.GetCurrentProcess().Kill();
                    }
                }

            }
        }






        #endregion
        #region 连接电脑管理系统

        Thread ConnectThread = null;
        /// <summary>
        /// 启动程序连接数据库
        /// </summary>
        /// <param name="url"></param>

        public void ConnectingToAService(string url)
        {
            if (ConnectThread != null)
                if (ConnectThread.IsAlive)
                    ConnectThread.Abort();

            ConnectToserver.url = url;
            ConnectToserver.TypeName = Config["Name"].ToString();
            ConnectToserver.Station = Config["Station"].ToString();
            ConnectToserver.MesFlag = Config["MesFlag"].ToString();
            ConnectThread = new Thread(ConnectToserver.UpLoad);
            ConnectThread.Start();


        }
        #endregion

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  主动更新AllDLL  #####################################################

        public void InitiativeUpdataAllDll()
        {
            Config["LoadMessagebox"] = "正在触发主动更新模板任务";
            Config["EngineerMode"] = false;
            List<string> listInfo = new List<string>();
            foreach (var item in VersionControl.GetAllDllInfo())
            {
                if (item.Key.Contains("CommectServer") || item.Key.Contains("MerryTestFramework.app")) continue;
                listInfo.Add(System.IO.Path.GetFileNameWithoutExtension(item.Key));
            }
            UpdataInfoForms forms1 = new UpdataInfoForms(listInfo);
            if (forms1.ShowDialog() == DialogResult.OK)
            {
                Dictionary<string, DateTime> TestData = new Dictionary<string, DateTime>();
                foreach (var item in forms1.listBox1.Items)
                {
                    TestData[item.ToString()] = new DateTime();
                }
                Task.Run(() =>
                {
                    DownloadAllDll(TestData, out Dictionary<string, bool> Download);
                    UpdataAlldll(Download);
                    Config["LoadMessageboxFlag"] = true;

                });

            }
            else
            {
                Config["LoadMessageboxFlag"] = true;
            }


        }




        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  主动更新机型  #####################################################
        public void InitiativeUpdataTestItem()
        {

            Config["LoadMessagebox"] = "正在触发主动更新机型任务";
            string TypeName = (string)Config["Name"];
            string DownloadPath = $@"{Config["adminPath"]}\AllDLL\Download";
            string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (MessageBox.Show($"准备下载\t\"{Config["Name"]}\"\r\n！！！！该功能需要跟专案负责人沟通好了再进行使用，更新失败需要进行手动替换\r\n\r\n下载数据源于版本管理系统的 Engineering mode", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                //下载辅助更新组件
                VersionControl.DownloadAllDLL("Download", DownloadPath);
                VersionControl.Compress($@"{Config["adminPath"]}\AllDLL", $"{DownloadPath}\\Download.zip", null);
                if (VersionControl.DownloadEngineerTestDll(TypeName, (string)Config["adminPath"], out string error))
                {
                    List<string> updateInfos = new List<string>
                        {
                            $"{Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)}",
                            $"{System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName}",
                            $"{Config["adminPath"]}\\AllDLL",
                            $"False",
                            $"{DownloadPath}\\{TypeName}.zip",
                            $"{Config["adminPath"]}\\TestItem"
                        };
                    File.WriteAllLines($@"{DownloadPath}\Paths.txt", updateInfos.ToArray(), Encoding.UTF8);
                    Process.Start($@"{DownloadPath}\Compress.exe");
                    for (int i = 2; i > 0; i--)
                    {
                        Config["LoadMessagebox"] = $"部分模块需要关闭程序更新倒计时{i}";
                        Thread.Sleep(1000);
                    }
                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    MessageBox.Show($"下载失败，没有相关信息。{error}");
                };

                Config["LoadMessageboxFlag"] = true;

            }
            else
            {
                Config["LoadMessageboxFlag"] = true;
            }




        }

    }


}

