using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace System
{
    internal class Bootstrapper
    {
        private IContainer _container;

        [ImportMany(typeof(IModule))] private IEnumerable<IModule> Modules { get; set; }

        public void BuildComposition()
        {
            GetMefContainer().ComposeParts(this);

            var builder = new ContainerBuilder();
            foreach (var module in Modules) builder.RegisterModule(module);
            _container = builder.Build();
        }

        private CompositionContainer GetMefContainer()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(GetAssemblyCatalog());
            catalog.Catalogs.Add(GetDirectoryCatalog());
            return new CompositionContainer(catalog);
        }

        private static DirectoryCatalog GetDirectoryCatalog()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location);
            var directoryCatalog = new DirectoryCatalog(path ?? throw new InvalidOperationException());
            return directoryCatalog;
        }

        private static AssemblyCatalog GetAssemblyCatalog()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null) throw new InvalidOperationException("GetEntryAssembly returned null.");
            var assemblyCatalog = new AssemblyCatalog(entryAssembly);
            return assemblyCatalog;
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }
    }
}