using Microsoft.Xrm.Sdk;
using Xunit;
using MGXRM.Common.Framework.Extensions;
using System;
using System.Collections.Generic;

namespace MGXRM.Common.Tests.Framework.Extensions
{
    public class EntityAttributeExtensionsTest
    {
        [Theory]
        [InlineData(17, 17, true)]
        [InlineData(17, 18, false)]
        [InlineData(-17, -17, true)]
        [InlineData(-17, -18, false)]
        [InlineData(0, 0, true)]
        public void SameAs_Int(int val1, int val2, bool expectedResult)
        {
            var result = val1.SameAs(val2);
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [InlineData("aa", "aa", true)]
        [InlineData("aa", "ab", false)]
        [InlineData("aa", "", false)]
        [InlineData("aa", null, false)]
        public void SameAs_Bool(string val1, string val2, bool expectedResult)
        {
            var result = val1.SameAs(val2);
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [InlineData(17.5, 17.5, true)]
        [InlineData(17.5, 18.5, false)]
        [InlineData(-17.5, -17.5, true)]
        [InlineData(-17.3, -17.5, false)]
        [InlineData(0.0, 0.0, true)]
        public void SameAs_Decimal(decimal val1, decimal val2, bool expectedResult)
        {
            var result = val1.SameAs(val2);
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [InlineData("aa", "aa", true)]
        [InlineData("aa", "ab", false)]
        [InlineData("aa", "", false)]
        [InlineData("aa", null, false)]
        public void SameAs_String(string val1, string val2, bool expectedResult)
        {
            var result = val1.SameAs(val2);
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [MemberData(nameof(GuidData))]
        public void SameAs_Guid(Guid val1, Guid val2, bool expectedResult)
        {
            var result = val1.SameAs(val2);
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [MemberData(nameof(MoneyData))]
        public void SameAs_Money(Money val1, Money val2, bool expectedResult)
        {
            var result = val1.SameAs(val2);
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [MemberData(nameof(EntityReferenceData))]
        public void SameAs_EntityReference(EntityReference val1, EntityReference val2, bool expectedResult)
        {
            var result = val1.SameAs(val2);
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [MemberData(nameof(OptionSetData))]
        public void SameAs_OptonSet(OptionSetValue val1, OptionSetValue val2, bool expectedResult)
        {
            var result = val1.SameAs(val2);
            Assert.Equal(result, expectedResult);
        }

        #region TestData
        public static IEnumerable<object[]> GuidData =>
        new List<object[]>
        {
            new object[] { new Guid("6313273f-6333-4084-ab66-39b17b80c22b"), new Guid("6313273f-6333-4084-ab66-39b17b80c22b"), true },
            new object[] { Guid.NewGuid(), Guid.NewGuid(), false },
        };

        public static IEnumerable<object[]> MoneyData =>
        new List<object[]>
        {
            new object[] { new Money((decimal)101.89), new Money((decimal)101.89), true },
            new object[] { new Money((decimal)101.89), new Money((decimal)888.54), false },
            new object[] { new Money((decimal)101.89), null, false },
        };

        public static IEnumerable<object[]> EntityReferenceData =>
        new List<object[]>
        {
            new object[] { new EntityReference("account", new Guid("6313273f-6333-4084-ab66-39b17b80c22b")), new EntityReference("account", new Guid("6313273f-6333-4084-ab66-39b17b80c22b")), true },
            new object[] { new EntityReference("account", new Guid("11111111-6333-4084-ab66-39b17b80c22b")), new EntityReference("account", new Guid("22222222-6333-4084-ab66-39b17b80c22b")), false },
            new object[] { new EntityReference("account", new Guid("6313273f-6333-4084-ab66-39b17b80c22b")), new EntityReference("contact", new Guid("6313273f-6333-4084-ab66-39b17b80c22b")), false },
            new object[] { new EntityReference("account", new Guid("6313273f-6333-4084-ab66-39b17b80c22b")), null, false },
        };

        public static IEnumerable<object[]> OptionSetData =>
        new List<object[]>
        {
            new object[] { new OptionSetValue(1234), new OptionSetValue(1234), true },
            new object[] { new OptionSetValue(1234), new OptionSetValue(5678), false },
            new object[] { new OptionSetValue(1234), null, false },
        };
        #endregion
    }
}
