using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.ContextManagement;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Model;
using MGXRM.Common.Framework.Repositories;
using MGXRM.Common.Tests.TestCore;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Xunit;

namespace MGXRM.Common.Tests.TestCore
{
    public class ModelSetupTest
    {
        #region Pipeline values

        [Fact]
        public void Defaults_To_A_Synchronous_PreOperation_Update_At_Depth_One()
        {
            var context = ModelSetup.For<TestSetupEntity>().Context;

            Assert.Equal("Update", context.Message);
            Assert.Equal(SdkMessageProcessingStep_Stage.Preoperation, context.Stage);
            Assert.Equal(SdkMessageProcessingStep_Mode.Synchronous, context.Mode);
            Assert.Equal(1, context.Depth);
        }

        [Theory]
        [InlineData("Create")]
        [InlineData("Update")]
        [InlineData("SetState")]
        public void Message_Is_Set_On_The_Context(string message)
        {
            Assert.Equal(message, ModelSetup.For<TestSetupEntity>().Message(message).Context.Message);
        }

        [Fact]
        public void Stage_Mode_And_Depth_Are_Set_On_The_Context()
        {
            var context = ModelSetup.For<TestSetupEntity>()
                .PostOperation()
                .Asynchronous()
                .Depth(3)
                .Context;

            Assert.Equal(SdkMessageProcessingStep_Stage.Postoperation, context.Stage);
            Assert.Equal(SdkMessageProcessingStep_Mode.Asynchronous, context.Mode);
            Assert.Equal(3, context.Depth);
        }

        [Fact]
        public void Entity_Logical_Name_Is_Taken_From_The_Early_Bound_Type()
        {
            Assert.Equal(TestSetupEntity.EntityLogicalName,
                ModelSetup.For<TestSetupEntity>().Context.PrimaryEntityName);
        }

        [Fact]
        public void Identity_Values_Are_Set_On_The_Context()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var context = ModelSetup.For<TestSetupEntity>().WithId(id).WithUser(userId).Context;

            Assert.Equal(id, context.PrimaryEntityId);
            Assert.Equal(userId, context.UserId);
        }

        [Fact]
        public void Input_And_Output_Parameters_Are_Set_On_The_Context()
        {
            var setup = ModelSetup.For<TestSetupEntity>()
                .WithInputParameter("Assignee", new EntityReference("systemuser", Guid.NewGuid()))
                .WithOutputParameter("id", Guid.NewGuid());

            Assert.True(setup.Context.InputParams.Contains("Assignee"));
            Assert.True(setup.Context.OutputParams.Contains("id"));
        }

        #endregion

        #region Images

        [Fact]
        public void Images_Are_Null_When_Not_Set_Up()
        {
            var setup = ModelSetup.For<TestSetupEntity>();

            Assert.Null(setup.Images.PreImage);
            Assert.Null(setup.Images.TargetImage);
            Assert.Null(setup.Images.PostImage);
        }

        [Fact]
        public void Target_Carries_Only_The_Attributes_The_Lambda_Sets()
        {
            var setup = ModelSetup.For<TestSetupEntity>().WithTarget(e => e.Name = "jones");

            Assert.Equal("jones", setup.Images.TargetImage.Name);
            Assert.Single(setup.Images.TargetImage.Attributes);
        }

        [Fact]
        public void Pre_And_Post_Images_Are_Readable_Through_The_Image_Manager()
        {
            var setup = ModelSetup.For<TestSetupEntity>()
                .PostOperation()
                .WithPreImage(e => e.Name = "smith")
                .WithPostImage(e => e.Name = "jones");

            Assert.Equal("smith", setup.Images.PreImage.Name);
            Assert.Equal("jones", setup.Images.PostImage.Name);
        }

        [Fact]
        public void Images_Are_Registered_Under_The_Context_Manager_Aliases()
        {
            var setup = ModelSetup.For<TestSetupEntity>()
                .PostOperation()
                .WithPreImage()
                .WithPostImage();

            Assert.True(setup.ExecutionContext.PreEntityImages.Contains(PluginContextManager.PreImageAlias));
            Assert.True(setup.ExecutionContext.PostEntityImages.Contains(PluginContextManager.PostImageAlias));
        }

        [Fact]
        public void Latest_Value_Comes_From_The_Target_Over_The_Pre_Image()
        {
            var setup = ModelSetup.For<TestSetupEntity>()
                .WithPreImage(e => e.Name = "smith")
                .WithTarget(e => e.Name = "jones");

            Assert.Equal("jones", setup.Images.GetLatestString(TestSetupEntity.Fields.Name));
        }

        [Fact]
        public void Latest_Value_Falls_Back_To_The_Pre_Image()
        {
            var setup = ModelSetup.For<TestSetupEntity>()
                .WithPreImage(e => e.Name = "smith")
                .WithTarget(e => e.Reference = new EntityReference("contact", Guid.NewGuid()));

            Assert.Equal("smith", setup.Images.GetLatestString(TestSetupEntity.Fields.Name));
        }

