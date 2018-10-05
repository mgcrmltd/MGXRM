using System.Linq;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.ImageManagement
{
    public class EntityAttributeVersion : IEntityAttributeVersion
    {
        private readonly Entity[] _images;

        public EntityAttributeVersion(params Entity[] entitiesInOrder)
        {
            _images = entitiesInOrder;
        }

        public object GetLatestImageVersion(string attributeName)
        {
            return (from entity in _images
                where entity != null && entity.Attributes.ContainsKey(attributeName)
                select entity.Attributes[attributeName]).FirstOrDefault();
        }

        public T GetLatestImageVersion<T>(string attributeName) where T : class
        {
            return GetLatestImageVersion(attributeName) as T;
        }
    }
}
