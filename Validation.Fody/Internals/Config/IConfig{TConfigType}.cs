namespace Validation.Fody.Internals.Config
{
    internal interface IConfig<TConfigType> : IConfig
        where TConfigType : class, new()
    {
        TConfigType Configuration { get; }
    }
}
