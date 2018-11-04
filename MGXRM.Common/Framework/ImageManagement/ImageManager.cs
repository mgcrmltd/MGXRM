using System;
using System.Linq;
using MGXRM.Common.Framework.Extensions;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.ImageManagement
{
    public class ImageManager<T> : IImageAttributeVersion, IImageManager<T> where T : Entity
    {
        #region Members and Properties
        private readonly Entity[] _images;
        protected IImageAttributeVersion ImageAttributeVersion;
        public Entity PreImage => GetImage(ImageType.Pre);
        public Entity PostImage => GetImage(ImageType.Post);
        public Entity TargetImage => GetImage(ImageType.Target);
        public Entity CombinedImageEntity {
            get
            {
                var inReverse = _images.Reverse();
                return inReverse.Aggregate<Entity, Entity>(null, (current, i) => current.ImposeEntity(i));
            }
        }
        #endregion

        #region Constructors

        public ImageManager(Entity preImage, Entity targetImage, Entity postImage)
        {
            _images = new[] {targetImage, postImage, preImage};
            ImageAttributeVersion = new ImageAttributeVersion(_images);
        }

        public ImageManager(Entity preImage, Entity targetImage, Entity postImage, IImageAttributeVersion imageAttributeVersion)
        {
            _images = new[] { targetImage, postImage, preImage };
            ImageAttributeVersion = imageAttributeVersion;
        }
        #endregion

        #region Methods
        public object GetLatestImageVersion(string attributeName)
        { 
            return ImageAttributeVersion.GetLatestImageVersion(attributeName);
        }

        public Entity GetLatestImage(string attributeName)
        {
            return ImageAttributeVersion.GetLatestImage(attributeName);
        }

        public T1 GetLatestImageVersion<T1>(string attributeName) where T1 : class
        {
            return ImageAttributeVersion.GetLatestImageVersion<T1>(attributeName);
        }

        public bool? GetLatestBool(string attributeName)
        {
            return GetLatestImage(attributeName)?.GetBool(attributeName);
        }

        public DateTime? GetLatestDate(string attributeName)
        {
            return GetLatestImage(attributeName)?.GetDateTime(attributeName);
        }

        public EntityReference GetLatestEntityReference(string attributeName)
        {
            return GetLatestImage(attributeName)?.GetEntityReference(attributeName);
        }
        
        public int? GetLatestInt(string attributeName)
        {
            return GetLatestImage(attributeName)?.GetInt(attributeName);
        }

        public Money GetLatestMoney(string attributeName)
        {
            return GetLatestImage(attributeName)?.GetMoney(attributeName);
        }

        public decimal? GetLatestMoneyValue(string attributeName)
        {
            return GetLatestImage(attributeName)?.GetMoneyValue(attributeName);
        }

        public OptionSetValue GetLatestOptionSet(string attributeName)
        {
            return GetLatestImage(attributeName)?.GetOptionSet(attributeName);
        }

        public string GetLatestString(string attributeName)
        {
            return GetLatestImage(attributeName)?.GetString(attributeName);
        }

        public Entity GetImage(ImageType type)
        {
            return _images.ElementAtOrDefault((int) type);
        }

        #endregion
    }
}

