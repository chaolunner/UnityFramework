using System.Collections.Generic;
using System;

namespace UniEasy.DI
{
    public class SingletonProviderCreator
    {
        readonly Dictionary<SingletonId, ProviderInfo> providerMap = new Dictionary<SingletonId, ProviderInfo>();
        readonly DiContainer container;

        public SingletonProviderCreator(DiContainer container)
        {
            this.container = container;
        }

        public IProvider GetOrCreateProvider(SingletonId singletonId, Func<DiContainer, Type, IProvider> providerCreator)
        {
            ProviderInfo providerInfo;

            if (providerMap.TryGetValue(singletonId, out providerInfo))
            {

            }
            else
            {
                providerInfo = new ProviderInfo(
                    new CachedProvider(
                        providerCreator(container, singletonId.ConcreteType)));
                providerMap.Add(singletonId, providerInfo);
            }

            return providerInfo.Provider;
        }

        public class ProviderInfo
        {
            public ProviderInfo(CachedProvider provider)
            {
                Provider = provider;
            }

            public CachedProvider Provider
            {
                get;
                private set;
            }
        }
    }
}
