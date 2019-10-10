using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models
{
    public class ProductViewModel
    {
        public IList<PortfolioProductUserDetails> ProductList { get; set; }
    }
}