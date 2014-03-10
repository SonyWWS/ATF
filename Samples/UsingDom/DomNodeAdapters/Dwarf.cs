namespace UsingDom
{
    /// <summary>
    /// Dwarf game object</summary>
    public class Dwarf : GameObject
    {

        /// <summary>
        /// Gets and sets the Dwarf's age</summary>
        public int Age
        {
            get { return GetAttribute<int>(GameSchema.dwarfType.ageAttribute); }
            set { SetAttribute(GameSchema.dwarfType.ageAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the Dwarf's experience level</summary>
        public int Experience
        {
            get { return GetAttribute<int>(GameSchema.dwarfType.experienceAttribute); }
            set { SetAttribute(GameSchema.dwarfType.experienceAttribute, value); }
        }
    }
}
