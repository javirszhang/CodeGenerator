using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerator.Core.Interfaces;
using System.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using CodeGenerator.Core.Common;

namespace CodeGenerator.Core.MySQLProvider
{
    public class MySQLDataFactory : DataFactory
    {
        public MySQLDataFactory(string connString) : base(connString)
        {
            ContainForeignTable = true;
        }

        public override DatabaseSchema GetDatabaseSchema()
        {
            DatabaseSchema db = new DatabaseSchema();
            db.Tables = new List<ITableSchema>();
            string sql = @"SELECT table_name,TABLE_COMMENT,IF(table_type='BASE TABLE','TABLE','VIEW') as object_type FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=@TARGET_SCHEMA";
            DbHelper helper = new DbHelper(this._connectionString);
            var data = helper.ListBySql(sql, new MySqlParameter("@TARGET_SCHEMA", this.DatabaseName));
            foreach (DataRow row in data.Rows)
            {
                MySQLTableSchema table = new MySQLTableSchema();
                table.Name = row["TABLE_NAME"] + string.Empty;
                table.Comment = row["TABLE_COMMENT"] + string.Empty;
                table.ObjectType = row["OBJECT_TYPE"] + string.Empty;
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
                        string[] array = this._connectionString.Trim().TrimEnd(';').Split(';');
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
        public override DataTable GetTableData(string table_name)
        {
            string sql = "SELECT * FROM " + table_name + " LIMIT 1000";
            DbHelper helper = new DbHelper(this._connectionString);
            return helper.ListBySql(sql);
        }
        private void SetPrimaryKey(MySQLTableSchema oracleTable)
        {
            string sql = @"select tc.table_name,tc.constraint_name,kc.column_name,tb.auto_increment 
from information_schema.table_constraints tc
left join information_schema.key_column_usage kc on tc.constraint_name = kc.constraint_name
and tc.table_schema = kc.table_schema and tc.table_name = kc.table_name
left join information_schema.tables tb on tb.table_name = tc.table_name and tb.table_schema = tc.table_schema
where	upper(tc.constraint_type) = 'PRIMARY KEY'  
and tc.table_name=@table_name and tc.table_schema=@table_schema";
            MySqlParameter para0 = new MySqlParameter("@table_name", oracleTable.Name);
            MySqlParameter para1 = new MySqlParameter("@table_schema", this.DatabaseName);
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para0, para1);
            Common.PrimaryKey key = new Common.PrimaryKey();
            key.Columns = new ColumnCollection();
            foreach (DataRow row in table.Rows)
            {
                string column_name = row.GetString("COLUMN_NAME");
                string constraint_name = row.GetString("CONSTRAINT_NAME");
                key.ConstraintName = constraint_name;
                IColumn pkCol = oracleTable.Columns.Find(it => it.Name == column_name);
                //pkCol.IsAutoIncrement = !string.IsNullOrEmpty(row["AUTO_INCREMENT"] + string.Empty);
                key.Columns.Add(pkCol);
            }
            oracleTable.PrimaryKey = key;
        }

        private void SetUniqueKey(MySQLTableSchema oracleTable)
        {
            string sql = @"select tc.table_name, tc.constraint_name,kc.column_name 
from information_schema.table_constraints tc,information_schema.key_column_usage kc 
where upper(tc.constraint_type)='UNIQUE' and tc.constraint_name=kc.constraint_name and tc.table_schema=kc.table_schema 
and tc.table_name=kc.table_name and tc.table_name=@table_name and tc.table_schema=@table_schema";
            MySqlParameter para0 = new MySqlParameter("@table_name", oracleTable.Name);
            MySqlParameter para1 = new MySqlParameter("@table_schema", this.DatabaseName);
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para0, para1);
            oracleTable.UniqueKeys = new List<Common.UniqueKey>();
            foreach (DataRow row in table.Rows)
            {
                string column_name = row.GetString("COLUMN_NAME");
                string constraint_name = row.GetString("CONSTRAINT_NAME");
                Common.UniqueKey key = oracleTable.UniqueKeys.Find(it => it.ConstraintName == constraint_name);
                if (key == null)
                {
                    key = new Common.UniqueKey();
                    key.Columns = new ColumnCollection();
                    key.ConstraintName = constraint_name;
                    oracleTable.UniqueKeys.Add(key);
                }
                key.Columns.Add(oracleTable.Columns.Find(it => it.Name == column_name));
            }

        }

