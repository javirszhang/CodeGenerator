using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeGenerator.Core.Utils
{
    public class StringUtils
    {
        public string ToPascalCase(string s)
        {
            string t = string.Empty;
            Regex regex = new Regex("[^_]*");
            MatchCollection mc = regex.Matches(s);
            foreach (Match m in mc)
            {
                if (string.IsNullOrEmpty(m.Value))
                    continue;
                t += m.Value.Substring(0, 1).ToUpper() + m.Value.Substring(1, m.Value.Length - 1).ToLower() + "_";
            }
            t = t.TrimEnd('_');
            return t;
        }

        public string SolveDefaultValue(string s, Type columnType, bool isNullable)
        {
            if (string.IsNullOrEmpty(s))//No Default Value
            {
                if (!isNullable)//Mondary
                {
                    if (columnType == typeof(string))
                        return "string.Empty";
                    else if (columnType == typeof(int) || typeof(decimal) == columnType)
                        return "0";
                    else
                        return "DBNull.Value";
                }
                else//Optional
                {
                    return "DBNull.Value";
                }
            }
            else// Has Default Value
            {
                s = s.Trim().TrimStart('\'').TrimEnd('\'').Replace('\n', ' ').Replace('\r', ' ');
                if ("null".Equals(s, StringComparison.CurrentCultureIgnoreCase))
                {
                    return "DBNull.Value";
                }
                if (s.ToUpper().StartsWith("SYSDATE"))
                    return "DateTime.Now";
                else if (columnType == typeof(int) || columnType == typeof(decimal))
                    return s;
                else
                    return "\"" + s + "\"";
            }
        }

        public string SolveForCondition(IColumn column)
        {
            if (column == null)
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append(column.Name.ToUpper() + "=:" + column.Name.ToUpper());
            return sb.ToString();
        }

        public string SolveForCondition(List<IColumn> columns)
        {
            if (columns == null || columns.Count <= 0)
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (IColumn ics in columns)
                sb.Append(" " + ics.Name.ToUpper() + "=:" + ics.Name.ToUpper() + " AND");
            if (sb.Length > 3)
                sb.Remove(sb.Length - 3, 3);
            return sb.ToString();
        }

        public string Inline(string s)
        {
            return s.Replace('\n', ' ').Replace('\r', ' ');
        }
        public string Cast(Type ColumnType, bool IsNullable)
        {
            if (!IsNullable)
            {
                if (ColumnType.IsPrimitive)
                    return "Convert.To" + ColumnType.Name;
                else
                    return "";
            }
            else
            {
                if (ColumnType.IsPrimitive)
                    return "Helper.To" + ColumnType.Name;
                else
                    return "";
            }
        }
    }
}
