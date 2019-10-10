IF OBJECT_ID('[Auth].[GetAllPortfolioProductUser]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetAllPortfolioProductUser];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetAllPortfolioProductUser]
	@PortfolioId  int,
    @UserId   bigint,
	@ProductId int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	;WITH ProductLogins ( UserId, ProductId, TotalAccounts ) 
	AS ( 
			SELECT 
				UserId
				, ProductId
				, COUNT(1) 
			FROM [Auth].[PortfolioProductUser] WITH(NOLOCK)
			WHERE 
				UserId = @UserId 
			GROUP BY 
			UserId, ProductId
		)

	SELECT 
		PPU.PortfolioProductUserID as PortfolioProductUserID
		,P.PortfolioId as PortfolioId
		,P.PortfolioName as PortfolioName
		,ISNULL(C.ClientCode,'') as ClientId
		,PR.ProductName as ProductName
		,PPU.UserID as UserId
		,PL.TotalAccounts as TotalAccounts
		,PPU.Title as Title
		,PR.ClassName
		,PR.SettingsUrl 
		,PR.ProductUrl	 
		,PR.SubDescription	 
		,PR.TitleId	 
		,PR.TitleUniqueId	 
		,PR.IsNewTab	 
		,PR.MetatagUniqueId	 

	FROM
		[Auth].[PortfolioProductUser] PPU WITH(NOLOCK)
		INNER JOIN [Auth].[Portfolio] P WITH(NOLOCK) ON P.PortfolioId = PPU.PortfolioID
		INNER JOIN [Auth].[PortfolioProduct] PP WITH(NOLOCK) ON P.PortfolioId = PP.PortfolioID AND PPU.ProductId = PP.ProductId
		INNER JOIN [Auth].[Product] PR WITH(NOLOCK) ON PP.ProductId = Pr.ProductId
		LEFT OUTER JOIN [Auth].[Clients] C WITH(NOLOCK) ON PR.ClientId = C.ClientId
		INNER JOIN ProductLogins PL ON PL.ProductId = PPU.ProductId
	WHERE
		P.PortfolioId = @PortfolioId
		AND
		PPU.UserId = @UserId
		AND
		1 = CASE WHEN @ProductId != 0 THEN CASE WHEN PR.ProductId = @ProductId THEN 1 ELSE 0 END ELSE 1 END

END
GO
