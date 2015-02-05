//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;

namespace FileExplorerSample
{
    /// <summary>
    /// Customized MainForm with a split panel for our two views</summary>
    [Export(typeof(Form))]
    [Export(typeof(MainForm))]
    [Export(typeof(Sce.Atf.Applications.IMainWindow))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MainForm : Sce.Atf.Applications.MainForm
    {
        /// <summary>
        /// Constructor</summary>
        public MainForm()
        {
            Text = "File Explorer Sample".Localize();

            m_splitContainer = new SplitContainer();
            m_splitContainer.Orientation = Orientation.Vertical;
            m_splitContainer.Dock = DockStyle.Fill;
            Controls.Add(m_splitContainer);
        }

        /// <summary>
        /// Gets the MainForm's SplitContainer, which is a control consisting of a movable bar that 
        /// divides a container's display area into two resizable panels</summary>
        public SplitContainer SplitContainer
        {
            get { return m_splitContainer; }
        }

        private SplitContainer m_splitContainer;
    }
}