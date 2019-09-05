using CodeGenerator.Core.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeGenerator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string startMode = args.Length > 0 ? args[0] : null;
            if (startMode == "/quiet" || startMode == "/vs")//控制台静默启动，或者作为外部工具在visual studio使用
            {
                string saveDirectory = null, db = null, table = null, templatePath = null, @namespace = null;
                try
                {
                    if (startMode == "/quiet")
                    {
                        saveDirectory = args[1]; db = args[2]; table = args[3]; templatePath = args[4]; @namespace = args[5];
                    }
                    else
                    {
                        var projFilePath = args[1];
                        var itemFile = args[2];
                        saveDirectory = Path.GetDirectoryName(itemFile);
                        db = args[3];
                        table = Path.GetFileName(itemFile).Replace(".generate", "").Replace(".cs", "");
                        @namespace = Form1.ReadNamespaceFromProjectXml(projFilePath);
                        templatePath = args[3];
                    }
                    //build as windows application, run from command line
                    //AttachConsole(-1);
                    //AllocConsole();
                    string errorMsg;
                    if (!Regex.IsMatch(saveDirectory, @"^[a-zA-Z]:\\"))
                    {
                        Console.WriteLine("文件保存路径无效");
                        return;
                    }
                    ConnectionSetting setting = new ConnectionSettingCollection()[db];
                    if (setting == null)
                    {
                        Console.WriteLine("数据库连接字符串无效");
                        return;
                    }
                    IList selectedItems = new List<string>();
                    selectedItems.Add(table);
                    if (!Form1.GenerateAll(saveDirectory, setting, selectedItems, templatePath, @namespace, out errorMsg))
                    {
                        Console.WriteLine("代码生成失败");
                        Console.WriteLine(errorMsg);
                    }
                    else
                    {
                        Console.WriteLine($"代码生成成功,Table={table},savingPath={saveDirectory}");
                    }
                }
                finally
                {
                    //FreeConsole();
                }

            }
            else
            {
                Console.WriteLine("start gui application");
                //string[] newArgs = null;
                //启动参数不满足静默模式，启动GUI程序
                if (args != null && args.Length != 3)
                {
                    args = new string[0];
                }
                //else
                //{
                //    newArgs = args;
                //}
                //build as Console Application, run GUI application
                //use FreeConsole to hide command line window, but still have a cmd window flash out
                FreeConsole();
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1(args));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("start gui application failed");
                    Console.WriteLine(ex.Message);
                }
            }
        }
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool AttachConsole(int processId);

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
    }
}
