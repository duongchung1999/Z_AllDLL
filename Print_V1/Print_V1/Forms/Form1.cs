using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Print
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            Thread.Sleep(50);
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            System.Drawing.Printing.PrinterSettings.StringCollection PrinterList = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
            foreach (var item in PrinterList)
                listBox1.Items.Add(item.ToString());
            this.TopMost = true;
        }
        public string printName = "";
        private void button1_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index == -1) return;

            string str = printName = listBox1.Items[index].ToString();
            this.Close();
        }
    }
}
