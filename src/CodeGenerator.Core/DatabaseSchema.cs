using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core
{
    public class DatabaseSchema
    {
        public List<ITableSchema> Tables { get; set; }
    }
}
