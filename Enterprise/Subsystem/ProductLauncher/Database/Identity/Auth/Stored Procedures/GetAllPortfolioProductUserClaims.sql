CREATE PROCEDURE [Auth].[GetAllPortfolioProductUserClaims]
	@PortfolioId  int,
    @ClientCode NVARCHAR (200),
    @UserId   bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		PPUC.PortfolioProductUserClaimsID as Id
		,P.PortfolioId as PortfolioId
		,C.ClientCode as ClientId
		,PPU.UserID as UserId
		,PPUC.Type as [Type]
		,PPUC.Value as [Value]
	FROM 
		[Auth].[PortfolioProductUser] PPU WITH(NOLOCK)
		INNER JOIN [Auth].[PortfolioProductUserClaims] PPUC WITH(NOLOCK) ON PPU.PortfolioProductUserID = PPUC.PortfolioProductUserID
		INNER JOIN [Auth].[Portfolio] P WITH(NOLOCK) ON P.PortfolioId = PPU.PortfolioID
		INNER JOIN [Auth].[PortfolioProduct] PP WITH(NOLOCK) ON PPU.PortfolioId = PP.PortfolioID
		INNER JOIN [Auth].[Product] PR WITH(NOLOCK) ON PP.ProductId = Pr.ProductId
		INNER JOIN [Auth].[Clients] C WITH(NOLOCK) ON PR.ClientId = C.ClientId
	WHERE
		PPU.PortfolioId = @PortfolioId
		AND
		C.ClientCode = @ClientCode
		AND
		PPU.UserId = @UserId

END
GO
