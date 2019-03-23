using System;

namespace UniEasy.DI
{
    public class NonLazyBinder
    {
        public NonLazyBinder(BindInfo bindInfo)
        {
            BindInfo = bindInfo;
        }

        protected BindInfo BindInfo;

        public void NonLazy()
        {
            BindInfo.NonLazy = true;
        }
    }
}
