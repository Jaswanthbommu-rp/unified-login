CREATE PROCEDURE [Auth].ApiScopesInsert
(
	@Enabled bit,
	@Name nvarchar(200),
	@DisplayName nvarchar(200),
	@Description nvarchar(1000),
	@Required bit,
	@Emphasize bit,
	@ShowInDiscoveryDocument bit
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[ApiScopes] 
	(    
		Enabled,
		Name,
		DisplayName,
		Description,
		Required,
		Emphasize,
		ShowInDiscoveryDocument,
		Created,
		NonEditable
	)
	VALUES 
	(
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
    FROM 
		[Auth].[ApiScopes]
	WHERE 
		Id = SCOPE_IDENTITY()
END
