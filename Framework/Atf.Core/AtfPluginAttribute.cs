//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// This attribute should be placed on an assembly (usually in the AssemblyInfo file) to 
    /// mark it as an ATF plugin. The optional userdata is for extensibility and is not
    /// currently used. Add this attribute to the AssemblyInfo file of the plugins, e.g.
    /// [assembly: AtfPluginAttribute()]</summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class AtfPluginAttribute : Attribute
    {
        /// <summary>
        /// Constructor</summary>
        public AtfPluginAttribute()
        {
        }

        /// <summary>
        /// Constructor with user data</summary>
        /// <param name="info">User data</param>
        public AtfPluginAttribute(string info)
        {
            m_info = info;
        }

        /// <summary>
        /// Gets the user data</summary>
        public string Info
        {
            get { return m_info; }
        }

        private readonly string m_info;
    }
}
