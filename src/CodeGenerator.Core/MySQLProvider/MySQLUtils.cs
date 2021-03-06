﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.MySQLProvider
{
    internal class MySQLUtils
    {
        public static Type TransformDatabaseType(string dbtype, int scale)
        {
            Type csharpType = null;
            switch (dbtype.ToUpper())
            {
                case "DECIMAL":
                case "FLOAT":
                case "DOUBLE":
                case "REAL":
                case "NUMBER":
                case "NUMERIC":
                case "INT":
                case "INTEGER":
                case "SMALLINT":
                case "TINYINT":
                case "MEDIUMINT":
                    csharpType = scale != 0 ? typeof(decimal) : typeof(int);
                    break;
                case "BIGINT":
                    csharpType = typeof(long);
                    break;
                case "BIT":
                    csharpType = typeof(bool);
                    break;
                case "NVARCHAR":
                case "VARCHAR":
                case "CHAR":
                case "NVARCHAR2":
                case "VARCHAR2":
                case "NCHAR":
                case "CLOB":
                case "NCLOB":
                case "LONG":
                case "TEXT":
                case "TINYTEXT":
                case "MEDIUMTEXT":
                case "BIGTEXT":
                case "JSON":
                    csharpType = typeof(string); break;
                case "DATE":
                case "DATETIME":
                    csharpType = typeof(DateTime); break;
                case "BLOB":
                case "RAW":
                case "LONG RAW":
                case "BFILE":
                default:
                    csharpType = typeof(object); break;
            }
            return csharpType;
        }

        public static bool IsNumeric(string dbtype)
        {
            string[] numericTypes = new string[] { "DECIMAL", "INTEGER", "FLOAT", "REAL", "NUMBER", "INT", "BIGINT", "DOUBLE", "SMALLINT", "INTEGER", "MEDIUMINT", "NUMERIC", "TINYINT" };
            return numericTypes.Contains(dbtype.ToUpper());
        }
    }
}
