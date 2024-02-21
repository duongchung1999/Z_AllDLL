using Newtonsoft.Json.Linq;
using SoundCheck_V1.API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();



        }
        int i = 0;
        string ShowResult
        {
            set
            {
                textBox2.Text += $"{i++} ： {value}\r\n";
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            tb_CMD.Text = listBox1.SelectedValue.ToString();
        }

        private void Connect_Click(object sender, EventArgs e)
        {
            ShowResult = ((Button)sender).Text + "||  " + SoundCheck.ConnectSoundCheck().ToString();

        }

        private void RunSqc_Click(object sender, EventArgs e)
        {
            ShowResult = ((Button)sender).Text + "||  " + SoundCheck.RunSequence(@"C:\tmp\Adjust_ANC OFF-20221102.sqc").ToString();

        }

        private void RunSqc2_Click(object sender, EventArgs e)
        {
            ShowResult = ((Button)sender).Text + "||  " + SoundCheck.RunSequence($@"C:\tmp\Adjust_ANC ON-20221102.sqc").ToString();

        }

        private void StartTest_Click(object sender, EventArgs e)
        {
            ShowResult = ((Button)sender).Text + "||  " + SoundCheck.SetSerialNumber("123456").ToString();
            ShowResult = ((Button)sender).Text + "||  " + SoundCheck.StartTest().ToString();

        }

        private void GetResult_Click(object sender, EventArgs e)
        {
            ShowResult = ((Button)sender).Text + "||  " + SoundCheck.GetFinalResults().ToString();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            MerryDllFramework.MerryDll dll = new MerryDllFramework.MerryDll();
            dll.Interface(new Dictionary<string, object>() {
                { "adminPath", $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}" },
                { "Name", $"IHT915" },//SqcPath
                { "SqcPath", $@"D:\Data\repos\MerryTest\Z_AllDll\SoundCheck_V1\WindowsFormsApp1\bin\Debug\aaa.sqc" },//


            });
            dll.ANC_Calibration();

        }

        private void Close_Click(object sender, EventArgs e)
        {
            SoundCheck.Disconnect();
            ShowResult = ((Button)sender).Text + "||  ";

        }

        private void GetStepResult_Click(object sender, EventArgs e)
        {
            ShowResult = ((Button)sender).Text + "||  " + SoundCheck.GetStepResult(tb_CMD.Text).ToString();


        }

        private void button2_Click(object sender, EventArgs e)
        {
            dynamic json = "";// SoundCheck.GetCurve(tb_CMD.Text);
            // Command completed successfully
            if (json.returnData.Value<Boolean>("Found"))
            {
                //textBoxCurveInfo.Text = "Name: " + json.returnData.Curve.Value<String>("Name") + Environment.NewLine +
                //                        "X Unit: " + json.returnData.Curve.Value<String>("XUnit") + Environment.NewLine +
                //                        "Y Unit: " + json.returnData.Curve.Value<String>("YUnit") + Environment.NewLine +
                //                        "Z Unit: " + json.returnData.Curve.Value<String>("ZUnit") + Environment.NewLine +
                //                        "X Scale: " + json.returnData.Curve.Value<String>("XDataScale") + Environment.NewLine +
                //                        "Y Scale: " + json.returnData.Curve.Value<String>("YDataScale") + Environment.NewLine +
                //                        "Z Scale: " + json.returnData.Curve.Value<String>("ZDataScale"); // Update Curve Info textbox 

                chartCurve.Series["Series1"].Points.Clear(); // Clear chart
                                                             //  chartCurve.Titles["Curve"].Text = (string)comboCurveNames.SelectedItem; // Set chart title
                try
                {
                    // Get X and Y data and plot them on the chart
                    double[] XData = json.returnData.Curve.Value<JArray>("XData").ToObject<double[]>();
                    double[] YData = json.returnData.Curve.Value<JArray>("YData").ToObject<double[]>();

                    for (int i = 0; i < XData.Length; i++)
                    {
                        chartCurve.Series["Series1"].Points.AddXY(XData[i], YData[i]);
                    }

                    chartCurve.ChartAreas[0].AxisX.Minimum = XData.Min();
                    chartCurve.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0}";

                    if (XData.Min() > 0 && json.returnData.Curve.Value<String>("XAxisScale") == "Log")
                    {
                        chartCurve.ChartAreas[0].AxisX.IsLogarithmic = true;
                    }
                    else
                    {
                        chartCurve.ChartAreas[0].AxisX.IsLogarithmic = false;
                    }
                }
                catch (Exception)
                {
                    //MessageBox.Show(x.Message.ToString());
                    // Not all curves will have array data. Ignore exceptions.
                }
            }
            else
            {
                // textBoxCurveInfo.Text = "Curve data not found!";
                chartCurve.ChartAreas[0].AxisX.IsLogarithmic = false;
                foreach (var series in chartCurve.Series)
                {
                    series.Points.Clear();
                }
                chartCurve.Titles["Curve"].Text = "";
            }
        }

        private void GetAllNames_Click(object sender, EventArgs e)
        {
            SoundCheck.GetAllCurves(out Dictionary<string, Dictionary<string, object>> xxx);
            SoundCheck.GetAllNames(out string[] Names);


        }

        private void button1_Click(object sender, EventArgs e)
        {
            MerryDllFramework.MerryDll dll = new MerryDllFramework.MerryDll();
            dll.Interface(new Dictionary<string, object>() { { "SqcPath", @"C:\Users\ch200001\source\repos\MerryTest\Z_AllDll\SoundCheck_V1\WindowsFormsApp1\bin\Debug\aaa.sqc" }, { "adminPath", "D:\\DAT" }, { "Name", "MX001" }, { "SN", "1234" } });
            dll.Run(new object[] { "dllname=SoundCheck_V1&method=Calibration_Sensitivity&FuncName=LFF_Gain&Frequency=1000&CurvesName=Fundamental-L FF MIC&Uplimit=LFF Calibration Upper Limit&LowLimit=LFF Calibration Lower Limit&TestCount=2" });

        }
    }
}
