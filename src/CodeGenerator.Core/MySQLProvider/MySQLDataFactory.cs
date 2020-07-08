﻿using System;
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
        /// <summary>
        /// 读取外键key时是否加载外键引用表结构
        /// </summary>
        public bool ContainForeignTable { get; set; }
        public MySQLDataFactory(string connString) : base(connString)
        {
            ContainForeignTable = true;
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
        public override DataTable GetTableData(string table_name)
        {
            string sql = "SELECT * FROM " + table_name;
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
            key.Columns = new List<IColumn>();
            foreach (DataRow row in table.Rows)
            {
                string column_name = row["COLUMN_NAME"] + string.Empty;
                string constraint_name = row["CONSTRAINT_NAME"] + string.Empty;
                key.ConstraintName = constraint_name;
                IColumn pkCol = oracleTable.Columns.Find(it => it.Name == column_name);
                pkCol.IsAutoIncrement = !string.IsNullOrEmpty(row["AUTO_INCREMENT"] + string.Empty);
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
                string column_name = row["COLUMN_NAME"] + string.Empty;
                string constraint_name = row["CONSTRAINT_NAME"] + string.Empty;
                Common.UniqueKey key = oracleTable.UniqueKeys.Find(it => it.ConstraintName == constraint_name);
                if (key == null)
                {
                    key = new Common.UniqueKey();
                    key.Columns = new List<IColumn>();
                    key.ConstraintName = constraint_name;
                    oracleTable.UniqueKeys.Add(key);
                }
                key.Columns.Add(oracleTable.Columns.Find(it => it.Name == column_name));
            }

        }

        private void SetForeignKey(MySQLTableSchema oracleTable)
        {
            string sql = @"select tc.table_name, tc.constraint_name,kc.column_name,kc.referenced_table_name,kc.referenced_table_schema  
from information_schema.table_constraints tc,information_schema.key_column_usage kc 
where tc.constraint_type='FOREIGN KEY' and tc.constraint_name=kc.constraint_name 
and tc.table_schema=kc.table_schema and tc.table_name=kc.table_name
and tc.table_name=@table_name and tc.table_schema=@table_schema";
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
                if (ContainForeignTable && key.ForeignTable == null)
                {
                    string foreignTable = row["referenced_table_name"] + string.Empty;
                    string referenced_schema = row["referenced_table_schema"] + string.Empty;
                    var fac = new MySQLDataFactory(this._connectionString);
                    fac.ContainForeignTable = false;
                    fac._db_name = referenced_schema;
                    key.ForeignTable = fac.GetTableSchema(foreignTable);
                }
                oracleTable.ForiegnKeys.Add(key);
            }
        }

        private void SetColumns(MySQLTableSchema oracleTable)
        {
            string sql = @"select column_name,data_type,
character_maximum_length as data_length,
numeric_precision as data_precision,
numeric_scale as data_scale,
is_nullable as nullable,
column_default as data_default,
column_comment as comments 
from information_schema.columns where table_schema=@target_schema and table_name=@target_table";
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql,
                new MySqlParameter("@TARGET_SCHEMA", this.DatabaseName),
                new MySqlParameter("@TARGET_TABLE", oracleTable.Name));
            oracleTable.Columns = new List<IColumn>();
            foreach (DataRow row in table.Rows)
            {
                int scale = row.GetInt("DATA_SCALE");
                string data_type = row["DATA_TYPE"] + string.Empty;
                MySQLColumn column = new MySQLColumn
                {
                    Name = row["COLUMN_NAME"] + string.Empty,
                    Comment = row["COMMENTS"] + string.Empty,
                    CsharpType = MySQLUtils.TransformDatabaseType(data_type, scale),
                    DbType = data_type,
                    DefaultValue = row["DATA_DEFAULT"] + string.Empty,
                    IsNullable = (row["NULLABLE"] + string.Empty) != "NO",
                    Length = row.GetInt("DATA_LENGTH"),
                    Scale = scale,
                    Table = oracleTable,
                    IsNumeric = MySQLUtils.IsNumeric(data_type)
                };
                oracleTable.Columns.Add(column);
            }
        }
    }
}
