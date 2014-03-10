using System.Collections.Generic;

using Sce.Atf.Dom;

namespace UsingDom
{
    /// <summary>
    /// Adapts DomNode to game</summary>
    public class Game : DomNodeAdapter
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the game's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            m_gameObjects = GetChildList<GameObject>(GameSchema.gameType.gameObjectChild);
        }

        /// <summary>
        /// Gets or sets the game's name</summary>
        public string Name
        {
            get { return GetAttribute<string>(GameSchema.gameType.nameAttribute); }
            set { SetAttribute(GameSchema.gameType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets a list of the game objects</summary>
        public IList<GameObject> GameObjects
        {
            get { return m_gameObjects; }
        }

        private IList<GameObject> m_gameObjects;
    }
}
