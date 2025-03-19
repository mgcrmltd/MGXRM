using System;
using FakeItEasy;
using FakeXrmEasy.Extensions;
using MGXRM.Common.Framework.ImageManagement;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace MGXRM.Common.Tests.Framework.ImageManagement
{
    public class ImageManagerTest
    {
        #region Members and constructors
        protected const string EntityName = "mgxrm_Entity";
        protected const string FieldName = "mgxrm_field";
        protected Entity FullEntity;
        public ImageManagerTest()
        {
            FullEntity = new Entity(EntityName);
            FullEntity["bool"] = true;
            FullEntity["dateTime"] = DateTime.Now;
            FullEntity["entityReference"] = new EntityReference(EntityName, Guid.NewGuid());
            FullEntity["int"] = 1;
            FullEntity["money"] = new Money((decimal)10.5);
            FullEntity["string"] = "Hello World";
            FullEntity["optionSetValue"] = new OptionSetValue(1);
            FullEntity["guid"] = Guid.NewGuid();
        }
        #endregion

        #region GetLatestAttribute tests
        [Fact]
        public void GetLatestBool_Calls_IEntityAttributeVersion_GetLatestImage_And_Casts()
        {
            var entityAttributeVersionFake = A.Fake<IImageAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImage(A<string>._)).Returns(FullEntity);

            var em = new ImageManager<Entity>(FullEntity, null, null, entityAttributeVersionFake);
            var val = em.GetLatestBool("bool");

            A.CallTo(() => entityAttributeVersionFake.GetLatestImage("bool")).MustHaveHappened();
            Assert.IsType<bool>(val);
        }

        [Fact]
        public void GetLatestBool_Can_Return_Null()
        {
            var em = new ImageManager<Entity>(null, null, null);
            var val = em.GetLatestBool(FieldName);
            Assert.Null(val);
        }

        [Fact]
        public void GetLatestDate_Calls_IEntityAttributeVersion_GetLatestImage()
        {
            var entityAttributeVersionFake = A.Fake<IImageAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImage(A<string>._)).Returns(FullEntity);

            var em = new ImageManager<Entity>(FullEntity, null, null, entityAttributeVersionFake);
            var val = em.GetLatestDate("dateTime");

            A.CallTo(() => entityAttributeVersionFake.GetLatestImage("dateTime")).MustHaveHappened();
            Assert.IsType<DateTime>(val);
        }

        [Fact]
        public void GetLatestDate_Can_Return_Null()
        {
            var em = new ImageManager<Entity>(null, null, null);
            var val = em.GetLatestDate(FieldName);
            Assert.Null(val);
        }

        [Fact]
        public void GetLatestEntityReference_Calls_IEntityAttributeVersion_GetLatestImage()
        {
            var entityAttributeVersionFake = A.Fake<IImageAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImage(A<string>._)).Returns(FullEntity);

            var em = new ImageManager<Entity>(FullEntity, null, null, entityAttributeVersionFake);
            em.GetLatestEntityReference("entityReference");

            A.CallTo(() => entityAttributeVersionFake.GetLatestImage("entityReference")).MustHaveHappened();
        }

        [Fact]
        public void GetLatestInt_Calls_IEntityAttributeVersion_GetLatestImage()
        {
            var entityAttributeVersionFake = A.Fake<IImageAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImage(A<string>._)).Returns(FullEntity);

            var em = new ImageManager<Entity>(null, FullEntity, null, entityAttributeVersionFake);
            em.GetLatestInt("int");

            A.CallTo(() => entityAttributeVersionFake.GetLatestImage("int")).MustHaveHappened();
        }
        
        [Fact]
        public void GetLatestMoney_Calls_IEntityAttributeVersion_GetLatestImage()
        {
            var entityAttributeVersionFake = A.Fake<IImageAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImage(A<string>._)).Returns(FullEntity);

            var em = new ImageManager<Entity>(FullEntity, null, null, entityAttributeVersionFake);
            var val = em.GetLatestMoney("money");

            A.CallTo(() => entityAttributeVersionFake.GetLatestImage("money")).MustHaveHappened();
            Assert.IsType<Money>(val);
        }

        [Fact]
        public void GetLatestMoney_Can_Return_Null()
        {
            var em = new ImageManager<Entity>(null, null, null);
            var val = em.GetLatestMoney(FieldName);
            Assert.Null(val);
        }

        [Fact]
        public void GetLatestMoneyValue_Calls_IEntityAttributeVersion_GetLatestImage()
        {
            var entityAttributeVersionFake = A.Fake<IImageAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImage(A<string>._)).Returns(FullEntity);

            var em = new ImageManager<Entity>(FullEntity, null, null, entityAttributeVersionFake);
            var val = em.GetLatestMoneyValue("money");

            A.CallTo(() => entityAttributeVersionFake.GetLatestImage("money")).MustHaveHappened();
            Assert.IsType<decimal>(val);
        }

        [Fact]
        public void GetLatestMoneyValue_Can_Return_Null()
        {
            var em = new ImageManager<Entity>(null, null, null);
            var val = em.GetLatestMoneyValue(FieldName);
            Assert.Null(val);
        }

        [Fact]
        public void GetLatestOptionSet_Calls_IEntityAttributeVersion_GetLatestImage()
        {
            var entityAttributeVersionFake = A.Fake<IImageAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImage(A<string>._)).Returns(FullEntity);

            var em = new ImageManager<Entity>(FullEntity, null, null, entityAttributeVersionFake);
            var val = em.GetLatestOptionSet("optionSetValue");

            A.CallTo(() => entityAttributeVersionFake.GetLatestImage("optionSetValue")).MustHaveHappened();
            Assert.IsType<OptionSetValue>(val);
        }

        [Fact]
        public void GetLatestString_Calls_IEntityAttributeVersion_GetLatestImageVersion()
        {
            var entityAttributeVersionFake = A.Fake<IImageAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImage(A<string>._)).Returns(FullEntity);

            var em = new ImageManager<Entity>(FullEntity, null, null, entityAttributeVersionFake);
            var val = em.GetLatestString("string");

            A.CallTo(() => entityAttributeVersionFake.GetLatestImage("string")).MustHaveHappened();
            Assert.IsType<string>(val);
        }
        
        [Fact]
        public void GetLatestGuid_Calls_IEntityAttributeVersion_GetLatestImage_And_Casts()
        {
            var entityAttributeVersionFake = A.Fake<IImageAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImage(A<string>._)).Returns(FullEntity);

            var em = new ImageManager<Entity>(FullEntity, null, null, entityAttributeVersionFake);
            var val = em.GetLatestGuid("guid");

            A.CallTo(() => entityAttributeVersionFake.GetLatestImage("guid")).MustHaveHappened();
            Assert.IsType<Guid>(val);
        }
        
        [Fact]
        public void GetLatestGuid_Can_Return_Null()
        {
            var em = new ImageManager<Entity>(null, null, null);
            var val = em.GetLatestGuid(FieldName);
            Assert.Null(val);
        }
        #endregion

        #region Get Image tests
        [Fact]
        public void GetLatestImageVersion_Calls_IEntityAttributeVersion_GetLatestImageVersion()
        {
            var entityAttributeVersionFake = A.Fake<IImageAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(A<string>._)).Returns("pre");

            var em = new ImageManager<Entity>(null, null, null, entityAttributeVersionFake);
            em.GetLatestImageVersion(FieldName);

            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(FieldName)).MustHaveHappened();
        }

        [Fact]
        public void CombinedImage_Uses_Target_Over_Post_Over_Pre()
        {
            var targetImage = new Entity(EntityName);
            targetImage.AddAttribute("target", null);

            var postImage = new Entity(EntityName);
            postImage.AddAttribute("target", "post");
            postImage.AddAttribute("post", "post");

            var preImage = new Entity(EntityName);
            preImage.AddAttribute("target", "pre");
            preImage.AddAttribute("post", "pre");
            preImage.AddAttribute("pre", "pre");

            var em = new ImageManager<Entity>(preImage, targetImage, postImage);
            Assert.Equal(em.CombinedImageEntity["target"], targetImage["target"]);
            Assert.Equal(em.CombinedImageEntity["post"], postImage["post"]);
            Assert.Equal(em.CombinedImageEntity["pre"], preImage["pre"]);
        }

        [Theory]
        [InlineData(ImageType.Pre, 2)]
        [InlineData(ImageType.Post, 1)]
        [InlineData(ImageType.Target, 0)]
        public void ImageType_Values_Correct(ImageType type, int value)
        {
            Assert.Equal((int)type, value);
        }

        [Theory]
        [InlineData(ImageType.Pre, "PRE")]
        [InlineData(ImageType.Post, "POST")]
        [InlineData(ImageType.Target, "TARGET")]
        public void GetImage_Returns_Specified_Image(ImageType type, string logicalName)
        {
            var preImage = new Entity("PRE");
            var postImage = new Entity("POST");
            var targetImage = new Entity("TARGET");

            var imageManager = new ImageManager<Entity>(preImage,targetImage,postImage);
            Assert.Equal(imageManager.GetImage(type).LogicalName, logicalName);
        }

        [Theory]
        [InlineData(ImageType.Pre)]
        [InlineData(ImageType.Post)]
        [InlineData(ImageType.Target)]
        public void GetImage_Returns_Null_If_Absent(ImageType type)
        {
            var imageManager = new ImageManager<Entity>(null, null, null);
            Assert.Null(imageManager.GetImage(type));
        }
        
        [Fact]
        public void PreImage_Returns_PreImage()
        {
            var preImage = new Entity("PRE");
            var postImage = new Entity("POST");
            var targetImage = new Entity("TARGET");

            var imageManager = new ImageManager<Entity>(preImage, targetImage, postImage);
            Assert.Equal(imageManager.PreImage.LogicalName, preImage.LogicalName);
        }

        [Fact]
        public void PostImage_Returns_PostImage()
        {
            var preImage = new Entity("PRE");
            var postImage = new Entity("POST");
            var targetImage = new Entity("TARGET");

            var imageManager = new ImageManager<Entity>(preImage, targetImage, postImage);
            Assert.Equal(imageManager.PostImage.LogicalName, postImage.LogicalName);
        }

        [Fact]
        public void TargetImage_Returns_TargetImage()
        {
            var preImage = new Entity("PRE");
            var postImage = new Entity("POST");
            var targetImage = new Entity("TARGET");

            var imageManager = new ImageManager<Entity>(preImage, targetImage, postImage);
            Assert.Equal(imageManager.TargetImage.LogicalName, targetImage.LogicalName);
        }
        #endregion

        #region Set and update tests
        [Fact]
        public void IsBeingSetOrUpdated_Returns_False_If_No_Target_Image()
        {
            var imageManager = new ImageManager<Entity>(null, null, null);
            Assert.Null(imageManager.TargetImage);
            Assert.False(imageManager.IsBeingSetOrUpdated(FieldName));
        }

        [Fact]
        public void IsBeingSetOrUpdated_Returns_False_If_Target_Does_Not_Contain_Attribute()
        {
            var imageManager = new ImageManager<Entity>(null, new Entity("TARGET"), null);
            Assert.False(imageManager.TargetImage.Contains(FieldName));
            Assert.False(imageManager.IsBeingSetOrUpdated(FieldName));
        }

        [Fact]
        public void IsBeingSetOrUpdated_Returns_True_If_Target_Contains_Attribute()
        {
            var targetImage = new Entity("TARGET");
            targetImage[FieldName] = 1;

            var imageManager = new ImageManager<Entity>(null, targetImage, null);
            Assert.True(imageManager.TargetImage.Contains(FieldName));
            Assert.True(imageManager.IsBeingSetOrUpdated(FieldName));
        }

        [Fact]
        public void IsBeingSetOrUpdated_Returns_True_If_Target_Contains_Null_Attribute()
        {
            var targetImage = new Entity("TARGET");
            targetImage[FieldName] = null;

            var imageManager = new ImageManager<Entity>(null, targetImage, null);
            Assert.True(imageManager.TargetImage.Contains(FieldName));
            Assert.True(imageManager.IsBeingSetOrUpdated(FieldName));
        }

        [Fact]
        public void IsBeingSetAsNull_Returns_False_If_Not_In_Target_Or_Target_Null()
        {
            var imageManager = new ImageManager<Entity>(null, new Entity("TARGET"), null);
            Assert.False(imageManager.TargetImage.Contains(FieldName));
            Assert.False(imageManager.IsBeingSetAsNull(FieldName));

            imageManager = new ImageManager<Entity>(null, null, null);
            Assert.Null(imageManager.TargetImage);
            Assert.False(imageManager.IsBeingSetAsNull(FieldName));
        }

        [Fact]
        public void IsBeingSetAsNull_Returns_False_If_In_Target_But_Not_Null()
        {
            var targetImage = new Entity("TARGET");
            targetImage[FieldName] = 1;

            var imageManager = new ImageManager<Entity>(null, targetImage, null);
            Assert.True(imageManager.TargetImage.Contains(FieldName));
            Assert.NotNull(imageManager.TargetImage[FieldName]);
            Assert.False(imageManager.IsBeingSetAsNull(FieldName));
        }

        [Fact]
        public void IsBeingSetAsNull_Returns_True_If_In_Target_And_Null()
        {
            var targetImage = new Entity("TARGET");
            targetImage[FieldName] = null;

            var imageManager = new ImageManager<Entity>(null, targetImage, null);
            Assert.True(imageManager.TargetImage.Contains(FieldName));
            Assert.Null(imageManager.TargetImage[FieldName]);
            Assert.True(imageManager.IsBeingSetAsNull(FieldName));
        }

        [Fact]
        public void SetOrUpdate_Throws_Exception_If_No_Target_Image()
        {
            var imageManager = new ImageManager<Entity>(null, null, null);
            Assert.Throws<InvalidPluginExecutionException>(() => imageManager.SetOrUpdate(FieldName, 1));
        }

        [Fact]
        public void SetOrUpdate_Sets_Value_In_Target_Image()
        {
            var imageManager = new ImageManager<Entity>(null, new Entity(EntityName), null);
            Assert.False(imageManager.TargetImage.Contains(FieldName));
            imageManager.SetOrUpdate(FieldName, 1);
            Assert.True(imageManager.TargetImage.Contains(FieldName));
        }

        [Fact]
        public void RemoveSetOrUpdateValue_Throws_Exception_If_No_Target_Image()
        {
            var imageManager = new ImageManager<Entity>(null, null, null);
            Assert.Throws<InvalidPluginExecutionException>(() => imageManager.RemoveSetOrUpdateValue(FieldName));
        }
        #endregion
    }
}
