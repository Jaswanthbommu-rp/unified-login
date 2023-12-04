CREATE PROCEDURE [Auth].[ApiResourceUpdate]
(
	@Id INT,
	@Enabled bit,
    @Name nvarchar(200),
    @DisplayName nvarchar(200),
    @Description nvarchar(1000),
    @AllowedAccessTokenSigningAlgorithms nvarchar(100),
    @ShowInDiscoveryDocument bit,
    @RequireResourceIndicator BIT,

	@Original_Enabled bit,
	@Original_Name nvarchar(200),
	@IsNull_DisplayName Int,
	@Original_DisplayName nvarchar(200),
	@IsNull_Description Int,
	@Original_Description nvarchar(1000),
	@IsNull_AllowedAccessTokenSigningAlgorithms Int,
	@Original_AllowedAccessTokenSigningAlgorithms nvarchar(100),
	@Original_ShowInDiscoveryDocument BIT,
	@Original_RequireResourceIndicator bit
)
AS
BEGIN
	SET NOCOUNT OFF;
	
	UPDATE [Auth].[ApiResources]
	  SET 
		   [Enabled] = @Enabled
		  ,[Name] = @Name
		  ,[DisplayName] = @DisplayName
		  ,[Description] = @Description
		  ,[AllowedAccessTokenSigningAlgorithms] = @AllowedAccessTokenSigningAlgorithms
		  ,[RequireResourceIndicator] = @RequireResourceIndicator
		  ,[ShowInDiscoveryDocument] = @ShowInDiscoveryDocument
		  ,[Updated] = GETUTCDATE()

	WHERE 
		(([Id] = @Id) 
			AND ([Name] = @Original_Name) 
			AND ((@IsNull_DisplayName = 1 AND [DisplayName] IS NULL) OR ([DisplayName] = @Original_DisplayName)) 
			AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
			AND ([Enabled] = @Original_Enabled) 
			AND ((@IsNull_AllowedAccessTokenSigningAlgorithms = 1 AND [AllowedAccessTokenSigningAlgorithms] IS NULL) OR ([AllowedAccessTokenSigningAlgorithms] = @Original_AllowedAccessTokenSigningAlgorithms))
			AND ([ShowInDiscoveryDocument] = @Original_ShowInDiscoveryDocument)
			AND ([RequireResourceIndicator] = @Original_RequireResourceIndicator)
		)	

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
		Id = @Id
END