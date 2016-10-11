using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Carbon.Framework.UnitOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Test.Common.Integration
{
    [TestClass]
    [DeploymentItem("ConnectionStrings.config")]
    [DeploymentItem("SharedAppSettings.config")]
    //[DeploymentItem(@"Files\ProjectImages.txt")]
    public class IntegrationTestBase
    {        
        private static readonly object _lock = new object();
        private DataSetup _dataSetup;

        public void SetupCleanTestModelSchema(Assembly modelAssembly)
        {
            DataSetup.SetupCleanTestModelSchema(modelAssembly);    
        }

        public void SetupCleanDomainModelSchema()
        {
            DataSetup.SetupCleanDomainModelSchema();    
        }
        
        public virtual IUnitOfWork CreateUnitOfWork()
        {
            return DataSetup.CreateUnitOfWork();
        }

        [TestInitialize]
        public virtual void Setup()
        {
            Monitor.Enter(_lock);
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            Monitor.Exit(_lock);
        }        

        public DataSetup DataSetup
        {
            get
            {
                if (_dataSetup == null)
                {
                    _dataSetup = new DataSetup();
                }
                return _dataSetup;
            }
        }
        public TestAppSettings TestAppSettings
        {
            get { return DataSetup.TestAppSettings; }
        }

        public async Task WaitFor(Func<bool> action, string failureMessage, TimeSpan? timeout = null)
        {
            timeout = timeout ?? TimeSpan.FromMinutes(2);
            var watch = new Stopwatch();
            watch.Start();
            bool done;
            bool elapsed;
            do
            {
                done = action();
                elapsed = watch.ElapsedMilliseconds >= timeout.Value.TotalMilliseconds;
                if (!done && !elapsed)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            } while (!done && !elapsed);

            if (!done)
            {
                Assert.Fail(failureMessage);
            }
        }
    }
}