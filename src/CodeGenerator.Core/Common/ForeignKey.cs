using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.Common
{
    public class ForeignKey : ConstraintKeyBase
    {
        public IForeignTable ForeignTable { get; set; }
    }
}
