CREATE PROCEDURE [Enterprise].[UpdatePropertyMappingReMap]
(
    @OriginalPropertyID BIGINT,
    @NewPropertyID BIGINT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @PropertyMappingID BIGINT;
	DECLARE @Now DATETIME;
	SET @Now = GETUTCDATE();

	DECLARE @UpdatedPropertyList TABLE ( PersonaId BIGINT, PropertyId BIGINT, ProductId INT );
		
	UPDATE Enterprise.PropertyMapping
		SET ThruDate = @Now
	
	OUTPUT
		inserted.personaid, 
		inserted.propertyid, 
		inserted.productid
	INTO
		@UpdatedPropertyList

	WHERE
		PropertyId = @OriginalPropertyID
		AND
		ThruDate IS NULL
	
	INSERT INTO Enterprise.PropertyMapping ( personaid, propertyid, productid, fromdate )
		SELECT PersonaId, @NewPropertyID, Productid, @Now
		FROM 
		@UpdatedPropertyList

	-- MAY REMOVE EVENTUALLY
	UPDATE Enterprise.PropertyInstance
		SET CustomerPropertyId = @NewPropertyID
	
	WHERE
		CustomerPropertyId = @OriginalPropertyID
	
	SELECT
		1 As Id,
		ErrorMessage = ''
END;
GO
