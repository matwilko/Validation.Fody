namespace Validation.Fody.Internals
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal sealed class Injector
    {
        private IDictionary<Type, Delegate> Services { get; } = new Dictionary<Type, Delegate>();

        public void RegisterOpenGeneric(Type openGeneric, Type implementation)
        {
            Func<Type[], Type, object> serviceDelegate = (genericArguments, dependingType) =>
            {
                var serviceType = implementation.MakeGenericType(genericArguments);
                return Resolve(serviceType, dependingType);
            };

            RegisterOpenGeneric(openGeneric, implementation, serviceDelegate);
        }

        public void RegisterOpenGeneric(Type openGeneric, Type implementation, Func<Type[], Type, object> genericFactory)
        {
            if (!openGeneric.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Must be an open generic type", nameof(openGeneric));
            }

            if (!implementation.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Must be an open generic type", nameof(implementation));
            }

            if (openGeneric.IsAssignableFrom(implementation) || openGeneric.GetGenericArguments().Length != implementation.GetGenericArguments().Length)
            {
                throw new ArgumentException("Must be compatible with the open generic type", nameof(implementation));
            }

            if ((!implementation.IsClass || implementation.IsAbstract) && !implementation.IsValueType)
            {
                throw new ArgumentException("Must be an instantiable type", nameof(implementation));
            }

            Services.Add(openGeneric, genericFactory);
        }

        public void Register<TService, TImplementation>()
            where TImplementation : TService
        {
            // Self-binding doesn't require an explicit service registration (and would lead to infinite recursion in Resolve anyway)
            if (typeof (TService) != typeof (TImplementation))
            {
                Register<TService, TImplementation>(InternalResolve<TImplementation>);
            }
        }

        public void Register<TService>(TService instance)
        {
            Register<TService, TService>(() => instance);
        }

        public void Register<TService, TImplementation>(Func<TImplementation> factory)
            where TImplementation : TService
        {
            Register<TService, TImplementation>(type => factory());
        }

        public void Register<TService, TImplementation>(Func<Type, TImplementation> factory)
            where TImplementation : TService
        {
            Func<Type, TService> serviceDelegate = type => factory(type);
            Services.Add(typeof (TService), serviceDelegate);
        }

        public TService Resolve<TService>() => InternalResolve<TService>(null);

        private TService InternalResolve<TService>(Type dependingType)
        {
            var serviceType = typeof (TService);

            Delegate factory;
            if (Services.TryGetValue(serviceType, out factory))
            {
                return ((Func<Type, TService>) factory)(dependingType);
            }
            else if (serviceType.IsGenericType && Services.TryGetValue(serviceType.GetGenericTypeDefinition(), out factory))
            {
                return (TService) ((Func<Type[], Type, object>) factory)(serviceType.GetGenericArguments(), dependingType);
            }
            else if (!serviceType.IsValueType && (!serviceType.IsClass || serviceType.IsAbstract))
            {
                throw new InvalidOperationException($"Could not resolve the type `{serviceType.FullName}` as it is either not registered in the injector or not self-bindable as it is not instantiable");
            }
            else
            {
                return BindClass<TService>();
            }
        }

        private static ConcurrentDictionary<Type, Func<Injector, Type, object>> ResolveFuncs { get; } = new ConcurrentDictionary<Type, Func<Injector, Type, object>>();

        private object Resolve(Type type, Type dependingType)
        {
            var func = ResolveFuncs.GetOrAdd(type, t => (Func<Injector, Type, object>) typeof (Injector)
                .GetMethod("InternalResolve", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(t)
                    .CreateDelegate(typeof (Func<Injector, Type, object>))
                );

            return func(this, dependingType);
        }

        private TService BindClass<TService>()
        {
            var serviceType = typeof (TService);
            var constructors = serviceType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .OrderByDescending(c => c.GetParameters().Length);

            var bindingExceptions = new List<InvalidOperationException>();

            foreach (var constructor in constructors)
            {
                var constructorParameters = constructor.GetParameters();
                var parameters = new object[constructorParameters.Length];
                bool success = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        parameters[i] = Resolve(constructorParameters[i].ParameterType, typeof(TService));
                    }
                    catch (InvalidOperationException bindingException)
                    {
                        success = false;
                        bindingExceptions.Add(bindingException);
                    }
                }

                if (success)
                {
                    return (TService) constructor.Invoke(parameters);
                }
            }

            throw new InvalidOperationException($"Could not resolve the type `{serviceType.FullName}` because none of its constructors had dependencies that could be fully satisfied", new AggregateException(bindingExceptions));
        }
    }
}
