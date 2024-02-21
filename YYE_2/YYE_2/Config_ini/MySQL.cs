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
    public static class MySqlHelper
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
        public static Task<List<string[]>> GetDataList(string sql)
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
                    return st;
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
}
