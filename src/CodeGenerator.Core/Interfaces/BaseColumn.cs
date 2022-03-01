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
                return $"({ResolvePropertyTypeFromComment()}){result}";
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
                else if (this.CsharpType == typeof(DateTime?))
                {
                    return "null";
                }
                else
                {
                    return "null";
                }
            }
            string pattern = @"^\((.+)\)$";
            if (Regex.IsMatch(this.DefaultValue, pattern))
            {
                this.DefaultValue = Regex.Replace(this.DefaultValue, pattern, "$1");
                return GetDefaultValue();
            }
            this.DefaultValue = this.DefaultValue.Trim().TrimStart('\'').TrimEnd('\'').Replace('\n', ' ').Replace('\r', ' ');
            if ("null".Equals(this.DefaultValue, StringComparison.CurrentCultureIgnoreCase))
            {
                return "null";
            }
            if (Regex.IsMatch(this.DefaultValue, "^0(\\.0+)?$"))
            {
                return "0";
            }
            if (this.DefaultValue.ToUpper().StartsWith("GETDATE") || "0000-00-00 00:00:00".Equals(this.DefaultValue))
            {
                return "DateTime.Now";
            }
            else if (this.IsNumeric)
            {
                return this.DefaultValue;
            }
            else
                return "\"" + this.DefaultValue + "\"";
        }
    }
}
