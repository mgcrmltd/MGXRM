
namespace MGXRM.Common.Framework.Interfaces
{
    public interface IImageAttributeVersion 
    {
        object GetLatestImageVersion(string attributeName);
        T GetLatestImageVersion<T>(string attributeName) where T : class;
    }
}
