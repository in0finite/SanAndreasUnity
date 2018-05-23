using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;
using Zenject.Internal;

#if !NOT_UNITY3D

using UnityEngine;

#endif

namespace Zenject
{
    public delegate bool BindingCondition(InjectContext c);

    public class InjectArgs
    {
        public List<TypeValuePair> ExtraArgs;
        public InjectContext Context;
        public object ConcreteIdentifier;
    }

    // Responsibilities:
    // - Expose methods to configure object graph via BindX() methods
    // - Look up bound values via Resolve() method
    // - Instantiate new values via InstantiateX() methods
    public class DiContainer : IInstantiator
    {
        private readonly Dictionary<BindingId, List<ProviderInfo>> _providers = new Dictionary<BindingId, List<ProviderInfo>>();
        private readonly List<DiContainer> _parentContainers = new List<DiContainer>();
        private readonly List<DiContainer> _ancestorContainers = new List<DiContainer>();
        private readonly Stack<LookupId> _resolvesInProgress = new Stack<LookupId>();

        private readonly SingletonProviderCreator _singletonProviderCreator;
        private readonly SingletonMarkRegistry _singletonMarkRegistry;
        private readonly LazyInstanceInjector _lazyInjector;

        private readonly Queue<IBindingFinalizer> _currentBindings = new Queue<IBindingFinalizer>();
        private readonly List<IBindingFinalizer> _childBindings = new List<IBindingFinalizer>();

        private readonly List<ILazy> _lateBindingsToValidate = new List<ILazy>();

#if !NOT_UNITY3D
        private Context _context;
#endif

        private bool _isFinalizingBinding;
        private bool _isValidating;
        private bool _isInstalling;
        private bool _hasDisplayedInstallWarning;

        public DiContainer(bool isValidating)
        {
            _isValidating = isValidating;

            _singletonMarkRegistry = new SingletonMarkRegistry();
            _lazyInjector = new LazyInstanceInjector(this);
            _singletonProviderCreator = new SingletonProviderCreator(this, _singletonMarkRegistry);

            ShouldCheckForInstallWarning = true;

            InstallDefaultBindings();
            FlushBindings();
            Assert.That(_currentBindings.IsEmpty());
        }

        private void InstallDefaultBindings()
        {
            Bind(typeof(DiContainer), typeof(IInstantiator)).FromInstance(this);
            Bind(typeof(Lazy<>)).FromMethodUntyped(CreateLazyBinding).Lazy();
        }

        private object CreateLazyBinding(InjectContext context)
        {
            // By cloning it this also means that Ids, optional, etc. are forwarded properly
            var newContext = context.Clone();
            newContext.MemberType = context.MemberType.GenericArguments().Single();

            var result = Activator.CreateInstance(
                typeof(Lazy<>).MakeGenericType(newContext.MemberType), new object[] { this, newContext });

            if (_isValidating)
            {
                // Unfortunately we can't validate each lazy binding here
                // because that could result in circular reference exceptions
                // And that might be exactly why you're using lazy in the first place
                _lateBindingsToValidate.Add(((ILazy)result));
            }

            return result;
        }

        public DiContainer()
            : this(false)
        {
        }

        public DiContainer(IEnumerable<DiContainer> parentContainers, bool isValidating)
            : this(isValidating)
        {
            _parentContainers = parentContainers.ToList();
            _ancestorContainers = FlattenInheritanceChain();

            if (!_parentContainers.IsEmpty())
            {
                foreach (var parent in _parentContainers)
                {
                    parent.FlushBindings();
                }

#if !NOT_UNITY3D
                DefaultParent = _parentContainers.First().DefaultParent;
#endif

                // Make sure to avoid duplicates which could happen if a parent container
                // appears multiple times in the inheritance chain
                foreach (var binding in _parentContainers.SelectMany(x => x._childBindings).Distinct())
                {
                    Assert.That(binding.CopyIntoAllSubContainers);
                    _currentBindings.Enqueue(binding);
                }

                FlushBindings();
            }
        }

        public DiContainer(IEnumerable<DiContainer> parentContainers)
            : this(parentContainers, false)
        {
        }

#if !NOT_UNITY3D

        private Context Context
        {
            get
            {
                if (_context == null)
                {
                    _context = Resolve<Context>();
                    Assert.IsNotNull(_context);
                }

                return _context;
            }
        }

#endif

        public bool ShouldCheckForInstallWarning
        {
            get; set;
        }

        // When true, this will throw exceptions whenever we create new game objects
        // This is helpful when used in places like EditorWindowKernel where we can't
        // assume that there is a "scene" to place objects
        public bool AssertOnNewGameObjects
        {
            get;
            set;
        }

        internal SingletonMarkRegistry SingletonMarkRegistry
        {
            get { return _singletonMarkRegistry; }
        }

        internal SingletonProviderCreator SingletonProviderCreator
        {
            get { return _singletonProviderCreator; }
        }

#if !NOT_UNITY3D

        public Transform DefaultParent
        {
            get;
            set;
        }

#endif

        public IEnumerable<DiContainer> ParentContainers
        {
            get { return _parentContainers; }
        }

        public bool ChecksForCircularDependencies
        {
            get
            {
#if ZEN_MULTITHREADING
                // When multithreading is supported we can't use a static field to track the lookup
                // TODO: We could look at the inject context though
                return false;
#else
                return true;
#endif
            }
        }

        public bool IsValidating
        {
            get { return _isValidating; }
        }

        // When this is true, it will log warnings when Resolve or Instantiate
        // methods are called
        // Used to ensure that Resolve and Instantiate methods are not called
        // during bind phase.  This is important since Resolve and Instantiate
        // make use of the bindings, so if the bindings are not complete then
        // unexpected behaviour can occur
        public bool IsInstalling
        {
            get { return _isInstalling; }
            set { _isInstalling = value; }
        }

        public IEnumerable<BindingId> AllContracts
        {
            get
            {
                FlushBindings();
                return _providers.Keys;
            }
        }

        public void ResolveDependencyRoots()
        {
            FlushBindings();
            foreach (var bindinPair in _providers)
            {
                foreach (var provider in bindinPair.Value)
                {
                    if (provider.NonLazy)
                    {
                        var context = new InjectContext(
                            this, bindinPair.Key.Type, bindinPair.Key.Identifier);
                        context.SourceType = InjectSources.Local;
                        context.Optional = true;

                        var matches = SafeGetInstances(
                            new ProviderPair(provider, this), context);

                        Assert.That(matches.Count() > 0);
                    }
                }
            }
        }

        // This will instantiate any binding that results in a type that derives from IValidatable
        // Note that we are looking at both the contract type and the mapped derived type
        // This means if you add the binding 'Container.Bind<IFoo>().To<Foo>()'
        // and Foo derives from both IFoo and IValidatable, then Foo will be instantiated
        // and then Validate() will be called on it.  Note that this will happen even if Foo is not
        // referenced anywhere in the normally resolved object graph
        public void ValidateValidatables()
        {
            Assert.That(IsValidating);

#if !NOT_UNITY3D
            Assert.That(Application.isEditor);
#endif

            foreach (var pair in _providers.ToList())
            {
                var bindingId = pair.Key;
                var providers = pair.Value;

                List<ProviderInfo> validatableProviders;

                var injectContext = new InjectContext(
                    this, bindingId.Type, bindingId.Identifier);

                if (bindingId.Type.DerivesFrom<IValidatable>())
                {
                    validatableProviders = providers;
                }
                else
                {
                    validatableProviders = providers
                        .Where(x => x.Provider.GetInstanceType(injectContext)
                                .DerivesFrom<IValidatable>()).ToList();
                }

                foreach (var provider in validatableProviders)
                {
                    var validatable = provider.Provider.GetInstance(injectContext) as IValidatable;

                    if (validatable != null)
                    {
                        validatable.Validate();
                    }
                }
            }

            foreach (var lazy in _lateBindingsToValidate)
            {
                lazy.Validate();
            }
        }

        public DiContainer CreateSubContainer()
        {
            return CreateSubContainer(_isValidating);
        }

        public void QueueForInject(object instance)
        {
            _lazyInjector.AddInstance(instance);
        }

        public void FlushInjectQueue()
        {
            _lazyInjector.LazyInjectAll();
        }

        // Note: this only does anything useful during the injection phase
        // It will inject on the given instance if it hasn't already been injected, but only
        // if the given instance has been queued for inject already by calling QueueForInject
        // In some rare cases this can be useful - for example if you want to add a binding in a
        // a higher level container to a resolve inside a lower level game object context container
        // since in this case you need the game object context to be injected so you can access its
        // Container property
        public T LazyInject<T>(T instance)
        {
            _lazyInjector.LazyInject(instance);
            return instance;
        }

        private DiContainer CreateSubContainer(bool isValidating)
        {
            return new DiContainer(new DiContainer[] { this }, isValidating);
        }

        public void RegisterProvider(
            BindingId bindingId, BindingCondition condition, IProvider provider, bool nonLazy)
        {
            var info = new ProviderInfo(provider, condition, nonLazy);

            if (_providers.ContainsKey(bindingId))
            {
                _providers[bindingId].Add(info);
            }
            else
            {
                _providers.Add(bindingId, new List<ProviderInfo> { info });
            }
        }

        // Wrap IEnumerable<> to avoid LINQ mistakes
        internal List<IProvider> GetAllProviderMatches(InjectContext context)
        {
            Assert.IsNotNull(context);
            return GetProviderMatchesInternal(context).Select(x => x.ProviderInfo.Provider).ToList();
        }

        // Be careful with this method since it is a coroutine
        private IEnumerable<ProviderPair> GetProviderMatchesInternal(InjectContext context)
        {
            Assert.IsNotNull(context);
            return GetProvidersForContract(context.GetBindingId(), context.SourceType)
                .Where(x => x.ProviderInfo.Condition == null || x.ProviderInfo.Condition(context));
        }

        private IEnumerable<DiContainer> GetAllContainersToLookup(InjectSources sourceType)
        {
            switch (sourceType)
            {
                case InjectSources.Local:
                    {
                        yield return this;
                        break;
                    }
                case InjectSources.Parent:
                    {
                        foreach (var parent in _parentContainers)
                        {
                            yield return parent;
                        }
                        break;
                    }
                case InjectSources.Any:
                    {
                        yield return this;
                        foreach (var ancestor in _ancestorContainers)
                        {
                            yield return ancestor;
                        }
                        break;
                    }
                case InjectSources.AnyParent:
                    {
                        foreach (var ancestor in _ancestorContainers)
                        {
                            yield return ancestor;
                        }
                        break;
                    }
                default:
                    {
                        throw Assert.CreateException();
                    }
            }
        }

