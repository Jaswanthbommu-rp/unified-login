CREATE PROCEDURE [Ident].[ResetEnterpriseUserCredential]
(@realPageId AS      UNIQUEIDENTIFIER,
 @newPasswordHash AS NVARCHAR(255),
 @newPasswordSalt AS NVARCHAR(255),
 @PartyId INT
)
AS
     BEGIN
         BEGIN TRY
             DECLARE @UserId AS BIGINT
			 DECLARE @oldPassword AS NVARCHAR(255)
			 DECLARE @oldPasswordSalt AS NVARCHAR(255)
			 DECLARE @currentUtcDate DATETIME;
			 DECLARE @ActivityConfigurationId INT
             
		   SELECT @currentUtcDate = GETUTCDATE();
             
		   SELECT @UserId = UL.UserId,
                    @oldPassword = UL.PasswordHash,
                    @oldPasswordSalt = UL.PasswordSalt
             FROM Enterprise.Party P
                  INNER JOIN Ident.UserLogin UL  ON P.PartyId = UL.PersonPartyId
             WHERE P.RealPageId = @realPageId;
             
		   UPDATE [ident].[UserLogin]
               SET
                   PasswordHash = @newPasswordHash,
                   PasswordSalt = @newPasswordSalt,
                   PasswordModifiedDate = @currentUtcDate
             WHERE userId = @UserId;

		-- insert old password in history table
             IF(@oldPassword IS NOT NULL)
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
         END TRY
         BEGIN CATCH
             SELECT @@ROWCOUNT AS 'rowCount',
                    0 AS Id,
                    ERROR_NUMBER() AS errorNumber,
                    ERROR_MESSAGE() AS errorMessage;
         END CATCH;
     END;