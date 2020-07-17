CREATE PROCEDURE [Auth].SearchClientClaim (  
  @ClientClaimsId INT = NULL   
 ,@ClientId INT = NULL   
 ,@Type NVARCHAR(200) = NULL   
 ,@Value NVARCHAR(200) = NULL   
)  
  
AS  
  
BEGIN  
  
 SELECT  
   [ClientClaimsId]  
  ,[ClientId]  
  ,[Type]  
  ,[Value]  
 FROM  
  [Auth].[ClientClaims]  
 WHERE   
  (@ClientClaimsId IS NULL  OR  [ClientClaimsId] = @ClientClaimsId)  
 AND  
  (@ClientId IS NULL  OR  [ClientId] = @ClientId)  
 AND  
  (@Type IS NULL OR [Type] = @Type OR CHARINDEX(@Type,[Type]) > 0)  
 AND  
  (@Value IS NULL OR [Value] = @Value OR CHARINDEX(@Value,[Value]) > 0)  
  
END