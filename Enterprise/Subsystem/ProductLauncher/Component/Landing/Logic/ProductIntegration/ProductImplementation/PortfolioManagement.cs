using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.PortofolioManagement;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class PortfolioManagement : ManageProductInvokerBase, IManageProductIntegration
	{
		#region Constructor

		/// <summary>
		/// 
		/// </summary>
		/// <param name="productType"></param>
		/// <param name="editorPersonaId"></param>
		/// <param name="subjectPersonaId"></param>
		/// <param name="userClaims"></param>
		public PortfolioManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base(productType, editorPersonaId, subjectPersonaId, userClaims)
		{
		}

		public PortfolioManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
			base(productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
		{ }

		#endregion

		#region Methods

		/// <summary>
		/// Get GLOBAL product roles
		/// </summary> 
		public override ListResponse GetProductRoles(RequestParameter dataFilter, string apiQuery = null)
		{
			WriteToDiagnosticLog(
				$"PortfolioManagement.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

			// Get end point for global role
			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);  //http://wmu-books.asseteye.net/api/gandk/Roles?isGlobalRoles=true
			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "true");

			return base.GetProductRoles(dataFilter, baseUrlAndQuery);
		}

		/// <summary>
		/// Override used in Get Roles
		/// </summary> 
		public override IntegrationProductUser GetProductUser(string baseUrlAndQuery = null, bool isThrowOnError = true)
		{
			baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);  //http://wmu-books.asseteye.net/api/gandk/Roles?isGlobalRoles=true
			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, SubjectUserDetails.ProductUserName);

			return base.GetProductUser(baseUrlAndQuery);
		}

		/// <summary>
		/// Get Portfolio Properties+Roles
		/// </summary> 
		public override ListResponse GetProductProperties(RequestParameter dataFilter, string apiQuery = null)
		{
			IList<PortfolioRoleProperty> propertiesList = new List<PortfolioRoleProperty>();

			// get all properties
			var allProperties = GetPortfolioProperties().ToList();

			// get all non-global roles
			var allPropertiesRoles = GetPortfolioPropertySpecificRoles().ToList();

			//build roles-entities object
			foreach (var role in allPropertiesRoles)
			{
				propertiesList.Add(new PortfolioRoleProperty(role, allProperties));
			}

			// if user already exists in product
			if (SubjectUserDetails != null && !string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
			{
				var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);
				baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, SubjectUserDetails.ProductUserName);
				var user = GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery);

				MergePropertyRoles(propertiesList, user.PropertyRoleList);
			}

			return new ListResponse
			{
				Records = propertiesList.Cast<object>().ToList(),
				TotalRows = propertiesList.Count,
				RowsPerPage = 9999,
				ErrorReason = string.Empty,
				TotalPages = 1
			};
		}

		/// <summary>
		/// Override this in product implementation if any product requires to create additional saml settings
		/// e.g. used in PAM
		/// </summary>
		protected override void CreateAdditionalSamlUserAttribute(long personaId, int productId, IntegrationProductUser productUser)
		{
			WriteToDiagnosticLog(
				$"PortfolioManagement.CreateAdditionalSamlUserAttribute - Product {ProductType} userLoginName - {productUser.LoginName} ; PMC {productUser.CompanyId} . At beginning of the method.");

			_dataCollector.CreateSamlUserAttribute(personaId, productId, SamlAttributeEnum.PMCID, productUser.CompanyId);
		}

		/// <summary>
		/// Returns true if user exists in the product
		/// </summary> 
		protected override bool CheckUserExistInProduct(string newUserLoginName, string baseUrlAndQuery = null)
		{
			baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);  //http://wmu-books.asseteye.net/api/gandk/Roles?isGlobalRoles=true
			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, newUserLoginName);

			var productUser = base.GetProductUser(baseUrlAndQuery, false);

			if (productUser != null && !string.IsNullOrEmpty(productUser.UserId))
			{
				return true;
			}

			return false;
		}
		
		#endregion

		#region Private Methods

		private IList<ProductRole> GetPortfolioPropertySpecificRoles()
		{
			// Get end point for property-role
			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);  //http://wmu-books.asseteye.net/api/gandk/Roles?isGlobalRoles=true
			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "false");

			// call API
			return GetResultFromApi<IList<ProductRole>>(baseUrlAndQuery);
		}

		private IList<ProductProperties> GetPortfolioProperties()
		{
			// Get end point for properties
			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyEndpoint);
			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);

			// call API
			return GetResultFromApi<IList<ProductProperties>>(baseUrlAndQuery);
		}

		private void MergePropertyRoles(IList<PortfolioRoleProperty> portfolioPropertyRoles, List<PAMRolePropertyList> userPropertyRoles)
		{
			if (userPropertyRoles != null) 
			{
				foreach (var role in userPropertyRoles)
				{
					foreach (var propRolesList in portfolioPropertyRoles.Where(x => x.GetRoleId == role.RoleId))
					{
						if (propRolesList.PropertiesList.Any(y => role.PropertyIds.Contains(y.GetPropertyId)))
						{
							propRolesList.IsAssigned = true;
							foreach(var property in propRolesList.PropertiesList.Where(z => role.PropertyIds.Contains(z.GetPropertyId)))
							{
								property.IsAssigned = true;
							}
						}
					}
				}
			}
		}

		#endregion
	}

	public class PortfolioRoleProperty : ProductRole
	{
		public PortfolioRoleProperty(ProductRole role, List<ProductProperties> properties)
		{
			SetName = role.GetName;
			SetRoleId = role.GetRoleId;
			PropertiesList = new List<ProductProperties>();
			PropertiesList.AddRange(properties.Select(a => new ProductProperties
			{
				SetPropertyId = a.GetPropertyId,
				SetName = a.GetName,
				PropertyType = a.PropertyType
			}));
		}
		public List<ProductProperties> PropertiesList { get; set; }
	}
}