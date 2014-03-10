//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Adaptation;

namespace UnitTests.Atf.Adaptation
{
    public class SimpleDecoratable : IDecoratable
    {
        #region IDecoratable Members

        IEnumerable<object> IDecoratable.GetDecorators(Type type)
        {
            AsAllCalled = true;

            if (type == typeof(string))
                return Decorators;
            return EmptyEnumerable<object>.Instance;
        }

        #endregion

        public bool AsAllCalled;
        public static readonly string[] Decorators = new string[] { "a", "b" }; 
    }
}
