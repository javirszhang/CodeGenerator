using CodeGenerator.Core.Common;
using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;

namespace CodeGenerator.Core.OracleProvider
{
    public class OracleDataFactory : DataFactory
    {
        public bool ContainForeignTable
        {
            get; set;
        }
        public OracleDataFactory(string connString) : base(connString)
        {
            ContainForeignTable = true;
        }

        public override DatabaseSchema GetDatabaseSchema()
        {
            DatabaseSchema db = new DatabaseSchema();
            db.Tables = new List<ITableSchema>();
            string sql = @"SELECT UO.OBJECT_NAME AS TABLE_NAME,UO.OBJECT_TYPE,UC.COMMENTS,UV.TEXT FROM USER_OBJECTS UO 
LEFT JOIN USER_TAB_COMMENTS UC ON UC.TABLE_NAME=UO.OBJECT_NAME 
LEFT JOIN USER_VIEWS UV ON UV.VIEW_NAME=UO.OBJECT_NAME
WHERE UO.OBJECT_TYPE IN ('VIEW','TABLE') ORDER BY UO.OBJECT_NAME ASC";
            DbHelper helper = new DbHelper(this._connectionString);
            var data = helper.ListBySql(sql, null);
            foreach (DataRow row in data.Rows)
            {
                OracleTableSchema table = new OracleTableSchema();
                table.Name = row["TABLE_NAME"] + string.Empty;
                table.Comment = row["COMMENTS"] + string.Empty;
                db.Tables.Add(table);
            }
            return db;
        }

        public override DataTable GetTableData(string table_name)
        {
            string sql = "SELECT * FROM " + table_name;
            DbHelper helper = new DbHelper(this._connectionString);
            return helper.ListBySql(sql);
        }
        private void SetColumns(OracleTableSchema oracleTable)
        {
            if (oracleTable == null)
            {
                return;
            }
            oracleTable.Columns = new ColumnCollection();
            string sql = @"SELECT TC.COLUMN_NAME,
       TC.DATA_TYPE,
       NVL(DECODE(TC.CHAR_LENGTH,0,TC.DATA_PRECISION,TC.CHAR_LENGTH),TC.DATA_LENGTH) AS DATA_LENGTH,
       TC.DATA_PRECISION,
       NVL(TC.DATA_SCALE, -1) DATA_SCALE,
       TC.NULLABLE,
       TC.DATA_DEFAULT,
       CC.COMMENTS
  FROM USER_TAB_COLUMNS TC
  LEFT JOIN USER_COL_COMMENTS CC
    ON TC.COLUMN_NAME = CC.COLUMN_NAME
   AND TC.TABLE_NAME = CC.TABLE_NAME
   WHERE TC.TABLE_NAME=:TABLE_NAME ORDER BY TC.COLUMN_ID ASC";
            List<IColumn> columns = new List<IColumn>();
            OracleParameter para = new OracleParameter("TABLE_NAME", oracleTable.Name);
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para);
            foreach (DataRow row in table.Rows)
            {
                int scale = Convert.ToInt32(row["DATA_SCALE"]);
                string data_type = row["DATA_TYPE"] + string.Empty;
                OracleColumn column = new OracleColumn(oracleTable, this)
                {
                    Name = row["COLUMN_NAME"] + string.Empty,
                    Comment = row["COMMENTS"] + string.Empty,
                    CsharpType = OracleUtils.TransformDatabaseType(data_type, scale),
                    DbType = data_type,
                    DefaultValue = (row["DATA_DEFAULT"] + string.Empty).Trim('\r', '\n'),
                    IsNullable = (row["NULLABLE"] + string.Empty) == "Y",
                    Length = Convert.ToInt32(row["DATA_LENGTH"]),
                    Scale = scale,
                    Table = oracleTable,
                    IsNumeric = OracleUtils.IsNumeric(data_type)
                };
                oracleTable.Columns.Add(column);
            }
        }
        private void SetForeignKey(OracleTableSchema oracleTable)
        {
            if (oracleTable == null)
            {
                return;
            }
            if (oracleTable.Columns == null || oracleTable.Columns.Count <= 0)
            {
                SetColumns(oracleTable);
            }
            string sql = @"SELECT UCC.CONSTRAINT_NAME, UCC.COLUMN_NAME,UC1.TABLE_NAME FOREIGN_TABLE_NAME
  FROM USER_CONSTRAINTS UC
  LEFT JOIN USER_CONS_COLUMNS UCC
    ON UC.CONSTRAINT_NAME = UCC.CONSTRAINT_NAME
  LEFT JOIN USER_CONSTRAINTS UC1 ON UC.R_CONSTRAINT_NAME=UC1.CONSTRAINT_NAME
 WHERE UC.CONSTRAINT_TYPE = 'R'
   AND UC.TABLE_NAME =:TABLE_NAME";
            OracleParameter para = new OracleParameter("TABLE_NAME", oracleTable.Name.ToUpper());
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para);

