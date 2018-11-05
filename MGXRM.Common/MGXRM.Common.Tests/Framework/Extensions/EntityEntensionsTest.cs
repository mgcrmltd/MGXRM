using Microsoft.Xrm.Sdk;
using Xunit;
using MGXRM.Common.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Collections;

namespace MGXRM.Common.Tests.Framework.Extensions
{
    public class EntityExtensionsTest
    {
        public static string EntityName = "mgxrm_Entity";
        public static string FieldName = "mgxrm_description";

        protected Entity FullEntity;
        protected Entity NullEntity;
        public EntityExtensionsTest()
        {
            FullEntity = new Entity(EntityName)
            {
                ["bool"] = true,
                ["dateTime"] = DateTime.Now,
                ["entityReference"] = new EntityReference(EntityName, Guid.NewGuid()),
                ["int"] = 1,
                ["money"] = new Money((decimal) 10.5),
                ["string"] = "Hello World",
                ["optionSetValue"] = new OptionSetValue(1)
            };

            NullEntity = new Entity(EntityName)
            {
                ["bool"] = null,
                ["dateTime"] = null,
                ["entityReference"] = null,
                ["int"] = null,
                ["money"] = null,
                ["string"] = null,
                ["optionSetValue"] = null
            };
        }

        [Fact]
        public void SameAs_Returns_False_If_OtherEntity_Null()
        {
            var originalEntity = new Entity();
            Entity otherEntity = null;
            Assert.False(originalEntity.SameAs(otherEntity));
        }

        [Fact]
        public void SameAs_Returns_False_If_includeIds_True_And_Ids_Not_Same()
        {
            var originalEntity = new Entity() { Id = Guid.NewGuid() };
            Entity otherEntity = new Entity() { Id = Guid.NewGuid() };
            Assert.False(originalEntity.SameAs(otherEntity));
        }

        [Fact]
        public void SameAs_Returns_False_If_EntityLogicalName_Not_Same()
        {
            var id = Guid.NewGuid();
            var originalEntity = new Entity() { Id = id, LogicalName = EntityName };
            Entity otherEntity = new Entity() { Id = id, LogicalName = "mgxrm_OtherEntity" };
            Assert.False(originalEntity.SameAs(otherEntity));
        }

        [Fact]
        public void SameAs_Returns_False_If_Attributes_Not_Same()
        {
            var id = Guid.NewGuid();
            var originalEntity = new Entity() {Id = id, LogicalName = EntityName};
            originalEntity.Attributes.Add(FieldName, "one");
            var otherEntity = new Entity() {Id = id, LogicalName = EntityName};
            otherEntity.Attributes.Add(FieldName, "two");
            Assert.False(originalEntity.SameAs(otherEntity));
        }

        [Theory]
        [InlineData(null, null, true)]
        [InlineData(null, null, false)]
        public void ImposeEntity_Returns_Null_If_Both_Images_Null(Entity original, Entity toImpose, bool imposeNullValues)
        {
            var returnValue = original.ImposeEntity(toImpose, imposeNullValues);
            Assert.Null(returnValue);
        }

