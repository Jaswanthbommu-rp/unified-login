CREATE PROCEDURE [Enterprise].[SavePersonaProductProperties]
(
	@PersonaId	BIGINT,
	@ProductId	INT,
	@json		varchar(max)
)
AS
BEGIN
	DELETE FROM Enterprise.PersonaProductProperty WHERE PersonaId =  @PersonaId AND ProductId = @ProductId
	INSERT INTO Enterprise.PersonaProductProperty(
		PersonaId
		,ProductId
		,PropertyId
		,PropertyInstanceId
		,CreateDate
		)
	SELECT 
		@PersonaId as PersonaId
		,@ProductId as Productid
		,ProductProperties.ProductPropertyId
		,ProductProperties.PropertyInstanceId
		,GETUTCDATE() as CreateDate
	
	FROM  OPENJSON (@json)
			WITH(
				ProductPropertyId varchar(50),
				PropertyInstanceId varchar(100)
			) AS ProductProperties
END