﻿using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core
{
    public abstract class ConstraintKeyBase
    {
        public ColumnCollection Columns { get; set; } = new ColumnCollection();
        public string ConstraintName { get; set; }
    }
}
