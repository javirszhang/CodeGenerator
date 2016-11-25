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
        List<IColumn> Columns { get; set; }
        List<ForiegnKey> ForiegnKeys { get; set; }
        List<UniqueKey> UniqueKeys { get; set; }
        PrimaryKey PrimaryKey { get; set; }
    }
}
