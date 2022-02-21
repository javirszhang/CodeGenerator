using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace CodeGenerator.Core.MssqlProvider
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
            SqlConnection conn = new SqlConnection(this._connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = conn;
            if (paras != null && paras.Length > 0)
                cmd.Parameters.AddRange(paras);

            DataTable dt = new DataTable();
            conn.Open();
            using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                dt.Load(dr);
            }
            conn.Close();
            return dt;
        }
    }
}
