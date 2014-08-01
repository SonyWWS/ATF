//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf
{
    public class FreezableCursor : Freezable
    {
        public System.Windows.Input.Cursor Cursor { get; set; }

        protected override Freezable CreateInstanceCore()
        {
            return new FreezableCursor();
        }
    }
}