        // Get the full list of ancestor Di Containers, making sure to avoid
        // duplicates and also order them in a breadth-first way
        private List<DiContainer> FlattenInheritanceChain()
        {
            var processed = new List<DiContainer>();

            var containerQueue = new Queue<DiContainer>();
            containerQueue.Enqueue(this);

            while (containerQueue.Count > 0)
            {
                var current = containerQueue.Dequeue();

                foreach (var parent in current.ParentContainers)
                {
                    if (!processed.Contains(parent))
                    {
                        processed.Add(parent);
                        containerQueue.Enqueue(parent);
                    }
                }
            }

            return processed;
        }

        private IEnumerable<ProviderPair> GetLocalProviderPairs(BindingId bindingId)
        {
            return GetLocalProviders(bindingId).Select(x => new ProviderPair(x, this));
        }

        private IEnumerable<ProviderPair> GetProvidersForContract(
            BindingId bindingId, InjectSources sourceType)
        {
            var containers = GetAllContainersToLookup(sourceType);

            foreach (var container in containers)
            {
                container.FlushBindings();
            }

            return containers
                .SelectMany(x => x.GetLocalProviderPairs(bindingId));
        }

        private List<ProviderInfo> GetLocalProviders(BindingId bindingId)
        {
            List<ProviderInfo> localProviders;

            if (_providers.TryGetValue(bindingId, out localProviders))
            {
                return localProviders;
            }

            // If we are asking for a List<int>, we should also match for any localProviders that are bound to the open generic type List<>
            // Currently it only matches one and not the other - not totally sure if this is better than returning both
            if (bindingId.Type.IsGenericType() && _providers.TryGetValue(new BindingId(bindingId.Type.GetGenericTypeDefinition(), bindingId.Identifier), out localProviders))
            {
                return localProviders;
            }

            return new List<ProviderInfo>();
        }

        public void Install<TInstaller>()
            where TInstaller : Installer
        {
            Instantiate<TInstaller>().InstallBindings();
        }

        // Note: You might want to use Installer<> as your base class instead to allow
        // for strongly typed parameters
        public void Install<TInstaller>(object[] extraArgs)
            where TInstaller : Installer
        {
            Instantiate<TInstaller>(extraArgs).InstallBindings();
        }

        public IList ResolveAll(InjectContext context)
        {
            Assert.IsNotNull(context);
            // Note that different types can map to the same provider (eg. a base type to a concrete class and a concrete class to itself)

            FlushBindings();
            CheckForInstallWarning(context);

            var matches = GetProviderMatchesInternal(context).ToList();

            if (matches.Any())
            {
                var instances = matches.SelectMany(x => SafeGetInstances(x, context)).ToArray();

                if (instances.Length == 0 && !context.Optional)
                {
                    throw Assert.CreateException(
                        "Could not find required dependency with type '{0}'.  Found providers but they returned zero results!", context.MemberType);
                }

                if (IsValidating)
                {
                    instances = instances.Select(x => x is ValidationMarker ? context.MemberType.GetDefaultValue() : x).ToArray();
                }

                return ReflectionUtil.CreateGenericList(context.MemberType, instances);
            }

            if (!context.Optional)
            {
                throw Assert.CreateException(
                    "Could not find required dependency with type '{0}' \nObject graph:\n {1}", context.MemberType, context.GetObjectGraphString());
            }

            return ReflectionUtil.CreateGenericList(context.MemberType, new object[] { });
        }

        private void CheckForInstallWarning(InjectContext context)
        {
            if (!ShouldCheckForInstallWarning)
            {
                return;
            }

            Assert.IsNotNull(context);
#if DEBUG || UNITY_EDITOR
            if (!_isInstalling)
            {
                return;
            }

            if (_hasDisplayedInstallWarning)
            {
                return;
            }

            if (context == null)
            {
                // No way to tell whether this is ok or not so just assume ok
                return;
            }

            var rootContext = context.ParentContextsAndSelf.Last();

            if (rootContext.MemberType.DerivesFrom<IInstaller>())
            {
                // Resolving/instantiating/injecting installers is valid during install phase
                return;
            }

            _hasDisplayedInstallWarning = true;
            // Feel free to comment this out if you are comfortable with this practice
            ModestTree.Log.Warn("Zenject Warning: It is bad practice to call Inject/Resolve/Instantiate before all the Installers have completed!  This is important to ensure that all bindings have properly been installed in case they are needed when injecting/instantiating/resolving.  Detected when operating on type '{0}'.  If you don't care about this, you can remove this warning or set 'Container.ShouldCheckForInstallWarning' to false.", rootContext.MemberType);
#endif
        }

        // Returns the concrete type that would be returned with Resolve<T>
        // without actually instantiating it
        // This is safe to use within installers
        public Type ResolveType<T>()
        {
            return ResolveType(typeof(T));
        }

        // Returns the concrete type that would be returned with Resolve(type)
        // without actually instantiating it
        // This is safe to use within installers
        public Type ResolveType(Type type)
        {
            return ResolveType(new InjectContext(this, type, null));
        }

        // Returns the concrete type that would be returned with Resolve(context)
        // without actually instantiating it
        // This is safe to use within installers
        public Type ResolveType(InjectContext context)
        {
            Assert.IsNotNull(context);

            ProviderPair provider;

            FlushBindings();

            var result = TryGetUniqueProvider(context, out provider);

            if (result == ProviderLookupResult.Multiple)
            {
                throw Assert.CreateException(
                    "Found multiple matches when only one was expected for type '{0}'{1}. \nObject graph:\n {2}",
                    context.MemberType,
                    (context.ObjectType == null ? "" : " while building object with type '{0}'".Fmt(context.ObjectType)),
                    context.GetObjectGraphString());
            }

            if (result != ProviderLookupResult.Success)
            {
                throw Assert.CreateException(
                    "Unable to resolve type '{0}'{1}. \nObject graph:\n{2}",
                    context.MemberType.ToString() + (context.Identifier == null ? "" : " with ID '{0}'".Fmt(context.Identifier.ToString())),
                    (context.ObjectType == null ? "" : " while building object with type '{0}'".Fmt(context.ObjectType)),
                    context.GetObjectGraphString());
            }

            Assert.IsNotNull(provider);
            return provider.ProviderInfo.Provider.GetInstanceType(context);
        }

        public List<Type> ResolveTypeAll(Type type)
        {
            return ResolveTypeAll(type, null);
        }

        public List<Type> ResolveTypeAll(Type type, object identifier)
        {
            return ResolveTypeAll(new InjectContext(this, type, identifier));
        }

        // Returns all the types that would be returned if ResolveAll was called with the given values
        public List<Type> ResolveTypeAll(InjectContext context)
        {
            Assert.IsNotNull(context);

            FlushBindings();

            var providers = GetProviderMatchesInternal(context).ToList();
            if (providers.Count > 0)
            {
                return providers.Select(
                    x => x.ProviderInfo.Provider.GetInstanceType(context))
                    .Where(x => x != null).ToList();
            }

            return new List<Type> { };
        }

        // Try looking up a single provider for a given context
        // Note that this method should not throw zenject exceptions
        private ProviderLookupResult TryGetUniqueProvider(
            InjectContext context, out ProviderPair providerPair)
        {
            Assert.IsNotNull(context);

            // Note that different types can map to the same provider (eg. a base type to a concrete class and a concrete class to itself)
            var providers = GetProviderMatchesInternal(context).ToList();

            if (providers.IsEmpty())
            {
                providerPair = null;
                return ProviderLookupResult.None;
            }

            if (providers.Count > 1)
            {
                // If we find multiple providers and we are looking for just one, then
                // try to intelligently choose one from the list before giving up

                // First try picking the most 'local' dependencies
                // This will bias towards bindings for the lower level specific containers rather than the global high level container
                // This will, for example, allow you to just ask for a DiContainer dependency without needing to specify [Inject(Source = InjectSources.Local)]
                // (otherwise it would always match for a list of DiContainer's for all parent containers)
                var sortedProviders = providers.Select(x => new { Pair = x, Distance = GetContainerHeirarchyDistance(x.Container) }).OrderBy(x => x.Distance).ToList();

                sortedProviders.RemoveAll(x => x.Distance != sortedProviders[0].Distance);

                if (sortedProviders.Count == 1)
                {
                    // We have one match that is the closest
                    providerPair = sortedProviders[0].Pair;
                }
                else
                {
                    // Try choosing the one with a condition before giving up and throwing an exception
                    // This is nice because it allows us to bind a default and then override with conditions
                    providerPair = sortedProviders.Where(x => x.Pair.ProviderInfo.Condition != null).Select(x => x.Pair).OnlyOrDefault();

                    if (providerPair == null)
                    {
                        return ProviderLookupResult.Multiple;
                    }
                }
            }
            else
            {
                providerPair = providers.Single();
            }

            Assert.IsNotNull(providerPair);
            return ProviderLookupResult.Success;
        }

        public object Resolve(InjectContext context)
        {
            // Note: context.Container is not necessarily equal to this, since
            // you can have some lookups recurse to parent containers
            Assert.IsNotNull(context);

            ProviderPair providerPair;

            FlushBindings();
            CheckForInstallWarning(context);

            var lookupContext = context;

            // The context used for lookups is always the same as the given context EXCEPT for Lazy<>
            // In CreateLazyBinding above, we forward the context to a new instance of Lazy<>
            // The problem is, we want the binding for Bind(typeof(Lazy<>)) to always match even
            // for members that are marked for a specific ID, so we need to discard the identifier
            // for this one particular case
            if (context.MemberType.IsGenericType() && context.MemberType.GetGenericTypeDefinition() == typeof(Lazy<>))
            {
                lookupContext = context.Clone();
                lookupContext.Identifier = null;
                lookupContext.SourceType = InjectSources.Local;
                lookupContext.Optional = false;
            }

            var result = TryGetUniqueProvider(lookupContext, out providerPair);

            if (result == ProviderLookupResult.Multiple)
            {
                throw Assert.CreateException(
                    "Found multiple matches when only one was expected for type '{0}'{1}. \nObject graph:\n {2}",
                    context.MemberType,
                    (context.ObjectType == null ? "" : " while building object with type '{0}'".Fmt(context.ObjectType)),
                    context.GetObjectGraphString());
            }

            if (result == ProviderLookupResult.None)
            {
                // If it's a generic list then try matching multiple instances to its generic type
                if (ReflectionUtil.IsGenericList(context.MemberType))
                {
                    var subType = context.MemberType.GenericArguments().Single();

                    var subContext = context.Clone();
                    subContext.MemberType = subType;
                    // By making this optional this means that all injected fields of type List<>
                    // will pass validation, which could be error prone, but I think this is better
                    // than always requiring that they explicitly mark their list types as optional
                    subContext.Optional = true;

                    return ResolveAll(subContext);
                }

                if (context.Optional)
                {
                    return context.FallBackValue;
                }

                throw Assert.CreateException("Unable to resolve type '{0}'{1}. \nObject graph:\n{2}",
                    context.MemberType.ToString() + (context.Identifier == null ? "" : " with ID '{0}'".Fmt(context.Identifier.ToString())),
                    (context.ObjectType == null ? "" : " while building object with type '{0}'".Fmt(context.ObjectType)),
                    context.GetObjectGraphString());
            }

