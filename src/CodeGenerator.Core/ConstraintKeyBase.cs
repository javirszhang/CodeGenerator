using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core
{
    public abstract class ConstraintKeyBase
    {
        public List<IColumn> Columns { get; set; }
        public string ConstraintName { get; set; }
    }
}
