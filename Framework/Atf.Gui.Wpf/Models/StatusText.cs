//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Text status item</summary>
    public class StatusText : StatusItem
    {
        /// <summary>
        /// Constructor with minimum width</summary>
        /// <param name="minWidth">Minimum width of status text item</param>
        public StatusText(int minWidth)
        {
            MinWidth = minWidth;
        }

        /// <summary>
        /// Constructor with text and minimum width</summary>
        /// <param name="text">Text in text status item</param>
        /// <param name="minWidth">Minimum width of status text item</param>
        public StatusText(string text, int minWidth)
        {
            m_text = text;
            MinWidth = minWidth;
        }

        /// <summary>
        /// Gets minimum width of status text item</summary>
        public int MinWidth { get; private set; }

        /// <summary>
        /// Gets or sets text in text status item</summary>
        public string Text
        {
            get { return m_text; }
            set
            {
                m_text = value;
                OnPropertyChanged(s_textArgs);
            }
        }
        private string m_text;

        /// <summary>
        /// Gets or sets text color</summary>
        public Brush ForeColor
        {
            get { return m_foreColor; }
            set
            {
                m_foreColor = value;
                OnPropertyChanged(s_foreColorArgs);
            }
        }
        private Brush m_foreColor;

        private static readonly PropertyChangedEventArgs s_textArgs
            = ObservableUtil.CreateArgs<StatusText>(x => x.Text);
        private static readonly PropertyChangedEventArgs s_foreColorArgs
            = ObservableUtil.CreateArgs<StatusText>(x => x.ForeColor);
    }
}
