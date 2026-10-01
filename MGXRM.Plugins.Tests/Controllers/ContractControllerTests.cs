using FakeItEasy;
using MGXRM.Common.Framework.Model;
using MGXRM.Plugins.Controllers;
using Xunit;

namespace MGXRM.Plugins.Tests.Controllers
{
    public class ContractControllerTests
    {
        private IContactModel _fakeModel;
        private ContactController _controller;

        public ContractControllerTests()
        {
            _fakeModel = A.Fake<IContactModel>();
            _controller = new ContactController(_fakeModel);
        }

        [Fact]
        public void PreCreateTests()
        {
            A.CallTo(() => _fakeModel.MakeSurnameUppercase()).DoesNothing();
            A.CallTo(() => _fakeModel.EnforceEmailIsUnique()).DoesNothing();
            _controller.PreCreate();
            A.CallTo(() => _fakeModel.MakeSurnameUppercase()).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _fakeModel.EnforceEmailIsUnique()).MustHaveHappenedOnceExactly());
            
        }

        [Fact]
        public void PreUpdateTests()
        {
            A.CallTo(() => _fakeModel.MakeSurnameUppercase()).DoesNothing();
            A.CallTo(() => _fakeModel.EnforceEmailIsUnique()).DoesNothing();
            _controller.PreUpdate();
            A.CallTo(() => _fakeModel.MakeSurnameUppercase()).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _fakeModel.EnforceEmailIsUnique()).MustHaveHappenedOnceExactly());
        }
        
        [Fact]
        public void PostUpdateTests()
        {
            A.CallTo(() => _fakeModel.EnforceSurnameRequired()).DoesNothing();
            _controller.PostUpdateSync();
            A.CallTo(() => _fakeModel.EnforceSurnameRequired()).MustHaveHappenedOnceExactly();
        }
    }
}