namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    public class ProductInternalSettingByType : ProductInternalSetting
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string BooksProductCode { get; set; }

        public bool ProductActive { get; set; }
    }
}
