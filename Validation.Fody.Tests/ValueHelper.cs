namespace Validation.Fody.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    internal static class ValueHelper
    {
        private static Random Random { get; } = new Random();

        private static MethodInfo RandomValueForInfo { get; } = typeof(ValueHelper).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(m => m.Name == "RandomValueFor" && m.IsGenericMethod);
        private static object[] EmptyParameters { get; } = new object[0];

        public static object RandomValueFor(Type t) => RandomValueForInfo.MakeGenericMethod(t).Invoke(null, EmptyParameters);

        public static T RandomValueFor<T>()
        {
            Delegate generator;
            if (Generators.TryGetValue(typeof(T), out generator))
            {
                return ((Func<T>) generator)();
            }
            else if (typeof(T).IsValueType && typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var innerType = Nullable.GetUnderlyingType(typeof(T));
                return (T) typeof(ValueHelper).GetMethod(nameof(GetNullableValue), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(innerType)
                    .Invoke(null, EmptyParameters);
            }
            else if (typeof(T).IsEnum)
            {
                var values = Enum.GetValues(typeof(T));
                var valueIndex = Random.Next(0, values.Length);
                return (T) values.GetValue(valueIndex);
            }
            else if (typeof(T).IsArray)
            {
                var elementType = typeof(T).GetElementType();
                return (T) typeof(ValueHelper).GetMethod(nameof(CreateArrayOf), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(elementType)
                    .Invoke(null, new object[] { Random.Next(0, 10) });
            }
            else if (typeof(T) == typeof(Array) || typeof(T) == typeof(IEnumerable) || typeof(T) == typeof(IList) || typeof(T) == typeof(ICollection))
            {
                return (T) typeof(ValueHelper).GetMethod(nameof(CreateArrayOf), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(typeof(DummyObject))
                    .Invoke(null, new object[] { Random.Next(0, 10) });
            }
            else if (typeof(T).IsGenericOfType(typeof(IEnumerable<>), typeof(IList<>), typeof(IReadOnlyList<>), typeof(ICollection<>)))
            {
                var elementType = typeof(T).GetGenericArguments().Single();
                return (T)typeof(ValueHelper).GetMethod(nameof(CreateArrayOf), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(elementType)
                    .Invoke(null, new object[] { Random.Next(0, 10) });
            }
            else if (typeof(T).IsGenericOfType(typeof(List<>)))
            {
                var elementType = typeof(T).GetGenericArguments().Single();
                return (T) typeof(ValueHelper).GetMethod(nameof(CreateListOf), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(elementType)
                    .Invoke(null, new object[] { Random.Next(0, 10) });
            }
            else if (typeof(T) == typeof(Exception))
            {
                var randomExceptionType = Exceptions[Helpers.RandomValueBetween(0, Exceptions.Length)];
                return (T) ConstructFromDefaultConstructor(randomExceptionType);
            }
            else if (typeof(Exception).IsAssignableFrom(typeof(T)))
            {
                return (T)ConstructFromDefaultConstructor(typeof(T));
            }
            else if (typeof(T) == typeof(Attribute))
            {
                var randomAttributeType = Attributes[Helpers.RandomValueBetween(0, Attributes.Length)];
                return (T)ConstructFromDefaultConstructor(randomAttributeType);
            }
            else if (typeof(Attribute).IsAssignableFrom(typeof(T)))
            {
                return (T)ConstructFromDefaultConstructor(typeof(T));
            }
            else if (typeof(T) == typeof(Delegate))
            {
                Action del = DummyMethod;
                return (T) (object) del;
            }
            else if (typeof(Delegate).IsAssignableFrom(typeof(T)))
            {
                var invokeMethod = typeof(T).GetMethod("Invoke");
                var parameterTypes = invokeMethod.GetParameters().Select(p => p.ParameterType).ToArray();
                var returnType = invokeMethod.ReturnType;

                var dynamicMethod = new DynamicMethod(
                    name: Guid.NewGuid().ToString(),
                    returnType: returnType,
                    parameterTypes: parameterTypes
                );

                var gen = dynamicMethod.GetILGenerator();
                if (returnType != typeof(void))
                {
                    var local = gen.DeclareLocal(returnType);
                    gen.Emit(OpCodes.Ldloca_S, local);
                    gen.Emit(OpCodes.Initobj, returnType);
                    gen.Emit(OpCodes.Ldloca_S, local);
                }
                
                gen.Emit(OpCodes.Ret);

                return (T) (object) dynamicMethod.CreateDelegate(typeof(T));
            }
            else if (typeof(T) == typeof(Type))
            {
                return (T) (object) Types[Helpers.RandomValueBetween(0, Types.Length)];
            }
            else if (typeof(T).GetConstructors().Any(c => c.GetParameters().Length == 0))
            {
                return (T) typeof(T).GetConstructors().Single(c => c.GetParameters().Length == 0).Invoke(EmptyParameters);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static object ConstructFromDefaultConstructor(Type t)
        {
            var constructor = t.GetConstructors().Single(c => c.GetParameters().Length == 0);
            return constructor.Invoke(EmptyParameters);
        }

        private static TElement[] CreateArrayOf<TElement>(int length)
        {
            var returnArray = new TElement[length];
            for (int i = 0; i < returnArray.Length; i++)
            {
                returnArray[i] = RandomValueFor<TElement>();
            }

            return returnArray;
        }

        private static List<TElement> CreateListOf<TElement>(int length)
        {
            var returnList = new List<TElement>(length);
            for (int i = 0; i < returnList.Count; i++)
            {
                returnList[i] = RandomValueFor<TElement>();
            }

            return returnList;
        }

        private static T? GetNullableValue<T>() where T : struct
        {
            return RandomValueFor<T>();
        }

        private static Type[] Exceptions { get; } = typeof(Exception).Assembly.ExportedTypes
            .Where(typeof(Exception).IsAssignableFrom)
            .Where(t => !t.IsAbstract)
            .Where(t => t.GetConstructors().Length > 0)
            .Where(t => t.GetConstructors().Any(c => c.GetParameters().Length == 0))
            .ToArray();

        private static Type[] Attributes { get; } = typeof(Attribute).Assembly.ExportedTypes
            .Where(typeof(Attribute).IsAssignableFrom)
            .Where(t => !t.IsAbstract)
            .Where(t => t.GetConstructors().Length > 0)
            .Where(t => t.GetConstructors().Any(c => c.GetParameters().Length == 0))
            .ToArray();

        private static Type[] Types { get; } = typeof(Type).Assembly.GetExportedTypes();

        private static IDictionary<Type, Delegate> Generators { get; } = typeof(ValueGenerators)
            .GetProperties(BindingFlags.NonPublic | BindingFlags.Static)
            .ToDictionary(
                p => p.PropertyType,
                p => p.GetMethod.CreateDelegate(typeof(Func<>).MakeGenericType(p.PropertyType))
            );

        private static void DummyMethod()
        {
        }

        private sealed class DummyObject
        {
        }

        private static bool IsGenericOfType(this Type type, params Type[] types)
        {
            return type.IsGenericType && types.Any(gtd => type.GetGenericTypeDefinition() == gtd);
        }

        private static class ValueGenerators
        {
            private static bool Boolean => Int32 < 0;
            private static byte Byte => (byte) Random.Next(byte.MinValue, byte.MaxValue);
            private static sbyte SByte => (sbyte) Random.Next(byte.MinValue, byte.MaxValue);
            private static ushort UShort => (ushort) Random.Next(ushort.MinValue, ushort.MaxValue);
            private static short Short => (short) Random.Next(short.MinValue, short.MaxValue);
            private static char Char => (char) Random.Next(char.MinValue, char.MaxValue);
            private static uint UInt => BitConverter.ToUInt32(Random.NextBytes(4), 0);
            private static int Int32 => Random.Next(int.MinValue, int.MaxValue);
            private static ulong ULong => BitConverter.ToUInt64(Random.NextBytes(8), 0);
            private static long Long => BitConverter.ToInt64(Random.NextBytes(8), 0);
            private static float Float => BitConverter.ToSingle(Random.NextBytes(4), 0);
            private static double Double => Random.NextDouble();
            private static decimal Decimal => new decimal(Int32, Int32, Int32, Boolean, (byte) Random.Next(0, 29));
            private static DateTime DateTime => new DateTime((long)(ULong % (ulong)DateTime.MaxValue.Ticks));
            private static string String => Helpers.RandomStringOfLength(10);
        }

        internal static byte[] NextBytes(this Random random, int numberOfBytes)
        {
            var byteArray = new byte[numberOfBytes];
            random.NextBytes(byteArray);
            return byteArray;
        }
        internal static int[] NextInts(this Random random, int numberOfInts)
        {
            var intArray = new int[numberOfInts];
            for (int i = 0; i < numberOfInts; i++)
            {
                intArray[i] = random.Next(int.MinValue, int.MaxValue);
            }

            return intArray;
        }
    }
}
