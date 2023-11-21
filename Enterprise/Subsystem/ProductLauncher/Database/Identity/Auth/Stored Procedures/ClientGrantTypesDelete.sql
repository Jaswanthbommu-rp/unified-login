CREATE PROCEDURE [Auth].ClientGrantTypesDelete
(
	@Original_ClientGrantTypeId bigint,
	@Original_ClientId int,
	@Original_GrantType nvarchar(250)
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[ClientGrantTypes] 
	WHERE 
		[Id] = @Original_ClientGrantTypeId
		AND [ClientId] = @Original_ClientId
		AND [GrantType] = @Original_GrantType

	SELECT @@RowCount [RowsAffected]
END