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
            this.label2 = new System.Windows.Forms.Label();
            this.YES1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(87, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 32);
            this.label2.TabIndex = 0;
            // 
            // YES1
            // 
            this.YES1.Location = new System.Drawing.Point(160, 145);
            this.YES1.Name = "YES1";
            this.YES1.Size = new System.Drawing.Size(117, 73);
            this.YES1.TabIndex = 1;
            this.YES1.Text = "Yes";
            this.YES1.UseVisualStyleBackColor = true;
            this.YES1.Click += new System.EventHandler(this.YES1_Click_1);
            // 
            // Box
            // 
            this.ClientSize = new System.Drawing.Size(428, 277);
            this.Controls.Add(this.YES1);
            this.Controls.Add(this.label2);
            this.Name = "Box";
            this.Load += new System.EventHandler(this.Box_Load_1);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button YES;
        private System.Windows.Forms.Button NO;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button YES1;
    }
}