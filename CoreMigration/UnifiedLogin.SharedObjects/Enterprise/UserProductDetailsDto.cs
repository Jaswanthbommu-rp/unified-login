#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    [ExcludeFromCodeCoverage]
    public class UserProductDetailsDto
    {
        public UserDataDto UserProfileDetails { get; set; } = new();
        public IList<ProductDetailDto> ProductList { get; set; } = new List<ProductDetailDto>();
    }
}
