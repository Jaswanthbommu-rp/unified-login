CREATE PROC Enterprise.LinkGlobalConfigurationToProduct
@ConfigurationId INT,
@ProductId INT,
@FromDate DATETIME = NULL,
@ThruDate DATETIME = NULL
AS
BEGIN
    
	UPDATE Enterprise.GlobalProductConfiguration
	SET ThruDate = GETUTCDATE()
	WHERE ProductId = @ProductId
	AND (ThruDate >= GETUTCDATE() OR ThruDate IS NULL);

	INSERT INTO Enterprise.GlobalProductConfiguration (   ConfigurationId , ProductId , FromDate , ThruDate )
	VALUES (   @ConfigurationId , @ProductId , ISNULL(@FromDate, GETUTCDATE()) , @ThruDate )

END
