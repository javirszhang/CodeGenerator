using CodeGenerator.Core.Common;
using CodeGenerator.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core.Interfaces
{
    public abstract class BaseTableSchema : ITableSchema
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public ColumnCollection Columns { get; set; }
        public List<ForeignKey> ForiegnKeys { get; set; }
        public PrimaryKey PrimaryKey { get; set; }
        public List<UniqueKey> UniqueKeys { get; set; }
        public string ObjectType { get; set; }
        public string ViewScript { get; set; }

        public ColumnCollection GetMandatoryColumns()
        {
            return new ColumnCollection(this.Columns?.FindAll(a => !a.IsNullable && !a.IsCreateTime()));
        }        
        public ColumnCollection GetNullableColumns()
        {
            return new ColumnCollection(this.Columns?.FindAll(a => a.IsNullable));
        }
        public string GetClassName()
        {
            var util = new xUtils();
            if ((this.Name.StartsWith("t", StringComparison.OrdinalIgnoreCase) || this.Name.StartsWith("v", StringComparison.OrdinalIgnoreCase)) && this.Name.Contains("_"))
            {
                return util.ToPascalCase(util.UnwrapTablePrefix(this.Name), false);
            }
            return util.ToPascalCase(this.Name, false);
        }
    }
}
