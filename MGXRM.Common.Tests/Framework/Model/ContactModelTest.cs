using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Model;
using MGXRM.Common.Tests.TestCore;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace MGXRM.Common.Tests.Framework.Model
{
    public class ContactModelTest
    {
        private static ContactModel ModelFor(ModelSetup<Contact> setup)
        {
            return new ContactModel(setup.Images, setup.Context, setup.Repository);
        }

        #region MakeSurnameUppercase

        [Fact]
        public void MakeSurnameUppercase_Uppercases_A_Surname_Being_Set_On_Create()
        {
            var setup = ModelSetup.For<Contact>()
                .Create().PreOperation().Synchronous()
                .WithTarget(c => c.LastName = "jones");

            ModelFor(setup).MakeSurnameUppercase();

            Assert.Equal("JONES", setup.Images.TargetImage.LastName);
        }

        [Fact]
        public void MakeSurnameUppercase_Uppercases_A_Surname_Being_Changed_On_Update()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation().Synchronous().Depth(1)
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.LastName = "jones");

            ModelFor(setup).MakeSurnameUppercase();

            Assert.Equal("JONES", setup.Images.TargetImage.LastName);
        }

        /// <summary>
        /// The surname is not in the target, so the step fired for some other attribute and the model must
        /// leave the record alone rather than writing the pre image value back.
        /// </summary>
        [Fact]
        public void MakeSurnameUppercase_Does_Nothing_When_The_Surname_Is_Not_Being_Set()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation().Synchronous()
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.FirstName = "bob");

            ModelFor(setup).MakeSurnameUppercase();

            Assert.False(setup.Images.TargetImage.Contains(Contact.Fields.LastName));
        }

        [Fact]
        public void MakeSurnameUppercase_Does_Nothing_When_The_Surname_Is_Being_Cleared()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation().Synchronous()
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.LastName = null);

            ModelFor(setup).MakeSurnameUppercase();

            Assert.Null(setup.Images.TargetImage.LastName);
        }

        [Fact]
        public void MakeSurnameUppercase_Leaves_An_Already_Uppercase_Surname_Alone()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation().Synchronous()
                .WithTarget(c => c.LastName = "JONES");

            ModelFor(setup).MakeSurnameUppercase();

            Assert.Equal("JONES", setup.Images.TargetImage.LastName);
        }

        #endregion

        #region EnforceSurnameRequired

        [Fact]
        public void EnforceSurnameRequired_Throws_When_The_Surname_Is_Being_Cleared()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PostOperation().Synchronous()
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.LastName = null);

            Assert.Throws<InvalidPluginExecutionException>(() => ModelFor(setup).EnforceSurnameRequired());
        }

        [Fact]
        public void EnforceSurnameRequired_Allows_A_Surname_Being_Changed()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PostOperation().Synchronous()
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.LastName = "jones");

            ModelFor(setup).EnforceSurnameRequired();
        }

        [Fact]
        public void EnforceSurnameRequired_Allows_An_Update_That_Does_Not_Touch_The_Surname()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PostOperation().Synchronous()
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.FirstName = "bob");

            ModelFor(setup).EnforceSurnameRequired();
        }

        #endregion
    }
}
