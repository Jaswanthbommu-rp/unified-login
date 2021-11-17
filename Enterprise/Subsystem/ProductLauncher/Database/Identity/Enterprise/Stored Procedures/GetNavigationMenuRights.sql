CREATE PROCEDURE [Enterprise].[GetNavigationMenuRights]
AS
	SELECT NavigationMenuId, r.RightName
	FROM Enterprise.NavigationMenuRights nmr
		INNER JOIN [Security].[Right] r on nmr.RightId = r.RightId
		INNER JOIN [Security].[RightRoute] rgr ON
			r.RightId = rgr.RightId