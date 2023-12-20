CREATE PROCEDURE [Auth].[ApiResourceSelect]
(
    @Id INT = 0
    ,@Name NVARCHAR(200) = NULL
)
AS
BEGIN
    SELECT [Id]
          ,[Enabled]
          ,[Name]
          ,[DisplayName]
          ,[Description]
          ,[AllowedAccessTokenSigningAlgorithms]
          ,[ShowInDiscoveryDocument]
          ,[RequireResourceIndicator]
          ,[Created]
          ,[Updated]
          ,[LastAccessed]
          ,[NonEditable]
      FROM [Auth].[ApiResources]
	  WHERE
      (@Id = 0 OR [Id] = @Id)
		  AND 
      ((@Name = '' OR @Name IS NULL) OR [Name] LIKE '%' + @Name + '%')
END
GO
