using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models
{
    public class PortfolioProductUserDetails : IPortfolioProductUserDetails
    {
        public int PortfolioProductUserId { get; set; } = 0;
        public int PortfolioId { get; set; } = 0;
        public string PortfolioName { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int UserId { get; set; } = 0;
        public string Title { get; set; } = "";
        public int TotalAccounts { get; set; } = 0;

    }
}