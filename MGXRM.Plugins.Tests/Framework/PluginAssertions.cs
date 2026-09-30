using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MGXRM.Common.Framework.ContextManagement;
using Xunit;

namespace MGXRM.Plugins.Tests.Framework
{
    /// <summary>
    /// Fluent assertions over a single plugin class. Every method returns the same instance so a whole
    /// registration can be verified in one chain:
    /// <code>
    /// PluginUnderTest
    ///     .IsRegisteredFor(MessageNameEnum.Update, Contact.EntityLogicalName)
    ///     .IsPreOperation()
    ///     .IsSynchronous()
    ///     .IsSandbox()
    ///     .HasOrder(10)
    ///     .HasSingleFilteringAttribute(Contact.Fields.LastName)
    ///     .MatchesRegisteredEvents()
    ///     .InvokesControllerOperation&lt;ContactController&gt;(nameof(ContactController.PreUpdate));
    /// </code>
    /// </summary>
    public class PluginAssertions<TPlugin> where TPlugin : Plugin
    {
        private readonly Type _pluginType;
        private readonly IList<CrmPluginRegistrationAttribute> _registrations;
        private readonly IList<RegisteredEvent> _events;
        private CrmPluginRegistrationAttribute _selected;

        public PluginAssertions() : this(typeof(TPlugin))
        {
        }

        /// <summary>
        /// Used by the assembly wide convention tests, which only know the plugin type at run time.
        /// </summary>
        internal PluginAssertions(Type pluginType)
        {
            _pluginType = pluginType;
            _registrations = PluginReflection.GetRegistrations(_pluginType);
            _events = PluginReflection.GetRegisteredEvents(_pluginType);
        }

        private string Name => _pluginType.Name;

        /// <summary>
        /// The registration currently under assertion. Where a plugin declares more than one step,
        /// pick one first with <see cref="ForStep"/> or <see cref="ForMessage(string)"/>.
        /// </summary>
        private CrmPluginRegistrationAttribute Current
        {
            get
            {
                if (_selected != null)
                    return _selected;

                Assert.True(_registrations.Count > 0,
                    $"{Name} has no CrmPluginRegistrationAttribute.");
                Assert.True(_registrations.Count == 1,
                    $"{Name} has {_registrations.Count} registrations - select one with ForStep(...) or ForMessage(...) before asserting on step values.");
                return _registrations[0];
            }
        }

        #region Selecting a registration

        public PluginAssertions<TPlugin> HasRegistrationCount(int expected)
        {
            Assert.True(_registrations.Count == expected,
                $"{Name} was expected to declare {expected} registration(s) but declares {_registrations.Count}.");
            return this;
        }

        public PluginAssertions<TPlugin> ForStep(string stepName)
        {
            var matches = _registrations.Where(r => r.Name == stepName).ToList();
            Assert.True(matches.Count == 1,
                $"{Name} was expected to declare exactly one registration named '{stepName}' but declares {matches.Count}. Steps: {StepNames()}.");
            _selected = matches[0];
            return this;
        }

        public PluginAssertions<TPlugin> ForMessage(MessageNameEnum message)
        {
            return ForMessage(message.ToString());
        }

        public PluginAssertions<TPlugin> ForMessage(string message)
        {
            var matches = _registrations.Where(r => r.Message == message).ToList();
            Assert.True(matches.Count == 1,
                $"{Name} was expected to declare exactly one registration for message '{message}' but declares {matches.Count}. Steps: {StepNames()}.");
            _selected = matches[0];
            return this;
        }

        #endregion

        #region Step values

        public PluginAssertions<TPlugin> IsRegisteredFor(MessageNameEnum message, string entityLogicalName)
        {
            return IsRegisteredFor(message.ToString(), entityLogicalName);
        }

        public PluginAssertions<TPlugin> IsRegisteredFor(string message, string entityLogicalName)
        {
            Assert.True(Current.Message == message,
                $"{Name} is registered for message '{Current.Message}' but '{message}' was expected.");
            Assert.True(Current.EntityLogicalName == entityLogicalName,
                $"{Name} is registered against entity '{Current.EntityLogicalName}' but '{entityLogicalName}' was expected.");
            return this;
        }

        public PluginAssertions<TPlugin> IsCustomApi(string uniqueName)
        {
            Assert.True(IsCustomApiRegistration(Current),
                $"{Name} is registered at stage {DescribeStage(Current.Stage)}. A custom api registration names only the api, leaving the stage unset.");
            Assert.True(Current.Message == uniqueName,
                $"{Name} is registered for custom api '{Current.Message}' but '{uniqueName}' was expected.");

            var mainOperation = _events
                .Where(e => e.Message == uniqueName && e.Stage == Plugins.CustomApi.MainOperationStage)
                .ToList();
            Assert.True(mainOperation.Count == 1,
                $"{Name} was expected to register one main operation handler for '{uniqueName}' at stage {Plugins.CustomApi.MainOperationStage} but registers {mainOperation.Count}. Registered events: {DescribeEvents()}.");

            return this;
        }

