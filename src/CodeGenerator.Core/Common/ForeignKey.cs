using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.Common
{
    public class ForeignKey : ConstraintKeyBase
    {
        /// <summary>
        /// 外键关系
        /// </summary>
        /// <param name="keyName">外键名称</param>
        /// <param name="thisColumn">本表列</param>
        /// <param name="foreignTable">外键表</param>
        /// <param name="foreignColumn">外键表列</param>
        public ForeignKey(string keyName, IColumn thisColumn, ITableSchema foreignTable, IColumn foreignColumn)
        {
            this.ConstraintName = keyName;
            this.ForeignTable = foreignTable;
            this.ForeignColumn = foreignColumn;
            this.Columns.Add(thisColumn);
        }
        public IColumn ForeignColumn { get; set; }
        public ITableSchema ForeignTable { get; set; }
        public IColumn GetKeyColumn()
        {
            return this.Columns.FirstOrDefault();
        }
    }
}
