﻿using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.MssqlProvider
{
    public class SqlServerDataFactory : DataFactory
    {
        public bool ContainForeignTable
        {
            get; set;
        }
        public SqlServerDataFactory(string connString) : base(connString)
        {
            this.ContainForeignTable = true;
        }

        public override DatabaseSchema GetDatabaseSchema()
        {
            string sql = @"SELECT tbs.name ,ds.value as comments      
FROM sysobjects tbs 
LEFT JOIN sys.extended_properties ds ON ds.major_id=tbs.id
where ds.minor_id=0 and tbs.xtype in ('U','V')";
            DatabaseSchema schema = new DatabaseSchema();
            schema.Tables = new List<ITableSchema>();
            DbHelper helper = new DbHelper(this._connectionString);
            var dt = helper.ListBySql(sql);
            foreach (DataRow row in dt.Rows)
            {
                var table = new SqlServerTableSchema
                {
                    Name = row["name"] + string.Empty,
                    Comment = row["comments"] + string.Empty
                };
                schema.Tables.Add(table);
            };
            return schema;
        }

        public override DataTable GetTableData(string table_name)
        {
            string sql = "SELECT * FROM " + table_name;
            DbHelper helper = new DbHelper(this._connectionString);
            return helper.ListBySql(sql);
        }

        public override ITableSchema GetTableSchema(string table_name)
        {
            string sql = @"SELECT tbs.name ,ds.value as comments ,tbs.xtype as  OBJECT_TYPE ,null as TEXT   
FROM sysobjects tbs 
LEFT JOIN sys.extended_properties ds ON ds.major_id=tbs.id
where ds.minor_id=0 and tbs.xtype in ('U','V') and tbs.name=@TABLE_NAME";
            DbHelper helper = new DbHelper(this._connectionString);
            var data = helper.ListBySql(sql, new SqlParameter("TABLE_NAME", table_name));
            string objectType = data.Rows[0]["OBJECT_TYPE"] + string.Empty;
            SqlServerTableSchema oracleTable = new SqlServerTableSchema();
            oracleTable.Name = table_name;
            oracleTable.Comment = data.Rows[0]["COMMENTS"] + string.Empty;
            oracleTable.ObjectType = objectType;
            if (objectType == "V")
            {
                oracleTable.ViewScript = data.Rows[0]["TEXT"].ToString();
            }
            SetColumns(oracleTable);
            SetForeignKey(oracleTable);
            SetUniqueKey(oracleTable);
            SetPrimaryKey(oracleTable);
            return oracleTable;
        }

        private void SetColumns(SqlServerTableSchema oracleTable)
        {
            if (oracleTable == null)
            {
                return;
            }
            oracleTable.Columns = new List<IColumn>();
            string sql = @"SELECT 
a.colorder COLUMN_ID,a.name COLUMN_NAME,
(case when COLUMNPROPERTY( a.id,a.name,'IsIdentity')=1 then 1 else 0 end) AUTOINCREMENT,
(case when (SELECT count(*) FROM sysobjects
WHERE (name in (SELECT name FROM sysindexes
WHERE (id = a.id) AND (indid in
(SELECT indid FROM sysindexkeys
WHERE (id = a.id) AND (colid in
(SELECT colid FROM syscolumns WHERE (id = a.id) AND (name = a.name)))))))
AND (xtype = 'PK'))>0 then 1 else 0 end) PK,b.name DATA_TYPE,a.length BYTE_LEN,
COLUMNPROPERTY(a.id,a.name,'PRECISION') as DATA_LENGTH,
isnull(COLUMNPROPERTY(a.id,a.name,'Scale'),0) as DATA_SCALE,(case when a.isnullable=1 then 1 else 0 end) NULLABLE,
isnull(e.text,'') DATA_DEFAULT,isnull(g.[value], ' ') AS  COMMENTS
FROM syscolumns a
left join systypes b on a.xtype=b.xusertype
inner join sysobjects d on a.id=d.id and d.xtype='U' and d.name<>'dtproperties'
left join syscomments e on a.cdefault=e.id
left join sys.extended_properties g on a.id=g.major_id AND a.colid=g.minor_id
left join sys.extended_properties f on d.id=f.class and f.minor_id=0
where b.name is not null
and d.name=@table_name 
order by a.id,a.colorder";
            List<IColumn> columns = new List<IColumn>();
            var para = new SqlParameter("table_name", oracleTable.Name);
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para);
            foreach (DataRow row in table.Rows)
            {
                int scale = Convert.ToInt32(row["DATA_SCALE"]);
                string data_type = row["DATA_TYPE"] + string.Empty;
                int len = Convert.ToInt32(row["DATA_LENGTH"]);
                var column = new SqlServerColumn
                {
                    Name = row["COLUMN_NAME"] + string.Empty,
                    Comment = row["COMMENTS"] + string.Empty,
                    CsharpType = SqlServerUtils.TransformDatabaseType(data_type, len, scale),
                    DbType = data_type,
                    DefaultValue = (row["DATA_DEFAULT"] + string.Empty).Trim('\r', '\n'),
                    IsNullable = (row["NULLABLE"] + string.Empty) == "Y",
                    Length = len,
                    Scale = scale,
                    Table = oracleTable,
                    IsAutoIncrement = Convert.ToInt32(row["AUTOINCREMENT"]) == 1,
                    IsNumeric = SqlServerUtils.IsNumeric(data_type)
                };
                oracleTable.Columns.Add(column);
            }
        }
        private void SetForeignKey(SqlServerTableSchema oracleTable)
        {
            if (oracleTable == null)
            {
                return;
            }
            if (oracleTable.Columns == null || oracleTable.Columns.Count <= 0)
            {
                SetColumns(oracleTable);
            }
            string sql = @"select obj.name CONSTRAINT_NAME,
main_col.name COLUMN_NAME,
sub.name FOREIGN_TABLE_NAME 
from sysforeignkeys fk left join sys.tables main on fk.fkeyid=main.object_id
left join sys.tables sub on sub.object_id=fk.rkeyid
left join syscolumns main_col on fk.fkey=main_col.colid and fk.fkeyid=main_col.id
left join sysobjects obj on obj.id=fk.constid
where main.name=@TABLE_NAME";
            var para = new SqlParameter("TABLE_NAME", oracleTable.Name.ToUpper());
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para);

            oracleTable.ForiegnKeys = new List<Common.ForeignKey>();

            foreach (DataRow row in table.Rows)
            {
                string column_name = row["COLUMN_NAME"] + string.Empty;
                string constraint_name = row["CONSTRAINT_NAME"] + string.Empty;

                Common.ForeignKey key = new Common.ForeignKey();
                key.Columns = new List<IColumn>();
                key.ConstraintName = constraint_name;
                if (key.ForeignTable == null && ContainForeignTable)
                {
                    string forignTable = row["FOREIGN_TABLE_NAME"] + string.Empty;
                    var fac = new SqlServerDataFactory(this._connectionString);
                    fac.ContainForeignTable = false;
                    key.ForeignTable = fac.GetTableSchema(forignTable);
                }
                key.Columns.Add(oracleTable.Columns.Find(it => it.Name == column_name));
                oracleTable.ForiegnKeys.Add(key);
            }
        }
        private void SetUniqueKey(SqlServerTableSchema oracleTable)
        {
            if (oracleTable == null)
            {
                return;
            }
            if (oracleTable.Columns == null || oracleTable.Columns.Count <= 0)
            {
                SetColumns(oracleTable);
            }
            string sql = @"SELECT  IDX.NAME AS CONSTRAINT_NAME,  COL.NAME AS COLUMN_NAME
FROM      SYS.INDEXES IDX JOIN
                SYS.INDEX_COLUMNS IDXCOL ON (IDX.OBJECT_ID = IDXCOL.OBJECT_ID AND IDX.INDEX_ID = IDXCOL.INDEX_ID AND
                IDX.IS_UNIQUE_CONSTRAINT = 1) JOIN
                SYS.TABLES TAB ON (IDX.OBJECT_ID = TAB.OBJECT_ID) JOIN
                SYS.COLUMNS COL ON (IDX.OBJECT_ID = COL.OBJECT_ID AND IDXCOL.COLUMN_ID = COL.COLUMN_ID) where tab.name=@TABLE_NAME";
            var para = new SqlParameter("TABLE_NAME", oracleTable.Name.ToUpper());
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para);

            List<Common.UniqueKey> uniques = new List<Common.UniqueKey>();

            foreach (DataRow row in table.Rows)
            {
                string column_name = row["COLUMN_NAME"] + string.Empty;
                string constraint_name = row["CONSTRAINT_NAME"] + string.Empty;
                Common.UniqueKey key = uniques.Find(it => it.ConstraintName == constraint_name);
                if (key == null)
                {
                    key = new Common.UniqueKey();
                    key.Columns = new List<IColumn>();
                    key.ConstraintName = constraint_name;
                    uniques.Add(key);
                }
                key.Columns.Add(oracleTable.Columns.Find(it => it.Name == column_name));
            }
            oracleTable.UniqueKeys = uniques;
        }
        private void SetPrimaryKey(SqlServerTableSchema oracleTable)
        {
            if (oracleTable == null)
            {
                return;
            }
            if (oracleTable.Columns == null || oracleTable.Columns.Count <= 0)
            {
                SetColumns(oracleTable);
            }
            string sql = @"SELECT    IDX.NAME AS CONSTRAINT_NAME,
                 COL.NAME AS COLUMN_NAME
FROM SYS.INDEXES IDX JOIN
                SYS.INDEX_COLUMNS IDXCOL ON (IDX.OBJECT_ID = IDXCOL.OBJECT_ID AND IDX.INDEX_ID = IDXCOL.INDEX_ID AND
                IDX.IS_PRIMARY_KEY = 1) JOIN
                SYS.TABLES TAB ON (IDX.OBJECT_ID = TAB.OBJECT_ID) JOIN
                SYS.COLUMNS COL ON (IDX.OBJECT_ID = COL.OBJECT_ID AND IDXCOL.COLUMN_ID = COL.COLUMN_ID) where tab.name=@TABLE_NAME";
            var para = new SqlParameter("TABLE_NAME", oracleTable.Name.ToUpper());
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para);
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
    }
}
