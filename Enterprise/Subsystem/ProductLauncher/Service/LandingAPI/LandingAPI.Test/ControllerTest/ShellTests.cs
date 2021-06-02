using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class ShellTests
    {
        private DefaultUserClaim _userClaim = new DefaultUserClaim() { CorrelationId = Guid.NewGuid() };

        [Fact]
        public void GetSideMenuStructureAndOrdering()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<PersonaActionRight>(StoredProcNameConstants.SP_ListPersonaRightsAndActionsByRoute, It.IsAny<object>()))
                .Returns(new List<PersonaActionRight>()
                {
                    new PersonaActionRight()
                    {
                        ObjectType = "Route",
                        Action = "Side Menu"
                    },
                    new PersonaActionRight()
                    {
                        ObjectType = "Right",
                        Action = "Dashboard"
                    }
                });

            mockRepository
                .Setup(m => m.GetMany<NavigationMenuEntry>(StoredProcNameConstants.SP_GetNavigationMenu, It.IsAny<object>()))
                .Returns(new List<NavigationMenuEntry>()
                {
                    new NavigationMenuEntry()
                    {
                        Id = 1,
                        Icon = "test icon 1",
                        OrderIndex = 1,
                        PageId = "page-id 1",
                        ParentId = null,
                        Title = "title 1",
                        URL = "http://url/"
                    },
                    new NavigationMenuEntry()
                    {
                        Id = 2,
                        Icon = "test icon 2",
                        OrderIndex = 0,
                        PageId = "page-id 2",
                        ParentId = null,
                        Title = "title 2",
                        URL = "http://url/"
                    },
                    new NavigationMenuEntry()
                    {
                        Id = 3,
                        Icon = "test icon 3",
                        OrderIndex = 0,
                        PageId = "page-id 3",
                        ParentId = 1,
                        Title = "title 3",
                        URL = "http://url/"
                    },
                    new NavigationMenuEntry()
                    {
                        Id = 4,
                        Icon = "test icon 4",
                        OrderIndex = 0,
                        PageId = "page-id 4",
                        ParentId = null,
                        Title = "title 4",
                        URL = "http://url/"
                    }
                });

            mockRepository
                .Setup(m => m.GetMany<NavigationMenuRightEntry>(StoredProcNameConstants.SP_GetNavigationMenuRights, It.IsAny<object>()))
                .Returns(new List<NavigationMenuRightEntry>()
                {
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 1,
                        RightName = "Dashboard"
                    },
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 2,
                        RightName = "Dashboard"
                    },
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 3,
                        RightName = "Dashboard"
                    },
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 4,
                        RightName = "not valid"
                    }
                });

            ShellController shellController = new ShellController(mockRepository.Object, mockMessageHandler.Object, _userClaim)
            {
                _personaId = 1234
            };

            var response = shellController.GetSideMenuNavigation();

            Assert.Equal(2, response.Count);
            Assert.Equal("title 2", response[0].Title);
            Assert.Null(response[0].Items);
            Assert.Equal("title 1", response[1].Title);
            Assert.Single(response[1].Items);
            Assert.Equal("title 3", response[1].Items[0].Title);
        }
    }
}