            Assert.That(result == ProviderLookupResult.Success);
            Assert.IsNotNull(providerPair);

            var instances = SafeGetInstances(providerPair, context);

            if (instances.IsEmpty())
            {
                if (context.Optional)
                {
                    return context.FallBackValue;
                }

                throw Assert.CreateException("Provider returned zero instances when one was expected!  While resolving type '{0}'{1}. \nObject graph:\n{2}",
                    context.MemberType.ToString() + (context.Identifier == null ? "" : " with ID '{0}'".Fmt(context.Identifier.ToString())),
                    (context.ObjectType == null ? "" : " while building object with type '{0}'".Fmt(context.ObjectType)),
                    context.GetObjectGraphString());
            }

            if (instances.Count() > 1)
            {
                throw Assert.CreateException("Provider returned multiple instances when only one was expected!  While resolving type '{0}'{1}. \nObject graph:\n{2}",
                    context.MemberType.ToString() + (context.Identifier == null ? "" : " with ID '{0}'".Fmt(context.Identifier.ToString())),
                    (context.ObjectType == null ? "" : " while building object with type '{0}'".Fmt(context.ObjectType)),
                    context.GetObjectGraphString());
            }

            return instances.First();
        }

        private IEnumerable<object> SafeGetInstances(ProviderPair providerPair, InjectContext context)
        {
            Assert.IsNotNull(context);

            var provider = providerPair.ProviderInfo.Provider;

            if (ChecksForCircularDependencies)
            {
                var lookupId = new LookupId(provider, context.GetBindingId());

                // Use the container associated with the provider to address some rare cases
                // which would otherwise result in an infinite loop.  Like this:
                // Container.Bind<ICharacter>().FromComponentInNewPrefab(Prefab).AsTransient()
                // With the prefab being a GameObjectContext containing a script that has a
                // ICharacter dependency.  In this case, we would otherwise use the _resolvesInProgress
                // associated with the GameObjectContext container, which will allow the recursive
                // lookup, which will trigger another GameObjectContext and container (since it is
                // transient) and the process continues indefinitely

                var providerContainer = providerPair.Container;

                if (providerContainer._resolvesInProgress.Where(x => x.Equals(lookupId)).Count() > 1)
                {
                    // Allow one before giving up so that you can do circular dependencies via postinject or fields
                    throw Assert.CreateException(
                        "Circular dependency detected! \nObject graph:\n {0}", context.GetObjectGraphString());
                }

                providerContainer._resolvesInProgress.Push(lookupId);
                try
                {
                    return provider.GetAllInstances(context);
                }
                finally
                {
                    Assert.That(providerContainer._resolvesInProgress.Peek().Equals(lookupId));
                    providerContainer._resolvesInProgress.Pop();
                }
            }
            else
            {
                return provider.GetAllInstances(context);
            }
        }

        private int GetContainerHeirarchyDistance(DiContainer container)
        {
            return GetContainerHeirarchyDistance(container, 0).Value;
        }

        private int? GetContainerHeirarchyDistance(DiContainer container, int depth)
        {
            if (container == this)
            {
                return depth;
            }

            int? result = null;

            foreach (var parent in _parentContainers)
            {
                var distance = parent.GetContainerHeirarchyDistance(container, depth + 1);

                if (distance.HasValue && (!result.HasValue || distance.Value < result.Value))
                {
                    result = distance;
                }
            }

            return result;
        }

        public IEnumerable<Type> GetDependencyContracts<TContract>()
        {
            return GetDependencyContracts(typeof(TContract));
        }

        public IEnumerable<Type> GetDependencyContracts(Type contract)
        {
            FlushBindings();

            foreach (var injectMember in TypeAnalyzer.GetInfo(contract).AllInjectables)
            {
                yield return injectMember.MemberType;
            }
        }

        public static bool CanCreateOrInjectDuringValidation(Type type)
        {
            // During validation, do not instantiate or inject anything except for
            // Installers, IValidatable's, or types marked with attribute ZenjectAllowDuringValidation
            // You would typically use ZenjectAllowDuringValidation attribute for data that you
            // inject into factories
            return type.DerivesFrom<IInstaller>()
                || type.DerivesFrom<IValidatable>()
#if !NOT_UNITY3D
                || type.DerivesFrom<Context>()
#endif
#if !(UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR)
                || type.HasAttribute<ZenjectAllowDuringValidationAttribute>()
#endif
            ;
        }

        private object InstantiateInternal(Type concreteType, bool autoInject, InjectArgs args)
        {
#if !NOT_UNITY3D
            Assert.That(!concreteType.DerivesFrom<UnityEngine.Component>(),
                "Error occurred while instantiating object of type '{0}'. Instantiator should not be used to create new mono behaviours.  Must use InstantiatePrefabForComponent, InstantiatePrefab, or InstantiateComponent.", concreteType);
#endif

            Assert.That(!concreteType.IsAbstract(), "Expected type '{0}' to be non-abstract", concreteType);

            FlushBindings();
            CheckForInstallWarning(args.Context);

            var typeInfo = TypeAnalyzer.GetInfo(concreteType);

            object newObj;

#if !NOT_UNITY3D
            if (concreteType.DerivesFrom<ScriptableObject>())
            {
                Assert.That(typeInfo.ConstructorInjectables.IsEmpty(),
                    "Found constructor parameters on ScriptableObject type '{0}'.  This is not allowed.  Use an [Inject] method or fields instead.");

                if (!IsValidating || CanCreateOrInjectDuringValidation(concreteType))
                {
                    newObj = ScriptableObject.CreateInstance(concreteType);
                }
                else
                {
                    newObj = new ValidationMarker(concreteType);
                }
            }
            else
#endif
            {
                Assert.IsNotNull(typeInfo.InjectConstructor,
                    "More than one (or zero) constructors found for type '{0}' when creating dependencies.  Use one [Inject] attribute to specify which to use.", concreteType);

                // Make a copy since we remove from it below
                var paramValues = new List<object>();

                foreach (var injectInfo in typeInfo.ConstructorInjectables)
                {
                    object value;

                    if (!InjectUtil.PopValueWithType(
                        args.ExtraArgs, injectInfo.MemberType, out value))
                    {
                        value = Resolve(injectInfo.CreateInjectContext(
                            this, args.Context, null, args.ConcreteIdentifier));
                    }

                    if (value is ValidationMarker)
                    {
                        Assert.That(IsValidating);
                        paramValues.Add(injectInfo.MemberType.GetDefaultValue());
                    }
                    else
                    {
                        paramValues.Add(value);
                    }
                }

                if (!IsValidating || CanCreateOrInjectDuringValidation(concreteType))
                {
                    try
                    {
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
                        using (ProfileBlock.Start("{0}.{1}()", concreteType, concreteType.Name))
#endif
                        {
                            newObj = typeInfo.InjectConstructor.Invoke(paramValues.ToArray());
                        }
                    }
                    catch (Exception e)
                    {
                        throw Assert.CreateException(
                            e, "Error occurred while instantiating object with type '{0}'", concreteType);
                    }
                }
                else
                {
                    newObj = new ValidationMarker(concreteType);
                }
            }

            if (autoInject)
            {
                InjectExplicit(newObj, concreteType, args);

                if (!args.ExtraArgs.IsEmpty())
                {
                    throw Assert.CreateException(
                        "Passed unnecessary parameters when injecting into type '{0}'. \nExtra Parameters: {1}\nObject graph:\n{2}",
                        newObj.GetType(), String.Join(",", args.ExtraArgs.Select(x => x.Type.Name()).ToArray()), args.Context.GetObjectGraphString());
                }
            }

            return newObj;
        }

        // InjectExplicit is only necessary when you want to inject null values into your object
        // otherwise you can just use Inject()
        // Note: Any arguments that are used will be removed from extraArgMap
        public void InjectExplicit(object injectable, List<TypeValuePair> extraArgs)
        {
            Type injectableType;

            if (injectable is ValidationMarker)
            {
                injectableType = ((ValidationMarker)injectable).MarkedType;
            }
            else
            {
                injectableType = injectable.GetType();
            }

            InjectExplicit(
                injectable,
                injectableType,
                new InjectArgs()
                {
                    ExtraArgs = extraArgs,
                    Context = new InjectContext(this, injectableType, null),
                    ConcreteIdentifier = null,
                });
        }

        public void InjectExplicit(
            object injectable, Type injectableType, InjectArgs args)
        {
            if (IsValidating)
            {
                var marker = injectable as ValidationMarker;

                if (marker != null && marker.InstantiateFailed)
                {
                    // Do nothing in this case because it already failed and so there
                    // could be many knock-on errors that aren't related to the user
                    return;
                }

                try
                {
                    InjectExplicitInternal(injectable, injectableType, args);
                }
                catch (Exception e)
                {
                    // Just log the error and continue to print multiple validation errors
                    // at once
                    ModestTree.Log.ErrorException(e);
                }
            }
            else
            {
                InjectExplicitInternal(injectable, injectableType, args);
            }
        }

        private void InjectExplicitInternal(
            object injectable, Type injectableType, InjectArgs args)
        {
            Assert.That(injectable != null);

            // Installers are the only things that we instantiate/inject on during validation
            bool isDryRun = IsValidating && !CanCreateOrInjectDuringValidation(injectableType);

            if (!isDryRun)
            {
                Assert.IsEqual(injectable.GetType(), injectableType);
            }

#if !NOT_UNITY3D
            Assert.That(injectableType != typeof(GameObject),
                "Use InjectGameObject to Inject game objects instead of Inject method");
#endif

            FlushBindings();
            CheckForInstallWarning(args.Context);

            var typeInfo = TypeAnalyzer.GetInfo(injectableType);

            foreach (var injectInfo in typeInfo.FieldInjectables.Concat(
                typeInfo.PropertyInjectables))
            {
                object value;

                if (InjectUtil.PopValueWithType(args.ExtraArgs, injectInfo.MemberType, out value))
                {
                    if (!isDryRun)
                    {
                        if (value is ValidationMarker)
                        {
                            Assert.That(IsValidating);
                        }
                        else
                        {
                            injectInfo.Setter(injectable, value);
                        }
                    }
                }
                else
                {
                    value = Resolve(
                        injectInfo.CreateInjectContext(
                            this, args.Context, injectable, args.ConcreteIdentifier));

                    if (injectInfo.Optional && value == null)
                    {
                        // Do not override in this case so it retains the hard-coded value
                    }
                    else
                    {
                        if (!isDryRun)
                        {
                            if (value is ValidationMarker)
                            {
                                Assert.That(IsValidating);
                            }
                            else
                            {
                                injectInfo.Setter(injectable, value);
                            }
                        }
                    }
                }
            }

