using CodeGenerator.Core.Common;
using CodeGenerator.Core.Interfaces;
using CodeGenerator.Core.Utils;
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
        private Entities.ConnectionSetting _setting;
        public CodeBuilder(string table_name, string template_name, string @namespace, Entities.ConnectionSetting setting)
        {
            this._template_name = template_name;
            this._namespace = @namespace;
            this._table_name = table_name;
            this._setting = setting;
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
                DataFactory factory = DatabaseResolver.GetDataFactory(this._setting);
                ITableSchema its = factory.GetTableSchema(_table_name);
                string template_fullpath = GetCodeDir();
                TemplateResolver th = new TemplateResolver(template_fullpath);
                xUtils util = new xUtils();
                th.Put("Table", its);
                th.Put("Utils", util);
                th.Put("Const", this.Constant);
                string text = th.BuildString(Path.GetFileName(Path.Combine(template_fullpath, _template_name)));
                string blockComment = System.Text.RegularExpressions.Regex.Replace(text, @"^\s*(/\*.+\n(.+\n)+?\s*\*+/)[^\0]+$", "$1");
                var customerDefine = BlockCommentDictionary(blockComment);
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

        private static Dictionary<string, string> BlockCommentDictionary(string blockComment)
        {
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
        private string GetCodeDir()
        {
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory + "template/" + _template_name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}
