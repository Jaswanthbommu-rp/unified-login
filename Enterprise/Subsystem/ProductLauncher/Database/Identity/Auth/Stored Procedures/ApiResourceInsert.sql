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
END
GO