            foreach (var method in typeInfo.PostInjectMethods)
            {
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
                using (ProfileBlock.Start("{0}.{1}()", injectableType, method.MethodInfo.Name))
#endif
                {
                    var paramValues = new List<object>();

                    foreach (var injectInfo in method.InjectableInfo)
                    {
                        object value;

                        if (!InjectUtil.PopValueWithType(args.ExtraArgs, injectInfo.MemberType, out value))
                        {
                            value = Resolve(
                                injectInfo.CreateInjectContext(this, args.Context, injectable, args.ConcreteIdentifier));
                        }

                        if (value is ValidationMarker)
                        {
                            Assert.That(IsValidating);
                            paramValues.Add(injectInfo.MemberType.GetDefaultValue());
                        }
                        else
                        {
                            paramValues.Add(value);
                        }
                    }

                    if (!isDryRun)
                    {
#if !NOT_UNITY3D
                        // Handle IEnumerators (Coroutines) as a special case by calling StartCoroutine() instead of invoking directly.
                        if (method.MethodInfo.ReturnType == typeof(IEnumerator))
                        {
                            StartCoroutine(injectable, method, paramValues);
                        }
                        else
#endif
                        {
                            method.MethodInfo.Invoke(injectable, paramValues.ToArray());
                        }
                    }
                }
            }

            if (!args.ExtraArgs.IsEmpty())
            {
                throw Assert.CreateException(
                    "Passed unnecessary parameters when injecting into type '{0}'. \nExtra Parameters: {1}\nObject graph:\n{2}",
                    injectableType, String.Join(",", args.ExtraArgs.Select(x => x.Type.Name()).ToArray()), args.Context.GetObjectGraphString());
            }
        }

#if !NOT_UNITY3D

        private void StartCoroutine(object injectable, PostInjectableInfo method, List<object> paramValues)
        {
            var startCoroutineOn = injectable as MonoBehaviour;

            // If the injectable isn't a MonoBehaviour, then start the coroutine on the context associated
            // with this container
            if (startCoroutineOn == null)
            {
                startCoroutineOn = TryResolve<Context>();
            }

            if (startCoroutineOn == null)
            {
                throw Assert.CreateException(
                    "Unable to find a suitable MonoBehaviour to start the '{0}.{1}' coroutine on.",
                    method.MethodInfo.DeclaringType, method.MethodInfo.Name);
            }

            var result = method.MethodInfo.Invoke(injectable, paramValues.ToArray()) as IEnumerator;

            startCoroutineOn.StartCoroutine(result);
        }

        // Don't use this unless you know what you're doing
        // You probably want to use InstantiatePrefab instead
        // This one will only create the prefab and will not inject into it
        // Also, this will always return the new game object as disabled, so that injection can occur before Awake / OnEnable / Start
        internal GameObject CreateAndParentPrefabResource(
            string resourcePath, GameObjectCreationParameters gameObjectBindInfo, InjectContext context, out bool shouldMakeActive)
        {
            var prefab = (GameObject)Resources.Load(resourcePath);

            Assert.IsNotNull(prefab,
                "Could not find prefab at resource location '{0}'".Fmt(resourcePath));

            return CreateAndParentPrefab(prefab, gameObjectBindInfo, context, out shouldMakeActive);
        }

        private GameObject GetPrefabAsGameObject(UnityEngine.Object prefab)
        {
            if (prefab is GameObject)
            {
                return (GameObject)prefab;
            }

            Assert.That(prefab is Component, "Invalid type given for prefab. Given object name: '{0}'", prefab.name);
            return ((Component)prefab).gameObject;
        }

        // Don't use this unless you know what you're doing
        // You probably want to use InstantiatePrefab instead
        // This one will only create the prefab and will not inject into it
        internal GameObject CreateAndParentPrefab(
            UnityEngine.Object prefab, GameObjectCreationParameters gameObjectBindInfo,
            InjectContext context, out bool shouldMakeActive)
        {
            Assert.That(prefab != null, "Null prefab found when instantiating game object");

            Assert.That(!AssertOnNewGameObjects,
                "Given DiContainer does not support creating new game objects");

            FlushBindings();

            var prefabAsGameObject = GetPrefabAsGameObject(prefab);

            var wasActive = prefabAsGameObject.activeSelf;

            if (wasActive)
            {
                prefabAsGameObject.SetActive(false);
            }

            shouldMakeActive = wasActive;

            try
            {
                GameObject gameObj;

                var transformParent = GetTransformGroup(gameObjectBindInfo, context);

                if (gameObjectBindInfo.Position.HasValue && gameObjectBindInfo.Rotation.HasValue)
                {
                    gameObj = (GameObject)GameObject.Instantiate(
                        prefabAsGameObject, gameObjectBindInfo.Position.Value, gameObjectBindInfo.Rotation.Value, transformParent);
                }
                else if (gameObjectBindInfo.Position.HasValue)
                {
                    gameObj = (GameObject)GameObject.Instantiate(
                        prefabAsGameObject, gameObjectBindInfo.Position.Value, prefabAsGameObject.transform.rotation, transformParent);
                }
                else if (gameObjectBindInfo.Rotation.HasValue)
                {
                    gameObj = (GameObject)GameObject.Instantiate(
                        prefabAsGameObject, prefabAsGameObject.transform.position, gameObjectBindInfo.Rotation.Value, transformParent);
                }
                else
                {
                    gameObj = (GameObject)GameObject.Instantiate(prefabAsGameObject, transformParent);
                }

                if (transformParent == null)
                {
                    // This ensures it gets added to the right scene instead of just the active scene
                    gameObj.transform.SetParent(Context.transform, false);
                    gameObj.transform.SetParent(null, false);
                }

                if (gameObjectBindInfo.Name != null)
                {
                    gameObj.name = gameObjectBindInfo.Name;
                }

                return gameObj;
            }
            finally
            {
                if (wasActive)
                {
                    // Always make sure to reset prefab state otherwise this change could be saved
                    // persistently
                    prefabAsGameObject.SetActive(true);
                }
            }
        }

        public GameObject CreateEmptyGameObject(string name)
        {
            return CreateEmptyGameObject(new GameObjectCreationParameters() { Name = name }, null);
        }

        public GameObject CreateEmptyGameObject(
            GameObjectCreationParameters gameObjectBindInfo, InjectContext context)
        {
            Assert.That(!AssertOnNewGameObjects,
                "Given DiContainer does not support creating new game objects");

            FlushBindings();

            var gameObj = new GameObject(gameObjectBindInfo.Name ?? "GameObject");
            var parent = GetTransformGroup(gameObjectBindInfo, context);

            if (parent == null)
            {
                // This ensures it gets added to the right scene instead of just the active scene
                gameObj.transform.SetParent(Context.transform, false);
                gameObj.transform.SetParent(null, false);
            }
            else
            {
                gameObj.transform.SetParent(parent, false);
            }

            return gameObj;
        }

        private Transform GetTransformGroup(
            GameObjectCreationParameters gameObjectBindInfo, InjectContext context)
        {
            Assert.That(!AssertOnNewGameObjects,
                "Given DiContainer does not support creating new game objects");

            if (gameObjectBindInfo.ParentTransform != null)
            {
                Assert.IsNull(gameObjectBindInfo.GroupName);
                Assert.IsNull(gameObjectBindInfo.ParentTransformGetter);

                return gameObjectBindInfo.ParentTransform;
            }

            if (gameObjectBindInfo.ParentTransformGetter != null)
            {
                Assert.IsNull(gameObjectBindInfo.GroupName);

                if (context == null)
                {
                    context = new InjectContext()
                    {
                        // This is the only information we can supply in this case
                        Container = this,
                    };
                }

                // NOTE: Null is fine here, will just be a root game object in that case
                return gameObjectBindInfo.ParentTransformGetter(context);
            }

            var groupName = gameObjectBindInfo.GroupName;

            if (DefaultParent == null)
            {
                if (groupName == null)
                {
                    return null;
                }

                return (GameObject.Find("/" + groupName) ?? CreateTransformGroup(groupName)).transform;
            }

            if (groupName == null)
            {
                return DefaultParent;
            }

            foreach (Transform child in DefaultParent)
            {
                if (child.name == groupName)
                {
                    return child;
                }
            }

            var group = new GameObject(groupName).transform;
            group.SetParent(DefaultParent, false);
            return group;
        }

        private GameObject CreateTransformGroup(string groupName)
        {
            var gameObj = new GameObject(groupName);
            gameObj.transform.SetParent(Context.transform, false);
            gameObj.transform.SetParent(null, false);
            return gameObj;
        }

#endif

        public T Instantiate<T>()
        {
            return Instantiate<T>(new object[0]);
        }

        public T Instantiate<T>(IEnumerable<object> extraArgs)
        {
            var result = Instantiate(typeof(T), extraArgs);

            if (IsValidating && !(result is T))
            {
                Assert.That(result is ValidationMarker);
                return default(T);
            }

            return (T)result;
        }

        public object Instantiate(Type concreteType)
        {
            return Instantiate(concreteType, new object[0]);
        }

        public object Instantiate(
            Type concreteType, IEnumerable<object> extraArgs)
        {
            Assert.That(!extraArgs.ContainsItem(null),
                "Null value given to factory constructor arguments when instantiating object with type '{0}'. In order to use null use InstantiateExplicit", concreteType);

            return InstantiateExplicit(
                concreteType, InjectUtil.CreateArgList(extraArgs));
        }

#if !NOT_UNITY3D

        // Add new component to existing game object and fill in its dependencies
        // This is the same as AddComponent except the [Inject] fields will be filled in
        // NOTE: Gameobject here is not a prefab prototype, it is an instance
        public TContract InstantiateComponent<TContract>(GameObject gameObject)
            where TContract : Component
        {
            return InstantiateComponent<TContract>(gameObject, new object[0]);
        }

        // Add new component to existing game object and fill in its dependencies
        // This is the same as AddComponent except the [Inject] fields will be filled in
        // NOTE: Gameobject here is not a prefab prototype, it is an instance
        public TContract InstantiateComponent<TContract>(
            GameObject gameObject, IEnumerable<object> extraArgs)
            where TContract : Component
        {
            return (TContract)InstantiateComponent(typeof(TContract), gameObject, extraArgs);
        }

        // Add new component to existing game object and fill in its dependencies
        // This is the same as AddComponent except the [Inject] fields will be filled in
        // NOTE: Gameobject here is not a prefab prototype, it is an instance
        public Component InstantiateComponent(
            Type componentType, GameObject gameObject)
        {
            return InstantiateComponent(componentType, gameObject, new object[0]);
        }

        // Add new component to existing game object and fill in its dependencies
        // This is the same as AddComponent except the [Inject] fields will be filled in
        // NOTE: Gameobject here is not a prefab prototype, it is an instance
        public Component InstantiateComponent(
            Type componentType, GameObject gameObject, IEnumerable<object> extraArgs)
        {
            return InstantiateComponentExplicit(
                componentType, gameObject, InjectUtil.CreateArgList(extraArgs));
        }

