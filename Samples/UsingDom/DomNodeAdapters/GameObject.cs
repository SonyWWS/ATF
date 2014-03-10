using Sce.Atf.Dom;

namespace UsingDom
{
    /// <summary>
    /// Game object</summary>
    public class GameObject : DomNodeAdapter
    {
        /// <summary>
        /// Gets or sets game object name</summary>
        public string Name
        {
            get { return GetAttribute<string>(GameSchema.gameObjectType.nameAttribute); }
            set { SetAttribute(GameSchema.gameObjectType.nameAttribute, value); }
        }
    }
}
