
namespace Audio
{
    partial class fMessage
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
            this.lblMes = new System.Windows.Forms.Label();
            this.btnMsNO = new System.Windows.Forms.Button();
            this.btnMsYES = new System.Windows.Forms.Button();
            this.timerMs = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lblMes
            // 
            this.lblMes.Font = new System.Drawing.Font("SimSun", 19.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMes.Location = new System.Drawing.Point(12, 19);
            this.lblMes.Name = "lblMes";
            this.lblMes.Size = new System.Drawing.Size(646, 161);
            this.lblMes.TabIndex = 10;
            this.lblMes.Text = "label1";
            this.lblMes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnMsNO
            // 
            this.btnMsNO.BackColor = System.Drawing.Color.Red;
            this.btnMsNO.Font = new System.Drawing.Font("SimSun", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnMsNO.Location = new System.Drawing.Point(413, 198);
            this.btnMsNO.Name = "btnMsNO";
            this.btnMsNO.Size = new System.Drawing.Size(200, 77);
            this.btnMsNO.TabIndex = 9;
            this.btnMsNO.Text = "NO";
            this.btnMsNO.UseVisualStyleBackColor = false;
            this.btnMsNO.Click += new System.EventHandler(this.btnMsNO_Click);
            // 
            // btnMsYES
            // 
            this.btnMsYES.BackColor = System.Drawing.Color.LimeGreen;
            this.btnMsYES.Font = new System.Drawing.Font("SimSun", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnMsYES.Location = new System.Drawing.Point(74, 198);
            this.btnMsYES.Name = "btnMsYES";
            this.btnMsYES.Size = new System.Drawing.Size(206, 77);
            this.btnMsYES.TabIndex = 8;
            this.btnMsYES.Text = "YES";
            this.btnMsYES.UseVisualStyleBackColor = false;
            this.btnMsYES.Click += new System.EventHandler(this.btnMsYES_Click);
            // 
            // timerMs
            // 
            this.timerMs.Tick += new System.EventHandler(this.timerMs_Tick);
            // 
            // fMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 302);
            this.Controls.Add(this.lblMes);
            this.Controls.Add(this.btnMsNO);
            this.Controls.Add(this.btnMsYES);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "fMessage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Message";
            this.Load += new System.EventHandler(this.fMessage_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fMessage_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblMes;
        private System.Windows.Forms.Button btnMsNO;
        private System.Windows.Forms.Button btnMsYES;
        private System.Windows.Forms.Timer timerMs;
    }
}