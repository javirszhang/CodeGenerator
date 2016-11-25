using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeGenerator.Core.Entities;
using CodeGenerator.Core;

namespace CodeGenerator
{
    public partial class NewConnection : Form
    {
        public NewConnection()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtConnectionName.Text.Trim();
            string type = cb_dbtype.SelectedItem.ToString();
            string connectionstring = txtConnectionString.Text.Trim();
            ConnectionSettingCollection csc = new ConnectionSettingCollection();
            if (csc[name] != null)
            {
                MessageBox.Show("此名称已存在，请更换名称");
                return;
            }
            ConnectionSetting setting = new ConnectionSetting();
            setting.ConnectionString = connectionstring;
            setting.Name = name;
            setting.Provider = (DatabaseType)Enum.Parse(typeof(DatabaseType),type);
            csc.Add(setting);
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void NewConnection_Load(object sender, EventArgs e)
        {
            cb_dbtype.DataSource = Enum.GetNames(typeof(DatabaseType));
            cb_dbtype.SelectedIndex = 0;
        }

        private void btnTestConnect_Click(object sender, EventArgs e)
        {
            string name = txtConnectionName.Text.Trim();
            string type = cb_dbtype.SelectedItem.ToString();
            string connectionstring = txtConnectionString.Text.Trim();
            ConnectionSetting setting = new ConnectionSetting();
            setting.ConnectionString = connectionstring;
            setting.Name = name;
            setting.Provider = (DatabaseType)Enum.Parse(typeof(DatabaseType),type);
            if (DatabaseResolver.TestConnection(setting))
                MessageBox.Show("测试连接成功");
            else
                MessageBox.Show("连接数据库失败");
            
        }
    }
}
