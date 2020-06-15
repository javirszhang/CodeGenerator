using CodeGenerator.Core.Entities;
using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core
{
    public class DatabaseResolver
    {
        public static DataFactory GetDataFactory(ConnectionSetting setting)
        {
            DataFactory factory = null;
            switch (setting.Provider)
            {
                case DatabaseType.Oracle:
                    factory = new OracleProvider.OracleDataFactory(setting.ConnectionString);
                    break;
                case DatabaseType.Mssql:
                    factory = new MssqlProvider.SqlServerDataFactory(setting.ConnectionString);
                    break;
                case DatabaseType.MySQL:
                    factory = new MySQLProvider.MySQLDataFactory(setting.ConnectionString);
                    break;
                default:
                    break;
            }
            return factory;
        }
        public static DbConnection CreateConnection(ConnectionSetting setting)
        {
            DbConnection conn = null;
            switch (setting.Provider)
            {
                case DatabaseType.MySQL:
                    conn = new MySql.Data.MySqlClient.MySqlConnection(setting.ConnectionString);
                    break;
                case DatabaseType.Mssql:
                    conn = new SqlConnection(setting.ConnectionString); break;
                case DatabaseType.Oracle:
                default:
                    conn = new OracleConnection(setting.ConnectionString); break;
            }
            return conn;
        }
        public static bool TestConnection(ConnectionSetting setting)
        {
            try
            {
                DbConnection conn = CreateConnection(setting);
                conn.Open();
                conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
