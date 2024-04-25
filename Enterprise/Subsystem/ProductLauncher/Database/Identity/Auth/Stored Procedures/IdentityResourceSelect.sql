CREATE PROCEDURE [Auth].[IdentityResourceSelect]
(
    @Id INT = 0
    ,@Name NVARCHAR(200) = NULL
)
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
      WHERE
      (@Id = 0 OR [Id] = @Id)
		  AND 
      ((@Name = '' OR @Name IS NULL) OR [Name] LIKE '%' + @Name + '%')
END
GO
