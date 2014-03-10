using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocalizableStringExtractor
{
    /// <summary>
    /// Contains the data for each English word or phrase that is to be translated.</summary>
    public class LocalizableString
    {
        public LocalizableString(string text, string context)
        {
            Text = text;
            Context = context;
        }
        public readonly string Text;
        public readonly string Context;
    }
}
