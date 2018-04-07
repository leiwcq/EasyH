using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyH.Core.DependencyInjection.Container
{
    public interface ITypeFinder
    {
        IList<Assembly> GetAssemblies();
        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom);
    }
}
