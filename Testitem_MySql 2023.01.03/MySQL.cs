using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerryDllFramework
{
    /// <summary>
    /// Mysql数据库操作类,由于读取数据库操作过于简单，不使用ORM框架
    /// </summary>
    internal static class MySqlHelper
    {
        private static string Constr = "database=te_test;Password=merry@TE;user ID=merryte;server=10.55.2.20";
        private static MySqlConnection conn = new MySqlConnection(Constr);
        /// <summary>
        /// 判断数据库是否连接成功
        /// </summary>
        /// <returns></returns>
        internal static bool IsOpen()
        {
            try
            {
                conn.Open();
                return conn.State == ConnectionState.Open ? true : false;
            }
            catch
            {
                return false;
            }
            finally
            {
                conn.Close();
            }
        }
        /// <summary>
        /// 异步查询单条数据
        /// </summary>
        /// <returns>返回字符串集合结果</returns>
        internal static Task<List<string>> GetData(string sql)
        {
            return Task.Run(() =>
            {
                var st = new List<string>();
                try
                {
                    conn.Open();
                    MySqlCommand comm2 = new MySqlCommand(sql, conn);
                    comm2.CommandTimeout = 5000;
                    MySqlDataReader readers = comm2.ExecuteReader();
                    while (readers.Read())
                    {
                        for (var i = 0; i < readers.VisibleFieldCount; i++)
                        {
                            st.Add(readers[i].ToString());
                        }
                    }
                    conn.Close();
                    return st;
                }
                catch
                {
                    conn.Close();
                    return null;
                }


            });


        }
        /// <summary>
        /// 查询多条数据--数组集合
        /// </summary>
        /// <returns>返回字符串数组集合结果</returns>
        internal static Task<List<string[]>> GetDataList(string sql)
        {
            var st = new List<string[]>();
            return Task.Run(() =>
            {
                try
                {
                    conn.Open();
                    MySqlCommand comm2 = new MySqlCommand(sql, conn);
                    comm2.CommandTimeout = 5000;

                    MySqlDataReader readers = comm2.ExecuteReader();
                    while (readers.Read())
                    {
                        string[] st1 = new string[readers.VisibleFieldCount];
                        for (int i = 0; i < readers.VisibleFieldCount; i++)
                        {
                            st1[i] = readers[i].ToString();
                        }
                        st.Add(st1);
                    }
                    if (st.Count() == 0)
                    {
                        conn.Close();
                        return null;
                    }
                    conn.Close();
                    return st;
                }
                catch
                {
                    conn.Close();
                    return null;
                }

            });

        }
        /// <summary>
        /// 查询多条数据--字典
        /// </summary>
        /// <returns>返回字符串数组集合结果</returns>
        internal static Task<Dictionary<string, string>> GetDataDictionary(string sql)
        {
            var st = new Dictionary<string, string>();
            return Task.Run(() =>
            {
                try
                {
                    conn.Open();
                    MySqlCommand comm2 = new MySqlCommand(sql, conn);
                    comm2.CommandTimeout = 5000;
                    MySqlDataReader readers = comm2.ExecuteReader();
                    while (readers.Read())
                    {
                        string[] st1 = new string[readers.VisibleFieldCount];
                        for (int i = 0; i < readers.VisibleFieldCount; i++)
                        {
                            st1[i] = readers[i].ToString();
                        }
                        StringBuilder sb = new StringBuilder();
                        for (var j = 1; j < st1.Length; j++)
                        {
                            sb.Append(st1[j]).Append(",");
                        }
                        sb.Remove(sb.Length - 1, 1);
                        st.Add(st1[0], sb.ToString());
                    }
                    if (st.Count() == 0)
                    {
                        conn.Close();
                        return null;
                    }
                    conn.Close();
                    return st;
                }
                catch
                {
                    conn.Close();
                    return null;
                }

            });

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
        static string Constr = "database=testlog;Password=merry@TE;user ID=merryte;server=10.55.2.20";

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
        internal static Task<bool> ExecuteSql(string TypeName, string sql)
        {
            return Task.Run(() =>
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
                    Console.WriteLine($"数据库插入语句：{sqltable + sql}");
                    cmd.CommandText = sqltable + sql;
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    File.AppendAllText(@".\TestData\SetMysqlBug.txt", $"{DateTime.Now.ToString()}\r\n{ex}\r\n");
                    return false;
                }

            });
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
            tableName = $"{  TypeName.Replace('-', '_')}_{year}_{quarter}";

            return false;
        }

    }
}