        #endregion

        #region Dependencies

        [Fact]
        public void Repository_And_Service_Are_Faked_And_Stable()
        {
            var setup = ModelSetup.For<TestSetupEntity>();

            Assert.NotNull(setup.Repository);
            Assert.Same(setup.Repository, setup.Repository);
            Assert.Same(setup.Service, setup.Context.Service);
        }

        [Fact]
        public void A_Supplied_Repository_Is_Used()
        {
            var repository = A.Fake<IRepository>();

            Assert.Same(repository, ModelSetup.For<TestSetupEntity>().WithRepository(repository).Repository);
        }

        #endregion

        #region Fake CRM

        [Fact]
        public void WithFakeCrm_Gives_The_Model_A_Real_Repository()
        {
            var setup = ModelSetup.For<TestSetupEntity>().WithFakeCrm();

            Assert.IsType<Repository>(setup.Repository);
            Assert.Same(setup.Service, setup.Repository.Service);
        }

        [Fact]
        public void A_Real_Repository_Query_Finds_Seeded_Records()
        {
            var existing = new TestSetupEntity { Id = Guid.NewGuid(), Name = "smith" };

            var setup = ModelSetup.For<TestSetupEntity>().WithFakeCrm(existing);

            var found = setup.Repository.RetrieveByAttribute(TestSetupEntity.EntityLogicalName,
                TestSetupEntity.Fields.Name, "smith");

            Assert.Single(found);
            Assert.Equal(existing.Id, found[0].Id);
        }

        [Fact]
        public void Seeded_Records_Without_An_Id_Are_Given_One()
        {
            var existing = new TestSetupEntity { Name = "smith" };

            var setup = ModelSetup.For<TestSetupEntity>().WithFakeCrm(existing);

            Assert.NotEqual(Guid.Empty, existing.Id);
            Assert.Single(setup.FakeCrm.CreateQuery<TestSetupEntity>());
        }

        [Fact]
        public void Records_A_Model_Creates_Are_Readable_From_The_Fake_Crm()
        {
            var setup = ModelSetup.For<TestSetupEntity>().WithFakeCrm();

            setup.Repository.Create(new TestSetupEntity { Name = "created" });

            Assert.Equal("created", setup.FakeCrm.CreateQuery<TestSetupEntity>().Single().Name);
        }

        [Fact]
        public void A_Supplied_Repository_Still_Wins_Over_The_Fake_Crm()
        {
            var repository = A.Fake<IRepository>();

            var setup = ModelSetup.For<TestSetupEntity>().WithFakeCrm().WithRepository(repository);

            Assert.Same(repository, setup.Repository);
        }

        [Fact]
        public void FakeCrm_Explains_Itself_When_It_Was_Not_Asked_For()
        {
            var setup = ModelSetup.For<TestSetupEntity>();

            var ex = Assert.Throws<InvalidOperationException>(() => setup.FakeCrm);
            Assert.Contains("WithFakeCrm", ex.Message);
        }

        #endregion

        #region Worked example - a model that uses the repository, tested both ways

        [Fact]
        public void A_Model_Can_Be_Tested_With_An_Arranged_Repository()
        {
            var repository = A.Fake<IRepository>();
            A.CallTo(() => repository.RetrieveByAttribute(TestSetupEntity.EntityLogicalName,
                    TestSetupEntity.Fields.Name, "smith"))
                .Returns(new List<Entity> { new TestSetupEntity { Id = Guid.NewGuid(), Name = "smith" } });

            var setup = ModelSetup.For<TestSetupEntity>()
                .Update().PreOperation()
                .WithTarget(e => e.Name = "smith")
                .WithRepository(repository);

            ModelFor(setup).FlagDuplicateName();

            A.CallTo(() => repository.Create(A<Entity>.That.Matches(
                e => (string)e[TestSetupEntity.Fields.Name] == "smith (duplicate)"))).MustHaveHappened();
        }

        [Fact]
        public void A_Model_Can_Be_Tested_Against_A_Seeded_Fake_Crm()
        {
            var existing = new TestSetupEntity { Id = Guid.NewGuid(), Name = "smith" };

            var setup = ModelSetup.For<TestSetupEntity>()
                .Update().PreOperation()
                .WithTarget(e => e.Name = "smith")
                .WithFakeCrm(existing);

            ModelFor(setup).FlagDuplicateName();

            var written = setup.FakeCrm.CreateQuery<TestSetupEntity>()
                .Where(e => e.Name == "smith (duplicate)")
                .ToList();
            Assert.Single(written);
        }

        [Fact]
        public void The_Model_Writes_Nothing_When_The_Query_Finds_No_Duplicate()
        {
            var setup = ModelSetup.For<TestSetupEntity>()
                .Update().PreOperation()
                .WithTarget(e => e.Name = "jones")
                .WithFakeCrm(new TestSetupEntity { Id = Guid.NewGuid(), Name = "smith" });

            ModelFor(setup).FlagDuplicateName();

            Assert.Single(setup.FakeCrm.CreateQuery<TestSetupEntity>());
        }

