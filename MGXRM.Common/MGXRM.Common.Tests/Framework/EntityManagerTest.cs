using MGXRM.Common.Framework;
using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace MGXRM.Common.Tests.Framework
{
    public class EntityManagerTest
    {
        [Fact]
        public void GetLatestImageVersion_Selects_TargetImage_Value_First()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            preImage.Attributes.Add("mgxrm_description", "pre");
            var targetImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            targetImage.Attributes.Add("mgxrm_description", "target");
            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            postImage.Attributes.Add("mgxrm_description", "post");

            var em = new EntityManager<Entity>(preImage, targetImage, postImage);
            Assert.Equal("target", em.GetLatestImageVersion("mgxrm_description"));
        }

        [Fact]
        public void GetLatestImageVersion_Selects_TargetImage_Value_If_Present_And_Null()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            preImage.Attributes.Add("mgxrm_description", "pre");
            var targetImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            targetImage.Attributes.Add("mgxrm_description", null);
            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            postImage.Attributes.Add("mgxrm_description", "post");

            Assert.Null(targetImage["mgxrm_description"]);
            var em = new EntityManager<Entity>(preImage, targetImage, postImage);
           
            Assert.Null(em.GetLatestImageVersion("mgxrm_description"));
        }

        [Fact]
        public void GetLatestImageVersion_Selects_PostImage_Value_If_Present_And_No_Target()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            preImage.Attributes.Add("mgxrm_description", "pre");
            var targetImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            postImage.Attributes.Add("mgxrm_description", "post");

            Assert.False(targetImage.Attributes.ContainsKey("mgxrm_description"));
            var em = new EntityManager<Entity>(preImage, targetImage, postImage);
            Assert.Equal("post",em.GetLatestImageVersion("mgxrm_description"));
        }

        [Fact]
        public void GetLatestImageVersion_Selects_PostImage_Value_If_Present_And_Null()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            preImage.Attributes.Add("mgxrm_description", "pre");
         
            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            postImage.Attributes.Add("mgxrm_description", null);

            Assert.Null(postImage["mgxrm_description"]);
            var em = new EntityManager<Entity>(preImage, null, postImage);

            Assert.Null(em.GetLatestImageVersion("mgxrm_description"));
        }

        [Fact]
        public void GetLatestImageVersion_Selects_PreImage_Value_If_Present_And_No_Target_Or_Post()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            preImage.Attributes.Add("mgxrm_description", "pre");

            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            var targetImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };

            var em = new EntityManager<Entity>(preImage, targetImage, postImage);

            Assert.Equal("pre",em.GetLatestImageVersion("mgxrm_description"));
        }

        [Fact]
        public void GetLatestImageVersion_Selects_PreImage_Value_If_Present_And_Null()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            preImage.Attributes.Add("mgxrm_description", null);
            
            Assert.Null(preImage["mgxrm_description"]);
            var em = new EntityManager<Entity>(preImage, null, null);

            Assert.Null(em.GetLatestImageVersion("mgxrm_description"));
        }

        [Fact]
        public void GetLatestImageVersion_Returns_Null_If_Cannot_Find_In_An_Image()
        {
            var preImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            var targetImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };
            var postImage = new Entity() { Id = Guid.NewGuid(), LogicalName = "mgxrm_Entity" };

            Assert.False(preImage.Attributes.ContainsKey("mgxrm_description"));
            Assert.False(targetImage.Attributes.ContainsKey("mgxrm_description"));
            Assert.False(postImage.Attributes.ContainsKey("mgxrm_description"));
            var em = new EntityManager<Entity>(preImage, null, preImage);
            Assert.Null(em.GetLatestImageVersion("mgxrm_description"));
        }
    }
}