        public T InstantiateComponentOnNewGameObject<T>()
            where T : Component
        {
            return InstantiateComponentOnNewGameObject<T>(typeof(T).Name);
        }

        public T InstantiateComponentOnNewGameObject<T>(IEnumerable<object> extraArgs)
            where T : Component
        {
            return InstantiateComponentOnNewGameObject<T>(typeof(T).Name, extraArgs);
        }

        public T InstantiateComponentOnNewGameObject<T>(string gameObjectName)
            where T : Component
        {
            return InstantiateComponentOnNewGameObject<T>(gameObjectName, new object[0]);
        }

        public T InstantiateComponentOnNewGameObject<T>(
            string gameObjectName, IEnumerable<object> extraArgs)
            where T : Component
        {
            return InstantiateComponent<T>(
                CreateEmptyGameObject(gameObjectName),
                extraArgs);
        }

        // Create a new game object from a prefab and fill in dependencies for all children
        public GameObject InstantiatePrefab(UnityEngine.Object prefab)
        {
            return InstantiatePrefab(
                prefab, GameObjectCreationParameters.Default);
        }

        // Create a new game object from a prefab and fill in dependencies for all children
        public GameObject InstantiatePrefab(UnityEngine.Object prefab, Transform parentTransform)
        {
            return InstantiatePrefab(
                prefab, new GameObjectCreationParameters() { ParentTransform = parentTransform });
        }

        // Create a new game object from a prefab and fill in dependencies for all children
        public GameObject InstantiatePrefab(
            UnityEngine.Object prefab, Vector3 position, Quaternion rotation, Transform parentTransform)
        {
            return InstantiatePrefab(
                prefab, new GameObjectCreationParameters()
                {
                    ParentTransform = parentTransform,
                    Position = position,
                    Rotation = rotation
                });
        }

        // Create a new game object from a prefab and fill in dependencies for all children
        public GameObject InstantiatePrefab(
            UnityEngine.Object prefab, GameObjectCreationParameters gameObjectBindInfo)
        {
            FlushBindings();

            bool shouldMakeActive;
            var gameObj = CreateAndParentPrefab(
                prefab, gameObjectBindInfo, null, out shouldMakeActive);

            InjectGameObject(gameObj);

            if (shouldMakeActive)
            {
                gameObj.SetActive(true);
            }

            return gameObj;
        }

        // Create a new game object from a resource path and fill in dependencies for all children
        public GameObject InstantiatePrefabResource(string resourcePath)
        {
            return InstantiatePrefabResource(resourcePath, GameObjectCreationParameters.Default);
        }

        // Create a new game object from a resource path and fill in dependencies for all children
        public GameObject InstantiatePrefabResource(string resourcePath, Transform parentTransform)
        {
            return InstantiatePrefabResource(resourcePath, new GameObjectCreationParameters() { ParentTransform = parentTransform });
        }

        // Create a new game object from a resource path and fill in dependencies for all children
        public GameObject InstantiatePrefabResource(
            string resourcePath, GameObjectCreationParameters creationInfo)
        {
            var prefab = (GameObject)Resources.Load(resourcePath);

            Assert.IsNotNull(prefab,
                "Could not find prefab at resource location '{0}'".Fmt(resourcePath));

            return InstantiatePrefab(prefab, creationInfo);
        }

        // Same as InstantiatePrefab but returns a component after it's initialized
        // and optionally allows extra arguments for the given component type
        public T InstantiatePrefabForComponent<T>(UnityEngine.Object prefab)
        {
            return (T)InstantiatePrefabForComponent(
                typeof(T), prefab, null, new object[0]);
        }

        // Same as InstantiatePrefab but returns a component after it's initialized
        // and optionally allows extra arguments for the given component type
        public T InstantiatePrefabForComponent<T>(
            UnityEngine.Object prefab, IEnumerable<object> extraArgs)
        {
            return (T)InstantiatePrefabForComponent(
                typeof(T), prefab, null, extraArgs);
        }

        public T InstantiatePrefabForComponent<T>(
            UnityEngine.Object prefab, Transform parentTransform)
        {
            return (T)InstantiatePrefabForComponent(
                typeof(T), prefab, parentTransform, new object[0]);
        }

        public T InstantiatePrefabForComponent<T>(
            UnityEngine.Object prefab, Transform parentTransform, IEnumerable<object> extraArgs)
        {
            return (T)InstantiatePrefabForComponent(
                typeof(T), prefab, parentTransform, extraArgs);
        }

        // Same as InstantiatePrefab but returns a component after it's initialized
        // and optionally allows extra arguments for the given component type
        public object InstantiatePrefabForComponent(
            Type concreteType, UnityEngine.Object prefab,
            Transform parentTransform, IEnumerable<object> extraArgs)
        {
            return InstantiatePrefabForComponent(
                concreteType, prefab, extraArgs,
                new GameObjectCreationParameters() { ParentTransform = parentTransform });
        }

        public object InstantiatePrefabForComponent(
            Type concreteType, UnityEngine.Object prefab,
            IEnumerable<object> extraArgs, GameObjectCreationParameters creationInfo)
        {
            return InstantiatePrefabForComponentExplicit(
                concreteType, prefab,
                InjectUtil.CreateArgList(extraArgs), creationInfo);
        }

        // Same as InstantiatePrefabResource but returns a component after it's initialized
        // and optionally allows extra arguments for the given component type
        public T InstantiatePrefabResourceForComponent<T>(string resourcePath)
        {
            return (T)InstantiatePrefabResourceForComponent(
                typeof(T), resourcePath, null, new object[0]);
        }

        // Same as InstantiatePrefabResource but returns a component after it's initialized
        // and optionally allows extra arguments for the given component type
        public T InstantiatePrefabResourceForComponent<T>(
            string resourcePath, IEnumerable<object> extraArgs)
        {
            return (T)InstantiatePrefabResourceForComponent(
                typeof(T), resourcePath, null, extraArgs);
        }

        public T InstantiatePrefabResourceForComponent<T>(
            string resourcePath, Transform parentTransform)
        {
            return (T)InstantiatePrefabResourceForComponent(
                typeof(T), resourcePath, parentTransform, new object[0]);
        }

        public T InstantiatePrefabResourceForComponent<T>(
            string resourcePath, Transform parentTransform, IEnumerable<object> extraArgs)
        {
            return (T)InstantiatePrefabResourceForComponent(
                typeof(T), resourcePath, parentTransform, extraArgs);
        }

        // Same as InstantiatePrefabResource but returns a component after it's initialized
        // and optionally allows extra arguments for the given component type
        public object InstantiatePrefabResourceForComponent(
            Type concreteType, string resourcePath, Transform parentTransform,
            IEnumerable<object> extraArgs)
        {
            Assert.That(!extraArgs.ContainsItem(null),
                "Null value given to factory constructor arguments when instantiating object with type '{0}'. In order to use null use InstantiatePrefabForComponentExplicit", concreteType);

            return InstantiatePrefabResourceForComponentExplicit(
                concreteType, resourcePath,
                InjectUtil.CreateArgList(extraArgs),
                new GameObjectCreationParameters() { ParentTransform = parentTransform });
        }

        public T InstantiateScriptableObjectResource<T>(string resourcePath)
            where T : ScriptableObject
        {
            return InstantiateScriptableObjectResource<T>(resourcePath, new object[0]);
        }

        public T InstantiateScriptableObjectResource<T>(
            string resourcePath, IEnumerable<object> extraArgs)
            where T : ScriptableObject
        {
            return (T)InstantiateScriptableObjectResource(
                typeof(T), resourcePath, extraArgs);
        }

        public object InstantiateScriptableObjectResource(
            Type scriptableObjectType, string resourcePath)
        {
            return InstantiateScriptableObjectResource(
                scriptableObjectType, resourcePath, new object[0]);
        }

        public object InstantiateScriptableObjectResource(
            Type scriptableObjectType, string resourcePath, IEnumerable<object> extraArgs)
        {
            Assert.DerivesFromOrEqual<ScriptableObject>(scriptableObjectType);
            return InstantiateScriptableObjectResourceExplicit(
                scriptableObjectType, resourcePath, InjectUtil.CreateArgList(extraArgs));
        }

        // Inject dependencies into any and all child components on the given game object
        public void InjectGameObject(GameObject gameObject)
        {
            FlushBindings();

            var monoBehaviours = new List<MonoBehaviour>();
            ZenUtilInternal.GetInjectableMonoBehaviours(gameObject, monoBehaviours);
            foreach (var monoBehaviour in monoBehaviours)
            {
                Inject(monoBehaviour);
            }
        }

        // Same as InjectGameObject except it will also search the game object for the
        // given component, and also optionally allow passing extra inject arguments into the
        // given component
        public T InjectGameObjectForComponent<T>(GameObject gameObject)
            where T : Component
        {
            return InjectGameObjectForComponent<T>(gameObject, new object[0]);
        }

        // Same as InjectGameObject except it will also search the game object for the
        // given component, and also optionally allow passing extra inject arguments into the
        // given component
        public T InjectGameObjectForComponent<T>(
            GameObject gameObject, IEnumerable<object> extraArgs)
            where T : Component
        {
            return (T)InjectGameObjectForComponent(gameObject, typeof(T), extraArgs);
        }

        // Same as InjectGameObject except it will also search the game object for the
        // given component, and also optionally allow passing extra inject arguments into the
        // given component
        public object InjectGameObjectForComponent(
            GameObject gameObject, Type componentType, IEnumerable<object> extraArgs)
        {
            return InjectGameObjectForComponentExplicit(
                gameObject, componentType,
                new InjectArgs()
                {
                    ExtraArgs = InjectUtil.CreateArgList(extraArgs),
                    Context = new InjectContext(this, componentType, null),
                    ConcreteIdentifier = null,
                });
        }

        // Same as InjectGameObjectForComponent except allows null values
        // to be included in the argument list.  Also see InjectUtil.CreateArgList
        public Component InjectGameObjectForComponentExplicit(
            GameObject gameObject, Type componentType, InjectArgs args)
        {
            if (!componentType.DerivesFrom<MonoBehaviour>() && !args.ExtraArgs.IsEmpty())
            {
                throw Assert.CreateException(
                    "Cannot inject into non-monobehaviours!  Argument list must be zero length");
            }

            var injectableMonoBehaviours = new List<MonoBehaviour>();
            ZenUtilInternal.GetInjectableMonoBehaviours(gameObject, injectableMonoBehaviours);
            foreach (var monoBehaviour in injectableMonoBehaviours)
            {
                if (monoBehaviour.GetType().DerivesFromOrEqual(componentType))
                {
                    InjectExplicit(monoBehaviour, monoBehaviour.GetType(), args);
                }
                else
                {
                    Inject(monoBehaviour);
                }
            }

            var matches = gameObject.GetComponentsInChildren(componentType, true);

            Assert.That(!matches.IsEmpty(),
                "Expected to find component with type '{0}' when injecting into game object '{1}'", componentType, gameObject.name);

            Assert.That(matches.Length == 1,
                "Found multiple component with type '{0}' when injecting into game object '{1}'", componentType, gameObject.name);

            return matches[0];
        }

#endif

