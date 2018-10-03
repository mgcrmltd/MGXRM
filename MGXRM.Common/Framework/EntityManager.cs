using System;
using MGXRM.Common.Framework.Extensions;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework
{
    public class EntityManager<T> : IEntityManager<T> where T : Entity
    {
        #region Members and Properties
        public Entity PreImage { get; private set; }
        public Entity TargetImage { get; private set; }
        public Entity PostImage { get; private set; }

        public T PreImageCast => PreImage.ToEntity<T>();
        public T TargetImageCast => TargetImage.ToEntity<T>();
        public T PostImageCast => PostImage.ToEntity<T>();

        public Entity CombinedImage => PreImage.ImposeEntity(PostImage).ImposeEntity(TargetImage);
        public T CombinedImageCast => CombinedImage.ToEntity<T>();
        #endregion

        #region Constructors
        public EntityManager() { }
        public EntityManager(Entity preImage, Entity targeImage, Entity postImage)
        {
            PreImage = preImage;
            PostImage = postImage;
            TargetImage = targeImage;
        }
        #endregion

        public object GetLatestImageVersion(string attributename)
        {
            if (TargetImage != null && TargetImage.Attributes.ContainsKey(attributename))
                return TargetImage.Attributes[attributename];
            if (PostImage != null && PostImage.Attributes.ContainsKey(attributename))
                return PostImage.Attributes[attributename];
            if (PreImage != null && PreImage.Attributes.ContainsKey(attributename))
                return PreImage.Attributes[attributename];

            return null;
        }

        public bool? GetLatestBool(string attributename)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetLatestDate(string attributename)
        {
            throw new NotImplementedException();
        }

        public EntityReference GetLatestEntityReference(string attributename)
        {
            throw new NotImplementedException();
        }
        
        public int? GetLatestInt(string attributename)
        {
            throw new NotImplementedException();
        }

        public Money GetLatestMoney(string attributename)
        {
            throw new NotImplementedException();
        }

        public decimal? GetLatestMoneyValue(string attributename)
        {
            throw new NotImplementedException();
        }

        public OptionSetValue GetLatestOptionSet(string attributename)
        {
            throw new NotImplementedException();
        }

        public string GetLatestOptionString(string attributename)
        {
            throw new NotImplementedException();
        }

        public string GetLatestString(string attributename)
        {
            throw new NotImplementedException();
        }

        public bool HasNonNullValue(Entity entity, string attributeName)
        {
            throw new NotImplementedException();
        }

        public bool HasValue(Entity entity, string attributeName)
        {
            throw new NotImplementedException();
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

        public void RemoveValue(Entity image, string attributeName)
        {
            throw new NotImplementedException();
        }

        public void SetOrUpdate(string attributeName, object value)
        {
            throw new NotImplementedException();
        }
    }
}

