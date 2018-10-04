using System;
using MGXRM.Common.Framework.Extensions;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework
{
    public class EntityManager<T> : IEntityAttributeVersion, IEntityManager<T> where T : Entity
    {
        #region Members and Properties
        public Entity PreImage { get; }
        public Entity TargetImage { get;  }
        public Entity PostImage { get; }

        public T PreImageCast => PreImage.ToEntity<T>();
        public T TargetImageCast => TargetImage.ToEntity<T>();
        public T PostImageCast => PostImage.ToEntity<T>();

        public Entity CombinedImage => PreImage.ImposeEntity(PostImage).ImposeEntity(TargetImage);
        public T CombinedImageCast => CombinedImage.ToEntity<T>();

        protected IEntityAttributeVersion EntityAttributeVersion;
        #endregion

        #region Constructors
        public EntityManager() { }
        public EntityManager(Entity preImage, Entity targetImage, Entity postImage)
        {
            PreImage = preImage;
            PostImage = postImage;
            TargetImage = targetImage;
            EntityAttributeVersion = new EntityAttributeVersion(TargetImage,PostImage,PreImage);
        }

        public EntityManager(Entity preImage, Entity targetImage, Entity postImage, IEntityAttributeVersion entityAttributeVersion)
        {
            PreImage = preImage;
            PostImage = postImage;
            TargetImage = targetImage;
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

        public bool ImageDateAfterToday(string attributeName)
        {
            throw new NotImplementedException();
        }

        public bool ImageDateBeforeToday(string attributeName)
        {
            throw new NotImplementedException();
        }

        public bool ImageDateInTheFuture(string attributeName)
        {
            throw new NotImplementedException();
        }

        public bool ImageDateInThePast(string attributeName)
        {
            throw new NotImplementedException();
        }

        public bool ImageValueEqualTo0(string attributeName)
        {
            throw new NotImplementedException();
        }

        public bool ImageValueGreaterThan0(string attributeName)
        {
            throw new NotImplementedException();
        }

        public bool IsBeingAssigned()
        {
            throw new NotImplementedException();
        }

        public bool IsBeingSetAsNull(string attributeName)
        {
            throw new NotImplementedException();
        }

        public bool IsBeingSetOrUpdated(string attributeName)
        {
            throw new NotImplementedException();
        }

        public void RemoveUpdateValue(string attributeName)
        {
            throw new NotImplementedException();
        }

        public void SetOrUpdate(string attributeName, object value)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

