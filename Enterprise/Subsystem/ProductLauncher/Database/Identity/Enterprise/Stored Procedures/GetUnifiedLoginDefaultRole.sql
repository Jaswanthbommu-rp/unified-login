CREATE PROCEDURE [Enterprise].[GetUnifiedLoginDefaultRole](@RealPageID UNIQUEIDENTIFIER)
AS
     BEGIN
            SELECT DISTINCT R.RoleId FROM ENterprise.Role R
            INNER JOIN Enterprise.[Right] RT
                ON R.ROleId= RT.RoleId
            INNER JOIN Enterprise.RightValueType RVT 
                ON RT.RightValueTypeId = RVT.RightValueTypeId
            INNER JOIN Enterprise.Organization O
            ON O.PartyId = R.PartyId
            INNER JOIN Enterprise.Party P
                ON P.PartyId = O.PartyId
            WHERE RVT.ProductId = 3
            AND  R.DefaultRole = 1
			AND P.RealPageId = @RealPageID;
     END;