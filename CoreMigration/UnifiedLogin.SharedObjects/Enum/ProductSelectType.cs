namespace UnifiedLogin.SharedObjects.Enum
{
    /// <summary>
    /// Type of products user 
    /// </summary>
    public enum ProductSelectType
    {
        /// <summary>
        /// Selects products including favorites
        /// </summary>
        ProductsWithFavorites = 1,

        /// <summary>
        /// Selects products that have been set as favorite
        /// </summary>
        FavoritesOnly = 2,

        /// <summary>
        /// Selects products that are resources only
        /// </summary>
        ResourcesOnly = 3
    }
}
