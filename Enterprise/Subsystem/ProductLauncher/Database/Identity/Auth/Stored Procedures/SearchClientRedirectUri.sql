CREATE PROCEDURE [Auth].SearchClientRedirectUri (  
  @ClientRedirectUriId INT = NULL   
 ,@ClientId BIGINT = NULL   
 ,@Uri NVARCHAR(4000) = NULL   
)  
  
AS  
  
BEGIN  
  
 SELECT  
   [ClientRedirectUriId]  
  ,[ClientId]  
  ,[Uri]  
 FROM  
  [Auth].[ClientRedirectUris]  
 WHERE   
  (@ClientRedirectUriId IS NULL  OR  [ClientRedirectUriId] = @ClientRedirectUriId)  
 AND  
  (@ClientId IS NULL  OR  [ClientId] = @ClientId)  
 AND  
  (@Uri IS NULL OR [Uri] = @Uri OR CHARINDEX(@Uri,[Uri]) > 0)  
  
END