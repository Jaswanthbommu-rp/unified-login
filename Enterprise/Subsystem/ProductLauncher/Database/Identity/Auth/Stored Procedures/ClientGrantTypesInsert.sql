CREATE PROCEDURE [Auth].[ClientGrantTypesInsert]
(
	@ClientId INT,
	@GrantType nvarchar(250)
)
AS
BEGIN
	SET NOCOUNT OFF;
	IF NOT EXISTS ( SELECT TOP (1) 1 FROM [Auth].[ClientGrantTypes] WHERE ClientId = @ClientId AND GrantType = @GrantType )
	BEGIN
		INSERT INTO [Auth].[ClientGrantTypes]
			   ([ClientId], [GrantType])
		 VALUES
			   (@ClientId, @GrantType)

		SELECT
			  Id
			, ClientId
			, GrantType
		FROM [Auth].[ClientGrantTypes] 
			WHERE (Id = SCOPE_IDENTITY())
	END
	ELSE
	BEGIN
		SELECT
			  Id
			, ClientId
			, GrantType
		FROM [Auth].[ClientGrantTypes] 
			WHERE ClientId = @ClientId AND GrantType = @GrantType
	END
END
