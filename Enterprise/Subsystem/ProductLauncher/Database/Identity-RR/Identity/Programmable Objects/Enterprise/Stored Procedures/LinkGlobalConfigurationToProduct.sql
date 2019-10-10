IF OBJECT_ID('[Enterprise].[LinkGlobalConfigurationToProduct]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[LinkGlobalConfigurationToProduct];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROC [Enterprise].[LinkGlobalConfigurationToProduct]
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
GO
