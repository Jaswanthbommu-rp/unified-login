using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
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
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// RoleType Controller to hold all RoleType management related APIs
    /// </summary>
    public class RoleTypeController : BaseApiController
	{
		#region Private variables
		private readonly IRoleTypeRepository _roleTypeRepository;
		IRepositoryResponse repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public RoleTypeController() : base()
		{
			_roleTypeRepository = new RoleTypeRepository();
		}

		/// <summary>
		/// Testing Constructor
		/// </summary>
		/// <param name="roleTypeRepository">RoleType Repository</param>
		public RoleTypeController(IRoleTypeRepository roleTypeRepository)
		{
			_roleTypeRepository = roleTypeRepository;
		}
        #endregion

        #region Public Methods
        /// <summary>
        /// List Role type details
        /// </summary>
        /// <param name="roleTypeName">RoleType Name</param>
        /// <param name="loginName">Optional User LoginName</param>
        /// <returns>A list of Role type details</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(IRoleType))]
		[SwaggerResponseExamples(typeof(IRoleType), typeof(RoleTypeExample))]
		[Route("roletypes")]
		[HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage ListRoleType(string roleTypeName = null, string loginName = null)
        {
			List<RoleType> roleTypeList = new List<RoleType>();

			IManageRoleType roleTypeLogic = new ManageRoleType(_roleTypeRepository);

			// see if the caller is authenticated and if so use the organization of the user to get the type list

			if (base._orgPartyId != 0 && roleTypeName.Equals("User Role", StringComparison.OrdinalIgnoreCase))
			{
				IManagePersona managePersona = new ManagePersona();
                var persona = managePersona.GetPersona(_personaId);
                if (persona == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");
                }

                roleTypeList = (List<RoleType>)roleTypeLogic.GetRoleTypeDependency(roleTypeId: persona.UserTypeId, partyId: base._orgPartyId, orgMasterId: persona.Organization.BooksCustomerMasterId, loginName: loginName);
				if ((DefaultUserClaim.EmployeeCompanyRealPageId != base._userClaims.OrganizationRealPageGuid && !base._userClaims.IsRPEmployee))
				{
					roleTypeList.RemoveAll(x => x.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
				}
			}
            else
            {
                roleTypeList = (List<RoleType>)roleTypeLogic.GetRoleType(roleTypeName: roleTypeName, partyId: null, orgMasterId: null, loginName: loginName);
            }

			// remove the RealPage employee role from showing for unauthenticated requests
			if (base._orgPartyId == 0)
			{
				roleTypeList.RemoveAll(x => x.Name.Equals("RealPage Employee", StringComparison.OrdinalIgnoreCase));
			}
			if (roleTypeList != null)
			{
				roleTypeList = roleTypeList.OrderBy(r => r.Name).ToList();
				ObjectListOutput<RoleType, IErrorData> output = new ObjectListOutput<RoleType, IErrorData>() { list = roleTypeList };
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			//When trying to get a list of roleTypes that doesn't exists
			return Request.CreateResponse(HttpStatusCode.NoContent, "No Data");
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