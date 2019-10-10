IF OBJECT_ID('[Enterprise].[CreateProductSettingType]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[CreateProductSettingType];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[CreateProductSettingType] (
    @ProductSettingTypeName VARCHAR(50),
    @ProductSettingTypeDescription VARCHAR(100),
	@ProductSettingTypeId INT OUTPUT
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		INSERT INTO Enterprise.ProductSettingType (
			Name,
			Description
		)
		OUTPUT	Inserted.ProductSettingTypeId AS Id,
				'' AS ErrorMessage
		VALUES (
			@ProductSettingTypeName,
			@ProductSettingTypeDescription
		);

		SET @ProductSettingTypeId = SCOPE_IDENTITY();
		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;
GO
