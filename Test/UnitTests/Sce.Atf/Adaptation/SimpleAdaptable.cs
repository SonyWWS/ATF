//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Adaptation;

namespace UnitTests.Atf.Adaptation
{
    public class SimpleAdaptable : IAdaptable
    {
        #region IAdaptable Members

        object IAdaptable.GetAdapter(Type type)
        {
            AsCalled = true;
            if (type == typeof(string))
                return Adapter;
            return null;
        }

        #endregion

        public bool AsCalled;
        public static readonly object Adapter = "a";
    }
}
