using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Contact Mechanism UsageType Controller to hold all Contact Mechanism UsageType management related APIs
	/// </summary>
	public class ContactMechanismUsageTypeController : BaseApiController
	{
		#region Private variables
		private readonly IContactMechanismUsageTypeRepository _contactMechanismUsageTypeRepository;
		IRepositoryResponse repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public ContactMechanismUsageTypeController() : base() { }

		/// <summary>
		/// Testing Constructor
		/// </summary>
		/// <param name="contactMechanismUsageTypeRepository">Contact Mechanism UsageType Repository</param>
		public ContactMechanismUsageTypeController(IContactMechanismUsageTypeRepository contactMechanismUsageTypeRepository)
		{
			_contactMechanismUsageTypeRepository = contactMechanismUsageTypeRepository;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// List contact mechanism usage type details
		/// </summary>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>A list of contact mechanism usage type details</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the Contact Mechanism UsageType", Type = typeof(IContactMechanismUsageType))]
		[SwaggerResponseExamples(typeof(IContactMechanismUsageType), typeof(ContactMechanismUsageTypeExample))]
		[Route("contactmechanismusagetypes")]
		[HttpGet]

        public HttpResponseMessage ListContactMechanismUsageType(string ContactMechanismUsageTypeName = null)
		{
			IList<ContactMechanismUsageType> contactMechanismUsageTypeList = new List<ContactMechanismUsageType>();

			IManageContactMechanismUsageType contactMechanismUsageTypeLogic = new ManageContactMechanismUsageType();

			if (_contactMechanismUsageTypeRepository == null)
			{
				contactMechanismUsageTypeList = contactMechanismUsageTypeLogic.ListContactMechanismUsageType(ContactMechanismUsageTypeName);
			}
			else
			{
				contactMechanismUsageTypeList = _contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName);
			}

			if (contactMechanismUsageTypeList != null)
			{
				ObjectListOutput<ContactMechanismUsageType, IErrorData> output = new ObjectListOutput<ContactMechanismUsageType, IErrorData>() { list = contactMechanismUsageTypeList };
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			//When trying to get a list of Contact Mechanism UsageTypes that doesn't exists
			return Request.CreateResponse(HttpStatusCode.NoContent, "No Data");
		}
		#endregion

		#region Get Examples
		/// <summary>
		/// Used to document examples of the Contact Mechanism UsageType Model webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class ContactMechanismUsageTypeExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>Contact Mechanism UsageType example</returns>
			public object GetExamples()
			{
				IContactMechanismUsageType example = new ContactMechanismUsageType()
				{
					ContactMechanismUsageTypeId = 201,
					ParentContactMechanismUsageTypeId = 202,
					Name = "Primary"
				};

				ObjectOutput<IContactMechanismUsageType, IErrorData> output = new ObjectOutput<IContactMechanismUsageType, IErrorData>() { obj = example };

				return output;
			}
		}
		#endregion
	}
}