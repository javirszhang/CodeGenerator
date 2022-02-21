using CodeGenerator.Core.Interfaces;
using CodeGenerator.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeGenerator.Core.MssqlProvider
{
    public class SqlServerColumn : BaseColumn
    {
        public override string GetDefaultValue()
        {
            if (string.IsNullOrEmpty(DefaultValue))
            {
                if (this.IsNullable)
                {
                    return "null";
                }
                if (this.CsharpType == typeof(string))
                {
                    return "string.Empty";
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
                return "DBNull.Value";
            }
            if (Regex.IsMatch(this.DefaultValue, "^0(\\.0+)?$"))
            {
                return "0";
            }
            if (this.DefaultValue.ToUpper().StartsWith("GETDATE") || "0000-00-00 00:00:00".Equals(this.DefaultValue))
            {
                return "DateTime.Now";
            }
            else if (this.CsharpType == typeof(int) || this.CsharpType == typeof(decimal) || this.CsharpType == typeof(long))
            {
                return this.DefaultValue;
            }
            else
                return "\"" + this.DefaultValue + "\"";
        }
    }
}
