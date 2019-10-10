namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models
{
    public interface IPortfolioProductUserDetails
    {
        string ClientName { get; set; }
        int PortfolioId { get; set; }
        string PortfolioName { get; set; }
        int PortfolioProductUserId { get; set; }
        string ProductName { get; set; }
        int TotalAccounts { get; set; }
        int UserId { get; set; }
        string Title { get; set; }
    }
}