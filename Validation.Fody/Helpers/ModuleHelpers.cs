namespace Validation.Fody.Helpers
{
    using Mono.Cecil;

    internal static class ModuleHelpers
    {
        public static ModuleDefinition GetCorLib(this ModuleDefinition module)
        {
            var corlibName = (AssemblyNameReference) module.TypeSystem.Corlib;
            return module.AssemblyResolver.Resolve(corlibName).MainModule;
        }

        public static ModuleDefinition GetCorLib(this TypeReference reference) => reference.Module.GetCorLib();
    }
}
