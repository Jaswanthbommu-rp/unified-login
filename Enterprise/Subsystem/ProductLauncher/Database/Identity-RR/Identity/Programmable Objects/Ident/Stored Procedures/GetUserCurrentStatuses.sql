IF OBJECT_ID('[Ident].[GetUserCurrentStatuses]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetUserCurrentStatuses];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetUserCurrentStatuses] (
    @RealPageId UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
	SELECT ul.UserId, st.StatusTypeId, st.Name, us.StatusSetDate, us.FromDate, us.ThruDate
	FROM Ident.UserLogin AS ul INNER JOIN
	 Enterprise.Party AS p ON p.PartyId = ul.PartyId INNER JOIN
	 Ident.UserCurrentStatus AS us ON us.UserId = ul.UserId INNER JOIN
	 Enterprise.StatusType AS st ON st.StatusTypeId = us.StatusTypeId
	WHERE (p.RealPageId = @RealPageId OR @RealPageId IS NULL) and us.ThruDate is not null --and us.ThruDate >= GETUTCDATE()
END
GO
GRANT EXECUTE ON  [Ident].[GetUserCurrentStatuses] TO [IdentityServer]
GO