        // When you call any of these Inject methods
        //    Any fields marked [Inject] will be set using the bindings on the container
        //    Any methods marked with a [Inject] will be called
        //    Any constructor parameters will be filled in with values from the container
        public void Inject(object injectable)
        {
            Inject(injectable, new object[0]);
        }

        // Same as Inject(injectable) except allows adding extra values to be injected
        public void Inject(object injectable, IEnumerable<object> extraArgs)
        {
            InjectExplicit(
                injectable, InjectUtil.CreateArgList(extraArgs));
        }

        // Resolve<> - Lookup a value in the container.
        //
        // Note that this may result in a new object being created (for transient bindings) or it
        // may return an already created object (for FromInstance or ToSingle, etc. bindings)
        //
        // If a single unique value for the given type cannot be found, an exception is thrown.
        //
        public TContract Resolve<TContract>()
        {
            return (TContract)Resolve(typeof(TContract));
        }

        public object Resolve(Type contractType)
        {
            return ResolveId(contractType, null);
        }

        public TContract ResolveId<TContract>(object identifier)
        {
            return (TContract)ResolveId(typeof(TContract), identifier);
        }

        public object ResolveId(Type contractType, object identifier)
        {
            return Resolve(
                new InjectContext(this, contractType, identifier));
        }

        // Same as Resolve<> except it will return null if a value for the given type cannot
        // be found.
        public TContract TryResolve<TContract>()
            where TContract : class
        {
            return (TContract)TryResolve(typeof(TContract));
        }

        public object TryResolve(Type contractType)
        {
            return TryResolveId(contractType, null);
        }

        public TContract TryResolveId<TContract>(object identifier)
            where TContract : class
        {
            return (TContract)TryResolveId(
                typeof(TContract), identifier);
        }

        public object TryResolveId(Type contractType, object identifier)
        {
            return Resolve(
                new InjectContext(this, contractType, identifier, true));
        }

        // Same as Resolve<> except it will return all bindings that are associated with the given type
        public List<TContract> ResolveAll<TContract>()
        {
            return (List<TContract>)ResolveAll(typeof(TContract));
        }

        public IList ResolveAll(Type contractType)
        {
            return ResolveIdAll(contractType, null);
        }

        public List<TContract> ResolveIdAll<TContract>(object identifier)
        {
            return (List<TContract>)ResolveIdAll(typeof(TContract), identifier);
        }

        public IList ResolveIdAll(Type contractType, object identifier)
        {
            return ResolveAll(
                new InjectContext(this, contractType, identifier, true));
        }

        // Removes all bindings
        public void UnbindAll()
        {
            FlushBindings();
            _providers.Clear();
        }

        // Remove all bindings bound to the given contract type
        public bool Unbind<TContract>()
        {
            return Unbind(typeof(TContract));
        }

        public bool Unbind(Type contractType)
        {
            return UnbindId(contractType, null);
        }

        public bool UnbindId<TContract>(object identifier)
        {
            return UnbindId(typeof(TContract), identifier);
        }

        public bool UnbindId(Type contractType, object identifier)
        {
            FlushBindings();

            var bindingId = new BindingId(contractType, identifier);

            return _providers.Remove(bindingId);
        }

        public void UnbindInterfacesTo<TConcrete>()
        {
            UnbindInterfacesTo(typeof(TConcrete));
        }

        public void UnbindInterfacesTo(Type concreteType)
        {
            foreach (var i in concreteType.Interfaces())
            {
                Unbind(i, concreteType);
            }
        }

        public bool Unbind<TContract, TConcrete>()
        {
            return Unbind(typeof(TContract), typeof(TConcrete));
        }

        public bool Unbind(Type contractType, Type concreteType)
        {
            return UnbindId(contractType, concreteType, null);
        }

        public bool UnbindId<TContract, TConcrete>(object identifier)
        {
            return UnbindId(typeof(TContract), typeof(TConcrete), identifier);
        }

        public bool UnbindId(Type contractType, Type concreteType, object identifier)
        {
            FlushBindings();

            var bindingId = new BindingId(contractType, identifier);

            List<ProviderInfo> providers;

            if (!_providers.TryGetValue(bindingId, out providers))
            {
                return false;
            }

            var matches = providers.Where(x => x.Provider.GetInstanceType(new InjectContext(this, contractType, identifier)).DerivesFromOrEqual(concreteType)).ToList();

            if (matches.IsEmpty())
            {
                return false;
            }

            foreach (var info in matches)
            {
                bool success = providers.Remove(info);
                Assert.That(success);
            }

            return true;
        }

        // Returns true if the given type is bound to something in the container
        public bool HasBinding(InjectContext context)
        {
            Assert.IsNotNull(context);

            FlushBindings();

            return GetProviderMatchesInternal(context).HasAtLeast(1);
        }

        public bool HasBinding<TContract>()
        {
            return HasBinding(typeof(TContract));
        }

        public bool HasBinding(Type contractType)
        {
            return HasBindingId(contractType, null);
        }

        public bool HasBindingId<TContract>(object identifier)
        {
            return HasBindingId(typeof(TContract), identifier);
        }

        public bool HasBindingId(Type contractType, object identifier)
        {
            return HasBinding(
                new InjectContext(this, contractType, identifier));
        }

        // Do not use this - it is for internal use only
        public void FlushBindings()
        {
            while (!_currentBindings.IsEmpty())
            {
                var binding = _currentBindings.Dequeue();

                _isFinalizingBinding = true;

                try
                {
                    binding.FinalizeBinding(this);
                }
                finally
                {
                    _isFinalizingBinding = false;
                }

                if (binding.CopyIntoAllSubContainers)
                {
                    _childBindings.Add(binding);
                }
            }
        }

        public BindFinalizerWrapper StartBinding()
        {
            Assert.That(!_isFinalizingBinding,
                "Attempted to start a binding during a binding finalizer.  This is not allowed, since binding finalizers should directly use AddProvider instead, to allow for bindings to be inherited properly without duplicates");

            FlushBindings();

            var bindingFinalizer = new BindFinalizerWrapper();
            _currentBindings.Enqueue(bindingFinalizer);
            return bindingFinalizer;
        }

        public ConcreteBinderGeneric<TContract> Rebind<TContract>()
        {
            return RebindId<TContract>(null);
        }

        public ConcreteBinderGeneric<TContract> RebindId<TContract>(object identifier)
        {
            UnbindId<TContract>(identifier);
            return Bind<TContract>().WithId(identifier);
        }

        public ConcreteBinderNonGeneric Rebind(Type contractType)
        {
            return RebindId(contractType, null);
        }

        public ConcreteBinderNonGeneric RebindId(Type contractType, object identifier)
        {
            UnbindId(contractType, identifier);
            return Bind(contractType).WithId(identifier);
        }

        // Map the given type to a way of obtaining it
        // Note that this can include open generic types as well such as List<>
        public ConcreteIdBinderGeneric<TContract> Bind<TContract>()
        {
            return Bind<TContract>(
                new BindInfo(typeof(TContract)));
        }

        internal ConcreteIdBinderGeneric<TContract> Bind<TContract>(BindInfo bindInfo)
        {
            Assert.That(!typeof(TContract).DerivesFrom<IPlaceholderFactory>(),
                "You should not use Container.Bind for factory classes.  Use Container.BindFactory instead.");
            Assert.That(bindInfo.ContractTypes.Contains(typeof(TContract)));

            return new ConcreteIdBinderGeneric<TContract>(
                bindInfo, StartBinding());
        }

        // Non-generic version of Bind<> for cases where you only have the runtime type
        // Note that this can include open generic types as well such as List<>
        public ConcreteIdBinderNonGeneric Bind(params Type[] contractTypes)
        {
            return Bind((IEnumerable<Type>)contractTypes);
        }

        public ConcreteIdBinderNonGeneric Bind(IEnumerable<Type> contractTypes)
        {
            return BindInternal(contractTypes, null);
        }

        private ConcreteIdBinderNonGeneric BindInternal(
            IEnumerable<Type> contractTypes, string contextInfo)
        {
            return BindInternal(
                new BindInfo(contractTypes.ToList(), contextInfo));
        }

        private ConcreteIdBinderNonGeneric BindInternal(BindInfo bindInfo)
        {
            Assert.That(bindInfo.ContractTypes.All(x => !x.DerivesFrom<IPlaceholderFactory>()),
                "You should not use Container.Bind for factory classes.  Use Container.BindFactory instead.");

            return new ConcreteIdBinderNonGeneric(bindInfo, StartBinding());
        }

#if !(UNITY_WSA && ENABLE_DOTNET)

        public ConcreteIdBinderNonGeneric Bind(
            Action<ConventionSelectTypesBinder> generator)
        {
            var conventionBindInfo = new ConventionBindInfo();
            generator(new ConventionSelectTypesBinder(conventionBindInfo));

            var contractTypesList = conventionBindInfo.ResolveTypes();

            Assert.That(contractTypesList.All(x => !x.DerivesFrom<IPlaceholderFactory>()),
                "You should not use Container.Bind for factory classes.  Use Container.BindFactory instead.");

            var bindInfo = new BindInfo(contractTypesList);

            // This is nice because it allows us to do things like Bind(all interfaces).To<Foo>()
            // (though of course it would be more efficient to use BindInterfacesTo in this case)
            bindInfo.InvalidBindResponse = InvalidBindResponses.Skip;

            return new ConcreteIdBinderNonGeneric(bindInfo, StartBinding());
        }

#endif

        // Bind all the interfaces for the given type to the same thing.
        //
        // Example:
        //
        //    public class Foo : ITickable, IInitializable
        //    {
        //    }
        //
        //    Container.BindInterfacesTo<Foo>().AsSingle();
        //
        //  This line above is equivalent to the following:
        //
        //    Container.Bind<ITickable>().ToSingle<Foo>();
        //    Container.Bind<IInitializable>().ToSingle<Foo>();
        //
        // Note here that we do not bind Foo to itself.  For that, use BindInterfacesAndSelfTo
        public FromBinderNonGeneric BindInterfacesTo<T>()
        {
            return BindInterfacesTo(typeof(T));
        }

        public FromBinderNonGeneric BindInterfacesTo(Type type)
        {
            var bindInfo = new BindInfo(
                type.Interfaces().ToList(), "BindInterfacesTo({0})".Fmt(type));

            // Almost always, you don't want to use the default AsTransient so make them type it
            bindInfo.RequireExplicitScope = true;
            return BindInternal(bindInfo).To(type);
        }

        // Same as BindInterfaces except also binds to self
        public FromBinderNonGeneric BindInterfacesAndSelfTo<T>()
        {
            return BindInterfacesAndSelfTo(typeof(T));
        }

