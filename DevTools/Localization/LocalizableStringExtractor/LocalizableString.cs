//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LocalizableStringExtractor
{
    /// <summary>
    /// Contains the data for each original word or phrase that is to be translated.</summary>
    public class LocalizableString
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableString"/> class</summary>
        /// <param name="text">The original text to be translated</param>
        /// <param name="context">The context string, to help the human translator determine
        /// how 'text' is used. Can be the empty string.</param>
        /// <param name="translation">The translation, for when this object is filled by
        /// reading a Localization.xml file. Can be the empty string, in which case
        /// Translation will be the same as Text.</param>
        public LocalizableString(string text, string context, string translation = "")
        {
            Text = text;
            Context = context;
            Translation = string.IsNullOrEmpty(translation) ? text : translation;
        }

        /// <summary>
        /// Gets the original text to be translated</summary>
        public readonly string Text;

        /// <summary>
        /// Gets the context</summary>
        public readonly string Context;

        /// <summary>
        /// Gets the translation, if any. Is intended to be filled in by reading the
        /// Localization.xml file, since the translation can't be in the source code.
        /// This read-only field is ignored by Equals() and GetHashCode().</summary>
        public readonly string Translation;

        /// <summary>
        /// Determines whether one LocalizableString is equal to another, by comparing their
        /// Text and Context fields. The Translation field is ignored.</summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>'true' if the specified object is equal to this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as LocalizableString;
            return
                other != null &&
                other.Text == Text &&
                other.Context == Context;
        }

        /// <summary>
        /// Returns a hash code for this instance</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            // http://stackoverflow.com/questions/18065251/concise-way-to-combine-field-hashcodes
            // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Text.GetHashCode();
                hash = hash * 23 + Context.GetHashCode();
                return hash;
            }
        }
    }
}
