CREATE PROC [Enterprise].[LinkProductConfigurationToPersona]
    @PersonaId BIGINT ,
    @ConfigurationId INT ,
    @ProductId INT ,
    @FromDate DATETIME = NULL ,
    @ThruDate DATETIME = NULL
AS
    BEGIN

        DECLARE @NOW DATETIME = GETUTCDATE();

        IF @FromDate IS NULL
            SET @FromDate = GETUTCDATE();

        UPDATE PersonaConfiguration
        SET    ThruDate = @NOW
        FROM   Enterprise.PersonaConfiguration
        WHERE  ProductId = @ProductId
               AND (   ThruDate >= @NOW
                       OR ThruDate IS NULL
                   )
               AND PersonaId = @PersonaId;

        INSERT INTO Enterprise.PersonaConfiguration (   PersonaId , ConfigurationId , ProductId , FromDate , ThruDate )
        VALUES ( @PersonaId , @ConfigurationId , @ProductId , @FromDate , @ThruDate );

    END;