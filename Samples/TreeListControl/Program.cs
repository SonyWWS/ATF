//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace TreeListControlDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.</summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
          
            var mainform = new Form1();
            var automation = new  AutomationSetup(mainform);
            Application.Run(mainform);
        }   
    }
}