        [Fact]
        public void The_Model_Ignores_The_Record_Being_Updated()
        {
            var id = Guid.NewGuid();

            var setup = ModelSetup.For<TestSetupEntity>()
                .Update().PreOperation()
                .WithId(id)
                .WithTarget(e => e.Name = "smith")
                .WithFakeCrm(new TestSetupEntity { Id = id, Name = "smith" });

            ModelFor(setup).FlagDuplicateName();

            Assert.Single(setup.FakeCrm.CreateQuery<TestSetupEntity>());
        }

        private static TestSetupModel ModelFor(ModelSetup<TestSetupEntity> setup)
        {
            return new TestSetupModel(setup.Images, setup.Context, setup.Repository);
        }

        #endregion

        #region Impossible setups

        [Fact]
        public void A_Pre_Stage_Cannot_Be_Asynchronous()
        {
            var setup = ModelSetup.For<TestSetupEntity>().PreOperation().Asynchronous();

            var ex = Assert.Throws<InvalidOperationException>(() => setup.Context);
            Assert.Contains("cannot be asynchronous", ex.Message);
        }

        [Fact]
        public void A_Create_Cannot_Have_A_Pre_Image()
        {
            var setup = ModelSetup.For<TestSetupEntity>().Create().WithPreImage();

            var ex = Assert.Throws<InvalidOperationException>(() => setup.Context);
            Assert.Contains("no pre image", ex.Message);
        }

        [Fact]
        public void A_Pre_Stage_Cannot_Have_A_Post_Image()
        {
            var setup = ModelSetup.For<TestSetupEntity>().PreOperation().WithPostImage();

            var ex = Assert.Throws<InvalidOperationException>(() => setup.Context);
            Assert.Contains("no post image", ex.Message);
        }

        [Fact]
        public void A_Delete_Cannot_Have_An_Entity_Target()
        {
            var setup = ModelSetup.For<TestSetupEntity>().Delete().WithTarget();

            var ex = Assert.Throws<InvalidOperationException>(() => setup.Context);
            Assert.Contains("EntityReference as its Target", ex.Message);
        }

        [Fact]
        public void Chain_Order_Does_Not_Affect_What_Is_Legal()
        {
            var setup = ModelSetup.For<TestSetupEntity>().WithPostImage(e => e.Name = "jones").PostOperation();

            Assert.Equal("jones", setup.Images.PostImage.Name);
        }

        [Fact]
        public void The_Setup_Cannot_Be_Changed_After_It_Is_Built()
        {
            var setup = ModelSetup.For<TestSetupEntity>();
            var unused = setup.Context;

            var ex = Assert.Throws<InvalidOperationException>(() => setup.Depth(2));
            Assert.Contains("already been built", ex.Message);
        }

        [Fact]
        public void The_Built_Context_And_Images_Are_Stable()
        {
            var setup = ModelSetup.For<TestSetupEntity>().WithTarget(e => e.Name = "jones");

            Assert.Same(setup.Context, setup.Context);
            Assert.Same(setup.Images, setup.Images);
        }

        #endregion
    }

    #region Test model and entity

    public class TestSetupModel : ModelBase<TestSetupEntity>
    {
        public TestSetupModel(IImageManager<TestSetupEntity> images, IContextManager<TestSetupEntity> context,
            IRepository repository) : base(images, context, repository)
        {
        }

        public void FlagDuplicateName()
        {
            var name = Images.GetLatestString(TestSetupEntity.Fields.Name);
            if (string.IsNullOrWhiteSpace(name))
                return;

            var matches = Repository.RetrieveByAttribute(TestSetupEntity.EntityLogicalName,
                TestSetupEntity.Fields.Name, name);

            if (matches.All(m => m.Id == Context.PrimaryEntityId))
                return;

            Repository.Create(new TestSetupEntity { Name = name + " (duplicate)" });
        }
    }

    [EntityLogicalName(EntityLogicalName)]
    public class TestSetupEntity : Entity
    {
        public const string EntityLogicalName = "mgxrm_testsetupentity";

        public static class Fields
        {
            public const string Name = "mgxrm_name";
            public const string Reference = "mgxrm_reference";
        }

        public TestSetupEntity() : base(EntityLogicalName)
        {
        }

        [AttributeLogicalName(Fields.Name)]
        public string Name
        {
            get => GetAttributeValue<string>(Fields.Name);
            set => SetAttributeValue(Fields.Name, value);
        }

        [AttributeLogicalName(Fields.Reference)]
        public EntityReference Reference
        {
            get => GetAttributeValue<EntityReference>(Fields.Reference);
            set => SetAttributeValue(Fields.Reference, value);
        }
    }

    #endregion
}
