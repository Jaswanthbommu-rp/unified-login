IF OBJECT_ID('[Auth].[GetEnterpriseUserStatus]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetEnterpriseUserStatus];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetEnterpriseUserStatus]
	@entepriseLoginName	nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Declare @entepriseUserId as bigint ,
	@IsLocked as bit

	SELECT @entepriseUserId = UserId, @IsLocked=IsLocked FROM Users WHERE LoginId = @entepriseLoginName  

	IF @entepriseUserId IS NULL
       SELECT Null as EnterpriseUserId,@IsLocked as IsLocked, 'false' AS IsUserExist
	Else
		SELECT @entepriseUserId as EnterpriseUserId,@IsLocked as IsLocked,'true' AS IsUserExist
	END
GO
