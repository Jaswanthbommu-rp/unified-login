IF OBJECT_ID('[Ident].[CreateActivityToken]') IS NOT NULL
	DROP PROCEDURE [Ident].[CreateActivityToken];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[CreateActivityToken]
	@realPageId uniqueidentifier ,
	@activityId as int
AS
BEGIN
	SET NOCOUNT ON;
	Declare @token varchar(50), 
	@currentUtcDate  datetime,	
	@OldActivityTokenId as int,
	@ActivityTokenExpirationMinutes as int

	-- Get expiration time for inputted activity
	select @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from [Ident].Activity where activityId=@activityId

	select @token=NEWID ()
	select @currentUtcDate = GetUtcDate()

	-- Check if any active token already exists for user
	SELECT @OldActivityTokenId=[ActivityTokenId]  
	FROM [Ident].[ActivityToken]
	where [ActivityId]=@ActivityId and RealPageId=@realPageId and IsActive=1 

	-- if exist then de-activate it
	if  @OldActivityTokenId is not null
		Update [Ident].[ActivityToken] set IsActive=0 where [ActivityId]=@ActivityId and RealPageId=@realPageId
	end
	
	-- create new token with expiration time
	INSERT INTO [Ident].[ActivityToken]
			([ActivityId], [RealPageId],[ActivityToken],[IsActive],[CreateDateTime],[ExpireDateTime])
	VALUES
			(@ActivityId,@realPageId,@token,1,@currentUtcDate,DateAdd(minute,@ActivityTokenExpirationMinutes,@currentUtcDate))

	-- return new token
	select @token as ActivityToken
GO
