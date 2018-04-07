namespace EasyH.Core.DependencyInjection.Container
{
    /// <summary>
    /// An enum to description the lifetime of a component.
    /// </summary>
    public enum LifeStyle
    {
        /// <summary>
        /// Represents a component is a transient component.
        /// </summary>
        PerDependency,

        /// <summary>
        /// Represents a component is a singleton component.
        /// </summary>
        SingleInstance,

        /// <summary>
        /// 
        /// </summary>
        PerLifetimeScope
    }
}
