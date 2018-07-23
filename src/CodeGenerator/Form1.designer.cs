namespace CodeGenerator
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cb_dbConnectionName = new System.Windows.Forms.ComboBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnOpenDir = new System.Windows.Forms.Button();
            this.btnChangeDir = new System.Windows.Forms.Button();
            this.txtSavePath = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.generateProcess = new System.Windows.Forms.ProgressBar();
            this.lb_selectedTables = new System.Windows.Forms.ListBox();
            this.lb_tables = new System.Windows.Forms.ListBox();
            this.txtSearchbox = new System.Windows.Forms.TextBox();
            this.tableTrees = new System.Windows.Forms.TreeView();
            this.txtNamespace = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cb_dbConnectionName);
            this.groupBox1.Controls.Add(this.btnConnect);
            this.groupBox1.Controls.Add(this.txtConnectionString);
            this.groupBox1.Controls.Add(this.btnDelete);
            this.groupBox1.Controls.Add(this.btnSave);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(628, 124);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "数据库";
            // 
            // cb_dbConnectionName
            // 
            this.cb_dbConnectionName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_dbConnectionName.FormattingEnabled = true;
            this.cb_dbConnectionName.Location = new System.Drawing.Point(44, 20);
            this.cb_dbConnectionName.Name = "cb_dbConnectionName";
            this.cb_dbConnectionName.Size = new System.Drawing.Size(406, 20);
            this.cb_dbConnectionName.TabIndex = 6;
            this.cb_dbConnectionName.SelectedIndexChanged += new System.EventHandler(this.cb_dbConnectionName_SelectedIndexChanged);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(456, 52);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(156, 58);
            this.btnConnect.TabIndex = 5;
            this.btnConnect.Text = "连接/测试";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Location = new System.Drawing.Point(44, 52);
            this.txtConnectionString.Multiline = true;
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(406, 58);
            this.txtConnectionString.TabIndex = 4;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(537, 18);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(456, 18);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "新增";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "名称";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnOpenDir);
            this.groupBox2.Controls.Add(this.btnChangeDir);
            this.groupBox2.Controls.Add(this.txtSavePath);
            this.groupBox2.Location = new System.Drawing.Point(3, 142);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(628, 69);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "文件目录";
            // 
            // btnOpenDir
            // 
            this.btnOpenDir.Location = new System.Drawing.Point(537, 25);
            this.btnOpenDir.Name = "btnOpenDir";
            this.btnOpenDir.Size = new System.Drawing.Size(75, 23);
            this.btnOpenDir.TabIndex = 4;
            this.btnOpenDir.Text = "打开";
            this.btnOpenDir.UseVisualStyleBackColor = true;
            this.btnOpenDir.Click += new System.EventHandler(this.btnOpenDir_Click);
            // 
            // btnChangeDir
            // 
            this.btnChangeDir.Location = new System.Drawing.Point(456, 25);
            this.btnChangeDir.Name = "btnChangeDir";
            this.btnChangeDir.Size = new System.Drawing.Size(75, 23);
            this.btnChangeDir.TabIndex = 3;
            this.btnChangeDir.Text = "更换";
            this.btnChangeDir.UseVisualStyleBackColor = true;
            this.btnChangeDir.Click += new System.EventHandler(this.btnChangeDir_Click);
            // 
            // txtSavePath
            // 
            this.txtSavePath.Location = new System.Drawing.Point(44, 28);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.Size = new System.Drawing.Size(406, 21);
            this.txtSavePath.TabIndex = 0;
            this.txtSavePath.Text = "E:\\CodeTemp";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.txtNamespace);
            this.groupBox4.Controls.Add(this.btnGenerate);
            this.groupBox4.Controls.Add(this.generateProcess);
            this.groupBox4.Controls.Add(this.lb_selectedTables);
            this.groupBox4.Controls.Add(this.lb_tables);
            this.groupBox4.Controls.Add(this.txtSearchbox);
            this.groupBox4.Location = new System.Drawing.Point(637, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(373, 566);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "数据表";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(255, 523);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(110, 37);
            this.btnGenerate.TabIndex = 4;
            this.btnGenerate.Text = "生成";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // generateProcess
            // 
            this.generateProcess.Location = new System.Drawing.Point(6, 494);
            this.generateProcess.Name = "generateProcess";
            this.generateProcess.Size = new System.Drawing.Size(359, 23);
            this.generateProcess.TabIndex = 3;
            // 
            // lb_selectedTables
            // 
            this.lb_selectedTables.FormattingEnabled = true;
            this.lb_selectedTables.ItemHeight = 12;
            this.lb_selectedTables.Location = new System.Drawing.Point(194, 52);
            this.lb_selectedTables.Name = "lb_selectedTables";
            this.lb_selectedTables.Size = new System.Drawing.Size(171, 436);
            this.lb_selectedTables.TabIndex = 2;
            this.lb_selectedTables.DoubleClick += new System.EventHandler(this.lb_selectedTables_DoubleClick);
            // 
            // lb_tables
            // 
            this.lb_tables.FormattingEnabled = true;
            this.lb_tables.ItemHeight = 12;
            this.lb_tables.Location = new System.Drawing.Point(6, 52);
            this.lb_tables.Margin = new System.Windows.Forms.Padding(10);
            this.lb_tables.Name = "lb_tables";
            this.lb_tables.Size = new System.Drawing.Size(182, 436);
            this.lb_tables.TabIndex = 1;
            this.lb_tables.DoubleClick += new System.EventHandler(this.lb_tables_DoubleClick);
            // 
            // txtSearchbox
            // 
            this.txtSearchbox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtSearchbox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtSearchbox.Location = new System.Drawing.Point(6, 20);
            this.txtSearchbox.Name = "txtSearchbox";
            this.txtSearchbox.Size = new System.Drawing.Size(359, 21);
            this.txtSearchbox.TabIndex = 0;
            this.txtSearchbox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearchbox_KeyDown);
            this.txtSearchbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearchbox_KeyPress);
            // 
            // tableTrees
            // 
            this.tableTrees.Location = new System.Drawing.Point(3, 221);
            this.tableTrees.Name = "tableTrees";
            this.tableTrees.Size = new System.Drawing.Size(628, 355);
            this.tableTrees.TabIndex = 4;
            this.tableTrees.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.tableTrees_DrawNode);
            // 
            // txtNamespace
            // 
            this.txtNamespace.Location = new System.Drawing.Point(69, 532);
            this.txtNamespace.Name = "txtNamespace";
            this.txtNamespace.Size = new System.Drawing.Size(180, 21);
            this.txtNamespace.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 535);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "命名空间";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1014, 581);
            this.Controls.Add(this.tableTrees);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "代码生成器_测试版V1.7";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cb_dbConnectionName;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnOpenDir;
        private System.Windows.Forms.Button btnChangeDir;
        private System.Windows.Forms.TextBox txtSavePath;
        private System.Windows.Forms.ListBox lb_selectedTables;
        private System.Windows.Forms.ListBox lb_tables;
        private System.Windows.Forms.TextBox txtSearchbox;
        private System.Windows.Forms.ProgressBar generateProcess;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.TreeView tableTrees;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNamespace;
    }
}

