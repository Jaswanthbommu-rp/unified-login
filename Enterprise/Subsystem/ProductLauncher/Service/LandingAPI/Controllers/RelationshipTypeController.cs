using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using System.Web.Http.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// RelationshipType Controller to hold all RelationshipType management related APIs
	/// </summary>
	public class RelationshipTypeController : BaseApiController
	{
		#region Private variables
		private readonly IRelationshipTypeRepository _relationshipTypeRepository;
		IRepositoryResponse repositoryResponse = new RepositoryResponse();
		private IManageRelationshipType _manageRelationshipType;
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public RelationshipTypeController() : base() { }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _manageRelationshipType = new ManageRelationshipType(_userClaims);

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// List Role type details
        /// </summary>
        /// <returns>A list of Role type details</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(IRelationshipType))]
		[SwaggerResponseExamples(typeof(IRelationshipType), typeof(RelationshipTypeExample))]
		[Route("relationshiptypes/{relationshipTypeName}")]
		[HttpGet]
		public HttpResponseMessage ListRelationshipType(string relationshipTypeName)
		{
            IList<RelationshipType> relationshipTypeList = new List<RelationshipType>();

			IManageRelationshipType relationshipTypeLogic = new ManageRelationshipType(_userClaims);

			if (_relationshipTypeRepository == null)
			{
				relationshipTypeList = relationshipTypeLogic.GetRelationshipType(relationshipTypeName);
			}
			else
			{
				relationshipTypeList = _relationshipTypeRepository.GetRelationshipType(relationshipTypeName);
			}

			if (relationshipTypeList != null)
			{
				ObjectListOutput<RelationshipType, IErrorData> output = new ObjectListOutput<RelationshipType, IErrorData>() { list = relationshipTypeList };
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			//When trying to get a list of relationshipTypes that doesn't exists
			return Request.CreateResponse(HttpStatusCode.NoContent, "No Data");
		}

        /// <summary>
        /// List UserRelationShiptypes details
        /// </summary>
        /// <returns>A list of Role type details</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(UserRelationShipType))]
        [SwaggerResponseExamples(typeof(UserRelationShipType), typeof(UserRelationShipExample))]
        [Route("userrelationshiptypes")]
        [HttpGet]
        public HttpResponseMessage ListUserRelationTypes() 
		{
			var userRelationships =_manageRelationshipType.GetUserRelationShipTypes();

            if (userRelationships != null)
            {
                ObjectListOutput<UserRelationShipType, IErrorData> output = new ObjectListOutput<UserRelationShipType, IErrorData>() { list = userRelationships };
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
            return Request.CreateResponse(HttpStatusCode.NoContent, "No Data");
        }
        #endregion

        #region Get Examples
        /// <summary>
        /// Used to document examples of the RelationshipType Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
		public class RelationshipTypeExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>RelationshipType example</returns>
			public object GetExamples()
			{
				IRelationshipType example = new RelationshipType()
				{
					RelationshipTypeId = 43,
					RoleTypeIdValidFrom = 401,
					RoleTypeIdValidTo = 201,
					Name = "User Relationship",
					Description = "User relationship between person and organization"
				};

				ObjectOutput<IRelationshipType, IErrorData> output = new ObjectOutput<IRelationshipType, IErrorData>() { obj = example };

				return output;
			}
		}

        /// <summary>
        /// Used to document examples of the RelationshipType Model webapi result
        /// </summary>
        public class UserRelationShipExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>UserRelationShip example</returns>
            public object GetExamples()
            {
                UserRelationShipType example = new UserRelationShipType()
                {
                    UserRelationshipName = "Employee",
                    ThirdPartyRelationshipId = 4,
                    SortIndex = 1,
                    PartyRoleTypeId = 401,
                    Description = "Employee user with email format username"
                };

                ObjectOutput<UserRelationShipType, IErrorData> output = new ObjectOutput<UserRelationShipType, IErrorData>() { obj = example };

                return output;
            }
        }
        #endregion
    }
}