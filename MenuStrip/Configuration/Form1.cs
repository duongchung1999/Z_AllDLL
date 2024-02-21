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
        public static Form1 GetForm1(string path)
        {
            if (form1 == null || form1.IsDisposed)
            {
                form1 = new Form1(path);
            }
            return form1;
        }
        public Form1(string Path)
        {
            InitializeComponent();
            path = Path;
        }
        string path;
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            using (StreamWriter writer = new StreamWriter(path, false)) { writer.WriteLine(textBox1.Text); }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                textBox1.Text = reader.ReadToEnd();
            }
            textBox1.SelectionStart = textBox1.Text.Length-1;


        }
    }
}
