namespace Config_ini.Forms
{
    partial class UpdataInfoForms
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.DeleteSelect = new System.Windows.Forms.Button();
            this.ConfirmUpdata = new System.Windows.Forms.Button();
            this.CancelUpdata = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(164, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(324, 379);
            this.listBox1.TabIndex = 0;
            // 
            // DeleteSelect
            // 
            this.DeleteSelect.Location = new System.Drawing.Point(25, 78);
            this.DeleteSelect.Name = "DeleteSelect";
            this.DeleteSelect.Size = new System.Drawing.Size(103, 37);
            this.DeleteSelect.TabIndex = 1;
            this.DeleteSelect.Text = "删除";
            this.DeleteSelect.UseVisualStyleBackColor = true;
            this.DeleteSelect.Click += new System.EventHandler(this.DeleteSelect_Click);
            // 
            // ConfirmUpdata
            // 
            this.ConfirmUpdata.Location = new System.Drawing.Point(25, 183);
            this.ConfirmUpdata.Name = "ConfirmUpdata";
            this.ConfirmUpdata.Size = new System.Drawing.Size(103, 37);
            this.ConfirmUpdata.TabIndex = 1;
            this.ConfirmUpdata.Text = "确定更新";
            this.ConfirmUpdata.UseVisualStyleBackColor = true;
            this.ConfirmUpdata.Click += new System.EventHandler(this.ConfirmUpdata_Click);
            // 
            // CancelUpdata
            // 
            this.CancelUpdata.Location = new System.Drawing.Point(25, 253);
            this.CancelUpdata.Name = "CancelUpdata";
            this.CancelUpdata.Size = new System.Drawing.Size(103, 37);
            this.CancelUpdata.TabIndex = 1;
            this.CancelUpdata.Text = "取消更新";
            this.CancelUpdata.UseVisualStyleBackColor = true;
            this.CancelUpdata.Click += new System.EventHandler(this.CancelUpdata_Click);
            // 
            // UpdataInfoForms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 424);
            this.Controls.Add(this.CancelUpdata);
            this.Controls.Add(this.ConfirmUpdata);
            this.Controls.Add(this.DeleteSelect);
            this.Controls.Add(this.listBox1);
            this.Name = "UpdataInfoForms";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UpdataInfoForms";
            this.Load += new System.EventHandler(this.UpdataInfoForms_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button DeleteSelect;
        private System.Windows.Forms.Button ConfirmUpdata;
        private System.Windows.Forms.Button CancelUpdata;
    }
}