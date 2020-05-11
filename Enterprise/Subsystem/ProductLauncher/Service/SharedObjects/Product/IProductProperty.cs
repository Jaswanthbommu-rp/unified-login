namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product
{
    public interface IProductProperty
    {
        /// <summary>
        /// The id of the property in the product
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// The name of the property in the product
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The first street address of the property in the product
        /// </summary>
        string Street1 { get; set; }

        /// <summary>
        /// The second street address of the property in the product
        /// </summary>
        string Street2 { get; set; }

        /// <summary>
        /// The city where the property is located
        /// </summary>
        string City { get; set; }

        /// <summary>
        /// The state where the property is located
        /// </summary>
        string State { get; set; }

        /// <summary>
        /// The zip code where the property is located
        /// </summary>
        string Zip { get; set; }

        /// <summary>
        /// Is the property assigned to the user
        /// </summary>
        bool? IsAssigned { get; set; }

        /// <summary>
        /// Is the property disabled to select by the user
        /// </summary>
        bool? disableSelection { get; set; }
    }
}
