using System;
using Microsoft.Xrm.Sdk;

namespace Ceox.Common.Framework.AzureEndpointLogger
{
    public class QueueLogger : IQueueLogger
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPluginExecutionContext _context;
        private readonly IServiceEndpointNotificationService _cloudService;

        public const string EndpointEntityName = "serviceendpoint";

        public QueueLogger(IPluginExecutionContext context,
            IServiceEndpointNotificationService cloudService)
        {
            _serviceProvider = null;
            _context = context;
            _cloudService = cloudService;
        }

        public QueueLogger(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _context = (IPluginExecutionContext)_serviceProvider.GetService(typeof(IPluginExecutionContext));
            _cloudService = (IServiceEndpointNotificationService)_serviceProvider.GetService(typeof(IServiceEndpointNotificationService));
            if (_cloudService == null)
                throw new InvalidPluginExecutionException("Failed to retrieve the service bus service.");
        }

        public string Execute(Guid endpointGuid)
        {
            return _cloudService.Execute(new EntityReference(EndpointEntityName, endpointGuid), _context);
        }
    }
}
