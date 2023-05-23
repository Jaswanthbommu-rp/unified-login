CREATE PROCEDURE [Auth].SearchClientGrantType (  
	@ClientId BIGINT = NULL   
)  
AS  
BEGIN
	SELECT [Id]
		  ,[GrantType]
		  ,[ClientId]
	  FROM [Auth].[ClientGrantTypes]
	WHERE
		ClientId = @ClientId
END

