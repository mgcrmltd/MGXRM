using Microsoft.Xrm.Sdk;
using Xunit;
using MGXRM.Common.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Collections;

namespace MGXRM.Common.Tests.Framework.Extensions
{
    public class EntityEntensionsTest
    {
        public static string EntityName = "mgxrm_Entity";
        public static string FieldName = "mgxrm_description";

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
        public void SameAs_Returns_False_If_Atributes_Not_Same()
        {
            var id = Guid.NewGuid();
            var originalEntity = new Entity() { Id = id, LogicalName = EntityName };
            originalEntity.Attributes.Add(FieldName, "one");
            Entity otherEntity = new Entity() { Id = id, LogicalName = EntityName };
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
        [ClassData(typeof(EntensionsImposeTestData))]
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

        [Theory]
        [ClassData(typeof(ExtensionsSameAsTestData))]
        public void SameAs_Works_For_Dynamics_Attribute_Types(object a, object b, bool same)
        {
            Assert.Equal(same, EntityExtensions.AttributeSameAs(a, b));
        }
    }

    public class ExtensionsSameAsTestData : IEnumerable<object[]>
    {
        Guid _guid1;
        Guid _guid2;

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
            yield return new object[] { new EntityReference() { Id = _guid1, LogicalName = EntityEntensionsTest.EntityName }, new EntityReference() { Id = _guid1, LogicalName = EntityEntensionsTest.EntityName }, true };
            yield return new object[] { new EntityReference() { Id = _guid1, LogicalName = EntityEntensionsTest.EntityName }, new EntityReference() { Id = _guid2, LogicalName = EntityEntensionsTest.EntityName }, false };
            yield return new object[] { new EntityReference() { Id = _guid1, LogicalName = EntityEntensionsTest.EntityName }, new EntityReference() { Id = _guid1, LogicalName = "mgxrm_OtherEntity" }, false };
            yield return new object[] { new OptionSetValue(187187187), new OptionSetValue(187187187), true };
            yield return new object[] { new OptionSetValue(187187187), new OptionSetValue(999999999), false };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class EntensionsImposeTestData : IEnumerable<object[]>
    {
        public readonly Entity OriginalImage;
        public readonly Entity ImageToImpose;

        public EntensionsImposeTestData()
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
