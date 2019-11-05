CREATE PROCEDURE [Person].[UpdatePerson] (
	@RealPageId UNIQUEIDENTIFIER ,
	@Title NVARCHAR(50) = NULL ,
	@FirstName Name = NULL ,
	@MiddleName Name = NULL ,
	@LastName Name = NULL ,
	@Suffix NVARCHAR(10) = NULL,
	@PreferredContactMethodId int = NULL
)
AS
BEGIN
	-- testing
	BEGIN TRY
        BEGIN TRANSACTION;
		UPDATE  [Person]
		SET     [Title] = ISNULL(@Title, Title) ,
				[FirstName] = ISNULL(@FirstName, FirstName) ,
				[MiddleName] = ISNULL(@MiddleName, MiddleName) ,
				[LastName] = ISNULL(@LastName, LastName) ,
				[Suffix] = ISNULL(@Suffix, Suffix),
				[PreferredContactMethodId] = ISNULL(@PreferredContactMethodId, PreferredContactMethodId)			
		OUTPUT	inserted.PartyId AS Id,
				'' AS ErrorMessage
		FROM    [Person].[Person]
				JOIN [Enterprise].[Party] ON [Party].[PartyId] = [Person].[PartyId]
		WHERE   [Party].[RealPageId] = @RealPageId;
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