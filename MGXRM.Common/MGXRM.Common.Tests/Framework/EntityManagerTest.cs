using MGXRM.Common.Framework;
using Microsoft.Xrm.Sdk;
using System;
using Xunit;
using FakeItEasy;
using MGXRM.Common.Framework.Interfaces;

namespace MGXRM.Common.Tests.Framework
{
    public class EntityManagerTest
    {
        protected const string EntityName = "mgxrm_Entity";
        protected const string FieldName = "mgxrm_field";

        [Fact]
        public void GetLatestImageVersion_Calls_IEntityAttributeVersion_GetLatestImageVersion()
        {
            var entityAttributeVersionFake = A.Fake<IEntityAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(A<string>._)).Returns("pre");

            var em = new EntityManager<Entity>(null, null, null, entityAttributeVersionFake);
            em.GetLatestImageVersion(FieldName);

            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(FieldName)).MustHaveHappened();
        }

        [Fact]
        public void GetLatestBool_Calls_IEntityAttributeVersion_GetLatestImageVersion_And_Casts()
        {
            var entityAttributeVersionFake = A.Fake<IEntityAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(A<string>._)).Returns(true);

            var em = new EntityManager<Entity>(null, null, null, entityAttributeVersionFake);
            var val = em.GetLatestBool(FieldName);

            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(FieldName)).MustHaveHappened();
            Assert.IsType<bool>(val);
        }

        [Fact]
        public void GetLatestBool_Can_Return_Null()
        {
            var em = new EntityManager<Entity>(null, null, null);
            var val = em.GetLatestBool(FieldName);
            Assert.Null(val);
        }

        [Fact]
        public void GetLatestDate_Calls_IEntityAttributeVersion_GetLatestImageVersion()
        {
            var entityAttributeVersionFake = A.Fake<IEntityAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(A<string>._)).Returns(DateTime.Now);

            var em = new EntityManager<Entity>(null, null, null, entityAttributeVersionFake);
            var val = em.GetLatestDate(FieldName);

            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(FieldName)).MustHaveHappened();
            Assert.IsType<DateTime>(val);
        }

        [Fact]
        public void GetLatestDate_Can_Return_Null()
        {
            var em = new EntityManager<Entity>(null, null, null);
            var val = em.GetLatestDate(FieldName);
            Assert.Null(val);
        }

        [Fact]
        public void GetLatestEntityReference_Calls_IEntityAttributeVersion_GetLatestImageVersion()
        {
            var entityAttributeVersionFake = A.Fake<IEntityAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(A<string>._)).Returns(null);

            var em = new EntityManager<Entity>(null, null, null, entityAttributeVersionFake);
            em.GetLatestEntityReference(FieldName);

            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion<EntityReference>(FieldName)).MustHaveHappened();
        }

        [Fact]
        public void GetLatestInt_Calls_IEntityAttributeVersion_GetLatestImageVersion()
        {
            var entityAttributeVersionFake = A.Fake<IEntityAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(A<string>._)).Returns(1);

            var em = new EntityManager<Entity>(null, null, null, entityAttributeVersionFake);
            em.GetLatestInt(FieldName);

            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(FieldName)).MustHaveHappened();
        }
        
        [Fact]
        public void GetLatestMoney_Calls_IEntityAttributeVersion_GetLatestImageVersion()
        {
            var entityAttributeVersionFake = A.Fake<IEntityAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(A<string>._)).Returns(new Money(10));

            var em = new EntityManager<Entity>(null, null, null, entityAttributeVersionFake);
            var val = em.GetLatestMoney(FieldName);

            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion<Money>(FieldName)).MustHaveHappened();
            Assert.IsType<Money>(val);
        }

        [Fact]
        public void GetLatestMoney_Can_Return_Null()
        {
            var em = new EntityManager<Entity>(null, null, null);
            var val = em.GetLatestMoney(FieldName);
            Assert.Null(val);
        }

        [Fact]
        public void GetLatestMoneyValue_Calls_IEntityAttributeVersion_GetLatestImageVersion()
        {
            var entityAttributeVersionFake = A.Fake<IEntityAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(A<string>._)).Returns(new Money(10));

            var em = new EntityManager<Entity>(null, null, null, entityAttributeVersionFake);
            var val = em.GetLatestMoneyValue(FieldName);

            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(FieldName)).MustHaveHappened();
            Assert.IsType<decimal>(val);
        }

        [Fact]
        public void GetLatestMoneValue_Can_Return_Null()
        {
            var em = new EntityManager<Entity>(null, null, null);
            var val = em.GetLatestMoneyValue(FieldName);
            Assert.Null(val);
        }

        [Fact]
        public void GetLatestOptionSet_Calls_IEntityAttributeVersion_GetLatestImageVersion()
        {
            var entityAttributeVersionFake = A.Fake<IEntityAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(A<string>._)).Returns(null);

            var em = new EntityManager<Entity>(null, null, null, entityAttributeVersionFake);
            em.GetLatestOptionSet(FieldName);

            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion<OptionSetValue>(FieldName)).MustHaveHappened();
        }

        [Fact]
        public void GetLatestString_Calls_IEntityAttributeVersion_GetLatestImageVersion()
        {
            var entityAttributeVersionFake = A.Fake<IEntityAttributeVersion>();
            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion(A<string>._)).Returns(null);

            var em = new EntityManager<Entity>(null, null, null, entityAttributeVersionFake);
            em.GetLatestString(FieldName);

            A.CallTo(() => entityAttributeVersionFake.GetLatestImageVersion<string>(FieldName)).MustHaveHappened();
        }

    }
}
