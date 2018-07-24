using CodeGenerator.Core.Common;
using CodeGenerator.Core.Interfaces;
using CodeGenerator.Core.Utils;
using Javirs.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Core
{
    public class CodeBuilder
    {
        private string _template_name;
        private string _namespace;
        private IConstant _constant;
        private string _table_name;
        private string[] _tables;
        private Entities.ConnectionSetting _setting;
        public CodeBuilder(string[] tables, string table_name, string template_name, string @namespace, Entities.ConnectionSetting setting)
        {
            this._template_name = template_name;
            this._namespace = @namespace;
            this._table_name = table_name;
            this._setting = setting;
            this._tables = tables;
        }
        protected IConstant Constant
        {
            get
            {
                if (_constant == null)
                {
                    _constant = new DefaultConstant(this._namespace);
                }
                return _constant;
            }
            set
            {
                _constant = value;
            }
        }
        public bool Build(string _codefilesavepath)
        {
            try
            {
                List<ITableSchema> allTables = GetAllTableSchema();
                ITableSchema currentTable = allTables.Find(it => it.Name.Equals(_table_name));
                string templateFileName;//= Path.GetFileName(_template_name);
                string templateDirPath;
                string templateFileFullPath = GetTemplateInfo(_template_name, out templateDirPath, out templateFileName);

                TemplateResolver th = new TemplateResolver(templateDirPath);
                xUtils util = new xUtils();
                th.Put("Tables", allTables);
                th.Put("Table", currentTable);
                th.Put("Utils", util);
                th.Put("Const", this.Constant);
                if (ContainRows(templateFileFullPath))
                {
                    var fac = DatabaseResolver.GetDataFactory(this._setting);
                    th.Put("Rows", fac.GetTableData(this._table_name).ToDynamic());
                }
                string text = th.BuildString(templateFileName);

                var customerDefine = BlockCommentDictionary(text);
                string filename = util.ToPascalCase(_table_name) + ".generate.cs";
                if (customerDefine.Count > 0 && customerDefine.ContainsKey("filename"))
                {
                    filename = customerDefine["filename"];
                }

                if (!Directory.Exists(_codefilesavepath))
                    Directory.CreateDirectory(_codefilesavepath);
                string fullsavefilename = _codefilesavepath + "/" + filename;
                if (File.Exists(fullsavefilename))
                    File.Delete(fullsavefilename);
                File.AppendAllText(fullsavefilename, text);
                return true;
            }
            catch (Exception ex)
            {
                this.ExceptionMessage = ex.Message;
                return false;
            }
        }
        private List<ITableSchema> GetAllTableSchema()
        {
            List<ITableSchema> list = new List<ITableSchema>();
            foreach (string name in _tables)
            {
                DataFactory fac = DatabaseResolver.GetDataFactory(_setting);
                ITableSchema its = fac.GetTableSchema(name);
                list.Add(its);
            }
            return list;
        }
        /// <summary>
        /// 是否要求包含表数据
        /// </summary>
        /// <param name="templatefile"></param>
        /// <returns></returns>
        private static bool ContainRows(string templatefile)
        {
            var text = File.ReadAllText(templatefile);
            var dictionary = BlockCommentDictionary(text);
            return dictionary.Count > 0 && dictionary.ContainsKey("rows") && "true".Equals(dictionary["rows"], StringComparison.OrdinalIgnoreCase);
        }
        private static Dictionary<string, string> BlockCommentDictionary(string text)
        {
            string blockComment = System.Text.RegularExpressions.Regex.Replace(text, @"^\s*(/\*.+\n(.+\n)+?\s*\*+/)[^\0]+$", "$1");
            var dictionary = new Dictionary<string, string>();
            StringReader reader = new StringReader(blockComment);
            string currentLine;
            char whitespace = (char)0x20;
            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim().TrimStart(whitespace, '*');
                int firstIndex = currentLine.IndexOf(':');
                if (firstIndex < 0)
                {
                    continue;
                }
                string name = currentLine.Substring(0, firstIndex).Trim().ToLower();
                string value = currentLine.Substring(firstIndex + 1).Trim();
                if (dictionary.ContainsKey(name))
                {
                    dictionary[name] = value;
                }
                else
                {
                    dictionary.Add(name, value);
                }
            }
            return dictionary;
        }
        public string ExceptionMessage { get; set; }
        private static string GetTemplateInfo(string template_name, out string directoryPath, out string template_filename)
        {
            string fullpath = AppDomain.CurrentDomain.BaseDirectory + "template/" + template_name;
            directoryPath = Path.GetDirectoryName(fullpath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            template_filename = Path.GetFileName(fullpath);
            return fullpath;
        }
    }
}
