using System;
using FakeItEasy;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.ContextManagement;
using MGXRM.Common.Framework.ImageManagement;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace MGXRM.Common.Tests.TestCore
{
    /// <summary>
    /// Entry point for describing the plugin pipeline position a model is being tested in.
    /// </summary>
    public static class ModelSetup
    {
        public static ModelSetup<T> For<T>() where T : Entity, new()
        {
            return new ModelSetup<T>();
        }
    }

    /// <summary>
    /// Describes a pipeline position and its images, then hands back the three things a model needs:
    /// <code>
    /// var setup = ModelSetup.For&lt;Contact&gt;()
    ///     .Update().PreOperation().Synchronous().Depth(1)
    ///     .WithPreImage(c =&gt; c.LastName = "smith")
    ///     .WithTarget(c =&gt; c.LastName = "jones");
    ///
    /// var model = new ContactModel(setup.Images, setup.Context, setup.Repository);
    /// model.MakeSurnameUppercase();
    ///
    /// Assert.Equal("JONES", setup.Images.TargetImage.LastName);
    /// </code>
    /// The context is a real <see cref="PluginContextManager{T}"/> over a faked
    /// <see cref="IPluginExecutionContext"/>, and the images are built from that context exactly the way
    /// ControllerBase builds them in production. A test written against this therefore exercises the real
    /// image aliases and the real context manager rather than a stand-in for them.
    /// </summary>
    public class ModelSetup<T> where T : Entity, new()
    {
        #region Members

        private readonly string _entityLogicalName;
        private string _message = "Update";
        private SdkMessageProcessingStep_Stage _stage = SdkMessageProcessingStep_Stage.Preoperation;
        private SdkMessageProcessingStep_Mode _mode = SdkMessageProcessingStep_Mode.Synchronous;
        private int _depth = 1;
        private Guid _primaryEntityId = Guid.NewGuid();
        private Guid _userId = Guid.NewGuid();
        private Guid _correlationId = Guid.NewGuid();
        private Guid _organizationId = Guid.NewGuid();
        private string _organizationName = "MGXRM";

        private T _preImage;
        private T _targetImage;
        private T _postImage;
        private readonly ParameterCollection _inputParameters = new ParameterCollection();
        private readonly ParameterCollection _outputParameters = new ParameterCollection();

        private IRepository _repository;
        private IOrganizationService _service;

        private IPluginExecutionContext _executionContext;
        private IContextManager<T> _builtContext;
        private IImageManager<T> _builtImages;

        #endregion

        internal ModelSetup()
        {
            _entityLogicalName = ResolveEntityLogicalName();
        }

        #region Messages

        public ModelSetup<T> Message(string message)
        {
            AssertNotBuilt();
            _message = message;
            return this;
        }

        public ModelSetup<T> Create()
        {
            return Message("Create");
        }

        public ModelSetup<T> Update()
        {
            return Message("Update");
        }

        public ModelSetup<T> Delete()
        {
            return Message("Delete");
        }

        public ModelSetup<T> Assign()
        {
            return Message("Assign");
        }

        public ModelSetup<T> SetState()
        {
            return Message("SetState");
        }

        public ModelSetup<T> SetStateDynamicEntity()
        {
            return Message("SetStateDynamicEntity");
        }

        public ModelSetup<T> Close()
        {
            return Message("Close");
        }

        public ModelSetup<T> Associate()
        {
            return Message("Associate");
        }

        public ModelSetup<T> Disassociate()
        {
            return Message("Disassociate");
        }

        #endregion

        #region Stage and mode

        public ModelSetup<T> Stage(SdkMessageProcessingStep_Stage stage)
        {
            AssertNotBuilt();
            _stage = stage;
            return this;
        }

        public ModelSetup<T> PreValidation()
        {
            return Stage(SdkMessageProcessingStep_Stage.Prevalidation);
        }

        public ModelSetup<T> PreOperation()
        {
            return Stage(SdkMessageProcessingStep_Stage.Preoperation);
        }

        public ModelSetup<T> PostOperation()
        {
            return Stage(SdkMessageProcessingStep_Stage.Postoperation);
        }

        public ModelSetup<T> Synchronous()
        {
            AssertNotBuilt();
            _mode = SdkMessageProcessingStep_Mode.Synchronous;
            return this;
        }

        public ModelSetup<T> Asynchronous()
        {
            AssertNotBuilt();
            _mode = SdkMessageProcessingStep_Mode.Asynchronous;
            return this;
        }

        public ModelSetup<T> Depth(int depth)
        {
            AssertNotBuilt();
            _depth = depth;
            return this;
        }

        #endregion

        #region Identity

        public ModelSetup<T> WithId(Guid primaryEntityId)
        {
            AssertNotBuilt();
            _primaryEntityId = primaryEntityId;
            return this;
        }

        public ModelSetup<T> WithUser(Guid userId)
        {
            AssertNotBuilt();
            _userId = userId;
            return this;
        }

        public ModelSetup<T> WithCorrelationId(Guid correlationId)
        {
            AssertNotBuilt();
            _correlationId = correlationId;
            return this;
        }

        public ModelSetup<T> WithOrganisation(Guid organizationId, string organizationName)
        {
            AssertNotBuilt();
            _organizationId = organizationId;
            _organizationName = organizationName;
            return this;
        }

        #endregion

        #region Images

        /// <summary>
        /// Adds a pre image. Only the attributes the lambda sets are present, which is what a real pre
        /// image looks like - it contains the attributes the step registration asked for, nothing else.
        /// </summary>
        public ModelSetup<T> WithPreImage(Action<T> configure = null)
        {
            AssertNotBuilt();
            _preImage = NewImage(configure);
            return this;
        }

        public ModelSetup<T> WithPreImage(T preImage)
        {
            AssertNotBuilt();
            _preImage = preImage;
            return this;
        }

        /// <summary>
        /// Adds the target. Only the attributes the lambda sets are present, matching a real update target
        /// which carries just the changed attributes.
        /// </summary>
        public ModelSetup<T> WithTarget(Action<T> configure = null)
        {
            AssertNotBuilt();
            _targetImage = NewImage(configure);
            return this;
        }

        public ModelSetup<T> WithTarget(T targetImage)
        {
            AssertNotBuilt();
            _targetImage = targetImage;
            return this;
        }

        public ModelSetup<T> WithPostImage(Action<T> configure = null)
        {
            AssertNotBuilt();
            _postImage = NewImage(configure);
            return this;
        }

        public ModelSetup<T> WithPostImage(T postImage)
        {
            AssertNotBuilt();
            _postImage = postImage;
            return this;
        }

        public ModelSetup<T> WithInputParameter(string name, object value)
        {
            AssertNotBuilt();
            _inputParameters[name] = value;
            return this;
        }

        public ModelSetup<T> WithOutputParameter(string name, object value)
        {
            AssertNotBuilt();
            _outputParameters[name] = value;
            return this;
        }

        #endregion

        #region Dependencies

        /// <summary>
        /// Replaces the faked repository, for a test that wants to arrange its own calls before the
        /// model is constructed.
        /// </summary>
        public ModelSetup<T> WithRepository(IRepository repository)
        {
            AssertNotBuilt();
            _repository = repository;
            return this;
        }

        public ModelSetup<T> WithService(IOrganizationService service)
        {
            AssertNotBuilt();
            _service = service;
            return this;
        }

        #endregion

        #region Built objects

        public IOrganizationService Service => _service ?? (_service = A.Fake<IOrganizationService>());

        public IRepository Repository => _repository ?? (_repository = A.Fake<IRepository>());

        public IContextManager<T> Context
        {
            get
            {
                Build();
                return _builtContext;
            }
        }

        /// <summary>
        /// Built from the context the same way ControllerBase does it, so the image aliases are exercised
        /// rather than bypassed.
        /// </summary>
        public IImageManager<T> Images
        {
            get
            {
                Build();
                return _builtImages;
            }
        }

        /// <summary>
        /// The faked execution context, for the rare test that needs to assert on it directly.
        /// </summary>
        public IPluginExecutionContext ExecutionContext
        {
            get
            {
                Build();
                return _executionContext;
            }
        }

        private void Build()
        {
            if (_builtContext != null)
                return;

            AssertSetupIsPossible();
            _executionContext = BuildExecutionContext();
            var contextManager = new PluginContextManager<T>(_executionContext, Service);
            _builtContext = contextManager;
            _builtImages = new ImageManager<T>(contextManager.PreImage, contextManager.TargetImage,
                contextManager.PostImage);
        }

        private IPluginExecutionContext BuildExecutionContext()
        {
            var context = A.Fake<IPluginExecutionContext>();

            A.CallTo(() => context.MessageName).Returns(_message);
            A.CallTo(() => context.Stage).Returns((int)_stage);
            A.CallTo(() => context.Mode).Returns((int)_mode);
            A.CallTo(() => context.Depth).Returns(_depth);
            A.CallTo(() => context.PrimaryEntityName).Returns(_entityLogicalName);
            A.CallTo(() => context.PrimaryEntityId).Returns(_primaryEntityId);
            A.CallTo(() => context.UserId).Returns(_userId);
            A.CallTo(() => context.InitiatingUserId).Returns(_userId);
            A.CallTo(() => context.CorrelationId).Returns(_correlationId);
            A.CallTo(() => context.OrganizationId).Returns(_organizationId);
            A.CallTo(() => context.OrganizationName).Returns(_organizationName);

            var inputParameters = new ParameterCollection();
            foreach (var parameter in _inputParameters)
                inputParameters[parameter.Key] = parameter.Value;
            if (_targetImage != null)
                inputParameters["Target"] = _targetImage;

            var preImages = new EntityImageCollection();
            if (_preImage != null)
                preImages[PluginContextManager.PreImageAlias] = _preImage;

            var postImages = new EntityImageCollection();
            if (_postImage != null)
                postImages[PluginContextManager.PostImageAlias] = _postImage;

            A.CallTo(() => context.InputParameters).Returns(inputParameters);
            A.CallTo(() => context.OutputParameters).Returns(_outputParameters);
            A.CallTo(() => context.PreEntityImages).Returns(preImages);
            A.CallTo(() => context.PostEntityImages).Returns(postImages);

            return context;
        }

        #endregion

        #region Guards

        private T NewImage(Action<T> configure)
        {
            var image = new T { Id = _primaryEntityId };
            if (string.IsNullOrWhiteSpace(image.LogicalName))
                image.LogicalName = _entityLogicalName;
            configure?.Invoke(image);
            return image;
        }

        private void AssertNotBuilt()
        {
            if (_builtContext != null)
                throw new InvalidOperationException(
                    "The setup has already been built. Finish configuring it before reading Context or Images.");
        }

        /// <summary>
        /// Rejects pipeline positions CRM would never produce, so a test cannot pass against a context
        /// that could not occur. Checked when the setup is built rather than as each value is set, so the
        /// order of the chain does not matter.
        /// </summary>
        private void AssertSetupIsPossible()
        {
            var isPreStage = _stage == SdkMessageProcessingStep_Stage.Prevalidation ||
                             _stage == SdkMessageProcessingStep_Stage.Preoperation;

            if (isPreStage && _mode == SdkMessageProcessingStep_Mode.Asynchronous)
                throw new InvalidOperationException(
                    $"A {_stage} step cannot be asynchronous - only post operation steps can be.");

            if (_preImage != null && _message == "Create")
                throw new InvalidOperationException(
                    "A Create step has no pre image - the record does not exist yet.");

            if (_postImage != null && isPreStage)
                throw new InvalidOperationException(
                    $"A {_stage} step has no post image - the operation has not happened yet.");

            if (_targetImage != null && HasEntityReferenceTarget())
                throw new InvalidOperationException(
                    $"A {_message} step has an EntityReference as its Target, not an Entity. Use WithInputParameter(\"Target\", reference) instead.");
        }

        private bool HasEntityReferenceTarget()
        {
            return _message == "Delete" || _message == "Assign" || _message == "Associate" ||
                   _message == "Disassociate";
        }

        private string ResolveEntityLogicalName()
        {
            var logicalName = new T().LogicalName;
            if (!string.IsNullOrWhiteSpace(logicalName))
                return logicalName;

            var attribute = (EntityLogicalNameAttribute)Attribute.GetCustomAttribute(typeof(T),
                typeof(EntityLogicalNameAttribute));
            if (attribute != null && !string.IsNullOrWhiteSpace(attribute.LogicalName))
                return attribute.LogicalName;

            return typeof(T).Name.ToLowerInvariant();
        }

        #endregion
    }
}
