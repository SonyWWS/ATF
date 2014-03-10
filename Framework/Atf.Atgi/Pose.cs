//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI Pose</summary>
    public class Pose : DomNodeAdapter, IPose
    {
        /// <summary>
        /// Gets and sets the pose name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.poseType.nameAttribute); }
            set { SetAttribute(Schema.poseType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the bind pose attribute</summary>
        public bool BindPose
        {
            get { return GetAttribute<bool>(Schema.poseType.bindPoseAttribute); }
            set { SetAttribute(Schema.poseType.bindPoseAttribute, value); }
        }

        /// <summary>
        /// Gets the list of pose elements</summary>
        public IList<IPoseElement> Elements
        {
            get { return GetChildList<IPoseElement>(Schema.poseType.elementChild); }
        }
    }
}

