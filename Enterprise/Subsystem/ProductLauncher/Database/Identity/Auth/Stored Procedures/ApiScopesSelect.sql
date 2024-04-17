CREATE PROCEDURE [Auth].[ApiScopesSelect]
(
    @Id INT = 0
)
AS
BEGIN
    SELECT [Id]
          ,[Enabled]
          ,[Name]
          ,[DisplayName]
          ,[Description]
          ,[Required]
          ,[Emphasize]
          ,[ShowInDiscoveryDocument]
          ,[Created]
          ,[Updated]
          ,[LastAccessed]
          ,[NonEditable]
      FROM [Auth].[ApiScopes]
	  WHERE
		(@Id = 0 OR [Id] = @Id)
END

GO