using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MGXRM.Common.Framework.ContextManagement;
using Xunit;

namespace MGXRM.Plugins.Tests.Framework
{
    public class PluginAssertions<TPlugin> where TPlugin : Plugin
    {
        private readonly Type _pluginType;
        private readonly IList<CrmPluginRegistrationAttribute> _registrations;
        private readonly IList<RegisteredEvent> _events;
        private CrmPluginRegistrationAttribute _selected;

        public PluginAssertions() : this(typeof(TPlugin))
        {
        }

        internal PluginAssertions(Type pluginType)
        {
            _pluginType = pluginType;
            _registrations = PluginReflection.GetRegistrations(_pluginType);
            _events = PluginReflection.GetRegisteredEvents(_pluginType);
        }

        private string Name => _pluginType.Name;

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

        public PluginAssertions<TPlugin> HasUniqueId()
        {
            HasId();
            PluginConventions.AssertRegistrationIdsAreUnique();
            return this;
        }

        #endregion

        #region Attribute / RegisteredEvents consistency

        public PluginAssertions<TPlugin> MatchesRegisteredEvents()
        {
            foreach (var registration in _registrations)
            {
                Assert.True(registration.Stage.HasValue,
                    $"{Name} step '{registration.Name}' has no stage, so it cannot be matched to a registered event.");

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
            if (!registration.Stage.HasValue)
                return false;

            return registeredEvent.Stage == (int)registration.Stage.Value
                   && registeredEvent.Message == registration.Message
                   && (string.IsNullOrWhiteSpace(registeredEvent.EntityLogicalName)
                       || registeredEvent.EntityLogicalName == registration.EntityLogicalName);
        }

        public PluginAssertions<TPlugin> IsConsistent()
        {
            return HasUniqueId()
                .ImagesUseContextManagerNames()
                .MatchesRegisteredEvents();
        }

        #endregion

        #region Execution

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
