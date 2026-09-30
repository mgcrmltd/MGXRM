using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MGXRM.Plugins.Tests.Framework
{
    public static class PluginConventions
    {
        public static void AssertRegistrationIdsAreUnique()
        {
            var steps = AllSteps().ToList();

            var missing = steps.Where(s => string.IsNullOrWhiteSpace(s.Registration.Id)).ToList();
            Assert.True(!missing.Any(),
                $"These steps have no Id, so spkl cannot update them in place: {Describe(missing)}.");

            var malformed = steps.Where(s => !Guid.TryParse(s.Registration.Id, out _)).ToList();
            Assert.True(!malformed.Any(),
                $"These steps have an Id that is not a guid: {Describe(malformed)}.");

            var duplicates = steps
                .GroupBy(s => Guid.Parse(s.Registration.Id))
                .Where(g => g.Count() > 1)
                .ToList();

            Assert.True(!duplicates.Any(),
                $"These step Ids are used more than once: {string.Join("; ", duplicates.Select(g => $"{g.Key} -> {Describe(g)}"))}.");
        }

        public static void AssertEveryPluginIsRegistered()
        {
            var unregistered = PluginReflection.GetPluginTypes()
                .Where(t => !PluginReflection.GetRegistrations(t).Any())
                .ToList();

            Assert.True(!unregistered.Any(),
                $"These plugins have no CrmPluginRegistrationAttribute and would never be deployed: {string.Join(", ", unregistered.Select(t => t.Name))}.");
        }

        public static void AssertRegistrationsMatchRegisteredEvents()
        {
            foreach (var pluginType in PluginReflection.GetPluginTypes())
                new PluginAssertions<Plugin>(pluginType).MatchesRegisteredEvents();
        }

        public static void AssertImagesUseContextManagerNames()
        {
            foreach (var pluginType in PluginReflection.GetPluginTypes())
                new PluginAssertions<Plugin>(pluginType).ImagesUseContextManagerNames();
        }

        private static IEnumerable<PluginStep> AllSteps()
        {
            return PluginReflection.GetPluginTypes()
                .SelectMany(t => PluginReflection.GetRegistrations(t).Select(r => new PluginStep(t, r)));
        }

        private static string Describe(IEnumerable<PluginStep> steps)
        {
            return string.Join(", ", steps.Select(s => s.ToString()));
        }

        private class PluginStep
        {
            public Type PluginType { get; }
            public CrmPluginRegistrationAttribute Registration { get; }

            public PluginStep(Type pluginType, CrmPluginRegistrationAttribute registration)
            {
                PluginType = pluginType;
                Registration = registration;
            }

            public override string ToString()
            {
                return $"{PluginType.Name} ('{Registration.Name}')";
            }
        }
    }
}
