using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.ServiceLayer.Services;
using NUnit.Framework;

namespace ServiceLayerTests
{
    public class ServiceFactoryTests
    {
        private ServiceFactory factory;

        [SetUp]
        public void Setup()
        {
            factory = new ServiceFactory();
        }

        [Test]
        public void UserService_IsNotNull()
        {
            Assert.IsNotNull(factory.UserService);
        }

        [Test]
        public void BoardService_IsNotNull()
        {
            Assert.IsNotNull(factory.BoardService);
        }

        [Test]
        public void TaskService_IsNotNull()
        {
            Assert.IsNotNull(factory.TaskService);
        }

        [Test]
        public void Services_AreSingletonsWithinFactory()
        {
            // Each property should return the same instance every time
            Assert.AreSame(factory.UserService, factory.UserService);
            Assert.AreSame(factory.BoardService, factory.BoardService);
            Assert.AreSame(factory.TaskService, factory.TaskService);
        }
    }
}
