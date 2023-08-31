CREATE PROCEDURE [Auth].[ApiScopesUpdate]
(
	@Id INT,
	@Enabled bit,
	@Name nvarchar(200),
	@DisplayName nvarchar(200),
	@Description nvarchar(1000),
	@Required bit,
	@Emphasize bit,
	@ShowInDiscoveryDocument bit,

	@Original_Enabled bit,
	@Original_Name nvarchar(200),
	@IsNull_DisplayName Int,
	@Original_DisplayName nvarchar(200),
	@IsNull_Description Int,
	@Original_Description nvarchar(1000),
	@Original_Required bit,
	@Original_Emphasize bit,
	@Original_ShowInDiscoveryDocument bit
)
AS
BEGIN
	SET NOCOUNT OFF;
	
	IF @Original_Name <> @Name
	BEGIN
		-- UPDATE ANY CLIENTS WITH THE OLD NAME TO THE NEW NAME
		UPDATE Auth.ClientScopes
			SET Scope = @Name
		WHERE
			Scope = @Original_Name
	END

	UPDATE [Auth].[ApiScopes] 
	SET 
		  [Enabled] = @Enabled
	    , [Name] = @Name
		, [DisplayName] = @DisplayName
		, [Description] = @Description
		, [Required] = @Required
		, [Emphasize] = @Emphasize
		, [ShowInDiscoveryDocument] = @ShowInDiscoveryDocument
	WHERE 
		(([Id] = @Id) 
			AND ([Name] = @Original_Name) 
			AND ((@IsNull_DisplayName = 1 AND [DisplayName] IS NULL) OR ([DisplayName] = @Original_DisplayName)) 
			AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
			AND ([Enabled] = @Original_Enabled) 
			AND ([Required] = @Original_Required) 
			AND ([Emphasize] = @Original_Emphasize) 
			AND ([ShowInDiscoveryDocument] = @Original_ShowInDiscoveryDocument) 
		)	

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
		Id = @Id
END