        public PluginAssertions<TPlugin> IsBoundTo(string entityLogicalName)
        {
            var bound = _events
                .Where(e => e.Message == Current.Message && e.EntityLogicalName == entityLogicalName)
                .ToList();
            Assert.True(bound.Count == 1,
                $"{Name} was expected to be bound to '{entityLogicalName}'. Registered events: {DescribeEvents()}.");
            return this;
        }

        public PluginAssertions<TPlugin> IsUnbound()
        {
            var unbound = _events
                .Where(e => e.Message == Current.Message && string.IsNullOrWhiteSpace(e.EntityLogicalName))
                .ToList();
            Assert.True(unbound.Count == 1,
                $"{Name} was expected to be unbound. Registered events: {DescribeEvents()}.");
            return this;
        }

        public PluginAssertions<TPlugin> HasStepName(string stepName)
        {
            Assert.True(Current.Name == stepName,
                $"{Name} has step name '{Current.Name}' but '{stepName}' was expected.");
            return this;
        }

        public PluginAssertions<TPlugin> IsSandbox()
        {
            Assert.True(Current.IsolationMode == IsolationModeEnum.Sandbox,
                $"{Name} is registered with isolation mode {Current.IsolationMode} but Sandbox was expected.");
            return this;
        }

        public PluginAssertions<TPlugin> IsNotSandbox()
        {
            Assert.True(Current.IsolationMode == IsolationModeEnum.None,
                $"{Name} is registered with isolation mode {Current.IsolationMode} but None was expected.");
            return this;
        }

        public PluginAssertions<TPlugin> IsSynchronous()
        {
            Assert.True(Current.ExecutionMode == ExecutionModeEnum.Synchronous,
                $"{Name} is registered as {Current.ExecutionMode} but Synchronous was expected.");
            return this;
        }

        public PluginAssertions<TPlugin> IsAsynchronous()
        {
            Assert.True(Current.ExecutionMode == ExecutionModeEnum.Asynchronous,
                $"{Name} is registered as {Current.ExecutionMode} but Asynchronous was expected.");
            return this;
        }

        public PluginAssertions<TPlugin> HasStage(StageEnum stage)
        {
            Assert.True(Current.Stage == stage,
                $"{Name} is registered at stage {DescribeStage(Current.Stage)} but {stage} was expected.");
            return this;
        }

        public PluginAssertions<TPlugin> IsPreValidation()
        {
            return HasStage(StageEnum.PreValidation);
        }

        public PluginAssertions<TPlugin> IsPreOperation()
        {
            return HasStage(StageEnum.PreOperation);
        }

        public PluginAssertions<TPlugin> IsPostOperation()
        {
            return HasStage(StageEnum.PostOperation);
        }

        public PluginAssertions<TPlugin> HasOrder(int executionOrder)
        {
            Assert.True(Current.ExecutionOrder == executionOrder,
                $"{Name} has execution order {Current.ExecutionOrder} but {executionOrder} was expected.");
            return this;
        }

        #endregion

        #region Filtering attributes

        public PluginAssertions<TPlugin> HasNoFilteringAttributes()
        {
            var actual = PluginReflection.GetFilteringAttributes(Current);
            Assert.True(actual.Length == 0,
                $"{Name} was expected to have no filtering attributes but has '{Current.FilteringAttributes}'.");
            return this;
        }

        public PluginAssertions<TPlugin> HasSingleFilteringAttribute()
        {
            var actual = PluginReflection.GetFilteringAttributes(Current);
            Assert.True(actual.Length == 1,
                $"{Name} was expected to have a single filtering attribute but has {actual.Length}: '{Current.FilteringAttributes}'.");
            return this;
        }

        public PluginAssertions<TPlugin> HasSingleFilteringAttribute(string filteringAttribute)
        {
            HasSingleFilteringAttribute();
            return HasFilteringAttributes(filteringAttribute);
        }

        /// <summary>
        /// Asserts the step filters on exactly the given attributes, in any order.
        /// </summary>
        public PluginAssertions<TPlugin> HasFilteringAttributes(params string[] filteringAttributes)
        {
            var actual = PluginReflection.GetFilteringAttributes(Current);
            var missing = filteringAttributes.Except(actual).ToList();
            var unexpected = actual.Except(filteringAttributes).ToList();

            Assert.True(!missing.Any() && !unexpected.Any(),
                $"{Name} filtering attributes were expected to be '{string.Join(",", filteringAttributes.OrderBy(a => a))}' but were '{string.Join(",", actual.OrderBy(a => a))}'.");
            return this;
        }

