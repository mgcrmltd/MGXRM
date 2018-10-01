using MGXRM.Common.Framework;
using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace MGXRM.Common.Tests.Framework
{
    public class EntityManagerTest
    {
        protected const string EntityName = "mgxrm_Entity";
        protected const string FieldName = "mgxrm_description";

        [Fact]
        public void GetLatestImageVersion_Selects_TargetImage_Value_First()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            preImage.Attributes.Add(FieldName, "pre");
            var targetImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            targetImage.Attributes.Add(FieldName, "target");
            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            postImage.Attributes.Add(FieldName, "post");

            var em = new EntityManager<Entity>(preImage, targetImage, postImage);
            Assert.Equal("target", em.GetLatestImageVersion(FieldName));
        }

        [Fact]
        public void GetLatestImageVersion_Selects_TargetImage_Value_If_Present_And_Null()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            preImage.Attributes.Add(FieldName, "pre");
            var targetImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            targetImage.Attributes.Add(FieldName, null);
            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            postImage.Attributes.Add(FieldName, "post");

            Assert.Null(targetImage[FieldName]);
            var em = new EntityManager<Entity>(preImage, targetImage, postImage);
           
            Assert.Null(em.GetLatestImageVersion(FieldName));
        }

        [Fact]
        public void GetLatestImageVersion_Selects_PostImage_Value_If_Present_And_No_Target()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            preImage.Attributes.Add(FieldName, "pre");
            var targetImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            postImage.Attributes.Add(FieldName, "post");

            Assert.False(targetImage.Attributes.ContainsKey(FieldName));
            var em = new EntityManager<Entity>(preImage, targetImage, postImage);
            Assert.Equal("post",em.GetLatestImageVersion(FieldName));
        }

        [Fact]
        public void GetLatestImageVersion_Selects_PostImage_Value_If_Present_And_Null()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            preImage.Attributes.Add(FieldName, "pre");
         
            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            postImage.Attributes.Add(FieldName, null);

            Assert.Null(postImage[FieldName]);
            var em = new EntityManager<Entity>(preImage, null, postImage);

            Assert.Null(em.GetLatestImageVersion(FieldName));
        }

        [Fact]
        public void GetLatestImageVersion_Selects_PreImage_Value_If_Present_And_No_Target_Or_Post()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            preImage.Attributes.Add(FieldName, "pre");

            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            var targetImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };

            var em = new EntityManager<Entity>(preImage, targetImage, postImage);

            Assert.Equal("pre",em.GetLatestImageVersion(FieldName));
        }

        [Fact]
        public void GetLatestImageVersion_Selects_PreImage_Value_If_Present_And_Null()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            preImage.Attributes.Add(FieldName, null);
            
            Assert.Null(preImage[FieldName]);
            var em = new EntityManager<Entity>(preImage, null, null);

            Assert.Null(em.GetLatestImageVersion(FieldName));
        }

        [Fact]
        public void GetLatestImageVersion_Returns_Null_If_Cannot_Find_In_An_Image()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            var targetImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };

            Assert.False(preImage.Attributes.ContainsKey(FieldName));
            Assert.False(targetImage.Attributes.ContainsKey(FieldName));
            Assert.False(postImage.Attributes.ContainsKey(FieldName));
            var em = new EntityManager<Entity>(preImage, null, preImage);
            Assert.Null(em.GetLatestImageVersion(FieldName));
        }
    }
}
