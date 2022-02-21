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
        public string ToString(string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                return ToString();
            }
            string[] array = format.Split(':');
            string scene = array[0];
            bool nullable = array.Length > 1 && array[1] == "nullable" && this.IsNullable;

            if (scene == "private")
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
        public abstract string GetDefaultValue();
    }
}
