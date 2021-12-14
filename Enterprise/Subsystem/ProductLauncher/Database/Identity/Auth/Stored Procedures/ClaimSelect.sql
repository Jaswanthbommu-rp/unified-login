CREATE PROCEDURE [Auth].[ClaimSelect]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT          
    ClaimId  
  , ClaimName  
  , SAMLAttributeName  
  , c.ProductId
  , p.Name as ProductName
 FROM   
  Auth.Claim  c
  JOIN Enterprise.Product p on P.ProductId = c.ProductId
END