CREATE PROCEDURE [Auth].[IdentityResourceInsert]
(
	@Name nvarchar(200),
    @DisplayName nvarchar(200),
    @Description nvarchar(1000),
    @Enabled bit,
    @Required bit,
    @Emphasize bit,
    @ShowInDiscoveryDocument bit
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[IdentityResources] 
		(
          [Enabled]
          ,[Name]
          ,[DisplayName]
          ,[Description]
          ,[Required]
          ,[Emphasize]
          ,[ShowInDiscoveryDocument]
          ,[Created]
          ,[NonEditable]
		) 
	VALUES (
		@Enabled
        , @Name
        , @DisplayName
        , @Description
        , @Required
        , @Emphasize
        , @ShowInDiscoveryDocument
        , GETUTCDATE()
        , 0
	);
	
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
		(Id = SCOPE_IDENTITY())
END

