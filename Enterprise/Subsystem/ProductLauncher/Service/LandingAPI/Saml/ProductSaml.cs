using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using static RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.SAML.RealPageSAML;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Saml
{
	/// <summary>
	/// 
	/// </summary>
	public class SAMLResponseCls
    {
        public string Action = string.Empty;
        public string SAMLResponse { get; set; } = string.Empty;
        public string RelayState { get; set; } = "";
    }
    /// <summary>
    /// 
    /// </summary>
    public class ProductSaml
    {
	    DefaultUserClaim _defaultUserClaim;

	    /// <summary>
	    /// 
	    /// </summary>
	    public ProductSaml(DefaultUserClaim userClaim)
	    {
		    _defaultUserClaim = userClaim;
	    }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ProductId"></param>
        /// <param name="PersonaId"></param>
        /// <param name="RelayStateSAMLAttribute"></param>
        /// <param name="FallBackURL"></param>
        /// <returns></returns>
        public HttpResponseMessage GetSaml(System.Net.Http.HttpRequestMessage Request, int ProductId, long PersonaId, string RelayStateSAMLAttribute = "", string FallBackURL = "")
        {
            if (ProductId == 8)
            {
                RelayStateSAMLAttribute = "userId";
            }
            if (ProductId == 14)
            {
                FallBackURL = "https://www.realpage.com/clientportal";
            }
            string Issuer = "GreenBook";
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            Guid realPageId = Guid.Parse((from nvp in currentClaimPrincipal.Claims where nvp.Type == "realPageId" select nvp.Value).FirstOrDefault());
            //int orgId = Convert.ToInt32((from nvp in currentClaimPrincipal.Claims where nvp.Type == "orgId" select nvp.Value).FirstOrDefault());

            ManagePersona personaManager = new ManagePersona();
            Persona persona = personaManager.GetActivePersona(realPageId);

            PersonaProductUserDetails productDetail = new PersonaProductUserDetails();

            // get the SAML settings for the given product
            ProductSamlSettings productSamlSettings = new ProductSamlSettings();
            ObjectCache productSamlSettingsCache = MemoryCache.Default;
            productSamlSettings = productSamlSettingsCache["productSamlSettings_" + ProductId.ToString()] as ProductSamlSettings;
            if (productSamlSettings == null)
            {
                try
                {
                    var samlRepository = new SamlRepository();
                    productSamlSettings = samlRepository.GetProductSamlSettingsByProductId(ProductId);
                }
                catch (Exception ex)
                {
                    //TODO: handle errors
                }
                if (productSamlSettings != null)
                {
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(600);
                    productSamlSettingsCache.Set("productSamlSettings_" + ProductId.ToString(), productSamlSettings, policy);
                }
            }

            if (ProductId == (int)ProductEnum.UnifiedUI)
            {
                ProductId = (int)ProductEnum.OneSite;
            }

            if (ProductId == (int)ProductEnum.OneSiteConversions)
            {
                ProductId = (int)ProductEnum.OneSite;
            }

            string productType = null;
            if (ProductId == (int)ProductEnum.ClientPortal)
            {
                productType = "IsResource";
            }

            // get the list of products for the given user
            IList<PersonaProductUserDetails> productListAll = new List<PersonaProductUserDetails>();
            IList<PersonaProductUserDetails> productList = new List<PersonaProductUserDetails>();
            try
            {
                var samlRepository = new SamlRepository();
                productListAll = samlRepository.ListAllProductsByPersonaId(PersonaId, ProductId, productType);
            }
            catch (Exception ex)
            {
                System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, innerException.Message);
                //TODO: handle errors
            }

            if (productListAll.Any(a => a.ProductId == ProductId))
            {
                productList.Add((from a in productListAll where a.ProductId == ProductId select a).FirstOrDefault());
            }

            if (productList.Count == 0)
            {
                if (String.IsNullOrEmpty(FallBackURL))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid product id or no product found");
                    //throw new Exception("Invalid product id or no product found");
                }
                else
                {
                    var response = Request.CreateResponse(HttpStatusCode.Moved);
                    response.Headers.Location = new Uri(FallBackURL);
                    return response;
                }
            }
            var model = new ProductViewModel
            {
                ProductList = productList
            };

            if (persona.PersonaId == 0)
            {
                if (productList.Count > 1)
                {
                    // multiple logins
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "multiple logins");
                    //return View(model);
                }
                else
                {
                    productDetail = productList[0];
                }
            }
            else
            {
                // check to see if the id passed is valid
                foreach (PersonaProductUserDetails prodDetail in productList)
                {
                    if (prodDetail.PersonaId == persona.PersonaId)
                    {
                        // found it for this user
                        //portfolioProductUserId = id;
                        productDetail = prodDetail;
                    }
                }
            }

            // if the id is still null, throw an exception
            if (productDetail.PersonaId == 0)// || productDetail.UserId != userId)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, " Invalid product id or no product found");
                //throw new Exception("Invalid persona id or no product found");
            }
            productList = new List<PersonaProductUserDetails>() { productDetail };
            /*
            model = new ProductViewModel
            {
                ProductList = productList
            };*/

            SAMLResponseCls samlResponse = GetSAMLDetails(ProductId, productDetail.PersonaId, productSamlSettings.SigningCertificateThumbprint, Issuer, productSamlSettings.SubjectIdSamlAttribute, RelayStateSAMLAttribute, productSamlSettings.LoginUri);
            // return to client
            samlResponse.Action = productSamlSettings.LoginUri;
            if (ProductId == 8)
            {
                samlResponse.RelayState = samlResponse.RelayState + ":login";
            }
            return Request.CreateResponse<SAMLResponseCls>(HttpStatusCode.OK, samlResponse);
        }

        /// <summary>
        /// Used to build the SAML response for the given application
        /// </summary>
        /// <param name="ProductId">The id of the product being logged into</param>
        /// <param name="PersonaId">The unique id of the product login details for the given user</param>
        /// <param name="SigningCertThumbprint">The thumbprint to search for the signing certificate for the SAML assertion to be signed with</param>
        /// <param name="Issuer">The name used to for the Issuer value of the SAML assertion</param>
        /// <param name="SAMLSubjectAttributeName">The name to use for the Subject attribute of the SAML assertion</param>
        /// <param name="Destination">The destination to use for the SAML assertion</param>
        /// <returns>SAML Assertion in an XMLDocument</returns>
        private SAMLResponseCls GetSAMLDetails(int ProductId, long PersonaId, string SigningCertThumbprint, string Issuer, string SAMLSubjectAttributeName, string SAMLRelayAttributeName, string Destination)
        {
            var samlRepository = new SamlRepository();
            var samlList = samlRepository.GetProductSamlDetails(PersonaId, ProductId);
            //var samlList = ManagerApiHelper.GetResultFromApiAsync<IList<SamlAttributes>>(UserToken, string.Format("api/saml/persona/product/attributes?personaid={0}&productid={1}", PersonaId, ProductId), false).Result;
            IManageProduct manageProduct = new ManageProduct(_defaultUserClaim);
            IList<ProductInternalSetting> productInternalSettingList = manageProduct.GetProductInternalSettings(ProductId);

            // add the enterprise username and user full name into the SAML attributes
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            string userName = (from nvp in currentClaimPrincipal.Claims where nvp.Type == "loginName" select nvp.Value).FirstOrDefault();
            string realPageId = (from nvp in currentClaimPrincipal.Claims where nvp.Type == "realPageId" select nvp.Value).FirstOrDefault();
            //samlList.Add(new SAML.SAMLAttributes() { Name = "EnterpriseLogin", SamlAttributeId = 0, Type = SAML.RealPageSAML2.AttributeURIs.Basic, Value = loginId });
            samlList.Add(new SamlAttributes() { Name = "EnterpriseUserId", SamlAttributeId = 0, Type = AttributeURIs.Basic, Value = realPageId });
            samlList.Add(new SamlAttributes() { Name = "EnterpriseLogin", SamlAttributeId = 0, Type = AttributeURIs.Basic, Value = userName });
            samlList.Add(new SamlAttributes() { Name = "GreenBookUrl", SamlAttributeId = 0, Type = AttributeURIs.Basic, Value = ConfigReader.GetLandingUri });

            // get the Subject value from the SAML Attributes to send to the product
            string samlSubject = "";
            string samlRelay = "";

            foreach (SamlAttributes attribute in samlList)
            {
                // try to find the saml attribute that contains the users unique id for the product to log the user in
                if (attribute.Name.ToUpper() == SAMLSubjectAttributeName.ToUpper())
                {
                    samlSubject = attribute.Value;
                }
                if (!String.IsNullOrEmpty(SAMLRelayAttributeName))
                {
                    if (attribute.Name.ToUpper() == SAMLRelayAttributeName.ToUpper())
                    {
                        if (ProductId == (int)ProductEnum.FinancialSuite)
                        {
                            samlRelay = attribute.Value.Replace("|", ":");
                        }
                        else
                        {
                            samlRelay = attribute.Value;
                        }
                    }
                }
            }

            X509Certificate2 signingCert = GetSigningCertificate(SigningCertThumbprint);
            Saml.RealPageSAML2 samlObj = new Saml.RealPageSAML2(signingCert, Issuer, productInternalSettingList);
            samlObj.Subject = samlSubject;
            samlObj.Destination = Destination;
            samlObj.TokenIssuer = Issuer;
            samlObj.AttributeList = samlList;
            samlObj.ProductId = ProductId;

            XmlDocument responseXMLDocument = samlObj.BuildAssertion();
            SAMLResponseCls samlResponse = new SAMLResponseCls();

            samlResponse.SAMLResponse = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseXMLDocument.OuterXml.ToString()));
            //samlResponse.SAMLXmlDocument = responseXMLDocument;
            samlResponse.RelayState = samlRelay;
            return samlResponse;
        }

        /// <summary>
		/// Used to get a certificate from the certificate store used for signing the SAML assertion
		/// </summary>
		/// <param name="thumbprint"></param>
		/// <returns></returns>
		private X509Certificate2 GetSigningCertificate(string thumbprint)
        {
            var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            // Get the first cert with the thumbprint
            if (certCollection.Count > 0)
            {
                X509Certificate2 cert = certCollection[0];
                certStore.Close();
                // Use certificate
                return cert;
            }
            certStore.Close();
            throw new Exception("No certificate specified or found for " + thumbprint);
        }
    }
}