        public FromBinderNonGeneric BindInterfacesAndSelfTo(Type type)
        {
            var bindInfo = new BindInfo(
                type.Interfaces().Concat(new[] { type }).ToList(), "BindInterfacesAndSelfTo({0})".Fmt(type));

            // Almost always, you don't want to use the default AsTransient so make them type it
            bindInfo.RequireExplicitScope = true;
            return BindInternal(bindInfo).To(type);
        }

        //  This is simply a shortcut to using the FromInstance method.
        //
        //  Example:
        //      Container.BindInstance(new Foo());
        //
        //  This line above is equivalent to the following:
        //
        //      Container.Bind<Foo>().FromInstance(new Foo());
        //
        public IdScopeConditionCopyNonLazyBinder BindInstance<TContract>(TContract instance)
        {
            var bindInfo = new BindInfo(typeof(TContract));
            var binding = StartBinding();

            binding.SubFinalizer = new ScopableBindingFinalizer(
                bindInfo, SingletonTypes.FromInstance, instance,
                (container, type) => new InstanceProvider(type, instance, container));

            return new IdScopeConditionCopyNonLazyBinder(bindInfo);
        }

        // Unfortunately we can't support setting scope / condition / etc. here since all the
        // bindings are finalized one at a time
        public void BindInstances(params object[] instances)
        {
            foreach (var instance in instances)
            {
                Assert.That(!ZenUtilInternal.IsNull(instance),
                    "Found null instance provided to BindInstances method");

                Bind(instance.GetType()).FromInstance(instance);
            }
        }

        private FactoryToChoiceIdBinder<TContract> BindFactoryInternal<TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : TFactoryContract, IFactory
            where TFactoryContract : IFactory
        {
            var bindInfo = new BindInfo(typeof(TFactoryContract));
            var factoryBindInfo = new FactoryBindInfo(typeof(TFactoryConcrete));

            StartBinding().SubFinalizer = new PlaceholderFactoryBindingFinalizer<TContract>(
                bindInfo, factoryBindInfo);

            return new FactoryToChoiceIdBinder<TContract>(
                bindInfo, factoryBindInfo);
        }

        public FactoryToChoiceIdBinder<TContract> BindIFactory<TContract>()
        {
            return BindFactoryInternal<TContract, IFactory<TContract>, Factory<TContract>>();
        }

        public FactoryToChoiceIdBinder<TContract> BindFactory<TContract, TFactory>()
            where TFactory : Factory<TContract>
        {
            return BindFactoryInternal<TContract, TFactory, TFactory>();
        }

        public FactoryToChoiceIdBinder<TContract> BindFactoryContract<TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : Factory<TContract>, TFactoryContract
            where TFactoryContract : IFactory
        {
            return BindFactoryInternal<TContract, TFactoryContract, TFactoryConcrete>();
        }

        public MemoryPoolInitialSizeBinder<TItemContract> BindMemoryPool<TItemContract>()
        {
            return BindMemoryPool<TItemContract, MemoryPool<TItemContract>>();
        }

        public MemoryPoolInitialSizeBinder<TItemContract> BindMemoryPool<TItemContract, TPool>()
            where TPool : IMemoryPool
        {
            return BindMemoryPool<TItemContract, TPool, TPool>();
        }

        public MemoryPoolInitialSizeBinder<TItemContract> BindMemoryPool<TItemContract, TPoolConcrete, TPoolContract>()
            where TPoolConcrete : TPoolContract, IMemoryPool
            where TPoolContract : IMemoryPool
        {
            var bindInfo = new BindInfo(typeof(TPoolContract));

            // This interface is used in the optional class PoolCleanupChecker
            // And also allow people to manually call DespawnAll() for all IMemoryPool
            // if they want
            bindInfo.ContractTypes.Add(typeof(IMemoryPool));

            var factoryBindInfo = new FactoryBindInfo(typeof(TPoolConcrete));
            var poolBindInfo = new MemoryPoolBindInfo();

            StartBinding().SubFinalizer = new MemoryPoolBindingFinalizer<TItemContract>(
                bindInfo, factoryBindInfo, poolBindInfo);

            return new MemoryPoolInitialSizeBinder<TItemContract>(
                bindInfo, factoryBindInfo, poolBindInfo);
        }

        private FactoryToChoiceIdBinder<TParam1, TContract> BindFactoryInternal<TParam1, TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : TFactoryContract, IFactory
            where TFactoryContract : IFactory
        {
            var bindInfo = new BindInfo(typeof(TFactoryContract));
            var factoryBindInfo = new FactoryBindInfo(typeof(TFactoryConcrete));

            StartBinding().SubFinalizer = new PlaceholderFactoryBindingFinalizer<TContract>(
                bindInfo, factoryBindInfo);

            return new FactoryToChoiceIdBinder<TParam1, TContract>(
                bindInfo, factoryBindInfo);
        }

        public FactoryToChoiceIdBinder<TParam1, TContract> BindIFactory<TParam1, TContract>()
        {
            return BindFactoryInternal<
                TParam1, TContract, IFactory<TParam1, TContract>, Factory<TParam1, TContract>>();
        }

        public FactoryToChoiceIdBinder<TParam1, TContract> BindFactory<TParam1, TContract, TFactory>()
            where TFactory : Factory<TParam1, TContract>
        {
            return BindFactoryInternal<
                TParam1, TContract, TFactory, TFactory>();
        }

        public FactoryToChoiceIdBinder<TParam1, TContract> BindFactoryContract<TParam1, TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : Factory<TParam1, TContract>, TFactoryContract
            where TFactoryContract : IFactory
        {
            return BindFactoryInternal<TParam1, TContract, TFactoryContract, TFactoryConcrete>();
        }

