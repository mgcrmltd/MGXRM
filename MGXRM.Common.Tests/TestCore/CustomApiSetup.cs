using System;
using FakeXrmEasy;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.CustomApi;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Tests.TestCore
{
    public static class CustomApiSetup
    {
        public static CustomApiSetup<T> For<T>(string uniqueName) where T : Entity, new()
        {
            return new CustomApiSetup<T>(uniqueName);
        }
    }

    public class CustomApiSetup<T> where T : Entity, new()
    {
        private readonly ModelSetup<T> _setup;
        private ICustomApiRequest _request;
        private ICustomApiResponse _response;

        internal CustomApiSetup(string uniqueName)
        {
            if (string.IsNullOrWhiteSpace(uniqueName))
                throw new ArgumentException("A custom api unique name is required.", nameof(uniqueName));

            _setup = ModelSetup.For<T>()
                .Message(uniqueName)
                .Stage(SdkMessageProcessingStep_Stage.MainOperation_Forinternaluseonly)
                .Synchronous();
        }

        public CustomApiSetup<T> WithTarget(EntityReference target)
        {
            return WithParameter(CustomApiRequest.TargetParameter, target);
        }

        public CustomApiSetup<T> WithParameter(string name, object value)
        {
            _setup.WithInputParameter(name, value);
            return this;
        }

        public CustomApiSetup<T> Depth(int depth)
        {
            _setup.Depth(depth);
            return this;
        }

        public CustomApiSetup<T> WithUser(Guid userId)
        {
            _setup.WithUser(userId);
            return this;
        }

        public CustomApiSetup<T> WithFakeCrm(params Entity[] existingRecords)
        {
            _setup.WithFakeCrm(existingRecords);
            return this;
        }

        public CustomApiSetup<T> WithRepository(IRepository repository)
        {
            _setup.WithRepository(repository);
            return this;
        }

        public IContextManager<T> Context => _setup.Context;
        public IImageManager<T> Images => _setup.Images;
        public IRepository Repository => _setup.Repository;
        public IOrganizationService Service => _setup.Service;
        public XrmFakedContext FakeCrm => _setup.FakeCrm;
        public IPluginExecutionContext ExecutionContext => _setup.ExecutionContext;

        public ParameterCollection OutputParameters => Context.OutputParams;

        public ICustomApiRequest Request => _request ?? (_request = new CustomApiRequest(Context.InputParams));

        public ICustomApiResponse Response => _response ?? (_response = new CustomApiResponse(Context.OutputParams));
    }
}
