using System;

namespace MGXRM.Common.Framework.AzureEndpointLogger
{
    public interface IQueueLogger
    {
        string Execute(Guid endpointGuid);
    }
}
