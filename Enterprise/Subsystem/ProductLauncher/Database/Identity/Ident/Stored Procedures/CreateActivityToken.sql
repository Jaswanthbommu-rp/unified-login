CREATE PROCEDURE [Ident].[CreateActivityToken] @PartyId    BIGINT,
                                                  @realPageId UNIQUEIDENTIFIER,
                                                  @ActivityTypeId AS INT
AS
     BEGIN
         SET NOCOUNT ON;
         DECLARE @token VARCHAR(50)
		 DECLARE @currentUtcDate DATETIME
		 DECLARE @OldActivityTokenId AS INT
		 DECLARE @ActivityTokenExpirationMinutes AS INT;
		 DECLARE @ActivityConfigurationId INT
		 DECLARE @ExpirationDate DATETIME

	-- Get expiration time for inputted activity
         SELECT 
			@ActivityTokenExpirationMinutes = A.ActivityTokenExpirationMinutes,
			@ActivityConfigurationId = A.ActivityConfigurationId
         FROM [Ident].ActivityConfiguration A
			INNER JOIN Ident.ActivityType AT 
				ON AT.ActivityTypeId = A.ActivityTypeId
         WHERE AT.ActivityTypeId = @ActivityTypeId 
			AND A.PartyId = @PartyId;
			
         SELECT @token = NEWID();
         SELECT @currentUtcDate = GETUTCDATE();

	-- Check if any active token already exists for user
         SELECT @OldActivityTokenId = [ActivityTokenId], @ExpirationDate = AT.ExpireDateTime
         FROM [Ident].[ActivityToken] AT
			INNER JOIN Ident.ActivityConfiguration A
				ON A.ActivityConfigurationId = AT.ActivityConfigurationId
         WHERE A.[ActivityConfigurationId] = @ActivityConfigurationId
               AND RealPageId = @realPageId
               AND IsActive = 1; 

	-- if exist then de-activate it
          
		 IF @OldActivityTokenId IS NOT NULL AND @currentUtcDate <= @ExpirationDate  
         BEGIN
			SELECT ActivityToken AS ActivityToken FROM [Ident].[ActivityToken]
				WHERE ActivityTokenId = @OldActivityTokenId
		 END
		 ELSE
		 BEGIN
		     UPDATE AT
               SET
                   AT.IsActive = 0
			 FROM [Ident].[ActivityToken] AT
			INNER JOIN Ident.ActivityConfiguration A
				ON A.ActivityConfigurationId = AT.ActivityConfigurationId
			WHERE A.[ActivityConfigurationId] = @ActivityConfigurationId
               AND RealPageId = @realPageId

			 INSERT INTO [Ident].[ActivityToken]
				([ActivityConfigurationId],
				 [RealPageId],
				 [ActivityToken],
				 [IsActive],
				 [CreateDateTime],
				 [ExpireDateTime]
				)
			 VALUES
				(@ActivityConfigurationId,
				 @realPageId,
				 @token,
				 1,
				 @currentUtcDate,
				 DATEADD(minute, @ActivityTokenExpirationMinutes, @currentUtcDate)
				);

		-- return new token
          SELECT @token AS ActivityToken;
 		 END;
	
	-- create new token with expiration time
       
END