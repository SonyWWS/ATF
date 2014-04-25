//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace Sce.Atf.DragDrop
{
    internal enum DropImageType : int
    {
        Invalid = -1,
        None = 0,
        Copy = (int)DragDropEffects.Copy,
        Move = (int)DragDropEffects.Move,
        Link = (int)DragDropEffects.Link,
        Label = 6,
        Warning = 7
    }
}