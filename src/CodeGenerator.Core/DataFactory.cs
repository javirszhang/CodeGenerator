﻿using CodeGenerator.Core.Entities;
using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core
{
    public abstract class DataFactory
    {
        public bool ContainForeignTable
        {
            get; set;
        }
        protected string _connectionString;
        public DataFactory(string connString)
        {
            this._connectionString = connString;
        }
        public abstract ITableSchema GetTableSchema(string table_name);
        public abstract DatabaseSchema GetDatabaseSchema();
        public abstract DataTable GetTableData(string table_name);
        
    }
}
