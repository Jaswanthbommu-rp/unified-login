#nullable enable
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class UserProductDetailsDto
    {
        public UserDataDto UserProfileDetails { get; set; } = new();
        public IList<ProductDetailDto> ProductList { get; set; } = new List<ProductDetailDto>();
    }
}
