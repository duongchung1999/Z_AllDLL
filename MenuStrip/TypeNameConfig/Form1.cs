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
        static string MessagePath;
        public static Form1 GetForm1(string configPath, string messagePath)
        {
            ConfigPath = configPath;
            MessagePath = messagePath;
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
            using (StreamWriter writer = new StreamWriter(ConfigPath, false, Encoding.UTF8)) { writer.WriteLine(textBox1.Text); }
            using (StreamWriter writer = new StreamWriter(MessagePath, false, Encoding.UTF8)) { writer.WriteLine(textBox2.Text); }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(ConfigPath)) File.Create(ConfigPath);
            using (StreamReader reader = new StreamReader(ConfigPath, Encoding.UTF8))
            {
                textBox1.Text = reader.ReadToEnd();
            }
            if (!File.Exists(MessagePath)) File.Create(MessagePath);
            using (StreamReader reader = new StreamReader(MessagePath, Encoding.UTF8))
            {
                textBox2.Text = reader.ReadToEnd();
            }

            textBox2.SelectionStart = textBox2.Text.Length - 1;
        }
    }
}
