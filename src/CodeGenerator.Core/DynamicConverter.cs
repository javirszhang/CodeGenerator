using CodeGenerator.Core.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeGenerator.Core
{
    /// <summary>
    /// 动态类型转换
    /// </summary>
    public class DynamicConverter
    {
        private DataTable _table;
        private bool _forceToCollection = false;
        /// <summary>
        /// 动态类型转换
        /// </summary>
        /// <param name="row"></param>
        public DynamicConverter(DataRow row)
        {
            this._table = row.Table;
        }
        /// <summary>
        /// 动态类型转换
        /// </summary>
        /// <param name="table"></param>
        /// <param name="forceToCollection"></param>
        public DynamicConverter(DataTable table, bool forceToCollection = true)
        {
            this._table = table;
            this._forceToCollection = forceToCollection;
        }
        /// <summary>
        /// Key的字符类型（大写，小写，驼峰）
        /// </summary>
        public KeyType KeyCase { get; set; }

        /// <summary>
        /// 转换为动态类型
        /// </summary>
        /// <param name="filterColumns">过滤列（只要这些列，不传则全选）</param>
        /// <returns></returns>
        public dynamic ToDynamic(params string[] filterColumns)
        {
            List<dynamic> list = new List<dynamic>();
            if (_table == null || _table.Rows == null || _table.Rows.Count <= 0)
            {
                return list;
            }
            if (_table.Rows.Count == 1 && !_forceToCollection)
            {
                return ToDynamicFromRow(_table.Rows[0], filterColumns);
            }

            foreach (DataRow row in _table.Rows)
            {
                OnBeforeRowAdding(list);
                dynamic obj = ToDynamicFromRow(row, filterColumns);
                list.Add(obj);
                OnAfterRowAdded(list);
            }
            return list;
        }
        private dynamic ToDynamicFromRow(DataRow row, params string[] filterColumns)
        {
            dynamic obj = new ExpandoObject();
            var comparer = StringComparer.OrdinalIgnoreCase;
            foreach (DataColumn col in this._table.Columns)
            {
                string orignalKey = col.ColumnName;
                NameValuePair nvp = new NameValuePair(GenerateKey(orignalKey, KeyCase), GetRowValue(row, orignalKey, col.DataType));
                nvp = OnFieldGenerating(nvp);
                if (nvp != null)
                {
                    if (filterColumns == null || filterColumns.Length <= 0 || (filterColumns.Length > 0 && (filterColumns.Contains(orignalKey, comparer) || filterColumns.Contains(nvp.Name, comparer))))
                    {
                        ((IDictionary<string, object>)obj).Add(nvp.Name, nvp.Value);
                    }
                }
            }
            return obj;
        }
        private static object GetRowValue(DataRow row, string columnName, Type dataType)
        {
            object value = row[columnName];
            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            if (dataType == typeof(int))
            {
                return Convert.ToInt32(value);
            }
            else if (dataType == typeof(uint))
            {
                return Convert.ToUInt32(value);
            }
            else if (dataType == typeof(long))
            {
                return Convert.ToInt64(value);
            }
            else if (dataType == typeof(byte))
            {
                return Convert.ToByte(value);
            }
            else if (dataType == typeof(short))
            {
                return Convert.ToInt16(value);
            }
            else if (dataType == typeof(decimal))
            {
                decimal d = Convert.ToDecimal(value);
                if (d == decimal.Truncate(d))//是整数
                {
                    return d < int.MaxValue ? (int)d : (long)d;
                }
                return value;
            }
            else
            {
                return value;
            }
        }
        /// <summary>
        /// 对DataRow的隐式转换
        /// </summary>
        /// <param name="row"></param>
        public static implicit operator DynamicConverter(DataRow row)
        {
            DynamicConverter dc = new DynamicConverter(row);
            return dc;
        }
        private static string GenerateKey(string key, KeyType type)
        {
            string transformKey = key;
            var utils = new xUtils();
            switch (type)
            {
                case KeyType.LowerCase:
                    transformKey = key.ToLower();
                    break;
                case KeyType.UpperCase:
                    transformKey = key.ToUpper();
                    break;
                case KeyType.PascalCase:
                    transformKey = utils.ToPascalCase(key);
                    break;
                case KeyType.PascalCaseWithoutSplit:
                    transformKey = utils.ToPascalCase(key, false);
                    break;
            }
            return transformKey;
        }
        /// <summary>
        /// 字段生成之前
        /// </summary>
        public event Func<NameValuePair, NameValuePair> FieldGenerating;
        /// <summary>
        /// 字段生成之前
        /// </summary>
        protected virtual NameValuePair OnFieldGenerating(NameValuePair nvp)
        {
            if (FieldGenerating != null)
            {
                return FieldGenerating(nvp);
            }
            return nvp;
        }
        /// <summary>
        /// 属性名的大小写格式
        /// </summary>
        public enum KeyType
        {
            /// <summary>
            /// 全小写
            /// </summary>
            LowerCase = 0,
            /// <summary>
            /// 全大写
            /// </summary>
            UpperCase = 1,
            /// <summary>
            /// 驼峰
            /// </summary>
            PascalCase = 2,
            /// <summary>
            /// 驼峰去掉分隔符
            /// </summary>
            PascalCaseWithoutSplit = 3
        }

        #region list event
        /// <summary>
        /// 一行数据生成之前
        /// </summary>
        public event Action<List<dynamic>> BeforeRowAdding;
        /// <summary>
        /// 一行数据生成之前
        /// </summary>
        protected virtual void OnBeforeRowAdding(List<dynamic> list)
        {
            if (BeforeRowAdding != null)
            {
                BeforeRowAdding(list);
            }
        }
        /// <summary>
        /// 一行数据生成之后
        /// </summary>
        public event Action<List<dynamic>> AfterRowAdded;
        /// <summary>
        /// 一行数据生成之后
        /// </summary>
        protected virtual void OnAfterRowAdded(List<dynamic> list)
        {
            if (AfterRowAdded != null)
            {
                AfterRowAdded(list);
            }
        }
        #endregion
    }
    public class NameValuePair
    {
        public NameValuePair(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
        public string Name { get; set; }
        public object Value { get; set; }
    }
}