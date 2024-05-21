
namespace DevEnv.Base.Ioc
{
    /// <summary>
    /// Provides a simplified and restriced IOC container registration interface.
    /// </summary>
    public interface IIocRegistry
    {
        /// <summary>
        /// Registers a specified singleton implementation type for a specified abstraction type.
        /// </summary>
        /// <typeparam name="TAbstraction">
        /// The interface/abstraction type
        /// </typeparam>
        /// <typeparam name="TImplementation">
        /// The singleton implementation type
        /// </typeparam>
        /// <returns>
        /// This instance, for multiple fluent calls
        /// </returns>
        IIocRegistry RegisterSingleton<TAbstraction, TImplementation>()
            where TAbstraction : class
            where TImplementation : class, TAbstraction;

        /// <summary>
        /// Registers a specified instance as the singleton implementation for a specified abstraction type.
        /// </summary>
        /// <typeparam name="TAbstraction">
        /// The interface/abstraction type
        /// </typeparam>
        /// <param name="instance">
        /// The singleton instance
        /// </param>
        /// <returns>
        /// This instance, for multiple fluent calls
        /// </returns>
        IIocRegistry RegisterSingletonInstance<TAbstraction>(TAbstraction instance)
            where TAbstraction : class;
    }
}
