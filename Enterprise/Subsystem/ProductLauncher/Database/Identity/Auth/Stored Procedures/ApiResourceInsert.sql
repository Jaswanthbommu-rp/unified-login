CREATE PROCEDURE [Auth].[ApiResourceInsert]
(
    @Enabled bit,
    @Name nvarchar(200),
    @DisplayName nvarchar(200),
    @Description nvarchar(1000),
    @AllowedAccessTokenSigningAlgorithms nvarchar(100),
    @ShowInDiscoveryDocument bit,
    @RequireResourceIndicator BIT
)
AS
BEGIN
	SET NOCOUNT OFF;
	IF @AllowedAccessTokenSigningAlgorithms = ''
	BEGIN
		SET @AllowedAccessTokenSigningAlgorithms = NULL
	END
	
    INSERT INTO [Auth].[ApiResources]
        (
            [Enabled]
            ,[Name]
            ,[DisplayName]
            ,[Description]
            ,[AllowedAccessTokenSigningAlgorithms]
            ,[ShowInDiscoveryDocument]
            ,[RequireResourceIndicator]
            ,[Created]
            ,[NonEditable]
        )
        VALUES
        (
            @Enabled
            , @Name
            , @DisplayName
            , @Description
            , @AllowedAccessTokenSigningAlgorithms
            , @ShowInDiscoveryDocument  
            , @RequireResourceIndicator
            , GETUTCDATE()
            , 0
        )

	SELECT 
           [Id]
          ,[Enabled]
            ,[Name]
            ,[DisplayName]
            ,[Description]
            ,[AllowedAccessTokenSigningAlgorithms]
            ,[ShowInDiscoveryDocument]
            ,[RequireResourceIndicator]
            ,[Created]
            ,[NonEditable]
	FROM [Auth].[ApiResources]
	WHERE 
		(Id = SCOPE_IDENTITY())
END
GO

