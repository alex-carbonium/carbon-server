using Carbon.Framework.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace Carbon.Test.Unit.Framework
{
    [TestClass]
    public class DependencyContainerTests : BaseTest
    {
        public class Dependency
        {
        }
        public class Consumer
        {
            private readonly Dependency _dependency;

            public Consumer(Dependency dependency)
            {
                _dependency = dependency;
            }

            public Dependency Dependency
            {
                get { return _dependency; }
            }
        }
        public interface IDependencyFactory
        {
            Dependency CreateDependency();
        }

        public class FactoryConsumer
        {
            private readonly IDependencyFactory _factory;

            public FactoryConsumer(IDependencyFactory factory)
            {
                _factory = factory;
            }

            public Dependency Consume()
            {
                return _factory.CreateDependency();
            }
        }

        [TestMethod]
        public void CloneMustCopyBindings()
        {
            //arrange
            var dependency = new Dependency();
            var container = new NinjectDependencyContainer(new StandardKernel());
            container.RegisterInstance(dependency);            

            //act
            var clone = container.BeginScope();           
            var consumer = clone.Resolve<Consumer>();

            //assert
            Assert.AreSame(dependency, consumer.Dependency, "Bindings not cloned");
        }        

        [TestMethod]
        public void MustBeAbleToOverrideBindings()
        {
            //arrange
            var container = new NinjectDependencyContainer(new StandardKernel());            
            var dependency1 = new Dependency();
            var dependency2 = new Dependency();
            container.RegisterInstance(dependency1);            
            container.RegisterInstance(dependency2);

            //act
            var consumer = container.Resolve<Consumer>();

            //assert
            Assert.AreSame(dependency2, consumer.Dependency, "Binding not overriden");
        }

        [TestMethod]
        public void MustBeAbleToOverrideBindingsAndClone()
        {
            //arrange
            var container = new NinjectDependencyContainer(new StandardKernel());
            var dependency1 = new Dependency();
            var dependency2 = new Dependency();
            container.RegisterInstance(dependency1);
            container.RegisterInstance(dependency2);

            //act
            var clone = container.BeginScope();
            var consumer = clone.Resolve<Consumer>();

            //assert
            Assert.AreSame(dependency2, consumer.Dependency, "Binding not overriden");
        }

        [TestMethod]
        public void BingingTypesMustBePropagated()
        {
            //arrange
            var container = new NinjectDependencyContainer(new StandardKernel());
            container.RegisterTypeSingleton<Dependency, Dependency>();

            //act
            var clone = container.BeginScope();
            var consumer1 = clone.Resolve<Consumer>();
            var consumer2 = clone.Resolve<Consumer>();

            //assert
            Assert.AreSame(consumer1.Dependency, consumer2.Dependency, "Binding types not propagated");
        }

        [TestMethod]
        public void ScopingShouldWork()
        {
            //arrange
            var container = new NinjectDependencyContainer(new StandardKernel());
            container.RegisterTypePerWebRequest<Dependency, Dependency>();

            //act
            var scope = container.BeginScope();
            var scopeInstance1 = scope.Resolve<Dependency>();
            var scopeInstance2 = scope.Resolve<Dependency>();            

            //assert            
            Assert.AreSame(scopeInstance1, scopeInstance2, "Scope should work");            
        }

        [TestMethod]
        public void FactoriesShouldUseCurrentScope()
        {
            //arrange
            var container = new NinjectDependencyContainer(new StandardKernel());
            container.RegisterTypePerWebRequest<Dependency, Dependency>();
            container.RegisterFactory<IDependencyFactory>();

            //act
            var scope1 = container.BeginScope();
            var consumer1 = scope1.Resolve<FactoryConsumer>();
            var scope1Dependency1 = consumer1.Consume();
            var scope1Dependency2 = consumer1.Consume();

            var scope2 = container.BeginScope();
            var consumer2 = scope2.Resolve<FactoryConsumer>();            
            var scope2Dependency2 = consumer2.Consume();

            //asser
            Assert.AreSame(scope1Dependency1, scope1Dependency2, "Scope not working");                                              
            Assert.AreNotSame(scope1Dependency1, scope2Dependency2, "Scope transition not working");                        
        }

        [TestMethod]
        public void FactoriesShouldDefineScopes()
        {
            //arrange
            var container = new NinjectDependencyContainer(new StandardKernel());
            container.RegisterTypePerWebRequest<Dependency, Dependency>();
            container.RegisterFactory<IDependencyFactory>();

            //act
            var consumer1 = container.Resolve<FactoryConsumer>();
            var scope1Dependency1 = consumer1.Consume();
            var scope1Dependency2 = consumer1.Consume();

            //assert
            Assert.AreSame(scope1Dependency1, scope1Dependency2, "Scope not defined");            
        }
    }
}