CREATE PROCEDURE [Person].[CreateElectronicAddress] (
	@ContactMechanismId INT,
	@ElectronicAddressString VARCHAR(255),
	@ElectronicAddressType VARCHAR(20) = 'Email'
)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

		UPDATE	Enterprise.ElectronicAddress
		SET		ElectronicAddressString = @ElectronicAddressString,
				ElectronicAddressType = @ElectronicAddressType
		WHERE	ContactMechanismID = @ContactMechanismId

        IF @@ROWCOUNT = 0
        BEGIN
			INSERT  INTO Enterprise.ElectronicAddress (
				ContactMechanismID,
				ElectronicAddressString,
				ElectronicAddressType
			)
			VALUES (
				@ContactMechanismId,
				@ElectronicAddressString,
				@ElectronicAddressType
			);
		END

		SELECT	@ContactMechanismId AS Id,
                '' AS ErrorMessage

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
    END CATCH;
END