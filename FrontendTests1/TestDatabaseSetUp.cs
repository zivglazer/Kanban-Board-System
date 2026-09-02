// File: FrontendUnitTests/TestDatabaseSetup.cs

using NUnit.Framework;
using Frontend.Model; // עדיין צריך בשביל להכיר את BackendController ו-UserModel, גם אם לא מאתחלים אותם כאן
using System;
using IntroSE.Kanban.Backend.ServiceLayer.Services; // חשוב!

namespace FrontendUnitTests
{
    [SetUpFixture]
    public class TestDatabaseSetup
    {

        public static ServiceFactory BackendServiceFactory;


        [OneTimeSetUp]
        public void RunBeforeAllTests()
        {
            Console.WriteLine($"--- Running OneTimeSetUp in FrontendUnitTests: Initializing Test Database ---");


            BackendServiceFactory = new ServiceFactory();
            //BackendServiceFactory.InitializeDatabaseForTests();
            BackendServiceFactory.DeleteData();

            Console.WriteLine($"--- Test Database Initialization Complete in FrontendUnitTests ---");
        }

        [OneTimeTearDown]
        public void RunAfterAllTests()
        {
            Console.WriteLine($"--- Running OneTimeTearDown in FrontendUnitTests: Disposing Test Database ---");
            if (BackendServiceFactory != null)
            {

                BackendServiceFactory.DeleteData();
               // BackendServiceFactory.DisposeDatabaseForTests();
            }
            Console.WriteLine($"--- Test Database Disposal Complete in FrontendUnitTests ---");
        }
    }
}