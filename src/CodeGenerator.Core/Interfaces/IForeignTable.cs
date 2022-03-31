using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.Interfaces
{
    public interface IForeignTable
    {
        IColumn RefColumn { get; set; }
        ITableSchema Table { get; set; }
    }
}
