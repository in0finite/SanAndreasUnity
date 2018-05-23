using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public interface IPlaceholderFactory : IValidatable
    {
    }

    // Placeholder factories can be used to choose a creation method in an installer, using FactoryBinder
    public abstract class PlaceholderFactory<TValue> : IPlaceholderFactory
    {
        private IProvider _provider;
        private InjectContext _injectContext;

        [Inject]
        private void Construct(IProvider provider, InjectContext injectContext)
        {
            Assert.IsNotNull(provider);
            Assert.IsNotNull(injectContext);

            _provider = provider;
            _injectContext = injectContext;
        }

        protected TValue CreateInternal(List<TypeValuePair> extraArgs)
        {
            try
            {
                var result = _provider.GetInstance(_injectContext, extraArgs);

                Assert.That(result == null || result.GetType().DerivesFromOrEqual<TValue>());

                return (TValue)result;
            }
            catch (Exception e)
            {
                throw new ZenjectException(
                    "Error during construction of type '{0}' via {1}.Create method!".Fmt(typeof(TValue), this.GetType().Name()), e);
            }
        }

        public virtual void Validate()
        {
            _provider.GetInstance(
                _injectContext, ValidationUtil.CreateDefaultArgs(ParamTypes.ToArray()));
        }

        protected abstract IEnumerable<Type> ParamTypes
        {
            get;
        }
    }
}