using System;
using System.Linq;
using MGXRM.Common.Framework.Extensions;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.ImageManagement
{
    public class EntityManager<T> : IEntityAttributeVersion, IEntityManager<T> where T : Entity
    {
        #region Members and Properties
        private readonly Entity[] _images;
        protected IEntityAttributeVersion EntityAttributeVersion;
        public Entity CombinedImageEntity {
            get
            {
                var inReverse = _images.Reverse();
                return inReverse.Aggregate<Entity, Entity>(null, (current, i) => current.ImposeEntity(i));
            }
        }
        #endregion

        #region Constructors

        public EntityManager(Entity preImage, Entity targetImage, Entity postImage)
        {
            _images = new[] {targetImage, postImage, preImage};
            EntityAttributeVersion = new EntityAttributeVersion(_images);
        }

        public EntityManager(Entity preImage, Entity targetImage, Entity postImage, IEntityAttributeVersion entityAttributeVersion)
        {
            _images = new[] { targetImage, postImage, preImage };
            EntityAttributeVersion = entityAttributeVersion;
        }
        #endregion

        #region Methods
        public object GetLatestImageVersion(string attributeName)
        { 
            return EntityAttributeVersion.GetLatestImageVersion(attributeName);
        }

        public T1 GetLatestImageVersion<T1>(string attributeName) where T1 : class
        {
            return EntityAttributeVersion.GetLatestImageVersion<T1>(attributeName);
        }

        public bool? GetLatestBool(string attributeName)
        {
            var obj = GetLatestImageVersion(attributeName);
            if (obj != null)
                return ((bool)GetLatestImageVersion(attributeName));
            return null;
        }

        public DateTime? GetLatestDate(string attributeName)
        {
            var obj = GetLatestImageVersion(attributeName);
            if (obj != null)
                return ((DateTime)GetLatestImageVersion(attributeName));
            return null;
        }

        public EntityReference GetLatestEntityReference(string attributeName)
        {
            return GetLatestImageVersion<EntityReference>(attributeName);
        }
        
        public int? GetLatestInt(string attributeName)
        {
            var obj = GetLatestImageVersion(attributeName);
            return (int?)obj;
        }

        public Money GetLatestMoney(string attributeName)
        {
            return GetLatestImageVersion<Money>(attributeName);
        }

        public decimal? GetLatestMoneyValue(string attributeName)
        {
            var obj = GetLatestImageVersion(attributeName);
            if (obj != null)
                return ((Money)GetLatestImageVersion(attributeName)).Value;
            return null;
        }

        public OptionSetValue GetLatestOptionSet(string attributeName)
        {
            return GetLatestImageVersion<OptionSetValue>(attributeName);
        }

        public string GetLatestString(string attributeName)
        {
            return GetLatestImageVersion<string>(attributeName);
        }
        #endregion
    }
}

