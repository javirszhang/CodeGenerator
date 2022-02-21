using System.Data;
using System.Data.Common;
using System.Data.OracleClient;

namespace CodeGenerator.Core.OracleProvider
{
    public class DbHelper
    {
        private string _connectionString;
        public DbHelper(string connectionString)
        {
            this._connectionString = connectionString;
        }
        public DataTable ListBySql(string sql,params DbParameter[] paras)
        {
            OracleConnection conn = new OracleConnection(this._connectionString);
            OracleCommand cmd = new OracleCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = conn;
            if (paras != null && paras.Length > 0)
                cmd.Parameters.AddRange(paras);

            DataTable dt = new DataTable();
            conn.Open();
            //DataSet ds = new DataSet();
            //OracleDataAdapter adapter = new OracleDataAdapter(cmd);
            //adapter.Fill(ds);
            //dt = ds.Tables[0];
            using (OracleDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                dt.Load(dr);
            }
            conn.Close();
            return dt;
        }
    }
}
