using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// RoleType Controller to hold all RoleType management related APIs
    /// </summary>
    public class RoleTypeController : BaseApiController
    {
        #region Private variables

        private IManagePersona _managePersona;
        private IManageRoleType _manageRoleType;
        private IProfileRepository _profileRepository;
        private IManageRelationshipType _manageRelationshipType;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RoleTypeController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="messageHandler"></param>
        /// <param name="userClaims"></param>
        public RoleTypeController(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims)
        {
            _manageRoleType = new ManageRoleType(repository);
            _managePersona = new ManagePersona(repository, userClaims, messageHandler);
            _profileRepository = new ProfileRepository(repository, userClaims, messageHandler);
            _userClaims = userClaims;            
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _manageRoleType = new ManageRoleType();
            _managePersona = new ManagePersona(_userClaims);
            _profileRepository = new ProfileRepository(_userClaims);
            _manageRelationshipType = new ManageRelationshipType(_userClaims);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// List Role type details
        /// </summary>
        /// <param name="roleTypeName">RoleType Name</param>
        /// <param name="loginName">Optional User LoginName</param>
        /// <param name="includeRelationShips"></param>
        /// <returns>A list of Role type details</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(IRoleType))]
        [SwaggerResponseExamples(typeof(IRoleType), typeof(RoleTypeExample))]
        [Route("roletypes")]
        [HttpGet]

        public HttpResponseMessage ListRoleType(string roleTypeName = null, string loginName = null, bool includeRelationShips = false)
        {
            var roleTypeList = new List<RoleType>();

            // see if the caller is authenticated and if so use the organization of the user to get the type list

            if (_userClaims.OrganizationPartyId != 0 && roleTypeName != null && roleTypeName.Equals("User Role", StringComparison.OrdinalIgnoreCase))
            {
                var persona = _managePersona.GetPersona(_userClaims.PersonaId);
                if (persona == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");
                }

                roleTypeList = (List<RoleType>)_manageRoleType.GetRoleTypeDependency(roleTypeId: persona.UserTypeId, partyId: _userClaims.OrganizationPartyId, orgMasterId: persona.Organization.BooksCustomerMasterId, loginName: loginName);
                if (!_userClaims.IsRPEmployee && persona.UserTypeId == (int)UserRoleType.ExternalUser)
                {
                    roleTypeList.RemoveAll(x => x.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
                    var externalUserRelationship = _profileRepository.GetExternalUserRelationship(_userClaims.OrganizationPartyId, _userClaims.UserId);
                    if (!string.IsNullOrEmpty(externalUserRelationship.OperatorCode) && !string.IsNullOrEmpty(externalUserRelationship.OperatorValue))
                    {
                        roleTypeList.RemoveAll(x => x.PartyRoleTypeId != 405);
                    }
                }
            }
            else
            {
                roleTypeList = (List<RoleType>)_manageRoleType.GetRoleType(roleTypeName: roleTypeName, partyId: null, orgMasterId: null, loginName: loginName);
            }

            // remove the RealPage employee role from showing for unauthenticated requests
            if (_userClaims.OrganizationPartyId == 0)
            {
                roleTypeList.RemoveAll(x => x.Name.Equals("RealPage Employee", StringComparison.OrdinalIgnoreCase));
            }

            if (roleTypeList == null) return Request.CreateResponse(HttpStatusCode.NoContent, "No Data");

            if (includeRelationShips)
            {
                var userRelationshipTypes = _manageRelationshipType.GetUserRelationShipTypes();

                foreach(var r in roleTypeList)
                {
                    r.UserRelationShipTypes = userRelationshipTypes.Where(c => c.PartyRoleTypeId == r.PartyRoleTypeId).ToList();
                }
            }

            var output = new ObjectListOutput<RoleType, IErrorData>() { list = roleTypeList };
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        #endregion

        #region Get Examples

        /// <summary>
        /// Used to document examples of the RoleType Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class RoleTypeExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>RoleType example</returns>
            public object GetExamples()
            {
                IRoleType example = new RoleType()
                {
                    PartyRoleTypeId = 401,
                    ParentPartyRoleTypeId = 400,
                    Name = "User"
                };

                ObjectOutput<IRoleType, IErrorData> output = new ObjectOutput<IRoleType, IErrorData>() { obj = example };

                return output;
            }
        }

        #endregion
    }
}