using MGXRM.Common.Framework.CustomApi;
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
        protected IEnvironmentVariableRepository EnvironmentVariables { get; }
        protected ICustomApiRequest Request { get; }
        protected ICustomApiResponse Response { get; }

        protected ModelBase(IImageManager<T> images, IContextManager<T> context, IRepository repository)
            : this(images, context, repository, new EnvironmentVariableRepository(repository))
        {
        }

        protected ModelBase(IImageManager<T> images, IContextManager<T> context, IRepository repository,
            IEnvironmentVariableRepository environmentVariables)
        {
            Images = images;
            Context = context;
            Repository = repository;
            EnvironmentVariables = environmentVariables;
            Request = new CustomApiRequest(context.InputParams);
            Response = new CustomApiResponse(context.OutputParams);
        }
    }
}
