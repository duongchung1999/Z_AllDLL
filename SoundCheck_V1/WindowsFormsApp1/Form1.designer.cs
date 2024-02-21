namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.RunSqc1 = new System.Windows.Forms.Button();
            this.tb_CMD = new System.Windows.Forms.TextBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.Connect = new System.Windows.Forms.Button();
            this.GetResult = new System.Windows.Forms.Button();
            this.StartTest = new System.Windows.Forms.Button();
            this.RunSqc2 = new System.Windows.Forms.Button();
            this.Close = new System.Windows.Forms.Button();
            this.GetStepResult = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.chartCurve = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.GetAllNames = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.chartCurve)).BeginInit();
            this.SuspendLayout();
            // 
            // RunSqc1
            // 
            this.RunSqc1.Location = new System.Drawing.Point(33, 168);
            this.RunSqc1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.RunSqc1.Name = "RunSqc1";
            this.RunSqc1.Size = new System.Drawing.Size(127, 38);
            this.RunSqc1.TabIndex = 0;
            this.RunSqc1.Text = "运行ANC OFF Sqc";
            this.RunSqc1.UseVisualStyleBackColor = true;
            this.RunSqc1.Click += new System.EventHandler(this.RunSqc_Click);
            // 
            // tb_CMD
            // 
            this.tb_CMD.Location = new System.Drawing.Point(33, 71);
            this.tb_CMD.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_CMD.Multiline = true;
            this.tb_CMD.Name = "tb_CMD";
            this.tb_CMD.Size = new System.Drawing.Size(257, 33);
            this.tb_CMD.TabIndex = 1;
            this.tb_CMD.Text = "NC off curve R";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(307, 12);
            this.listBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(225, 424);
            this.listBox1.TabIndex = 2;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(539, 12);
            this.textBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(249, 424);
            this.textBox2.TabIndex = 3;
            // 
            // Connect
            // 
            this.Connect.Location = new System.Drawing.Point(33, 124);
            this.Connect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Connect.Name = "Connect";
            this.Connect.Size = new System.Drawing.Size(127, 38);
            this.Connect.TabIndex = 0;
            this.Connect.Text = "建立连接";
            this.Connect.UseVisualStyleBackColor = true;
            this.Connect.Click += new System.EventHandler(this.Connect_Click);
            // 
            // GetResult
            // 
            this.GetResult.Location = new System.Drawing.Point(33, 296);
            this.GetResult.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetResult.Name = "GetResult";
            this.GetResult.Size = new System.Drawing.Size(127, 38);
            this.GetResult.TabIndex = 0;
            this.GetResult.Text = "获取测试结果";
            this.GetResult.UseVisualStyleBackColor = true;
            this.GetResult.Click += new System.EventHandler(this.GetResult_Click);
            // 
            // StartTest
            // 
            this.StartTest.Location = new System.Drawing.Point(33, 252);
            this.StartTest.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.StartTest.Name = "StartTest";
            this.StartTest.Size = new System.Drawing.Size(127, 38);
            this.StartTest.TabIndex = 0;
            this.StartTest.Text = "开始测试";
            this.StartTest.UseVisualStyleBackColor = true;
            this.StartTest.Click += new System.EventHandler(this.StartTest_Click);
            // 
            // RunSqc2
            // 
            this.RunSqc2.Location = new System.Drawing.Point(33, 210);
            this.RunSqc2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.RunSqc2.Name = "RunSqc2";
            this.RunSqc2.Size = new System.Drawing.Size(127, 38);
            this.RunSqc2.TabIndex = 0;
            this.RunSqc2.Text = "运行ANC ON Sqc";
            this.RunSqc2.UseVisualStyleBackColor = true;
            this.RunSqc2.Click += new System.EventHandler(this.RunSqc2_Click);
            // 
            // Close
            // 
            this.Close.Location = new System.Drawing.Point(33, 425);
            this.Close.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Close.Name = "Close";
            this.Close.Size = new System.Drawing.Size(127, 38);
            this.Close.TabIndex = 0;
            this.Close.Text = "断开连接";
            this.Close.UseVisualStyleBackColor = true;
            // 
            // GetStepResult
            // 
            this.GetStepResult.Location = new System.Drawing.Point(33, 339);
            this.GetStepResult.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetStepResult.Name = "GetStepResult";
            this.GetStepResult.Size = new System.Drawing.Size(127, 38);
            this.GetStepResult.TabIndex = 0;
            this.GetStepResult.Text = "GetStepResult";
            this.GetStepResult.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(33, 382);
            this.button2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(127, 38);
            this.button2.TabIndex = 0;
            this.button2.Text = "获取曲线";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // chartCurve
            // 
            this.chartCurve.BorderlineColor = System.Drawing.Color.DimGray;
            this.chartCurve.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.AxisX.MinorTickMark.Enabled = true;
            chartArea1.Name = "ChartArea1";
            this.chartCurve.ChartAreas.Add(chartArea1);
            this.chartCurve.Location = new System.Drawing.Point(793, 12);
            this.chartCurve.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chartCurve.Name = "chartCurve";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.IsVisibleInLegend = false;
            series1.Name = "Series1";
            this.chartCurve.Series.Add(series1);
            this.chartCurve.Size = new System.Drawing.Size(608, 424);
            this.chartCurve.TabIndex = 10;
            this.chartCurve.Text = "chart1";
            title1.Name = "Curve";
            title1.Text = "Curve";
            this.chartCurve.Titles.Add(title1);
            // 
            // GetAllNames
            // 
            this.GetAllNames.Location = new System.Drawing.Point(165, 252);
            this.GetAllNames.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetAllNames.Name = "GetAllNames";
            this.GetAllNames.Size = new System.Drawing.Size(127, 38);
            this.GetAllNames.TabIndex = 0;
            this.GetAllNames.Text = "获取所有曲线名称";
            this.GetAllNames.UseVisualStyleBackColor = true;
            this.GetAllNames.Click += new System.EventHandler(this.GetAllNames_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(33, 11);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(127, 38);
            this.button1.TabIndex = 0;
            this.button1.Text = "试试";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1431, 521);
            this.Controls.Add(this.chartCurve);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.tb_CMD);
            this.Controls.Add(this.StartTest);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.GetStepResult);
            this.Controls.Add(this.Close);
            this.Controls.Add(this.GetAllNames);
            this.Controls.Add(this.GetResult);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Connect);
            this.Controls.Add(this.RunSqc2);
            this.Controls.Add(this.RunSqc1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chartCurve)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button RunSqc1;
        private System.Windows.Forms.TextBox tb_CMD;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button Connect;
        private System.Windows.Forms.Button GetResult;
        private System.Windows.Forms.Button StartTest;
        private System.Windows.Forms.Button RunSqc2;
        private System.Windows.Forms.Button Close;
        private System.Windows.Forms.Button GetStepResult;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartCurve;
        private System.Windows.Forms.Button GetAllNames;
        private System.Windows.Forms.Button button1;
    }
}

