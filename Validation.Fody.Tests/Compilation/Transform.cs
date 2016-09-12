namespace Validation.Fody.Tests.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Mono.Cecil;

    internal static class Transform
    {
        public static TransformResult FromAttributeSource(MemberType memberType, Type memberReflectedType, string attributeSource)
        {
            var typeName = GetTypeName(memberReflectedType);

            switch (memberType)
            {
                case MemberType.InstanceProperty:
                {
                    var source = "public class A { " + attributeSource + " public " + typeName + " B { get; set; } }";
                    return FromSource(source);
                }

                case MemberType.StaticProperty:
                {
                    var source = "public static class A { " + attributeSource + " public static " + typeName + " B { get; set; } }";
                    return FromSource(source);
                }

                case MemberType.InstanceMethod:
                {
                    var source = "public class A { public void B(" + attributeSource + " " + typeName + " param) {} }";
                    return FromSource(source);
                }

                case MemberType.StaticMethod:
                {
                    var source = "public class A { public static void B(" + attributeSource + " " + typeName + " param) {} }";
                    return FromSource(source);
                }

                case MemberType.InstanceConstructor:
                {
                    var source = "public class A { public A(" + attributeSource + " " + typeName + " param) {} }";
                    return FromSource(source);
                }

                default:
                    throw new InvalidOperationException("Unknown member type");
            }
        }

        public static TransformResult<T> FromAttributeSource<T>(MemberType memberType, string attributeSource)
        {
            var typeName = GetTypeName(typeof(T));

            switch (memberType)
            {
                case MemberType.InstanceProperty:
                {
                    var source = "public class A { " + attributeSource + " public " + typeName + " B { get; set; } }";
                    var result = FromSource(source);
                    return !result.Success
                        ? new TransformResult<T>(result.BuildMessages)
                        : new TransformResult<T>(Method.FromProperty<T>(result.Assembly, "A", "B"), result.BuildMessages);
                }

                case MemberType.StaticProperty:
                {
                    var source = "public static class A { " + attributeSource + " public static " + typeName + " B { get; set; } }";
                    var result = FromSource(source);
                    return !result.Success
                        ? new TransformResult<T>(result.BuildMessages)
                        : new TransformResult<T>(Method.FromProperty<T>(result.Assembly, "A", "B"), result.BuildMessages);
                }

                case MemberType.InstanceMethod:
                {
                    var source = "public class A { public void B(" + attributeSource + " " + typeName + " param) {} }";
                    var result = FromSource(source);
                    return !result.Success
                        ? new TransformResult<T>(result.BuildMessages)
                        : new TransformResult<T>(Method.FromMethod<T>(result.Assembly, "A", "B"), result.BuildMessages);
                }

                case MemberType.StaticMethod:
                {
                    var source = "public class A { public static void B(" + attributeSource + " " + typeName + " param) {} }";
                    var result = FromSource(source);
                    return !result.Success
                        ? new TransformResult<T>(result.BuildMessages)
                        : new TransformResult<T>(Method.FromMethod<T>(result.Assembly, "A", "B"), result.BuildMessages);
                }

                case MemberType.InstanceConstructor:
                {
                    var source = "public class A { public A(" + attributeSource + " " + typeName + " param) {} }";
                    var result = FromSource(source);
                    return !result.Success
                        ? new TransformResult<T>(result.BuildMessages)
                        : new TransformResult<T>(Method.FromConstructor<T>(result.Assembly, "A"), result.BuildMessages);
                }

                default:
                    throw new InvalidOperationException("Unknown member type");
            }
        }

        public static TransformResult FromSource(string source)
        {
            var compilation = Compilation.CompileSource(source);
            var assemblyStream = Compilation.GetAssemblyAsStream(compilation);
            var assembly = Compilation.GetAssemblyDefinitionFromStream(assemblyStream);

            var messageList = PerformWeaving(assembly.Modules.Single());

            if (messageList.Any(m => m.Level == MessageLevel.Error))
            {
                return new TransformResult(messageList);
            }

            var loadedAssembly = Compilation.GetLoadedAssemblyFromDefinition(assembly);
            return new TransformResult(loadedAssembly, messageList);
        }

        private static ImmutableList<BuildMessage> PerformWeaving(ModuleDefinition moduleDefinition)
        {
            var messageList = new List<BuildMessage>();

            new ModuleWeaver
            {
                LogError = m => messageList.Add(new BuildMessage(MessageLevel.Error, m)),
                LogInfo = m => messageList.Add(new BuildMessage(MessageLevel.Info, m)),
                ModuleDefinition = moduleDefinition
            }.Execute();

            return messageList.ToImmutableList();
        }
        
        public static string GetTypeName(Type t)
        {
            if (t.IsGenericTypeDefinition || (t.IsGenericType && !t.IsConstructedGenericType))
            {
                throw new InvalidOperationException("Cannot use open generic");
            }
            else if (t.IsArray)
            {
                var elementType = t.GetElementType();
                return GetTypeName(elementType) + "[]";
            }
            else if (t.IsGenericType && t.IsValueType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var elementType = t.GetGenericArguments().Single();
                return GetTypeName(elementType) + "?";
            }
            else if (t.IsGenericType)
            {
                var ns = t.Namespace != null ? t.Namespace + "." : string.Empty;
                return ns + t.Name.Substring(0, t.Name.Length - 2) + "<" + string.Join(", ", t.GetGenericArguments().Select(GetTypeName)) + ">";
            }
            else
            {
                var ns = t.Namespace != null ? t.Namespace + "." : string.Empty;
                return ns + t.Name;
            }
        }

        public static Action<object> GetAction(this TransformResult result, MemberType memberType, Type type)
        {
            if (memberType == MemberType.InstanceProperty || memberType == MemberType.StaticProperty)
            {
                return Method.FromProperty(result.Assembly, "A", "B");
            }
            else if (memberType == MemberType.InstanceMethod || memberType == MemberType.StaticMethod)
            {
                return Method.FromMethod(result.Assembly, "A", "B");
            }
            else if (memberType == MemberType.InstanceConstructor)
            {
                return (Action<object>) Method.FromConstructor(result.Assembly, "A", new [] { type });
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
