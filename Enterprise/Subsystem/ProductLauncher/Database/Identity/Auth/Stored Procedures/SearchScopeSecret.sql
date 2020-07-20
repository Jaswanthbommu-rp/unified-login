CREATE PROCEDURE [Auth].SearchScopeSecret (  
  @ScopeSecretId INT = NULL   
 ,@ScopeId INT = NULL   
 ,@Description NVARCHAR(2000) = NULL   
 ,@Type NVARCHAR(500) = NULL   
 ,@Value NVARCHAR(500) = NULL   
 ,@Expiration DATETIMEOFFSET = NULL   
)  
  
AS  
  
BEGIN  
  
 SELECT  
   [ScopeSecretId]  
  ,[ScopeId]  
  ,[Description]  
  ,[Type]  
  ,[Value]  
  ,[Expiration]  
 FROM  
  [Auth].[ScopeSecrets]  
 WHERE   
  (@ScopeSecretId IS NULL  OR  [ScopeSecretId] = @ScopeSecretId)  
 AND  
  (@ScopeId IS NULL  OR  [ScopeId] = @ScopeId)  
 AND  
  (@Description IS NULL OR [Description] = @Description OR CHARINDEX(@Description,[Description]) > 0)  
 AND  
  (@Type IS NULL OR [Type] = @Type OR CHARINDEX(@Type,[Type]) > 0)  
 AND  
  (@Value IS NULL OR [Value] = @Value OR CHARINDEX(@Value,[Value]) > 0)  
 AND  
  (@Expiration IS NULL  OR  [Expiration] = @Expiration)  
  
END