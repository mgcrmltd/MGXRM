using System;

namespace MGXRM.Plugins
{
    public abstract class CustomApi : Plugin
    {
        public const int MainOperationStage = 30;

        protected CustomApi(Type childClassName, string uniqueName)
            : this(childClassName, uniqueName, string.Empty)
        {
        }

        protected CustomApi(Type childClassName, string uniqueName, string boundEntityLogicalName)
            : base(childClassName)
        {
            if (string.IsNullOrWhiteSpace(uniqueName))
                throw new ArgumentException("A custom api unique name is required.", nameof(uniqueName));

            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(
                MainOperationStage, uniqueName, boundEntityLogicalName ?? string.Empty, ExecuteCustomApi));
        }

        protected abstract void ExecuteCustomApi(LocalPluginContext localContext);
    }
}
