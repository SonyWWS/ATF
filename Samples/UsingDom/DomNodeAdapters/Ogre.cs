namespace UsingDom
{
    /// <summary>
    /// Ogre game object</summary>
    public class Ogre : GameObject
    {
        /// <summary>
        /// Gets and sets the Ogre's size</summary>
        public int Size
        {
            get { return GetAttribute<int>(GameSchema.ogreType.sizeAttribute); }
            set { SetAttribute(GameSchema.ogreType.sizeAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the Ogre's strength</summary>
        public int Strength
        {
            get { return GetAttribute<int>(GameSchema.ogreType.strengthAttribute); }
            set { SetAttribute(GameSchema.ogreType.strengthAttribute, value); }
        }
    }
}
