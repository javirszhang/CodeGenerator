using CodeGenerator.Core.Interfaces;
using CodeGenerator.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeGenerator.Core.OracleProvider
{
    public class OracleColumn : BaseColumn
    {
        public OracleColumn(ITableSchema table, DataFactory factory) : base(table, factory)
        {
        }
    }
}
