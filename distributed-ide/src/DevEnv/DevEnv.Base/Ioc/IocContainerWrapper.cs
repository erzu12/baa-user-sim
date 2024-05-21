using DevEnv.Base.RuntimeChecks;

namespace DevEnv.Base.Ioc
{
    /// <summary>
    /// Implements the <see cref="IIocRegistry"/>, and wraps the actual IOC container instance via a registration delegate.
    /// </summary>
    public class IocContainerWrapper : IIocRegistry
    {
        private readonly Action<Type, Type> registerSingleton;
        private readonly Action<Type, object> registerSingletonInstance;

        public IocContainerWrapper(
            Action<Type, Type> registerSingleton,
            Action<Type, object> registerSingletonInstance)
        {
            Argument.AssertNotNull(registerSingleton, nameof(registerSingleton));
            Argument.AssertNotNull(registerSingletonInstance, nameof(registerSingletonInstance));

            this.registerSingleton = registerSingleton;
            this.registerSingletonInstance = registerSingletonInstance;
        }

        public IIocRegistry RegisterSingleton<TAbstraction, TImplementation>()
            where TAbstraction : class
            where TImplementation : class, TAbstraction
        {
            this.registerSingleton(typeof(TAbstraction), typeof(TImplementation));

            return this;
        }

        public IIocRegistry RegisterSingletonInstance<TAbstraction>(TAbstraction instance)
            where TAbstraction : class
        {
            Argument.AssertNotNull(instance, nameof(instance));

            this.registerSingletonInstance(typeof(TAbstraction), instance);

            return this;
        }
    }
}
