namespace Validation.Fody.Helpers
{
    using System;
    using System.Linq;
    using Mono.Cecil;

    internal static class NullableHelpers
    {
        public static MethodReference NullableHasValue<T>(this ModuleDefinition module)
            where T : struct
        {
            return NullableHasValue(module, typeof (T));
        }

        public static MethodReference NullableHasValue(this ModuleDefinition module, Type t)
        {
            return NullableHasValue(module, module.Import(t));
        }

        public static MethodReference NullableHasValue(this ModuleDefinition module, TypeReference typeReference)
        {
            if (!typeReference.IsValueType)
            {
                throw new ArgumentException("Type must be a struct to get nullable HasValue proeprty getter method");
            }

            // Fashioned in part from http://paste2.org/YeD09NEI, published by Jb Evain
            
            var nullableDef = module.GetCorLib().GetType("System.Nullable`1");
            var hasValueDef = nullableDef.Methods.Single(m => m.Name == "get_HasValue");
            var hasValue = module.Import(hasValueDef);

            return hasValue.ResolveOnGenericDeclaringType(typeReference);
        }

        public static TypeReference GetNullableInnerType(this TypeReference reference)
        {
            if (reference.Module == reference.GetCorLib() && reference.Namespace == "System" && reference.Name != "Nullable`1")
            {
                throw new ArgumentException("Must provide a Nullable<T> reference");
            }

            var genericType = reference as GenericInstanceType;
            if (genericType == null)
            {
                throw new ArgumentException("Type provided is not a closed Nullable<T>, perhaps you passed an open Nullable<>?");
            }

            return genericType.GenericArguments.Single();
        }
    }
}
