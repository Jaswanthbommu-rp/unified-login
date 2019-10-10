CREATE PROCEDURE [Auth].[ClientRedirectUrisInsert]
(
	@ClientId INT
	,@Uri nvarchar(2000)
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[ClientRedirectUris]
           ([ClientId]
           ,[Uri])
     VALUES
           (@ClientId
           ,@Uri)

	SELECT
		ClientRedirectUriId as Id
		, ClientId
		, Uri 
	FROM [Auth].[ClientRedirectUris] WHERE (ClientRedirectUriId = SCOPE_IDENTITY())

END


