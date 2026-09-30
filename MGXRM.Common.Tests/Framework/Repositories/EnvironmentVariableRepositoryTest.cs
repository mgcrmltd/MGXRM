using System;
using System.Collections.Generic;
using FakeItEasy;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Repositories;
using MGXRM.Common.Tests.TestCore;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace MGXRM.Common.Tests.Framework.Repositories
{
    public class EnvironmentVariableRepositoryTest
    {
        private const string SchemaName = "mgxrm_ApiUrl";

        #region The query, against seeded records

        [Fact]
        public void The_Current_Value_Wins_Over_The_Default()
        {
            var definitionId = Guid.NewGuid();
            var repository = SeededWith(
                Definition(definitionId, SchemaName, "https://default.example.com"),
                CurrentValue(definitionId, "https://current.example.com"));

            Assert.Equal("https://current.example.com", repository.GetString(SchemaName));
        }

        [Fact]
        public void The_Default_Is_Used_When_This_Environment_Has_Not_Overridden_It()
        {
            var definitionId = Guid.NewGuid();
            var repository = SeededWith(Definition(definitionId, SchemaName, "https://default.example.com"));

            Assert.Equal("https://default.example.com", repository.GetString(SchemaName));
        }

        [Fact]
        public void An_Empty_Current_Value_Falls_Back_To_The_Default()
        {
            var definitionId = Guid.NewGuid();
            var repository = SeededWith(
                Definition(definitionId, SchemaName, "https://default.example.com"),
                CurrentValue(definitionId, string.Empty));

            Assert.Equal("https://default.example.com", repository.GetString(SchemaName));
        }

        [Fact]
        public void Null_Comes_Back_When_There_Is_Neither_A_Value_Nor_A_Default()
        {
            var definitionId = Guid.NewGuid();
            var repository = SeededWith(Definition(definitionId, SchemaName, null));

            Assert.Null(repository.GetString(SchemaName));
        }

        [Fact]
        public void Only_The_Requested_Variable_Is_Read()
        {
            var wanted = Guid.NewGuid();
            var other = Guid.NewGuid();
            var repository = SeededWith(
                Definition(wanted, SchemaName, "wanted"),
                Definition(other, "mgxrm_Other", "other"),
                CurrentValue(other, "other current"));

            Assert.Equal("wanted", repository.GetString(SchemaName));
        }

        [Fact]
        public void A_Missing_Definition_Throws()
        {
            var repository = SeededWith(Definition(Guid.NewGuid(), SchemaName, "something"));

            var ex = Assert.Throws<InvalidPluginExecutionException>(() => repository.GetString("mgxrm_Missing"));
            Assert.Contains("mgxrm_Missing", ex.Message);
        }

        #endregion

        #region Typed values

        [Theory]
        [InlineData("42", 42)]
        [InlineData("-7", -7)]
        [InlineData(" 42 ", 42)]
        public void Whole_Numbers_Are_Parsed(string stored, int expected)
        {
            Assert.Equal(expected, Holding(stored).GetWholeNumber(SchemaName));
        }

        [Theory]
        [InlineData("10.5", 10.5)]
        [InlineData("-0.25", -0.25)]
        public void Decimals_Are_Parsed(string stored, double expected)
        {
            Assert.Equal((decimal)expected, Holding(stored).GetDecimal(SchemaName));
        }

        [Theory]
        [InlineData("yes", true)]
        [InlineData("no", false)]
        [InlineData("Yes", true)]
        [InlineData("NO", false)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("1", true)]
        [InlineData("0", false)]
        public void Booleans_Accept_The_Forms_Dataverse_And_People_Store(string stored, bool expected)
        {
            Assert.Equal(expected, Holding(stored).GetBoolean(SchemaName));
        }

        [Fact]
        public void Guids_Are_Parsed()
        {
            var id = Guid.NewGuid();

            Assert.Equal(id, Holding(id.ToString()).GetGuid(SchemaName));
        }

        [Fact]
        public void Guids_Are_Parsed_In_Braced_Form()
        {
            var id = Guid.NewGuid();

            Assert.Equal(id, Holding(id.ToString("B")).GetGuid(SchemaName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void An_Unset_Variable_Is_Null_For_Every_Type(string stored)
        {
            var repository = Holding(stored);

            Assert.Null(repository.GetString(SchemaName));
            Assert.Null(repository.GetWholeNumber(SchemaName));
            Assert.Null(repository.GetDecimal(SchemaName));
            Assert.Null(repository.GetBoolean(SchemaName));
            Assert.Null(repository.GetGuid(SchemaName));
        }

        #endregion

        #region Unusable values

        [Fact]
        public void A_Value_That_Is_Not_A_Whole_Number_Throws()
        {
            var ex = Assert.Throws<InvalidPluginExecutionException>(() => Holding("ten").GetWholeNumber(SchemaName));
            Assert.Contains("ten", ex.Message);
            Assert.Contains(SchemaName, ex.Message);
        }

        [Fact]
        public void A_Decimal_Is_Not_A_Whole_Number()
        {
            Assert.Throws<InvalidPluginExecutionException>(() => Holding("10.5").GetWholeNumber(SchemaName));
        }

        [Fact]
        public void A_Value_That_Is_Not_A_Number_Throws()
        {
            Assert.Throws<InvalidPluginExecutionException>(() => Holding("lots").GetDecimal(SchemaName));
        }

        [Fact]
        public void A_Value_That_Is_Not_Yes_Or_No_Throws()
        {
            Assert.Throws<InvalidPluginExecutionException>(() => Holding("maybe").GetBoolean(SchemaName));
        }

        [Fact]
        public void A_Value_That_Is_Not_A_Guid_Throws()
        {
            Assert.Throws<InvalidPluginExecutionException>(() => Holding("not-a-guid").GetGuid(SchemaName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void A_Missing_Schema_Name_Throws(string schemaName)
        {
            Assert.Throws<ArgumentException>(() => Holding("x").GetString(schemaName));
        }

        #endregion

        #region Caching

        [Fact]
        public void A_Variable_Is_Only_Queried_Once()
        {
            var inner = FakeRepositoryHolding("42");
            var repository = new EnvironmentVariableRepository(inner);

            repository.GetWholeNumber(SchemaName);
            repository.GetWholeNumber(SchemaName);
            repository.GetString(SchemaName);

            A.CallTo(() => inner.RetrieveMultiple(A<QueryBase>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Schema_Names_Are_Cached_Case_Insensitively()
        {
            var inner = FakeRepositoryHolding("42");
            var repository = new EnvironmentVariableRepository(inner);

            repository.GetString(SchemaName);
            repository.GetString(SchemaName.ToUpperInvariant());

            A.CallTo(() => inner.RetrieveMultiple(A<QueryBase>._)).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region Helpers

        private static IEnvironmentVariableRepository SeededWith(params Entity[] records)
        {
            var setup = ModelSetup.For<EnvironmentVariableDefinition>().WithFakeCrm(records);
            return new EnvironmentVariableRepository(setup.Repository);
        }

        private static IEnvironmentVariableRepository Holding(string value)
        {
            return new EnvironmentVariableRepository(FakeRepositoryHolding(value));
        }

        private static IRepository FakeRepositoryHolding(string value)
        {
            var found = new EnvironmentVariableDefinition(Guid.NewGuid())
            {
                SchemaName = SchemaName,
                DefaultValue = value
            };

            var repository = A.Fake<IRepository>();
            A.CallTo(() => repository.RetrieveMultiple(A<QueryBase>._)).Returns(new List<Entity> { found });
            return repository;
        }

        private static EnvironmentVariableDefinition Definition(Guid id, string schemaName, string defaultValue)
        {
            return new EnvironmentVariableDefinition(id)
            {
                SchemaName = schemaName,
                DefaultValue = defaultValue
            };
        }

        private static EnvironmentVariableValue CurrentValue(Guid definitionId, string value)
        {
            return new EnvironmentVariableValue(Guid.NewGuid())
            {
                EnvironmentVariableDefinitionId =
                    new EntityReference(EnvironmentVariableDefinition.EntityLogicalName, definitionId),
                Value = value
            };
        }

        #endregion
    }
}
