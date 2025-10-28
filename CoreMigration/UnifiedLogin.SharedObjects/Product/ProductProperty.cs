using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Ops;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnifiedLogin.SharedObjects.Swagger;

namespace UnifiedLogin.SharedObjects.Product
{
    /// <summary>
    /// Used to store information about a property
    /// </summary>
    public class ProductProperty
    {
        /// <summary>
        /// The id of the property in the product
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The name of the property in the product
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// The first street address of the property in the product
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Street1 { get; set; }

        /// <summary>
        /// The second street address of the property in the product
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Street2 { get; set; }

        /// <summary>
        /// The city where the property is located
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }

        /// <summary>
        /// The state where the property is located
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        /// <summary>
        /// The zip code where the property is located
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Zip { get; set; }

        /// <summary>
        /// Is the property assigned to the users
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAssigned { get; set; } = false;

        /// <summary>
        /// Is the property disabled to the users
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? disableSelection { get; set; } = false;

        /// <summary>
        /// Alias for the property
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }

        /// <summary>
        /// Is the property active
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Active { get; set; }

        private string _instanceId = null;

        /// <summary>
        /// The UPFM property instance id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string InstanceId
        {
            get => _instanceId;
            set => _instanceId = value.ToLower();
        }

        /// <summary>
        /// The geo latitude of the property
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Latitude { get; set; }

        /// <summary>
        /// The geo longitude of the property
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Longitude { get; set; }

        /// <summary>
        /// Customer property Id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CustomerPropertyId { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        #region Examples

        /// <summary>
        /// Used to document examples of the Property Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class PropertyExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Property example</returns>
            public object GetExamples()
            {
                IList<ProductProperty> list = new List<ProductProperty>();
                ProductProperty example = new ProductProperty()
                {
                    ID = "1234567",
                    Name = "Test Property",
                    Street1 = "Street 1",
                    Street2 = "Street 2",
                    City = "Some City",
                    State = "Some State",
                    Zip = "12345",
                    IsAssigned = true
                };
                list.Add(example);
                ListResponse output = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = 1,
                    RowsPerPage = 1000,
                    TotalPages = 1,
                };
                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the Property Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class PropertySimpleExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Property example</returns>
            public object GetExamples()
            {
                IList<ProductProperty> list = new List<ProductProperty>();
                ProductProperty example = new ProductProperty()
                {
                    ID = "123",
                    Name = "Test Property",
                    IsAssigned = true
                };
                list.Add(example);
                ListResponse output = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = 1,
                    RowsPerPage = 1000,
                    TotalPages = 1,
                };
                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the Property Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class CompanyPropertiesSimpleExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Property example</returns>
            public object GetExamples()
            {
                List<UserCompaniesProperties> userCompaniesProperties = new List<UserCompaniesProperties>
                {
                    new UserCompaniesProperties{
                    Id = "123-HA",
                    OrganizationName = "Test PMC",
                    InstanceId = new Guid("a1463003-9a65-4e21-8897-dc4c826fab31"),
                    Properties = new List<Properties> {
                    new Properties{ InstanceId = "e1463003-9a65-4e21-8897-dc4c826fab32",Id = "1234",PropertyName = "Test Property1"},
                    new Properties{ InstanceId = "f1463003-9a65-4e21-8897-dc4c826fab33",Id = "5678",PropertyName = "Test Property2"},
                    }
                    },
                    new UserCompaniesProperties{
                    Id = "456-HA",
                    OrganizationName = "Test PMC2",
                    InstanceId = new Guid("e1463003-9a65-4e21-8897-dc4c826fab34"),
                    Properties = new List<Properties> {
                    new Properties{ InstanceId = "e1463003-9a65-4e21-8897-dc4c826fab35",Id = "4321",PropertyName = "Test Property3"},
                    new Properties{ InstanceId = "f1463003-9a65-4e21-8897-dc4c826fab36",Id = "8765",PropertyName = "Test Property4"},
                    }
                    }
                };                
                return userCompaniesProperties;
            }
        }

        /// <summary>
        /// Used to document examples of the Property Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class GetOpsAssetsResponse : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Property example</returns>
            public object GetExamples()
            {
                IList<Portfolio> portfolioList = new List<Portfolio>()
                {
                    new Portfolio()
                    {
                        ID = "1321743",
                        Name = "Archstone Lexington",
                        Code = "TX527",
                        Status = "active",
                        ParentAssetId = null,
                        AssetType = null,
                        IsAssigned = false
                    },
                    new Portfolio()
                    {
                        ID = "1322948",
                        Name = "Archstone Redmond Lakeview",
                        Code = "WA539",
                        Status = "active",
                        ParentAssetId = null,
                        AssetType = null,
                        IsAssigned = false
                    }
                };
                ListResponse listResponse = new ListResponse()
                {
                    Records = portfolioList.Cast<object>().ToList(),
                    TotalRows = portfolioList.Count,
                    RowsPerPage = portfolioList.Count,
                    TotalPages = 1,
                    ErrorReason = string.Empty,
                    IsError = false
                };
                return listResponse;
            }
        }

        /// <summary>
        /// Used to document examples of the Property Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class GetOpsAssetGroupResponse : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Property example</returns>
            public object GetExamples()
            {
                IList<AssetGroup> assetGroupList = new List<AssetGroup>()
                {
                    new AssetGroup()
                    {
                        ID = "19984",
                        Name = "AVA Back Bay fna Avalon Prudential Center III",
                        Code = "MA040",
                        Description = string.Empty,
                        Status = "active",
                        GroupType = "property",
                        AssetID = "1316308",
                        IsAssigned = false
                    },
                    new AssetGroup()
                    {
                        ID = "19607",
                        Name = "AVA Ballard",
                        Code = "WA023",
                        Description = string.Empty,
                        Status = "active",
                        GroupType = "property",
                        AssetID = "1307920",
                        IsAssigned = false
                    }
                };
                ListResponse listResponse = new ListResponse()
                {
                    Records = assetGroupList.Cast<object>().ToList(),
                    TotalRows = assetGroupList.Count,
                    RowsPerPage = assetGroupList.Count,
                    TotalPages = 1,
                    ErrorReason = string.Empty,
                    IsError = false
                };
                return listResponse;
            }
        }

        /// <summary>
        /// Used to document examples of the Property Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class CreateOpsAssetGroupResponse : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Property example</returns>
            public object GetExamples()
            {
                IList<AssetGroup> assetGroupList = new List<AssetGroup>()
                {
                    new AssetGroup()
                    {
                        ID = "19984",
                        Name = "AVA Back Bay fna Avalon Prudential Center III"
                    }
                };
                ListResponse listResponse = new ListResponse()
                {
                    Records = assetGroupList.Cast<object>().ToList(),
                    TotalRows = assetGroupList.Count,
                    RowsPerPage = assetGroupList.Count,
                    TotalPages = 1,
                    ErrorReason = string.Empty,
                    IsError = false
                };
                return listResponse;
            }
        }

        /// <summary>
        /// Used to document examples of the Property Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class UpdateOpsAssetGroupResponse : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Property example</returns>
            public object GetExamples()
            {
                IList<AssetGroup> assetGroupList = new List<AssetGroup>()
                {
                    new AssetGroup()
                    {
                        ID = "19984",
                        Name = "AVA Back Bay fna Avalon Prudential Center III",
                        Code = "MA040",
                        Description = string.Empty,
                        Status = "active",
                        GroupType = "property",
                        AssetID = "1316308",
                        IsAssigned = false,
                        property_list = new List<Portfolio>()
                        {
                            new Portfolio()
                            {
                                ID = "1316308",
                                Name = "AVA Back Bay fna Avalon Prudential Center III",
                                Code = "MA040",
                                Status = "active",
                                ParentAssetId = null,
                                AssetType = new AssetType()
                                {
                                    Id = "2367",
                                    Name = "Property",
                                    SystemName = "Property"
                                }
                            }
                        }
                    }
                };
                ListResponse listResponse = new ListResponse()
                {
                    Records = assetGroupList.Cast<object>().ToList(),
                    TotalRows = assetGroupList.Count,
                    RowsPerPage = assetGroupList.Count,
                    TotalPages = 1,
                    ErrorReason = string.Empty,
                    IsError = false
                };
                return listResponse;
            }
        }

        #endregion
    }
}
