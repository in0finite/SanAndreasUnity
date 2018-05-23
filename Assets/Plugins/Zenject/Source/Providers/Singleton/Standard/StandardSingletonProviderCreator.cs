using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class StandardSingletonProviderCreator
    {
        private readonly SingletonMarkRegistry _markRegistry;
        private readonly Dictionary<SingletonId, ProviderInfo> _providerMap = new Dictionary<SingletonId, ProviderInfo>();
        private readonly DiContainer _container;

        public StandardSingletonProviderCreator(
            DiContainer container,
            SingletonMarkRegistry markRegistry)
        {
            _markRegistry = markRegistry;
            _container = container;
        }

        public IProvider GetOrCreateProvider(
            StandardSingletonDeclaration dec, Func<DiContainer, Type, IProvider> providerCreator)
        {
            // These ones are actually fine when used with Bind<GameObject>() (see TypeBinderBase.ToPrefabSelf)
            //Assert.IsNotEqual(dec.Type, SingletonTypes.ToPrefab);
            //Assert.IsNotEqual(dec.Type, SingletonTypes.ToPrefabResource);

            Assert.IsNotEqual(dec.Type, SingletonTypes.FromSubContainerInstaller);
            Assert.IsNotEqual(dec.Type, SingletonTypes.FromSubContainerMethod);
            Assert.IsNotEqual(dec.Type, SingletonTypes.FromSubContainerPrefab);
            Assert.IsNotEqual(dec.Type, SingletonTypes.FromSubContainerPrefabResource);

            _markRegistry.MarkSingleton(dec.Id, dec.Type);

            ProviderInfo providerInfo;

            if (_providerMap.TryGetValue(dec.Id, out providerInfo))
            {
                Assert.That(providerInfo.Type == dec.Type,
                    "Cannot use both '{0}' and '{1}' for the same dec.Type/ConcreteIdentifier!", providerInfo.Type, dec.Type);

                Assert.That(providerInfo.Arguments.Count == dec.Arguments.Count,
                    "Invalid use of binding '{0}'.  Ambiguous set of creation properties found (argument length mismatch)", dec.Type);

                foreach (var pair in providerInfo.Arguments.Zipper(dec.Arguments))
                {
                    var arg1 = pair.First;
                    var arg2 = pair.Second;

                    Assert.That(arg1.Type == arg2.Type && object.Equals(arg1.Value, arg2.Value),
                        "Invalid use of binding '{0}'.  Ambiguous set of creation properties found (argument value mismatch)", dec.Type);
                }

                Assert.That(object.Equals(providerInfo.SingletonSpecificId, dec.SpecificId),
                    "Invalid use of binding '{0}'.  Found ambiguous set of creation properties.", dec.Type);
            }
            else
            {
                providerInfo = new ProviderInfo(
                    dec.Type,
                    new CachedProvider(
                        providerCreator(_container, dec.Id.ConcreteType)),
                    dec.SpecificId,
                    dec.Arguments);

                _providerMap.Add(dec.Id, providerInfo);
            }

            return providerInfo.Provider;
        }

        public class ProviderInfo
        {
            public ProviderInfo(
                SingletonTypes type,
                CachedProvider provider,
                object singletonSpecificId,
                List<TypeValuePair> arguments)
            {
                Type = type;
                Provider = provider;
                SingletonSpecificId = singletonSpecificId;
                Arguments = arguments;
            }

            public List<TypeValuePair> Arguments
            {
                get;
                private set;
            }

            public object SingletonSpecificId
            {
                get;
                private set;
            }

            public SingletonTypes Type
            {
                get;
                private set;
            }

            public CachedProvider Provider
            {
                get;
                private set;
            }
        }
    }
}