        private void SetForeignKey(MySQLTableSchema mysqlTable)
        {
            string sql = @"select tc.table_name, tc.constraint_name,kc.column_name,kc.referenced_table_name,kc.referenced_table_schema,kc.referenced_column_name  
from information_schema.table_constraints tc,information_schema.key_column_usage kc 
where tc.constraint_type='FOREIGN KEY' and tc.constraint_name=kc.constraint_name 
and tc.table_schema=kc.table_schema and tc.table_name=kc.table_name
and tc.table_schema=@table_schema and tc.table_name=@table_name";
            MySqlParameter para0 = new MySqlParameter("@table_name", mysqlTable.Name);
            MySqlParameter para1 = new MySqlParameter("@table_schema", this.DatabaseName);
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para0, para1);
            foreach (DataRow row in table.Rows)
            {

                string column_name = row.GetString("COLUMN_NAME");
                var thisColumn = mysqlTable.Columns.Find(c => c.Name == column_name);
                string constraint_name = row.GetString("CONSTRAINT_NAME");
                ITableSchema foreignTable = null;
                IColumn foreignColumn = null;
                if (ContainForeignTable)
                {
                    string referenced_schema = row.GetString("referenced_table_schema");
                    string foreignTableName = row.GetString("referenced_table_name");
                    string referencedColumnName = row.GetString("referenced_column_name");
                    var fac = new MySQLDataFactory(this._connectionString);
                    fac.ContainForeignTable = false;
                    fac._db_name = referenced_schema;
                    foreignTable = fac.GetTableSchema(foreignTableName);
                    foreignColumn = foreignTable.Columns.Find(c => c.Name.Equals(referencedColumnName, StringComparison.OrdinalIgnoreCase));
                }
                var key = new ForeignKey(constraint_name, thisColumn, foreignTable, foreignColumn);
                mysqlTable.ForeignKeys.Add(key);
            }
        }

        private void SetColumns(MySQLTableSchema mysqlTable)
        {
            string sql = @"select column_name,data_type,
character_maximum_length as data_length,
numeric_precision as data_precision,
numeric_scale as data_scale,
is_nullable as nullable,
column_default as data_default,
column_comment as comments,
case when extra='auto_increment' then 1 else 0 end as auto_increment
from information_schema.columns where table_schema=@target_schema and table_name=@target_table order by ordinal_position";
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql,
                new MySqlParameter("@TARGET_SCHEMA", this.DatabaseName),
                new MySqlParameter("@TARGET_TABLE", mysqlTable.Name));
            mysqlTable.Columns = new ColumnCollection();
            foreach (DataRow row in table.Rows)
            {
                int scale = row.GetInt("DATA_SCALE");
                string data_type = row.GetString("DATA_TYPE");
                MySQLColumn column = new MySQLColumn(mysqlTable, this)
                {
                    Name = row.GetString("COLUMN_NAME"),
                    Comment = row.GetString("COMMENTS"),
                    CsharpType = MySQLUtils.TransformDatabaseType(data_type, scale),
                    DbType = data_type,
                    DefaultValue = row.GetString("DATA_DEFAULT"),
                    IsNullable = row.GetString("NULLABLE") != "NO",
                    Length = row.GetInt("DATA_LENGTH", "json".Equals(data_type, StringComparison.OrdinalIgnoreCase) ? 4000 : 0),
                    Scale = scale,
                    Table = mysqlTable,
                    IsNumeric = MySQLUtils.IsNumeric(data_type),
                    IsAutoIncrement = row.GetInt("auto_increment") == 1,
                };
                mysqlTable.Columns.Add(column);
            }
        }

        public override ITableSchema GetTableSchema(string table_name)
        {
            var db = GetDatabaseSchema();
            MySQLTableSchema mysqlTable = (MySQLTableSchema)db.Tables.Find(it => it.Name.Equals(table_name));
            SetColumns(mysqlTable);
            SetForeignKey(mysqlTable);
            SetUniqueKey(mysqlTable);
            SetPrimaryKey(mysqlTable);
            return mysqlTable;
        }
    }
}
