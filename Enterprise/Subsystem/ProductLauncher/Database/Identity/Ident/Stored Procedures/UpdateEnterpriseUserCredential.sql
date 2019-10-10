CREATE PROCEDURE [Ident].[UpdateEnterpriseUserCredential]
(@EnterpriseUserName AS NVARCHAR(255),
 @correctAnswerToken AS NVARCHAR(50),
 @ActivityTypeId AS     INT,
 @NewPasswordHash AS    NVARCHAR(255),
 @passwordSalt AS       NVARCHAR(255),
 @PartyId            INT
)
AS
     BEGIN
         SET NOCOUNT ON;
         DECLARE @UserId AS BIGINT;
         DECLARE @realPageId AS UNIQUEIDENTIFIER;
         DECLARE @oldPassword AS NVARCHAR(255);
         DECLARE @oldPasswordSalt AS NVARCHAR(255);
         DECLARE @currentUtcDate AS DATETIME;
         DECLARE @ActivityConfigurationId INT;
         SELECT @currentUtcDate = GETUTCDATE();
         SELECT @UserId = ul.UserId,
                @oldPassword = ul.PasswordHash,
                @oldPasswordSalt = ul.PasswordSalt,
                @realPageId = p.RealPageId
         FROM Ident.UserLogin ul
              INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
              INNER JOIN Enterprise.Party P ON UL.PersonPartyId = p.PartyId
         WHERE ul.LoginName = @EnterpriseUserName;
         IF EXISTS
			(
				SELECT TOP 1 [ActivityToken]
				FROM Ident.ActivityToken AT
					 INNER JOIN Ident.ActivityConfiguration AC ON AC.ActivityConfigurationId = AT.ActivityConfigurationId
					 INNER JOIN Ident.ActivityType ATP ON ATP.ActivityTypeId = AC.ActivityTypeId
				WHERE ATP.[ActivityTypeId] = @ActivityTypeId
					  AND IsActive = 1
					  AND realPageId = @realPageId
					  AND ActivityToken = @correctAnswerToken
					  AND AC.PartyId = @PartyId
			)
             BEGIN
		-- update password
                 UPDATE Ident.UserLogin
                   SET
                       PasswordHash = @NewPasswordHash,
                       PasswordSalt = @passwordSalt,
                       PasswordModifiedDate = @currentUtcDate
                 WHERE UserId = @UserId;
                 SELECT @ActivityConfigurationId = AC.ActivityConfigurationId
                 FROM Ident.ActivityToken AT
                      INNER JOIN Ident.ActivityConfiguration AC ON AC.ActivityConfigurationId = AT.ActivityConfigurationId
                      INNER JOIN Ident.ActivityType ATP ON ATP.ActivityTypeId = AC.ActivityTypeId
                 WHERE ATP.[ActivityTypeId] = @ActivityTypeId
                       AND AC.PartyId = @PartyId;

		-- insert old pwd in history table
                 IF(@oldPassword IS NOT NULL)
                     INSERT INTO Ident.[PasswordHistory]
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
		
		-- reset token flags
                 UPDATE Ident.[ActivityToken]
                   SET
                       isActive = 0
                 FROM Ident.ActivityToken AT
                      INNER JOIN Ident.ActivityConfiguration AC ON AC.ActivityConfigurationId = AT.ActivityConfigurationId
                      INNER JOIN Ident.ActivityType ATP ON ATP.ActivityTypeId = AC.ActivityTypeId
                 WHERE AT.realPageId = @realPageId
                       AND ATP.ActivityTypeId IN(2, 6)
                       AND AC.PartyId = @PartyId;
                 UPDATE Ident.[ActivityAttempts]
                   SET
                       [AttemptCount] = 0
                 FROM Ident.[ActivityAttempts] AA
                      INNER JOIN Ident.ActivityConfiguration AC ON AC.ActivityConfigurationId = AA.ActivityConfigurationId
                      INNER JOIN Ident.ActivityType ATP ON ATP.ActivityTypeId = AC.ActivityTypeId
                 WHERE AA.[EnterpriseUserName] = @EnterpriseUserName
                       AND AA.LastAttemptDateTime >= DATEADD(day, -3, @currentUtcDate)
                       AND ATP.ActivityTypeId IN(2, 5, 6);
             END;
             ELSE
         SELECT @UserId = NULL; --RAISERROR('Activity token expired', 16,16)

         SELECT @UserId;
     END;