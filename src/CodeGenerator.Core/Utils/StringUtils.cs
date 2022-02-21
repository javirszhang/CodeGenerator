using CodeGenerator.Core.Common;
using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeGenerator.Core.Utils
{
    public class xUtils
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
                t += Pascal(m.Value) + "_";
            }
            t = t.TrimEnd('_');
            return t;
        }
        public string ToPascalCase(string s, bool includeSplit)
        {
            string t = string.Empty;
            Regex regex = new Regex("[^_]*");
            MatchCollection mc = regex.Matches(s);
            foreach (Match m in mc)
            {
                if (string.IsNullOrEmpty(m.Value))
                    continue;
                t += Pascal(m.Value);
                if (includeSplit)
                {
                    t += "_";
                }
            }
            if (includeSplit)
            {
                t = t.TrimEnd('_');
            }
            return t;
        }
        private static string Pascal(string s)
        {
            char first = s[0];
            string rest = s.Substring(1);
            bool isUpper = IsAllUpCase(rest);
            if (char.IsUpper(first) && !isUpper)
            {
                return s;
            }
            return char.ToUpper(first) + (isUpper ? rest.ToLower() : rest);
        }
        /// <summary>
        /// 检查字符串是否全部大写
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static bool IsAllUpCase(string s)
        {
            foreach (var item in s)
            {
                if (char.IsLower(item))
                {
                    return false;
                }
            }
            return true;
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
                    else if (columnType == typeof(DateTime))
                        return "DateTime.Now";
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
                if (Regex.IsMatch(s, "^0(\\.0+)?$"))
                {
                    return "0";
                }
                if (s.ToUpper().StartsWith("SYSDATE") || s.ToUpper().IndexOf("CURRENT_TIMESTAMP") > -1 || "0000-00-00 00:00:00".Equals(s))
                {
                    return "DateTime.Now";
                }
                else if (columnType == typeof(int) || columnType == typeof(decimal) || columnType == typeof(long))
                {
                    return s;
                }
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

        public bool IsForeignKey(IColumn column, List<ForeignKey> foreignKeys)
        {
            if (foreignKeys == null || foreignKeys.Count <= 0 || column == null)
            {
                return false;
            }
            foreach (ForeignKey fk in foreignKeys)
            {
                foreach (IColumn col in fk.Columns)
                {
                    if (col.Name.Equals(column.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public object GetObjectFromArray(Array array, int index)
        {
            if (index >= array.Length)
            {
                return null;
            }
            return array.GetValue(index);
        }

        public string UnwrapTablePrefix(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                return tableName;
            }
            int index = tableName.IndexOf('_');
            if (index < 0)
            {
                return tableName;
            }
            string suffix = tableName.Substring(index + 1);
            return ToPascalCase(suffix);
        }

        public string MatchValue(string input, string pattern, int groupindex)
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
        public string PropertyTypeFromComment(string comment, string defaultValue, bool nullable)
        {
            string matchName = MatchValue(comment, "\\$(.+?)\\$", 1);
            if (!string.IsNullOrEmpty(matchName) && nullable)
            {
                matchName += "?";
            }
            return matchName ?? defaultValue;
        }
        public string PropertyTypeFromComment(string comment, string defaultValue)
        {
            return this.PropertyTypeFromComment(comment, defaultValue, false);
        }
    }
}
