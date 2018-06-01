using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.MySQLProvider
{
    internal class DbHelper
    {
        private string _connectionString;
        public DbHelper(string connectionString)
        {
            this._connectionString = connectionString;
        }
        public DataTable ListBySql(string sql, params DbParameter[] paras)
        {
            MySqlConnection conn = new MySqlConnection(this._connectionString);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = conn;
            if (paras != null && paras.Length > 0)
                cmd.Parameters.AddRange(paras);

            DataTable dt = new DataTable();
            conn.Open();
            using (MySqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                dt.Load(dr);
            }
            conn.Close();
            return dt;
        }
    }
}