        private FactoryToChoiceIdBinder<TParam1, TParam2, TContract> BindFactoryInternal<TParam1, TParam2, TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : TFactoryContract, IFactory
            where TFactoryContract : IFactory
        {
            var bindInfo = new BindInfo(typeof(TFactoryContract));
            var factoryBindInfo = new FactoryBindInfo(typeof(TFactoryConcrete));

            StartBinding().SubFinalizer = new PlaceholderFactoryBindingFinalizer<TContract>(
                bindInfo, factoryBindInfo);

            return new FactoryToChoiceIdBinder<TParam1, TParam2, TContract>(
                bindInfo, factoryBindInfo);
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TContract> BindIFactory<TParam1, TParam2, TContract>()
        {
            return BindFactoryInternal<
                TParam1, TParam2, TContract, IFactory<TParam1, TParam2, TContract>, Factory<TParam1, TParam2, TContract>>();
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TContract> BindFactory<TParam1, TParam2, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TContract>
        {
            return BindFactoryInternal<
                TParam1, TParam2, TContract, TFactory, TFactory>();
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TContract> BindFactoryContract<TParam1, TParam2, TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : Factory<TParam1, TParam2, TContract>, TFactoryContract
            where TFactoryContract : IFactory
        {
            return BindFactoryInternal<TParam1, TParam2, TContract, TFactoryContract, TFactoryConcrete>();
        }

        private FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TContract> BindFactoryInternal<TParam1, TParam2, TParam3, TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : TFactoryContract, IFactory
            where TFactoryContract : IFactory
        {
            var bindInfo = new BindInfo(typeof(TFactoryContract));
            var factoryBindInfo = new FactoryBindInfo(typeof(TFactoryConcrete));

            StartBinding().SubFinalizer = new PlaceholderFactoryBindingFinalizer<TContract>(
                bindInfo, factoryBindInfo);

            return new FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TContract>(
                bindInfo, factoryBindInfo);
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TContract> BindIFactory<TParam1, TParam2, TParam3, TContract>()
        {
            return BindFactoryInternal<
                TParam1, TParam2, TParam3, TContract, IFactory<TParam1, TParam2, TParam3, TContract>, Factory<TParam1, TParam2, TParam3, TContract>>();
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TContract> BindFactory<TParam1, TParam2, TParam3, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TParam3, TContract>
        {
            return BindFactoryInternal<
                TParam1, TParam2, TParam3, TContract, TFactory, TFactory>();
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TContract> BindFactoryContract<TParam1, TParam2, TParam3, TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : Factory<TParam1, TParam2, TParam3, TContract>, TFactoryContract
            where TFactoryContract : IFactory
        {
            return BindFactoryInternal<TParam1, TParam2, TParam3, TContract, TFactoryContract, TFactoryConcrete>();
        }

        private FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindFactoryInternal<TParam1, TParam2, TParam3, TParam4, TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : TFactoryContract, IFactory
            where TFactoryContract : IFactory
        {
            var bindInfo = new BindInfo(typeof(TFactoryContract));
            var factoryBindInfo = new FactoryBindInfo(typeof(TFactoryConcrete));

            StartBinding().SubFinalizer = new PlaceholderFactoryBindingFinalizer<TContract>(
                bindInfo, factoryBindInfo);

            return new FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TContract>(
                bindInfo, factoryBindInfo);
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindIFactory<TParam1, TParam2, TParam3, TParam4, TContract>()
        {
            return BindFactoryInternal<
                TParam1, TParam2, TParam3, TParam4, TContract, IFactory<TParam1, TParam2, TParam3, TParam4, TContract>, Factory<TParam1, TParam2, TParam3, TParam4, TContract>>();
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindFactory<TParam1, TParam2, TParam3, TParam4, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TParam3, TParam4, TContract>
        {
            return BindFactoryInternal<
                TParam1, TParam2, TParam3, TParam4, TContract, TFactory, TFactory>();
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindFactoryContract<TParam1, TParam2, TParam3, TParam4, TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : Factory<TParam1, TParam2, TParam3, TParam4, TContract>, TFactoryContract
            where TFactoryContract : IFactory
        {
            return BindFactoryInternal<TParam1, TParam2, TParam3, TParam4, TContract, TFactoryContract, TFactoryConcrete>();
        }

        private FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract> BindFactoryInternal<TParam1, TParam2, TParam3, TParam4, TParam5, TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : TFactoryContract, IFactory
            where TFactoryContract : IFactory
        {
            var bindInfo = new BindInfo(typeof(TFactoryContract));
            var factoryBindInfo = new FactoryBindInfo(typeof(TFactoryConcrete));

            StartBinding().SubFinalizer = new PlaceholderFactoryBindingFinalizer<TContract>(
                bindInfo, factoryBindInfo);

            return new FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>(
                bindInfo, factoryBindInfo);
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract> BindIFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>()
        {
            return BindFactoryInternal<
                TParam1, TParam2, TParam3, TParam4, TParam5, TContract, IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>, Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>>();
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract> BindFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>
        {
            return BindFactoryInternal<
                TParam1, TParam2, TParam3, TParam4, TParam5, TContract, TFactory, TFactory>();
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract> BindFactoryContract<TParam1, TParam2, TParam3, TParam4, TParam5, TContract, TFactoryContract, TFactoryConcrete>()
            where TFactoryConcrete : Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>, TFactoryContract
            where TFactoryContract : IFactory
        {
            return BindFactoryInternal<TParam1, TParam2, TParam3, TParam4, TParam5, TContract, TFactoryContract, TFactoryConcrete>();
        }

        public T InstantiateExplicit<T>(List<TypeValuePair> extraArgs)
        {
            return (T)InstantiateExplicit(typeof(T), extraArgs);
        }

        public object InstantiateExplicit(Type concreteType, List<TypeValuePair> extraArgs)
        {
            bool autoInject = true;

            return InstantiateExplicit(
                concreteType,
                autoInject,
                new InjectArgs()
                {
                    ExtraArgs = extraArgs,
                    Context = new InjectContext(this, concreteType, null),
                    ConcreteIdentifier = null,
                });
        }

        public object InstantiateExplicit(Type concreteType, bool autoInject, InjectArgs args)
        {
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start("Zenject.Instantiate({0})", concreteType))
#endif
            {
                if (IsValidating)
                {
                    try
                    {
                        return InstantiateInternal(concreteType, autoInject, args);
                    }
                    catch (Exception e)
                    {
                        // Just log the error and continue to print multiple validation errors
                        // at once
                        ModestTree.Log.ErrorException(e);
                        return new ValidationMarker(concreteType, true);
                    }
                }
                else
                {
                    return InstantiateInternal(concreteType, autoInject, args);
                }
            }
        }

#if !NOT_UNITY3D

        public Component InstantiateComponentExplicit(
            Type componentType, GameObject gameObject, List<TypeValuePair> extraArgs)
        {
            Assert.That(componentType.DerivesFrom<Component>());

            FlushBindings();

            var monoBehaviour = (Component)gameObject.AddComponent(componentType);
            InjectExplicit(monoBehaviour, extraArgs);
            return monoBehaviour;
        }

        public object InstantiateScriptableObjectResourceExplicit(
            Type scriptableObjectType, string resourcePath, List<TypeValuePair> extraArgs)
        {
            var objects = Resources.LoadAll(resourcePath, scriptableObjectType);

            Assert.That(!objects.IsEmpty(),
                "Could not find resource at path '{0}' with type '{1}'", resourcePath, scriptableObjectType);

            Assert.That(objects.Length == 1,
                "Found multiple scriptable objects at path '{0}' when only 1 was expected with type '{1}'", resourcePath, scriptableObjectType);

            var newObj = ScriptableObject.Instantiate(objects.Single());

            InjectExplicit(newObj, extraArgs);

            return newObj;
        }

        // Same as InstantiatePrefabResourceForComponent except allows null values
        // to be included in the argument list.  Also see InjectUtil.CreateArgList
        public object InstantiatePrefabResourceForComponentExplicit(
            Type componentType, string resourcePath, List<TypeValuePair> extraArgs,
            GameObjectCreationParameters creationInfo)
        {
            return InstantiatePrefabResourceForComponentExplicit(
                componentType, resourcePath,
                new InjectArgs()
                {
                    ExtraArgs = extraArgs,
                    Context = new InjectContext(this, componentType, null),
                }, creationInfo);
        }

        public object InstantiatePrefabResourceForComponentExplicit(
            Type componentType, string resourcePath, InjectArgs args,
            GameObjectCreationParameters creationInfo)
        {
            var prefab = (GameObject)Resources.Load(resourcePath);
            Assert.IsNotNull(prefab,
                "Could not find prefab at resource location '{0}'".Fmt(resourcePath));
            return InstantiatePrefabForComponentExplicit(
                componentType, prefab, args, creationInfo);
        }

        public object InstantiatePrefabForComponentExplicit(
            Type componentType, UnityEngine.Object prefab,
            List<TypeValuePair> extraArgs)
        {
            return InstantiatePrefabForComponentExplicit(
                componentType, prefab, extraArgs, GameObjectCreationParameters.Default);
        }

        public object InstantiatePrefabForComponentExplicit(
            Type componentType, UnityEngine.Object prefab,
            List<TypeValuePair> extraArgs, GameObjectCreationParameters gameObjectBindInfo)
        {
            return InstantiatePrefabForComponentExplicit(
                componentType, prefab,
                new InjectArgs()
                {
                    ExtraArgs = extraArgs,
                    Context = new InjectContext(this, componentType, null),
                }, gameObjectBindInfo);
        }

        // Same as InstantiatePrefabForComponent except allows null values
        // to be included in the argument list.  Also see InjectUtil.CreateArgList
        public object InstantiatePrefabForComponentExplicit(
            Type componentType, UnityEngine.Object prefab,
            InjectArgs args, GameObjectCreationParameters gameObjectBindInfo)
        {
            Assert.That(!AssertOnNewGameObjects,
                "Given DiContainer does not support creating new game objects");

            FlushBindings();

            Assert.That(componentType.IsInterface() || componentType.DerivesFrom<Component>(),
                "Expected type '{0}' to derive from UnityEngine.Component", componentType);

            bool shouldMakeActive;
            var gameObj = CreateAndParentPrefab(prefab, gameObjectBindInfo, args.Context, out shouldMakeActive);

            var component = InjectGameObjectForComponentExplicit(
                gameObj, componentType, args);

            if (shouldMakeActive)
            {
                gameObj.SetActive(true);
            }

            return component;
        }

#endif

        ////////////// Execution order ////////////////

        public void BindExecutionOrder<T>(int order)
        {
            BindExecutionOrder(typeof(T), order);
        }

        public void BindExecutionOrder(Type type, int order)
        {
            Assert.That(type.DerivesFrom<ITickable>() || type.DerivesFrom<IInitializable>() || type.DerivesFrom<IDisposable>() || type.DerivesFrom<ILateDisposable>() || type.DerivesFrom<IFixedTickable>() || type.DerivesFrom<ILateTickable>(),
                "Expected type '{0}' to derive from one or more of the following interfaces: ITickable, IInitializable, ILateTickable, IFixedTickable, IDisposable, ILateDisposable", type);

            if (type.DerivesFrom<ITickable>())
            {
                BindTickableExecutionOrder(type, order);
            }

            if (type.DerivesFrom<IInitializable>())
            {
                BindInitializableExecutionOrder(type, order);
            }

            if (type.DerivesFrom<IDisposable>())
            {
                BindDisposableExecutionOrder(type, order);
            }

            if (type.DerivesFrom<ILateDisposable>())
            {
                BindLateDisposableExecutionOrder(type, order);
            }

            if (type.DerivesFrom<IFixedTickable>())
            {
                BindFixedTickableExecutionOrder(type, order);
            }

            if (type.DerivesFrom<ILateTickable>())
            {
                BindLateTickableExecutionOrder(type, order);
            }
        }

        public void BindTickableExecutionOrder<T>(int order)
            where T : ITickable
        {
            BindTickableExecutionOrder(typeof(T), order);
        }

        public void BindTickableExecutionOrder(Type type, int order)
        {
            Assert.That(type.DerivesFrom<ITickable>(),
                "Expected type '{0}' to derive from ITickable", type);

            BindInstance(
                ModestTree.Util.ValuePair.New(type, order)).WhenInjectedInto<TickableManager>();
        }

        public void BindInitializableExecutionOrder<T>(int order)
            where T : IInitializable
        {
            BindInitializableExecutionOrder(typeof(T), order);
        }

        public void BindInitializableExecutionOrder(Type type, int order)
        {
            Assert.That(type.DerivesFrom<IInitializable>(),
                "Expected type '{0}' to derive from IInitializable", type);

            BindInstance(
                ModestTree.Util.ValuePair.New(type, order)).WhenInjectedInto<InitializableManager>();
        }

        public void BindDisposableExecutionOrder<T>(int order)
            where T : IDisposable
        {
            BindDisposableExecutionOrder(typeof(T), order);
        }

        public void BindLateDisposableExecutionOrder<T>(int order)
            where T : ILateDisposable
        {
            BindLateDisposableExecutionOrder(typeof(T), order);
        }

        public void BindDisposableExecutionOrder(Type type, int order)
        {
            Assert.That(type.DerivesFrom<IDisposable>(),
                "Expected type '{0}' to derive from IDisposable", type);

            BindInstance(
                ModestTree.Util.ValuePair.New(type, order)).WhenInjectedInto<DisposableManager>();
        }

        public void BindLateDisposableExecutionOrder(Type type, int order)
        {
            Assert.That(type.DerivesFrom<ILateDisposable>(),
            "Expected type '{0}' to derive from ILateDisposable", type);

            BindInstance(
                ModestTree.Util.ValuePair.New(type, order)).WithId("Late").WhenInjectedInto<DisposableManager>();
        }

        public void BindFixedTickableExecutionOrder<T>(int order)
            where T : IFixedTickable
        {
            BindFixedTickableExecutionOrder(typeof(T), order);
        }

        public void BindFixedTickableExecutionOrder(Type type, int order)
        {
            Assert.That(type.DerivesFrom<IFixedTickable>(),
                "Expected type '{0}' to derive from IFixedTickable", type);

            Bind<ModestTree.Util.ValuePair<Type, int>>().WithId("Fixed")
                .FromInstance(ModestTree.Util.ValuePair.New(type, order)).WhenInjectedInto<TickableManager>();
        }

        public void BindLateTickableExecutionOrder<T>(int order)
            where T : ILateTickable
        {
            BindLateTickableExecutionOrder(typeof(T), order);
        }

        public void BindLateTickableExecutionOrder(Type type, int order)
        {
            Assert.That(type.DerivesFrom<ILateTickable>(),
                "Expected type '{0}' to derive from ILateTickable", type);

            Bind<ModestTree.Util.ValuePair<Type, int>>().WithId("Late")
                .FromInstance(ModestTree.Util.ValuePair.New(type, order)).WhenInjectedInto<TickableManager>();
        }

        ////////////// Types ////////////////

        private class ProviderPair
        {
            public ProviderPair(
                ProviderInfo providerInfo,
                DiContainer container)
            {
                ProviderInfo = providerInfo;
                Container = container;
            }

            public ProviderInfo ProviderInfo
            {
                get;
                private set;
            }

            public DiContainer Container
            {
                get;
                private set;
            }
        }

        public enum ProviderLookupResult
        {
            Success,
            Multiple,
            None
        }

        private struct LookupId
        {
            public readonly IProvider Provider;
            public readonly BindingId BindingId;

            public LookupId(
                IProvider provider, BindingId bindingId)
            {
                Provider = provider;
                BindingId = bindingId;
            }
        }

        public class ProviderInfo
        {
            public ProviderInfo(IProvider provider, BindingCondition condition, bool nonLazy)
            {
                Provider = provider;
                Condition = condition;
                NonLazy = nonLazy;
            }

            public bool NonLazy
            {
                get;
                private set;
            }

            public IProvider Provider
            {
                get;
                private set;
            }

            public BindingCondition Condition
            {
                get;
                private set;
            }
        }
    }
}