        [Theory]
        [ClassData(typeof(ExtensionsImposeTestData))]
        public void ImposeEntity_Never_Returns_Reference_To_Input_Entities(Entity original, Entity toImpose, bool imposeNullValues)
        {
            var returnValue = original.ImposeEntity(toImpose, imposeNullValues);
            Assert.NotSame(returnValue, original);
            Assert.NotSame(returnValue, toImpose);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ImposeEntity_Returns_Copy_Of_Original_If_ToImpose_Null(bool imposeNullValues)
        {
            var originalEntity = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            originalEntity.Attributes.Add(FieldName, "one");
            var imposed = originalEntity.ImposeEntity(null, imposeNullValues);
            Assert.True(EntityExtensions.SameAs(imposed, originalEntity));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ImposeEntity_Returns_Copy_Of_ToImpose_If_Original_Null(bool imposeNullValues)
        {
            Entity originalEntity = null;
            var toImpose = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            toImpose.Attributes.Add(FieldName, "one");
            var imposed = originalEntity.ImposeEntity(toImpose, imposeNullValues);
            Assert.True(EntityExtensions.SameAs(imposed, toImpose));
        }

        [Fact]
        public void ImposeEntity_Imposes_Nulls_If_Specified()
        {
            var originalEntity = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            originalEntity.Attributes.Add(FieldName, "one");
            var imposeEntity = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            imposeEntity.Attributes.Add(FieldName, null);
            var imposed = originalEntity.ImposeEntity(imposeEntity);
            Assert.Null(imposed[FieldName]);
        }

        [Fact]
        public void ImposeEntity_Does_Not_Impose_Nulls_If_Specified()
        {
            var originalEntity = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            originalEntity.Attributes.Add(FieldName, "one");
            var imposeEntity = new Entity() { Id = Guid.NewGuid(), LogicalName = EntityName };
            imposeEntity.Attributes.Add(FieldName, null);
            var imposed = originalEntity.ImposeEntity(imposeEntity, false);
            Assert.NotNull(imposed[FieldName]);
        }

        [Theory]
        [ClassData(typeof(ExtensionsSameAsTestData))]
        public void SameAs_Works_For_Dynamics_Attribute_Types(object a, object b, bool same)
        {
            Assert.Equal(same, EntityExtensions.AttributeSameAs(a, b));
        }

        [Fact]
        public void HasNonNullValue_Returns_False_If_Entity_Null()
        {
            Entity entity = null;
            Assert.False(entity.HasNonNullValue("mgxrm_randon"));
        }

        [Fact]
        public void HasNonNullValue_Returns_False_If_Not_Found()
        {
            Entity entity = null;
            Assert.False(entity.HasNonNullValue(FieldName));
        }

        [Fact]
        public void RemoveAttribute_Returns_False_If_Entity_Null()
        {
            Entity entity = null;
            Assert.False(entity.RemoveAttribute(FieldName));
        }

        [Fact]
        public void RemoveAttribute_Returns_False_If_Attribute_Not_Found()
        {
            var entity = new Entity(EntityName);
            Assert.False(entity.Contains(FieldName));
            Assert.False(entity.RemoveAttribute(FieldName));
        }

        [Fact]
        public void RemoveAttribute_Returns_True_If_Attribute_Removed()
        {
            var entity = new Entity(EntityName);
            entity.Attributes.Add(FieldName,true);
            Assert.True(entity.Contains(FieldName));
            Assert.True(entity.RemoveAttribute(FieldName));
            Assert.False(entity.Contains(FieldName));
        }

        #region Get attribute tests
        [Fact]
        public void GetBool_Returns_And_Casts_And_Handles_Null()
        {
           Assert.IsType<bool>(FullEntity.GetBool("bool"));
            Assert.Null(NullEntity.GetBool("bool"));
        }

        [Fact]
        public void GetDate_Returns_And_Casts_And_Handles_Null()
        {
            Assert.IsType<DateTime>(FullEntity.GetDateTime("dateTime"));
            Assert.Null(NullEntity.GetDateTime("dateTime"));
        }

        [Fact]
        public void GetEntityReference_Returns_And_Casts_And_Handles_Null()
        {
            Assert.IsType<EntityReference>(FullEntity.GetEntityReference("entityReference"));
            Assert.Null(NullEntity.GetEntityReference("entityReference"));
        }

        [Fact]
        public void GetInt_Returns_And_Casts_And_Handles_Null()
        {
            Assert.IsType<int>(FullEntity.GetInt("int"));
            Assert.Null(NullEntity.GetInt("int"));
        }

        [Fact]
        public void GetMoney_Returns_And_Casts_And_Handles_Null()
        {
            Assert.IsType<Money>(FullEntity.GetMoney("money"));
            Assert.Null(NullEntity.GetMoney("money"));
        }

        [Fact]
        public void GetMoneyValue_Returns_And_Casts_And_Handles_Null()
        {
            Assert.IsType<decimal>(FullEntity.GetMoneyValue("money"));
            Assert.Null(NullEntity.GetMoneyValue("money"));
        }

        [Fact]
        public void GetOptionSet_Returns_And_Casts_And_Handles_Null()
        {
            Assert.IsType<OptionSetValue>(FullEntity.GetOptionSet("optionSetValue"));
            Assert.Null(NullEntity.GetOptionSet("optionSetValue"));
        }
        
        [Fact]
        public void GetString_Returns_And_Casts_And_Handles_Null()
        {
            Assert.IsType<string>(FullEntity.GetString("string"));
            Assert.Null(NullEntity.GetString("string"));
        }
        #endregion
    }


    public class ExtensionsSameAsTestData : IEnumerable<object[]>
    {
        readonly Guid _guid1;
        readonly Guid _guid2;

        public ExtensionsSameAsTestData()
        {
            _guid1 = Guid.NewGuid();
            _guid2 = Guid.NewGuid();
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { 1, 1, true };
            yield return new object[] { 1, 2, false };
            yield return new object[] { 1, -1, false };
            yield return new object[] { (decimal)1.5, (decimal)1.5, true };
            yield return new object[] { (decimal)1.5, (decimal)2.5, false };
            yield return new object[] { (double)1.5, (double)1.5, true };
            yield return new object[] { (double)1.5, (double)2.5, false };
            yield return new object[] { (float)1.5, (float)1.5, true };
            yield return new object[] { (float)1.5, (float)2.5, false };
            yield return new object[] { (float)1.5, (float)1.5, true };
            yield return new object[] { (float)1.5, (float)2.5, false };
            yield return new object[] { _guid1, _guid1, true };
            yield return new object[] { _guid1, _guid2, false };
            yield return new object[] { null, null, true };
            yield return new object[] { 1, null, false };
            yield return new object[] { null, 1, false };
            yield return new object[] { new Money((decimal)10.50), new Money((decimal)10.50), true };
            yield return new object[] { new Money((decimal)10.50), new Money((decimal)3.75), false };
            yield return new object[] { new EntityReference() { Id = _guid1, LogicalName = EntityExtensionsTest.EntityName }, new EntityReference() { Id = _guid1, LogicalName = EntityExtensionsTest.EntityName }, true };
            yield return new object[] { new EntityReference() { Id = _guid1, LogicalName = EntityExtensionsTest.EntityName }, new EntityReference() { Id = _guid2, LogicalName = EntityExtensionsTest.EntityName }, false };
            yield return new object[] { new EntityReference() { Id = _guid1, LogicalName = EntityExtensionsTest.EntityName }, new EntityReference() { Id = _guid1, LogicalName = "mgxrm_OtherEntity" }, false };
            yield return new object[] { new OptionSetValue(187187187), new OptionSetValue(187187187), true };
            yield return new object[] { new OptionSetValue(187187187), new OptionSetValue(999999999), false };
            yield return new object[] { new DateTime(2018,01,01), new DateTime(2018, 01, 01), true };
            yield return new object[] { new DateTime(2018, 01, 01), new DateTime(2017, 02, 02), false };
            yield return new object[] { true, true, true };
            yield return new object[] { false,false, true};
            yield return new object[] { true, false, false };
            yield return new object[] { false, true, false };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class ExtensionsImposeTestData : IEnumerable<object[]>
    {
        public readonly Entity OriginalImage;
        public readonly Entity ImageToImpose;

        public ExtensionsImposeTestData()
        {
            OriginalImage = new Entity("xname");
            OriginalImage.Attributes.Add("xvalue1", "original");

            ImageToImpose = new Entity("xname");
            ImageToImpose.Attributes.Add("xvalue1", "impose");
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { OriginalImage, ImageToImpose, true };
            yield return new object[] { OriginalImage, ImageToImpose, false };
            yield return new object[] { null, ImageToImpose, true };
            yield return new object[] { null, ImageToImpose, false };
            yield return new object[] { OriginalImage, null, true };
            yield return new object[] { OriginalImage, null, false };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
