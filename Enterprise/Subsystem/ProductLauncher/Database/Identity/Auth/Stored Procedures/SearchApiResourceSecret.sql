CREATE PROCEDURE [Auth].SearchApiResourceSecret (  
    @ApiResourceId INT = NULL   
)  
  
AS  
  
BEGIN  

    SELECT [Id]
          ,[ApiResourceId]
          ,[Description]
          ,[Value]
          ,[Expiration]
          ,[Type]
          ,[Created]
    FROM 
      [Auth].[ApiResourceSecrets]
    WHERE
        @ApiResourceId IS NULL OR ApiResourceId = @ApiResourceId
END