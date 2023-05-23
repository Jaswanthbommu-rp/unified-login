CREATE PROCEDURE [Auth].SearchClientPostLogoutRedirectUri (  
	@ClientId BIGINT = NULL   

)  
  
AS  
  
BEGIN  
	SELECT [Id]
		  ,[PostLogoutRedirectUri]
		  ,[ClientId]
	  FROM [Auth].[ClientPostLogoutRedirectUris]
	WHERE
		ClientId = @ClientId
END