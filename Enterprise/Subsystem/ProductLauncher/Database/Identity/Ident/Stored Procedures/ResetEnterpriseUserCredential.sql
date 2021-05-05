CREATE PROCEDURE [Ident].[ResetEnterpriseUserCredential]
(@realPageId AS      UNIQUEIDENTIFIER,
 @newPasswordHash AS NVARCHAR(255),
 @newPasswordSalt AS NVARCHAR(255),
 @PartyId INT
)
AS
     BEGIN
         BEGIN TRY
             DECLARE @UserId AS BIGINT,
					 @oldPassword AS NVARCHAR(255),
					 @oldPasswordSalt AS NVARCHAR(255),
					 @currentUtcDate DATETIME,
					 @ActivityConfigurationId INT,
					 @passwordModifiedDate DATETIME
		   
		   SELECT @currentUtcDate = GETUTCDATE()
           SELECT @passwordModifiedDate = @currentUtcDate

		   SELECT @UserId = UL.UserId,
                    @oldPassword = UL.PasswordHash,
                    @oldPasswordSalt = UL.PasswordSalt
             FROM Enterprise.Party P
                  INNER JOIN Ident.UserLogin UL  ON P.PartyId = UL.PersonPartyId
             WHERE P.RealPageId = @realPageId;
             
		   IF @newPasswordHash IS NULL AND @newPasswordSalt IS NULL
		   BEGIN
				-- RESETTING USERS PASSWORD
				SET @passwordModifiedDate = NULL
		   END

		   UPDATE [ident].[UserLogin]
               SET
                   PasswordHash = @newPasswordHash,
                   PasswordSalt = @newPasswordSalt,
                   PasswordModifiedDate = @passwordModifiedDate
             WHERE userId = @UserId;

		-- insert old password in history table
             IF(@oldPassword IS NOT NULL)
			 begin
				 SELECT @ActivityConfigurationId = ActivityConfigurationId
					FROM Ident.ActivityConfiguration AC 
						INNER JOIN Ident.ActivityType AT
							ON AT.ActivityTypeId = AC.ActivityTypeId
						WHERE AT.ActivityTypeId = 2
							AND AC.PartyId = @PartyId
                 
					 INSERT INTO [Ident].[PasswordHistory]
					 ([UserId],
					  [ActivityConfigurationId],
					  [ChangedPasswordHash],
					  [ChangedPasswordSalt],
					  [ChangedPasswordDateTime]
					 )
					 VALUES
					 (@UserId,
					  @ActivityConfigurationId,
					  @oldPassword,
					  @oldPasswordSalt,
					  @currentUtcDate
					 );
					SELECT @@ROWCOUNT AS 'rowCount',
							@UserId AS Id,
							0 AS errorNumber,
							'' AS errorMessage;
				END
				ELSE
                BEGIN
					SELECT 1 AS 'rowCount',
							@UserId AS Id,
							0 AS errorNumber,
							'' AS errorMessage;
				END
	     END TRY
         BEGIN CATCH
             SELECT @@ROWCOUNT AS 'rowCount',
                    0 AS Id,
                    ERROR_NUMBER() AS errorNumber,
                    ERROR_MESSAGE() AS errorMessage;
         END CATCH;
     END;