using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CodeGenerator.Core.Utils;

namespace CodeGenerator.Core.Entities
{
    public class ConnectionSetting
    {
        public DatabaseType Provider { get; set; }
        public string ConnectionString { get; set; }
        public string Name { get; set; }      
    }

}
