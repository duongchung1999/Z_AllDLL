using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using MerryDllFramework;
using System.IO;
using System.Threading;

namespace AutoTest.Class
{
    public partial class fNetworkMonitor : Form
    {
        //Data data = new Data();
        private readonly Ping ping;
        string name = "Default";
        string model = "Default";
        string type_mode = "Default";
        
        Dictionary<string, object> Config;

        public fNetworkMonitor(string computerName, string Model, string TypeMode, Dictionary<string, object> Config)
        {
            
            InitializeComponent();
            ping = new Ping();
            name = computerName;
            model = Model;
            type_mode = TypeMode;
            this.Config = Config;
        }

        private void fNetworkMonitor_Load(object sender, EventArgs e)
        {
            CreatLog();
            CheckPC_name();
            Check_time();
            start_run();
            statusLabel.ForeColor = Color.White;
            this.StartPosition = FormStartPosition.Manual;
            Screen scr = Screen.PrimaryScreen; //đi lấy màn hình chính
            this.Left = (scr.WorkingArea.Width - this.Width);
            this.Top = 20;
            
        }
        private void start_run()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            timer1.Enabled = true;
        }
        
        private void CheckPC_name()

        {
            string comment = $"Select * from Model where model = '{model}'";
            try
            {
                object cpu = null;
                DataSet ds = MySqlHelper.Query(comment);
                try
                {
                    dataGridView1.DataSource = ds.Tables[0];
                    cpu = dataGridView1.Rows[0].Cells[0].Value;
                }
                catch
                {

                }
                if (cpu == null)
                {
                    // 增加 name 到SQL
                    string cmd2 = $"insert into Model(model) values('{model}')";
                    MySqlHelper.Query(cmd2);
                }
            }

            catch
            {

            }
            string cmd = $"Select * from Network where computer = '{name}'";
            
            try
            {
                object cpu = null;
                DataSet ds = MySqlHelper.Query(cmd);
                try
                {
                    dataGridView1.DataSource = ds.Tables[0];
                    cpu = dataGridView1.Rows[0].Cells[0].Value;
                }
                catch
                {

                }
                if (cpu == null)
                {
                    // 增加 name 到SQL
                    string cmd2 = $"insert into Network(computer,status1,model,type_mode) values('{name}','connected','{model}','{type_mode}')";
                    MySqlHelper.Query(cmd2);
                    Thread.Sleep(200);
                }
                //string cmd3 = $"Update Network  set performency = '100%',test_times='0',pass_times='0' where computer = '{name}'";
                //sql.Query(cmd3);
            }

            catch
            {

            }

        }
        private void Check_time()
        {
            string cmd = $"Select date,pass_times,test_times from Network where computer = '{name}'";

            try
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd");
                string times = null;
                DataSet ds = MySqlHelper.Query(cmd);
                try
                {
                    dataGridView1.DataSource = ds.Tables[0];
                    times = dataGridView1.Rows[0].Cells[0].Value.ToString();
                }
                catch
                {

                }
                if (times == null)
                {
                    // 增加 name 到SQL
                    string cmd2 = $"insert into Network(date) values('{time}')";
                    MySqlHelper.Query(cmd2);
                    Thread.Sleep(200);
                    string cmd3 = $"Update Network  set performency = '100%',test_times='0',pass_times='0' where computer = '{name}'";
                    MySqlHelper.Query(cmd3);
                    Config["testtimes"] = Convert.ToDouble(0);
                    Config["passtimes"] = Convert.ToDouble(0);
                }
                else if (times != time)
                {
                    //// 增加 name 到SQL
                    //Config["testtimes"] = 0;
                    //Config["Passtimes"] = 0;
                    string cmd2 = $"update Network set date = '{time}' where computer = '{name}'";
                    MySqlHelper.Query(cmd2);
                    Config["testtimes"] = Convert.ToDouble(0);
                    Config["passtimes"] = Convert.ToDouble(0);
                    Thread.Sleep(200);
                }
                else
                {
                    
                    Config["testtimes"] = Convert.ToDouble(dataGridView1.Rows[0].Cells[2].Value);
                    Config["passtimes"] = Convert.ToDouble(dataGridView1.Rows[0].Cells[1].Value);
                }
            }

            catch
            {

            }
        }
        private bool SetConnectStatus()
        {
            //string name = Config["ComputerName"].ToString();
            string cmd = $"Update Network  set status1 = 'connected' , type_mode = '{type_mode}', model = '{model}' where computer = '{name}'";
            try
            {
                DataSet ds = MySqlHelper.Query(cmd);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }

        }
        private void CreatLog()
        {
            string path = $".\\log\\Network {DateTime.Now:yyyy-MM-dd}-log.txt";
            string path2 = ".\\log";
            if (!Directory.Exists(path2))
            {
                Directory.CreateDirectory(path2);
                Thread.Sleep(1000);
            }
            if (!File.Exists(path))
            {
                using (File.Create(path))
                {
                };
                Thread.Sleep(1000);
            }
        }
        private void Savelog(string msg)
        {
            string path = $".\\log\\Network {DateTime.Now:yyyy-MM-dd}-log.txt";
            string log = File.ReadAllText(path);
            log = log + "\n\n" + $"{DateTime.Now:yyyy-MM-dd hh:mm:ss}  :\n" + msg;
            File.WriteAllText(path, log);
        }

        
        private void timer1_Tick(object sender, EventArgs e)
        {
            string endpoint = $"10.175.5.101";
            PingReply reply = null;
            try
            {

                reply = ping.Send(endpoint);
               
            }
            catch (Exception ex)
            {
                //timer1.Enabled = false;
                //string msg = $"Lỗi khi thực hiện ping/执行 ping 时出错: {ex}";
                //Savelog(msg);
                //fNotification f = new fNotification(msg);
                //f.ShowDialog();
            }

            if (reply != null && reply.Status == IPStatus.Success)
            {
                statusLabel.Text = "Connected";
                this.BackColor = Color.Green;
                SetConnectStatus();
                textBoxResult.Text = $"Reply from {reply.Address}: bytes={reply.Buffer.Length} time={reply.RoundtripTime}ms TTL={reply.Options.Ttl}\n";
            }
            else
            {
                textBoxResult.Text = "";
                statusLabel.Text = "Disconnected";
                this.BackColor = Color.Red;
                //timer1.Enabled = false;
                //MessageBox.Show($"Không thể ping đến địa chỉ mạng/无法 ping 网络地址 {endpoint}");
                string msg = $"Không thể ping đến địa chỉ mạng/无法 ping 网络地址 {endpoint}";
                Savelog(msg);
                //fNotification f = new fNotification(msg);

                //f.ShowDialog();
                Thread.Sleep(1000);
                //timer1.Enabled = true;
            }
        }

        private void statusLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
