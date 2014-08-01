//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications.WebServices
{
    /// <summary>
    /// Form for submitting bugs to the SourceForge bug tracker.
    /// </summary>
    /// <remarks>
    /// Note that each project has a unique identifier used to
    /// map to the SourceForge project (for example, "com.scea.screamtool").
    /// This identifier can be specified with the ProjectMappingAttribute.
    /// The mapping itself is set up at
    ///   http://ship.scea.com/appupdate
    /// </remarks>
    public partial class FeedbackForm : CommonDialog
    {
        public FeedbackForm()
        {
            InitializeComponent();
        }
    }

    internal class FeedbackFormViewModel : DialogViewModelBase
    {
        public FeedbackFormViewModel()
        {
            Title = "Send Feedback".Localize();
        }
    }
}
