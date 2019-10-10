IF OBJECT_ID('[Auth].[usp_GetClientForDeveloperLogin]') IS NOT NULL
	DROP PROCEDURE [Auth].[usp_GetClientForDeveloperLogin];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE PROCEDURE [Auth].[usp_GetClientForDeveloperLogin]
	@loginId NVARCHAR(128)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SELECT C.* FROM Auth.Developer I
		INNER JOIN Auth.DeveloperClients IC ON IC.DeveloperId = I.DeveloperId
		INNER JOIN Auth.Clients C ON C.ClientId = IC.ClientId
		WHERE I.LoginId = @loginId
END
GO
