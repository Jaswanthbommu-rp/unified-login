CREATE PROCEDURE [Person].[CreateTelecommunicationsNumber] (
	@ContactMechanismId INT,
	@CountryCode VARCHAR(10) = 1,
	@AreaCode VARCHAR(10),
	@Phonenumber VARCHAR(15),
	@ISOCode Varchar(5),
    @Default BIT
)
AS
BEGIN
    BEGIN TRY
		-- Check if the ContactMechanism Exists, If it does, then update it.
        BEGIN TRANSACTION; 

        UPDATE  t
        SET     t.CountryCode = @CountryCode,
                t.AreaCode = @AreaCode,
                t.PhoneNumber = @Phonenumber,
				t.ISOCode = @ISOCode,
                t.[Default] = @Default
        FROM    Enterprise.TelecommunicationsNumber t
        WHERE   ContactMechanismID = @ContactMechanismId;

        IF @@ROWCOUNT = 0
        BEGIN
			INSERT  INTO Enterprise.TelecommunicationsNumber (
				ContactMechanismID ,
				CountryCode ,
				AreaCode ,
				PhoneNumber,
				ISOCode,
                [Default]
			)
			VALUES (
				@ContactMechanismId , -- ContactMechanismID - int
				@CountryCode , -- CountryCode - varchar(10)
				@AreaCode , -- AreaCode - varchar(10)
				@Phonenumber,  -- PhoneNumber - varchar(10)
				@ISOCode,
                @Default
			);		    
        END;

		SELECT	@ContactMechanismId AS Id,
                '' AS ErrorMessage
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id ,
                ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
