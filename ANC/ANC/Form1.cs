using ANCSettingTool.InterfaceInfo;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using SoundCheck_V1.API;
using SoundCheck_V1.TemplateANC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace ANC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        public static string AllDllPath = "";
        public static string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string ConfigPath = $@"{exePath}\ANC_Parameter.ini";

        public static Dictionary<string, object> dicConfig = new Dictionary<string, object>();
        Func<string> SwitchANCOff
        {
            get
            {
                return (Func<string>)dicConfig[$"{cbb_CMDList.Text}.SwitchANCOff"];
            }
        }
        Func<string> SwitchFBOnly
        {
            get
            {
                return (Func<string>)dicConfig[$"{cbb_CMDList.Text}.SwitchFBOnly"];
            }
        }
        Func<string> SwitchFFOnly
        {
            get
            {
                return (Func<string>)dicConfig[$"{cbb_CMDList.Text}.SwitchFFOnly"];
            }
        }
        Func<string> SwitchHybrid
        {
            get
            {
                return (Func<string>)dicConfig[$"{cbb_CMDList.Text}.SwitchHybrid"];
            }
        }
        Func<string> SwitchAiroThru
        {
            get
            {
                return (Func<string>)dicConfig[$"{cbb_CMDList.Text}.SwitchAiroThru"];
            }
        }

        Func<string> GetGain
        {
            get
            {
                return (Func<string>)dicConfig[$"{cbb_CMDList.Text}.GetGain"];
            }
        }
        Func<string, string> SetGain
        {
            get
            {
                return (Func<string, string>)dicConfig[$"{cbb_CMDList.Text}.SetGain"];
            }
        }
        Func<string, string> SaveGain
        {
            get
            {
                return (Func<string, string>)dicConfig[$"{cbb_CMDList.Text}.SaveGain"];
            }
        }

        #region 禁用 关闭按钮
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);//设置此窗体为活动窗体
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        #endregion
        bool loadFlag = false;
        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                TopMost = true;
                BringToFront();
                int x = System.Windows.Forms.SystemInformation.WorkingArea.Width - (int)((double)this.Size.Width / 1);
                int y = System.Windows.Forms.SystemInformation.WorkingArea.Height - (int)((double)this.Size.Height / 1);
                this.StartPosition = FormStartPosition.Manual; //窗体的位置由Location属性决定
                this.Location = (Point)new Size(x, y);         //窗体的起始位置为(x,y)
                tabControl1.SelectedIndex = 4;
                loadCMDList();
                LoadDicConfig();
                initControl();
                LoadConfig();
                await Task.Run(() => Thread.Sleep(200));

                SetForegroundWindow(Handle);
                SolidBrush active = new SolidBrush(Color.Yellow);
                if (cbb_CMDList.Text.Trim() == "")
                {
                    MessageBox.Show("程序没有选择指令集，无法启动正常调试功能,请在左边选择指令集");
                }
                loadFlag = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }

        }
        void initControl()
        {
            dg_FBAvgFreqs.AllowUserToAddRows = false;
            dg_FFAvgFreqs.AllowUserToAddRows = false;
            str_FBLTarget.AllowUserToAddRows = false;
            str_FBRTarget.AllowUserToAddRows = false;
            str_FFLTarget.AllowUserToAddRows = false;
            str_FFRTarget.AllowUserToAddRows = false;

            dg_FBAvgFreqs.Rows.Add(new object[12]);
            dg_FFAvgFreqs.Rows.Add(new object[12]);
            str_FBLTarget.Rows.Add(new object[12]);
            str_FBRTarget.Rows.Add(new object[12]);
            str_FFLTarget.Rows.Add(new object[12]);
            str_FFRTarget.Rows.Add(new object[12]);
            for (int i = 0; i < dg_FBAvgFreqs.ColumnCount; i++)
            {
                dg_FBAvgFreqs[i, 0].Value = "";
                dg_FBAvgFreqs[i, 0].Style.BackColor = System.Drawing.SystemColors.ScrollBar;
            }
            dg_FBAvgFreqs[0, 0].Value = "频率";
            for (int i = 0; i < dg_FFAvgFreqs.ColumnCount; i++)
            {
                dg_FFAvgFreqs[i, 0].Value = "";
                dg_FFAvgFreqs[i, 0].Style.BackColor = System.Drawing.SystemColors.ScrollBar;
            }
            dg_FFAvgFreqs[0, 0].Value = "频率";
 

            str_FBLTarget.Rows.Add(new object[12]);
            str_FBLTarget.Rows.Add(new object[12]);
            for (int i = 0; i < str_FBLTarget.ColumnCount; i++)
            {
                str_FBLTarget[i, 0].Value = "";
                str_FBLTarget[i, 1].Value = "";
                str_FBLTarget[i, 2].Value = "";
            }
            str_FBLTarget[0, 0].Value = "频率";
            str_FBLTarget[0, 1].Value = "感度";
            str_FBLTarget[0, 2].Value = "范围";


            str_FBRTarget.Rows.Add(new object[12]);
            str_FBRTarget.Rows.Add(new object[12]);
            for (int i = 0; i < str_FBRTarget.ColumnCount; i++)
            {
                str_FBRTarget[i, 0].Value = "";
                str_FBRTarget[i, 1].Value = "";
                str_FBRTarget[i, 2].Value = "";
            }
            str_FBRTarget[0, 0].Value = "频率";
            str_FBRTarget[0, 1].Value = "感度";
            str_FBRTarget[0, 2].Value = "范围";

            str_FFLTarget.Rows.Add(new object[12]);
            str_FFLTarget.Rows.Add(new object[12]);
            for (int i = 0; i < str_FFLTarget.ColumnCount; i++)
            {
                str_FFLTarget[i, 0].Value = "";
                str_FFLTarget[i, 1].Value = "";
                str_FFLTarget[i, 2].Value = "";
            }
            str_FFLTarget[0, 0].Value = "频率";
            str_FFLTarget[0, 1].Value = "感度";
            str_FFLTarget[0, 2].Value = "范围";



            str_FFRTarget.Rows.Add(new object[12]);
            str_FFRTarget.Rows.Add(new object[12]);
            for (int i = 0; i < str_FFRTarget.ColumnCount; i++)
            {
                str_FFRTarget[i, 0].Value = "";
                str_FFRTarget[i, 1].Value = "";
                str_FFRTarget[i, 2].Value = "";
            }
            str_FFRTarget[0, 0].Value = "频率";
            str_FFRTarget[0, 1].Value = "感度";
            str_FFRTarget[0, 2].Value = "范围";
        }
        void LoadConfig()
        {
            INIClass._path = ConfigPath;
            Dictionary<string, string> Section_rb = INIClass.GetSection("RadioButton");
            Dictionary<string, string> Section_nb = INIClass.GetSection("NumericUpDown");
            Dictionary<string, string> Section_cb = INIClass.GetSection("CheckBox");
            Dictionary<string, string> Section_dg = INIClass.GetSection("DataGridView");
            Dictionary<string, string> Section_Str = INIClass.GetSection("String");
            Dictionary<string, string> Section_ccb = INIClass.GetSection("ComboBox");


            foreach (FieldInfo item in this.GetType().GetFields())
            {
                if (Section_rb.ContainsValue(item.Name))
                {
                    RadioButton rb = (RadioButton)item.GetValue(this);
                    rb.Checked = true;
                    continue;
                }
                if (Section_nb.ContainsKey(item.Name))
                {
                    NumericUpDown rb = (NumericUpDown)item.GetValue(this);
                    double.TryParse(Section_nb[item.Name], out var val);
                    rb.Value = (decimal)val;
                    continue;

                }
                if (Section_cb.ContainsKey(item.Name))
                {
                    CheckBox rb = (CheckBox)item.GetValue(this);
                    bool.TryParse(Section_cb[item.Name], out var val);
                    rb.Checked = val;
                    continue;

                }
                if (Section_dg.ContainsKey(item.Name))
                {
                    DataGridView dg = (DataGridView)item.GetValue(this);

                    string[] row = Section_dg[item.Name].Split(',');
                    int i = 1;
                    foreach (var ro in row)
                    {
                        if (i < dg.ColumnCount)
                        {
                            dg[i, 0].Value = ro.ToString();
                            i++;
                        }

                    }
                    continue;
                }
                if (Section_Str.ContainsKey(item.Name))
                {
                    DataGridView dg = (DataGridView)item.GetValue(this);
                    Dictionary<string, double[]> dic = Section_Str[item.Name].TargetStrDic();

                    for (int i = 0; i < dic.Count; i++)
                    {
                        dg[i + 1, 0].Value = dic.ElementAt(i).Key;
                        dg[i + 1, 1].Value = dic.ElementAt(i).Value[0];
                        if (dic.ElementAt(i).Value[1] == 100)
                            dg[i + 1, 2].Value = "";
                        else
                            dg[i + 1, 2].Value = dic.ElementAt(i).Value[1];

                    }
                    continue;

                }
                if (Section_ccb.ContainsKey(item.Name))
                {
                    ComboBox cbb = (ComboBox)item.GetValue(this);
                    for (int i = 0; i < cbb.Items.Count; i++)
                        if (cbb.Items[i].ToString() == Section_ccb[item.Name])
                            cbb.SelectedIndex = i;
                    continue;
                }
                bool.TryParse(INIClass.GetValue("Button", "btn_TopMost"), out bool _TopMost);
                if (_TopMost)
                {
                    this.TopMost = true;
                    btn_TopMost.Text = "取消置顶";
                    btn_TopMost.BackColor = Color.SkyBlue;

                }
                else
                {
                    this.TopMost = false;
                    btn_TopMost.Text = "选择置顶";
                    btn_TopMost.BackColor = Color.Transparent;

                }

            }


        }
        void LoadDicConfig()
        {
            string _Path = $@"{exePath}\ParameterInfo.txt";
            if (!File.Exists(_Path))
                return;
            string[] dicstrs = File.ReadAllLines(_Path);

            foreach (var item in dicstrs)
            {
                string[] strs = item.Split('=');
                if (strs.Length < 3)
                {
                    continue;
                }
                if (strs[2] == "System.String")
                {
                    dicConfig[strs[0]] = (strs[1]);
                }
                else if (strs[2] == "System.Boolean")
                {
                    dicConfig[strs[0]] = bool.Parse(strs[1]);

                }
                else if (strs[2] == "System.Int32")
                {
                    dicConfig[strs[0]] = int.Parse(strs[1]);
                }
                else if (strs[2] == "System.Double")
                {
                    dicConfig[strs[0]] = double.Parse(strs[1]);

                }
                else
                {
                    MessageBox.Show(item);
                }





            }

        }

        void loadCMDList()
        {
            cbb_CMDList.Items.Clear();
            string dllPath = "";
            string xmlPath = "";
            if (!Directory.Exists(AllDllPath))
                return;
            string[] _paths = Directory.GetDirectories(AllDllPath);
            foreach (var _path in _paths)
            {
                string DirecName = Path.GetFileName(_path);
                bool is_CMD_List = false;
                string CMDListName = "";

                dllPath = $@"{_path}\{DirecName}.dll";
                xmlPath = $@"{_path}\{DirecName}.xml";
                if (!File.Exists(xmlPath) || !File.Exists(dllPath))
                    continue;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);
                XmlNodeList nodes = xmlDoc.DocumentElement.SelectNodes("/doc/members/member");

                foreach (XmlNode node in nodes)
                {
                    string[] Namespaces = node.Attributes["name"].Value.Split(':');
                    Console.WriteLine(node.Attributes["name"].Value);
                    if (Namespaces[0] != "T") continue;
                    XmlNode SumaryCode = node.SelectSingleNode("summary");
                    if (SumaryCode.OuterXml.Contains(" dllName=") && SumaryCode.OuterXml.Contains(" ANC_CMD_List="))
                    {
                        CMDListName = SumaryCode.Attributes["ANC_CMD_List"].Value;
                        string dllName = SumaryCode.Attributes["dllName"].Value;
                        dic[CMDListName] = new _Parameter_();
                        dic[CMDListName]._CMD_ListName = CMDListName;
                        dic[CMDListName]._Type_Namespace = "MerryDllFramework.MerryDll";
                        dic[CMDListName]._Type_Path = dllPath;
                        dic[CMDListName]._TypeName = dllName;
                        is_CMD_List = true;
                    }
                }
                if (!is_CMD_List)
                    continue;
                foreach (XmlNode node in nodes)
                {
                    string[] Namespaces = node.Attributes["name"].Value.Split(':');
                    Console.WriteLine(node.Attributes["name"].Value);
                    if (Namespaces[0] != "T") continue;
                    XmlNode SumaryCode = node.SelectSingleNode("summary");


                    if (SumaryCode.OuterXml.Contains(" ClassType="))
                    {

                        if (SumaryCode.Attributes["ClassType"].Value == "UserControl")
                        {
                            string ClassType = SumaryCode.Attributes["ClassType"].Value;
                            dic[CMDListName]._UserControlNamespace = Namespaces[1];
                            dic[CMDListName]._UserControClassType = ClassType;
                            dic[CMDListName]._XmlPath = xmlPath;
                        }
                    }
                }
            }
            string TypeDirectory = $"{Path.GetDirectoryName(ConfigPath)}";
            string TypeName = $"{Path.GetDirectoryName(ConfigPath)}";
            dllPath = $@"{TypeDirectory}\{TypeName}.dll";
            xmlPath = $@"{TypeDirectory}\{TypeName}.xml";
            if (File.Exists(xmlPath) && File.Exists(dllPath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);
                XmlNodeList nodes = xmlDoc.DocumentElement.SelectNodes("/doc/members/member");
                string CMDListName = "";
                bool is_CMD_List = false;
                foreach (XmlNode node in nodes)
                {
                    string[] Namespaces = node.Attributes["name"].Value.Split(':');
                    Console.WriteLine(node.Attributes["name"].Value);
                    if (Namespaces[0] != "T") continue;
                    XmlNode SumaryCode = node.SelectSingleNode("summary");
                    if (SumaryCode.OuterXml.Contains(" dllName=") && SumaryCode.OuterXml.Contains(" ANC_CMD_List="))
                    {
                        CMDListName = SumaryCode.Attributes["ANC_CMD_List"].Value;
                        string dllName = SumaryCode.Attributes["dllName"].Value;
                        dic[CMDListName] = new _Parameter_();
                        dic[CMDListName]._CMD_ListName = CMDListName;
                        dic[CMDListName]._Type_Namespace = "MerryDllFramework.MerryDll";
                        dic[CMDListName]._Type_Path = dllPath;
                        dic[CMDListName]._TypeName = dllName;
                        is_CMD_List = true;
                    }
                }
                if (is_CMD_List)
                {
                    foreach (XmlNode node in nodes)
                    {
                        string[] Namespaces = node.Attributes["name"].Value.Split(':');
                        Console.WriteLine(node.Attributes["name"].Value);
                        if (Namespaces[0] != "T") continue;
                        XmlNode SumaryCode = node.SelectSingleNode("summary");
                        if (SumaryCode.OuterXml.Contains(" ClassType="))
                        {
                            if (SumaryCode.Attributes["ClassType"].Value == "UserControl")
                            {
                                string ClassType = SumaryCode.Attributes["ClassType"].Value;
                                dic[CMDListName]._UserControlNamespace = Namespaces[1];
                                dic[CMDListName]._UserControClassType = ClassType;
                                dic[CMDListName]._XmlPath = xmlPath;
                            }
                        }
                    }

                }




            }



            foreach (var item in dic)
                cbb_CMDList.Items.Add(item.Key);

        }

        private void bt_Cancel_Click(object sender, EventArgs e)
           => this.Close();

        private void bt_Enter_Click(object sender, EventArgs e)
        {
            INIClass._path = ConfigPath;
            Savegb();
            foreach (FieldInfo item in this.GetType().GetFields())
            {
                if (item.FieldType == typeof(NumericUpDown))
                {
                    NumericUpDown rb = (NumericUpDown)item.GetValue(this);
                    INIClass.SetValue("NumericUpDown", rb.Name, rb.Value.ToString());
                    continue;

                }
                if (item.FieldType == typeof(CheckBox))
                {
                    CheckBox rb = (CheckBox)item.GetValue(this);
                    INIClass.SetValue("CheckBox", rb.Name, rb.Checked.ToString());
                    continue;

                }
            }
            string FBColStr = "";
            for (int i = 1; i < dg_FBAvgFreqs.ColumnCount; i++)
            {
                FBColStr += $"{dg_FBAvgFreqs[i, 0].Value},";
            }
            INIClass.SetValue("DataGridView", "dg_FBAvgFreqs", FBColStr);
            string FFColStr = "";
            for (int i = 1; i < dg_FFAvgFreqs.ColumnCount; i++)
            {
                FFColStr += $"{dg_FFAvgFreqs[i, 0].Value},";
            }
            INIClass.SetValue("DataGridView", "dg_FFAvgFreqs", FFColStr);
 
            StringBuilder FBLstring = new StringBuilder();
            for (int i = 1; i < str_FBLTarget.ColumnCount; i++)
            {
                string strFreq = str_FBLTarget[i, 0].Value.ToString();
                string strSends = str_FBLTarget[i, 1].Value.ToString();
                string strReging = str_FBLTarget[i, 2].Value.ToString();
                if (double.TryParse(strFreq, out double freq))
                {
                    if (!double.TryParse(strSends, out double Value))
                    {
                        MessageBox.Show($"FBL 设置频率“{strFreq}” 同时需要设定感度", "ANC 参数设定提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (strReging.Trim().Length > 0 && double.TryParse(strReging, out double Reg))
                        FBLstring.Append($@"{{{freq}:{Value}:{Reg}}}");
                    else
                        FBLstring.Append($@"{{{freq}:{Value}}}");

                }
            }
            StringBuilder FBRstring = new StringBuilder();
            for (int i = 1; i < str_FBRTarget.ColumnCount; i++)
            {
                string strFreq = str_FBRTarget[i, 0].Value.ToString();
                string strSends = str_FBRTarget[i, 1].Value.ToString();
                string strReging = str_FBRTarget[i, 2].Value.ToString();
                if (double.TryParse(strFreq, out double freq))
                {


                    if (!double.TryParse(strSends, out double Value))
                    {
                        MessageBox.Show($"FBR 设置频率“{strFreq}” 同时需要设定感度", "ANC 参数设定提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (strReging.Trim().Length > 0 && double.TryParse(strReging, out double Reg))
                        FBRstring.Append($@"{{{freq}:{Value}:{Reg}}}");
                    else
                        FBRstring.Append($@"{{{freq}:{Value}}}");

                }
            }

            StringBuilder FFLstring = new StringBuilder();
            for (int i = 1; i < str_FFLTarget.ColumnCount; i++)
            {
                string strFreq = str_FFLTarget[i, 0].Value.ToString();
                string strSends = str_FFLTarget[i, 1].Value.ToString();
                string strReging = str_FFLTarget[i, 2].Value.ToString();
                if (double.TryParse(strFreq, out double freq))
                {


                    if (!double.TryParse(strSends, out double Value))
                    {
                        MessageBox.Show($"FFL 设置频率“{strFreq}” 同时需要设定感度", "ANC 参数设定提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (strReging.Trim().Length > 0 && double.TryParse(strReging, out double Reg))
                        FFLstring.Append($@"{{{freq}:{Value}:{Reg}}}");
                    else
                        FFLstring.Append($@"{{{freq}:{Value}}}");

                }
            }
            StringBuilder FFRstring = new StringBuilder();
            for (int i = 1; i < str_FFRTarget.ColumnCount; i++)
            {
                string strFreq = str_FFRTarget[i, 0].Value.ToString();
                string strSends = str_FFRTarget[i, 1].Value.ToString();
                string strReging = str_FFRTarget[i, 2].Value.ToString();
                if (double.TryParse(strFreq, out double freq))
                {


                    if (!double.TryParse(strSends, out double Value))
                    {
                        MessageBox.Show($"FFR 设置频率“{strFreq}” 同时需要设定感度", "ANC 参数设定提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (strReging.Trim().Length > 0 && double.TryParse(strReging, out double Reg))
                        FFRstring.Append($@"{{{freq}:{Value}:{Reg}}}");
                    else
                        FFRstring.Append($@"{{{freq}:{Value}}}");

                }
            }
            INIClass.SetValue("ComboBox", "cbb_CMDList", cbb_CMDList.Text.ToString());
            INIClass.SetValue("String", "str_FBLTarget", FBLstring.ToString());
            INIClass.SetValue("String", "str_FBRTarget", FBRstring.ToString());
            INIClass.SetValue("String", "str_FFLTarget", FFLstring.ToString());
            INIClass.SetValue("String", "str_FFRTarget", FFRstring.ToString());
            INIClass.SetValue("Button", "btn_TopMost", this.TopMost.ToString());



            MessageBox.Show("保存成功 设定已生效 无需重启", "ANC 参数设定提示", MessageBoxButtons.OK, MessageBoxIcon.None);

        }
        void Savegb()
        {
            foreach (Control gbControls in gb_CalibrationDetails.Controls)
                if (gbControls.GetType() == typeof(RadioButton))
                    if (((RadioButton)gbControls).Checked)
                        INIClass.SetValue("RadioButton", "gb_CalibrationDetails", ((RadioButton)gbControls).Name);
            foreach (Control gbControls in gb_CalibrationMethod.Controls)
                if (gbControls.GetType() == typeof(RadioButton))
                    if (((RadioButton)gbControls).Checked)
                        INIClass.SetValue("RadioButton", "gb_CalibrationMethod", ((RadioButton)gbControls).Name);
            foreach (Control gbControls in gb_FinalExecution.Controls)
                if (gbControls.GetType() == typeof(RadioButton))
                    if (((RadioButton)gbControls).Checked)
                        INIClass.SetValue("RadioButton", "gb_FinalExecution", ((RadioButton)gbControls).Name);
            foreach (Control gbControls in gb_InitGainSeting.Controls)
                if (gbControls.GetType() == typeof(RadioButton))
                    if (((RadioButton)gbControls).Checked)
                        INIClass.SetValue("RadioButton", "gb_InitGainSeting", ((RadioButton)gbControls).Name);
            foreach (Control gbControls in gb_ResultType.Controls)
                if (gbControls.GetType() == typeof(RadioButton))
                    if (((RadioButton)gbControls).Checked)
                        INIClass.SetValue("RadioButton", "gb_ResultType", ((RadioButton)gbControls).Name);
            foreach (Control gbControls in gb_FBMinType.Controls)
                if (gbControls.GetType() == typeof(RadioButton))
                    if (((RadioButton)gbControls).Checked)
                        INIClass.SetValue("RadioButton", "gb_FBMinType", ((RadioButton)gbControls).Name);
            foreach (Control gbControls in gb_FBAvgType.Controls)
                if (gbControls.GetType() == typeof(RadioButton))
                    if (((RadioButton)gbControls).Checked)
                        INIClass.SetValue("RadioButton", "gb_FBAvgType", ((RadioButton)gbControls).Name);
            foreach (Control gbControls in gb_FFMinType.Controls)
                if (gbControls.GetType() == typeof(RadioButton))
                    if (((RadioButton)gbControls).Checked)
                        INIClass.SetValue("RadioButton", "gb_FFMinType", ((RadioButton)gbControls).Name);
            foreach (Control gbControls in gb_FFAvgType.Controls)
                if (gbControls.GetType() == typeof(RadioButton))
                    if (((RadioButton)gbControls).Checked)
                        INIClass.SetValue("RadioButton", "gb_FFAvgType", ((RadioButton)gbControls).Name);
 
            foreach (Control gbControls in gb_CalibrationFeature.Controls)
                if (gbControls.GetType() == typeof(RadioButton))
                    if (((RadioButton)gbControls).Checked)
                        INIClass.SetValue("RadioButton", "gb_CalibrationFeature", ((RadioButton)gbControls).Name);

        }

        private void rb_FBMinRegion_CheckedChanged(object sender, EventArgs e)
        {
            nb_FBMinSratrFreq.Enabled = true;
            nb_FBMinENDFreq.Enabled = true;
            nb_FBMinSingleFreq.Enabled = false;

        }

        private void rb_FBMinSingle_CheckedChanged(object sender, EventArgs e)
        {
            nb_FBMinSratrFreq.Enabled = false;
            nb_FBMinENDFreq.Enabled = false;
            nb_FBMinSingleFreq.Enabled = true;
        }

        private void rb_FBAvgRegion_CheckedChanged(object sender, EventArgs e)
        {
            nb_FBAvgSratrFreq.Enabled = true;
            nb_FBAvgENDFreq.Enabled = true;
            dg_FBAvgFreqs.Enabled = false;
            for (int i = 0; i < dg_FBAvgFreqs.ColumnCount; i++)
            {
                dg_FBAvgFreqs[i, 0].Style.BackColor = System.Drawing.SystemColors.ScrollBar;
            }

        }

        private void rb_FBAvgSingle_CheckedChanged(object sender, EventArgs e)
        {
            nb_FBAvgSratrFreq.Enabled = false;
            nb_FBAvgENDFreq.Enabled = false;
            dg_FBAvgFreqs.Enabled = true;
            for (int i = 0; i < dg_FBAvgFreqs.ColumnCount; i++)
            {
                dg_FBAvgFreqs[i, 0].Style.BackColor = System.Drawing.SystemColors.Window;
            }

        }
        private void rb_FFMinRegion_CheckedChanged(object sender, EventArgs e)
        {
            nb_FFMinSratrFreq.Enabled = true;
            nb_FFMinENDFreq.Enabled = true;
            nb_FFMinSingleFreq.Enabled = false;
        }

        private void rb_FFMinSingle_CheckedChanged(object sender, EventArgs e)
        {
            nb_FFMinSratrFreq.Enabled = false;
            nb_FFMinENDFreq.Enabled = false;
            nb_FFMinSingleFreq.Enabled = true;
        }

        private void rb_FFAvgRegion_CheckedChanged(object sender, EventArgs e)
        {
            nb_FFAvgSratrFreq.Enabled = true;
            nb_FFAvgENDFreq.Enabled = true;
            dg_FFAvgFreqs.Enabled = false;
            for (int i = 0; i < dg_FFAvgFreqs.ColumnCount; i++)
            {
                dg_FFAvgFreqs[i, 0].Style.BackColor = System.Drawing.SystemColors.ScrollBar;
            }
        }

        private void rb_FFAvgSingle_CheckedChanged(object sender, EventArgs e)
        {
            nb_FFAvgSratrFreq.Enabled = false;
            nb_FFAvgENDFreq.Enabled = false;
            dg_FFAvgFreqs.Enabled = true;
            for (int i = 0; i < dg_FFAvgFreqs.ColumnCount; i++)
            {
                dg_FFAvgFreqs[i, 0].Style.BackColor = System.Drawing.SystemColors.Window;
            }
        }



        private void nb_TestMinCount_ValueChanged(object sender, EventArgs e)
        {
            if (nb_TestMinCount.Value >= nb_TestMaxCount.Value)
            {
                nb_TestMinCount.Value = nb_TestMaxCount.Value;
            }
        }
        bool maxcountFlag = false;
        bool LoadPlug_in = true;
        private void nb_TestMaxCount_ValueChanged(object sender, EventArgs e)
        {
            if (nb_TestMaxCount.Value < nb_TestMinCount.Value)
            {
                nb_TestMaxCount.Value = nb_TestMinCount.Value;
            }
            if (loadFlag && !maxcountFlag)
            {
                maxcountFlag = true;
                MessageBox.Show("最大的尝试次数 设定 需要跟Sound Check 的设定一致  是步骤“Cycle Index_ANC”", "ANC 参数设定提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
        Dictionary<string, _Parameter_> dic = new Dictionary<string, _Parameter_>();
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loadFlag)
                return;
            if (rb_LuxshareFeatrue.Checked && ((TabControl)sender).SelectedTab.Name == "tb_MerryFeatrueSetting")
            {
                MessageBox.Show("校准条件特色选的是“校准逻辑 2”\r\n可以在“校准逻辑 2”选项卡设置参数", "ANC 参数设定提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

            }

            if (rb_MerryFeature.Checked && ((TabControl)sender).SelectedTab.Name == "tb_LuxshareFeatrueSetting")
            {
                MessageBox.Show("校准条件特色选的是“校准逻辑 1”\r\n可以在“校准逻辑 1”选项卡设置参数", "ANC 参数设定提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

            }

        }

        private void nb_FBMinUplimit_ValueChanged(object sender, EventArgs e)
        {

        }
        private void cbb_CMDList_SelectedIndexChanged(object sender, EventArgs e)
        {
            gb_Plug_in.Controls.Clear();
            if (cbb_CMDList.SelectedIndex < 0)
                return;
            InvorkDll.Config = dicConfig;
            string SelectName = cbb_CMDList.Items[cbb_CMDList.SelectedIndex].ToString();
            InvorkDll.LoadDll(dic[SelectName]._TypeName, dic[SelectName]._Type_Path, dic[SelectName]._Type_Namespace);
            InvorkDll.LoadDll(dic[SelectName]._TypeName, dic[SelectName]._Type_Path, dic[SelectName]._UserControlNamespace);
            Control obj = (Control)InvorkDll.DllObject_Namespace[dic[SelectName]._UserControlNamespace];
            gb_Plug_in.Controls.Add(obj);

        }

        private void btn_TopMost_Click(object sender, EventArgs e)
        {
            if (this.TopMost)
            {
                this.TopMost = false;
                btn_TopMost.Text = "选择置顶";
                btn_TopMost.BackColor = Color.Transparent;

            }
            else
            {
                this.TopMost = true;
                btn_TopMost.Text = "取消置顶";
                btn_TopMost.BackColor = Color.SkyBlue;

            }
        }


        private void bt_Calculate_Data_Click(object sender, EventArgs e)
        {
            Button bt = (Button)sender;
            Curves(bt.Tag.ToString());
            bt.BackColor = Color.Lime;
        }


        OpenFileDialog openFileDialog2 = new OpenFileDialog
        {
            //设置打开的文件的类型，注意过滤器的语法
            Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
            //设置这个对话框的起始位置
            InitialDirectory = Path.GetDirectoryName($@"{System.Windows.Forms.Application.ExecutablePath}")
        };

        void Curves(string supplementFileName)
        {
            #region MyRegion

            string FilePath = "";

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                //获取用户选择的文件完整路径，并存储到一个字符串数组中
                string[] paths = openFileDialog2.FileNames;
                foreach (string path in paths)
                {
                    openFileDialog2.InitialDirectory = Path.GetDirectoryName(path);
                    //获取文件路径
                    FilePath = path;
                    break;

                }

            }

            if (!File.Exists(FilePath))
                return;

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

            //  StringBuilder ConsoleStrL = new StringBuilder();
            StringBuilder txtData = new StringBuilder();



            //            ConsoleStrL.Append(@"Dictionary<double, Dictionary<double, double[]>> " + supplementFileName + @"GianCardinal = new Dictionary<double, Dictionary<double, double[]>>()
            //{
            //");
            foreach (var item in LearTrimDataL)
            {

                //ConsoleStrL.Append("    { " + item.Key + ",new Dictionary<double,double[]>(){ \r\n");
                foreach (var data in item.Value)
                {
                    txtData.Append($"{item.Key}&{data.Key},");

                    //string doubleStr = "";
                    for (int i = 0; i < data.Value.Length; i++)
                    {
                        //doubleStr += $"{data.Value[i]},";
                        txtData.Append($"{data.Value[i]},");
                    }
                    //ConsoleStrL.Append(@"        { " + data.Key + ", new double[] { " + doubleStr + "} },\r\n");
                    txtData.Append("\r\n");

                }
                txtData.Append("\r\n");

                //ConsoleStrL.Append(" }},\r\n");

            }
            //ConsoleStrL.Append(@"};");
            Directory.CreateDirectory($@"{exePath}\ANC_Cardinal");
            //File.WriteAllText($@".\{TypeName}\{supplementFileName}_Dictionary.txt", ConsoleStrL.ToString());

            string TypeNameDirectory = Path.GetDirectoryName(ConfigPath);
            Directory.CreateDirectory($@"{TypeNameDirectory}\ANC_Cardinal");
            File.WriteAllText($@"{TypeNameDirectory}\ANC_Cardinal\{supplementFileName}_data.ini", txtData.ToString());

            MessageBox.Show("OK");
            #endregion
        }

        private void bt_SC_StartRun_Click(object sender, EventArgs e)
        {
            if (!SoundCheck.IsConnect)
            {
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                    if (thisProc.ProcessName.Contains("MerryTestFramework"))
                        thisProc.Kill();
                string Receive = SoundCheck.ConnectSoundCheck();
                if (Receive.Contains("False"))
                {
                    Receive.ShowMSG();
                    return;

                }
            }
            Task.Run(() =>
            {
                try
                {
                    string sn = $"{nb_LFBSetGain.Text}_{nb_RFBSetGain.Text}_{nb_LFFSetGain.Text}_{nb_RFFSetGain.Text}";
                    sn = SoundCheck.SetSerialNumber(sn);
                    if (sn.ContainsFalse())
                        sn.ShowMSG();
                    Thread.Sleep(50);
                    sn = SoundCheck.StartTest();
                    if (sn.ContainsFalse())
                        sn.ShowMSG();
                    Thread.Sleep(200);
                    SoundCheck.GetFinalResults();

                }
                catch (Exception)
                {
                    throw;
                }
            });

        }
        bool STOPFlag = false;
        private void bt_SC_ForeachRun_Click(object sender, EventArgs e)
        {
            if (!SoundCheck.IsConnect)
            {
                foreach (System.Diagnostics.Process thisProc in System.Diagnostics.Process.GetProcesses())
                    if (thisProc.ProcessName.Contains("MerryTestFramework"))
                        thisProc.Kill();
                string Receive = SoundCheck.ConnectSoundCheck();
                if (Receive.Contains("False"))
                {
                    Receive.ShowMSG();
                    return;

                }
            }

            if (nb_MaxGain.Value < nb_MinGain.Value)
            {
                MessageBox.Show($"遍历的Gain值设定错误，目前最大值<最小值，无法遍历");
                return;
            }
            bt_SC_StartRun.Enabled = false;
            bt_SC_ForeachRun.Enabled = false;
            Task.Run(() =>
            {

                try
                {
                    void setGain()
                    {
                        Thread.Sleep(100);
                        bt_SetGain_Click(null, null);
                        Thread.Sleep(50);
                    }
                    if (rb_FB.Checked)
                    {
                        nb_LFBSetGain.Value = 0;
                        nb_RFBSetGain.Value = 0;
                        bt_SwitchFB_Click(null, null);

                    }
                    else if (rb_FF.Checked)
                    {
                        nb_LFFSetGain.Value = 0;
                        nb_RFFSetGain.Value = 0;
                        bt_SwitchFF_Click(null, null);

                    }
                    else
                    {
                        nb_LFFSetGain.Value = 0;
                        nb_RFFSetGain.Value = 0;
                        bt_SwitchHybrid_Click(null, null);
                    }
                    setGain();
                    decimal Gain = nb_MaxGain.Value + nb_Span.Value;
                    while (true)
                    {
                        if (STOPFlag)
                            return;
                        Gain -= nb_Span.Value;
                        if (rb_FB.Checked)
                            nb_RFBSetGain.Value = nb_LFBSetGain.Value = Gain;
                        else
                            nb_RFFSetGain.Value = nb_LFFSetGain.Value = Gain;

                        setGain();
                        SoundCheck.SetSerialNumber(Gain.ToString());
                        Thread.Sleep(50);
                        string Receive = SoundCheck.StartTest();
                        if (Receive.Contains("False"))
                        {
                            MessageBox.Show($"未知原因导致无法启动SoundCheck  {Receive}");
                            return;
                        }
                        Thread.Sleep(200);
                        SoundCheck.GetFinalResults();
                        Thread.Sleep(500);
                        if (nb_MinGain.Value >= Gain)
                            return;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return;
                }
                finally
                {
                    bt_GetGain_Click(null, null);
                    bt_SC_StartRun.Enabled = true;
                    bt_SC_ForeachRun.Enabled = true;
                    STOPFlag = false;

                }
            });



        }



        private void bt_OpenDirectory_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", exePath);
        }

        private void bt_GetGain_Click(object sender, EventArgs e)
        {
            //读取Gain值

            string str = GetGain().ShowCMDMSG(true, "ANC.GetGain");
            JToken Resul = str.StringToJson();
            if (!Resul.Value<bool>("cmdCompleted"))
            {
                $"ANC.GetGain".AddFalse();
                return;
            }
            nb_LFBSetGain.Value = (decimal)Resul.Value<double>("FB_Gain_L").Round();
            nb_RFBSetGain.Value = (decimal)Resul.Value<double>("FB_Gain_R").Round();
            nb_LFFSetGain.Value = (decimal)Resul.Value<double>("FF_Gain_L").Round();
            nb_RFFSetGain.Value = (decimal)Resul.Value<double>("FF_Gain_R").Round();
        }



        private void bt_SetGain_Click(object sender, EventArgs e)
        {
            SetGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",nb_LFBSetGain.Value},
                                            {"FB_Gain_R",nb_RFBSetGain.Value },
                                            {"FF_Gain_L",nb_LFFSetGain.Value },
                                            {"FF_Gain_R",nb_RFFSetGain.Value },
                                            {"MSG","Calibration_Init" },
                                            {"cmdCompleted","" },

                                        }.ObjectToJson()).ShowCMDMSG(true, "ANC.SetGain");

        }

        private void bt_SaveGain_Click(object sender, EventArgs e)
        {
            SaveGain(new Dictionary<string, object>()
                                        {
                                            {"FB_Gain_L",nb_LFBSetGain.Value},
                                            {"FB_Gain_R",nb_RFBSetGain.Value },
                                            {"FF_Gain_L",nb_LFFSetGain.Value },
                                            {"FF_Gain_R",nb_RFFSetGain.Value },
                                            {"MSG","Calibration_Save" },
                                            {"cmdCompleted","" },

                                        }.ObjectToJson()).ShowCMDMSG(true, "ANC.SaveGain");

        }

        private void bt_SwitchANCOFF_Click(object sender, EventArgs e)
        {
            SwitchANCOff().ShowCMDMSG(true, "ANC.SwitchANCOff");
        }

        private void bt_SwitchFB_Click(object sender, EventArgs e)
        {
            SwitchFBOnly().ShowCMDMSG(true, "ANC.SwitchFBOnly");
        }

        private void bt_SwitchFF_Click(object sender, EventArgs e)
        {
            SwitchFFOnly().ShowCMDMSG(true, "ANC.SwitchFFOnly");

        }
        private void bt_SwitchHybrid_Click(object sender, EventArgs e)
        {
            SwitchHybrid().ShowCMDMSG(true, "ANC.SwitchHybrid");
        }
        private void bt_SwitchAiroThru_Click(object sender, EventArgs e)
        {
            SwitchAiroThru().ShowCMDMSG(true, "ANC.SwitchAiroThru");

        }
        private void bt_SC_Stop_Click(object sender, EventArgs e)
        {
            STOPFlag = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void cb_EnterBalance_CheckedChanged(object sender, EventArgs e)
        {
            tabControl2.Enabled = !cb_EnterBalance.Checked;
        }
    }
    public static class Ext
    {
        public static Dictionary<string, double[]> TargetStrDic(this string str)
        {

            Dictionary<string, double[]> TargetS = new Dictionary<string, double[]>();
            foreach (var item in str.Replace("}", "").Replace(" ", "").Split('{'))
            {
                if (item.Trim() == "") continue;
                List<double> list = new List<double>();
                string[] strs = item.Split(':');
                if (strs.Length >= 3)
                {
                    list.Add(double.Parse(strs[1]));
                    list.Add(double.Parse(strs[2]));
                }
                else
                {
                    list.Add(double.Parse(strs[1]));
                    list.Add(100);
                }

                TargetS[strs[0]] = list.ToArray();
            }
            return TargetS;
        }
        public static string ShowMSG(this string msg)
        {
            MessageBox.Show(msg);
            return msg;
        }
    }
}
