
namespace Common
{
    partial class Box
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
            this.YES = new System.Windows.Forms.Button();
            this.NO = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // YES
            // 
            this.YES.Location = new System.Drawing.Point(89, 182);
            this.YES.Name = "YES";
            this.YES.Size = new System.Drawing.Size(90, 46);
            this.YES.TabIndex = 1;
            this.YES.Text = "是(Y)";
            this.YES.UseVisualStyleBackColor = true;
            this.YES.Click += new System.EventHandler(this.YES_Click);
            // 
            // NO
            // 
            this.NO.Location = new System.Drawing.Point(304, 182);
            this.NO.Name = "NO";
            this.NO.Size = new System.Drawing.Size(90, 46);
            this.NO.TabIndex = 2;
            this.NO.Text = "否(N)";
            this.NO.UseVisualStyleBackColor = true;
            this.NO.Click += new System.EventHandler(this.NO_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(493, 162);
            this.label1.TabIndex = 5;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Box
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 264);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NO);
            this.Controls.Add(this.YES);
            this.KeyPreview = true;
            this.Name = "Box";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Messagebox";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Box_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Box_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button YES;
        private System.Windows.Forms.Button NO;
        private System.Windows.Forms.Label label1;
    }
}