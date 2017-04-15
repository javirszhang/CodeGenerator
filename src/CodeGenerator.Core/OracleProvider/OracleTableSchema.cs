using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerator.Core.Common;

namespace CodeGenerator.Core.OracleProvider
{
    internal class OracleTableSchema : ITableSchema
    {
        public List<IColumn> Columns
        {
            get;
            set;
        }

        public List<ForeignKey> ForiegnKeys
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        public string Comment { get; set; }
        public PrimaryKey PrimaryKey
        {
            get;
            set;
        }

        public List<UniqueKey> UniqueKeys
        {
            get;
            set;
        }
    }
}
