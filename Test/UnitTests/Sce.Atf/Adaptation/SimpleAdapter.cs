//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Adaptation;

namespace UnitTests.Atf.Adaptation
{
    public class SimpleAdapter : Adapter, ISimpleInterface
    {
        public SimpleAdapter()
        {
        }

        public SimpleAdapter(object adaptee)
            : base(adaptee)
        {
        }

        protected override void OnAdapteeChanged(object oldAdaptee)
        {
            OnAdapteeChangedCalled = true;
        }

        protected override object Adapt(Type type)
        {
            AdaptCalled = true;
            return base.Adapt(type);
        }

        public bool OnAdapteeChangedCalled;
        public bool AdaptCalled;
    }
}
