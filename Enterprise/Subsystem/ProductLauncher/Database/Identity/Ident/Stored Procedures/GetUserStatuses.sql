CREATE PROCEDURE [Ident].[GetUserStatuses](@RealPageId UNIQUEIDENTIFIER)
AS
     BEGIN
         SELECT ULP.StatusTypeId as StatusTypeId,
                ST.Name,
                ULP.FromDate,
                ULP.ThruDate,
                ULP.ThruDate
         FROM Ident.Userlogin UL
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = ul.UserId
			INNER JOIN Enterprise.Party P ON P.PartyId = UL.PersonPartyId
			INNER JOIN Enterprise.StatusType AS st ON st.StatusTypeId =  ULP.StatusTypeId
         WHERE(p.RealPageId = @RealPageId)
     END;
