CREATE PROCEDURE [Auth].ClientClaimsUpdate
(
	@ClientId int,
	@Type nvarchar(100),
	@Value nvarchar(100),
	@Original_ClientClaimsId int,
	@Original_ClientId int,
	@Original_Type nvarchar(100),
	@Original_Value nvarchar(100),
	@ClientClaimsId int
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Auth].[ClientClaims] 
		SET 
			[ClientId] = @ClientId
			, [Type] = @Type
			, [Value] = @Value 
		WHERE 
			(([Id] = @Original_ClientClaimsId) 
			AND 
			([ClientId] = @Original_ClientId) 
			AND ([Type] = @Original_Type) 
			AND ([Value] = @Original_Value))
	
	SELECT 
		  Id
		, ClientId
		, Type
		, Value 
	FROM 
		Auth.ClientClaims 
	WHERE 
		(Id = @ClientClaimsId)
END
