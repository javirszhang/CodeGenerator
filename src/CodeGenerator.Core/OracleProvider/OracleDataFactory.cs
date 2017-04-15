using CodeGenerator.Core.Entities;
using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.OracleProvider
{
    public class OracleDataFactory : DataFactory
    {
        public OracleDataFactory(string connString) : base(connString)
        {

        }

        public override DatabaseSchema GetDatabaseSchema()
        {
            DatabaseSchema db = new DatabaseSchema();
            db.Tables = new List<ITableSchema>();
            string sql = @"SELECT TAB.TABLE_NAME,UC.COMMENTS FROM (
SELECT TABLE_NAME FROM USER_TABLES
UNION ALL
SELECT VIEW_NAME FROM USER_VIEWS) TAB LEFT JOIN USER_TAB_COMMENTS UC ON UC.TABLE_NAME=TAB.TABLE_NAME ORDER BY TAB.TABLE_NAME ASC";
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

        public override ITableSchema GetTableSchema(string table_name)
        {
            string sql = @"SELECT TAB.TABLE_NAME,UC.COMMENTS FROM (
SELECT TABLE_NAME FROM USER_TABLES
UNION ALL
SELECT VIEW_NAME FROM USER_VIEWS) TAB LEFT JOIN USER_TAB_COMMENTS UC ON UC.TABLE_NAME=TAB.TABLE_NAME WHERE TAB.TABLE_NAME=:TABLE_NAME ORDER BY TAB.TABLE_NAME ASC";
            DbHelper helper = new DbHelper(this._connectionString);
            var data = helper.ListBySql(sql, new OracleParameter("TABLE_NAME", table_name));

            OracleTableSchema oracleTable = new OracleTableSchema();
            oracleTable.Name = table_name;
            oracleTable.Comment = data.Rows[0]["COMMENTS"] + string.Empty;
            SetColumns(oracleTable);
            SetForeignKey(oracleTable);
            SetUniqueKey(oracleTable);
            SetPrimaryKey(oracleTable);
            return oracleTable;
        }
        private void SetColumns(OracleTableSchema oracleTable)
        {
            if (oracleTable == null)
            {
                return;
            }
            oracleTable.Columns = new List<IColumn>();
            string sql = @"SELECT TC.COLUMN_NAME,
       TC.DATA_TYPE,
       TC.DATA_LENGTH,
       TC.DATA_PRECISION,
       NVL(TC.DATA_SCALE, -1) DATA_SCALE,
       TC.NULLABLE,
       TC.DATA_DEFAULT,
       CC.COMMENTS
  FROM USER_TAB_COLUMNS TC
  LEFT JOIN USER_COL_COMMENTS CC
    ON TC.COLUMN_NAME = CC.COLUMN_NAME
   AND TC.TABLE_NAME = CC.TABLE_NAME
   WHERE TC.TABLE_NAME=:TABLE_NAME";
            List<IColumn> columns = new List<IColumn>();
            OracleParameter para = new OracleParameter("TABLE_NAME", oracleTable.Name);
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para);
            foreach (DataRow row in table.Rows)
            {
                int scale = Convert.ToInt32(row["DATA_SCALE"]);
                string data_type = row["DATA_TYPE"] + string.Empty;
                OracleColumn column = new OracleColumn
                {
                    Name = row["COLUMN_NAME"] + string.Empty,
                    Comment = row["COMMENTS"] + string.Empty,
                    CsharpType = OracleUtils.TransformDatabaseType(data_type, scale),
                    DbType = data_type,
                    DefaultValue = row["DATA_DEFAULT"] + string.Empty,
                    IsNullable = (row["NULLABLE"] + string.Empty) == "Y",
                    Length = Convert.ToInt32(row["DATA_LENGTH"]),
                    Scale = scale,
                    Table = oracleTable,
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
            string sql = @"SELECT UCC.CONSTRAINT_NAME, UCC.COLUMN_NAME
  FROM USER_CONSTRAINTS UC
  LEFT JOIN USER_CONS_COLUMNS UCC
    ON UC.CONSTRAINT_NAME = UCC.CONSTRAINT_NAME
 WHERE UC.CONSTRAINT_TYPE = 'R'
   AND UC.TABLE_NAME =:TABLE_NAME";
            OracleParameter para = new OracleParameter("TABLE_NAME", oracleTable.Name.ToUpper());
            DbHelper helper = new DbHelper(this._connectionString);
            var table = helper.ListBySql(sql, para);

            List<Common.ForeignKey> foreignes = new List<Common.ForeignKey>();

            foreach (DataRow row in table.Rows)
            {
                string column_name = row["COLUMN_NAME"] + string.Empty;
                string constraint_name = row["CONSTRAINT_NAME"] + string.Empty;
                Common.ForeignKey key = foreignes.Find(it => it.ConstraintName == constraint_name);
                if (key == null)
                {
                    key = new Common.ForeignKey();
                    key.Columns = new List<IColumn>();
                    key.ConstraintName = constraint_name;
                    foreignes.Add(key);
                }
                key.Columns.Add(oracleTable.Columns.Find(it => it.Name == column_name));
            }
            oracleTable.ForiegnKeys = foreignes;
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
                    key.Columns = new List<IColumn>();
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
