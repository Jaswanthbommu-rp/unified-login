CREATE PROCEDURE [Auth].[ApiScopesSelect]
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
END

GO

