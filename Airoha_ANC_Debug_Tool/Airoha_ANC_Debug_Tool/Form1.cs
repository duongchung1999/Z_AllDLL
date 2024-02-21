using MerryDllFramework;
using MerryTest.testitem;
using Microsoft.VisualBasic;
using SoundCheck_V1.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace Airoha_ANC_Debug_Tool
{
    public partial class Form1 : Form
    {
        UIAdaptiveSize uisize;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            uisize = new UIAdaptiveSize
            {
                Width = Width,
                Height = Height,
                FormsName = this.Text,
                X = Width,
                Y = Height,
            };
            uisize.SetInitSize(this);


        }
        GetHandle GetDeviceClass = new GetHandle();
        Dictionary<string, MerryTest.testitem.UsbInfo.Info> DevList = new Dictionary<string, MerryTest.testitem.UsbInfo.Info>();
        CallDevice cmd = new CallDevice();
        string _msg = "";
        string sqcPath = "";
        string WriteMsg
        {
            get { return _msg; }
            set
            {
                _msg = value;
                AB_tb_log.Text += $"{DateTime.Now.ToString("HH:mm:ss")} | Wirte | {_msg}\r\n";
            }
        }
        string ReadMsg
        {
            get { return _msg; }
            set
            {
                _msg = value;
                AB_tb_log.Text += $"{DateTime.Now.ToString("HH:mm:ss")} | Read  | {_msg}\r\n";
            }
        }


        private async void Form1_Load(object sender, EventArgs e)
        {
            await Task.Run(() => Thread.Sleep(500));
            tabPage.Enabled = false;
            int.TryParse(Hini.GetValue("Forms", "tabPageSelectIndex"), out int selectTabIndex);
            tabPage.SelectedIndex = selectTabIndex;
            AB1585();
            BT2200();
            GetDevceList();
            LoadConfig();
            List<string> postList = new List<string>();
            string PortName = "";
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                Console.WriteLine(searcher);
                var portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
                var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();
                foreach (string s in portList)
                {
                    postList.Add(s.Substring(0, s.LastIndexOf('(')));
                    if (s.Contains("Prolific USB-to-Serial Comm Port"))
                        PortName = s;
                }
            }
            cb_ComPortList.Items.AddRange(postList.ToArray());
            if (cb_ComPortList.Items.Count > 0)
                cb_ComPortList.Text = PortName;


            bool.TryParse(Hini.GetValue("Forms", "TopMost"), out bool result);
            this.TopMost = result;
            bt_TopMost.Text = bt_Topm2.Text = this.TopMost ? "取消置顶" : "程序置顶";
            tabPage.Enabled = true;
            //RefreshingFlag = true;

        }
        List<object> doubles = new List<object>();
        List<object> doubleb = new List<object>();

        void AB1585()
        {

            AB_tb_log.Text = "";
            gb_Dev_lb_DeviceList.Items.Clear();
            AB_cb_LFB.Items.Clear();
            AB_cb_LFF.Items.Clear();
            AB_cb_RFB.Items.Clear();
            AB_cb_RFF.Items.Clear();
            AB_cb_L_SPK_Gain.Items.Clear();
            AB_cb_R_SPK_Gain.Items.Clear();
            AB_cb_MaxGain.Items.Clear();
            AB_cb_MinGain.Items.Clear();
            for (double i = 6; i > 0; i -= 0.1)
            {
                i = Math.Round(i, 1);
                doubles.Add(i);


            }
            for (double i = 0; i >= -90; i -= 0.1)
            {

                i = Math.Round(i, 1);
                doubles.Add(i);

            }
            for (double i = 4; i >= -4; i -= 0.1)
            {
                i = Math.Round(i, 1);
                doubleb.Add(i);


            }

            AB_cb_LFB.Items.AddRange(doubles.ToArray());
            AB_cb_LFF.Items.AddRange(doubles.ToArray());
            AB_cb_RFB.Items.AddRange(doubles.ToArray());
            AB_cb_RFF.Items.AddRange(doubles.ToArray());
            AB_cb_MaxGain.Items.AddRange(doubles.ToArray());
            AB_cb_MinGain.Items.AddRange(doubles.ToArray());
            AB_cb_L_SPK_Gain.Items.AddRange(doubleb.ToArray());
            AB_cb_R_SPK_Gain.Items.AddRange(doubleb.ToArray());

            AB_cb_L_SPK_Gain.SelectedIndex = 40;
            AB_cb_R_SPK_Gain.SelectedIndex = 40;
            AB_cb_LFB.SelectedIndex = 60;
            AB_cb_LFF.SelectedIndex = 60;
            AB_cb_RFB.SelectedIndex = 60;
            AB_cb_RFF.SelectedIndex = 60;
            AB_cb_MaxGain.SelectedIndex = 0;
            AB_cb_MinGain.SelectedIndex = 120;






        }

        void BT2200()
        {

            cb_ComPortList.Items.Clear();

            BT_tb_log.Text = "";

            BT_cb_LFB.Items.Clear();
            BT_cb_LFF.Items.Clear();
            BT_cb_RFB.Items.Clear();
            BT_cb_RFF.Items.Clear();
            BT_cb_L_SPK_Gain.Items.Clear();
            BT_cb_R_SPK_Gain.Items.Clear();
            BT_cb_MaxGain.Items.Clear();
            BT_cb_MinGain.Items.Clear();



            BT_cb_LFB.Items.AddRange(doubles.ToArray());
            BT_cb_LFF.Items.AddRange(doubles.ToArray());
            BT_cb_RFB.Items.AddRange(doubles.ToArray());
            BT_cb_RFF.Items.AddRange(doubles.ToArray());
            BT_cb_MaxGain.Items.AddRange(doubles.ToArray());
            BT_cb_MinGain.Items.AddRange(doubles.ToArray());
            BT_cb_L_SPK_Gain.Items.AddRange(doubleb.ToArray());
            BT_cb_R_SPK_Gain.Items.AddRange(doubleb.ToArray());
            BT_cb_L_SPK_Gain.SelectedIndex = 40;
            BT_cb_R_SPK_Gain.SelectedIndex = 40;
            BT_cb_LFB.SelectedIndex = 60;
            BT_cb_LFF.SelectedIndex = 60;
            BT_cb_RFB.SelectedIndex = 60;
            BT_cb_RFF.SelectedIndex = 60;
            BT_cb_MaxGain.SelectedIndex = 0;
            BT_cb_MinGain.SelectedIndex = 120;


        }
        void GetDevceList()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (tabPage.SelectedIndex != 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    List<string> _list = new List<string>();
                    _list.AddRange(DevList.Keys);

                    GetDeviceClass.GetHidDevicePathIlst(_list, out Dictionary<string, MerryTest.testitem.UsbInfo.Info> deviceList);
                    foreach (var item in deviceList)
                    {

                        string icppath = "";



                        foreach (var icp in _list)
                        {
                            if (icp.Contains(item.Key))
                            {
                                icppath = icp;
                            }
                        }
                        _list.Remove(icppath);


                        if (item.Value.ProductName == null || item.Key == "" || item.Value.ProductName == "")
                        {
                            continue;
                        }

                        string Name = $"{item.Value.ProductName} ( I:{item.Value.I}, O:{item.Value.O}, F:{item.Value.F}) ({item.Value.Path} )";




                        if (DevList.ContainsKey(Name))
                            continue;


                        DevList[Name] = item.Value;
                        gb_Dev_lb_DeviceList.Items.Add(Name);



                    }
                    if (_list.Count <= 0)
                        continue;
                    List<int> removeIndex = new List<int>();
                    foreach (var item in _list)
                    {
                        DevList.Remove(item);
                        gb_Dev_lb_DeviceList.Items.Remove(item);
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        void LoadConfig()
        {


            string[] strs = Hini.INIGetAllSectionNames();
            sqcPath = Hini.GetValue("_path", "SqcPath");
            Dictionary<string, Dictionary<string, string>> config = new Dictionary<string, Dictionary<string, string>>();
            foreach (string str in strs)
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

                string[] keyvalue = Hini.INIGetAllItems(str);
                foreach (var item in keyvalue)
                {
                    string[] ss = item.Split('=');
                    keyValuePairs[ss[0]] = ss[1];
                }
                config[str] = keyValuePairs;
            }

            foreach (var item in config)
            {
                if (item.Key.Contains("TextBox"))
                {

                    foreach (var keyValuePair in item.Value)
                    {
                        TextBox tb = (TextBox)this.gb_CMD.Controls[keyValuePair.Key];
                        if (tb == null)
                        {
                            continue;
                        }
                        List<byte> value = new List<byte>();
                        for (int i = 0; i < keyValuePair.Value.Length; i += 2)
                        {
                            value.Add(Convert.ToByte(keyValuePair.Value.Substring(i, 2), 16));
                        }
                        string Text = Encoding.UTF8.GetString(value.ToArray());

                        tb.Text = Text;
                    }
                }
                else if (item.Key.Contains("Button"))
                {

                    foreach (var keyValuePair in item.Value)
                    {
                        Button bt = (Button)this.gb_CMD.Controls[keyValuePair.Key];

                        if (bt == null)
                        {
                            continue;
                        }
                        List<byte> value = new List<byte>();
                        for (int i = 0; i < keyValuePair.Value.Length; i += 2)
                        {
                            value.Add(Convert.ToByte(keyValuePair.Value.Substring(i, 2), 16));
                        }
                        string Text = Encoding.UTF8.GetString(value.ToArray());

                        bt.Text = Text;
                    }
                }


            }
            foreach (var item in config)
            {
                if (item.Key.Contains("TextBox"))
                {

                    foreach (var keyValuePair in item.Value)
                    {
                        TextBox tb = (TextBox)this.gb_BT_CMD.Controls[keyValuePair.Key];
                        if (tb == null)
                        {
                            continue;
                        }
                        List<byte> value = new List<byte>();
                        for (int i = 0; i < keyValuePair.Value.Length; i += 2)
                        {
                            value.Add(Convert.ToByte(keyValuePair.Value.Substring(i, 2), 16));
                        }
                        string Text = Encoding.UTF8.GetString(value.ToArray());

                        tb.Text = Text;
                    }
                }
                else if (item.Key.Contains("Button"))
                {

                    foreach (var keyValuePair in item.Value)
                    {
                        Button bt = (Button)this.gb_BT_CMD.Controls[keyValuePair.Key];

                        if (bt == null)
                        {
                            continue;
                        }
                        List<byte> value = new List<byte>();
                        for (int i = 0; i < keyValuePair.Value.Length; i += 2)
                        {
                            value.Add(Convert.ToByte(keyValuePair.Value.Substring(i, 2), 16));
                        }
                        string Text = Encoding.UTF8.GetString(value.ToArray());

                        bt.Text = Text;
                    }
                }
                else if (item.Key.Contains("CheckBox"))
                {
                    foreach (var keyValuePair in item.Value)
                    {
                        CheckBox cb = (CheckBox)this.gb_BT_CMD.Controls[keyValuePair.Key];

                        if (cb == null)
                        {
                            continue;
                        }
                        bool.TryParse(keyValuePair.Value, out bool result);
                        cb.Checked = result;
                    }
                }



            }
            string BD = Hini.GetValue("TextBox", $"{BT_gb_tb_Address.Name}");
            if (BD.Trim().Length > 0)
                BT_gb_tb_Address.Text = BD;
            string UUID = Hini.GetValue("TextBox", $"{BT_gb_tb_UUID.Name}");
            if (UUID.Trim().Length > 0)
                BT_gb_tb_UUID.Text = UUID;
        }


        private void gb_CMD_tb_DoubleClick(object sender, EventArgs e)
        {
            TextBox text = (TextBox)sender;
            //遍历所有控件
            string[] tbName = text.Name.Split('_');
            if (tbName.Length < 4)
                return;
            Button btn = null;

            if (text.Name.Contains("AB"))
            {
                String Name = $"AB_gb_CMD_bt_{tbName[4]}";

                Control C = this.gb_CMD.Controls[Name];
                btn = (Button)C;

            }
            else if (text.Name.Contains("BT"))
            {
                String Name = $"BT_gb_CMD_bt_{tbName[4]}";
                Control C = this.gb_BT_CMD.Controls[Name];
                btn = (Button)C;
            }

            if (btn == null)
                return;

            string Barcode = Interaction.InputBox("请输入注释", "提示", $"{btn.Text}", -1, -1);
            if (Barcode != "")
                btn.Text = Barcode;
        }

        private void bt_Save_CMD_Click(object sender, EventArgs e)
        {

            foreach (Control control in this.gb_CMD.Controls)
            {
                Type t = control.GetType();

                if (t == typeof(Button))
                {
                    Button btn = (Button)control;
                    byte[] ASCIIvalue = Encoding.UTF8.GetBytes(btn.Text);
                    string asc = "";
                    foreach (var item in ASCIIvalue)
                    {
                        asc += $"{item.ToString("x2")}";
                    }

                    Hini.SetValue("Button", btn.Name, asc);
                }
                if (t == typeof(TextBox))
                {
                    TextBox tb = (TextBox)control;
                    byte[] ASCIIvalue = Encoding.UTF8.GetBytes(tb.Text);
                    string asc = "";
                    foreach (var item in ASCIIvalue)
                    {
                        asc += $"{item.ToString("x2")}";
                    }
                    Hini.SetValue("TextBox", tb.Name, asc);
                }
            }

            foreach (Control control in this.gb_BT_CMD.Controls)
            {
                Type t = control.GetType();

                if (t == typeof(Button))
                {
                    Button btn = (Button)control;
                    byte[] ASCIIvalue = Encoding.UTF8.GetBytes(btn.Text);
                    string asc = "";
                    foreach (var item in ASCIIvalue)
                    {
                        asc += $"{item.ToString("x2")}";
                    }

                    Hini.SetValue("Button", btn.Name, asc);
                }
                if (t == typeof(TextBox))
                {
                    TextBox tb = (TextBox)control;
                    byte[] ASCIIvalue = Encoding.UTF8.GetBytes(tb.Text);
                    string asc = "";
                    foreach (var item in ASCIIvalue)
                    {
                        asc += $"{item.ToString("x2")}";
                    }
                    Hini.SetValue("TextBox", tb.Name, asc);
                }
                if (t == typeof(CheckBox))
                {

                    CheckBox tb = (CheckBox)control;
                    byte[] ASCIIvalue = Encoding.UTF8.GetBytes(tb.Text);
                    string asc = "";
                    foreach (var item in ASCIIvalue)
                    {
                        asc += $"{item.ToString("x2")}";
                    }
                    Hini.SetValue("CheckBox", $"{tb.Name}", $"{tb.Checked}");
                }
            }

            Hini.SetValue("TextBox", $"{BT_gb_tb_UUID.Name}", $"{BT_gb_tb_UUID.Text}");
            Hini.SetValue("TextBox", $"{BT_gb_tb_Address.Name}", $"{BT_gb_tb_Address.Text}");







        }



        private void gb_CMD_bt_Click(object sender, EventArgs e)
        {
            int index = gb_Dev_lb_DeviceList.SelectedIndex;
            if (index < 0)
                return;




            try
            {
                Button btn = (Button)sender;

                //遍历所有控件
                string[] tbName = btn.Name.Split('_');

                string cmdstr = "";
                string tx = gb_CMD.Controls[$"AB_gb_CMD_tb_{tbName[4]}"].Text.Replace(" ", "").Replace("\r\n", "");
                if (tx == "")
                    return;
                for (int i = 0; i < tx.Length; i += 2)
                {
                    cmdstr += $"{tx.Substring(i, 2)} ";
                }
                cmdstr = cmdstr.Trim();
                cmd.SendReport(WriteMsg = cmdstr, "");
                ReadMsg = $"{cmd._allValue}";
            }
            catch
            {

                AB_tb_log.Text += $"False\r\n";
            }

        }

        private void tb_log_TextChanged(object sender, EventArgs e)
        {
            this.AB_tb_log.SelectionStart = this.AB_tb_log.Text.Length;
            this.AB_tb_log.ScrollToCaret();
        }

        private void bt_RunSC_Click(object sender, EventArgs e)
        {

            if (!SoundCheck.IsConnect)
            {
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                    if (thisProc.ProcessName.Contains("MerryTestFramework"))
                        thisProc.Kill();
                string Receive = SoundCheck.ConnectSoundCheck();
                AB_tb_log.Text += $"{DateTime.Now.ToString("HH:mm:ss")} | SC | {Receive}\r\n";
                if (Receive.Contains("False"))
                    return;
            }
            if (RunSQC)
            {
                string Receive = SoundCheck.RunSequence(sqcPath);
                if (Receive.Contains("False"))
                    return;
                Thread.Sleep(3000);
                RunSQC = false;
            }
            bt_RunSC.Enabled = false;
            bt_FroRunSC.Enabled = false;
            Task.Run(() =>
            {
                try
                {
                    string sn = $"{AB_cb_LFB.Text}_{AB_cb_LFF.Text}_{AB_cb_RFB.Text}_{AB_cb_RFF.Text}";
                    SoundCheck.SetSerialNumber(sn);

                    Thread.Sleep(50);
                    SoundCheck.StartTest();
                    Thread.Sleep(200);
                    SoundCheck.GetFinalResults();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    EnableBtn();
                }
            });


        }
        void EnableBtn()
        {
            bt_RunSC.Enabled = true;
            bt_FroRunSC.Enabled = true;
            AB_gb_Gain_bt_Get.Enabled = true;
            AB_gb_Gain_bt_Set.Enabled = true;
            AB_gb_Gain_bt_SaveGain.Enabled = true;
            gb_Dev_lb_DeviceList.Enabled = true;

            bt_RunSC2.Enabled = true;
            BT_bt_FroRunSC2.Enabled = true;
            BT_gb_Gain_bt_Get.Enabled = true;
            BT_gb_Gain_bt_Set.Enabled = true;
            BT_gb_Gain_bt_SaveGain.Enabled = true;


        }
        void DisableBtn()
        {
            bt_RunSC.Enabled = false;
            bt_FroRunSC.Enabled = false;
            AB_gb_Gain_bt_Get.Enabled = false;
            AB_gb_Gain_bt_Set.Enabled = false;
            AB_gb_Gain_bt_SaveGain.Enabled = false;
            gb_Dev_lb_DeviceList.Enabled = false;

            bt_RunSC2.Enabled = false;
            BT_bt_FroRunSC2.Enabled = false;
            BT_gb_Gain_bt_Get.Enabled = false;
            BT_gb_Gain_bt_Set.Enabled = false;
            BT_gb_Gain_bt_SaveGain.Enabled = false;

        }

        private void bt_FroRunSC_Click(object sender, EventArgs e)
        {
            if (AB_cb_MaxGain.SelectedIndex >= AB_cb_MinGain.SelectedIndex)
            {
                AB_tb_log.Text += "debug MAX Gain <= MIN Gain Unable to debug\r\n";
                return;
            }


            if (!SoundCheck.IsConnect)
            {
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                    if (thisProc.ProcessName.Contains("MerryTestFramework"))
                        thisProc.Kill();
                string Receive = SoundCheck.ConnectSoundCheck();
                AB_tb_log.Text += $"{DateTime.Now.ToString("HH:mm:ss")} | SC | {Receive}\r\n";
                if (Receive.Contains("False"))
                    return;
            }
            if (RunSQC)
            {
                string Receive = SoundCheck.RunSequence(sqcPath);
                if (Receive.Contains("False"))
                    return;
                Thread.Sleep(3000);
                RunSQC = false;
            }
            DisableBtn();
            bt_SC_Stop.Enabled = true;
            STOPFlag = false;

            if (AB_rb_FB.Checked)
            {
                cmd.SendReport(WriteMsg = $"06 0B 00 05 5A 07 00 06 0E 00 0A 01 02 01", "05 5B");
                ReadMsg = $"{cmd._allValue}";
            }
            else if (AB_rb_FF.Checked)
            {
                cmd.SendReport(WriteMsg = $"06 0B 00 05 5A 07 00 06 0E 00 0A 01 01 01", "05 5B");
                ReadMsg = $"{cmd._allValue}";
            }
            else if (AB_rb_Hybrid.Checked)
            {
                cmd.SendReport(WriteMsg = $"06 0B 00 05 5A 07 00 06 0E 00 0A 01 00 01", "05 5B");

            }

            Task.Run(() =>
            {
                try
                {
                    AB_cb_LFB.SelectedIndex = 61;
                    AB_cb_RFB.SelectedIndex = 61;
                    AB_cb_LFF.SelectedIndex = 61;
                    AB_cb_RFF.SelectedIndex = 61;
                    Thread.Sleep(200);

                    gb_Gain_bt_Set_Click(null, null);
                    Thread.Sleep(500);
                    AB_cb_LFB.SelectedIndex = 60;
                    AB_cb_RFB.SelectedIndex = 60;
                    AB_cb_LFF.SelectedIndex = 60;
                    AB_cb_RFF.SelectedIndex = 60;
                    Thread.Sleep(200);
                    gb_Gain_bt_Set_Click(null, null);
                    Thread.Sleep((500));
                    double minGain = double.Parse(AB_cb_MinGain.Text);
                    int i = 0;
                    for (int b = 0; b < AB_cb_MaxGain.Items.Count; b++)
                    {
                        if (AB_cb_MaxGain.Items[i].ToString() == AB_cb_MaxGain.Text)
                        {
                            i = b; break;
                        }
                    }


                    for (; i < AB_cb_LFB.Items.Count; i++)
                    {
                        if (STOPFlag)
                            return;
                        string strName = "";

                        if (AB_rb_FB.Checked)
                        {
                            AB_cb_LFB.SelectedIndex = i;
                            AB_cb_RFB.SelectedIndex = i;
                            if (double.Parse(AB_cb_LFB.Text) < minGain)
                            {
                                return;
                            }
                            strName = AB_cb_LFB.Text;
                        }
                        else if (AB_rb_FF.Checked)
                        {
                            AB_cb_LFF.SelectedIndex = i;
                            AB_cb_RFF.SelectedIndex = i;
                            if (double.Parse(AB_cb_LFF.Text) < minGain)
                            {
                                return;
                            }
                            strName = AB_cb_LFF.Text;

                        }
                        Thread.Sleep(100);
                        gb_Gain_bt_Set_Click(null, null);
                        Thread.Sleep(800);

                        SoundCheck.SetSerialNumber(strName);
                        Thread.Sleep(100);

                        string Receive = SoundCheck.StartTest();
                        if (Receive.Contains("False"))
                            return;
                        Thread.Sleep(200);
                        SoundCheck.GetFinalResults();
                        Thread.Sleep(200);
                    }

                }
                catch (Exception ex)
                {
                    AB_tb_log.Text += $"{DateTime.Now.ToString("HH:mm:ss")} | SC | {ex}\r\n";
                    return;
                }
                finally
                {
                    EnableBtn();
                    gb_Gain_bt_Get_Click(null, null);

                }
            });



        }
        bool STOPFlag = false;
        private void bt_SC_Stop_Click(object sender, EventArgs e)
        {
            STOPFlag = true;
        }

        private void gb_Gain_bt_Get_Click(object sender, EventArgs e)
        {
            string value = "False";
            if (AB_rb_Master.Checked)
            {
                value = cmd.SendReport(WriteMsg = "06 08 00 05 5a 04 00 06 0e 00 0d", "07 10 00 05 5B 0C 00 06 0E 00", "11 12 13 14 15 16 17 18");
            }
            else if (AB_rb_Slave.Checked)
            {
                value = cmd.SendReport(WriteMsg = "06 08 80 05 5a 04 00 06 0e 00 0d", "07 10 80 05 5B 0C 00 06 0E 00", "11 12 13 14 15 16 17 18");
            }
            else if (AB_rb_Both.Checked)
            {
                value = cmd.SendReport(WriteMsg = "06 08 00 05 5a 04 00 06 0e 00 0d", "07 10 00 05 5B 0C 00 06 0E 00", "11 12 13 14 15 16 17 18");

            }
            ReadMsg = $"{cmd._allValue}";
            if (value.Contains("False"))
            {
                return;
            }
            string[] HexGain = value.Split(' ');
            AB_cb_LFF.Text = $"{HexGain[0]}{HexGain[1]}".HEXGainToGain().ToString();
            AB_cb_LFB.Text = $"{HexGain[2]}{HexGain[3]}".HEXGainToGain().ToString();
            AB_cb_RFF.Text = $"{HexGain[4]}{HexGain[5]}".HEXGainToGain().ToString();
            AB_cb_RFB.Text = $"{HexGain[6]}{HexGain[7]}".HEXGainToGain().ToString();


        }
        private void gb_Gain_bt_Set_Click(object sender, EventArgs e)
        {
            string value = "False";


            double LFB = Math.Round(Convert.ToDouble(AB_cb_LFB.Text), 1);
            double LFF = Math.Round(Convert.ToDouble(AB_cb_LFF.Text), 1);
            double RFB = Math.Round(Convert.ToDouble(AB_cb_RFB.Text), 1);
            double RFF = Math.Round(Convert.ToDouble(AB_cb_RFF.Text), 1);
            string StrLFB = $"{LFB.VolToHEXGain()}";
            string StrLFF = $"{LFF.VolToHEXGain()}";
            string StrRFB = $"{RFB.VolToHEXGain()}";
            string StrRFF = $"{RFF.VolToHEXGain()}";

            if (AB_rb_Master.Checked)
            {
                value = cmd.SendReport(WriteMsg = $"06 10 00 05 5a 0c 00 06 0e 00 0c {StrLFF} {StrLFB} {StrRFF} {StrRFB}", "07 10 00 05 5B 0C 00 06 0E 00", "11 12 13 14 15 16 17 18");
            }
            else if (AB_rb_Slave.Checked)
            {
                value = cmd.SendReport(WriteMsg = $"06 10 80 05 5a 0c 00 06 0e 00 0c {StrLFF} {StrLFB} {StrRFF} {StrRFB}", "07 10 80 05 5B 0C 00 06 0E 00", "11 12 13 14 15 16 17 18");
            }
            else if (AB_rb_Both.Checked)
            {
                value = cmd.SendReport(WriteMsg = $"06 10 00 05 5a 0c 00 06 0e 00 0c {StrLFF} {StrLFB} {StrRFF} {StrRFB}", "07 10 00 05 5B 0C 00 06 0E 00", "11 12 13 14 15 16 17 18");
                ReadMsg = $" {cmd._allValue} ";
                value = cmd.SendReport(WriteMsg = $"06 10 80 05 5a 0c 00 06 0e 00 0c {StrLFF} {StrLFB} {StrRFF} {StrRFB}", "07 10 80 05 5B 0C 00 06 0E 00", "11 12 13 14 15 16 17 18");
            }
            ReadMsg = $" {cmd._allValue}  ";
            if (value.Contains("False"))
            {
                return;
            }


        }
        private void gb_Gain_bt_SaveGain_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("你正在修改IC的增益 “Gain”，点击“是”增益值会保存在IC。\r\n请确认是否修改", "提示", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                AB_tb_log.Text += "取消修改“Gain”\r\n";

                return;
            }

            string value = "False";


            double LFB = Math.Round(Convert.ToDouble(AB_cb_LFB.Text), 1);
            double LFF = Math.Round(Convert.ToDouble(AB_cb_LFF.Text), 1);
            double RFB = Math.Round(Convert.ToDouble(AB_cb_RFB.Text), 1);
            double RFF = Math.Round(Convert.ToDouble(AB_cb_RFF.Text), 1);
            string StrLFB = $"{LFB.VolToHEXGain()}";
            string StrLFF = $"{LFF.VolToHEXGain()}";
            string StrRFB = $"{RFB.VolToHEXGain()}";
            string StrRFF = $"{RFF.VolToHEXGain()}";

            if (AB_rb_Master.Checked)
            {
                value = cmd.SendReport(WriteMsg = $"06 10 00 05 5a 0c 00 06 0e 00 0e {StrLFF} {StrLFB} {StrRFF} {StrRFB}", "07 10 00 05 5B 0C 00 06 0E 00", "11 12 13 14 15 16 17 18");
            }
            else if (AB_rb_Slave.Checked)
            {
                value = cmd.SendReport(WriteMsg = $"06 10 80 05 5a 0c 00 06 0e 00 0e {StrLFF} {StrLFB} {StrRFF} {StrRFB}", "07 10 80 05 5B 0C 00 06 0E 00", "11 12 13 14 15 16 17 18");
            }
            else if (AB_rb_Both.Checked)
            {
                value = cmd.SendReport(WriteMsg = $"06 10 00 05 5a 0c 00 06 0e 00 0e {StrLFF} {StrLFB} {StrRFF} {StrRFB}", "07 10 00 05 5B 0C 00 06 0E 00", "11 12 13 14 15 16 17 18");
                ReadMsg = $" {cmd._allValue} ";
                value = cmd.SendReport(WriteMsg = $"06 10 80 05 5a 0c 00 06 0e 00 0e {StrLFF} {StrLFB} {StrRFF} {StrRFB}", "07 10 80 05 5B 0C 00 06 0E 00", "11 12 13 14 15 16 17 18");
            }
            ReadMsg = $" {cmd._allValue}  ";
            if (value.Contains("False"))
            {
                return;
            }



        }


        bool RunSQC = false;
        private void bt_SQC_Click(object sender, EventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = $"所有文件(*.sqc)|*.sqc";
            dialog.InitialDirectory = sqcPath;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                RunSQC = true;
                string path_ = sqcPath = dialog.FileName;
                Hini.SetValue("_path", "SqcPath", dialog.FileName);
            }
        }

        private void gb_Dev_lb_DeviceList_SelectedValueChanged(object sender, EventArgs e)
        {
            int index = gb_Dev_lb_DeviceList.SelectedIndex;
            if (index < 0)
                return;


            cmd.info = DevList[gb_Dev_lb_DeviceList.Items[index].ToString()];

            if (cmd.info.I == 62 && cmd.info.O == 62)
            {

                gb_Gain_bt_Get_Click(null, null);
            }
        }

        private void bt_LFB_Click(object sender, EventArgs e)
        {
            Curves(OpenFile(), "L_FB_Curve");

        }

        private void bt_RFB_Click(object sender, EventArgs e)
        {
            Curves(OpenFile(), "R_FB_Curve");

        }

        private void bt_LFF_Click(object sender, EventArgs e)
        {
            Curves(OpenFile(), "L_FF_Curve");

        }

        private void bt_RFF_Click(object sender, EventArgs e)
        {
            Curves(OpenFile(), "R_FF_Curve");

        }

        string TypeName = "MX001";
        void Curves(string FilePath, string supplementFileName)
        {
            #region MyRegion
            if (!File.Exists(FilePath)) { return; }
            string Barcode = Interaction.InputBox("请输入机型，会生产带有机型名称的文件夹", "提示", TypeName, -1, -1);
            if (Barcode.Length <= 0)
            {
                MessageBox.Show("未输入机型名称，取消生成");
                tb_data.Text += "取消生成\r\n";
                return;
            }
            TypeName = Barcode;
            string FileName = Path.GetFileName(FilePath);
            string[] Ldata = File.ReadAllLines(FilePath);
            Dictionary<string, Dictionary<double, double[]>> LearTrimDataL = new Dictionary<string, Dictionary<double, double[]>>();

            for (int row = 0; row < Ldata.Length; row++)
            {
                Dictionary<double, double[]> rowDataL = new Dictionary<double, double[]>();

                //将1行的数据切割出来
                List<double> doubleDataList = new List<double>();
                string[] Data = Ldata[row].Split('\t');

                string calibration = Data[0];



                for (int i = 1; i < Data.Length - 1; i++)
                {

                    doubleDataList.Add(Math.Round(double.Parse(Data[i]), 2));
                }

                for (int i = 0; i < Ldata.Length; i++)
                {
                    //将1行的数据切割出来
                    List<double> subtracted = new List<double>();
                    string[] subtractedData = Ldata[i].Split('\t');

                    for (int c = 1; c < subtractedData.Length - 1; c++)
                    {

                        subtracted.Add(Math.Round(double.Parse(subtractedData[c]), 2));
                    }
                    List<double> diffData = new List<double>();

                    for (int j = 0; j < subtracted.Count; j++)
                    {
                        double diffdouble = Math.Round(doubleDataList[j] - subtracted[j], 2);

                        diffData.Add(diffdouble);
                    }
                    rowDataL[Math.Round(double.Parse(subtractedData[0]), 2)] = diffData.ToArray();
                }
                LearTrimDataL[calibration] = rowDataL;

            }

            StringBuilder ConsoleStrL = new StringBuilder();
            StringBuilder txtData = new StringBuilder();



            ConsoleStrL.Append(@"Dictionary<double, Dictionary<double, double[]>> " + supplementFileName + @"GianCardinal = new Dictionary<double, Dictionary<double, double[]>>()
{
");
            foreach (var item in LearTrimDataL)
            {

                ConsoleStrL.Append("    { " + item.Key + ",new Dictionary<double,double[]>(){ \r\n");
                foreach (var data in item.Value)
                {
                    txtData.Append($"{item.Key}&{data.Key},");

                    string doubleStr = "";
                    for (int i = 0; i < data.Value.Length; i++)
                    {
                        doubleStr += $"{data.Value[i]},";
                        txtData.Append($"{data.Value[i]},");
                    }
                    ConsoleStrL.Append(@"        { " + data.Key + ", new double[] { " + doubleStr + "} },\r\n");
                    txtData.Append("\r\n");

                }
                txtData.Append("\r\n");

                ConsoleStrL.Append(" }},\r\n");

            }
            ConsoleStrL.Append(@"};");
            Directory.CreateDirectory($@".\{TypeName}");
            File.WriteAllText($@".\{TypeName}\{supplementFileName}_Dictionary.txt", ConsoleStrL.ToString());
            tb_data.Text += $"{supplementFileName}_Dictionary.txt 生成完成\r\n";

            File.WriteAllText($@".\{TypeName}\{supplementFileName}_data.txt", txtData.ToString());
            tb_data.Text += $"{supplementFileName}_data.txt 生成完成\r\n";

            MessageBox.Show("OK");
            #endregion
        }

        OpenFileDialog openFileDialog2 = new OpenFileDialog
        {
            //设置打开的文件的类型，注意过滤器的语法
            Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
            //设置这个对话框的起始位置

            InitialDirectory = Path.GetDirectoryName($@"{System.Windows.Forms.Application.ExecutablePath}")
        };
        string OpenFile()
        {
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                //获取用户选择的文件完整路径，并存储到一个字符串数组中
                string[] paths = openFileDialog2.FileNames;

                foreach (string path in paths)
                {
                    openFileDialog2.InitialDirectory = Path.GetDirectoryName(path);
                    //获取文件路径
                    return path;

                }

            }
            return "";
        }

        private void bt_OpenDirectory_Click(object sender, EventArgs e)
        {
            string v_OpenFolderPath = $@".\";
            System.Diagnostics.Process.Start("explorer.exe", v_OpenFolderPath);
        }

        private void bt_TopMost_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
            Hini.SetValue("Forms", "TopMost", $"{this.TopMost}");
            bt_TopMost.Text = bt_Topm2.Text = this.TopMost ? "取消置顶" : "程序置顶";
        }

        private void cb_ComPortList_Click(object sender, EventArgs e)
        {

        }

        private void cb_ComPortList_DropDown(object sender, EventArgs e)
        {
            cb_ComPortList.Items.Clear();
            List<string> postList = new List<string>();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                var portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
                var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();
                foreach (string s in portList)
                {
                    string port = s.Substring(0, s.LastIndexOf('('));
                    postList.Add(port);
                }
            }
            cb_ComPortList.Items.AddRange(postList.ToArray());
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Hini.SetValue("Forms", "tabPageSelectIndex", tabPage.SelectedIndex.ToString());

            bt_Save_CMD_Click(null, null);

        }

        //####################################################################  BT ####################################################################


        #region BT

        SerialPort BTPort = new SerialPort();

        private void BT_gb_bt_OpenPort_Click(object sender, EventArgs e)
        {
            if (!BTPort.IsOpen)
            {
                string PortName = cb_ComPortList.Text.Substring(0, cb_ComPortList.Text.IndexOf(" "));
                BTPort = new SerialPort
                {
                    PortName = PortName,
                    BaudRate = 921600,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Parity = Parity.None
                };

                if (!BTPort.IsOpen) BTPort.Open();
                BTPort.DataReceived += BTPort_DataReceived;
                BT_gb_bt_OpenPort.BackColor = Color.Tomato;
                BT_gb_bt_OpenPort.Text = "Close";

                BT_gb_bt_MacPair.Enabled = true;
                BT_gb_bt_AutoMaticPair.Enabled = true;
                BT_gb_bt_Disconnect.Enabled = true;
                BT_gb_bt_EnterA2DP.Enabled = true;
                BT_gb_bt_EnterHFP.Enabled = true;
                BT_gb_bt_SPPCon.Enabled = true;
                BT_gb_bt_RST.Enabled = true;
                BT_gb_bt_Power.Enabled = true;
                BT_gb_bt_SetUUID.Enabled = true;
                BT_gb_bt_SYSInfo.Enabled = true;
                BT_gb_tb_UUID.Enabled = true;
                BT_gb_tb_Address.Enabled = true;
                gb_BT_CMD.Enabled = true;
            }
            else
            {
                BTPort.Close();
                BTPort.Dispose();
                BT_gb_bt_OpenPort.Text = "Open";
                BT_gb_bt_OpenPort.BackColor = Color.Lime;

            }
        }


        List<string> PortlogStr = new List<string>();
        List<string> PortlogBytes = new List<string>();


        private void BTPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (ReadFlag)
                return;
            SerialPort serialPort = sender as SerialPort;
            Thread.Sleep(10);
            byte[] l = new byte[serialPort.BytesToRead];
            serialPort.Read(l, 0, l.Length);
            if (l.Length <= 0)
                return;

            StringBuilder sb = new StringBuilder();
            foreach (byte b in l)
                sb.Append($" {b.ToString("x2").PadLeft(2, '0')}");
            string str = $"{DateTime.Now:HH:mm:ss}|Read|ASCII|{Encoding.ASCII.GetString(l)}";
            if (str.Substring(str.Length - 2, 2).Contains("\r\n"))
            {
                str = str.Substring(0, str.Length - 2);
            }
            if (str.Substring(0, 1).Contains(" "))
            {
                str = str.Substring(0, 1);
            }
            string strb = $"{DateTime.Now:HH:mm:ss}|Read|Byte|{sb.ToString().ToUpper()}";
            lock (PortlogStr)
            {
                lock (PortlogBytes)
                {
                    PortlogStr.Add(str);
                    PortlogBytes.Add(strb);
                    if (l.Min() < 9)
                    {
                        BT_tb_log.Text += $"{strb}\r\n";
                    }
                    else
                    {
                        BT_tb_log.Text += $"{str}\r\n";
                    }
                }
            }

        }

        void WriteASCIICMD(string ASCII)
        {
            if (BTPort.IsOpen)
            {
                BTPort.Write($"{ASCII}\r\n");
                BTPort.NewLine = "\r\n";
                string str = $"{DateTime.Now:HH:mm:ss}|Write|{ASCII}";
                BT_tb_log.Text += $"{str}\r\n";
            }
            else
            {
                BT_tb_log.Text += $"{false}\r\n";

            }
        }
        void WriteByteCMD(string ByteCMD)
        {
            List<byte> l = new List<byte>();
            string _ByteCMD = ByteCMD.Replace(" ", "").Trim();
            for (int i = 0; i < _ByteCMD.Length; i += 2)
            {
                l.Add(Convert.ToByte(_ByteCMD.Substring(i, 2), 16));
            }

            if (BTPort.IsOpen)
            {
                BTPort.Write(l.ToArray(), 0, l.Count());
                string str = $"{DateTime.Now:HH:mm:ss}|Write|{ByteCMD}";
                BT_tb_log.Text += $"{str}\r\n";
            }
            else
            {
                BT_tb_log.Text += $"{false}\r\n";
            }
        }
        void ReadLog(out string ReadStr, out string byteStr)
        {
            ReadStr = "";
            byteStr = "";
            if (BTPort.BytesToRead > 0)
            {
                byte[] ReadByte = null;
                byteStr = "";
                ReadByte = new byte[BTPort.BytesToRead];
                BTPort.Read(ReadByte, 0, ReadByte.Length);
                foreach (var item in ReadByte) byteStr += $"{item.ToString("X2").PadLeft(2, '0')} ";
                ReadStr = Encoding.ASCII.GetString(ReadByte);
                if (ReadByte.Min() <= 9)
                {
                    BT_tb_log.Text += $"{DateTime.Now:HH:mm:ss}|Read|Byte|{byteStr}\r\n";

                }
                else
                {
                    BT_tb_log.Text += $"{DateTime.Now:HH:mm:ss}|Read|ASCII|{ReadStr}\r\n";

                }


            }
        }

        private void BT_SendCmd_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (!BTPort.IsOpen) { return; }
                Button bt = (Button)sender;
                if (bt.Name.Contains("BT_gb_bt_SetUUID"))
                {
                    WriteASCIICMD(BT_gb_tb_UUID.Text);

                    return;
                }
                if (bt.Name.Contains("BT_gb_bt_AutoMaticPair"))
                {
                    Connect(">NO_MAC_CON");
                    return;
                }
                if (bt.Name.Contains("BT_gb_bt_MacPair"))
                {
                    Connect($">CONN={BT_gb_tb_Address.Text.ToUpper().Replace(" ", "")}");
                }
                if (bt.Name.Contains("BT_gb_bt_Disconnect"))
                {
                    lb_BTName.Text = "";
                    lb_BDAddress.Text = "";

                    WriteASCIICMD($">DISC");

                }

                if (bt.Name.Contains("BT_gb_bt_SYSInfo"))
                {
                    WriteASCIICMD($">SYS_INFO_GET=?");
                }

                if (bt.Name.Contains("BT_gb_bt_SPPCon"))
                {
                    WriteASCIICMD($">SPP_CONN");
                }


                if (bt.Name.Contains("BT_gb_bt_RST"))
                {
                    WriteASCIICMD($">RST");
                }
                if (bt.Name.Contains("BT_gb_bt_Power"))
                {
                    WriteASCIICMD($">SET_IQY_RANGE=50");
                }
                if (bt.Name.Contains("BT_gb_bt_Disconnect"))
                {
                    WriteASCIICMD($">DISC");
                }

                if (bt.Name.Contains("BT_gb_bt_EnterA2DP"))
                {
                    WriteASCIICMD($">OPEN A2DP");
                }
                if (bt.Name.Contains("BT_gb_bt_EnterHFP"))
                {
                    WriteASCIICMD($">OPEN HFP");
                }

                return;


            });

        }

        bool ReadFlag = false;

        void Connect(string ConnectCMD)
        {
            string readStr = "";
            bool IsConnect = false;
            ReadFlag = true;
            try
            {
                PortlogStr.Clear();
                PortlogBytes.Clear();

                for (int d = 0; d < 1; d++)
                {
                    WriteASCIICMD(ConnectCMD);
                    Thread.Sleep(2000);
                    for (int i = 0; i < 20; i++)
                    {
                        ReadLog(out readStr, out _);
                        if (readStr.Contains("Success") ||
                            readStr.Contains("avrcp_success") ||
                            readStr.Contains("a2dp_connect_success")
                            )
                        {
                            IsConnect = true;
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                }
                if (IsConnect)
                {

                    WriteASCIICMD(">GET_CONN_INFO");
                    Thread.Sleep(1000);

                    ReadLog(out readStr, out _);
                    if (readStr.Contains("DEVICE="))
                    {
                        string[] strs = readStr.Split(new string[] { "DEVICE=" }, StringSplitOptions.None);
                        lb_BDAddress.Text = strs[1].Substring(0, 12).ToUpper();
                        string[] strs2 = readStr.Split(new string[] { "NAME=" }, StringSplitOptions.None);
                        lb_BTName.Text = strs2[1];


                    }
                }
            }
            finally
            {
                ReadFlag = false;
            }

        }

        void gb_BT_CMD_WriteMCD(object sender, EventArgs e)
        {
            if (!BTPort.IsOpen)
                return;
            // "BT_gb_CMD_bt_1";
            CheckBox cb = null;
            TextBox tb = null;
            Button bt = (Button)sender;
            string ids = bt.Name.Split('_')[4];
            string cbName = $"BT_gb_CMD_cb_{ids}";
            string tbName = $"BT_gb_CMD_tb_{ids}";

            cb = (CheckBox)this.gb_BT_CMD.Controls[cbName];
            tb = (TextBox)this.gb_BT_CMD.Controls[tbName];
            if (cb.Checked)
            {
                WriteByteCMD(tb.Text.Trim());

            }
            else
            {
                WriteASCIICMD(tb.Text.Trim());
            }

        }

        private void BT_tb_log_TextChanged(object sender, EventArgs e)
        {
            this.BT_tb_log.SelectionStart = this.BT_tb_log.Text.Length;
            this.BT_tb_log.ScrollToCaret();
        }

        private void BT_gb_Gain_bt_SaveGain_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("你正在修改IC的增益 “Gain”，点击“是”增益值会保存在IC。\r\n请确认是否修改", "提示", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                BT_tb_log.Text += "取消修改“Gain”\r\n";

                return;
            }

            double LFB = Math.Round(Convert.ToDouble(BT_cb_LFB.Text), 1);
            double LFF = Math.Round(Convert.ToDouble(BT_cb_LFF.Text), 1);
            double RFB = Math.Round(Convert.ToDouble(BT_cb_RFB.Text), 1);
            double RFF = Math.Round(Convert.ToDouble(BT_cb_RFF.Text), 1);
            string StrLFB = $"{LFB.VolToHEXGain()}";
            string StrLFF = $"{LFF.VolToHEXGain()}";
            string StrRFB = $"{RFB.VolToHEXGain()}";
            string StrRFF = $"{RFF.VolToHEXGain()}";
            string CMD = $"05 5A 0C 00 06 0E 00 0E {StrLFF} {StrLFB} {StrRFF} {StrRFB}";
            WriteByteCMD(CMD);

        }

        private void BT_gb_Gain_bt_Set_Click(object sender, EventArgs e)
        {
            double LFB = Math.Round(Convert.ToDouble(BT_cb_LFB.Text), 1);
            double LFF = Math.Round(Convert.ToDouble(BT_cb_LFF.Text), 1);
            double RFB = Math.Round(Convert.ToDouble(BT_cb_RFB.Text), 1);
            double RFF = Math.Round(Convert.ToDouble(BT_cb_RFF.Text), 1);
            string StrLFB = $"{LFB.VolToHEXGain()}";
            string StrLFF = $"{LFF.VolToHEXGain()}";
            string StrRFB = $"{RFB.VolToHEXGain()}";
            string StrRFF = $"{RFF.VolToHEXGain()}";
            string CMD = $"05 5A 0C 00 06 0E 00 0c {StrLFF} {StrLFB} {StrRFF} {StrRFB}";
            WriteByteCMD(CMD);
        }

        private void BT_gb_Gain_bt_Get_Click(object sender, EventArgs e)
        {
            if (!BTPort.IsOpen)
            {
                MessageBox.Show("未打开串口");
                return;
            }
            ReadFlag = true;
            try
            {
                WriteByteCMD("05 5A 04 00 06 0E 00 0D");
                Thread.Sleep(250);
                ReadLog(out _, out string ByteStr);
                if (ByteStr.Contains("05 5B 0C 00 06 0E 00 0D"))
                {
                    string[] HexGain = ByteStr.Replace("05 5B 0C 00 06 0E 00 0D ", "").Split(' ');
                    BT_cb_LFF.Text = $"{HexGain[0]}{HexGain[1]}".HEXGainToGain().ToString();
                    BT_cb_LFB.Text = $"{HexGain[2]}{HexGain[3]}".HEXGainToGain().ToString();
                    BT_cb_RFF.Text = $"{HexGain[4]}{HexGain[5]}".HEXGainToGain().ToString();
                    BT_cb_RFB.Text = $"{HexGain[6]}{HexGain[7]}".HEXGainToGain().ToString();
                }



            }
            finally
            {
                ReadFlag = false;
            }

        }



        #endregion

        private void BT_gc_bt_GetSPK_Click(object sender, EventArgs e)
        {
            if (!BTPort.IsOpen)
            {
                MessageBox.Show("未打开串口");
                return;
            }


            ReadFlag = true;
            try
            {


                WriteByteCMD("05 5A 06 00 00 0A 90 E0 38 00");
                Thread.Sleep(250);
                ReadLog(out _, out string ByteStr);
                if (ByteStr.Contains("05 5B 3C 00 00 0A 38 00 0C FE"))
                {
                    string[] HexGain = ByteStr.Replace("05 5B 3C 00 00 0A 38 00 0C FE ", "").Split(' ');
                    BT_cb_L_SPK_Gain.Text = $"{HexGain[0]}{HexGain[1]}".HEXGainToGain().ToString();
                    BT_cb_R_SPK_Gain.Text = $"{HexGain[4]}{HexGain[5]}".HEXGainToGain().ToString();
                }



            }
            finally
            {
                ReadFlag = false;
            }
        }
        bool ShowFlag = true;
        private void BT_gc_bt_SetSPK_Click(object sender, EventArgs e)
        {
            if (!BTPort.IsOpen)
            {
                MessageBox.Show("未打开串口");
                return;
            }
            if (ShowFlag) MessageBox.Show("IHT916 专用，其他机型请先暂停使用");
            ReadFlag = true;
            string ByteStr = "";
            try
            {
                WriteByteCMD("05 5A 02 00 01 0E");
                Thread.Sleep(250);
                ReadLog(out _, out ByteStr);

                WriteByteCMD("05 5A 06 00 00 0A 90 E0 38 00");
                Thread.Sleep(250);
                ReadLog(out _, out ByteStr);

                if (ByteStr.Contains("05 5B 3C 00 00 0A 38 00 0C"))
                {
                    string[] HexGain = ByteStr.Replace("05 5B 3C 00 00 0A 38 00 0C ", "").Split(' ');
                    HexGain[1] = double.Parse(BT_cb_L_SPK_Gain.Text).VolToHEXGain().Split(' ')[0];
                    HexGain[2] = double.Parse(BT_cb_L_SPK_Gain.Text).VolToHEXGain().Split(' ')[1];
                    HexGain[5] = double.Parse(BT_cb_R_SPK_Gain.Text).VolToHEXGain().Split(' ')[0];
                    HexGain[6] = double.Parse(BT_cb_R_SPK_Gain.Text).VolToHEXGain().Split(' ')[1];

                    string SaveCMD = $"05 5A 3C 00 01 0A 90 E0 0C";
                    for (int i = 0; i < HexGain.Length; i++)
                        SaveCMD += $" {HexGain[i]}";

                    WriteByteCMD(SaveCMD.Trim());
                    Thread.Sleep(250);
                    ReadLog(out _, out ByteStr);

                    WriteByteCMD("05 5A 02 00 02 0E");
                    Thread.Sleep(250);
                    ReadLog(out _, out ByteStr);

                }






            }
            finally
            { ReadFlag = false; }




        }
        bool RefreshingFlag;
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (!RefreshingFlag) return;
            var newX = Width;
            var newY = Height;

            uisize.UpdateSize(Width, Height, this);
            uisize.X = newX;
            uisize.Y = newY;
        }

        private void BT_bt_FroRunSC2_Click(object sender, EventArgs e)
        {
            if (BT_cb_MaxGain.SelectedIndex >= BT_cb_MinGain.SelectedIndex)
            {
                BT_tb_log.Text += "debug MAX Gain <= MIN Gain Unable to debug\r\n";
                return;
            }


            if (!SoundCheck.IsConnect)
            {
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                    if (thisProc.ProcessName.Contains("MerryTestFramework"))
                        thisProc.Kill();
                string Receive = SoundCheck.ConnectSoundCheck();
                BT_tb_log.Text += $"{DateTime.Now:HH:mm:ss} | SC | {Receive}\r\n";
                if (Receive.Contains("False"))
                    return;
            }
            if (RunSQC)
            {
                string Receive = SoundCheck.RunSequence(sqcPath);
                if (Receive.Contains("False"))
                    return;
                Thread.Sleep(3000);
                RunSQC = false;
            }
            DisableBtn();
            BT_bt_Stop2.Enabled = bt_SC_Stop.Enabled = true;
            STOPFlag = false;

            if (BT_rb_FB.Checked)
            {
                WriteByteCMD($"05 5A 07 00 06 0E 00 0A 01 02 01");

            }
            else if (BT_rb_FF.Checked)
            {
                WriteByteCMD($"05 5A 07 00 06 0E 00 0A 01 01 01");
            }
            else if (BT_rb_Hybrid.Checked)
            {
                WriteByteCMD($"05 5A 07 00 06 0E 00 0A 01 00 01");
            }

            Task.Run(() =>
            {
                try
                {
                    BT_cb_LFB.SelectedIndex = 61;
                    BT_cb_RFB.SelectedIndex = 61;
                    BT_cb_LFF.SelectedIndex = 61;
                    BT_cb_RFF.SelectedIndex = 61;
                    BT_gb_Gain_bt_Set_Click(null, null);
                    Thread.Sleep((1000));
                    BT_cb_LFB.SelectedIndex = 60;
                    BT_cb_RFB.SelectedIndex = 60;
                    BT_cb_LFF.SelectedIndex = 60;
                    BT_cb_RFF.SelectedIndex = 60;
                    BT_gb_Gain_bt_Set_Click(null, null);
                    Thread.Sleep((1000));
                    double minGain = double.Parse(BT_cb_MinGain.Text);
                    int i = 0;

                    for (int b = 0; b < BT_cb_MaxGain.Items.Count; b++)
                    {
                        if (BT_cb_MaxGain.Items[b].ToString() == BT_cb_MaxGain.Text)
                        {
                            i = b; break;
                        }
                    }




                    for (; i < BT_cb_LFB.Items.Count; i++)
                    {
                        if (STOPFlag)
                            return;
                        string strName = "";
                        if (BT_rb_FB.Checked)
                        {
                            BT_cb_LFB.SelectedIndex = i;
                            BT_cb_RFB.SelectedIndex = i;
                            if (double.Parse(BT_cb_LFB.Text) < minGain)
                            {
                                return;
                            }
                            strName = BT_cb_LFB.Text;
                        }
                        else if (BT_rb_FF.Checked)
                        {
                            BT_cb_LFF.SelectedIndex = i;
                            BT_cb_RFF.SelectedIndex = i;
                            if (double.Parse(BT_cb_LFF.Text) < minGain)
                            {
                                return;
                            }
                            strName = BT_cb_LFF.Text;

                        }
                        else if (BT_rb_Hybrid.Checked)
                        {
                            BT_cb_LFF.SelectedIndex = i;
                            BT_cb_RFF.SelectedIndex = i;
                            if (double.Parse(BT_cb_LFF.Text) < minGain)
                            {
                                return;
                            }
                            strName = BT_cb_LFF.Text;
                        }
                        Thread.Sleep(200);
                        BT_gb_Gain_bt_Set_Click(null, null);
                        Thread.Sleep(1300);

                        SoundCheck.SetSerialNumber(strName);
                        Thread.Sleep(100);
                        string Receive = SoundCheck.StartTest();
                        if (Receive.Contains("False"))
                            return;
                        Thread.Sleep(200);
                        SoundCheck.GetFinalResults();
                        Thread.Sleep(500);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    BT_tb_log.Text += $"{DateTime.Now.ToString("HH:mm:ss")} | SC | {ex}\r\n";
                    return;
                }
                finally
                {
                    EnableBtn();
                    BT_gb_Gain_bt_Get_Click(null, null);

                }
            });
        }

        private void BT_bt_FroRunSC3_Click(object sender, EventArgs e)
        {


            if (!SoundCheck.IsConnect)
            {
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                    if (thisProc.ProcessName.Contains("MerryTestFramework"))
                        thisProc.Kill();
                string Receive = SoundCheck.ConnectSoundCheck();
                BT_tb_log.Text += $"{DateTime.Now:HH:mm:ss} | SC | {Receive}\r\n";
                if (Receive.Contains("False"))
                    return;
            }
            if (RunSQC)
            {
                string Receive = SoundCheck.RunSequence(sqcPath);
                if (Receive.Contains("False"))
                    return;
                Thread.Sleep(3000);
                RunSQC = false;
            }
            DisableBtn();

            BT_bt_Stop2.Enabled = bt_SC_Stop.Enabled = true;
            STOPFlag = false;


            Task.Run(() =>
            {
                try
                {
                    BT_cb_L_SPK_Gain.SelectedIndex = 41;
                    BT_cb_R_SPK_Gain.SelectedIndex = 41;
                    ShowFlag = false;
                    BT_gc_bt_SetSPK_Click(null, null);
                    Thread.Sleep((1000));
                    double minGain = double.Parse(BT_cb_MinGain.Text);
                    int i = BT_cb_MaxGain.SelectedIndex;


                    for (; i < BT_cb_L_SPK_Gain.Items.Count; i++)
                    {
                        if (STOPFlag)
                            return;

                        BT_cb_L_SPK_Gain.SelectedIndex = i;
                        BT_cb_R_SPK_Gain.SelectedIndex = i;

                        Thread.Sleep(100);
                        BT_gc_bt_SetSPK_Click(null, null);
                        SoundCheck.SetSerialNumber(BT_cb_L_SPK_Gain.Items[i].ToString());
                        Thread.Sleep(50);
                        string Receive = SoundCheck.StartTest();
                        if (Receive.Contains("False"))
                            return;
                        Thread.Sleep(200);
                        SoundCheck.GetFinalResults();
                        Thread.Sleep(500);
                    }

                }
                catch (Exception ex)
                {
                    BT_tb_log.Text += $"{DateTime.Now.ToString("HH:mm:ss")} | SC | {ex}\r\n";
                    return;
                }
                finally
                {
                    EnableBtn();
                    BT_cb_L_SPK_Gain.SelectedIndex = 40;
                    BT_cb_R_SPK_Gain.SelectedIndex = 40;
                    BT_gc_bt_SetSPK_Click(null, null);
                    ShowFlag = true;

                }
            });
        }

        private void bt_GainToCMD_Click(object sender, EventArgs e)
        {
            tb_ToCMD.Text = double.Parse(tb_GainTo.Text).VolToHEXGain();

        }

        private void bt_CMDToGain_Click(object sender, EventArgs e)
        {

            tb_ToGain.Text = (tb_CMDTo.Text).HEXGainToGain().ToString();


        }
    }
}
