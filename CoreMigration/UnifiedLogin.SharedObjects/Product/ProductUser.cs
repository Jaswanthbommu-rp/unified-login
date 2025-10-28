using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Swagger;

namespace UnifiedLogin.SharedObjects.Product
{
    /// <summary>
    /// Used to determine if the product user is associated to the object
    /// </summary>
    public class ProductUser : IProductUser
    {
        /// <summary>
        /// The id of the product user
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// The login name of the product user
        /// </summary>
        public string UserLogin { get; set; }

        /// <summary>
        /// The users first name
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }

        /// <summary>
        /// The users last name
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }

        /// <summary>
        /// The name of the product user
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// If the user is assigned to the object
        /// </summary>
        public bool IsAssigned { get; set; }

        #region Examples

        /// <summary>
        /// Used to document examples of the User Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class UserExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Role example</returns>
            public object GetExamples()
            {
                IList<ProductUser> list = new List<ProductUser>();

                list.Add(new ProductUser()
                {
                    UserId = 1234,
                    UserLogin = "username",
                    UserName = "test user",
                    IsAssigned = true
                });
                list.Add(new ProductUser()
                {
                    UserId = 776,
                    UserLogin = "differntuser",
                    UserName = "anouth user",
                    IsAssigned = false
                });

                ListResponse output = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = 1000,
                    TotalPages = 1
                };
                Dictionary<string, string> additionalInfo = new Dictionary<string, string>();
                additionalInfo.Add("somekey", "somevalue");
                output.Additional = additionalInfo;
                return output;
            }
        }
        #endregion
    }
}
