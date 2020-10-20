CREATE   PROCEDURE [Ident].[UpdateEnterpriseUserCredential_V2]
(
	@EnterpriseUserName AS NVARCHAR(255),
	@NewPasswordHash AS NVARCHAR(255),
	@passwordSalt AS NVARCHAR(255),
	@PartyId INT,
	@ActivityTypeId AS INT
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
	SELECT	@currentUtcDate = GETUTCDATE();

	-- Getting old Password details
	SELECT	@UserId = ul.UserId,
			@oldPassword = ul.PasswordHash,
			@oldPasswordSalt = ul.PasswordSalt,
			@realPageId = P.RealPageId
	FROM	Ident.UserLogin ul
		INNER JOIN Ident.UserLoginPersona ULP
			ON ULP.UserLoginId = ul.UserId
		INNER JOIN Enterprise.Party P
			ON ul.PersonPartyId = P.PartyId
	WHERE ul.LoginName = @EnterpriseUserName;

	-- update password
	UPDATE	Ident.UserLogin
	SET PasswordHash = @NewPasswordHash,
		PasswordSalt = @passwordSalt,
		PasswordModifiedDate = @currentUtcDate
	WHERE UserId = @UserId;

	SELECT	@ActivityConfigurationId = AC.ActivityConfigurationId
	FROM	Ident.ActivityToken AT
		INNER JOIN Ident.ActivityConfiguration AC
			ON AC.ActivityConfigurationId = AT.ActivityConfigurationId
		INNER JOIN Ident.ActivityType ATP
			ON ATP.ActivityTypeId = AC.ActivityTypeId
	WHERE ATP.[ActivityTypeId] = @ActivityTypeId
		AND AC.PartyId = @PartyId
		AND AT.RealPageId = @realPageId;

	-- insert old pwd in history table
	IF ((@oldPassword IS NOT NULL) AND (@ActivityConfigurationId IS NOT NULL))
		INSERT INTO Ident.[PasswordHistory]
		(
			[UserId],
			[ActivityConfigurationId],
			[ChangedPasswordHash],
			[ChangedPasswordSalt],
			[ChangedPasswordDateTime]
		)
		VALUES
		(@UserId, @ActivityConfigurationId, @oldPassword, @oldPasswordSalt, @currentUtcDate);

	-- Update Activity Attempts
	UPDATE	Ident.[ActivityAttempts]
	SET [AttemptCount] = 0
	FROM	Ident.[ActivityAttempts] AA
		INNER JOIN Ident.ActivityConfiguration AC
			ON AC.ActivityConfigurationId = AA.ActivityConfigurationId
		INNER JOIN Ident.ActivityType ATP
			ON ATP.ActivityTypeId = AC.ActivityTypeId
	WHERE AA.[EnterpriseUserName] = @EnterpriseUserName
		AND AA.LastAttemptDateTime >= DATEADD(DAY, -3, @currentUtcDate)
		AND ATP.ActivityTypeId IN ( 2, 5, 6 );

	SELECT	@UserId;
END;
