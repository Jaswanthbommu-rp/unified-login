using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.SAML;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Helper;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Client;
using static RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.SAML.RealPageSAML;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Controllers
{
    //[RequireHttps]
    /// <summary>
    /// Product Controller
    /// </summary>
    public class ProductController : BaseController
	{
		/// <summary>
		/// Used to log into the OneSite system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult OneSite(long personaId = 0)
		{
			// get the OneSite product details
			return GetProductDetails((int)ProductEnum.OneSite, personaId);
		}

		/// <summary>
		/// Used to log into the UnifiedUI system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult UnifiedUI(long personaId = 0)
		{
			// get the UnifiedUI product details
			//int ProductId = (int)ProductEnum.UnifiedUI;

			var model = new ProductViewModel();
			return View(model);
			//return GetProductDetails((int)ProductEnum.UnifiedUI, id);
		}

		/// <summary>
		/// Used to log into the AssetOptimizer system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult AssetOptimizer(long personaId = 0)
		{
			// get the asset optimizer product details
			return GetProductDetails((int)ProductEnum.AssetOptimizer, personaId);
		}

		/// <summary>
		/// Used to log into the Propertyware system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult Propertyware(long personaId = 0)
		{
			// get the propertyware product details
			return GetProductDetails((int)ProductEnum.Propertyware, personaId);
		}

		[Authorize]
		public ActionResult Lead2Lease(long personaId = 0)
		{
			// get the Lead2Lease product details
			return GetProductDetails((int)ProductEnum.Lead2Lease, personaId);
		}

		/// <summary>
		/// Used to log into the Yieldstar system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult Yieldstar(long personaId = 0)
		{
			// get the Yieldstar product details
			return GetProductDetails((int)ProductEnum.Yieldstar, personaId);
		}

		/// <summary>
		/// Used to log into the Accounting system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult Accounting(long personaId = 0)
		{
			// get the Accounting product details
			ActionResult accountingResult = GetProductDetails((int)ProductEnum.FinancialSuite, personaId, "userId");

			ViewBag.RelayState = ViewBag.RelayState + ":login";
			return accountingResult;
		}

		/// <summary>
		/// Used to log into the MarketingCenter system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult MarketingCenter(long personaId = 0)
		{
			// get the MarketingCenter product details
			return GetProductDetails((int)ProductEnum.MarketingCenter, personaId);
		}

		/// <summary>
		/// Used to log into the ProspectContactCenter system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult ProspectContactCenter(long personaId = 0)
		{
			// get the ProspectContactCenter product details
			return GetProductDetails((int)ProductEnum.ProspectContactCenter, personaId);
		}

		/// <summary>
		/// Used to log into the Social system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult Social(long personaId = 0)
		{
			// get the Social product details
			return GetProductDetails((int)ProductEnum.Social, personaId);
		}

		/// <summary>
		/// Used to log into the OpsBid system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult OpsBid(long personaId = 0)
		{
			// get the OpsBid product details
			return GetProductDetails((int)ProductEnum.OpsBid, personaId);
		}

		/// <summary>
		/// Used to log into the OpsBuyer system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult OpsBuyer(long personaId = 0)
		{
			// get the OpsBuyer product details
			return GetProductDetails((int)ProductEnum.OpsBuyer, personaId);
		}

		/// <summary>
		/// Used to log into the ClientPortal system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult ClientPortal(long personaId = 0)
		{
			// get the ClientPortal product details
			return GetProductDetails((int)ProductEnum.ClientPortal, personaId, null, "https://www.realpage.com/clientportal");
		}

        /// <summary>
        /// Used to log into the AdminSupportPortal system
        /// </summary>
        /// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
        /// <returns></returns>
        [Authorize]
        public ActionResult AdminSupportPortal(long personaId = 0)
        {
            // get the ClientPortal product details
            return GetProductDetails((int)ProductEnum.AdminSupportPortal, personaId, null, "https://www.realpage.com/clientportal");
        }
        /// <summary>
        /// Used to log into the Insurance system
        /// </summary>
        /// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
        /// <returns></returns>
        [Authorize]
		public ActionResult Insurance(long personaId = 0)
		{
			// get the Insurance product details
			return GetProductDetails((int)ProductEnum.Insurance, personaId);
		}

		/// <summary>
		/// Used to log into the Portfolio Management system
		/// </summary>
		[Authorize]
		public ActionResult PortfolioManagement(long personaId = 0)
		{
			if (CheckForViewOnlyAccess())
			{
				return RedirectToAction("ReadOnly", "Error");
			}
			try
			{
				var productUrl = GetProductRedirectUrl((int)ProductEnum.PortfolioManagement);
				return Redirect(productUrl);
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "PortfolioManagement", $"Exception in personaId {personaId}." }, exception: ex);
				return new HttpStatusCodeResult(500);
			}
		}

		/// <summary>
		/// Used to log into the VendorServices system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult VendorServices(long personaId = 0)
		{
			// get the VendorServices product details
			return GetProductDetails((int)ProductEnum.VendorServices, personaId);
		}

		/// <summary>
		/// Used to log into the ResidentPortal system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult ResidentPortal(long personaId = 0)
		{
			// get the ResidentPortal product details
			return GetProductDetails((int)ProductEnum.ResidentPortal, personaId);
		}

        /// <summary>
        /// Used to log a user into a SAML product
        /// </summary>
        /// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
        /// <returns></returns>
        [Authorize]
        public ActionResult PropertyPhotos(long personaId = 0)
        {
            // get the PropertyPhotos product details
            return GetProductDetails((int)ProductEnum.PropertyPhotos, personaId);
        }

        /// <summary>
        /// Used to log a user into an OAuth client
        /// </summary>
        /// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
        /// <returns></returns>
        [Authorize]
		public ActionResult UtilityManagement(long personaId = 0)
		{
			try
			{
				if (CheckForViewOnlyAccess())
				{
					return RedirectToAction("ReadOnly", "Error");
				}
				else
				{
					var returnUrlForGreenBook = BuildLoginToken(ProductEnum.UtilityManagement, "rum", personaId, "id_token token", "openid profile roles nwpscope apibrowser managerapi", "form_post", Request["access_token"]);
					Response.Redirect(returnUrlForGreenBook, false);
				}

			}
			catch (Exception ex)
			{
				System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
				return new HttpStatusCodeResult(innerException.Response.StatusCode, innerException.Message);
			}
			return View();
		}

		/// <summary>
		/// Used to log a user into an OAuth client
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult OnSite(long personaId = 0)
		{
			try
			{
				if (CheckForViewOnlyAccess())
				{
					return RedirectToAction("ReadOnly", "Error");
				}
				else
				{
					//var returnUrlForGreenBook = GetProductRedirectUrl((int)ProductEnum.OnSite);
					var returnUrlForGreenBook = BuildLoginToken(ProductEnum.OnSite, "onsite", personaId, "id_token token", "openid profile userinfoapi", "form_post", Request["access_token"]);
					Response.Redirect(returnUrlForGreenBook, false);
				}
			}
			catch (Exception ex)
			{
				System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
				return new HttpStatusCodeResult(innerException.Response.StatusCode, innerException.Message);
			}
			return View();
		}

		/// <summary>
		/// Used to log a user into an OAuth client
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult SelfProvisioningPortal(long personaId = 0)
		{
			try
			{
				if (CheckForViewOnlyAccess())
				{
					return RedirectToAction("ReadOnly", "Error");
				}
				else
				{
					//var returnUrlForGreenBook = GetProductRedirectUrl((int)ProductEnum.SelfProvisioningPortal);
					var returnUrlForGreenBook = BuildLoginToken(ProductEnum.SelfProvisioningPortal, "SelfProvisioningPortal", personaId, "id_token token", "openid profile selfprovisioningportal", "form_post", Request["access_token"]);
					Response.Redirect(returnUrlForGreenBook, false);
				}
			}
			catch (Exception ex)
			{
				System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
				return new HttpStatusCodeResult(innerException.Response.StatusCode, innerException.Message);
			}
			return View();
		}

		/// <summary>
		/// Used to log a user into an OAuth client
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult VendorMarketplace(long personaId = 0)
		{
			try
			{
				if (CheckForViewOnlyAccess())
				{
					return RedirectToAction("ReadOnly", "Error");
				}
				else
				{
					var returnUrlForGreenBook = GetProductRedirectUrl((int)ProductEnum.VendorMarketplace);
					Response.Redirect(returnUrlForGreenBook, false);
				}
			}
			catch (Exception ex)
			{
				System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
				return new HttpStatusCodeResult(innerException.Response.StatusCode, innerException.Message);
			}
			return View();
		}

		/// <summary>
		/// Used to log a user into an OAuth client
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult IntegrationMarketPlace(long personaId = 0)
		{
			try
			{
				var productUrl = GetProductRedirectUrl((int)ProductEnum.IntegrationMarketplace);
				return Redirect(productUrl);
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "IntegrationMarketPlace", $"Exception in personaId {personaId}." }, exception: ex);
				return new HttpStatusCodeResult(500);
			}
		}

		/// <summary>
		/// Used to log a user into an OAuth client
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult Cimpl(long personaId = 0)
		{
			try
			{
				var productUrl = GetProductRedirectUrl((int)ProductEnum.CIMPL);
				return Redirect(productUrl);
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "Cimpl", $"Exception in personaId {personaId}." }, exception: ex);
				return new HttpStatusCodeResult(500);
			}
		}

		/// <summary>
		/// Used to log a user into an OAuth client
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult ILMLeadManagement(long personaId = 0)
		{
			try
			{
				if (CheckForViewOnlyAccess())
				{
					return RedirectToAction("ReadOnly", "Error");
				}
				else
				{
					//var returnUrlForGreenBook = GetProductRedirectUrl((int)ProductEnum.LeadManagement);
					var returnUrlForGreenBook = BuildLoginToken(ProductEnum.LeadManagement, "leadmanagement", personaId, "code", "openid profile userinfoapi", null, Request["access_token"]);
					Response.Redirect(returnUrlForGreenBook, false);
				}
			}
			catch (Exception ex)
			{
				System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
				return new HttpStatusCodeResult(innerException.Response.StatusCode, innerException.Message);
			}
			return View();
		}

		/// <summary>
		/// Used to log a user into an OAuth client
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult ILMLeadAnalytics(long personaId = 0)
		{
			try
			{
				if (CheckForViewOnlyAccess())
				{
					return RedirectToAction("ReadOnly", "Error");
				}
				else
				{
					//var returnUrlForGreenBook = GetProductRedirectUrl((int)ProductEnum.LeadAnalytics);
					var returnUrlForGreenBook = BuildLoginToken(ProductEnum.LeadAnalytics, "leadanalytics", personaId, "code", "openid profile userinfoapi", null, Request["access_token"]);
					Response.Redirect(returnUrlForGreenBook, false);
				}
			}
			catch (Exception ex)
			{
				System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
				return new HttpStatusCodeResult(innerException.Response.StatusCode, innerException.Message);
			}
			return View();
		}

		/// <summary>
		/// Used to log a user into an OAuth client
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult SettingsManagement(long personaId = 0)
		{
			try
			{
				if (CheckForViewOnlyAccess())
				{
					return RedirectToAction("ReadOnly", "Error");
				}
				else
				{
					//var returnUrlForGreenBook = GetProductRedirectUrl((int)ProductEnum.SettingsManagement);
					var returnUrlForGreenBook = BuildLoginToken(ProductEnum.SettingsManagement, "settings-management", personaId, "code id_token token", "openid profile roles offline_access rplandingapi unifiedsettingsapi settings-management-tool", "form_post", Request["access_token"]);
					Response.Redirect(returnUrlForGreenBook, false);
				}
			}
			catch (Exception ex)
			{
				System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
				return new HttpStatusCodeResult(innerException.Response.StatusCode, innerException.Message);
			}
			return View();
		}

		/// <summary>
		/// Used to log into the RPDM system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult RPDM(long personaId = 0)
		{
			// get the iDoc product details
			return GetProductDetails((int)ProductEnum.RPDocumentManagement, personaId);
		}

		/// <summary>
		/// Used to log into the OneSiteConversions system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult OneSiteConversions(long personaId = 0)
		{
			// get the OneSiteConversions product details
			return GetProductDetails((int)ProductEnum.OneSiteConversions, personaId);
		}

		/// <summary>
		/// Used to log a user into an OAuth client
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult ResearchApplication(long personaId = 0)
		{
			try
			{
				var returnUrlForGreenBook = GetProductRedirectUrl((int)ProductEnum.ResearchApplication);
				Response.Redirect(returnUrlForGreenBook, false);
			}
			catch (Exception ex)
			{
				System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
				return new HttpStatusCodeResult(innerException.Response.StatusCode, innerException.Message);
			}
			return View();
		}

		/// <summary>
		/// Used to log a user into an OAuth client
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult OmniChannel(long personaId = 0)
		{
			try
			{
				if (CheckForViewOnlyAccess())
				{
					return RedirectToAction("ReadOnly", "Error");
				}
				else
				{
					var returnUrlForGreenBook = GetProductRedirectUrl((int)ProductEnum.OmniChannel);
					Response.Redirect(returnUrlForGreenBook, false);
				}
			}
			catch (Exception ex)
			{
				System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
				return new HttpStatusCodeResult(innerException.Response.StatusCode, innerException.Message);
			}
			return View();
		}

		/// <summary>
		/// Used to log a user into the Unified Amenities product
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult UnifiedAmenities(long personaId = 0)
		{
			try
			{
				if (CheckForViewOnlyAccess())
				{
					return RedirectToAction("ReadOnly", "Error");
				}
				else
				{
					var returnUrlForGreenBook = GetProductRedirectUrl((int)ProductEnum.UnifiedAmenities);
					Response.Redirect(returnUrlForGreenBook, false);
				}
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
			}
			return View();
		}

		/// <summary>
		/// Used to log a user into the Migration Tool product
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult MigrationTool(long personaId = 0)
		{
			try
			{
				if (CheckForViewOnlyAccess())
				{
					return RedirectToAction("ReadOnly", "Error");
				}
				else
				{
					var defaultUserClaim = GetDefaultUserClaim();
					GbProductMap booksProductDetail = ProductHelper.GetBooksMasterProductDetail(defaultUserClaim, (int)ProductEnum.MigrationTool);

					LogActivity.WriteActivity(new ActivityDetails
					{
						LogActivityTypeName = "Migration Tool",
						LogCategoryName = LogActivityCategoryType.User.ToString(),
						CorrelationId = defaultUserClaim.CorrelationId.ToString(),
						BooksMasterOrganizationId = defaultUserClaim.OrganizationMasterId,
						OrganizationPartyId = defaultUserClaim.OrganizationPartyId,
						Message = $"{defaultUserClaim.FirstName} {defaultUserClaim.LastName} accessed Migration Tool.",

						FromUserLoginName = defaultUserClaim.LoginName,
						FromUserLoginId = defaultUserClaim.UserId,
						FromUserFirstName = defaultUserClaim.FirstName,
						FromUserLastName = defaultUserClaim.LastName,
						FromUserRealpageId = defaultUserClaim.UserRealPageGuid.ToString(),

						BooksProductCode = booksProductDetail.BooksProductCode,
					});
					var productSamlSettings = ProductSamlSettings(ProductEnum.MigrationTool, Request["access_token"]);
					Response.Redirect(productSamlSettings.LoginUri, false);
				}

			}
			catch (Exception ex)
			{
				System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
				return new HttpStatusCodeResult(innerException.Response.StatusCode, innerException.Message);
			}
			return View();
		}

		#region Ao Products

		/// <summary>
		/// Used to log into the Business Intelligence system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult BusinessIntelligence(long personaId = 0)
		{
			try
			{
				var productUrl = GetProductRedirectUrl((int)ProductEnum.AoBusinessIntelligence);
				return Redirect(productUrl);
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "BusinessIntelligence", $"Exception in personaId {personaId}." }, exception: ex);
				return new HttpStatusCodeResult(500);
			}

			// get the asset optimizer product details
			//return GetProductDetails((int)ProductEnum.AoBusinessIntelligence, personaId);
		}

		/// <summary>
		/// Used to log into the Investment Analytics system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult InvestmentAnalytics(long personaId = 0)
		{
			try
			{
				var productUrl = GetProductRedirectUrl((int)ProductEnum.AoInvestmentAnalytics);
				return Redirect(productUrl);
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "InvestmentAnalytics", $"Exception in personaId {personaId}." }, exception: ex);
				return new HttpStatusCodeResult(500);
			}

			// get the asset optimizer product details
			//return GetProductDetails((int)ProductEnum.AoInvestmentAnalytics, personaId);
		}

		/// <summary>
		/// Used to log into the Axiometrics system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult Axiometrics(long personaId = 0)
		{
			try
			{
				var productUrl = GetProductRedirectUrl((int)ProductEnum.AoAxiometrics);
				return Redirect(productUrl);
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "Axiometrics", $"Exception in personaId {personaId}." }, exception: ex);
				return new HttpStatusCodeResult(500);
			}

			// get the asset optimizer product details
			//return GetProductDetails((int)ProductEnum.AoAxiometrics, personaId);
		}

		/// <summary>
		/// Used to log into the Performance Analytics system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult PerformanceAnalytics(long personaId = 0)
		{
			try
			{
				var productUrl = GetProductRedirectUrl((int)ProductEnum.AoPerformanceAnalytics);
				return Redirect(productUrl);
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "PerformanceAnalytics", $"Exception in personaId {personaId}." }, exception: ex);
				return new HttpStatusCodeResult(500);
			}

			// get the asset optimizer product details
			//return GetProductDetails((int)ProductEnum.AoPerformanceAnalytics, personaId);
		}

		/// <summary>
		/// Used to log into the Revenue Management system
		/// </summary>
		/// <param name="personaId">If passed, it will contain the unique id of the Persona to use to log into the product</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult RevenueManagement(long personaId = 0)
		{
			try
			{
				var productUrl = GetProductRedirectUrl((int)ProductEnum.AoRevenueManagement);
				return Redirect(productUrl);
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "RevenueManagement", $"Exception in personaId {personaId}." }, exception: ex);
				return new HttpStatusCodeResult(500);
			}

			// get the asset optimizer product details
			//return GetProductDetails((int)ProductEnum.AoRevenueManagement, personaId);
		}

		#endregion

		/// <summary>
		/// Used to log as Realpage Employee Admin/User to  Company User (User Impersonation)
		/// </summary>
		/// <param name="realPageId">realpageId of the Company User</param>
		/// <returns></returns>
		[Authorize]
        [Obsolete]
		public ActionResult RealPageEmployeeUPAccessNOTUSED(string realPageId = null)
		{
			return View();
		}

		/// <summary>
		/// Used to log as Realpage Employee Admin/User to  Edit Redbook Settings
		/// </summary>
		/// <param name="impersonatedUserRealPageId">Impersonated User RealPageId</param>
		/// <param name="companyId">Master companyId of the Company User</param>
		/// <param name="companyName">Company Name</param>
		/// <returns></returns>
		[Authorize]
		public ActionResult RealPageEmployeeSettingsAccess(Guid impersonatedUserRealPageId, string companyId = null, string companyName = null)
		{
			var defaultUserClaim = GetDefaultUserClaim();
			GbProductMap booksProductDetail = ProductHelper.GetBooksMasterProductDetail(_userClaims, (int)ProductEnum.SupportTool);

			LogActivity.WriteActivity(new ActivityDetails
			{
				LogActivityTypeName = "Support Tool",
				LogCategoryName = LogActivityCategoryType.User.ToString(),
				CorrelationId = defaultUserClaim.CorrelationId.ToString(),
				BooksMasterOrganizationId = defaultUserClaim.OrganizationMasterId,
				OrganizationPartyId = defaultUserClaim.OrganizationPartyId,
				Message = $"{defaultUserClaim.FirstName} {defaultUserClaim.LastName} accessed Unified Settings via Support Tool for {companyName}.",

				FromUserLoginName = defaultUserClaim.LoginName,
				FromUserLoginId = defaultUserClaim.UserId,
				FromUserFirstName = defaultUserClaim.FirstName,
				FromUserLastName = defaultUserClaim.LastName,
				FromUserRealpageId = defaultUserClaim.UserRealPageGuid.ToString(),

				BooksProductCode = booksProductDetail.BooksProductCode,
			});

			return RedirectToAction("Index", "setting", new { CompanyId = companyId, CompanyName = companyName });
		}

		/// <summary>
		/// Used to log into the Report system of any product
		/// </summary>
		[Authorize]
		public ActionResult ProductReport(string inputParams)
		{
			ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
			long personaId = 0;

			if (currentClaimPrincipal.Identity.IsAuthenticated)
			{
				var userClaims = new DefaultUserClaim(currentClaimPrincipal);
				personaId = userClaims.PersonaId;
				var loginName = userClaims.LoginName;


				string stringInputParams = Encoding.UTF8.GetString(Convert.FromBase64String(inputParams));
				var inputQueryParams = HttpUtility.ParseQueryString(stringInputParams);

				string productCodeString = inputQueryParams["productCode"];
				string productUserLogin = inputQueryParams["productUserLogin"];
				string reportParams = inputQueryParams["reportParams"];

				Enum.TryParse(productCodeString, out ProductEnum productCode);

				// check if report user has same loginName as claims
				if (productUserLogin.ToLower() != loginName.ToLower())
				{
					Response.Redirect(ConfigReader.GetLandingUri + "#/error/access-denied");
					return null;
				}

				switch (productCode)
				{
					case ProductEnum.AssetOptimizer:
						{
							return GetProductDetails((int)ProductEnum.AssetOptimizer, personaId, "", "", true, reportParams);
						}
					default:
						{
							Response.Write($"Wrong product code received {productCodeString}.");
							Response.End();
							return null;
						}
				}
			}

			Response.Redirect(ConfigReader.GetLandingUri + "#/error/access-denied");
			return View();
		}

		#region Private functions

		/// <summary>
		/// Used to log a user into an OAuth client
		/// </summary>
		/// <param name="product"></param>
		/// <param name="productName"></param>
		/// <param name="personaId"></param>
		/// <param name="responseType"></param>
		/// <param name="scopesForAuth"></param>
		/// <param name="responseMode"></param>
		/// <param name="usertoken"></param>
		/// <returns></returns>
		private string BuildLoginToken(ProductEnum product, string productName, long personaId, string responseType, string scopesForAuth, string responseMode, string usertoken)
		{
			ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
			
			Persona persona = GetPersona(_userClaims.UserRealPageGuid, personaId);

			if (persona == null)
			{
				throw new Exception("Error getting product details, no persona found");
			}
			if (RealPageSAML.ProductDetails((int)product, persona, out var getOneSitePMCURL, out var getDocMgtDomain, out var getMarketingCenterUrl, out var productList))
			{
				throw new Exception("Error getting product details");
			}

			// only check status for products that have a status
			if (productList?.Count > 0)
			{
				PersonaProductUserDetails productDetail = productList[0];

				if (productDetail.ProductStatus != (int)ProductBatchStatusType.Success)
				{
					Response.Redirect(ConfigReader.GetLandingUri + "#/error/access-denied");
				}
			}

			// get the product details
			var idp = (from nvp in currentClaimPrincipal.Claims where nvp.Type == "idp" select nvp.Value).FirstOrDefault();

			if (string.IsNullOrEmpty(idp))
				throw new Exception("No idp included in redirect querystring!!");

			var state = Guid.NewGuid().ToString("N");
			var nonce = Guid.NewGuid().ToString("N");
			var client = new OAuth2Client(new Uri(ConfigReader.GetIssuerUri + "/connect/authorize"));

			// get the product Login url
			var productSamlSettings = ProductSamlSettings(product, usertoken);
			string loginUri = productSamlSettings.LoginUri;

			AddActivityLog((int)product);

			return client.CreateAuthorizeUrl(productName, responseType, scopesForAuth,
				loginUri, state, nonce, acrValues: $"idp:{idp}", responseMode: responseMode);
		}

		private static ProductSamlSettings ProductSamlSettings(ProductEnum product, string usertoken)
		{
			// get the SAML settings for the given product
			ProductSamlSettings productSamlSettings = new ProductSamlSettings();
			RPObjectCache rpcache = new RPObjectCache();
			var cacheKey = $"productSamlSettings_{(int)product}";
			productSamlSettings = rpcache.GetFromCache<ProductSamlSettings>(cacheKey, 300, () =>
			{
				// load from api
				var samlRepository = new SamlRepository();
				return samlRepository.GetProductSamlSettingsByProductId((int)product);
			});
			return productSamlSettings;
		}

		/// <summary>
		/// Used to log a user into SAML enabled products
		/// </summary>
		/// <param name="productId">The id of the product being logged into</param>
		/// <param name="personaId">The unique user/product login id to use to log the user into a specific product</param>
		/// <param name="relayStateSamlAttribute">The SAML Relay value</param>
		/// <param name="fallBackUrl">The url to use if the user has no product login</param>
		/// <param name="isProductReport">If product calls report</param>
		/// <param name="reportParams">Report parameters to send</param>
		/// <returns></returns>
		private ActionResult GetProductDetails(int productId, long personaId, string relayStateSamlAttribute = "", string fallBackUrl = "", bool isProductReport = false, string reportParams = "")
		{
			var usertoken = Request["access_token"];

			RealPageSAML rpsaml = new RealPageSAML(_userClaims);

            ProductLoginResponse productLoginResponse = rpsaml.GetProductDetailsSAML(ConfigReader.GetLandingUri, productId, personaId, usertoken, relayStateSamlAttribute, fallBackUrl, "", isProductReport, reportParams);


			if (!string.IsNullOrEmpty(productLoginResponse.ErrorMessage))
			{
				if (productLoginResponse.ErrorMessage.Equals("AccessDenied", StringComparison.OrdinalIgnoreCase))
				{
					return RedirectToAction("ReadOnly", "Error");
				}
				else
				{
					Response.Write(productLoginResponse.ErrorMessage);
				}
				Response.End();
				return null;
			}

			if (productLoginResponse.IsRedirect)
			{
				Response.Redirect(productLoginResponse.RedirectUrl);
				return View();
			}

			if (productLoginResponse.IsSAML)
			{
				// return to client
				ViewBag.Action = productLoginResponse.SamlResponse.Destination;
				ViewBag.SAMLResponse = productLoginResponse.SamlResponse.SAMLBase64Encoded;
				ViewBag.RelayState = productLoginResponse.SamlResponse.RelayState;

				// add activity log
				if (isProductReport)
				{
					AddActivityLog(productId, isEmailLinkActivity: isProductReport);
				}
				else
				{
					AddActivityLog(productId);
				}				
			}

			var model = new ProductViewModel();
			return View(model);
		}

		/// <summary>
		/// Used to get the persona for the given RealPage user
		/// </summary>
		/// <param name="RealPageId">The id of the person</param>
		/// <param name="PersonaId">The personaid for the person</param>
		/// <returns>Persona object</returns>
		private Persona GetPersona(Guid RealPageId, long PersonaId)
		{
			Persona persona = new Persona();
			ManagePersona personaManager = new ManagePersona();

			if (PersonaId == 0)
			{
				// get the current users default persona
				try
				{
					//persona = personaManager.GetActivePersona(RealPageId);
					PersonaId = _userClaims.PersonaId;
				}
				catch (Exception ex)
				{
					return null;
				}
			}
			else
			{
				try
				{
					// verify the persona belongs to the caller
					persona = personaManager.GetPersona(PersonaId);
					bool hasImpersonate = _userClaims.Rights.Any(p => p.Equals("AccessToUnifiedPlatform", StringComparison.OrdinalIgnoreCase));
					if (persona == null || (persona.RealPageId != RealPageId && !hasImpersonate))
					{
						throw new Exception("Invalid persona");
					}
				}
				catch (Exception ex)
				{
					return null;
				}
			}
			return persona;
		}

		/// <summary>
		/// Add Activity Log
		/// </summary>
		/// <param name="productId">productId</param>
		/// <param name="message">message</param>
		/// <param name="isEmailLinkActivity">Indicates if activity because user accessed link from email (e.g. report) </param>
		private void AddActivityLog(int productId, string message = "", bool isEmailLinkActivity = false)
		{
			try
			{
				GbProductMap booksProductDetail = ProductHelper.GetBooksMasterProductDetail(_userClaims, productId);

				var logActivityTypeName = "Product Access";

				if (isEmailLinkActivity)
				{
					logActivityTypeName = "Product Activity";
					message = $"{_userClaims.FirstName} {_userClaims.LastName} opened {booksProductDetail.Name} Scheduled Reports from an email link..";
				}

				if (string.IsNullOrEmpty(message))
				{
					if(string.IsNullOrEmpty(_userClaims.ImpersonatedByName))
					{
						message = $"User {_userClaims.FirstName} {_userClaims.LastName} accessed product {booksProductDetail.Name}.";
					}
					else
					{
						message = $"RealPage user {_userClaims.ImpersonatedByName} accessed product {booksProductDetail.Name}.";
					}
				}

				LogActivity.WriteActivity(new ActivityDetails
				{
					LogActivityTypeName = logActivityTypeName,
					LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
					CorrelationId = _userClaims.CorrelationId.ToString(),
					BooksMasterOrganizationId = _userClaims.OrganizationMasterId,
					OrganizationPartyId = _userClaims.OrganizationPartyId,
					Message = message,

					FromUserLoginName = _userClaims.LoginName,
					FromUserLoginId = _userClaims.UserId,
					FromUserFirstName = _userClaims.FirstName,
					FromUserLastName = _userClaims.LastName,
					FromUserRealpageId = _userClaims.UserRealPageGuid.ToString(),

					BooksProductCode = booksProductDetail.BooksProductCode
				});
			}
			catch { }
		}

		/// <summary>
		/// Get DefaultUser Claim
		/// </summary>
		/// <returns>DefaultUserClaim object</returns>
		private DefaultUserClaim GetDefaultUserClaim()
		{
			return new DefaultUserClaim(ClaimsPrincipal.Current);
		}

		
		private bool CheckForViewOnlyAccess()
		{
			return (_userClaims.Rights.Any(p => p.Equals("ViewOnlySupportToolAccess", StringComparison.OrdinalIgnoreCase)));
		}


		/// <summary>
		/// Product Redirect Url
		/// </summary>
		/// <param name="productId">The id of the product being logged into</param>
		/// <param name="isProductReport">If product calls report</param>
		/// <param name="reportParams">Report parameters to send</param>
		/// <returns>Product Redirect Url</returns>
		private string GetProductRedirectUrl(int productId, bool isProductReport = false, string reportParams = "")
		{
			// get the SAML settings for the given product
			var productSamlSettings = new ProductSamlSettings();
			RPObjectCache rpcache = new RPObjectCache();
			var cacheKey = $"productSamlSettings_{productId}";
			productSamlSettings = rpcache.GetFromCache<ProductSamlSettings>(cacheKey, 600, () =>
			{
				// load from api
				var samlRepository = new SamlRepository();
				return samlRepository.GetProductSamlSettingsByProductId(productId);
			});

			string productUrl = productSamlSettings.LoginUri;

			if (isProductReport)
			{
				//TODO: TBD for future - Modify below code once report supports OAuth
				// get SAML for Product = ProductReport
				//response = GetSAMLDetails(productId, usertoken,
				//  productSamlSettings.SigningCertificateThumbprint, Issuer,
				//  productSamlSettings.SubjectIdSamlAttribute, relayStateSamlAttribute, samlEndpointURL, samlList, reportParams, true);
			}

			// add activity log
			if (isProductReport)
			{
				AddActivityLog(productId, isEmailLinkActivity: isProductReport);
			}
			else
			{
				AddActivityLog(productId);
			}

			return productUrl;
		}
		#endregion
	}


}