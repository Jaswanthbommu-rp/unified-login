CREATE PROCEDURE [Auth].[GetAuthenticateUser]
	@enterpriseUserName	nvarchar(50),
	@hashedPassword nvarchar(255),
	@activityId as int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
   
	if exists(SELECT [UserId] FROM [Auth].[Users] where [LoginId]=@enterpriseUserName and PasswordHash=@hashedPassword)
		BEGIN			
			SELECT [UserId] as enterpriseUserId,[LoginId] as enterpriseUserName,[Firstname],[LastName] ,[IsActive],[IdentityProvider]
			  ,[Title],[Email],[Phone],[IsLocked]  FROM [Auth].[Users] 
			  where [LoginId]=@enterpriseUserName and PasswordHash=@hashedPassword

			update [Auth].[ActivityAttempts] set [AttemptCount]  =  0 where  activityAttemptsId=@activityId and [EnterpriseUserName]=@enterpriseUserName

			print 'updated'
		END
	else
		Select null
END