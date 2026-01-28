using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    /// <summary>
    /// Details about the products assigned to the user
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UserProducts
    {
        /// <summary>
        /// Product id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the product
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The url to the product for login
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The product description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The icon for the product
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The Family id the product belongs to
        /// </summary>
        public int? FamilyId { get; set; }

        /// <summary>
        /// The Family the product belongs to
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Does the product open in a new window
        /// </summary>
        public bool IsNewTab { get; set; }

        /// <summary>
        /// Has the product been favourited
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Is the product a resource
        /// </summary>
        public bool IsResource { get; set; }

        /// <summary>
        /// /The status of the product, 7 errored, 8 success, 10 deleted
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Books product code
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// Should the application show in the app switcher
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowInAppSwitcher { get; set; }
    }
}
