using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CodeGenerator.Core.Entities
{
    public class ConnectionSettingCollection : IEnumerable, IList
    {
        private List<ConnectionSetting> _settings;
        public ConnectionSettingCollection()
        {
            _settings = Load();
        }
        private static List<ConnectionSetting> list;
        private List<ConnectionSetting> Load()
        {
            if (list == null)
            {
                list = new List<ConnectionSetting>();
                try
                {
                    string filename = "connectionsettings.config";
                    string fullpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                    if (File.Exists(fullpath))
                    {
                        string text = File.ReadAllText(fullpath);
                        XElement root = XElement.Parse(text);
                        foreach (var item in root.Elements("ConnectionSetting"))
                        {
                            ConnectionSetting cs = new ConnectionSetting();
                            cs.Provider = (DatabaseType)Enum.Parse(typeof(DatabaseType), item.Element("Provider").Value);
                            cs.Name = item.Element("Name").Value;
                            cs.ConnectionString = item.Element("ConnectionString").Value;
                            list.Add(cs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return list;
                }
            }
            return list;
        }
        public ConnectionSettingCollection Add(ConnectionSetting setting)
        {
            this._settings.Add(setting);
            Save2File();
            return this;
        }
        private void Save2File()
        {
            string filename = "connectionsettings.config";
            string fullpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            StringBuilder sb = new StringBuilder();
            sb.Append("<ConnectionSettingCollection>\n");
            foreach (var item in this._settings)
            {
                sb.Append("    <ConnectionSetting>\n");
                sb.Append("        " + BuildXmlTag("Name", item.Name) + "\n");
                sb.Append("        " + BuildXmlTag("Provider", item.Provider.ToString()) + "\n");
                sb.Append("        " + BuildXmlTag("ConnectionString", item.ConnectionString, true) + "\n");
                sb.Append("    </ConnectionSetting>\n");
            }
            sb.Append("</ConnectionSettingCollection>");
            FileInfo file = new FileInfo(fullpath);
            if (file.Exists)
                file.Delete();
            File.AppendAllText(fullpath, sb.ToString());
        }
        private static string BuildXmlTag(string name, string value, bool cdata = false)
        {
            if (cdata)
            {
                value = "<![CDATA[" + value + "]]>";
            }
            return "<" + name + ">" + value + "</" + name + ">";
        }
        public IEnumerator GetEnumerator()
        {
            return new SettingEnumerator(this);
        }
        public class SettingEnumerator : IEnumerator
        {
            private int _index = -1;
            private List<ConnectionSetting> _settings;
            public SettingEnumerator(ConnectionSettingCollection csc)
            {
                this._settings = csc._settings;
            }

            public object Current
            {
                get { return _settings[_index]; }
            }

            public bool MoveNext()
            {
                _index++;
                return _index <= _settings.Count - 1;
            }

            public void Reset()
            {
                _index = -1;
            }
        }

        public ConnectionSetting this[int index]
        {
            get
            {
                return this._settings[index];
            }
        }
        public ConnectionSetting this[string name]
        {
            get
            {
                return this._settings.Find(it => it.Name == name);
            }
        }

        public int Add(object value)
        {
            this._settings.Add((ConnectionSetting)value);
            return 1;
        }

        public void Clear()
        {
            this._settings.Clear();
        }

        public bool Contains(object value)
        {
            return this._settings.Contains((ConnectionSetting)value);
        }

        public int IndexOf(object value)
        {
            return this._settings.IndexOf((ConnectionSetting)value);
        }

        public void Insert(int index, object value)
        {
            this._settings.Insert(index, (ConnectionSetting)value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
        public void Remove(ConnectionSetting cs)
        {
            this._settings.Remove(cs);
            Save2File();
        }
        void IList.Remove(object value)
        {
            this._settings.Remove((ConnectionSetting)value);
        }

        public void RemoveAt(int index)
        {
            this._settings.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return this._settings[index];
            }
            set
            {
                this._settings[index] = (ConnectionSetting)value;
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return this._settings.Count; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }
        private static object _syncRoot = new object();
        public object SyncRoot
        {
            get { return _syncRoot; }
        }
    }
}
