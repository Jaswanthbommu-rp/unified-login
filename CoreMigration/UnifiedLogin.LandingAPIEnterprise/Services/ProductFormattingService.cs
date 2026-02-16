using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.LandingAPIEnterprise.Services
{
    /// <summary>
    /// Service for converting and formatting product data for UI consumption
    /// </summary>
    public interface IProductFormattingService
    {
        Task<List<UserProducts>> ConvertDashboardProductsToRAUL(IList<PersonaProductUserDetails> products, IManageProduct manageProduct);
        Task<List<UserProducts>> ConvertPersonaProductsToRAUL(IList<PersonaProduct> products, long personaId, IManageProduct manageProduct);
        Task<List<UserProducts>> ConvertDashboardProductsToRAULv2(IList<PersonaProductUserDetails> products, long personaId, IManageProduct manageProduct);
    }

    public class ProductFormattingService : IProductFormattingService
    {
        public async Task<List<UserProducts>> ConvertDashboardProductsToRAUL(IList<PersonaProductUserDetails> products, IManageProduct manageProduct)
        {
            var productIconSettings = manageProduct.GetProductSettingByType("ProductIcon");
            var productList = new List<UserProducts>();

            foreach (var prodDetail in products.Where(p => !string.IsNullOrEmpty(p.ProductUrl)))
            {
                var userProduct = CreateUserProduct(prodDetail, productIconSettings.ToList(), null);
                productList.Add(userProduct);
            }

            return productList.OrderBy(p => p.FamilyName).ThenBy(p => p.Name).ToList();
        }

        public async Task<List<UserProducts>> ConvertPersonaProductsToRAUL(IList<PersonaProduct> products, long personaId, IManageProduct manageProduct)
        {
            var productIconSettings = manageProduct.GetProductSettingByType("ProductIcon");
            var productList = new List<UserProducts>();

            foreach (var prodDetail in products)
            {
                var userProduct = new UserProducts
                {
                    Id = prodDetail.ProductId,
                    Name = prodDetail.Name,
                    Description = prodDetail.Description,
                    Url = BuildProductUrl(prodDetail.Url, prodDetail.ProductId, personaId),
                    Label = productIconSettings.FirstOrDefault(f => f.ProductId == prodDetail.ProductId)?.Value,
                    FamilyId = prodDetail.FamilyId,
                    FamilyName = prodDetail.FamilyName,
                    IsFavorite = prodDetail.isFavorite,
                    IsNewTab = prodDetail.IsNewTab,
                    IsResource = prodDetail.IsResource,
                    Status = prodDetail.StatusTypeId,
                    ProductCode = prodDetail.BooksProductCode,
                    ShowInAppSwitcher = prodDetail.ShowInAppSwitcher
                };
                productList.Add(userProduct);
            }

            return productList.OrderBy(p => p.FamilyName).ThenBy(p => p.Name).ToList();
        }

        public async Task<List<UserProducts>> ConvertDashboardProductsToRAULv2(IList<PersonaProductUserDetails> products, long personaId, IManageProduct manageProduct)
        {
            var productIconSettings = manageProduct.GetProductSettingByType("ProductIcon");
            var productList = new List<UserProducts>();

            foreach (var prodDetail in products.Where(p => !string.IsNullOrEmpty(p.ProductUrl)))
            {
                var userProduct = CreateUserProduct(prodDetail, productIconSettings.ToList(), personaId);
                productList.Add(userProduct);
            }

            return productList.OrderBy(p => p.FamilyName).ThenBy(p => p.Name).ToList();
        }

        #region Private Helper Methods

        private UserProducts CreateUserProduct(PersonaProductUserDetails prodDetail, List<ProductInternalSettingByType> iconSettings, long? personaId)
        {
            return new UserProducts
            {
                Id = prodDetail.ProductId,
                Name = prodDetail.ProductName,
                Description = prodDetail.ProductDescription,
                Url = BuildProductUrl(prodDetail.ProductUrl, prodDetail.ProductId, personaId),
                Label = iconSettings.FirstOrDefault(f => f.ProductId == prodDetail.ProductId)?.Value,
                FamilyId = prodDetail.FamilyId,
                FamilyName = prodDetail.Family,
                IsFavorite = prodDetail.IsFavorite,
                IsNewTab = prodDetail.IsNewTab,
                IsResource = prodDetail.IsResource,
                Status = prodDetail.ProductStatus,
                ProductCode = ((ProductEnum)prodDetail.ProductId).ToEnumDescription()
                //PersonaId = prodDetail.PersonaId
            };
        }

        private string BuildProductUrl(string productUrl, int productId, long? personaId)
        {
            if (string.IsNullOrEmpty(productUrl))
                return productUrl;

            if (productUrl.ToUpper().Contains("HTTP"))
                return productUrl;

            var landingUri = ConfigReader.GetLandingUri;

            if (personaId.HasValue && personaId > 0)
                return landingUri + $"product-redirect.html?prod={productId}&persona={personaId}";

            return landingUri + productUrl;
        }

        #endregion
    }
}
