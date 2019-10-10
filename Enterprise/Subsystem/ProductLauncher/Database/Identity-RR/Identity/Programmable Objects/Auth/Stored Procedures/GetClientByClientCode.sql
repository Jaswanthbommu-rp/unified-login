IF OBJECT_ID('[Auth].[GetClientByClientCode]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetClientByClientCode];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetClientByClientCode]
	@ClientCode nvarchar(200)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT * FROM Auth.Clients WHERE ClientCode = @ClientCode
END
GO
