using CodeGenerator.Core.Common;
using CodeGenerator.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeGenerator.Core.Interfaces
{
    public abstract class BaseColumn : IColumn
    {
        private readonly DataFactory _factory;
        public BaseColumn(ITableSchema table, DataFactory factory)
        {
            this.Table = table;
            this._factory = factory;
        }
        private string _comment;
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; AnalysisComment(value); }
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
                else if (this.CsharpType == typeof(bool))
                    _PrimativeTypeName = "bool";
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
        public string GetCamelCaseName()
        {
            var util = new xUtils();
            return util.ToPascalCase(this.Name, false);
        }
        public string GetCamelCaseName(bool smallCamel)
        {
            var util = new xUtils();
            var name = util.ToPascalCase(this.Name, false);
            return smallCamel ? name.Substring(0, 1).ToLower() + name.Substring(1) : name;
        }
        public string GetBigCamelName() => GetCamelCaseName();
        public string GetSmallCamelName() => GetCamelCaseName(true);
        public string GetPropertyTypeName()
        {
            return GetPropertyTypeName(false);
        }
        public string GetPropertyTypeName(bool nullableReference)
        {
            string typeName = ResolvePropertyTypeFromComment();
            if (!nullableReference || !this.IsNullable || (nullableReference && typeName.EndsWith("?")))
            {
                return typeName;
            }
            return typeName + "?";
        }
        private string MatchValue(string input, string pattern, int groupindex)
        {
            Regex regex = new Regex(pattern);
            var match = regex.Match(input);
            if (groupindex > match.Groups.Count - 1)
            {
                groupindex = 0;
            }
            string value = match.Groups[groupindex].Value;
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
        public string ResolvePropertyTypeFromComment()
        {
            string matchName = MatchValue(this.Comment, "\\$(.+?)\\$", 1);
            if (!string.IsNullOrEmpty(matchName) && this.IsNullable)
            {
                matchName += "?";
            }
            return matchName ?? this.PrimativeTypeName;
        }
        public override string ToString()
        {
            var util = new xUtils();
            return $"{GetPropertyTypeName()} {util.ToPascalCase(this.Name, false)}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format">null,field,parameter</param>
        /// <returns></returns>
        public string ToString(string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                return ToString();
            }
            string[] array = format.Split(':');
            string scene = array[0];
            bool nullable = array.Length > 1 && array[1] == "nullable" && this.IsNullable;

            if (scene == "field")
            {
                return $"{GetPropertyTypeName(nullable)} _{GetCamelCaseName(true)}";
            }
            else if (scene == "parameter")
            {
                return $"{GetPropertyTypeName(nullable)} {GetCamelCaseName(true)}";
            }
            return ToString();
        }
        public bool IsCreateTime()
        {
            return "create_time".Equals(this.Name, StringComparison.OrdinalIgnoreCase) ||
                "createtime".Equals(this.Name, StringComparison.OrdinalIgnoreCase) ||
                "creationtime".Equals(this.Name, StringComparison.OrdinalIgnoreCase) ||
                "creation_time".Equals(this.Name, StringComparison.OrdinalIgnoreCase);

        }
        public virtual string GetDefaultValue()
        {
            if (this.Table.PrimaryKey.Columns.Contains(this) && !this.IsAutoIncrement && this.IsNumeric)
            {
                return "Snowflake.Next()";
            }
            string result = ResolveDefaultValue();
            if (IsEnumField() && result != "0")
            {
                return Regex.IsMatch(result, "^\\d+$") ?
                    $"({ResolvePropertyTypeFromComment()}){result}" :
                    $"Enum.Parse<{ResolvePropertyTypeFromComment()}>({result})";
            }
            return result;
        }
        public bool IsEnumField()
        {
            return Regex.IsMatch(this.Comment, "\\$(.+?)\\$");
        }
        private string ResolveDefaultValue()
        {
            if (string.IsNullOrEmpty(DefaultValue))
            {
                if (this.IsNullable)
                {
                    return "null";
                }
                if (this.CsharpType == typeof(string))
                {
                    return "null";
                }
                else if (this.IsNumeric)
                {
                    return "0";
                }
                else if (this.CsharpType == typeof(DateTime))
                {
                    return "DateTime.Now";
                }
                else if (this.CsharpType == typeof(bool))
                {
                    return "false";
                }
                else
                {
                    return "null";
                }
            }
            this.DefaultValue = this.DefaultValue.Trim().Trim('\'').Replace("\n", "").Replace("\r", "");
            string pattern = @"^\((.+)\)$";
            if (Regex.IsMatch(this.DefaultValue, pattern))
            {
                this.DefaultValue = Regex.Replace(this.DefaultValue, pattern, "$1");
                return GetDefaultValue();
            }
            if ("null".Equals(this.DefaultValue, StringComparison.CurrentCultureIgnoreCase))
            {
                return "null";
            }
            if (Regex.IsMatch(this.DefaultValue, "^0(\\.0+)?$"))//0.00
            {
                this.DefaultValue = "0";
            }
            bool msDate = this.DefaultValue.ToUpper().Contains("GETDATE");
            bool oraDate = this.DefaultValue.ToUpper().Contains("SYSDATE");
            bool myDate = this.DefaultValue.ToUpper().Contains("CURRENT_TIMESTAMP");
            if (msDate || oraDate || myDate || "0000-00-00 00:00:00".Equals(this.DefaultValue))
            {
                return "DateTime.Now";
            }
            else if (this.IsNumeric)
            {
                return this.DefaultValue;
            }
            else if (this.CsharpType == typeof(bool))
            {
                return this.DefaultValue == "1" ? "true" : "false";
            }
            else
                return "\"" + this.DefaultValue + "\"";
        }

        private void AnalysisComment(string comment)
        {
            string pattern = "ref\\(([\\w_]+)\\.([\\w_]+)\\)";
            var matches = Regex.Match(comment, pattern);
            if (!matches.Success)
            {
                return;
            }
            string foreignTableName = matches.Groups[1].Value;
            string foreignColumn = matches.Groups[2].Value;
            if (this.Table.ForeignKeys.Any(x => x.Columns.Contains(this)))
            {
                return;
            }
            var foreignTab = _factory.GetTableSchema(foreignTableName);
            var key = new ForeignKey($"virtual_fk_from_comment_{this.Name}", this, foreignTab, foreignTab.Columns.Find(c => c.Name.Equals(foreignColumn, StringComparison.OrdinalIgnoreCase)));
            this.Table.ForeignKeys.Add(key);
        }
    }
}
