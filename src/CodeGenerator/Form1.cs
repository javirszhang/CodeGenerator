using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using CodeGenerator.Core.Entities;
using CodeGenerator.Core;
using CodeGenerator.Core.Interfaces;
using System.Threading;
using System.Xml.Linq;
using System.Threading.Tasks;
//using Microsoft.Data.ConnectionUI;

namespace CodeGenerator
{
    public partial class Form1 : Form
    {
        private string _namespace, _savePath;
        public Form1()
        {
            InitializeComponent();
            _savePath = txtSavePath.Text.Trim();
            _namespace = "Winner.YXH.****.DataAccess";
        }
        public Form1(string[] args)
            : this()
        {
            if (args.Length == 2)
            {
                string projectDir = args[0];
                string projectFileName = args[1];
                ResolveForNamespace(projectDir, projectFileName);
            }
            else
            {
                _namespace = "Winner.YXH.****.DataAccess";
                _savePath = txtSavePath.Text.Trim();
            }
            txtSavePath.Text = this._savePath;
            //MessageBox.Show(string.Format("Namespace={0}\r\nSavePath={1}", this._namespace, this._savePath));
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            NewConnection form = new NewConnection();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                cb_dbConnectionName.Items.Clear();
                ConnectionSettingCollection csc = new ConnectionSettingCollection();
                foreach (ConnectionSetting item in csc)
                {
                    cb_dbConnectionName.Items.Add(item.Name);
                }
                cb_dbConnectionName.SelectedIndex = csc.Count - 1;
            }

            #region Microsoft Connection Dialog
            //DataConnectionDialog dialog = new DataConnectionDialog();
            //dialog.DataSources.Add(DataSource.SqlDataSource);
            //dialog.DataSources.Add(DataSource.OracleDataSource);
            //dialog.DataSources.Add(DataSource.AccessDataSource);
            //dialog.DataSources.Add(DataSource.OdbcDataSource);
            //dialog.DataSources.Add(DataSource.SqlFileDataSource);
            //dialog.SelectedDataSource = DataSource.OracleDataSource;
            //dialog.SelectedDataProvider = DataProvider.OracleDataProvider;
            //if (DataConnectionDialog.Show(dialog, this) == DialogResult.OK)
            //{
            //    ConnectionSettingCollection csc = new ConnectionSettingCollection();
            //    if (csc[dialog.SelectedDataSource.Name] != null)
            //    {
            //        MessageBox.Show("此名称已存在，请更换名称");
            //        return;
            //    }
            //    ConnectionSetting setting = new ConnectionSetting();
            //    setting.ConnectionString = dialog.ConnectionString;
            //    setting.Name = dialog.SelectedDataSource.Name;
            //    setting.Provider = (DatabaseType)Enum.Parse(typeof(DatabaseType), type);
            //    csc.Add(setting);
            //}
            #endregion
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            ConnectionSettingCollection csc = new ConnectionSettingCollection();
            csc.Remove(csc[cb_dbConnectionName.SelectedItem.ToString()]);
            txtConnectionString.Text = string.Empty;
            RefreshCombox();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            lb_tables.Items.Clear();
            ConnectionSettingCollection csc = new ConnectionSettingCollection();
            string connName = cb_dbConnectionName.SelectedItem.ToString();
            ConnectionSetting setting = csc[connName];
            DataFactory factory = DatabaseResolver.GetDataFactory(setting);
            DatabaseSchema db = factory.GetDatabaseSchema();
            List<string> tables = new List<string>();
            foreach (var table in db.Tables)
            {
                tables.Add(table.Name);
                lb_tables.Items.Add(table.Name);
            }
            AutoCompleteStringCollection autoColl = new AutoCompleteStringCollection();
            autoColl.AddRange(tables.ToArray());
            txtSearchbox.AutoCompleteCustomSource = autoColl;
        }

        private void btnChangeDir_Click(object sender, EventArgs e)
        {

        }

