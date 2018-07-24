using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.MySQLProvider
{
    public class MySQLColumn : IColumn
    {
        public string Comment
        {
            get;
            set;
        }

        public Type CsharpType
        {
            get;
            set;
        }

        public string DbType
        {
            get;
            set;
        }

        public string DefaultValue
        {
            get;
            set;
        }

        public bool IsNullable
        {
            get;
            set;
        }

        public bool IsNumeric
        {
            get;
            set;
        }

        public int Length
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        string _PrimativeTypeName;
        public string PrimativeTypeName
        {
            get
            {
                if (this.CsharpType == typeof(string))
                    _PrimativeTypeName = "string";
                else if (this.CsharpType == typeof(decimal))
                    _PrimativeTypeName = "decimal";
                else if (this.CsharpType == typeof(int))
                    _PrimativeTypeName = "int";
                else if (this.CsharpType == typeof(object))
                    _PrimativeTypeName = "object";
                else if (this.CsharpType == typeof(long))
                    _PrimativeTypeName = "long";
                else
                    _PrimativeTypeName = this.CsharpType.Name;
                if (this.CsharpType.IsValueType && IsNullable)
                {
                    _PrimativeTypeName += "?";
                }
                return _PrimativeTypeName;
            }
            set
            {
                _PrimativeTypeName = value;
            }
        }
        /// <summary>
        /// 是否自动增长
        /// </summary>
        public bool IsAutoIncrement { get; set; }
        public int Scale
        {
            get;
            set;
        }

        public ITableSchema Table
        {
            get;
            set;
        }
    }
}
