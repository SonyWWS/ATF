//Sony Computer Entertainment Confidential

using System.Collections.Generic;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// Group of objects</summary>
    public class Group
    {
        /// <summary>
        /// FaceSet dictionary</summary>
        public Dictionary<string, FaceSet> FaceSets = new Dictionary<string, FaceSet>();
        /// <summary>
        /// Group name</summary>
        public string Name = "default";
    }
}