        private void btnOpenDir_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSavePath.Text))
            {
                MessageBox.Show("目录不存在", "友情提示");
                return;
            }
            if (!Directory.Exists(txtSavePath.Text))
                Directory.CreateDirectory(txtSavePath.Text);
            System.Diagnostics.Process.Start(txtSavePath.Text);
        }
        private string DbConnectionName { get; set; }
        private string TemplatePath { get; set; }
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            //ConnectionSetting setting = new ConnectionSettingCollection()[cb_dbConnectionName.SelectedItem.ToString()];
            this.DbConnectionName = cb_dbConnectionName.SelectedItem.ToString();
            this.TemplatePath = (tableTrees.SelectedNode.Parent.Text + "/" == TreeRootName + "/" ? "" : tableTrees.SelectedNode.Parent.Text + "/") + tableTrees.SelectedNode.Text;
            if (lb_selectedTables.Items.Count <= 0)
            {
                MessageBox.Show("Please choose the tables which you want to generate!");
                return;
            }
            if (tableTrees.SelectedNode == null || (tableTrees.SelectedNode.Nodes != null && tableTrees.SelectedNode.Nodes.Count > 0))
            {
                MessageBox.Show("The template which you choose was not the standard VTL");
                return;
            }
            Task.Factory.StartNew(() =>
            {
                string errorMsg;
                GenerateAll(out errorMsg);
            });
            //int count = lb_selectedTables.Items.Count;
            //int step = 100 % count == 0 ? 100 / count : 100 / count + 1;
            //string templatepath = (tableTrees.SelectedNode.Parent.Text + "/" == TreeRootName + "/" ? "" : tableTrees.SelectedNode.Parent.Text + "/") + tableTrees.SelectedNode.Text;
            //List<Task> tasks = new List<Task>();
            //foreach (object obj in lb_selectedTables.Items)
            //{
            //    GenerateParameter para = new GenerateParameter
            //    {
            //        TableName = obj.ToString(),
            //        Setting = setting,
            //        Step = step,
            //        SavePath = this._savePath, //txtSavePath.Text.Trim(),
            //        TemplatePath = templatepath
            //    };
            //    tasks.Add(Task.Factory.StartNew(state =>
            //    {
            //        Generate(state);
            //    }, para));
            //}
            //Task.Factory.StartNew(() =>
            //{
            //    Task.WaitAll(tasks.ToArray());
            //    this.Invoke((MethodInvoker)delegate
            //    {
            //        lb_selectedTables.Items.Clear();
            //        generateProcess.Value = 0;
            //        _rate = 0;
            //        MessageBox.Show("All generation has been completed!");
            //    });
            //});
        }
        private void GenerateAll(out string errorMsg)
        {
            ConnectionSetting setting = new ConnectionSettingCollection()[this.DbConnectionName];
            int count = lb_selectedTables.Items.Count;
            int step = 100 % count == 0 ? 100 / count : 100 / count + 1;

            errorMsg = "";
            foreach (object obj in lb_selectedTables.Items)
            {
                GenerateParameter para = new GenerateParameter
                {
                    TableName = obj.ToString(),
                    Setting = setting,
                    Step = step,
                    SavePath = this._savePath, //txtSavePath.Text.Trim(),
                    TemplatePath = this.TemplatePath
                };
                if (!Generate(para))
                {
                    errorMsg += "[" + para.TableName + "]";
                }
                this.Invoke((MethodInvoker)delegate
                {
                    this.Rate = para.Step;
                    this.generateProcess.Value = this.Rate;
                });

            }
            this.Invoke((MethodInvoker)delegate
            {
                lb_selectedTables.Items.Clear();
                generateProcess.Value = 0;
                _rate = 0;
                MessageBox.Show("All generate tasks has been completed");
            });

        }
        private static int _rate = 0;
        private void ResolveForNamespace(string projectDir, string projectFileName)
        {
            try
            {
                Microsoft.Build.Evaluation.ProjectCollection projCollection = new Microsoft.Build.Evaluation.ProjectCollection();
                var proj = projCollection.LoadProject(Path.Combine(projectDir, projectFileName));
                foreach (var item in proj.Properties)
                {
                    if (item.Name == "RootNamespace")
                    {
                        this._namespace = item.EvaluatedValue;
                        break;
                    }
                }
                this._savePath = Path.Combine(projectDir, "GenerateCode");
            }
            catch (Exception ex)
            {
                File.AppendAllText("C:\\CodeGenerator.txt", string.Format("{0}{1}  错误信息：{2}{3},堆栈信息：{4}", Environment.NewLine, DateTime.Now.ToString(), ex.Message, Environment.NewLine, ex.StackTrace));
            }
        }
        protected int Rate
        {
            get
            {
                lock (lockobj)
                {
                    if (_rate > 100)
                        return 100;
                    return _rate;
                }
            }
            set
            {
                lock (lockobj)
                {
                    _rate += value;
                }
            }
        }
        private static object lockobj = new object();

        private bool Generate(object obj)
        {
            GenerateParameter para = obj as GenerateParameter;
            IConstant constant = new Constant(this._namespace);
            //Rate = para.Step;
            CodeBuilder builder = new CodeBuilder(para.TableName, para.TemplatePath, _namespace, para.Setting);
            bool result = builder.Build(para.SavePath);
            File.AppendAllText(@"D:\CodeGenerate.log", string.Format("{4}\tGenerate Table {0}，Result is {1}，Build Path at {2}{3}{5}", para.TableName, result, para.SavePath, Environment.NewLine, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                builder.ExceptionMessage));
            return result;
        }
        
        
        protected class GenerateParameter
        {
            public string TableName { get; set; }
            public ConnectionSetting Setting { get; set; }
            public string TemplatePath { get; set; }
            public string SavePath { get; set; }
            public int Step { get; set; }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            ConnectionSettingCollection csc = new ConnectionSettingCollection();
            foreach (ConnectionSetting item in csc)
            {
                cb_dbConnectionName.Items.Add(item.Name);
            }
            if (csc.Count > 0)
                cb_dbConnectionName.SelectedIndex = 0;
            string path = AppDomain.CurrentDomain.BaseDirectory + "Template/";            
            //tableTrees.DrawMode = TreeViewDrawMode.OwnerDrawAll; //是否自绘树形UI
            tableTrees.Nodes.Add(ListDir(path, null));
            tableTrees.ExpandAll();
        }

        #region ListItem Operation
        private Dictionary<string, string> mydic = new Dictionary<string, string>();
        protected const string TreeRootName = "代码生成器--模版簇";
        private TreeNode ListDir(string path, TreeNode node)
        {
            if (node == null)
            {
                node = new TreeNode(TreeRootName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo file in di.GetFiles())
                {
                    node.Nodes.Add(new TreeNode(file.Name));
                }
                foreach (DirectoryInfo item in di.GetDirectories())
                {
                    TreeNode t = new TreeNode(item.Name);
                    node.Nodes.Add(t);
                    ListDir(item.FullName, t);
                }
            }
            else
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo file in di.GetFiles())
                {
                    node.Nodes.Add(new TreeNode(file.Name));
                }
                foreach (DirectoryInfo item in di.GetDirectories())
                {
                    TreeNode n = new TreeNode(item.Name);
                    node.Nodes.Add(n);
                    ListDir(item.FullName, n);
                }
            }
            return node;

        }
        private void RefreshCombox()
        {
            cb_dbConnectionName.Items.Clear();
            ConnectionSettingCollection csc = new ConnectionSettingCollection();
            foreach (ConnectionSetting item in csc)
            {
                cb_dbConnectionName.Items.Add(item.Name);
            }
            if (csc.Count > 0)
                cb_dbConnectionName.SelectedIndex = 0;
        }


        private void lb_tables_DoubleClick(object sender, EventArgs e)
        {
            if (!lb_selectedTables.Items.Contains(lb_tables.SelectedItem))
                lb_selectedTables.Items.Add(lb_tables.SelectedItem);
        }

        private void lb_selectedTables_DoubleClick(object sender, EventArgs e)
        {
            lb_selectedTables.Items.Remove(lb_selectedTables.SelectedItem);
        }

        private void cb_dbConnectionName_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConnectionSettingCollection csc = new ConnectionSettingCollection();
            this.txtConnectionString.Text = csc[cb_dbConnectionName.SelectedItem.ToString()].ConnectionString;
        }

        private void tableTrees_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            //e.DrawDefault = true;
            //if (e.Node.Nodes != null && e.Node.Nodes.Count > 0)
            //{
            //    e.Graphics.DrawString(e.Node.Text+"xx", e.Node.NodeFont, Brushes.Blue, e.Bounds.Right, e.Bounds.Top);
            //}
        }

        private void txtSearchbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                lb_selectedTables.Items.Add(((TextBox)sender).Text);
            }
        }

        private void txtSearchbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textbox = sender as TextBox;
                lb_selectedTables.Items.Add(textbox.Text);
                textbox.Text = "";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0);
        }
        #endregion
    }
}
