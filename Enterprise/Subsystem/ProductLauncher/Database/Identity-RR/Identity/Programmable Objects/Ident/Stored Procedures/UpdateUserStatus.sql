IF OBJECT_ID('[Ident].[UpdateUserStatus]') IS NOT NULL
	DROP PROCEDURE [Ident].[UpdateUserStatus];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[UpdateUserStatus]
    @RealPageId UNIQUEIDENTIFIER ,
    @StatusTypeId int,
	@FromDate DATETIME,
	@ThruDate DATETIME
AS
    BEGIN
	DECLARE @UserId bigint;

		SELECT @UserId=  Ident.UserLogin.UserId FROM Ident.UserLogin INNER JOIN
		Enterprise.Party ON Ident.UserLogin.PartyId = Enterprise.Party.PartyId where Enterprise.Party.RealPageId=@RealPageId
		 
		merge [Ident].[UserCurrentStatus] with(HOLDLOCK) as target
			using (values (@UserId,@StatusTypeId,GETUTCDATE(),@FromDate,@ThruDate))
			as source (UserId,StatusTypeId,StatusSetDate,FromDate,ThruDate)
			on target.UserId = @UserId and target.StatusTypeId=@StatusTypeId
		when matched then
			update set StatusSetDate =  GETUTCDATE (),	FromDate = source.FromDate, ThruDate = source.ThruDate
		when not matched by target then
			insert ( UserId,StatusTypeId,StatusSetDate, FromDate, ThruDate)
			values ( @UserId,@StatusTypeId, GETUTCDATE (),@FromDate,@ThruDate);
			 
    END;
GO