        #endregion

        #region Images

        /// <summary>
        /// Asserts a pre image is registered, and that it is registered under the alias
        /// <see cref="PluginContextManager{T}"/> reads.
        /// </summary>
        public PluginAssertions<TPlugin> HasPreImage()
        {
            return HasImage(ImageTypeEnum.PreImage);
        }

        public PluginAssertions<TPlugin> HasPreImage(params string[] imageAttributes)
        {
            return HasImage(ImageTypeEnum.PreImage, imageAttributes);
        }

        public PluginAssertions<TPlugin> HasPostImage()
        {
            return HasImage(ImageTypeEnum.PostImage);
        }

        public PluginAssertions<TPlugin> HasPostImage(params string[] imageAttributes)
        {
            return HasImage(ImageTypeEnum.PostImage, imageAttributes);
        }

        public PluginAssertions<TPlugin> HasNoImages()
        {
            var images = PluginReflection.GetImages(Current);
            Assert.True(images.Count == 0,
                $"{Name} was expected to register no images but registers {string.Join(", ", images.Select(i => $"'{i.Name}' ({i.Type})"))}.");
            return this;
        }

        private PluginAssertions<TPlugin> HasImage(ImageTypeEnum imageType, string[] imageAttributes = null)
        {
            var images = PluginReflection.GetImages(Current).Where(i => i.Type == imageType).ToList();
            Assert.True(images.Count == 1,
                $"{Name} was expected to register a single {imageType} but registers {images.Count}.");

            var image = images[0];
            Assert.True(image.Name == image.ExpectedName,
                $"{Name} registers its {imageType} as '{image.Name}'. PluginContextManager only reads '{image.ExpectedName}', so this image would never reach the controller.");

            if (imageAttributes != null)
            {
                var missing = imageAttributes.Except(image.Attributes).ToList();
                var unexpected = image.Attributes.Except(imageAttributes).ToList();
                Assert.True(!missing.Any() && !unexpected.Any(),
                    $"{Name} {imageType} attributes were expected to be '{string.Join(",", imageAttributes.OrderBy(a => a))}' but were '{string.Join(",", image.Attributes.OrderBy(a => a))}'.");
            }

            return this;
        }

        /// <summary>
        /// Asserts every image on every registration of this plugin is named the way
        /// <see cref="PluginContextManager{T}"/> expects.
        /// </summary>
        public PluginAssertions<TPlugin> ImagesUseContextManagerNames()
        {
            foreach (var registration in _registrations)
            {
                foreach (var image in PluginReflection.GetImages(registration))
                {
                    Assert.True(image.ExpectedName != null,
                        $"{Name} step '{registration.Name}' registers image '{image.Name}' as {image.Type}. PluginContextManager reads a pre image and a post image under separate aliases, so register two images rather than one of type Both.");
                    Assert.True(image.Name == image.ExpectedName,
                        $"{Name} step '{registration.Name}' registers its {image.Type} as '{image.Name}' but PluginContextManager only reads '{image.ExpectedName}'.");
                }
            }
            return this;
        }

        #endregion

        #region Ids

        public PluginAssertions<TPlugin> HasId()
        {
            foreach (var registration in _registrations)
            {
                Guid parsed;
                Assert.True(!string.IsNullOrWhiteSpace(registration.Id),
                    $"{Name} step '{registration.Name}' has no Id. Without one spkl cannot update the step in place and will create a duplicate.");
                Assert.True(Guid.TryParse(registration.Id, out parsed),
                    $"{Name} step '{registration.Name}' has Id '{registration.Id}', which is not a guid.");
            }
            return this;
        }

        /// <summary>
        /// Asserts this plugin has an Id on every step, and that no other plugin in the assembly reuses it.
        /// </summary>
        public PluginAssertions<TPlugin> HasUniqueId()
        {
            HasId();
            PluginConventions.AssertRegistrationIdsAreUnique();
            return this;
        }

        #endregion

        #region Attribute / RegisteredEvents consistency

