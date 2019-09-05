using CodeGenerator.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.Interfaces
{
    public interface ITableSchema
    {
        string Name { get; set; }
        string Comment { get; set; }
        List<IColumn> Columns { get; set; }
        List<ForeignKey> ForiegnKeys { get; set; }
        List<UniqueKey> UniqueKeys { get; set; }
        PrimaryKey PrimaryKey { get; set; }
        /// <summary>
        /// 对象类型，TABLE or VIEW
        /// </summary>
        string ObjectType { get; set; }
        /// <summary>
        /// 视图脚本
        /// </summary>
        string ViewScript { get; set; }
    }
}
