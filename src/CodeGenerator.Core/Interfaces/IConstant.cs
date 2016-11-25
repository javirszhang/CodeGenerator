using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.Interfaces
{
    public interface IConstant
    {
        string CurrentTime { get; }
        string Version { get; }
        string Namespace { get; }
    }
}