        /// <summary>
        /// Asserts the CrmPluginRegistrationAttribute values line up with the
        /// <c>base.RegisteredEvents.Add(...)</c> entries in the constructor. A mismatch means the step is
        /// deployed but the plugin body never runs.
        /// </summary>
        public PluginAssertions<TPlugin> MatchesRegisteredEvents()
        {
            foreach (var registration in _registrations)
            {
                var matches = _events.Where(e => Matches(e, registration)).ToList();
                Assert.True(matches.Count == 1,
                    $"{Name} step '{registration.Name}' ({registration.Message} on '{registration.EntityLogicalName}' at stage {DescribeStage(registration.Stage)}) matches {matches.Count} entries in RegisteredEvents. Registered events: {DescribeEvents()}.");
            }

            foreach (var registeredEvent in _events)
            {
                var matches = _registrations.Where(r => Matches(registeredEvent, r)).ToList();
                Assert.True(matches.Count == 1,
                    $"{Name} registers the event [{registeredEvent}] but {matches.Count} CrmPluginRegistrationAttribute(s) match it, so it would never be deployed as a step.");
            }

            return this;
        }

        private static bool Matches(RegisteredEvent registeredEvent, CrmPluginRegistrationAttribute registration)
        {
            var expectedStage = registration.Stage.HasValue
                ? (int)registration.Stage.Value
                : Plugins.CustomApi.MainOperationStage;

            return registeredEvent.Stage == expectedStage
                   && registeredEvent.Message == registration.Message
                   && (string.IsNullOrWhiteSpace(registeredEvent.EntityLogicalName)
                       || registeredEvent.EntityLogicalName == registration.EntityLogicalName);
        }

        private static bool IsCustomApiRegistration(CrmPluginRegistrationAttribute registration)
        {
            return !registration.Stage.HasValue;
        }

        /// <summary>
        /// The consistency checks that should hold for every plugin: an Id that is unique across the
        /// assembly, images named the way the framework reads them, and attributes that agree with the
        /// registered events.
        /// </summary>
        public PluginAssertions<TPlugin> IsConsistent()
        {
            return HasUniqueId()
                .ImagesUseContextManagerNames()
                .MatchesRegisteredEvents();
        }

        #endregion

        #region Execution

        /// <summary>
        /// Asserts the execute method for the current step constructs <typeparamref name="TController"/>
        /// and calls <paramref name="operationName"/> on it. This reads the IL of the execute method, so
        /// it needs no fake pipeline - use it for the usual "plugin delegates straight to a controller"
        /// shape, and write a normal test with fakes when the method does real work.
        /// </summary>
        public PluginAssertions<TPlugin> InvokesControllerOperation<TController>(string operationName)
        {
            var calls = CallsForCurrentStep();

            Assert.True(calls.Any(c => c.IsConstructor && typeof(TController).IsAssignableFrom(c.DeclaringType)),
                $"{Name} step '{Current.Name}' never constructs a {typeof(TController).Name}. It calls: {Describe(calls)}.");

            Assert.True(calls.Any(c => IsOperation<TController>(c, operationName)),
                $"{Name} step '{Current.Name}' does not call {typeof(TController).Name}.{operationName}. It calls: {Describe(calls)}.");

            return this;
        }

        public PluginAssertions<TPlugin> DoesNotInvokeControllerOperation<TController>(string operationName)
        {
            var calls = CallsForCurrentStep();
            Assert.True(!calls.Any(c => IsOperation<TController>(c, operationName)),
                $"{Name} step '{Current.Name}' calls {typeof(TController).Name}.{operationName} but was expected not to.");
            return this;
        }

        private static bool IsOperation<TController>(MethodBase method, string operationName)
        {
            return method.Name == operationName
                   && method.DeclaringType != null
                   && method.DeclaringType.IsAssignableFrom(typeof(TController));
        }

        private IList<MethodBase> CallsForCurrentStep()
        {
            return PluginReflection.GetCalledMethods(HandlerForCurrentStep(), _pluginType);
        }

        private MethodInfo HandlerForCurrentStep()
        {
            var matches = _events.Where(e => Matches(e, Current)).ToList();
            Assert.True(matches.Count == 1,
                $"{Name} step '{Current.Name}' matches {matches.Count} entries in RegisteredEvents, so its execute method is ambiguous. Registered events: {DescribeEvents()}.");
            return matches[0].Handler;
        }

        private static string Describe(IEnumerable<MethodBase> calls)
        {
            var described = calls
                .Where(c => c.DeclaringType != null)
                .Select(c => $"{c.DeclaringType.Name}.{c.Name}")
                .Distinct()
                .ToList();
            return described.Any() ? string.Join(", ", described) : "nothing";
        }

        #endregion

        private string StepNames()
        {
            return string.Join(", ", _registrations.Select(r => $"'{r.Name}'"));
        }

        private string DescribeEvents()
        {
            return _events.Any() ? string.Join(" | ", _events.Select(e => $"[{e}]")) : "none";
        }

        private static string DescribeStage(StageEnum? stage)
        {
            return stage.HasValue ? stage.Value.ToString() : "(none)";
        }
    }
}
