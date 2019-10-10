-- Add an Street Address
CREATE PROCEDURE Person.CreateStreetAddress (
    @ContactMechanismId INT,
    @StreetAddress1 NVARCHAR(50),
    @StreetAddress2 NVARCHAR(50) = NULL,
    @StreetAddress3 NVARCHAR(50) = NULL
)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

		INSERT  INTO Enterprise.StreetAddress
		(
			ContactMechanismID,
			StreetAddress1,
			StreetAddress2,
			StreetAddress3
		)
		VALUES
		(
			@ContactMechanismId,
			@StreetAddress1,
			@StreetAddress2,
			@StreetAddress3
		);

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