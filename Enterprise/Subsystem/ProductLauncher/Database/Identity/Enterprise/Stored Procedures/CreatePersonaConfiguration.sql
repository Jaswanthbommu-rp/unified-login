CREATE PROCEDURE [Enterprise].[CreatePersonaConfiguration](
	 @PersonaId int
	,@ProductId int
	,@FromDate datetime = NULL
	,@ThruDate datetime = NULL
	,@UsePrimaryProperties BIT = 0
)

AS

BEGIN

	SET NOCOUNT ON;
	DECLARE @NOW datetime = GETUTCDATE();
	DECLARE @ProductSettingId int = NULL
	DECLARE @ConfigurationId int = NULL

	IF @Fromdate IS NULL
		SET @FromDate = @NOW;

	--check the exoistence of a PersonaConfiguration for persona and product
		--inser or update (by exipration
		--get the configurationID
	SELECT @ConfigurationId = ConfigurationId 
	FROM PersonaConfiguration 
	WHERE PersonaId = @PersonaId AND ProductId = @ProductId
	AND ((@NOW BETWEEN FromDate AND ThruDate) OR (@NOW >= FromDate AND ThruDate IS NULL))

	 BEGIN TRY
				IF @ConfigurationId IS NULL
				BEGIN
					INSERT INTO Configuration (CreateDate) 
					VALUES (@NOW);

					SELECT @ConfigurationId = SCOPE_IDENTITY();

					INSERT INTO PersonaConfiguration (PersonaId,ConfigurationId, ProductId, FromDate, ThruDate,UsePrimaryProperties)
					OUTPUT Inserted.ConfigurationId AS Id, '' AS ErrorMessage
					VALUES(@PersonaId,@ConfigurationId,@ProductId, @FromDate,@ThruDate,@UsePrimaryProperties);
				END
				ELSE
				BEGIN
					SELECT @ConfigurationId as Id, '' AS ErrorMessage
				END
        END TRY
        BEGIN CATCH
            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;

        END CATCH;
END;

