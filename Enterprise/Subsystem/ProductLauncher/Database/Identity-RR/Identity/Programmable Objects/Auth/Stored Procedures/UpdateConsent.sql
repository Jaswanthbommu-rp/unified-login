IF OBJECT_ID('[Auth].[UpdateConsent]') IS NOT NULL
	DROP PROCEDURE [Auth].[UpdateConsent];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[UpdateConsent]
	@SubjectCode  NVARCHAR (200)  ,
    @ClientCode NVARCHAR (200) ,
    @Scopes   NVARCHAR (2000) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	update [Auth].[Consents] set Scopes=@Scopes WHERE SubjectCode = @SubjectCode and  ClientCode=@ClientCode

END
GO
