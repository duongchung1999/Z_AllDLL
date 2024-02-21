
namespace MerryDllFramework
{
    partial class CurvesForms
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CurvesForms));
            this.Balance = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lb_L = new System.Windows.Forms.Label();
            this.lb_Balance = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btn_Close = new System.Windows.Forms.Button();
            this.CurvesDiffTimer = new System.Windows.Forms.Timer(this.components);
            this.CurvesTimer_ = new System.Windows.Forms.Timer(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.L_R = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.button1 = new System.Windows.Forms.Button();
            this.lb_R = new System.Windows.Forms.Label();
            this.lb_Gain = new System.Windows.Forms.Label();
            this.lb_tatle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Balance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.L_R)).BeginInit();
            this.SuspendLayout();
            // 
            // Balance
            // 
            this.Balance.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.Name = "ChartArea1";
            this.Balance.ChartAreas.Add(chartArea1);
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend1.Name = "Legend1";
            this.Balance.Legends.Add(legend1);
            this.Balance.Location = new System.Drawing.Point(470, 37);
            this.Balance.Margin = new System.Windows.Forms.Padding(4);
            this.Balance.Name = "Balance";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.Balance.Series.Add(series1);
            this.Balance.Size = new System.Drawing.Size(451, 298);
            this.Balance.TabIndex = 0;
            this.Balance.Text = "chart1";
            // 
            // lb_L
            // 
            this.lb_L.BackColor = System.Drawing.Color.LimeGreen;
            this.lb_L.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_L.ForeColor = System.Drawing.Color.Yellow;
            this.lb_L.Location = new System.Drawing.Point(12, 8);
            this.lb_L.Name = "lb_L";
            this.lb_L.Size = new System.Drawing.Size(67, 25);
            this.lb_L.TabIndex = 2;
            this.lb_L.Text = "L:";
            this.lb_L.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lb_Balance
            // 
            this.lb_Balance.BackColor = System.Drawing.Color.LimeGreen;
            this.lb_Balance.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_Balance.Location = new System.Drawing.Point(465, 8);
            this.lb_Balance.Name = "lb_Balance";
            this.lb_Balance.Size = new System.Drawing.Size(139, 25);
            this.lb_Balance.TabIndex = 2;
            this.lb_Balance.Text = "Balance：";
            this.lb_Balance.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btn_Close
            // 
            this.btn_Close.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Close.Location = new System.Drawing.Point(758, 397);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(148, 49);
            this.btn_Close.TabIndex = 3;
            this.btn_Close.Text = "关闭 ：5";
            this.btn_Close.UseVisualStyleBackColor = true;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // CurvesDiffTimer
            // 
            this.CurvesDiffTimer.Interval = 1000;
            this.CurvesDiffTimer.Tick += new System.EventHandler(this.CurvesDiffTimer_Tick);
            // 
            // CurvesTimer_
            // 
            this.CurvesTimer_.Interval = 1000;
            this.CurvesTimer_.Tick += new System.EventHandler(this.CurvesTimer__Tick);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 353);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(740, 103);
            this.textBox1.TabIndex = 4;
            // 
            // L_R
            // 
            this.L_R.BorderlineColor = System.Drawing.Color.Gray;
            this.L_R.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea2.Name = "ChartArea1";
            this.L_R.ChartAreas.Add(chartArea2);
            legend2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend2.Name = "Legend1";
            this.L_R.Legends.Add(legend2);
            this.L_R.Location = new System.Drawing.Point(12, 37);
            this.L_R.Name = "L_R";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "Legend1";
            series2.Name = "xx8";
            this.L_R.Series.Add(series2);
            this.L_R.Size = new System.Drawing.Size(451, 298);
            this.L_R.TabIndex = 5;
            this.L_R.Text = "chart1";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(758, 355);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(148, 36);
            this.button1.TabIndex = 3;
            this.button1.Text = "延时 + 600s";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lb_R
            // 
            this.lb_R.BackColor = System.Drawing.Color.LimeGreen;
            this.lb_R.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_R.ForeColor = System.Drawing.Color.Blue;
            this.lb_R.Location = new System.Drawing.Point(85, 8);
            this.lb_R.Name = "lb_R";
            this.lb_R.Size = new System.Drawing.Size(67, 25);
            this.lb_R.TabIndex = 2;
            this.lb_R.Text = "R:";
            this.lb_R.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lb_Gain
            // 
            this.lb_Gain.BackColor = System.Drawing.Color.LimeGreen;
            this.lb_Gain.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_Gain.ForeColor = System.Drawing.Color.Blue;
            this.lb_Gain.Location = new System.Drawing.Point(669, 8);
            this.lb_Gain.Name = "lb_Gain";
            this.lb_Gain.Size = new System.Drawing.Size(252, 25);
            this.lb_Gain.TabIndex = 2;
            this.lb_Gain.Text = "Last: Next:";
            this.lb_Gain.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lb_tatle
            // 
            this.lb_tatle.BackColor = System.Drawing.Color.GhostWhite;
            this.lb_tatle.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_tatle.ForeColor = System.Drawing.Color.Black;
            this.lb_tatle.Location = new System.Drawing.Point(335, 7);
            this.lb_tatle.Name = "lb_tatle";
            this.lb_tatle.Size = new System.Drawing.Size(67, 25);
            this.lb_tatle.TabIndex = 2;
            this.lb_tatle.Text = "R:";
            this.lb_tatle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CurvesForms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 464);
            this.Controls.Add(this.L_R);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.lb_Balance);
            this.Controls.Add(this.lb_Gain);
            this.Controls.Add(this.lb_tatle);
            this.Controls.Add(this.lb_R);
            this.Controls.Add(this.lb_L);
            this.Controls.Add(this.Balance);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CurvesForms";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CurvesForms";
            this.Load += new System.EventHandler(this.CurvesForms_Load);
            this.Resize += new System.EventHandler(this.CurvesForms_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.Balance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.L_R)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.DataVisualization.Charting.Chart Balance;
        public System.Windows.Forms.Label lb_Balance;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.Timer CurvesDiffTimer;
        private System.Windows.Forms.Timer CurvesTimer_;
        private System.Windows.Forms.TextBox textBox1;
        public System.Windows.Forms.DataVisualization.Charting.Chart L_R;
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.Label lb_R;
        public System.Windows.Forms.Label lb_L;
        public System.Windows.Forms.Label lb_Gain;
        public System.Windows.Forms.Label lb_tatle;
    }
}