using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.Class
{
    class FileWorking
    {
        public FileWorking(Dictionary<string, object> Config)
             => this.Config = Config;
        Dictionary<string, object> Config;

        //INIHelper config = new INIHelper();
        string Charge = "";
        string Discharge = "";
        string Recharge = "";
        string _path = "";
        bool flag = false;
        string path_OK = "";
        string path_NG = "";
        string path_MES_Error = "";
        //string path_Log = @"D:\Burnin\Log\log.txt";
        string log_MES = "";
        string log_NG = "";

        public void AutoTest()
        {
            // MessageBox.Show("1");
            path_OK = (string)Config["path_OK"];
            path_NG = (string)Config["path_NG"];
            path_MES_Error = (string)Config["path_MesError"];
            log_MES = $"{Config["path_MesError"]}\\log.txt";
            log_NG = $"{Config["path_NG"]}\\log.txt";
            string path = (string)Config["FileRead"];  //@"D:\Doc\1";
            CheckFolder();
            //MessageBox.Show("2");
            //MessageBox.Show(path);
            Config["StartTest"] = false;
            Config["MoveFile"] = false;
            Config["StartMove"] = false;
            Config["MesError"] = false;
            string log = File.ReadAllText(log_NG);
            MessageBox.Show("AutoTest=2模式");
            Console.WriteLine("AutoTest=2模式");
           
            try 
            {
                while (true)
                {
                    string[] allFile = Directory.GetFiles(path);
                    
                    foreach (var item in allFile)
                    {
                        if (!item.Contains("xls"))
                        {
                            string log1 = File.ReadAllText(log_NG);
                            string message = $"Time:{DateTime.Now:yy-MM-dd  HH:mm:ss} Complete!\n{path} have not File!\n";
                            log1 += Config["MesError"].ToString();
                            File.WriteAllText(log_NG, log1);
                            Console.WriteLine(message);
                            continue;
                        }
                        _path = item;
                        Config["PathFile"] = _path;
                        
                        string[] a = _path.Split('\\', '.');
                        Config["SN"] = a[(int)Config["NumberFile"]];
                        //ReadExcel(item);
                        Config["StartTest"] = true;
                        //flag = false;
                        while ((bool)Config["StartTest"])
                            Thread.Sleep(50);
                        MessageBox.Show("OK");
                        ChangeFilePosition();
                        break;

                    }

                    if ((bool)Config["MoveFile"])
                    {

                        

                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                string a = $"SN: {Config["SN"]}, 设备： {Config["设备"]}，单元： {Config["单元"]}，Time:{DateTime.Now:yy-MM-dd  HH:mm:ss} \n{ex.ToString()} \n\n";
                Console.WriteLine(a);
                log +=  a;
                File.WriteAllText(log_NG, log);
                MessageBox.Show(a);
            }
        }
        public void CheckFolder()
        {
            if (!Directory.Exists(path_OK))
            {
                Directory.CreateDirectory(path_OK);
                Thread.Sleep(1000);
            }
            if (!Directory.Exists(path_NG))
            {
                Directory.CreateDirectory(path_NG);
                Thread.Sleep(1000);
            }
            if (!Directory.Exists(path_MES_Error))
            {
                Directory.CreateDirectory(path_MES_Error);
                Thread.Sleep(1000);
            }


            if (!File.Exists(log_NG))
            {
                using (File.Create(log_NG))
                {
                };
                Thread.Sleep(1000);
            }
            if (!File.Exists(log_MES))
            {
                using (File.Create(log_MES))
                {
                };
                Thread.Sleep(1000);
            }
        }
        private void ReadExcel(string path)
        {
            //MessageBox.Show(path);
            DataSet ds1 = ExcelToDataset($"{path}", "select * from [Info$]");
            var a = ds1.Tables[0].Rows[6].ItemArray[0];
            var b = ds1.Tables[0].Rows[6].ItemArray[1];
            var c = ds1.Tables[0].Rows[6].ItemArray[2];
            DataSet ds = ExcelToDataset($"{path}", $"select * from [Cycle_{a}_{b}_{c}$]");
            //string[] nameTable = URL.Split('.','/') ;
            //List<string> a = new List<string>();
            Charge = ds.Tables[0].Rows[0].ItemArray[2].ToString();
            Discharge = ds.Tables[0].Rows[0].ItemArray[3].ToString();
            Recharge = ds.Tables[0].Rows[1].ItemArray[2].ToString();
            // MessageBox.Show(Discharge);
            Config["Charge"] = (Convert.ToDecimal(Discharge)).ToString("#0.00");
            Config["Discharge"] = (Convert.ToDecimal(Discharge)).ToString("#0.00");
            Config["Recharge"] = (Convert.ToDecimal(Recharge)).ToString("#0.00");
            //MessageBox.Show(Config["Charge"].ToString());
            //MessageBox.Show(Config["Discharge"].ToString());
            //MessageBox.Show(Config["Recharge"].ToString());
            flag = true;

        }
        public void ChangeFilePosition()
        {
            //flag = false;
            //conn.Close();
            //string[] a = _path.Split('\\');
            if ((bool)Config["TestResultFlag"])
            {
                string log = $"SN: {Config["SN"]}, 设备： {Config["设备"]}，单元： {Config["单元"]}， Time:{DateTime.Now:yy-MM-dd  HH:mm:ss} \nOK \n\n";
                Console.WriteLine(log);
                string checkExits = $"{path_OK}\\{Config["SN"]}.xls";
                DeleteFile(checkExits);
                File.Move(_path, $"{path_OK}\\{Config["SN"]}.xls");
            }

            if (!(bool)Config["TestResultFlag"])
            {
                if (!(((string)Config["MesError"]).Contains("OK")))
                {
                    string log = File.ReadAllText(log_MES);
                    string m = $"设备： {Config["设备"]}，单元： {Config["单元"]}，{Config["MesError"]}";
                    Console.WriteLine(m);
                    log += m;
                    File.WriteAllText(log_MES, log);
                    try
                    {
                        string checkExits = $@"{ path_MES_Error }\{Config["SN"]}.xls";
                        DeleteFile(checkExits);
                        File.Move(_path, $"{path_MES_Error}\\{Config["SN"]}.xls");
                    }

                    catch (Exception ex)
                    {
                        string a1 = $"SN: {Config["SN"]}, 设备： {Config["设备"]}，单元： {Config["单元"]}，Time:{DateTime.Now:yy-MM-dd  HH:mm:ss} \n{ex.ToString()} \n\n";
                        log += a1;
                        File.WriteAllText(log_MES, log);
                        MessageBox.Show(a1);
                    }
                }

                else
            {
                string log = File.ReadAllText(log_NG);
                string m = $"SN: {Config["SN"]}, 设备： {Config["设备"]}，单元： {Config["单元"]}，Time:{DateTime.Now:yy-MM-dd  HH:mm:ss} \n充电容量={Config["Charge"]}   放电容量={Config["Discharge"]}  回充容量={Config["Recharge"]}\n\n";
                Console.WriteLine(m);
                log += m;
                File.WriteAllText(log_NG, log);
                
                    try
                    {
                        string checkExits = $"{ path_NG }\\{Config["SN"]}.xls";
                        //MessageBox.Show(checkExits);
                        DeleteFile(checkExits);
                           
                        Thread.Sleep(100);
                        File.Move(_path, $"{path_NG}\\{Config["SN"]}.xls");
                    }

                    catch (Exception ex)
                    {
                        string a1 = $"SN: {Config["SN"]}, 设备： {Config["设备"]}，单元： {Config["单元"]}，Time:{DateTime.Now:yy-MM-dd  HH:mm:ss} \n{ex.ToString()} \n\n";
                        log += a1;
                        File.WriteAllText(log_NG, log);
                        MessageBox.Show(a1);
                    }
                    //MessageBox.Show((string)Config["SN"]);
                }
                
            }
        }
        public void DeleteFile(string path)
        {
            //path = $@"{ path_MES_Error }\{Config["SN"]}.xls";
            if ((File.Exists(path)))
                File.Delete(path);
        }

        OleDbConnection conn;
        public DataSet ExcelToDataset(string filepath, string strExcel)
        {
            string strconn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filepath.Trim() + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1'";
            conn = new OleDbConnection(strconn);
            conn.Open();
            //var a = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            //string strExcel = "";
            DataSet ds = new DataSet();

            //strExcel = "select * from [" + TableName + "$]";
            OleDbDataAdapter mycommand = new OleDbDataAdapter(strExcel, conn);
            mycommand.Fill(ds);
            conn.Close();

            return ds;
        }

    }
    //class INIHelper
    //{
    //    public string _path;
    //    [DllImport("kernel32.dll")]
    //    private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVar, int size, string INIpath);
    //    public string GetValue(string section, string key)
    //    {

    //        StringBuilder var = new StringBuilder(512);
    //        int length = GetPrivateProfileString(section, key, "", var, 512, _path);
    //        if (length <= 0) SetValue(section, key, "");
    //        return var.ToString().Trim();
    //    }

    //    [DllImport("kernel32.dll")]
    //    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

    //    public long SetValue(string section, string Key, string value)
    //    {
    //        return WritePrivateProfileString(section, Key, value, _path);
    //    }
    //    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    //    [return: MarshalAs(UnmanagedType.Bool)]     //可以没有此行
    //    public static extern bool WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);


    //}
}
