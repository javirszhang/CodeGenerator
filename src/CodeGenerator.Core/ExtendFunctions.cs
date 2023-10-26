using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core
{
    public static class ExtendFunctions
    {
        public static string GetString(this DataRow row, string columnName)
        {
            object b = row[columnName];
            if (b == DBNull.Value || b == null)
            {
                return string.Empty;
            }
            return b.ToString();
        }
        public static int GetInt(this DataRow row, string columnName)
        {
            return row.GetInt(columnName, 0);
        }
        public static int GetInt(this DataRow row, string columnName, int defaultValue)
        {
            object b = row[columnName];
            if (b == DBNull.Value || b == null || string.IsNullOrEmpty(b.ToString()))
            {
                return defaultValue;
            }
            return Convert.ToInt32(b);
        }
        public static decimal GetDecimal(this DataRow row, string columnName)
        {
            object ob = row[columnName];
            if (ob == null || ob == DBNull.Value)
            {
                return 0;
            }
            return Convert.ToDecimal(ob);
        }

        public static event Action<int> Progress;

        public static void OnProcess(int v)
        {
            Progress?.Invoke(v);
        }

        /// <summary>
        /// 转换为动态对象
        /// </summary>
        /// <param name="table">数据表</param>
        /// <param name="selectColumns">选择的数据列</param>
        /// <returns></returns>
        public static dynamic ToDynamic(this System.Data.DataTable table, params string[] selectColumns)
        {
            if (table == null)
            {
                List<object> list = new List<object>();
                return list;
            }
            return ToDynamic(table, forceToCollection: true, selectColumns: selectColumns);
        }
        /// <summary>
        /// 转换为动态对象
        /// </summary>
        /// <param name="row">数据行</param>
        /// <param name="selectColumns"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(this System.Data.DataRow row, params string[] selectColumns)
        {
            if (row == null)
            {
                return null;
            }
            return ToDynamic(row.Table, forceToCollection: false, selectColumns: selectColumns);
        }
        /// <summary>
        /// 转换为动态对象
        /// </summary>
        /// <param name="table">数据表</param>
        /// <param name="forceToCollection">是否强制转换为集合</param>
        /// <param name="selectColumns">过滤列（只要这些列，不传则全选）</param>
        /// <param name="keyCase">动态属性的字符串格式（小写，大写，驼峰，驼峰不带分隔符）</param>
        /// <param name="OnFieldGenerating">生成动态属性时</param>
        /// <param name="OnBeforeRowAdding">生成集合一行数据之前</param>
        /// <param name="OnAfterRowAdded">生产集合一行数据之后</param>
        /// <returns></returns>
        public static dynamic ToDynamic(this System.Data.DataTable table, bool forceToCollection = true,
            string[] selectColumns = null,
            DynamicConverter.KeyType keyCase = DynamicConverter.KeyType.LowerCase,
            Func<NameValuePair, NameValuePair> OnFieldGenerating = null,
            Action<List<dynamic>> OnBeforeRowAdding = null,
            Action<List<dynamic>> OnAfterRowAdded = null)
        {
            DynamicConverter converter = new DynamicConverter(table, forceToCollection);
            converter.KeyCase = keyCase;
            converter.FieldGenerating += OnFieldGenerating;
            converter.BeforeRowAdding += OnBeforeRowAdding;
            converter.AfterRowAdded += OnAfterRowAdded;
            return converter.ToDynamic(selectColumns);
        }
    }
}
