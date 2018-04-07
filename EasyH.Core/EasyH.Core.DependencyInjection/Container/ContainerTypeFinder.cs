using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyH.Core.DependencyInjection.Container
{
    public class ContainerTypeFinder : ITypeFinder
    {

        public IList<Assembly> GetAssemblies()
        {
            //由于注册文件可能分布在不同类库，为此获取当前应用程序域中所有程序集，而不是当前程序集
            var assemblies= AppDomain.CurrentDomain.GetAssemblies();
            return assemblies;
        }

        public IList<Assembly> Assemblies
        {
            get { return GetAssemblies(); }
        }

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom)
        {
            try
            {
                var list = new List<Type>();
                foreach (var typesToRegister in GetAssemblies().Select(item => item.GetTypes()
                .Where(p => assignTypeFrom.IsAssignableFrom(p) && p != assignTypeFrom)
                .Where(type => !String.IsNullOrEmpty(type.Namespace))
                .Where(type => type.IsPublic)
                .Where(type => !type.IsAbstract)
                )
                .Where(typesToRegister => typesToRegister.Any()))
                {
                    list.AddRange(typesToRegister);
                }
                return list;
            }
            catch (ReflectionTypeLoadException exception)
            {
                if (exception.LoaderExceptions != null)
                {
                    throw new Exception(exception.LoaderExceptions[0].Message);
                }
                throw;
            }
         
        }
    }
}
