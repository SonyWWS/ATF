//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI Anim attribute data</summary>
    public class AnimData : DomNodeAdapter, IAnimData
    {
        /// <summary>
        /// Gets or sets the number of keys</summary>
        public int NumKeys
        {
            get { return GetAttribute<int>(Schema.animChannelType_animData.numKeysAttribute); }
            set { SetAttribute(Schema.animChannelType_animData.numKeysAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the key stride</summary>
        public int KeyStride
        {
            get { return GetAttribute<int>(Schema.animChannelType_animData.keyStrideAttribute); }
            set { SetAttribute(Schema.animChannelType_animData.keyStrideAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the time offset</summary>
        public float TimeOffset
        {
            get { return GetAttribute<float>(Schema.animChannelType_animData.timeOffsetAttribute); }
            set { SetAttribute(Schema.animChannelType_animData.timeOffsetAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the duration</summary>
        public float Duration
        {
            get { return GetAttribute<float>(Schema.animChannelType_animData.durationAttribute); }
            set { SetAttribute(Schema.animChannelType_animData.durationAttribute, value); }
        }

        /// <summary>
        /// Gets the child AnimData list</summary>
        public float[] KeyValues
        {
            get { return GetAttribute<float[]>(Schema.animChannelType_animData.keyValuesAttribute); }
            set { SetAttribute(Schema.animChannelType_animData.keyValuesAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the key time array</summary>
        public float[] KeyTimes
        {
            get { return GetAttribute<float[]>(Schema.animChannelType_animData.keyTimesAttribute); }
            set { SetAttribute(Schema.animChannelType_animData.keyTimesAttribute, value); }
        }

        /// <summary>
        /// Get the entire array of tangents</summary>
        public float[] Tangents
        {
            get { return GetAttribute<float[]>(Schema.animChannelType_animData.tangentsAttribute); }
            set { SetAttribute(Schema.animChannelType_animData.tangentsAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the interpolation type string</summary>
        public string InterpolationType
        {
            get { return GetAttribute<string>(Schema.animChannelType_animData.interpAttribute); }
            set { SetAttribute(Schema.animChannelType_animData.interpAttribute, value); }
        }
    };

    /// <summary>
    /// ATGI animation channel</summary>
    public class AnimChannel : DomNodeAdapter, IAnimChannel //, IHierarchical, IListable
    {
        /// <summary>
        /// Gets and sets the AnimChannel name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.animChannelType.nameAttribute); }
            set { SetAttribute(Schema.animChannelType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the child AnimData object - there is only one</summary>
        public IAnimData Data
        {
            get { return GetChild<IAnimData>(Schema.animChannelType.animDataChild); }
        }

        /// <summary>
        /// Gets or sets the channel name</summary>
        public string Channel
        {
            get { return GetAttribute<string>(Schema.animChannelType.channelAttribute); }
            set { SetAttribute(Schema.animChannelType.channelAttribute, value); }
        }

        /// <summary>
        /// Gets the input object name</summary>
        public string InputObject
        {
            get { return GetAttribute<string>(Schema.animChannelType.inputObjectAttribute); }
            set { SetAttribute(Schema.animChannelType.inputObjectAttribute, value); }
        }

        /// <summary>
        /// Gets the input channel name</summary>
        public string InputChannel
        {
            get { return GetAttribute<string>(Schema.animChannelType.inputChannelAttribute); }
            set { SetAttribute(Schema.animChannelType.inputChannelAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the target DOM object</summary>
        public object Target
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the value index</summary>
        public int ValueIndex
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets this animation to be enabled or disabled</summary>
        public bool Enabled
        {
            get { return m_enabled; }
            set { m_enabled = value; }
        }

        private bool m_enabled;
    }
}

