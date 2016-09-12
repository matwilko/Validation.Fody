namespace Validation.Fody.Tests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    internal static class Method
    {
        private static Action<object> WrapMethod(MethodInfo method, object instance = null)
        {
            var dynamicMethod = new DynamicMethod(
                name: Guid.NewGuid().ToString(),
                returnType: typeof(void),
                parameterTypes: method.IsStatic ? new [] { typeof(object) } : new [] { instance.GetType(), typeof(object) }
            );

            var methodParam = method.GetParameters().Single().ParameterType;

            var generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);

            if (!method.IsStatic)
            {
                generator.Emit(OpCodes.Ldarg_1);
            }
            
            if (methodParam.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, methodParam);
            }
            else
            {
                generator.Emit(OpCodes.Castclass, methodParam);
            }

            generator.Emit(OpCodes.Call, method);
            if (method.ReturnType != typeof(void))
            {
                generator.Emit(OpCodes.Pop);
            }

            generator.Emit(OpCodes.Ret);

            var del = method.IsStatic
                ? dynamicMethod.CreateDelegate(typeof(Action<object>))
                : dynamicMethod.CreateDelegate(typeof(Action<object>), instance);

            return (Action<object>) del;
        }

        public static Action<object> FromProperty(Assembly assembly, string typeName, string propertyName)
        {
            var type = assembly.GetType(typeName);
            var propertySetMethod = type.GetProperty(propertyName).SetMethod;
            if (propertySetMethod.IsStatic)
            {
                return WrapMethod(propertySetMethod);
            }
            else
            {
                var instance = Activator.CreateInstance(type);
                return WrapMethod(propertySetMethod, instance);
            }
        }

        public static Action<T> FromProperty<T>(Assembly assembly, string typeName, string propertyName)
        {
            var type = assembly.GetType(typeName);
            var propertySetMethod = type.GetProperty(propertyName).SetMethod;
            if (propertySetMethod.IsStatic)
            {
                return (Action<T>) propertySetMethod.CreateDelegate(typeof(Action<T>));
            }
            else
            {
                var instance = Activator.CreateInstance(type);
                return (Action<T>) propertySetMethod.CreateDelegate(typeof(Action<T>), instance);
            }
        }

        public static Action<object> FromMethod(Assembly assembly, string typeName, string methodName)
        {
            var type = assembly.GetType(typeName);
            var method = type.GetMethod(methodName);
            if (method.IsStatic)
            {
                return WrapMethod(method);
            }
            else
            {
                var instance = Activator.CreateInstance(type);
                return WrapMethod(method, instance);
            }
        }

        private static TAction FromMethodInternal<TAction>(Assembly assembly, string typeName, string methodName)
        {
            var type = assembly.GetType(typeName);
            var method = type.GetMethod(methodName);
            if (method.IsStatic)
            {
                return (TAction) (object) method.CreateDelegate(typeof(TAction));
            }
            else
            {
                var instance = Activator.CreateInstance(type);
                return (TAction) (object) method.CreateDelegate(typeof(TAction), instance);
            }
        }
        
        public static Action<T> FromMethod<T>(Assembly assembly, string typeName, string methodName)
            => FromMethodInternal<Action<T>>(assembly, typeName, methodName);

        public static Action<T1, T2> FromMethod<T1, T2>(Assembly assembly, string typeName, string methodName)
            => FromMethodInternal<Action<T1, T2>>(assembly, typeName, methodName);

        public static Action<T1, T2, T3> FromMethod<T1, T2, T3>(Assembly assembly, string typeName, string methodName)
                => FromMethodInternal<Action<T1, T2, T3>>(assembly, typeName, methodName);

        public static Action<T1, T2, T3, T4> FromMethod<T1, T2, T3, T4>(Assembly assembly, string typeName, string methodName)
                => FromMethodInternal<Action<T1, T2, T3, T4>>(assembly, typeName, methodName);

        public static Action<T1, T2, T3, T4, T5> FromMethod<T1, T2, T3, T4, T5>(Assembly assembly, string typeName, string methodName)
                => FromMethodInternal<Action<T1, T2, T3, T4, T5>>(assembly, typeName, methodName);

        public static Action<T1, T2, T3, T4, T5, T6> FromMethod<T1, T2, T3, T4, T5, T6>(Assembly assembly, string typeName, string methodName)
                => FromMethodInternal<Action<T1, T2, T3, T4, T5, T6>>(assembly, typeName, methodName);

        public static Action<T1, T2, T3, T4, T5, T6, T7> FromMethod<T1, T2, T3, T4, T5, T6, T7>(Assembly assembly, string typeName, string methodName)
                => FromMethodInternal<Action<T1, T2, T3, T4, T5, T6, T7>>(assembly, typeName, methodName);

        public static Delegate FromConstructor(Assembly assembly, string typeName, Type[] parameterTypes)
        {
            var type = assembly.GetType(typeName);
            var constructor = type.GetConstructors()
                .Single(c => c.GetParameters().Length == parameterTypes.Length && c.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));


            var method = new DynamicMethod(
                name: "Construct_" + typeName,
                returnType: typeof(void),
                parameterTypes: parameterTypes.Select(t => typeof(object)).ToArray()
            );

            var generator = method.GetILGenerator();

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                generator.Emit(OpCodes.Ldarg_S, i);
                var paramType = parameterTypes[i];
                if (paramType.IsValueType)
                {
                    generator.Emit(OpCodes.Unbox_Any, paramType);
                }
                else
                {
                    generator.Emit(OpCodes.Castclass, paramType);
                }
            }

            generator.Emit(OpCodes.Newobj, constructor);
            generator.Emit(OpCodes.Pop);
            generator.Emit(OpCodes.Ret);

            var actionType = typeof(Action).Assembly.ExportedTypes
                .Where(t => t.Name.StartsWith("Action") && typeof(Delegate).IsAssignableFrom(t))
                .Single(t => t.GetGenericArguments().Length == parameterTypes.Length);
            
            return method.CreateDelegate(actionType.MakeGenericType(parameterTypes.Select(t => typeof(object)).ToArray()));
        }
        
        private static TAction FromConstructorInternal<TAction>(Assembly assembly, string typeName)
        {
            var type = assembly.GetType(typeName);
            var constructor = type.GetConstructor(typeof(TAction).GetGenericArguments());

            var method = new DynamicMethod(
                name: "Construct_" + typeName,
                returnType: typeof(void),
                parameterTypes: typeof(TAction).GetGenericArguments()
            );

            var generator = method.GetILGenerator();

            for(int i = 0; i < typeof(TAction).GetGenericArguments().Length; i++ )
            {
                generator.Emit(OpCodes.Ldarg_S, i);
            }

            generator.Emit(OpCodes.Newobj, constructor);
            generator.Emit(OpCodes.Pop);
            generator.Emit(OpCodes.Ret);

            return (TAction) (object) method.CreateDelegate(typeof(TAction));
        }
        
        public static Action<T> FromConstructor<T>(Assembly assembly, string typeName)
            => FromConstructorInternal<Action<T>>(assembly, typeName);

        public static Action<T1, T2> FromConstructor<T1, T2>(Assembly assembly, string typeName)
            => FromConstructorInternal<Action<T1, T2>>(assembly, typeName);

        public static Action<T1, T2, T3> FromConstructor<T1, T2, T3>(Assembly assembly, string typeName)
                => FromConstructorInternal<Action<T1, T2, T3>>(assembly, typeName);

        public static Action<T1, T2, T3, T4> FromConstructor<T1, T2, T3, T4>(Assembly assembly, string typeName)
                => FromConstructorInternal<Action<T1, T2, T3, T4>>(assembly, typeName);

        public static Action<T1, T2, T3, T4, T5> FromConstructor<T1, T2, T3, T4, T5>(Assembly assembly, string typeName)
                => FromConstructorInternal<Action<T1, T2, T3, T4, T5>>(assembly, typeName);

        public static Action<T1, T2, T3, T4, T5, T6> FromConstructor<T1, T2, T3, T4, T5, T6>(Assembly assembly, string typeName)
                => FromConstructorInternal<Action<T1, T2, T3, T4, T5, T6>>(assembly, typeName);

        public static Action<T1, T2, T3, T4, T5, T6, T7> FromConstructor<T1, T2, T3, T4, T5, T6, T7>(Assembly assembly, string typeName)
                => FromConstructorInternal<Action<T1, T2, T3, T4, T5, T6, T7>>(assembly, typeName);
    }
}
