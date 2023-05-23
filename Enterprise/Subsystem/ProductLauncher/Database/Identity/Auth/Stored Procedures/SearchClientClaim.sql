CREATE PROCEDURE [Auth].SearchClientClaim (  
	@ClientId INT = NULL   
)  
  
AS  
  
BEGIN  
	SELECT [Id]
		  ,[Type]
		  ,[Value]
		  ,[ClientId]
	  FROM [Auth].[ClientClaims]
	  WHERE
      	  ClientId = @ClientId
  
END