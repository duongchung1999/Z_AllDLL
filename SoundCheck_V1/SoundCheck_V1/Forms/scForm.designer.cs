
namespace MerryDllFramework
{
    partial class scForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(scForm));
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.YES = new System.Windows.Forms.Button();
            this.NO = new System.Windows.Forms.Button();
            this.la_time = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.BackColor = System.Drawing.Color.Yellow;
            this.progressBar1.ForeColor = System.Drawing.Color.Yellow;
            this.progressBar1.Location = new System.Drawing.Point(13, 89);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(621, 49);
            this.progressBar1.TabIndex = 10;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // YES
            // 
            this.YES.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.YES.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.YES.Location = new System.Drawing.Point(161, 147);
            this.YES.Margin = new System.Windows.Forms.Padding(4);
            this.YES.Name = "YES";
            this.YES.Size = new System.Drawing.Size(97, 46);
            this.YES.TabIndex = 0;
            this.YES.Text = "是";
            this.YES.UseVisualStyleBackColor = true;
            this.YES.Click += new System.EventHandler(this.YES_Click);
            // 
            // NO
            // 
            this.NO.BackColor = System.Drawing.Color.Gainsboro;
            this.NO.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.NO.Location = new System.Drawing.Point(376, 147);
            this.NO.Margin = new System.Windows.Forms.Padding(4);
            this.NO.Name = "NO";
            this.NO.Size = new System.Drawing.Size(97, 46);
            this.NO.TabIndex = 1;
            this.NO.Text = "否";
            this.NO.UseVisualStyleBackColor = false;
            this.NO.Click += new System.EventHandler(this.NO_Click);
            // 
            // la_time
            // 
            this.la_time.AutoSize = true;
            this.la_time.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.la_time.Location = new System.Drawing.Point(284, 162);
            this.la_time.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.la_time.Name = "la_time";
            this.la_time.Size = new System.Drawing.Size(42, 20);
            this.la_time.TabIndex = 2;
            this.la_time.Text = "time";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.LawnGreen;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(622, 73);
            this.label1.TabIndex = 11;
            this.label1.Text = "等待Sound Check完全启动后 点击“是”";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // scForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(647, 204);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.la_time);
            this.Controls.Add(this.NO);
            this.Controls.Add(this.YES);
            this.Controls.Add(this.progressBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "scForm";
            this.Text = "scForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.scForm_FormClosed);
            this.Load += new System.EventHandler(this.scForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button YES;
        private System.Windows.Forms.Button NO;
        private System.Windows.Forms.Label la_time;
        private System.Windows.Forms.Label label1;
    }
}