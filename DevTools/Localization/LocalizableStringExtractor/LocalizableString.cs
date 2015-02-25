//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace LocalizableStringExtractor
{
    /// <summary>
    /// Contains the data for each English word or phrase that is to be translated.</summary>
    public class LocalizableString : IComparable<LocalizableString>
    {
        public LocalizableString(string text, string context)
        {
            Text = text;
            Context = context;
        }
        public readonly string Text;
        public readonly string Context;

        #region IComparable

        public int CompareTo(LocalizableString other)
        {
            return Text.CompareTo(other.Text);
        }

        #endregion

        public override bool Equals(object obj)
        {
            var other = obj as LocalizableString;
            return
                other != null &&
                other.Text == Text &&
                other.Context == Context;
        }

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
