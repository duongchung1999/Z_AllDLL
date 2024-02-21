using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Config_ini.Class
{
    /// <summary>
    /// Mysql数据库操作类,由于读取数据库操作过于简单，不使用ORM框架
    /// </summary>
    internal class MySqlHelper
    {
        const string Constr = //"Server=10.175.5.59;database=te_test;uid=merryte;pwd=merry@TE;AllowLoadLocalInfile=true";
                          "database=te_test;Password=merry@TE;user ID=merryte;server=10.175.5.59;SslMode=none";
        static MySqlConnection conn = new MySqlConnection(Constr);
        static MySqlCommand comm;
        /// <summary>
        /// 判断数据库是否连接成功
        /// </summary>
        /// <returns></returns>
        public static bool Open()
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Dispose();
                    conn.Open();
                }
                return conn.State == ConnectionState.Open ? true : false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 查询多条数据--数组集合
        /// </summary>
        /// <returns>返回字符串数组集合结果</returns>
        public static bool GetDataList(string sql, out string[][] dataTable)
        {
            dataTable = new string[0][];

            try
            {
               // MessageBox.Show($"检查状态{Open()}");

                if (!Open()) return false;

                comm = new MySqlCommand(sql, conn);
                comm.CommandTimeout = 3000;
                MySqlDataReader readers = comm.ExecuteReader();
                var st = new List<string[]>();
                while (readers.Read())
                {
                    string[] st1 = new string[readers.VisibleFieldCount];
                    for (int i = 0; i < readers.VisibleFieldCount; i++)
                        st1[i] = readers[i].ToString();
                    st.Add(st1);
                }
                dataTable = st.ToArray();
                readers.Dispose();
                comm.Dispose();
                return dataTable.Length != 0;
            }
            catch (Exception ex)
            {

                return false;
            }

        }
        /// <summary>
        /// 增删改
        /// </summary>
        /// <returns>返回执行是否成功</returns>
        internal static Task<bool> ExecuteSql(string sql)
        {
            return Task.Run(() =>
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.CommandTimeout = 5000;
                    cmd.ExecuteNonQuery();

                    conn.Close();
                    return true;
                }
                catch
                {
                    conn.Close();
                    return false;
                }

            });
        }
    }
    internal static class MySqlTestData
    {
        public static string TableName = "";
        static string Constr = "database=testlog;Password=merry@TE;user ID=merryte;server=10.175.5.59";

        static MySqlConnection conn = new MySqlConnection(Constr);
        static MySqlCommand cmd;
        static MySqlDataReader readers;

        /// <summary>
        /// 判断数据库是否连接成功
        /// </summary>
        /// <returns></returns>
        internal static bool IsOpen() => conn.State == ConnectionState.Open ? true : false;
        internal static bool IsClose() => conn.State == ConnectionState.Closed ? true : false;
        internal static void Closed()
        {
            if (!IsClose())
                conn.Close();
        }
        /// <summary>
        /// 查询多条数据--数组集合
        /// </summary>
        /// <returns>返回字符串数组集合结果</returns>
        static Task<List<string[]>> GetDataList(string sql)
        {
            var st = new List<string[]>();
            return Task.Run(() =>
            {
                if (!IsOpen())
                {
                    if (!IsClose()) Closed();
                    conn.Open();
                    cmd.Connection = conn;
                }
                cmd.CommandText = sql;
                readers = cmd.ExecuteReader();
                while (readers.Read())
                {
                    string[] st1 = new string[readers.VisibleFieldCount];
                    for (int i = 0; i < readers.VisibleFieldCount; i++)
                    {
                        st1[i] = readers[i].ToString();
                    }
                    st.Add(st1);
                }
                if (!readers.IsClosed) readers.Close();
                return st;
            });

        }
        /// <summary>
        /// 增删改
        /// </summary>
        /// <returns>返回执行是否成功</returns>
        internal static bool ExecuteSql(string TypeName, string sql)
        {

            try
            {
                if (!IsOpen())
                {
                    if (!IsClose()) Closed();
                    conn.Open();
                    cmd = new MySqlCommand();
                    cmd.CommandTimeout = 5000;
                    cmd.Connection = conn;
                    if (TableName == "")
                    {
                        if (!CheckTypeName(TypeName, out TableName))
                        {
                            string CreateTable = $@"CREATE TABLE IF NOT EXISTS `{TableName}`(
		`SN` VARCHAR(80),
   `Time` TIMESTAMP NOT NULL DEFAULT  CURRENT_TIMESTAMP,
   `Result` VARCHAR(5) ,
   `Testlog` LONGTEXT ,
   `Station`  VARCHAR(20),
   workorders VARCHAR(20),
   `MachineName` VARCHAR(50),
  `TestTime`  VARCHAR(10)
)ENGINE = MyISAM DEFAULT CHARSET = utf8; ";
                            cmd.CommandText = CreateTable;
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            try
                            {
                                cmd.CommandText = $"alter table {TableName} add column TestTime varchar(50) ;";
                                cmd.ExecuteNonQuery();
                            }
                            catch { }
                        }
                    }

                }
                string sqltable = $" INSERT INTO {TableName}";
                cmd.CommandText = sqltable + sql;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText($@".\Log\错误信息{DateTime.Now:MM_dd}.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                return false;
            }

        }
        static bool CheckTypeName(string TypeName, out string tableName)
        {
            string year = DateTime.Now.ToString("yy");
            int month = int.Parse(DateTime.Now.ToString("MM"));
            string quarter = "0";
            TypeName = TypeName.ToLower();
            if (month <= 3)
            {
                quarter = "1";
            }
            else if (month <= 6)
            {
                quarter = "2";
            }
            else if (month <= 9)
            {
                quarter = "3";
            }
            else if (month <= 12)
            {
                quarter = "4";
            }
            List<string[]> SqlTable = GetDataList("SHOW TABLES").Result;
            foreach (var item in SqlTable)
            {
                string[] name = item[0].ToLower().Split('_');
                if (TypeName.Contains("_"))
                {
                    if (name.Length == 4)
                    {
                        string[] Typename4 = TypeName.Split('_');

                        if (Typename4[0] == name[0] && Typename4[1] == name[1] && year == name[2] && quarter == name[3])
                        {
                            tableName = item[0];
                            return true;
                        }
                    }

                }
                if (TypeName.Contains("-"))
                {
                    if (name.Length == 4)
                    {
                        TypeName = TypeName.Replace('-', '_');
                        string[] Typename4 = TypeName.Split('_');

                        if (Typename4[0] == name[0] && Typename4[1] == name[1] && year == name[2] && quarter == name[3])
                        {
                            tableName = item[0];
                            return true;
                        }
                    }
                }
                if (name.Length == 3)
                {

                    if (TypeName == name[0] && year == name[1] && quarter == name[2])
                    {
                        tableName = item[0];
                        return true;
                    }
                }


            }
            tableName = $"{TypeName.Replace('-', '_')}_{year}_{quarter}";

            return false;
        }

    }
}
