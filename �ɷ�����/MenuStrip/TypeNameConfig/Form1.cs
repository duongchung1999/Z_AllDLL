using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace Configuration
{
    public partial class Form1 : Form
    {
        static Form1 form1;
        static string ConfigPath;

        public static Form1 GetForm1(string configPath)
        {
            ConfigPath = configPath;

            if (form1 == null || form1.IsDisposed)
            {
                form1 = new Form1();

            }
            return form1;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

            File.WriteAllText(ConfigPath, textBox1.Text);

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(ConfigPath)) File.Create(ConfigPath);
            textBox1.Text = File.ReadAllText(ConfigPath);
        }
    }
}
