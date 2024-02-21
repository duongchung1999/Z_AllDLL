
namespace MerryDllFramework
{
    partial class ChormeLogin
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
            this.label1 = new System.Windows.Forms.Label();
            this.tb_User = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_ReadInfo = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_OrderWorks = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(6, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "工号：";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb_User
            // 
            this.tb_User.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb_User.Location = new System.Drawing.Point(87, 13);
            this.tb_User.Name = "tb_User";
            this.tb_User.Size = new System.Drawing.Size(292, 31);
            this.tb_User.TabIndex = 1;
            this.tb_User.Text = "MECL";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(6, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 29);
            this.label3.TabIndex = 2;
            this.label3.Text = "信息：";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb_ReadInfo
            // 
            this.tb_ReadInfo.Enabled = false;
            this.tb_ReadInfo.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb_ReadInfo.Location = new System.Drawing.Point(87, 107);
            this.tb_ReadInfo.Multiline = true;
            this.tb_ReadInfo.Name = "tb_ReadInfo";
            this.tb_ReadInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tb_ReadInfo.Size = new System.Drawing.Size(292, 79);
            this.tb_ReadInfo.TabIndex = 2;
            this.tb_ReadInfo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_User_KeyDown);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(232, 212);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(93, 40);
            this.button1.TabIndex = 4;
            this.button1.Text = "关闭";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(108, 212);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(93, 40);
            this.button2.TabIndex = 2;
            this.button2.Text = "登录";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(6, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 29);
            this.label2.TabIndex = 0;
            this.label2.Text = "工单：";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb_OrderWorks
            // 
            this.tb_OrderWorks.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb_OrderWorks.Location = new System.Drawing.Point(87, 60);
            this.tb_OrderWorks.Name = "tb_OrderWorks";
            this.tb_OrderWorks.Size = new System.Drawing.Size(292, 31);
            this.tb_OrderWorks.TabIndex = 0;
            this.tb_OrderWorks.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_User_KeyDown);
            // 
            // ChormeLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 266);
            this.ControlBox = false;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tb_ReadInfo);
            this.Controls.Add(this.tb_OrderWorks);
            this.Controls.Add(this.tb_User);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ChormeLogin";
            this.Text = "Chorme 系统登录";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox tb_User;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox tb_ReadInfo;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox tb_OrderWorks;
    }
}