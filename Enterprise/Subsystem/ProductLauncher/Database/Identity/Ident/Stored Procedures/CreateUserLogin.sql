CREATE PROCEDURE [Ident].[CreateUserLogin] (
	@RealPageId uniqueidentifier,
	@LoginName varchar(255),
	@CreateUserSourceType nvarchar(50) = NULL
)
AS
BEGIN
	BEGIN TRY
		DECLARE @PartyId bigint,
			@UserId bigint,
			@CreateUserSourceId int,
			@Now datetime = GETUTCDATE()

		SELECT @UserId = UserId
		FROM Ident.UserLogin
		WHERE [LoginName] = @LoginName;

		SELECT @PartyId = PartyId
		FROM	Enterprise.Party
		WHERE	RealPageId = @RealPageId

		IF @CreateUserSourceType IS NULL
		BEGIN
			SELECT @CreateUserSourceId = TypeId
			FROM [Enterprise].[CreateUserSourceType]
			WHERE TypeName = 'UnifiedPlatform';
		END;
		ELSE
		BEGIN
			SELECT	@CreateUserSourceId = TypeId
			FROM	[Enterprise].[CreateUserSourceType]
			WHERE	TypeName = @CreateUserSourceType;
		END;
		BEGIN TRAN;
			IF @UserId IS NOT NULL
			BEGIN
				SELECT	@UserId AS Id,
							'The User Login already exists.' AS ErrorMessage;
			END;
		ELSE
		BEGIN
			INSERT INTO Ident.UserLogin (
				PersonPartyId,
				[LoginName],
				CreateUserSourceId,
				CreateDate
			)
			VALUES (
				@PartyId,
				@LoginName,
				@CreateUserSourceId,
				@Now
			);
			SET @UserId = SCOPE_IDENTITY();
			SELECT	@UserId AS Id,
						'' AS ErrorMessage;
		END;
		COMMIT;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID int;
		EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;
		SELECT	0 AS Id,
					ErrorMessage
		FROM	dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
		ROLLBACK;
	END CATCH;
END;