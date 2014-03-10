using System;
using System.Windows.Forms;
using Sce.Atf;

namespace UsingDirect2D
{
    /// <summary>
    /// This sample application demonstrates how to use Direct2D.
    /// This sample shows how to use various ATF Direct2D classes, such as D2dHwndGraphics, D2dTextFormat and D2dBitmapBrush.
    /// It also demonstrates ATF vector math classes, such as Matrix3x2F.
    /// It does not use MEF.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-Using-Direct2D-Sample. </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application</summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mainForm = new Form1
            {
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };
            Application.Run(mainForm);
        }
    }
}
