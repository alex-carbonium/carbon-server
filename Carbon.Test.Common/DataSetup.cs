using System;
using System.Collections.Generic;
using System.Reflection;
using Carbon.Data;
using Carbon.Framework.UnitOfWork;
using Carbon.Framework.Util;

namespace Carbon.Test.Common
{
    public class DataSetup
    {
        //private IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly DataUtil _dataUtil;

        public DataSetup()
        {
            //TestAppSettings = new TestAppSettings();
            _dataUtil = new DataUtil(TestAppSettings);
        }

        public void SetupCleanDomainModelSchema()
        {
//            NHibernateConfig.ModelAssembly = null;
//            SetupSchema();
        }

        public void SetupCleanTestModelSchema(Assembly assembly)
        {
//            NHibernateConfig.ModelAssembly = assembly;
//            SetupSchema();
        }

        public void SetupSchema()
        {
//            DependencyContainer = DependencyConfiguration.Configure();
//            DataLayerConfig.RegisterImplementation(DependencyContainer);
//            JobSchedulingConfig.Register(DependencyContainer);
//            DependencyContainer.RegisterInstance<AppSettings>(TestAppSettings);
//            DropDatabase(ConnectionString);
//            NHibernateConfig.RecreateSchema(ConnectionString, TestAppSettings);
//            _unitOfWorkFactory = NHibernateConfig.ConfigureSessionFactory(ConnectionString, DependencyContainer);
        }

        public void DropDatabase(string connectionString)
        {
//            var builder = new DbConnectionStringBuilder { ConnectionString = ConnectionString };
//#if MYSQL
//            var database = builder["database"];
//            builder["database"] = null;
//#else
//            var database = builder["initial catalog"];
//            builder["initial catalog"] = null;
//#endif 

//            try
//            {
//#if MYSQL
//                _dataUtil.Execute(string.Format("drop schema `{0}`;", database), 
//                        builder.ConnectionString);
//#else
//                _dataUtil.Execute(string.Format(@"
//ALTER DATABASE [{0}]
//SET SINGLE_USER 
//WITH ROLLBACK IMMEDIATE;
//DROP DATABASE [{0}];
//", database), 
//                        builder.ConnectionString);
//#endif 
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.Message);
//            }
//#if MYSQL            
//            _dataUtil.Execute(
//                string.Format(
//                    "CREATE DATABASE `{0}` DEFAULT CHARACTER SET utf8 DEFAULT COLLATE utf8_unicode_ci;",
//                    database), builder.ConnectionString);            
//#else
//            _dataUtil.Execute(
//                string.Format(
//                    "CREATE DATABASE {0};",
//                    database), builder.ConnectionString);
//#endif
        }

        private readonly List<IDependencyContainer> _scopeLinks = new List<IDependencyContainer>();

        public IUnitOfWork CreateUnitOfWork()
        {
            var scope = DependencyContainer.BeginScope();
            _scopeLinks.Add(scope); // added link so GC will not delete scope
            return scope.Resolve<IUnitOfWork>();            
        }

        //public IUnitOfWorkFactory UnitOfWorkFactory
        //{
        //    get { return _unitOfWorkFactory; }
        //}

        public string ConnectionString
        {
            get { return _dataUtil.GetConnectionString(); }
        }

        public DataUtil Util
        {
            get { return _dataUtil; }
        }

        public TestAppSettings TestAppSettings { get; private set; }
        public IDependencyContainer DependencyContainer { get; private set; }
    }
}