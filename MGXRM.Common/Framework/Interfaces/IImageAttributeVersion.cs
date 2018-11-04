using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.Interfaces
{
    public interface IImageAttributeVersion 
    {
        object GetLatestImageVersion(string attributeName);
        Entity GetLatestImage(string attributeName);
        T GetLatestImageVersion<T>(string attributeName) where T : class;
    }
}
