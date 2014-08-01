//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Sce.Atf.Wpf.Skins
{
    /// <summary>
    /// Abstract base class to assist with loading skin resources</summary>
    public abstract class Skin
    {
        /// <summary>
        /// Gets the name of the skin</summary>
        public string Name { get; private set; }

        /// <summary>
        /// Loads the skin's resources</summary>
        public virtual void Load()
        {
            if (Resources.Count != 0)
            {
                // Already loaded
                return;
            }

            try
            {
                LoadResources();
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            foreach (var skinResource in Resources)
            {
                Application.Current.Resources.MergedDictionaries.Add(skinResource);
            }
        }

        /// <summary>
        /// Unloads the skin's resources</summary>
        public virtual void Unload()
        {
            foreach (var skinResource in Resources)
            {
                Application.Current.Resources.MergedDictionaries.Remove(skinResource);
            }

            Resources.Clear();
        }

        /// <summary>
        /// Static instance of a null skin</summary>
        public static readonly Skin Null = new NullSkin();

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">The name of the skin</param>
        protected Skin(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the skin's resources. This is an empty list if the resources
        /// have not yet been loaded.</summary>
        protected List<ResourceDictionary> Resources
        {
            get { return m_resources; }
        }

        /// <summary>
        /// Load the skin-specific resources</summary>
        protected abstract void LoadResources();

        private Skin() { }

        private readonly List<ResourceDictionary> m_resources = new List<ResourceDictionary>();

        private sealed class NullSkin : Skin
        {
            protected override void LoadResources() { }
        }

    }
}
