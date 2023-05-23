CREATE PROCEDURE [Auth].SearchClientRedirectUri (  
  @ClientId BIGINT = NULL   
 )  
  
AS  
  
BEGIN  
	SELECT [Id]
		  ,[RedirectUri]
		  ,[ClientId]
	  FROM [Auth].[ClientRedirectUris]
	  WHERE
		ClientId = @ClientId
END