            foreach (DataRow row in table.Rows)
            {
                string column_name = row["COLUMN_NAME"] + string.Empty;
                var thisColumn = oracleTable.Columns.Find(c => c.Name == column_name);
                string constraint_name = row["CONSTRAINT_NAME"] + string.Empty;
                ITableSchema foreignTable = null;
                IColumn foreignColumn = null;
                if (ContainForeignTable)
                {
                    string foreignTableName = row["FOREIGN_TABLE_NAME"] + string.Empty;
                    var fac = new OracleDataFactory(this._connectionString);
                    fac.ContainForeignTable = false;
                    foreignTable = fac.GetTableSchema(foreignTableName);
                    foreignColumn = foreignTable.PrimaryKey.Columns.FirstOrDefault();
                }
                var key = new ForeignKey(constraint_name, thisColumn, foreignTable, foreignColumn);
                oracleTable.ForeignKeys.Add(key);
            }
        }
        private void SetUniqueKey(OracleTableSchema oracleTable)
        {
            if (oracleTable == null)
            {
                return;
            }
            if (oracleTable.Columns == null || oracleTable.Columns.Count <= 0)
            {
                SetColumns(oracleTable);
            }
            string sql = @"SELECT UCC.CONSTRAINT_NAME, UCC.COLUMN_NAME
  FROM USER_CONSTRAINTS UC
  LEFT JOIN USER_CONS_COLUMNS UCC
    ON UC.CONSTRAINT_NAME = UCC.CONSTRAINT_NAME
 WHERE UC.CONSTRAINT_TYPE = 'U'
   AND UC.TABLE_NAME =:TABLE_NAME";
            OracleParameter para = new OracleParameter("TABLE_NAME", oracleTable.Name.ToUpper());
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
                    key.Columns = new ColumnCollection();
                    key.ConstraintName = constraint_name;
                    uniques.Add(key);
                }
                key.Columns.Add(oracleTable.Columns.Find(it => it.Name == column_name));
            }
            oracleTable.UniqueKeys = uniques;
        }
        private void SetPrimaryKey(OracleTableSchema oracleTable)
        {
            if (oracleTable == null)
            {
                return;
            }
            if (oracleTable.Columns == null || oracleTable.Columns.Count <= 0)
            {
                SetColumns(oracleTable);
            }
            string sql = @"SELECT UCC.CONSTRAINT_NAME, UCC.COLUMN_NAME
  FROM USER_CONSTRAINTS UC
  LEFT JOIN USER_CONS_COLUMNS UCC
    ON UC.CONSTRAINT_NAME = UCC.CONSTRAINT_NAME
 WHERE UC.CONSTRAINT_TYPE = 'P'
   AND UC.TABLE_NAME =:TABLE_NAME";
            OracleParameter para = new OracleParameter("TABLE_NAME", oracleTable.Name.ToUpper());
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para);
            PrimaryKey key = new PrimaryKey();
            foreach (DataRow row in table.Rows)
            {
                string column_name = row["COLUMN_NAME"] + string.Empty;
                string constraint_name = row["CONSTRAINT_NAME"] + string.Empty;
                key.ConstraintName = constraint_name;
                key.Columns.Add(oracleTable.Columns.Find(it => it.Name == column_name));
            }
            oracleTable.PrimaryKey = key;
        }

        public override ITableSchema GetTableSchema(string table_name)
        {
            string sql = @"SELECT UO.OBJECT_NAME AS TABLE_NAME,UO.OBJECT_TYPE,UC.COMMENTS,UV.TEXT FROM USER_OBJECTS UO 
LEFT JOIN USER_TAB_COMMENTS UC ON UC.TABLE_NAME=UO.OBJECT_NAME 
LEFT JOIN USER_VIEWS UV ON UV.VIEW_NAME=UO.OBJECT_NAME
WHERE UO.OBJECT_TYPE IN ('VIEW','TABLE') AND UO.OBJECT_NAME=:TABLE_NAME ORDER BY UO.OBJECT_NAME ASC";
            DbHelper helper = new DbHelper(this._connectionString);
            //Console.WriteLine(this._connectionString);
            var data = helper.ListBySql(sql, new OracleParameter("TABLE_NAME", table_name.ToUpper()));
            //Console.WriteLine("查询数据条数：" + data.Rows.Count);
            string objectType = data.Rows[0]["OBJECT_TYPE"] + string.Empty;
            OracleTableSchema oracleTable = new OracleTableSchema();
            oracleTable.Name = table_name;
            oracleTable.Comment = data.Rows[0]["COMMENTS"] + string.Empty;
            oracleTable.ObjectType = objectType;
            if (objectType == "VIEW")
            {
                oracleTable.ViewScript = data.Rows[0]["TEXT"].ToString();
            }
            SetColumns(oracleTable);
            SetPrimaryKey(oracleTable);
            SetForeignKey(oracleTable);
            SetUniqueKey(oracleTable);
            return oracleTable;
        }
    }
}
