CREATE PROCEDURE Enterprise.CreateProductSettingType (
    @ProductSettingTypeName VARCHAR(50),
    @ProductSettingTypeDescription VARCHAR(100),
	@ProductSettingTypeSensitiveData TINYINT,
	@ProductSettingTypeId INT OUTPUT
)
AS
BEGIN
	BEGIN TRY
		IF EXISTS ( SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE Name = @ProductSettingTypeName )
		BEGIN
			SELECT @ProductSettingTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE Name = @ProductSettingTypeName
		END
		ELSE
		BEGIN
			BEGIN TRANSACTION;
			INSERT INTO Enterprise.ProductSettingType (
				Name,
				Description,
				SensitiveData
			)
			OUTPUT	Inserted.ProductSettingTypeId AS Id,
					'' AS ErrorMessage
			VALUES (
				@ProductSettingTypeName,
				@ProductSettingTypeDescription,
				@ProductSettingTypeSensitiveData
			);

			SET @ProductSettingTypeId = SCOPE_IDENTITY();
			COMMIT;
		END
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
