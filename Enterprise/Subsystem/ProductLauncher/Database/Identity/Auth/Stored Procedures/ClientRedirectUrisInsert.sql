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
           ,[RedirectUri])
     VALUES
           (@ClientId
           ,@Uri)

	SELECT
		  Id
		, ClientId
		, RedirectUri
	FROM [Auth].[ClientRedirectUris] WHERE (Id = SCOPE_IDENTITY())

END


