IF OBJECT_ID('[Auth].[GetUserProductDetailsByUserId]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetUserProductDetailsByUserId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetUserProductDetailsByUserId]
	@userId		int 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	 

SELECT        Auth.Users.UserId, Auth.Users.LoginId, Auth.Users.Firstname, Auth.Users.LastName, Auth.Users.Title, Auth.Users.Email, Auth.Users.Phone, Auth.Portfolio.PortfolioName as companyname
FROM            Auth.Users INNER JOIN
                         Auth.UserProviderPortfolio ON Auth.Users.UserId = Auth.UserProviderPortfolio.UserId INNER JOIN
                         Auth.ProviderPortfolio ON Auth.UserProviderPortfolio.ProviderPortfolioId = Auth.ProviderPortfolio.ProviderPortfolioId INNER JOIN
                         Auth.Portfolio ON Auth.ProviderPortfolio.PortfolioIdId = Auth.Portfolio.PortfolioId
						 and Auth.Users.UserId = @userId 

END
GO
