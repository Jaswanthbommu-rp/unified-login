CREATE PROCEDURE [Auth].SearchClientSecret (  
	@ClientId BIGINT = NULL 
)  
  
AS  
  
BEGIN  
	SELECT [Id]
		  ,[ClientId]
		  ,[Description]
		  ,[Value]
		  ,[Expiration]
		  ,[Type]
		  ,[Created]
	  FROM [Auth].[ClientSecrets]
	WHERE   
	  ClientId = @ClientId
END