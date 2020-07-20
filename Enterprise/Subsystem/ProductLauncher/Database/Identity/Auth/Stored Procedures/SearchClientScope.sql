CREATE PROCEDURE [Auth].SearchClientScope (  
  @ClientScopeId INT = NULL   
 ,@ClientId BIGINT = NULL   
 ,@Scope NVARCHAR(400) = NULL   
)  
  
AS  
  
BEGIN  
  
 SELECT  
   [ClientScopeId]  
  ,[ClientId]  
  ,[Scope]  
 FROM  
  [Auth].[ClientScopes]  
 WHERE   
  (@ClientScopeId IS NULL  OR  [ClientScopeId] = @ClientScopeId)  
 AND  
  (@ClientId IS NULL  OR  [ClientId] = @ClientId)  
 AND  
  (@Scope IS NULL OR [Scope] = @Scope OR CHARINDEX(@Scope,[Scope]) > 0)  
  
END