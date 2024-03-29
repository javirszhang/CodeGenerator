﻿using CodeGenerator.Core.Common;
using CodeGenerator.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator.Core.Interfaces
{
    public abstract class BaseTableSchema : ITableSchema
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public ColumnCollection Columns { get; set; } = new ColumnCollection();
        public List<ForeignKey> ForeignKeys { get; set; } = new List<ForeignKey>();
        public PrimaryKey PrimaryKey { get; set; }
        public List<UniqueKey> UniqueKeys { get; set; } = new List<UniqueKey>();
        public string ObjectType { get; set; }
        public string ViewScript { get; set; }

        public ColumnCollection GetMandatoryColumns()
        {
            return new ColumnCollection(this.Columns?.FindAll(a => !a.IsNullable));
        }
        public ColumnCollection GetMandatoryColumns(bool includeCreateTime)
        {
            var mandatoryColumns = GetMandatoryColumns();
            return includeCreateTime ? mandatoryColumns : new ColumnCollection(mandatoryColumns.FindAll(a => !a.IsCreateTime()));
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
        public string GetBigCamelName() => GetClassName();
        public string GetSmallCamelName()
        {
            string name = GetClassName();
            return name.Substring(0, 1).ToLower() + name.Substring(1);

        }
        public bool ContainEnumField()
        {
            return Columns.Any(x => x.IsEnumField());
        }
    }
}
