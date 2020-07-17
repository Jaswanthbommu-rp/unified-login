CREATE PROCEDURE [Auth].SearchClientPostLogoutRedirectUri (  
  @ClientPostLogoutRedirectUriId INT = NULL   
 ,@ClientId BIGINT = NULL   
 ,@Uri NVARCHAR(4000) = NULL   
)  
  
AS  
  
BEGIN  
  
 SELECT  
   [ClientPostLogoutRedirectUriId]  
  ,[ClientId]  
  ,[Uri]  
 FROM  
  [Auth].[ClientPostLogoutRedirectUris]  
 WHERE   
  (@ClientPostLogoutRedirectUriId IS NULL  OR  [ClientPostLogoutRedirectUriId] = @ClientPostLogoutRedirectUriId)  
 AND  
  (@ClientId IS NULL  OR  [ClientId] = @ClientId)  
 AND  
  (@Uri IS NULL OR [Uri] = @Uri OR CHARINDEX(@Uri,[Uri]) > 0)  
  
END