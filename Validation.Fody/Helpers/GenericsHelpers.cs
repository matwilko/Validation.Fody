namespace Validation.Fody.Helpers
{
    using Mono.Cecil;
    using Mono.Cecil.Rocks;

    internal static class GenericsHelpers
    {
        public static MethodReference ResolveOnGenericDeclaringType(this MethodReference self, params TypeReference[] arguments)
        {
            var reference = new MethodReference(self.Name, self.ReturnType)
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                DeclaringType = self.DeclaringType.MakeGenericInstanceType(arguments),
                CallingConvention = self.CallingConvention,
            };

            foreach (var parameter in self.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (var genericParameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(genericParameter.Name, reference));

            return reference;
        }
    }
}
