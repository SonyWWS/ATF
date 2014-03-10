//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA animation clip</summary>
    class AnimationClip : DomNodeAdapter, IAnimClip
    {

        #region IAnimClip Members

        /// <summary>
        /// Gets the child Anim list</summary>
        public IList<IAnim> Anims
        {
            get
            {
                var animations = new List<Animation>();
                foreach (DomNode instance in GetChildList<DomNode>(Schema.animation_clip.instance_animationChild))
                    animations.Add(instance.GetAttribute(Schema.InstanceWithExtra.urlAttribute).As<Animation>());

                return new List<IAnim>(animations.AsIEnumerable<IAnim>());
            }
        }

        #endregion

        #region INameable Members

        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.animation_clip.nameAttribute); }
            set { DomNode.SetAttribute(Schema.animation_clip.nameAttribute, value); }
        }

        #endregion
    }
}
