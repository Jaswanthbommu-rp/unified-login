CREATE PROCEDURE [Auth].ClientClaimsDelete
(
	@Original_ClientClaimsId INT,
	@Original_ClientId INT,
	@Original_Type NVARCHAR(100),
	@Original_Value NVARCHAR(100)
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
	SELECT @@RowCount [RowsAffected]
END