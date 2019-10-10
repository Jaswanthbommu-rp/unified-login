IF OBJECT_ID('[Auth].[CreateActivityToken]') IS NOT NULL
	DROP PROCEDURE [Auth].[CreateActivityToken];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[CreateActivityToken]
	@EnterpriseUserId bigint ,
	@activityId as int
AS
BEGIN
	SET NOCOUNT ON;
	Declare @token varchar(50),
	
	@OldActivityTokenId as int,
	@ActivityTokenExpirationMinutes as int

	select @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from Auth.Activity where activityId=@activityId

	select @token=NEWID ()
	
	-- Check if any active token already exists for user
	SELECT @OldActivityTokenId=[ActivityTokenId]  
	FROM [Auth].[ActivityToken]
	where [ActivityId]=@ActivityId and UserId=@EnterpriseUserId and IsActive=1-- and [ExpireDateTime] > DateAdd(minute,15,getdate())

	if  @OldActivityTokenId is not null
		Update [Auth].[ActivityToken] set IsActive=0 where [ActivityId]=@ActivityId and UserId=@EnterpriseUserId
	end
	
	INSERT INTO [Auth].[ActivityToken]
           ( ActivityId,
			[UserId]
           ,[ActivityToken]
           ,[IsActive]
           ,[CreateDateTime]
           ,[ExpireDateTime])
     VALUES
			( @ActivityId,
				@EnterpriseUserId
			   ,@token
			   ,1
			   ,GetDate()
			   ,DateAdd(minute,@ActivityTokenExpirationMinutes,GetDate())
		   )

	select @token as ActivityToken
GO
