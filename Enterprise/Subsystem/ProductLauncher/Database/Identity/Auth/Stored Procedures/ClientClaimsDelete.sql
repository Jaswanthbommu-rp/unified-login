CREATE PROCEDURE [Auth].ClientClaimsDelete
(
	@Original_ClientClaimsId int,
	@Original_ClientId int,
	@Original_Type nvarchar(100),
	@Original_Value nvarchar(100)
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[ClientClaims] 
	WHERE 
		(([Id] = @Original_ClientClaimsId) 
		AND 
		([ClientId] = @Original_ClientId) 
		AND ([Type] = @Original_Type) 
		AND ([Value] = @Original_Value))
END