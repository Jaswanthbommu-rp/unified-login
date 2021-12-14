CREATE PROCEDURE [Auth].[ClaimSelect]  
AS  
BEGIN  
 SET NOCOUNT ON;  
 SELECT          
    c.ClaimId  
  , c.ClaimName  
  , c.SAMLAttributeName  
  , c.ProductId
  , p.Name as ProductName
  , (SELECT COUNT(1) FROM Auth.ClientUserClaim WHERE C.ClaimId = ClaimId) [UsedCount]
 FROM   
  Auth.Claim  c
  JOIN Enterprise.Product p on P.ProductId = c.ProductId
END