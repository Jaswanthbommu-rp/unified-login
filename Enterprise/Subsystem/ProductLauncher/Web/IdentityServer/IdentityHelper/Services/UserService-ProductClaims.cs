using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Script.Serialization;
using RPModel = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services
{
    public partial class UserService
    {
        private IEnumerable<Claim> GetClaimsFromUnifiedAmenities(Persona persona)
        {
            var claims = new List<Claim>();

            var roles = _userRoleManager.GetProductRolesByPersona(persona.PersonaId, ProductEnum.UnifiedAmenities);
            if (roles != null)
            {
                claims.AddRange(roles.Select(a => new Claim("role", a.Name)).ToList());
                claims.AddRange(roles.Select(a => new Claim("roleId", a.ID)).ToList());
                claims.AddRange(roles.Select(a => new Claim("rolealias", a.Alias)).ToList());
            }

            foreach (var productRole in roles)
            {
                var roleRights = _userRoleManager.ListRightsByRole(persona.OrganizationPartyId, persona.Organization.RealPageId, ProductEnum.UnifiedAmenities, Convert.ToInt32(productRole.ID));
                claims.AddRange(roleRights.Select(a => new Claim("right", a.Alias)).ToList());
            }

            return claims;
        }

        private Claim GetSamlUserClaimAndAttributesForProduct(string claimType, string samlAttrName, long personaId, ProductEnum product, out IList<SamlAttributes> samlAttributes)
        {
            var samlProductSettings = GetSamlSettingsForProduct(product);
            if (!string.IsNullOrEmpty(samlProductSettings.LoginUri))
            {
                samlAttributes = GetSamlProductAttributesForPersona(personaId, product);
                if (samlAttributes.Any())
                {
                    // check the saml attribute name to see if it needs to be parsed
                    bool splitSamlValue = false;
                    int splitIndex = 0;
                    char splitChar = new char();

                    if (samlAttrName.Contains("~"))
                    {
                        splitSamlValue = true;
                        var parseSamlAttribute = samlAttrName.Split('~');
                        splitChar = parseSamlAttribute[1].Substring(0, 1)[0];
                        splitIndex = Convert.ToInt16(parseSamlAttribute[1].Substring(1, parseSamlAttribute[1].Length - 1));
                        samlAttrName = parseSamlAttribute[0];
                    }
                    var samlValue = (from saml in samlAttributes where saml.Name == samlAttrName select saml.Value).FirstOrDefault();

                    if (splitSamlValue && !string.IsNullOrEmpty(samlValue))
                    {
                        samlValue = samlValue.Split(splitChar)[splitIndex];
                    }
                    return new Claim(claimType, samlValue);
                }
            }

            samlAttributes = null;
            return null;
        }

        /// <summary>
        /// Not currently used
        /// </summary>
        /// <param name="primaryOrgPartyId"></param>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private IEnumerable<Claim> GetPortfolioProductUserClaims(long primaryOrgPartyId, string clientId, long userId)
        {
            var claims = new List<Claim>();
            var orgClientClaims = new List<RPModel.PortfolioProductUserClaims>();

            if (orgClientClaims == null) return claims;

            foreach (var orgCliClaim in orgClientClaims)
            {
                var value = orgCliClaim.Value;
                if (value.Contains("{") && value.Contains("}"))
                {
                    var serializer = new JavaScriptSerializer();
                    var tokenObj = serializer.Deserialize<Dictionary<string, object>>(orgCliClaim.Value);
                    foreach (var entry in tokenObj)
                    {
                        claims.Add(new Claim(entry.Key, entry.Value.ToString()));
                    }
                }
                else
                {
                    claims.Add(new Claim(orgCliClaim.Type, orgCliClaim.Value));
                }
            }

            return claims;
        }

        private IEnumerable<Claim> GetOrganizationClaims(RPModel.Organization organization)
        {
            var claims = new List<Claim>
            {
                new Claim("orgPartyId", organization.PartyId.ToString()),
                new Claim("orgId", organization.RealPageId.ToString()),
                new Claim("orgName", organization.Name),
                new Claim("orgMasterId", organization.BooksMasterId.ToString()),
                new Claim("orgCompanyMasterId", organization.BooksCustomerMasterId.ToString()),
                new Claim("orgType", organization.organizationType.Name),
                new Claim("orgDomain", organization.OrganizationDomain.Name)
            };
            return claims;
        }

        private static IEnumerable<Claim> GetClaimsFromProductApiEndpoint(string apiUrl, string accessToken, string nwpUserId)
        {
            var claims = new List<Claim>();
            ClaimResponse nwpClaims = new ClaimResponse() { Claims = new List<Services.RPClaim>() };
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    var response = client.GetAsync(apiUrl).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;

                        // try to reuse this once we can support newer c# versions
                        //if (JsonConvert.DeserializeObject(jsonContent, typeof(ClaimResponse)) is ClaimResponse claimResponse)
                        //    claims.AddRange(claimResponse.Claims.Select(a => new Claim(a.Type, a.Value)).ToList());
                        nwpClaims = JsonConvert.DeserializeObject(jsonContent, typeof(ClaimResponse)) as ClaimResponse;
                    }
                    nwpClaims.Claims.Add(new RPClaim() { Type = "productuserid", Value = nwpUserId });
                }
            }
            catch (Exception)
            {
                // ignored
            }
            if (nwpClaims.Claims.Count > 0)
            {
                claims.AddRange(nwpClaims.Claims.Select(a => new Claim(a.Type, a.Value)).ToList());
            }
            return claims;
        }

        private IList<SamlAttributes> GetSamlProductAttributesForPersona(long personaId, ProductEnum product)
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"samlAttributesCache_{personaId}_{(int)product}";
            IList<SamlAttributes> samlAttributes = rpCache.GetFromCache<IList<SamlAttributes>>(cacheKey, 600, () =>
            {
                return _samlRepository.GetProductSamlDetails(personaId, (int)product);
            });

            return samlAttributes;
        }

        private RPModel.ProductSamlSettings GetSamlSettingsForProduct(ProductEnum product)
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"samlProductSetting_{(int)product}";

            RPModel.ProductSamlSettings samlProductSettings = rpCache.GetFromCache<RPModel.ProductSamlSettings>(cacheKey, 600, () =>
            {
                return _samlRepository.GetProductSamlSettingsByProductId((int)product);
            });

            return samlProductSettings;
        }
    }
}
