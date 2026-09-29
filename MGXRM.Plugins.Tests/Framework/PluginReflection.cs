using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using MGXRM.Common.Framework.ContextManagement;

namespace MGXRM.Plugins.Tests.Framework
{
    /// <summary>
    /// One entry from the <c>base.RegisteredEvents.Add(...)</c> calls in a plugin's constructor.
    /// </summary>
    public class RegisteredEvent
    {
        public int Stage { get; }
        public string Message { get; }
        public string EntityLogicalName { get; }
        public MethodInfo Handler { get; }

        internal RegisteredEvent(int stage, string message, string entityLogicalName, MethodInfo handler)
        {
            Stage = stage;
            Message = message;
            EntityLogicalName = entityLogicalName;
            Handler = handler;
        }

        public override string ToString()
        {
            return $"Stage {Stage}, Message '{Message}', Entity '{EntityLogicalName}' -> {Handler?.Name}";
        }
    }

    /// <summary>
    /// One <see cref="CrmPluginRegistrationAttribute"/> image slot, flattened so the two numbered
    /// slots on the attribute can be treated as a collection.
    /// </summary>
    public class RegisteredImage
    {
        public int Slot { get; }
        public string Name { get; }
        public ImageTypeEnum Type { get; }
        public string[] Attributes { get; }

        internal RegisteredImage(int slot, string name, ImageTypeEnum type, string attributes)
        {
            Slot = slot;
            Name = name;
            Type = type;
            Attributes = PluginReflection.SplitCsv(attributes);
        }

        /// <summary>
        /// The alias <see cref="PluginContextManager{T}"/> will look for, or null where the framework
        /// cannot read an image of this type at all (see <see cref="ImageTypeEnum.Both"/>).
        /// </summary>
        public string ExpectedName
        {
            get
            {
                switch (Type)
                {
                    case ImageTypeEnum.PreImage:
                        return PluginContextManager.PreImageAlias;
                    case ImageTypeEnum.PostImage:
                        return PluginContextManager.PostImageAlias;
                    default:
                        return null;
                }
            }
        }
    }

    /// <summary>
    /// Reflection helpers over the plugin assembly: the registration attributes, the registered events
    /// declared in plugin constructors, and the calls a plugin's execute method makes.
    /// </summary>
    public static class PluginReflection
    {
        public static Assembly PluginAssembly => typeof(Plugin).Assembly;

        /// <summary>
        /// Every concrete class in the plugin assembly that derives from <see cref="Plugin"/>.
        /// </summary>
        public static IEnumerable<Type> GetPluginTypes()
        {
            return GetPluginTypes(PluginAssembly);
        }

        public static IEnumerable<Type> GetPluginTypes(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => typeof(Plugin).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(Plugin))
                .OrderBy(t => t.FullName);
        }

        public static IList<CrmPluginRegistrationAttribute> GetRegistrations(Type pluginType)
        {
            return pluginType
                .GetCustomAttributes(typeof(CrmPluginRegistrationAttribute), false)
                .Cast<CrmPluginRegistrationAttribute>()
                .ToList();
        }

        /// <summary>
        /// Reads the protected RegisteredEvents collection populated by the plugin's constructor.
        /// </summary>
        public static IList<RegisteredEvent> GetRegisteredEvents(Type pluginType)
        {
            var plugin = (Plugin)Activator.CreateInstance(pluginType);
            var property = typeof(Plugin).GetProperty("RegisteredEvents", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property == null)
                throw new InvalidOperationException("Plugin.RegisteredEvents could not be found by reflection - has the base class changed?");

            var events = (Collection<Tuple<int, string, string, Action<Plugin.LocalPluginContext>>>)property.GetValue(plugin);
            return events
                .Select(e => new RegisteredEvent(e.Item1, e.Item2, e.Item3, e.Item4?.Method))
                .ToList();
        }

        public static IList<RegisteredImage> GetImages(CrmPluginRegistrationAttribute registration)
        {
            var images = new List<RegisteredImage>();
            if (!string.IsNullOrWhiteSpace(registration.Image1Name))
                images.Add(new RegisteredImage(1, registration.Image1Name, registration.Image1Type, registration.Image1Attributes));
            if (!string.IsNullOrWhiteSpace(registration.Image2Name))
                images.Add(new RegisteredImage(2, registration.Image2Name, registration.Image2Type, registration.Image2Attributes));
            return images;
        }

        public static string[] GetFilteringAttributes(CrmPluginRegistrationAttribute registration)
        {
            return SplitCsv(registration.FilteringAttributes);
        }

        /// <summary>
        /// The methods called by <paramref name="handler"/>, following any private helpers declared on
        /// <paramref name="declaringPluginType"/> so a plugin that delegates through its own methods is
        /// still matched.
        /// </summary>
        public static IList<MethodBase> GetCalledMethods(MethodInfo handler, Type declaringPluginType)
        {
            var calls = new List<MethodBase>();
            var visited = new HashSet<MethodBase>();
            Walk(handler, declaringPluginType, calls, visited);
            return calls;
        }

        private static void Walk(MethodBase method, Type declaringPluginType, ICollection<MethodBase> calls, ISet<MethodBase> visited)
        {
            if (method == null || !visited.Add(method))
                return;

            foreach (var called in IlReader.GetCalledMethods(method))
            {
                calls.Add(called);
                if (called.DeclaringType == declaringPluginType)
                    Walk(called, declaringPluginType, calls, visited);
            }
        }

        internal static string[] SplitCsv(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new string[0];

            return value.Split(',')
                .Select(v => v.Trim())
                .Where(v => v.Length > 0)
                .ToArray();
        }
    }
}
