CREATE PROCEDURE [Auth].SearchClientScope (  
	@ClientId BIGINT = NULL
)  
  
AS  
BEGIN  
	SELECT [Id]
		  ,[Scope]
		  ,[ClientId]
	  FROM [Auth].[ClientScopes]
	  WHERE
		ClientId = @ClientId
END