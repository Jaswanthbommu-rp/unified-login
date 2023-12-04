CREATE PROCEDURE [Auth].[ApiResourceSelect]
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
END

GO
