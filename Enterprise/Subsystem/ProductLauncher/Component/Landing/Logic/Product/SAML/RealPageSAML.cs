using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using Serilog.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.SAML
{

    public class RealPageSAML
	{
		private readonly List<ProductInternalSetting> _productInternalSettingList = new List<ProductInternalSetting>();
		private readonly DefaultUserClaim _userClaims;

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="userClaims"></param>
		public RealPageSAML(DefaultUserClaim userClaims)
		{
			_userClaims = userClaims;
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="SigningCertificate"></param>
		/// <param name="Issuer"></param>
		/// <param name="ProductInternalSettingList"></param>
		public RealPageSAML(X509Certificate2 SigningCertificate, string Issuer, List<ProductInternalSetting> ProductInternalSettingList)
		{
			_Issuer = Issuer;
			_SigningCertificate = SigningCertificate;
			_productInternalSettingList = ProductInternalSettingList;
		}

		#region Privates
		/// <summary>
		/// 
		/// </summary>
		private string _Destination { get; set; } = "";
		/// <summary>
		/// 
		/// </summary>
		private string _Issuer { get; set; } = "";
		/// <summary>
		/// 
		/// </summary>
		private string _Subject { get; set; } = "";
		/// <summary>
		/// 
		/// </summary>
		private string _TokenIssuer { get; set; } = "";
		/// <summary>
		/// 
		/// </summary>
		private X509Certificate2 _SigningCertificate { get; set; }
		/// <summary>
		/// 
		/// </summary>
		private int _ProductId { get; set; } = 0;

		#endregion

		#region Public

		/// <summary>
		/// Used to set the destination URL for the SAML request
		/// </summary>
		public string Subject
		{
			get { return _Subject == null ? "" : _Subject; }
			set { _Subject = value; }
		}

		/// <summary>
		/// Used to set the destination URL for the SAML request
		/// </summary>
		public string Destination
		{
			get { return _Destination; }
			set { _Destination = value; }
		}

		public string TokenIssuer
		{
			get { return _TokenIssuer; }
			set { _TokenIssuer = value; }
		}

		public int ProductId
		{
			get { return _ProductId; }
			set { _ProductId = value; }
		}
		/// <summary>
		/// 
		/// </summary>
		public IList<SamlAttributes> AttributeList;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private XmlDocument BuildAssertion()
		{

			DateTime issueInstant = DateTime.UtcNow.ToUniversalTime();

			// build the SAML assertion
			Saml2Assertion assertion = new Saml2Assertion(new Saml2NameIdentifier(_Issuer, new Uri(RealPageSAML.AssertionUri)));

			assertion.Subject = new Saml2Subject(new Saml2NameIdentifier(_Subject, new Uri(RealPageSAML.NameIDFormatUris.Unspecified)));

            assertion.Conditions = new Saml2Conditions()
            {
                NotBefore = DateTime.UtcNow.AddHours(-1),
                NotOnOrAfter = DateTime.UtcNow.AddHours(1)
            };

            // SalesForce required SAML info
            if (_ProductId == (int)ProductEnum.ClientPortal || _ProductId == (int)ProductEnum.AdminSupportPortal)
			{
				assertion.Subject = new Saml2Subject(new Saml2NameIdentifier(_Subject, new Uri(RealPageSAML.NameIDFormatUris.Email)));
				Saml2SubjectConfirmation conf = new Saml2SubjectConfirmation(new Uri("urn:oasis:names:tc:SAML:2.0:cm:bearer"));
				Saml2AudienceRestriction audience = new Saml2AudienceRestriction();
				audience.Audiences.Add(new Uri("https://saml.salesforce.com"));
				assertion.Conditions = new Saml2Conditions()
                {
                    NotBefore = DateTime.UtcNow.AddHours(-1),
                    NotOnOrAfter = DateTime.UtcNow.AddHours(1)
                };
                assertion.Conditions.AudienceRestrictions.Add(audience);
				conf.NameIdentifier = new Saml2NameIdentifier(_Subject)
				{
					Format = new Uri(RealPageSAML.NameIDFormatUris.Unspecified)
				};
				conf.SubjectConfirmationData = new Saml2SubjectConfirmationData();
				string recipient = _productInternalSettingList.First(a => a.Name.ToUpper() == "SAMLRECIPIENT").Value;
				conf.SubjectConfirmationData.Recipient = new Uri(recipient);
				conf.SubjectConfirmationData.NotOnOrAfter = DateTime.UtcNow.AddHours(1);
				assertion.Subject.SubjectConfirmations.Add(conf);
			}
			// SalesForce required SAML info

			// TESTING ONLY!!!
            if (_ProductId == 80)
            {
                assertion.Subject = new Saml2Subject(new Saml2NameIdentifier(_Subject, new Uri(RealPageSAML.NameIDFormatUris.Email)));
                var conf = new Saml2SubjectConfirmation(new Uri("urn:oasis:names:tc:SAML:2.0:cm:bearer"));
                var audience = new Saml2AudienceRestriction();
                audience.Audiences.Add(new Uri("https://learningmanager.adobe.com"));
                assertion.Conditions = new Saml2Conditions()
                {
                    NotBefore = DateTime.UtcNow.AddHours(-1),
                    NotOnOrAfter = DateTime.UtcNow.AddHours(1)
                };
                assertion.Conditions.AudienceRestrictions.Add(audience);
                //conf.NameIdentifier = new Saml2NameIdentifier(_Subject)
                //{
                //    Format = new Uri(RealPageSAML.NameIDFormatUris.Email)
                //};
                conf.SubjectConfirmationData = new Saml2SubjectConfirmationData();
                var recipient = _productInternalSettingList.First(a => a.Name.ToUpper() == "SAMLRECIPIENT").Value;
                conf.SubjectConfirmationData.Recipient = new Uri(recipient);
                conf.SubjectConfirmationData.NotOnOrAfter = DateTime.UtcNow.AddHours(1);
                assertion.Subject.SubjectConfirmations.Add(conf);
            }

            assertion.Id = new Saml2Id();
			assertion.IssueInstant = issueInstant;
			assertion.Issuer = new Saml2NameIdentifier(_Issuer);


			// SalesForce required SAML info
			if (_ProductId == (int)ProductEnum.ClientPortal || _ProductId == (int)ProductEnum.AdminSupportPortal)
			{
				Saml2AudienceRestriction ar = new Saml2AudienceRestriction(new Uri("https://saml.salesforce.com"));
				assertion.Conditions.AudienceRestrictions.Add(ar);
			}
			// SalesForce required SAML info

			var authn = new Saml2AuthenticationStatement(new Saml2AuthenticationContext(new Uri(RealPageSAML.PasswordUri)));

            if (_ProductId == 80)
            {
                authn = new Saml2AuthenticationStatement(new Saml2AuthenticationContext(new Uri(RealPageSAML.PasswordProtected)));
            }

			authn.SessionIndex = Guid.NewGuid().ToString();
			assertion.Statements.Add(authn);

			List<Saml2Attribute> samlAttributes = new List<Saml2Attribute>();

			foreach (SamlAttributes attr in AttributeList)
			{
				samlAttributes.Add(new Saml2Attribute(attr.Name, attr.Value) { NameFormat = new Uri(attr.Type), FriendlyName = attr.Name });
			}

            if (_ProductId != 80)
            {
                var attrstatement = new Saml2AttributeStatement(samlAttributes);
                assertion.Statements.Add(attrstatement);
            }

            var clientSigningCredentials = new X509SigningCredentials(_SigningCertificate, RealPageSAML.Algorithms.SHA1_SignatureMethod, RealPageSAML.Algorithms.SHA1_DigestMethod);
			assertion.SigningCredentials = clientSigningCredentials;

			var stoken = new Saml2SecurityToken(assertion);
			var handler = new Saml2SecurityTokenHandler();
			var desc = new SecurityTokenDescriptor()
			{
				Token = stoken,
				TokenIssuerName = _TokenIssuer
			};

			var sw = new StringWriter();

			// use the Saml2Assertion to build the Assertion XML object, but sign the assertion later once it can include the correct namespaces
			handler.WriteToken(new XmlTextWriter(sw) { Namespaces = true }, stoken);
			XmlDocument assertionXMLDocument = new XmlDocument();
			assertionXMLDocument.LoadXml(sw.ToString());

			// add the saml prefix to the namespaces in the document
			AddPrefix(assertionXMLDocument.DocumentElement, "saml");

			XmlDocument responseXMLDocument = new XmlDocument();
			XmlElement responseXmlElement = responseXMLDocument.CreateElement(RealPageSAML.Prefixes.SAMLP, "Response", RealPageSAML.NamespaceURIs.Protocol);

			string responseSaml2Id = new Saml2Id().Value;
			responseXmlElement.SetAttribute("ID", responseSaml2Id);
			responseXmlElement.SetAttribute("Version", RealPageSAML.Version);
			responseXmlElement.SetAttribute("IssueInstant", issueInstant.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
			responseXmlElement.SetAttribute("Destination", _Destination);
			XmlElement issuerXmlElement = responseXMLDocument.CreateElement(RealPageSAML.Prefixes.SAML, "Issuer", RealPageSAML.NamespaceURIs.Assertion);
			issuerXmlElement.InnerText = _Issuer;
			responseXmlElement.AppendChild(issuerXmlElement);

			XmlElement statusXmlElement = responseXMLDocument.CreateElement(RealPageSAML.Prefixes.SAMLP, "Status", RealPageSAML.NamespaceURIs.Protocol);
			XmlElement statusCodeXmlElement = responseXMLDocument.CreateElement(RealPageSAML.Prefixes.SAMLP, "StatusCode", RealPageSAML.NamespaceURIs.Protocol);
			statusCodeXmlElement.SetAttribute("Value", RealPageSAML.StatusUris.Success);
			statusXmlElement.AppendChild(statusCodeXmlElement);
			responseXmlElement.AppendChild(statusXmlElement);

			XmlNamespaceManager nsmgr = new XmlNamespaceManager(assertionXMLDocument.NameTable);
			nsmgr.AddNamespace("sig", RealPageSAML.NamespaceURIs.Signature);
			XmlNode signature = assertionXMLDocument.SelectSingleNode("//sig:Signature", nsmgr);
			// remove the signature created by the Saml2Assertion WriteToken process because the xml being signed isn't in the correct form for products to accecpt it
			assertionXMLDocument.DocumentElement.RemoveChild(signature);

			XmlNode importAssertion = responseXmlElement.OwnerDocument.ImportNode(assertionXMLDocument.DocumentElement, true);
			responseXmlElement.AppendChild(importAssertion);
			responseXMLDocument.AppendChild(responseXmlElement);

			// begin new signing procedure for the SAML assertion xml
			Reference reference = new Reference()
			{
				Uri = "#" + responseSaml2Id
			};
			SignedXml signedXml = new SignedXml(responseXMLDocument)
			{
				SigningKey = _SigningCertificate.GetRSAPrivateKey(),
			};

			signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

			//canonicalize
			XmlDsigExcC14NTransform e14t = new XmlDsigExcC14NTransform("#default samlp saml ds xs xsi");
			XmlDsigEnvelopedSignatureTransform envT = new XmlDsigEnvelopedSignatureTransform(false);
			reference.AddTransform(envT); // add first

            if (_ProductId != 80)
            {
                reference.AddTransform(e14t);
            }

            KeyInfo keyInfo = new KeyInfo();
			KeyInfoX509Data keyInfoData = new KeyInfoX509Data(_SigningCertificate);
			KeyInfoName kin = new KeyInfoName();
			RSACryptoServiceProvider rsaprovider = (RSACryptoServiceProvider)_SigningCertificate.PublicKey.Key;
			RSAKeyValue rkv = new RSAKeyValue(rsaprovider);
			keyInfo.AddClause(keyInfoData);
			signedXml.KeyInfo = keyInfo;

			// Add the reference to the SignedXml object.
			signedXml.AddReference(reference);

			// Compute the signature.
			signedXml.ComputeSignature();

			// Get the XML representation of the signature and save 
			// it to an XmlElement object.
			XmlElement xmlDigitalSignature = signedXml.GetXml();

			responseXMLDocument.DocumentElement.InsertBefore(responseXMLDocument.ImportNode(xmlDigitalSignature, true), importAssertion);

			// test to see if the result xml can be validated
			/*
            SignedXml testSignedXml = new SignedXml(responseXMLDocument);
            XmlNamespaceManager nsmgr1 = new XmlNamespaceManager(assertionXMLDocument.NameTable);
            nsmgr1.AddNamespace("sig", RealPageSAML.NamespaceURIs.Signature);
            XmlElement sig = responseXMLDocument.SelectSingleNode("//sig:Signature", nsmgr1) as XmlElement;
            testSignedXml.LoadXml(sig);
            if (!testSignedXml.CheckSignature(_SigningCertificate, true))
            {
                throw new Exception("signature check failed");
            }
            */

			return responseXMLDocument;
		}

        public IList<SamlAttributes> createUserBatchIfRequired(long personaId, int productId)
        {
            BatchProductBulkUpdateRepository productBulkUpdateRepository = new BatchProductBulkUpdateRepository(_userClaims);

            SamlRepository samlRepository = new SamlRepository();
            IList<SamlAttributes> samlAttributeDetails = new List<SamlAttributes>();
            var productInternalSettingList = GetProductInternalSettings(productId);
            var userCreationSettingInfo = productInternalSettingList.FirstOrDefault(a => a.Name.Equals("IsUserCreationOnTileClick", StringComparison.OrdinalIgnoreCase))?.Value;
            bool isUserCreationRequired = false;
            if (userCreationSettingInfo != null)
            {
                isUserCreationRequired = Convert.ToBoolean(userCreationSettingInfo);
            }

            var samlDetails = samlRepository.GetProductSamlDetails(personaId, productId);
            if (samlDetails.Count() == 0 && isUserCreationRequired)
            {
                OrganizationRepository organizationRepository = new OrganizationRepository();
                UserRepository userRepository = new UserRepository(_userClaims);
                var retryCheckCount = 5;
                var statusCheckSleep = 5000;

                var statusCheckSleepSetting = productInternalSettingList.FirstOrDefault(a => a.Name.Equals("BatchUserProductStatusSleepTimeout", StringComparison.OrdinalIgnoreCase))?.Value;
                var retrySetting = productInternalSettingList.FirstOrDefault(a => a.Name.Equals("BatchUserProductStatusRetryCount", StringComparison.OrdinalIgnoreCase))?.Value;
                var defaultUserRoleId = productInternalSettingList.FirstOrDefault(a => a.Name.Equals("DefaultUserRoleId", StringComparison.OrdinalIgnoreCase))?.Value;
                Guid editorGuid = organizationRepository.GetOrganizationAdminUserRealPageId(_userClaims.OrganizationRealPageGuid);

                var userinfo = userRepository.GetUserDetails(userRealPageId: editorGuid.ToString());
                IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
                if (_userClaims.ImpersonatedBy != Guid.Empty)
                {
                    UserLoginRepository userLoginRepository = new UserLoginRepository();
                    impersonatorUserLoginOnly = userLoginRepository.GetUserLoginOnly(_userClaims.ImpersonatedBy);
                }

                if (retrySetting != null)
                {
                    retryCheckCount = Convert.ToInt16(retrySetting);
                }

                if (statusCheckSleepSetting != null)
                {
                    statusCheckSleep = Convert.ToInt32(statusCheckSleepSetting);
                }
                samlAttributeDetails = productBulkUpdateRepository.CreateBatch(userinfo.PersonaId, personaId, editorGuid, productId, retryCheckCount, statusCheckSleep, defaultUserRoleId, impersonatorUserLoginOnly.UserId);
            }
			if (samlDetails.Count() == 0)
			{
				return samlAttributeDetails;
			}
			else
			{
				return samlDetails;
			}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unifiedLoginUri"></param>
        /// <param name="productId"></param>
        /// <param name="personaId"></param>
        /// <param name="userToken"></param>
        /// <param name="relayStateSamlAttribute"></param>
        /// <param name="fallBackUrl"></param>
        /// <param name="isProductReport"></param>
        /// <param name="reportParams"></param>
        /// <returns></returns>
        public ProductLoginResponse GetProductDetailsSAML(string unifiedLoginUri, int productId, long personaId, string userToken, string relayStateSamlAttribute = "", string fallBackUrl = "", string issuer = "", bool isProductReport = false, string reportParams = "")
		{
			ProductLoginResponse response = new ProductLoginResponse();

			string samlEndpointURL = "";
			int activityProductId = productId;

            string Issuer = string.IsNullOrEmpty(issuer) ? "GreenBook" : issuer;
            if (_userClaims.Rights.Any(p => p.Equals("ViewOnlySupportToolAccess", StringComparison.OrdinalIgnoreCase)))
			{
				response.ErrorMessage = "AccessDenied";
				return response;
			}

			Persona persona = GetPersona(_userClaims.UserRealPageGuid, personaId);

			PersonaProductUserDetails productDetail = new PersonaProductUserDetails();

			var productSamlSettings = GetProductSamlSettings(productId);

			samlEndpointURL = productSamlSettings.LoginUri;

			if (ProductDetails(productId, persona, out var getOneSitePMCURL, out var getDocMgtDomain, out var getMarketingCenterUrl, out var productList))
			{
				response.ErrorMessage = "There was a problem getting the product details";
				return response;
			}

			if (productList.Count == 0)
			{
				if (String.IsNullOrEmpty(fallBackUrl))
				{
					response.ErrorMessage = "Invalid product id or no product found";
					return response;
				}
				else
				{
					response.RedirectUrl = fallBackUrl;
					response.IsRedirect = true;
					return response;
				}
			}

			if (persona.PersonaId == 0)
			{
				productDetail = productList[0];
			}
			else
			{
				// check to see if the id passed is valid
				foreach (PersonaProductUserDetails prodDetail in productList)
				{
					if (prodDetail.PersonaId == persona.PersonaId)
					{
						// found it for this user
						productDetail = prodDetail;
					}
				}
			}

			// if the id is still null, throw an exception
			if (productDetail.PersonaId == 0)
			{
				response.ErrorMessage = "Invalid product id or no product found";
				return response;
			}
			else
			{
				personaId = productDetail.PersonaId;
			}

			productList = new List<PersonaProductUserDetails>() { productDetail };

			if (productDetail.ProductStatus != (int)ProductBatchStatusType.Success)
			{
				response.IsRedirect = true;
				response.RedirectUrl = unifiedLoginUri + "error/401";
				return response;
			}

			switch (productId)
			{
				case (int)ProductEnum.UnifiedUI:
					productId = (int)ProductEnum.OneSite;
					break;
				case (int)ProductEnum.OneSiteConversions:
					productId = (int)ProductEnum.OneSite;
					//productType = "IsResource";
					break;
				case (int)ProductEnum.PropertyPhotos:
					productId = (int)ProductEnum.MarketingCenter;
                    break;
                default:
                    break;
            }

            SamlRepository samlRepository = new SamlRepository();
            var samlList = samlRepository.GetProductSamlDetails(personaId, productId);

            if (getOneSitePMCURL)
			{
				// need to get the PMC's url for OneSite for the SAML post because of cookie issues
				samlEndpointURL = GetOneSitePMCURL(samlEndpointURL, personaId, samlList);
			}

			if (getDocMgtDomain)
			{
				samlEndpointURL = GetDocManagementDomainURL(samlEndpointURL, personaId, samlList);
			}

			if (getMarketingCenterUrl)
			{
				var marketingCenterSettings = GetProductSamlSettings((int)ProductEnum.MarketingCenter);
				samlList.Add(new SamlAttributes() { Name = "RedirectUrl", SamlAttributeId = 0, Type = SAML.RealPageSAML.AttributeURIs.Basic, Value = samlEndpointURL });
				samlEndpointURL = marketingCenterSettings.LoginUri;
			}

			if (isProductReport)
			{
				// extend SAML URL with product params
				//if (!string.IsNullOrEmpty(reportParams))
				//	samlEndpointURL = samlEndpointURL + "/" + reportParams;

				// get SAML for Product = ProductReport
				response.SamlResponse = GetSAMLDetails(unifiedLoginUri, productId, userToken,
				  productSamlSettings.SigningCertificateThumbprint, Issuer,
				  productSamlSettings.SubjectIdSamlAttribute, relayStateSamlAttribute, samlEndpointURL, samlList, reportParams, true);
			}
			else
			{
				// get SAML for Product
				response.SamlResponse = GetSAMLDetails(unifiedLoginUri, productId, userToken,
				  productSamlSettings.SigningCertificateThumbprint, Issuer,
				  productSamlSettings.SubjectIdSamlAttribute, relayStateSamlAttribute, samlEndpointURL, samlList);
			}
			response.IsSAML = true;
			return response;
		}

		/// <summary>
		/// Used to get SAML settings for the specified product
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		private static ProductSamlSettings GetProductSamlSettings(int productId)
		{
			// get the SAML settings for the given product
			ProductSamlSettings productSamlSettings = new ProductSamlSettings();
			SamlRepository samlRepository = new SamlRepository();

			RPObjectCache rpcache = new RPObjectCache();
			var cacheKey = $"productSamlSettings_{(int)productId}";
			productSamlSettings = rpcache.GetFromCache<ProductSamlSettings>(cacheKey, 600, () =>
			{
				// load from api

				if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)productId))
				{
					return samlRepository.GetProductSamlSettingsByProductId((int)ProductEnum.AssetOptimizer);
				}

				return samlRepository.GetProductSamlSettingsByProductId(productId);
			});
			return productSamlSettings;
		}

		/// <summary>
		/// Used to get various settings for the product being logged into
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="persona"></param>
		/// <param name="getOneSitePMCURL"></param>
		/// <param name="getDocMgtDomain"></param>
		/// <param name="getMarketingCenterURL"></param>
		/// <param name="productList"></param>
		/// <returns></returns>
		public static bool ProductDetails(int productId, Persona persona, out bool getOneSitePMCURL, out bool getDocMgtDomain, out bool getMarketingCenterURL, out IList<PersonaProductUserDetails> productList)
		{
			string productType = null;
			getOneSitePMCURL = false;
			getDocMgtDomain = false;
			getMarketingCenterURL = false;

			switch (productId)
			{
				case (int)ProductEnum.OneSite:
					getOneSitePMCURL = true;
					break;
				case (int)ProductEnum.UnifiedUI:
					productId = (int)ProductEnum.OneSite;
					break;
				case (int)ProductEnum.OneSiteConversions:
					productId = (int)ProductEnum.OneSite;
					//productType = "IsResource";
					break;
				case (int)ProductEnum.ClientPortal:
					productType = "IsResource";
					break;
				case (int)ProductEnum.ResearchApplication:
					productType = "IsResource";
					break;
				case (int)ProductEnum.MigrationTool:
					productType = "IsResource";
					break;
				case (int)ProductEnum.IntegrationMarketplace:
					productId = (int)ProductEnum.IntegrationMarketplace;
					productType = "IsResource";
					break;
				case (int)ProductEnum.RPDocumentManagement:
					getDocMgtDomain = true;
					break;
				case (int)ProductEnum.PropertyPhotos:
					productId = (int)ProductEnum.MarketingCenter;
					getMarketingCenterURL = true;
					break;
                case (int)ProductEnum.AdminSupportPortal:
                    productType = "IsResource";
                    break;
                default:
					break;
			}

			// get the list of products for the given user
			IList<PersonaProductUserDetails> productListAll = new List<PersonaProductUserDetails>();
			productList = new List<PersonaProductUserDetails>();
			var samlRepository = new SamlRepository();

			try
			{
				productListAll = samlRepository.ListAllProductsByPersonaId(persona.PersonaId, productId, productType);
			}
			catch (Exception ex)
			{
				System.Web.Http.HttpResponseException innerException = (System.Web.Http.HttpResponseException)ex.InnerException;
				{
					return true;
				}
			}

			if (productListAll.Any(a => a.ProductId == productId))
			{
				productList.Add((from a in productListAll where a.ProductId == productId select a).FirstOrDefault());
			}

			return false;
		}

		/// <summary>
		/// Used to build the SAML response for the given application
		/// </summary>
		/// <param name="unifiedLoginURL">The url for unified login</param>
		/// <param name="productId">The id of the product being logged into</param> 
		/// <param name="userToken">The current user log in token used to verify the user</param>
		/// <param name="signingCertThumbprint">The thumbprint to search for the signing certificate for the SAML assertion to be signed with</param>
		/// <param name="issuer">The name used to for the Issuer value of the SAML assertion</param>
		/// <param name="samlSubjectAttributeName">The name to use for the Subject attribute of the SAML assertion</param>
		/// <param name="samlRelayAttributeName"></param>
		/// <param name="destination">The destination to use for the SAML assertion</param>
		/// <param name="samlList"></param>
		/// <param name="reportParams">Report params to send in SAML</param>
		/// <returns>SAML Assertion in an XMLDocument</returns>
		public SAMLResponse GetSAMLDetails(string unifiedLoginURL, int productId, string userToken, string signingCertThumbprint, string issuer, string samlSubjectAttributeName, string samlRelayAttributeName, string destination, IList<SamlAttributes> samlList, string reportParams = "", bool isProductReport = false)
		{
			var productInternalSettingList = GetProductInternalSettings(productId);

			// add the enterprise username and user full name into the SAML attributes

			samlList.Add(new SamlAttributes() { Name = "EnterpriseUserId", SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = _userClaims.UserRealPageGuid.ToString() });
			samlList.Add(new SamlAttributes() { Name = "EnterpriseLogin", SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = _userClaims.LoginName });
			samlList.Add(new SamlAttributes() { Name = "GreenBookUrl", SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = unifiedLoginURL });
			samlList.Add(new SamlAttributes() { Name = "GreenBookToken", SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = userToken });

			// If product belongs to AO then add product name in SAML
			if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)productId))
			{
				samlList.Add(new SamlAttributes() { Name = "Product", SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = ProductEnumHelper.GetAoProductId((ProductEnum)productId) });
			}

			// for product calling reporting framework - set product = ProductReport
			if (isProductReport)
			{
				samlList.Add(new SamlAttributes() { Name = "Product", SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = "ProductReport" });
				samlList.Add(new SamlAttributes() { Name = "ReportParams", SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = reportParams });
			}

			// get the Subject value from the SAML Attributes to send to the product
			string samlSubject = "";
			string samlRelay = "";

			foreach (SamlAttributes attribute in samlList)
			{
				// try to find the saml attribute that contains the users unique id for the product to log the user in
				if (attribute.Name.Equals(samlSubjectAttributeName, StringComparison.OrdinalIgnoreCase))
				{
					samlSubject = attribute.Value;
				}
				if (!string.IsNullOrEmpty(samlRelayAttributeName))
				{
					if (attribute.Name.Equals(samlRelayAttributeName, StringComparison.OrdinalIgnoreCase))
					{
						if (productId == (int)ProductEnum.FinancialSuite)
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

            if (string.IsNullOrEmpty(samlSubject))
            {
                if (string.IsNullOrEmpty(_userClaims?.LoginName))
                {
                    throw new Exception("Empty SAML Subject");
				}
                samlSubject = _userClaims.LoginName;
            }

			X509Certificate2 signingCert = GetSigningCertificate(signingCertThumbprint);
			RealPageSAML saml = new RealPageSAML(signingCert, issuer, productInternalSettingList)
			{
				Subject = samlSubject,
				Destination = destination,
				TokenIssuer = issuer,
				AttributeList = samlList,
				ProductId = productId
			};

			XmlDocument responseXmlDocument = saml.BuildAssertion();
			SAMLResponse samlResponse = new SAMLResponse
			{
				SAMLBase64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseXmlDocument.OuterXml.ToString())),
				RelayState = samlRelay,
				Destination = destination
			};
			return samlResponse;
		}

		#endregion

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
				// get the current users persona
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
		/// Used to get the PMC URl for the given OneSite user login
		/// </summary>
		/// <param name="samlEndpointURL"></param>
		/// <param name="personaId"></param>
		/// <param name="samlList"></param>
		/// <returns>OneSite PMCURL</returns>
		private string GetOneSitePMCURL(string samlEndpointURL, long personaId, IList<SamlAttributes> samlList)
		{
			string pmcID = "";
			PMCInfo pmcInfo = new PMCInfo();

			if (samlList.Any(a => a.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase)))
			{
				SamlAttributes sa = (from a in samlList
									 where a.Name.ToUpper() == "USERID"
									 select a).FirstOrDefault();
				if (sa != null)
				{
					pmcID = sa.Value.Split('|')[0];
					var _manageProductOneSite = new ManageProductOneSite(_userClaims);
					pmcInfo = _manageProductOneSite.GetPMCURL(personaId);
					if (pmcInfo?.PMCURL != null)
					{
						string pmcURL = pmcInfo.PMCURL;
						Uri samlUrl = new Uri(samlEndpointURL);

						pmcInfo.PMCURL = samlUrl.Scheme + "://" + pmcURL + samlUrl.PathAndQuery;
					}
					return pmcInfo.PMCURL;
				}
			}

			return samlEndpointURL;
		}

		/// <summary>
		/// Get Document Management DomainURL
		/// </summary>
		/// <param name="samlEndpointURL">samlEndpointURL</param>
		/// <param name="personaId">personaId</param>
		/// <param name="samlList">samlList</param>
		/// <returns>Document Management DomainURL</returns>
		private string GetDocManagementDomainURL(string samlEndpointURL, long personaId, IList<SamlAttributes> samlList)
		{
			ListResponse domainInfoResult = new ListResponse();
			var _manageProductRPDocumentManagement = new ManageProductRPDocumentManagement(_userClaims);

			if (samlList.Any(a => a.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase)))
			{
				SamlAttributes sa = (from a in samlList
									 where a.Name.ToUpper() == "USERID"
									 select a).FirstOrDefault();
				if (sa != null)
				{
					domainInfoResult = _manageProductRPDocumentManagement.GetDomain(personaId);
					if (domainInfoResult.Additional != null)
					{
						samlEndpointURL = samlEndpointURL.Replace("{{domain}}", domainInfoResult.Additional.ToString());
					}

					return samlEndpointURL;
				}
			}

			return samlEndpointURL;
		}

		/// <summary>
		/// Used to get a certificate from the certificate store used for signing the SAML assertion
		/// </summary>
		/// <param name="thumbprint"></param>
		/// <returns>X.509 Signing Certificate</returns>
		private X509Certificate2 GetSigningCertificate(string thumbprint)
		{
			var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly | OpenFlags.IncludeArchived);
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

            WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "GetSigningCertificate", $"No certificate specified or found for thumbprint {thumbprint}" });
			throw new Exception("No certificate specified or found for " + thumbprint);
		}

		/// <summary>
		/// Used to get product specific internal settings
		/// </summary>
		/// <param name="ProductId">The product id to get settings for</param>
		/// <returns>List of Product Internal Setting</returns>
		private List<ProductInternalSetting> GetProductInternalSettings(int ProductId)
		{
			IManageProduct manageProduct = new ManageProduct(_userClaims);
			var productInternalSettings = manageProduct.GetProductInternalSettings(ProductId);

			return productInternalSettings;
		}

		/// <summary>
		/// Used to add the xml namespace to the prefix of the node
		/// </summary>
		/// <param name="xn"></param>
		/// <param name="prefix"></param>
		private void AddPrefix(XmlNode xn, string prefix)
		{
			if (xn.HasChildNodes)
			{
				for (int i = 0; i < xn.ChildNodes.Count; i++)
				{
					AddPrefix(xn.ChildNodes[i], prefix);
				}
				xn.Prefix = prefix;
			}
			else
			{
				xn.Prefix = prefix;
			}
		}

		/// <summary>
		/// The SAML version
		/// </summary>
		public const string Version = "2.0";

		/// <summary>
		/// The Assertion URI
		/// </summary>
		public const string AssertionUri = "urn:oasis:names:tc:SAML:2.0:assertion";

		/// <summary>
		/// The Password URI
		/// </summary>
		public const string PasswordUri = "urn:oasis:names:tc:SAML:2.0:ac:classes:Password";

        public const string PasswordProtected = "urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport";

        /// <summary>
        /// The SAML XML prefixes
        /// </summary>
        public static class Prefixes
		{
			/// <summary>
			/// The SAML prefix
			/// </summary>
			public const string SAML = "saml";

			/// <summary>
			/// The SAML protocol prefix
			/// </summary>
			public const string SAMLP = "samlp";

			/// <summary>
			/// The SAML metadata prefix
			/// </summary>
			public const string MD = "md";
		}

		/// <summary>
		/// The SAML XML namespace URIs
		/// </summary>
		public static class NamespaceURIs
		{
			/// <summary>
			/// The SAML assertion namespace URI
			/// </summary>
			public const string Assertion = "urn:oasis:names:tc:SAML:2.0:assertion";

			/// <summary>
			/// The SAML protocol namespace URI
			/// </summary>
			public const string Protocol = "urn:oasis:names:tc:SAML:2.0:protocol";

			/// <summary>
			/// The SAML metadata namespace URI
			/// </summary>
			public const string Metadata = "urn:oasis:names:tc:SAML:2.0:metadata";

			/// <summary>
			/// The SAML encryption signature namespage
			/// </summary>
			public const string Signature = "http://www.w3.org/2000/09/xmldsig#";
		}
		public static class AttributeURIs
		{
			/// <summary>
			/// The SAML basic attribute URI
			/// </summary>
			public const string Basic = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic";
			/// <summary>
			/// The SAML dotnet attribute URI
			/// </summary>
			public const string DotNet = "urn:oasis:names:tc:SAML:2.0:attrname-format:dotnet";

		}

		public static class StatusUris
		{
			/// <summary>
			/// The SAML status Success URI
			/// </summary>
			public const string Success = "urn:oasis:names:tc:SAML:2.0:status:Success";
		}

		public static class NameIDFormatUris
		{
			/// <summary>
			/// The SAML NameID format unspecified URI
			/// </summary>
			public const string Unspecified = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified";
			/// <summary>
			/// The SAML NameID format email URI
			/// </summary>
			public const string Email = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";
		}

		public static class Algorithms
		{
			/// <summary>
			/// The SAML SHA1 signature method URI
			/// </summary>
			public const string SHA1_SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
			/// <summary>
			/// The SAML SHA1 digest method URI
			/// </summary>
			public const string SHA1_DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
		}

		/// <summary>
		/// SAMLResponse
		/// </summary>
		public class SAMLResponse
		{
			/// <summary>
			/// RelayState
			/// </summary>
			public string RelayState { get; set; } = "";

			/// <summary>
			/// SAML Destination URL
			/// </summary>
			public string Destination { get; set; } = "";

			/// <summary>
			/// Base64 encoded text of the SAML
			/// </summary>
			public string SAMLBase64Encoded { get; set; } = "";
		}

		public class ProductLoginResponse
		{
			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public string RedirectUrl { get; set; }
			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public SAMLResponse SamlResponse { get; set; }
			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public string ErrorMessage { get; set; } = null;

			public bool IsSAML { get; set; } = false;
			public bool IsRedirect { get; set; } = false;
            public string AccessToken { get; set; }

        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            string correlationId = "";
            if (_userClaims != null)
            {
                correlationId = (_userClaims.CorrelationId != Guid.Empty) ? _userClaims.CorrelationId.ToString() : "";
            }

            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Newtonsoft.Json.Formatting.Indented), false);
            }

            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId);

            logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
        }
    }
}
