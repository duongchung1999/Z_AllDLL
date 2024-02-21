using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SoundCheck_V1.API
{
    internal static class MySqlTestData
    {
        public static string TableName = "";
        static string Constr = "database=AE_Curve;Password=merry@TE;user ID=merryte;server=10.55.2.20";

        static MySqlConnection conn = new MySqlConnection(Constr);
        static MySqlCommand cmd;
        static MySqlDataReader readers;

        /// <summary>
        /// 判断数据库是否连接成功
        /// </summary>
        /// <returns></returns>
        internal static bool IsOpen() => conn.State == ConnectionState.Open;
        internal static bool IsClose() => conn.State == ConnectionState.Closed;
        internal static void Closed()
        {
            if (!IsClose())
                conn.Close();
        }
        /// <summary>
        /// 查询多条数据--数组集合
        /// </summary>
        /// <returns>返回字符串数组集合结果</returns>
        static List<string[]> GetDataList(string sql)
        {
            var st = new List<string[]>();
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



        }
        /// <summary>
        /// 增删改
        /// </summary>
        /// <returns>返回执行是否成功</returns>
        internal static string ExecuteSql(string TypeName, string sql)
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
    `Time` TIMESTAMP NOT NULL DEFAULT  CURRENT_TIMESTAMP,
    `SN` VARCHAR(80),
    `Sequence` VARCHAR(100) ,
    `Station`  VARCHAR(50),
    `TestTime`  VARCHAR(10),
    `CurveName` LONGTEXT ,
    `CurveData` LONGTEXT ,
    `workorders` VARCHAR(20),
    `MachineName` VARCHAR(50),
    `dllver` VARCHAR(30)

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
                return "True";
            }
            catch (Exception ex)
            {
                return $"{ex.Message} False";
            }

            ;
        }
        static bool CheckTypeName(string TypeName, out string tableName)
        {
            string year = DateTime.Now.ToString("yy");
            int month = int.Parse(DateTime.Now.ToString("MM"));

            TypeName = TypeName.Replace("-", "_").ToLower();
            if (month <= 6)
            {
                TypeName = $"{year}_1_{TypeName}";
            }
            else
            {
                TypeName = $"{year}_2_{TypeName}";
            }
            tableName = TypeName;
            List<string[]> SqlTable = GetDataList("SHOW TABLES");
            foreach (var item in SqlTable)
            {
                if (TypeName == item[0])
                {
                    return true;
                }
            }
            return false;
        }

    }

}
