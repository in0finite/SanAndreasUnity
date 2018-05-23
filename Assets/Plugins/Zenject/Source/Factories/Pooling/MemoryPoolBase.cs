using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class PoolExceededFixedSizeException : Exception
    {
        public PoolExceededFixedSizeException(string errorMessage)
            : base(errorMessage)
        {
        }
    }

    [Serializable]
    public class MemoryPoolSettings
    {
        public int InitialSize;
        public PoolExpandMethods ExpandMethod;
    }

    public abstract class MemoryPoolBase<TContract> : IValidatable, IMemoryPool
    {
        private Stack<TContract> _inactiveItems;
        private IFactory<TContract> _factory;
        private MemoryPoolSettings _settings;

        private int _activeCount;

        [Inject]
        private void Construct(
            IFactory<TContract> factory,
            DiContainer container,
            MemoryPoolSettings settings)
        {
            _settings = settings;
            _factory = factory;

            _inactiveItems = new Stack<TContract>(settings.InitialSize);

            if (!container.IsValidating)
            {
                for (int i = 0; i < settings.InitialSize; i++)
                {
                    _inactiveItems.Push(AllocNew());
                }
            }
        }

        public IEnumerable<TContract> InactiveItems
        {
            get { return _inactiveItems; }
        }

        public int NumTotal
        {
            get { return NumInactive + NumActive; }
        }

        public int NumInactive
        {
            get { return _inactiveItems.Count; }
        }

        public int NumActive
        {
            get { return _activeCount; }
        }

        public Type ItemType
        {
            get { return typeof(TContract); }
        }

        public void Despawn(TContract item)
        {
            if (_inactiveItems.Contains(item))
            {
                throw Assert.CreateException(
                    "Tried to return an item to pool {0} twice", this.GetType());
            }

            _activeCount--;

            _inactiveItems.Push(item);

            OnDespawned(item);
        }

        private TContract AllocNew()
        {
            try
            {
                var item = _factory.Create();
                if (item == null)
                {
                    throw Assert.CreateException(
                        "Factory '{0}' returned null value when creating via {1}!", _factory.GetType(), this.GetType());
                }
                OnCreated(item);
                return item;
            }
            catch (Exception e)
            {
                throw new ZenjectException(
                    "Error during construction of type '{0}' via {1}.Create method!".Fmt(
                        typeof(TContract), this.GetType().Name()), e);
            }
        }

        void IValidatable.Validate()
        {
            try
            {
                _factory.Create();
            }
            catch (Exception e)
            {
                throw new ZenjectException(
                    "Validation for factory '{0}' failed".Fmt(this.GetType()), e);
            }
        }

        protected TContract GetInternal()
        {
            TContract item;

            if (_inactiveItems.Count == 0)
            {
                ExpandPool();
                Assert.That(!_inactiveItems.IsEmpty());
            }

            item = _inactiveItems.Pop();

            _activeCount++;

            OnSpawned(item);
            return item;
        }

        /// <summary>
        /// Expands the pool by the additional size.
        ///
        /// This bypasses the configured expansion method (OneAtATime or Doubling), but still enforces the Fixed size
        /// constraint.
        /// </summary>
        /// <param name="additionalSize">The additional number of items to allocate in the pool.</param>
        /// <exception cref="PoolExceededFixedSizeException">if the pool is configured with a fixed size.</exception>
        public void ExpandPoolBy(int additionalSize)
        {
            if (_settings.ExpandMethod == PoolExpandMethods.Fixed)
            {
                throw new PoolExceededFixedSizeException(
                    "Pool factory '{0}' exceeded its max size of '{1}'!"
                    .Fmt(this.GetType(), NumTotal));
            }

            for (int i = 0; i < additionalSize; i++)
            {
                _inactiveItems.Push(AllocNew());
            }
        }

        private void ExpandPool()
        {
            switch (_settings.ExpandMethod)
            {
                case PoolExpandMethods.Fixed:
                    {
                        throw new PoolExceededFixedSizeException(
                            "Pool factory '{0}' exceeded its max size of '{1}'!"
                            .Fmt(this.GetType(), NumTotal));
                    }
                case PoolExpandMethods.OneAtATime:
                    {
                        _inactiveItems.Push(AllocNew());
                        break;
                    }
                case PoolExpandMethods.Double:
                    {
                        if (NumTotal == 0)
                        {
                            _inactiveItems.Push(AllocNew());
                        }
                        else
                        {
                            var oldSize = NumTotal;

                            for (int i = 0; i < oldSize; i++)
                            {
                                _inactiveItems.Push(AllocNew());
                            }
                        }
                        break;
                    }
                default:
                    {
                        throw Assert.CreateException();
                    }
            }
        }

        protected virtual void OnDespawned(TContract item)
        {
            // Optional
        }

        protected virtual void OnSpawned(TContract item)
        {
            // Optional
        }

        protected virtual void OnCreated(TContract item)
        {
            // Optional
        }
    }
}