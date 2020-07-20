CREATE PROCEDURE [Auth].SearchClientSecret (  
  @ClientSecretId INT = NULL   
 ,@ClientId BIGINT = NULL   
 ,@Value NVARCHAR(500) = NULL   
 ,@Type NVARCHAR(500) = NULL   
 ,@Description NVARCHAR(4000) = NULL   
 ,@Expiration DATETIMEOFFSET = NULL   
)  
  
AS  
  
BEGIN  
  
 SELECT  
   [ClientSecretId]  
  ,[ClientId]  
  ,[Value]  
  ,[Type]  
  ,[Description]  
  ,[Expiration]  
 FROM  
  [Auth].[ClientSecrets]  
 WHERE   
  (@ClientSecretId IS NULL  OR  [ClientSecretId] = @ClientSecretId)  
 AND  
  (@ClientId IS NULL  OR  [ClientId] = @ClientId)  
 AND  
  (@Value IS NULL OR [Value] = @Value OR CHARINDEX(@Value,[Value]) > 0)  
 AND  
  (@Type IS NULL OR [Type] = @Type OR CHARINDEX(@Type,[Type]) > 0)  
 AND  
  (@Description IS NULL OR [Description] = @Description OR CHARINDEX(@Description,[Description]) > 0)  
 AND  
  (@Expiration IS NULL  OR  [Expiration] = @Expiration)  
  
END