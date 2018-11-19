using FakeItEasy;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Model;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace MGXRM.Common.Tests.Framework.Model
{
    public class ModelBaseTest
    {
        [Fact]
        public void Context_Images_Repository_Set_In_Constructor()
        {
            var images = A.Fake<IImageManager<TestEntityClass>>();
            var context = A.Fake<IContextManager>();
            var repo = A.Fake<IRepository>();
            var model = new TestModelBaseClass(images,context,repo);
            Assert.Same(images, model.GetImageManager());
            Assert.Same(context, model.GetContextManager());
            Assert.Same(repo, model.GetRepository());
        }
    }

    #region Test classes
    public class TestModelBaseClass : ModelBase<TestEntityClass>
    {
        public TestModelBaseClass(IImageManager<TestEntityClass> images, IContextManager context, IRepository repository) :
            base(images, context, repository)
        {
        }

        public IImageManager<TestEntityClass> GetImageManager()
        {
            return Images;
        }

        public IContextManager GetContextManager()
        {
            return Context;
        }

        public IRepository GetRepository()
        {
            return Repository;
        }
    }

    public class TestEntityClass : Entity
    {
    }
    #endregion
}
