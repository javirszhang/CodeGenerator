using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerator.Core.Interfaces;
using System.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace CodeGenerator.Core.MySQLProvider
{
    public class MySQLDataFactory : DataFactory
    {
        public MySQLDataFactory(string connString) : base(connString)
        {
        }

        public override DatabaseSchema GetDatabaseSchema()
        {
            DatabaseSchema db = new DatabaseSchema();
            db.Tables = new List<ITableSchema>();
            string sql = @"SELECT TABLE_NAME,TABLE_COMMENT FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=@TARGET_SCHEMA";
            DbHelper helper = new DbHelper(this._connectionString);
            var data = helper.ListBySql(sql, new MySqlParameter("@TARGET_SCHEMA", this.DatabaseName));
            foreach (DataRow row in data.Rows)
            {
                MySQLTableSchema table = new MySQLTableSchema();
                table.Name = row["TABLE_NAME"] + string.Empty;
                table.Comment = row["TABLE_COMMENT"] + string.Empty;
                db.Tables.Add(table);
            }
            return db;
        }
        private string _db_name;
        private Dictionary<string, string> _connKV;
        public string DatabaseName
        {
            get
            {
                if (string.IsNullOrEmpty(_db_name))
                {
                    if (_connKV == null || _connKV.Count <= 0)
                    {
                        string[] array = this._connectionString.Split(';');
                        _connKV = new Dictionary<string, string>();
                        foreach (string item in array)
                        {
                            string[] kv = item.Split('=');
                            _connKV.Add(kv[0].Trim(), kv[1].Trim());
                        }
                    }
                    if (_connKV.ContainsKey("database"))
                    {
                        _db_name = _connKV["database"];
                    }
                }
                return _db_name;
            }
        }
        public override ITableSchema GetTableSchema(string table_name)
        {
            var db = GetDatabaseSchema();
            MySQLTableSchema oracleTable = (MySQLTableSchema)db.Tables.Find(it => it.Name.Equals(table_name));
            SetColumns(oracleTable);
            SetForeignKey(oracleTable);
            SetUniqueKey(oracleTable);
            SetPrimaryKey(oracleTable);
            return oracleTable;
        }

        private void SetPrimaryKey(MySQLTableSchema oracleTable)
        {
            string sql = @"select tc.table_name, tc.constraint_name,kc.column_name 
from information_schema.table_constraints tc,information_schema.key_column_usage kc 
where tc.constraint_type='PRIMARY KEY' and tc.constraint_name=kc.constraint_name and tc.table_schema=kc.table_schema and tc.table_name=kc.table_name and tc.table_name=@table_name and tc.table_schema=@table_schema";
            MySqlParameter para0 = new MySqlParameter("@table_name", oracleTable.Name);
            MySqlParameter para1 = new MySqlParameter("@table_schema", this.DatabaseName);
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para0, para1);
            Common.PrimaryKey key = new Common.PrimaryKey();
            key.Columns = new List<IColumn>();
            foreach (DataRow row in table.Rows)
            {
                string column_name = row["COLUMN_NAME"] + string.Empty;
                string constraint_name = row["CONSTRAINT_NAME"] + string.Empty;
                key.ConstraintName = constraint_name;
                key.Columns.Add(oracleTable.Columns.Find(it => it.Name == column_name));
            }
            oracleTable.PrimaryKey = key;
        }

        private void SetUniqueKey(MySQLTableSchema oracleTable)
        {
            string sql = @"select tc.table_name, tc.constraint_name,kc.column_name 
from information_schema.table_constraints tc,information_schema.key_column_usage kc 
where tc.constraint_type='UNIQUE' and tc.constraint_name=kc.constraint_name and tc.table_schema=kc.table_schema and tc.table_name=kc.table_name and tc.table_name=@table_name and tc.table_schema=@table_schema";
            MySqlParameter para0 = new MySqlParameter("@table_name", oracleTable.Name);
            MySqlParameter para1 = new MySqlParameter("@table_schema", this.DatabaseName);
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para0, para1);
            oracleTable.UniqueKeys = new List<Common.UniqueKey>();
            foreach (DataRow row in table.Rows)
            {
                Common.UniqueKey key = new Common.UniqueKey();
                key.Columns = new List<IColumn>();
                string column_name = row["COLUMN_NAME"] + string.Empty;
                string constraint_name = row["CONSTRAINT_NAME"] + string.Empty;
                key.ConstraintName = constraint_name;
                key.Columns.Add(oracleTable.Columns.Find(it => it.Name == column_name));
                oracleTable.UniqueKeys.Add(key);
            }

        }

        private void SetForeignKey(MySQLTableSchema oracleTable)
        {
            string sql = @"select tc.table_name, tc.constraint_name,kc.column_name 
from information_schema.table_constraints tc,information_schema.key_column_usage kc 
where tc.constraint_type='FOREIGN KEY' and tc.constraint_name=kc.constraint_name and tc.table_schema=kc.table_schema and tc.table_name=kc.table_name and tc.table_name=@table_name and tc.table_schema=@table_schema";
            MySqlParameter para0 = new MySqlParameter("@table_name", oracleTable.Name);
            MySqlParameter para1 = new MySqlParameter("@table_schema", this.DatabaseName);
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para0, para1);
            oracleTable.ForiegnKeys = new List<Common.ForeignKey>();
            foreach (DataRow row in table.Rows)
            {
                Common.ForeignKey key = new Common.ForeignKey();
                key.Columns = new List<IColumn>();
                string column_name = row["COLUMN_NAME"] + string.Empty;
                string constraint_name = row["CONSTRAINT_NAME"] + string.Empty;
                key.ConstraintName = constraint_name;
                key.Columns.Add(oracleTable.Columns.Find(it => it.Name == column_name));
                oracleTable.ForiegnKeys.Add(key);
            }
        }

        private void SetColumns(MySQLTableSchema oracleTable)
        {
            string sql = @"SELECT COLUMN_NAME,DATA_TYPE,
CHARACTER_MAXIMUM_LENGTH AS DATA_LENGTH,
NUMERIC_PRECISION AS DATA_PRECISION,
NUMERIC_SCALE AS DATA_SCALE,
IS_NULLABLE AS NULLABLE,
COLUMN_DEFAULT AS DATA_DEFAULT,
COLUMN_COMMENT AS COMMENTS 
FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=@TARGET_SCHEMA AND TABLE_NAME=@TARGET_TABLE";
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql,
                new MySqlParameter("@TARGET_SCHEMA", this.DatabaseName),
                new MySqlParameter("@TARGET_TABLE", oracleTable.Name));
            oracleTable.Columns = new List<IColumn>();
            foreach (DataRow row in table.Rows)
            {
                int scale = row.GetInt("DATA_SCALE"); //Convert.ToInt32(row["DATA_SCALE"]);
                string data_type = row["DATA_TYPE"] + string.Empty;
                MySQLColumn column = new MySQLColumn
                {
                    Name = row["COLUMN_NAME"] + string.Empty,
                    Comment = row["COMMENTS"] + string.Empty,
                    CsharpType = MySQLUtils.TransformDatabaseType(data_type, scale),
                    DbType = data_type,
                    DefaultValue = row["DATA_DEFAULT"] + string.Empty,
                    IsNullable = (row["NULLABLE"] + string.Empty) != "NO",
                    Length = row.GetInt("DATA_LENGTH"), //Convert.ToInt32(row["DATA_LENGTH"]+string.Empty),
                    Scale = scale,
                    Table = oracleTable,
                    IsNumeric = MySQLUtils.IsNumeric(data_type)
                };
                oracleTable.Columns.Add(column);
            }
        }
    }
}
