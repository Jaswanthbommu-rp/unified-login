CREATE PROCEDURE [Auth].[SearchIdentityResources]
AS
BEGIN
    SELECT 
           [Id]
          ,[Enabled]
          ,[Name]
          ,[DisplayName]
          ,[Description]
          ,[Required]
          ,[Emphasize]
          ,[ShowInDiscoveryDocument]
          ,[Created]
          ,[Updated]
          ,[NonEditable]
      FROM [Auth].[IdentityResources]
END

GO

