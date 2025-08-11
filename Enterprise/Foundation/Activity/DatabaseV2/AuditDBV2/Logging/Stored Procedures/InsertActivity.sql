

CREATE PROCEDURE [Logging].[InsertActivity] 
(
	@LogTypeId INT = NULL,
	@LogType NVARCHAR(100) = NULL, 
	@LogCategoryType NVARCHAR(100) = NULL,
	@Message NVARCHAR(400),
	@FromUserLoginName NVARCHAR(200),
	@FromUserFirstName NVARCHAR(50),
	@FromUserLastName NVARCHAR(50),
	@FromUserRealpageId UNIQUEIDENTIFIER, 
	@OrganizationPartyId BIGINT,
	@Timestamp DATETIME,
	@ContextId NVARCHAR(200) = NULL, 
	@ContextReferenceId NVARCHAR(400) = NULL,
	@ApplicationCorrelationId NVARCHAR(200) = NULL,
	@AdditionalInformationTPV ADDITIONALINFO READONLY,
	@ActivityId BIGINT OUTPUT,
	@IsRealPageEmployee BIT = 0
)
AS
BEGIN
SET NOCOUNT ON;

	/*
		,@ContextId --Unified Login:- NULL, since all activities are at company level. Unified Settings:-	Company Level: NULL or Setting’s Company id ,Property Level: Setting’s Property Id,	Template Level: Setting’s Template Id
		,@ContextReferenceId  --Unified Login:-	Store User RealPageId,Unified Settings:- Store Setting {SourceId}_{MappingKey}_{InstanceId},Unified Reporting:-	Store Reporting Key
	*/	

	DECLARE @Now DATETIME= GETUTCDATE();
	DECLARE @FromUserId BIGINT;

	SELECT @LogTypeId = LogTypeId
	FROM Logging.LogType AS LT
	INNER JOIN Logging.LogCategoryType LCT ON LT.LogCategoryTypeId = LCT.LogCategoryTypeId
	WHERE
	(@LogType IS NULL AND @LogCategoryType IS NULL AND LogTypeId = @LogTypeId)
	OR
	(LT.[Name] = @LogType AND (LCT.[Name] = @LogCategoryType OR @LogCategoryType IS NULL))

	IF (@LogTypeId IS NULL OR @FromUserRealpageId IS NULL)
	RETURN

			
	SELECT @FromUserId = [UserId]
	FROM Logging.UserLogin
	WHERE RealPageId = @FromUserRealpageId;
		
	IF (@FromUserId IS NOT NULL)
	BEGIN

		UPDATE Logging.UserLogin 
		SET FirstName = @FromUserFirstName, LastName = @FromUserLastName,LoginName = @FromUserLoginname
		WHERE
			RealPageId = @FromUserRealpageId

	END
	ELSE
	BEGIN
				
		INSERT INTO Logging.UserLogin(LoginName,  FirstName, LastName, RealPageId)
		VALUES( @FromUserLoginname,  @FromUserFirstName, @FromUserLastName, @FromUserRealpageId );

		SELECT @FromUserId = SCOPE_IDENTITY()
			
	END;
	
	--Process Activity Table
	BEGIN TRY

		INSERT INTO Logging.Activity
		(
					
			OrganizationPartyId
			,LogTypeId
			,[Message]
			,ContextId
			,ContextReferenceId
			,ApplicationTimeStamp
			,CreatedBy
			,CreatedDate
			,IsRealPageEmployee
            ,ApplicationCorrelationId
		)
		VALUES
		(
					
			ISNULL(@OrganizationPartyId, 0)
			,@LogTypeId
			,@Message
			,@ContextId 
			,@ContextReferenceId  
			,@Timestamp
			,@FromUserId
			,@Now
			,@IsRealPageEmployee
            ,@ApplicationCorrelationId
		)
			
		SET @ActivityId = SCOPE_IDENTITY();				
			
		--Process Additional information if there is any
		INSERT INTO Logging.ActivityDetail( ActivityId, [Key], Value )
		SELECT 
			@ActivityId,[Key],[Value]
		FROM
			@AdditionalInformationTPV			
		
		SELECT @ActivityId AS Id, '' AS ErrorMessage;

	END TRY
	BEGIN CATCH
		
		SELECT 0 AS Id,ERROR_MESSAGE() AS ErrorMessage
	
	END CATCH;

END;

