using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerator.Core.Common;

namespace CodeGenerator.Core.Utils
{
    public class gTools
    {
        public bool IsForeignKey(IColumn column, List<ForeignKey> foreignKeys)
        {
            if (foreignKeys == null || foreignKeys.Count <= 0 || column == null)
            {
                return false;
            }
            foreach (ForeignKey fk in foreignKeys)
            {
                foreach (IColumn col in fk.Columns)
                {
                    if (col.Name.Equals(column.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
