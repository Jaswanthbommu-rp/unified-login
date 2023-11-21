CREATE PROCEDURE [Auth].ClientGrantTypesSelect
(
	@ClientId INT = 0
)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT       
		Id
		,GrantType
		,ClientId
	FROM            
		Auth.ClientGrantTypes
	WHERE
		@ClientId = 0 OR ClientId = @ClientId
END
GO