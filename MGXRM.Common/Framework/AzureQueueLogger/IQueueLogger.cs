using System;

namespace Ceox.Common.Framework.AzureEndpointLogger
{
    public interface IQueueLogger
    {
        string Execute(Guid endpointGuid);
    }
}
