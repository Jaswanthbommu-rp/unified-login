IF OBJECT_ID('[Enterprise].[GetOrganization]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[GetOrganization];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[GetOrganization]
	@RealPageId UNIQUEIDENTIFIER
AS
	BEGIN
		SELECT 
			o.PartyId ,
			o.Name ,
			RealPageId ,
			CreateDate
		FROM [Enterprise].Organization AS o  
			JOIN [Enterprise].Party ON Party.PartyId = o.PartyId  
		WHERE RealPageId = @RealPageId  
	END;
GO
