using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;

namespace UniEasy.DI
{
    public delegate bool BindingCondition(InjectContext context);

    public class ProviderInfo
    {
        public ProviderInfo(IProvider provider, BindingCondition condition)
        {
            Provider = provider;
            Condition = condition;
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

    public class DiContainer
    {
        public static string DependencyRootIdentifier = "DependencyRoot";
        readonly Dictionary<BindingId, List<ProviderInfo>> providers = new Dictionary<BindingId, List<ProviderInfo>>();
        readonly SingletonProviderCreator singletonProviderCreator;
        readonly Queue<IBindingFinalizer> currentBindings = new Queue<IBindingFinalizer>();
        readonly List<IBindingFinalizer> childBindings = new List<IBindingFinalizer>();

        public DiContainer()
        {
            singletonProviderCreator = new SingletonProviderCreator(this);
        }

        public DiContainer(DiContainer parentContainer)
        {
            singletonProviderCreator = new SingletonProviderCreator(this);
            if (parentContainer != null)
            {
                parentContainer.FlushBindings();
                for (int i = 0; i < parentContainer.childBindings.Count; i++)
                {
                    currentBindings.Enqueue(parentContainer.childBindings[i]);
                }

                FlushBindings();
            }
        }

        public DiContainer CreateSubContainer()
        {
            return new DiContainer(this);
        }

        public SingletonProviderCreator SingletonProviderCreator
        {
            get
            {
                return singletonProviderCreator;
            }
        }

        public T Instantiate<T>()
        {
            bool autoInject = true;
            return (T)Instantiate(typeof(T), autoInject);
        }

        public T Instantiate<T>(bool autoInject)
        {
            return (T)Instantiate(typeof(T), autoInject);
        }

        public object Instantiate(Type concreteType)
        {
            bool autoInject = true;
            return Instantiate(concreteType, autoInject);
        }

        public object Instantiate(Type concreteType, bool autoInject)
        {
            FlushBindings();

            var typeInfo = TypeAnalyzer.GetInfo(concreteType);

            object newObj = null;

            if (concreteType.DerivesFrom<ScriptableObject>())
            {
                newObj = ScriptableObject.CreateInstance(concreteType);
            }
            else
            {
                // Make a copy since we remove from it below
                var paramValues = new List<object>();
                var injectables = typeInfo.ConstructorInjectables.ToArray();
                for (int i = 0; i < injectables.Length; i++)
                {
                    object value = Resolve(injectables[i].CreateInjectContext(this, null));
                    paramValues.Add(value);
                }

                try
                {
                    newObj = typeInfo.InjectConstructor.Invoke(paramValues.ToArray());
                }
                catch
                {
                }
                if (newObj == null)
                    newObj = concreteType.GetDefaultValue();
            }

            if (autoInject)
            {
                Inject(newObj);
            }

            return newObj;
        }

        public void Inject(object injectable)
        {
            FlushBindings();
            var type = injectable.GetType();
            var typeInfo = TypeAnalyzer.GetInfo(type);

            var injectInfos = typeInfo.FieldInjectables.Concat(typeInfo.PropertyInjectables).ToArray();
            for (int i = 0; i < injectInfos.Length; i++)
            {
                var injectInfo = injectInfos[i];
                var injectContext = injectInfo.CreateInjectContext(this, injectable);
                injectInfo.Setter(injectable, Resolve(injectContext));
            }

            var postInjectMethods = typeInfo.PostInjectMethods.ToArray();
            for (int j = 0; j < postInjectMethods.Length; j++)
            {
                var paramValues = new List<object>();
                var injectableInfo = postInjectMethods[j].InjectableInfo.ToArray();
                for (int k = 0; k < injectableInfo.Length; k++)
                {
                    var value = Resolve(injectableInfo[k].CreateInjectContext(this, injectable));
                    paramValues.Add(value);
                }
                postInjectMethods[j].MethodInfo.Invoke(injectable, paramValues.ToArray());
            }
        }

        public void UnbindAll()
        {
            FlushBindings();
            providers.Clear();
        }

        public bool Unbind<TContract>()
        {
            return Unbind<TContract>(null);
        }

        public bool Unbind<TContract>(object identifier)
        {
            FlushBindings();

            var bindingId = new BindingId(typeof(TContract), identifier);

            return providers.Remove(bindingId);
        }

        // Returns true if the given type is bound to something in the container
        public bool HasBinding(InjectContext context)
        {
            FlushBindings();

            List<ProviderInfo> val;

            if (!providers.TryGetValue(context.GetBindingId(), out val))
            {
                return false;
            }

            return val.Where(p => p.Condition == null || p.Condition(context)).HasAtLeast(1);
        }

        public bool HasBinding<TContract>()
        {
            return HasBinding<TContract>(null);
        }

        public bool HasBinding<TContract>(object identifier)
        {
            return HasBinding(
                new InjectContext(this, typeof(TContract), identifier));
        }

        // Do not use this - it is for internal use only
        public void FlushBindings()
        {
            while (!currentBindings.IsEmpty())
            {
                var binding = currentBindings.Dequeue();
                binding.FinalizeBinding(this);

                childBindings.Add(binding);
            }
        }

        public BindFinalizerWrapper StartBinding()
        {
            FlushBindings();
            var bindingFinalizer = new BindFinalizerWrapper();
            currentBindings.Enqueue(bindingFinalizer);
            return bindingFinalizer;
        }

        public ConcreteBinderGeneric<TContract> Rebind<TContract>()
        {
            return Rebind<TContract>(null);
        }

        public ConcreteBinderGeneric<TContract> Rebind<TContract>(object identifier)
        {
            Unbind<TContract>(identifier);
            return Bind<TContract>().WithId(identifier);
        }

        public ConcreteIdBinderGeneric<TContract> Bind<TContract>()
        {
            var bindInfo = new BindInfo(typeof(TContract));
            return new ConcreteIdBinderGeneric<TContract>(bindInfo, StartBinding());
        }

        public ConcreteIdBinderNonGeneric Bind(params Type[] contractTypes)
        {
            return Bind((IEnumerable<Type>)contractTypes);
        }

        public ConcreteIdBinderNonGeneric Bind(IEnumerable<Type> contractTypes)
        {
            var contractTypesList = contractTypes.ToList();
            var bindInfo = new BindInfo(contractTypesList);
            return new ConcreteIdBinderNonGeneric(bindInfo, StartBinding());
        }

        // This is equivalent to calling NonLazy() at the end of your bind statement
        // It's only in rare cases where you need to call this instead of NonLazy()
        public void BindRootResolve<TContract>()
        {
            BindRootResolve<TContract>(null);
        }

        public void BindRootResolve<TContract>(object identifier)
        {
            BindRootResolve(identifier, new Type[] { typeof(TContract) });
        }

        public void BindRootResolve(IEnumerable<Type> rootTypes)
        {
            BindRootResolve(null, rootTypes);
        }

        public void BindRootResolve(object identifier, IEnumerable<Type> rootTypes)
        {
            // NonLazy param used Constructor Injection so don't distinguish with Id,
            // Add a DependencyRootIdentifier just to prevent another injection binded.
            Bind<object>().WithId(DependencyRootIdentifier).To(rootTypes).FromResolve(identifier);
        }

        public void RegisterProvider(BindingId bindingId, BindingCondition condition, IProvider provider)
        {
            var info = new ProviderInfo(provider, condition);

            if (providers.ContainsKey(bindingId))
            {
                providers[bindingId].Add(info);
            }
            else
            {
                providers.Add(bindingId, new List<ProviderInfo>() { info });
            }
        }

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

        public object Resolve(InjectContext context)
        {
            FlushBindings();
            IProvider provider;
            var result = TryGetUniqueProvider(context, out provider);
            if (result)
            {
                return SafeGetInstances(provider, context).SingleOrDefault();
            }
            return null;
        }

        public IList ResolveAll(InjectContext context)
        {
            FlushBindings();

            var matches = GetProviderMatchesInternal(context).ToList();

            if (matches.Any())
            {
                var instances = matches.SelectMany(m => SafeGetInstances(m.Provider, context)).ToArray();

                return ReflectionUtility.CreateGenericList(context.MemberType, instances);
            }

            return ReflectionUtility.CreateGenericList(context.MemberType, new object[] { });
        }

        IEnumerable<object> SafeGetInstances(IProvider provider, InjectContext context)
        {
            var runner = provider.GetAllInstancesWithInjectSplit(context);

            // First get instance
            bool hasMore = runner.MoveNext();

            var instances = runner.Current;

            // Now do injection
            while (hasMore)
            {
                hasMore = runner.MoveNext();
            }

            return instances;
        }

        internal bool TryGetUniqueProvider(InjectContext context, out IProvider provider)
        {
            var providers = GetProviderMatchesInternal(context).ToList();
            if (providers.IsEmpty())
            {
                provider = null;
                return false;
            }
            if (providers.Count > 1)
            {
                provider = providers.LastOrDefault().Provider;
            }
            else
            {
                provider = providers.Single().Provider;
            }
            return true;
        }

        IEnumerable<ProviderInfo> GetProviderMatchesInternal(InjectContext context)
        {
            return GetProvidersForContract(context.GetBindingId()).Where(p => p.Condition == null || p.Condition(context));
        }

        IEnumerable<ProviderInfo> GetProvidersForContract(BindingId bindingId)
        {
            return GetLocalProviders(bindingId).Select(p => p);
        }

        List<ProviderInfo> GetLocalProviders(BindingId bindingId)
        {
            List<ProviderInfo> localProviders;
            if (providers.TryGetValue(bindingId, out localProviders))
            {
                return localProviders;
            }
            return new List<ProviderInfo>();
        }
    }
}
