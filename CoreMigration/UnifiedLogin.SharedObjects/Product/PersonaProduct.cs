namespace UnifiedLogin.SharedObjects.Product
{
    public class PersonaProduct
    {
        public int ProductId { get; set; }
        public string Name   { get; set; }
        public string BooksProductCode{ get; set; }
        public bool isFavorite { get; set; }
        public int StatusTypeId { get; set; }
        public string Description { get; set; }
        public int FamilyId { get; set; }
        public string FamilyName { get; set; }
        public bool IsNewTab { get; set; }
        public bool IsResource { get; set; }
        public string Url { get; set; }
        public bool ShowInAppSwitcher { get;set; }

    }
}
