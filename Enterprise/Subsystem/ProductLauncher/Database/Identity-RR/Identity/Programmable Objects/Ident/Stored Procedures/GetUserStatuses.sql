IF OBJECT_ID('[Ident].[GetUserStatuses]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetUserStatuses];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetUserStatuses] (
    @RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT st.StatusTypeId, st.Name, us.StatusSetDate, us.FromDate, us.ThruDate
	FROM Ident.UserLogin AS ul INNER JOIN
	 Enterprise.Party AS p ON p.PartyId = ul.PartyId INNER JOIN
	 Ident.UserCurrentStatus AS us ON us.UserId = ul.UserId INNER JOIN
	 Enterprise.StatusType AS st ON st.StatusTypeId = us.StatusTypeId
	WHERE (p.RealPageId = @RealPageId) and us.ThruDate IS NOT NULL
END
GO
