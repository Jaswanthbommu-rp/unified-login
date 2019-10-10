IF OBJECT_ID('[Auth].[UpdateActivityTokenFlag]') IS NOT NULL
	DROP PROCEDURE [Auth].[UpdateActivityTokenFlag];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[UpdateActivityTokenFlag]
	 @enterpriseUserName as nvarchar(50),
		@activityId as int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @UserId as bigint

	select @UserId=userid from users where LoginId=@enterpriseUserName

	update [Auth].[ActivityToken] set isActive=0 WHERE userid=@userid and activityId=@activityId

END
GO
