using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerator.Core.Common;

namespace CodeGenerator.Core.MySQLProvider
{
    public class MySQLTableSchema : ITableSchema
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public List<IColumn> Columns { get; set; }
        public List<ForeignKey> ForiegnKeys { get; set; }
        public PrimaryKey PrimaryKey { get; set; }
        public List<UniqueKey> UniqueKeys { get; set; }
        public string ObjectType { get; set; }
        public string ViewScript { get; set; }
    }
}
