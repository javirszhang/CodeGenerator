using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.Interfaces
{
    public class ForeignTable : IForeignTable
    {
        public ForeignTable(ITableSchema table, string foreignColumnName)
        {
            this.RefColumn = table.Columns.Find(c => c.Name == foreignColumnName);
            this.Table = table;
        }
        public IColumn RefColumn { get; set; }
        public ITableSchema Table { get; set; }
    }
}
