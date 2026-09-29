using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Repositories;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.Model
{
    public abstract class ModelBase<T> where T : Entity
    {
        protected IImageManager<T> Images { get; }
        protected IContextManager<T> Context { get; }
        protected IRepository Repository { get; }

        /// <summary>
        /// Environment variables, available to every model because most of them end up needing a
        /// configured value at some point.
        /// </summary>
        protected IEnvironmentVariableRepository EnvironmentVariables { get; }

        protected ModelBase(IImageManager<T> images, IContextManager<T> context, IRepository repository)
            : this(images, context, repository, new EnvironmentVariableRepository(repository))
        {
        }

        /// <summary>
        /// Takes the environment variable repository instead of building one, for a test that wants to
        /// state what a variable holds rather than seed the records behind it.
        /// </summary>
        protected ModelBase(IImageManager<T> images, IContextManager<T> context, IRepository repository,
            IEnvironmentVariableRepository environmentVariables)
        {
            Images = images;
            Context = context;
            Repository = repository;
            EnvironmentVariables = environmentVariables;
        }
    }
}
