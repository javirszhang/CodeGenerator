using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.Interfaces
{
    public class ColumnCollection : List<IColumn>
    {
        public ColumnCollection() { }
        public ColumnCollection(IEnumerable<IColumn> columns)
        {
            if (columns != null && columns.Any())
            {
                this.AddRange(columns);
            }
        }
        public string ToMethodParameters()
        {
            IEnumerable<string> args = this.Select(x => x.ToString("parameter"));
            return string.Join(", ", args);
        }
    }
}
