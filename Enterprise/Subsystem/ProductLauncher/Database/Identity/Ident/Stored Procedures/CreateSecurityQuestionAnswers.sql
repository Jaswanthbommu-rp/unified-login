CREATE PROCEDURE [Ident].[CreateSecurityQuestionAnswers]
(@enterpriseUserName AS  NVARCHAR(255),
 @activityToken AS       NVARCHAR(50),
 @ActivityTypeId AS          INT,
 @securityQuestion1Id AS INT,
 @securityAnswer1 AS     NVARCHAR(50),
 @securityQuestion2Id AS INT,
 @securityAnswer2 AS     NVARCHAR(50),
 @securityQuestion3Id AS INT,
 @securityAnswer3 AS     NVARCHAR(50),
 @PartyId INT
)
AS
     BEGIN
         BEGIN TRY
             DECLARE @realPageId AS UNIQUEIDENTIFIER
			 DECLARE @userid AS BIGINT
			 DECLARE @insertDateTime AS SMALLDATETIME;
             
			 SELECT @realPageId = p.RealPageId,
                    @userid = u.userid
             FROM Ident.UserLogin u
			 INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = u.UserId
                  INNER JOIN Person.Persona PE ON PE.UserLoginPersonaId = ULP.UserLoginPersonaId--PE.UserId = U.userId
                  INNER JOIN Enterprise.Party p ON u.PersonPartyId = p.PartyId
             WHERE u.LoginName = @EnterpriseUserName;
             
			 IF EXISTS
             (
                 SELECT TOP 1 [ActivityToken]
                 FROM [Ident].ActivityToken AT
					INNER JOIN Ident.ActivityConfiguration AC
						ON AC.ActivityConfigurationId = AT.ActivityConfigurationId
					INNER JOIN Ident.ActivityType ATP
						ON ATP.ActivityTypeId = AC.ActivityTypeId
                 WHERE ATP.ActivityTypeId = @ActivityTypeId
                       AND AT.IsActive = 1
                       AND AT.realPageId = @realPageId
                       AND AT.ActivityToken = @activityToken
					   AND AC.PartyId = @PartyId
             )
                 BEGIN
                     SELECT @insertDateTime = GETUTCDATE();
                     DELETE FROM [Ident].[UserSecurityAnswer]
                     WHERE userid = @userid; -- delete old questions-answers

                     INSERT INTO [Ident].[UserSecurityAnswer]
                     ([UserId],
                      [SecurityQuestionId],
                      [Answer],
                      [CreateDateTime]
                     )
                     VALUES
                     (@UserId,
                      @securityQuestion1Id,
                      @securityAnswer1,
                      @insertDateTime
                     ),
                     (@UserId,
                      @securityQuestion2Id,
                      @securityAnswer2,
                      @insertDateTime
                     ),
                     (@UserId,
                      @securityQuestion3Id,
                      @securityAnswer3,
                      @insertDateTime
                     );
                     SELECT @@ROWCOUNT AS 'rowCount',
                            @UserId AS Id,
                            0 AS errorNumber,
                            '' AS errorMessage;
             END;
         END TRY
         BEGIN CATCH
             SELECT @@ROWCOUNT AS 'rowCount',
                    0 AS Id,
                    ERROR_NUMBER() AS errorNumber,
                    ERROR_MESSAGE() AS errorMessage;
         END CATCH;
     END;